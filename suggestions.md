1. Required technology stack
The assignment explicitly requires:
Layer	Required technology	Notes
Backend	C# with .NET 8 or newer	Mandatory
Database	PostgreSQL	Mandatory
ORM	Your choice	Entity Framework Core is my recommendation
Frontend	React 18 or newer	Mandatory
Frontend language	JavaScript or TypeScript	Your choice; I recommend TypeScript
Frontend tooling	Vite	Recommended by the assignment
UI library	Your choice	Material UI, Ant Design, Tailwind CSS, etc.
Containerization	Docker	Mandatory
Orchestration	Docker Compose	Root docker-compose.yml required


My recommended stack
Component	Technology
Backend framework	ASP.NET Core Web API (.NET 8)
Language	C#
ORM	Entity Framework Core
Database	PostgreSQL
Frontend	React 18 + TypeScript
Build tool	Vite
Styling	Tailwind CSS
API communication	Axios
Backend validation	FluentValidation or Data Annotations
Database migrations	EF Core Migrations
Testing	xUnit + integration tests
Containers	Docker + Docker Compose


Important: Don't use NestJS for this assignment. The backend must be built with C# and .NET 8+.
2. Understanding the database requirements
Before designing tables, understand the business rules.

The school offers admissions by class and shift.

For example:
- Class 6 – Morning Shift: 120 seats
- Class 6 – Day Shift: 100 seats
- Class 7 – Morning Shift: 100 seats
Each class/shift combination has its own seat capacity.

A guardian reserves a seat for a student. The seat stays reserved for a limited time. If the admission fee is confirmed before expiry, the reservation becomes confirmed. Otherwise, the seat becomes available again.

The important rules are:
1. A student can have only one active application per class/shift.
2. A student can apply to another class/shift.
3. Active applications include HELD and CONFIRMED.
4. A reservation expires after the configured duration.
5. An expired reservation cannot be confirmed.
6. Confirmation must be idempotent: repeating confirmation cannot consume another seat.
7. Seats must never be overbooked.
8. Cancelling a reservation must release its seat.
9. Data must survive application restarts and Docker restarts.
3. Recommended database architecture
I recommend starting with two main tables:
1. class_offerings
2. admission_holds
You don't need a separate students table for this assignment because the assignment uses the X-Student-Id request header to identify students. There's no requirement to build student registration or login.

Here's the relationship:
       class_offerings
       ───────────────
       id (PK)
       name
       total_seats
       held_seats
       confirmed_seats
       created_at
             |
             | 1
             |
             | Many
             |
       admission_holds
       ───────────────
       id (PK)
       class_id (FK)
       student_id
       status
       expires_at
       created_at
       updated_at

One class offering can have many admission holds.

For example, a Class 6 Morning Shift offering can have 120 seats and hundreds of historical reservation records, while only 120 seats can be held or confirmed at any one time.
Table 1: class_offerings
This table represents a specific class and shift.
Column	Data type	Constraints	Purpose
id	UUID	Primary Key	Unique offering ID
name	VARCHAR(150)	NOT NULL	Class 6 - Morning Shift
total_seats	INTEGER	NOT NULL, > 0	Total capacity
held_seats	INTEGER	NOT NULL, DEFAULT 0	Currently reserved seats
confirmed_seats	INTEGER	NOT NULL, DEFAULT 0	Confirmed admissions
created_at	TIMESTAMPTZ	NOT NULL	Creation time
updated_at	TIMESTAMPTZ	NOT NULL	Last update


Why keep held_seats and confirmed_seats?

Because you can quickly calculate available seats:
available_seats =
    total_seats - held_seats - confirmed_seats

Example:
Total seats:       120
Held seats:         15
Confirmed seats:    80

Available seats:    25

These counters must only be changed inside database transactions. Never update them independently from their corresponding reservation records.
Table 2: admission_holds
This table stores all reservation and admission records.
Column	Data type	Constraints	Purpose
id	UUID	Primary Key	Unique hold ID
class_id	UUID	Foreign Key	References class_offerings
student_id	VARCHAR(50)	NOT NULL	Student's identity
status	VARCHAR(20)	NOT NULL	Current application status
expires_at	TIMESTAMPTZ	Nullable	Reservation expiry
created_at	TIMESTAMPTZ	NOT NULL	Creation time
updated_at	TIMESTAMPTZ	NOT NULL	Last update


Application statuses
I recommend these four statuses:
Status	Meaning
HELD	Seat temporarily reserved
CONFIRMED	Admission confirmed
EXPIRED	Reservation expired
CANCELLED	Reservation cancelled


The assignment explicitly requires the first three. CANCELLED is a reasonable additional status for implementing the cancellation endpoint.
Important rule: One active application per student per class
You should enforce this at the database level, not just in your C# service.

In PostgreSQL, use a partial unique index:
CREATE UNIQUE INDEX uq_active_student_class
ON admission_holds (student_id, class_id)
WHERE status IN ('HELD', 'CONFIRMED');

This means:
- Student A can apply to Class 6 Morning.
- Student A cannot create another active application for Class 6 Morning.
- Student A can apply to Class 6 Day.
- Student A can apply to Class 7 Morning.
The database itself prevents duplicate active applications.
4. PostgreSQL schema
Here's a practical starting schema.
CREATE TABLE class_offerings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    name VARCHAR(150) NOT NULL,

    total_seats INTEGER NOT NULL CHECK (total_seats > 0),

    held_seats INTEGER NOT NULL DEFAULT 0,

    confirmed_seats INTEGER NOT NULL DEFAULT 0,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_seat_capacity
    CHECK (
        held_seats >= 0
        AND confirmed_seats >= 0
        AND held_seats + confirmed_seats <= total_seats
    )
);

Now the reservation table:
CREATE TABLE admission_holds (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    class_id UUID NOT NULL
        REFERENCES class_offerings(id)
        ON DELETE RESTRICT,

    student_id VARCHAR(50) NOT NULL,

    status VARCHAR(20) NOT NULL DEFAULT 'HELD',

    expires_at TIMESTAMPTZ,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_hold_status
    CHECK (
        status IN (
            'HELD',
            'CONFIRMED',
            'EXPIRED',
            'CANCELLED'
        )
    )
);

Add the unique index:
CREATE UNIQUE INDEX uq_active_student_class
ON admission_holds (student_id, class_id)
WHERE status IN ('HELD', 'CONFIRMED');

Add useful indexes:
CREATE INDEX idx_holds_class_id
ON admission_holds(class_id);

CREATE INDEX idx_holds_student_id
ON admission_holds(student_id);

CREATE INDEX idx_holds_expiration
ON admission_holds(expires_at)
WHERE status = 'HELD';

These indexes help with:
- Finding applications belonging to a student.
- Loading applications for a class.
- Finding expired reservations.
5. The most important part: concurrency handling
This is probably the most important technical challenge in the assignment.

Imagine:
Class 6 Morning Shift

Total seats: 120
Confirmed:   119
Held:          0
Available:     1

Now 50 guardians click the reserve button simultaneously.

Only one person should successfully reserve the final seat.
What you should NOT do
Avoid this approach:
var availableSeats = classOffering.TotalSeats
    - classOffering.HeldSeats
    - classOffering.ConfirmedSeats;

if (availableSeats > 0)
{
    // Create reservation
}

Why?

Because multiple requests could read availableSeats = 1 before any request updates the database.

All 50 requests might think a seat is available.

This is a race condition.
Recommended solution: Database transaction + row-level locking
Use PostgreSQL's SELECT ... FOR UPDATE inside a transaction.

The flow should be:
Guardian requests a seat
          |
          v
Begin database transaction
          |
          v
Lock the selected class row
          |
          v
Check available seats
          |
          v
Check existing active application
          |
          v
Is a seat available?
       /       \
     No         Yes
     |           |
     v           v
 Return 409   Create HELD record
                  |
                  v
             Increase held_seats
                  |
                  v
             Commit transaction
                  |
                  v
             Return reservation

The class row lock ensures that requests competing for the same class/shift are processed safely.

Important: The availability check, hold creation and counter update must happen within the same transaction.

Also, make sure that expired holds are handled before deciding whether the class is full.
6. How reservation expiry should work
The assignment requires a configurable hold duration:
HOLD_DURATION_SECONDS=600

Default: 600 seconds (10 minutes).

When a student reserves a seat:
Current time: 10:00 AM
Hold duration: 10 minutes
Expiry time: 10:10 AM

Store the expiry timestamp in PostgreSQL:
expires_at = current UTC time + HOLD_DURATION_SECONDS

When the reservation expires:
1. Change its status from HELD to EXPIRED.
2. Decrease held_seats by one.
3. Make the seat available again.
4. Prevent confirmation of the expired reservation.
I recommend implementing an expiry service in the .NET backend that periodically checks expired holds.

But don't rely only on the background service.

Important: When a new reservation request arrives, the backend should also process expired holds for that class before checking capacity. This prevents an expired reservation from temporarily blocking a seat just because the background service hasn't run yet.
7. Confirmation workflow
When the guardian clicks ভর্তি ফি দিন, the assignment expects simulated successful payment and confirmation.

The flow:
POST /holds/{holdId}/confirm
          |
          v
Begin transaction
          |
          v
Lock relevant class and hold records
          |
          v
Check student ownership
          |
          v
Check reservation status
          |
          v
Is status HELD?
      /       \
    No         Yes
    |           |
    v           v
Check status   Check expiry
    |           |
    |           v
    |       Still valid?
    |        /      \
    |      No        Yes
    |      |          |
    |      v          v
    |   EXPIRED    CONFIRMED
    |                 |
    |                 v
    |          held_seats - 1
    |          confirmed_seats + 1
    |                 |
    v                 v
Return result      Commit

Idempotency
Suppose the guardian clicks the confirmation button twice.

The second request must not increment confirmed_seats again.

If the hold is already CONFIRMED, return the existing confirmed result without changing the counters.

This is essential for the assignment's idempotency test.
8. How the API connects to the database
The assignment specifies these API endpoints:
Method	Endpoint	Purpose
POST	/classes	Create a class/shift offering
GET	/classes	List classes and seat availability
GET	/classes/{classId}	Get a specific class
POST	/classes/{classId}/holds	Reserve a seat
GET	/holds/{holdId}	Get reservation status
POST	/holds/{holdId}/confirm	Confirm admission
DELETE	/holds/{holdId}	Cancel reservation
GET	/me/holds	Get current student's applications


For student-related endpoints, use:
X-Student-Id: 20152691234567890

The assignment doesn't require a login or registration system.
9. Other assignment instructions you must not miss
Here's the implementation checklist.
Requirement	What you need to do
Backend	Use C# with .NET 8+
Frontend	Use React 18+
Database	PostgreSQL
ORM	Choose your own
Backend URL	http://localhost:8080
Database connection	Configure through environment variables
Frontend API URL	Must be configurable through an environment variable
Hold duration	HOLD_DURATION_SECONDS, default 600
Docker	Root-level docker-compose.yml
Docker services	PostgreSQL, backend and frontend
Startup	Everything must start using docker compose up
External installation	Reviewer's machine should need Docker only
UI	Both school admin and guardian interfaces
Admin	Create class/shift offerings and view seat information
Guardian	Browse classes, reserve, confirm, cancel and view applications
Expiry	Show EXPIRED in the UI
Expiry button	Disable ভর্তি ফি দিন for expired reservations
Refresh	Applications must remain visible after browser refresh/reopen
Persistence	Data must survive application and container restarts
Errors	Invalid inputs return 4xx, not 500
Error messages	Understandable messages, not blank pages
Concurrency	Never allow more holds + confirmations than capacity
Idempotency	Repeated confirmation must not increase confirmed seats twice
Tests	At least three automated tests
Functional test	Demonstrate the admission workflow
Concurrency test	Simultaneous requests must not exceed capacity
Idempotency test	Repeated confirmation increments only once
AI documentation	AI_USAGE.md
Assumptions	ASSUMPTIONS.md
Submission	Public GitHub repository
Git history	Meaningful incremental commits
Deadline	11 November 2026, 11:59:59 PM


Also, the assignment explicitly says that all tasks must be possible through the UI. The reviewer shouldn't need Postman or direct database access.
10. Recommended backend project structure
Since you're using .NET, I'd keep the backend organized like this:
backend/
│
├── Controllers/
│   ├── ClassesController.cs
│   └── HoldsController.cs
│
├── Data/
│   └── AdmissionDbContext.cs
│
├── Entities/
│   ├── ClassOffering.cs
│   └── AdmissionHold.cs
│
├── DTOs/
│   ├── CreateClassRequest.cs
│   ├── ClassResponse.cs
│   ├── CreateHoldResponse.cs
│   └── HoldResponse.cs
│
├── Services/
│   ├── ClassService.cs
│   ├── HoldService.cs
│   └── HoldExpirationService.cs
│
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
│
├── Migrations/
│
├── Tests/
│
├── Program.cs
├── appsettings.json
└── Dockerfile

One recommendation: keep the seat allocation and confirmation logic inside services, not controllers. Controllers should mainly handle HTTP requests and responses.
My implementation recommendation
Build in this order:
1. PostgreSQL database and EF Core entities.
2. Database migrations and constraints.
3. Class creation and listing APIs.
4. Reservation API with concurrency protection.
5. Expiry handling.
6. Confirmation and cancellation APIs.
7. Guardian UI.
8. Admin UI.
9. Automated tests.
10. Docker Compose and documentation.
One thing I would prioritize above fancy UI: get the reservation transaction and concurrency protection correct first. A beautiful admission system that accidentally allocates 121 seats when capacity is 120 will fail the core assignment.