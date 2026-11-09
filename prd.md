Product Requirements Document (PRD)
School Admission System
Project: School Admission System
Assignment: Assistant Software Engineer – Take-home Assignment
Organization: Onnorokom Projukti Limited
Deadline: 11 November 2026, 11:59:59 PM
Priority: High
Document Type: Technical PRD
1. Project Overview
1.1 Purpose
Build a web-based School Admission System that allows parents or guardians to apply for student admission through a first-come, first-served process.

The school has limited seats for each class and shift.

Admission follows these rules:
- No lottery system.
- First come, first served.
- Each class/shift has a fixed seat capacity.
- Applying temporarily reserves a seat.
- Parents must confirm admission by submitting the admission fee within the reservation period.
- If the reservation expires without confirmation, the seat becomes available again.
The system must support thousands of simultaneous applications without allowing more students to reserve or confirm seats than the available capacity.
1.2 Project Objectives
1. Provide a simple admission application interface.
2. Allow school administrators to create admission classes.
3. Display real-time seat availability.
4. Temporarily reserve seats for students.
5. Allow admission confirmation.
6. Automatically expire unpaid reservations.
7. Prevent duplicate or excessive reservations.
8. Maintain admission information after application restarts.
9. Provide a fully Dockerized application.
2. Technology Stack
The following technologies are mandatory or recommended by the assignment.
Layer	Technology	Requirement
Backend	C# / .NET 8+	Mandatory
Database	PostgreSQL	Mandatory
ORM	Entity Framework Core	Recommended implementation choice
Frontend	React 18+	Mandatory
Language	TypeScript	Recommended
Build Tool	Vite	Recommended
UI Library	Any suitable library	Developer choice
Containerization	Docker	Mandatory
Orchestration	Docker Compose	Mandatory
Testing	Automated tests	Mandatory


Recommended architecture
Use a modular backend architecture:
- Controllers
- Services
- Repositories
- Entities
- DTOs
- Validators
- Middleware
- Database migrations
Frontend:
- React components
- Pages
- Services/API clients
- Hooks
- Types/interfaces
- State management
- Routing
These are architectural recommendations, not additional requirements imposed by the assignment.
3. User Roles
The application has two primary user categories.
3.1 School Administrator
Responsible for managing admission classes and seat capacities.

Permissions:
- Create new classes.
- Define class names.
- Set total seat capacity.
- View all admission classes.
- Monitor held seats.
- Monitor confirmed seats.
- Monitor available seats.
Important: The assignment does not explicitly require administrator authentication.
3.2 Parent / Guardian
Responsible for applying for student admission.

Permissions:
- Identify a student using their birth registration number.
- View available classes.
- Apply for admission.
- Temporarily reserve a seat.
- View reservation information.
- Confirm admission.
- Cancel an application.
- View their own applications.
- Check application status.
Important: Login is NOT required.

The student birth registration number acts as the student's identity.
4. Functional Requirements
FR-01: Student Identification
The application must provide a student identification field.

Requirements:
- No login or registration page.
- Parents enter the student's birth registration number.
- The frontend sends the student identifier through the X-Student-Id HTTP header.
- Backend uses this identifier to associate admission applications with students.
Example:

X-Student-Id: 20152691234567890

The same student identifier must be used consistently across frontend and backend.
FR-02: Class Management
School administrators must be able to create admission classes.

Each class represents a unique class/shift combination.

Examples:
- Class 6 – Morning Shift
- Class 6 – Day Shift
- Class 7 – Morning Shift
- Class 7 – Day Shift
Each class must have:
Field	Description
id	Unique class identifier
name	Class and shift name
totalSeats	Total available seats


Example request:

POST /classes
{
  "name": "Class 6 - Morning Shift",
  "totalSeats": 120
}

Expected response:
{
  "id": "c_123",
  "name": "Class 6 - Morning Shift",
  "totalSeats": 120
}

Expected HTTP status: 201 Created.
FR-03: Class Listing and Seat Availability
The application must display all available classes.

Each class must show:
- Class name.
- Total seats.
- Held seats.
- Confirmed seats.
- Available seats.
- Application button.
Example:
{
  "id": "c_123",
  "name": "Class 6 - Morning Shift",
  "totalSeats": 120,
  "heldSeats": 15,
  "confirmedSeats": 80,
  "availableSeats": 25
}

Seat calculation:

Available Seats = Total Seats - Held Seats - Confirmed Seats

The application must refresh seat information automatically or through appropriate page/data refresh mechanisms.
FR-04: Seat Reservation
Parents must be able to reserve a seat.

Endpoint:

POST /classes/{classId}/holds

Required header:

X-Student-Id

Expected response:
{
  "holdId": "h_789",
  "classId": "c_123",
  "studentId": "20152691234567890",
  "status": "HELD",
  "expiresAt": "2026-11-01T04:10:00Z"
}

Requirements:
1. Verify that the class exists.
2. Check seat availability.
3. Prevent reservation when no seats remain.
4. Create a temporary seat reservation.
5. Associate the reservation with the student.
6. Generate an expiration timestamp.
7. Return reservation information.
If no seats are available, return:

409 Conflict
Reservation duration
The reservation duration must be configurable using:

HOLD_DURATION_SECONDS

Default:

600 seconds (10 minutes)

The application must not hardcode the reservation duration.
FR-05: Reservation Status
Parents must be able to check reservation status.

Endpoint:

GET /holds/{holdId}

Expected response:
{
  "holdId": "h_789",
  "classId": "c_123",
  "status": "HELD",
  "expiresAt": "2026-11-01T04:10:00Z"
}

Supported statuses:
Status	Meaning
HELD	Seat temporarily reserved
CONFIRMED	Admission successfully confirmed
EXPIRED	Reservation expired
CANCELLED	Application cancelled


FR-06: Admission Confirmation
Parents must be able to confirm admission.

Endpoint:

POST /holds/{holdId}/confirm

Required header:

X-Student-Id

Important assignment-specific assumption:

There is no actual payment gateway integration.

When the parent clicks the "ভর্তি ফি দিন" (Pay Admission Fee) button, the system must assume that payment succeeds.

Expected response:
{
  "holdId": "h_789",
  "status": "CONFIRMED"
}

Expected HTTP status: 200 OK.

Requirements:
- Verify student ownership.
- Verify reservation status.
- Verify reservation expiration.
- Confirm admission.
- Release the temporary reservation status.
- Increase confirmed seat count exactly once.
- Prevent duplicate confirmation.
Critical: Calling the confirmation endpoint multiple times for the same reservation must never increase confirmed seats more than once.
FR-07: Application Cancellation
Parents must be able to cancel their reservations.

Endpoint:

DELETE /holds/{holdId}

Required header:

X-Student-Id

Requirements:
- Verify student ownership.
- Cancel an eligible reservation.
- Release the held seat.
- Update seat availability.
Expected response:

204 No Content
FR-08: Student Application History
Parents must be able to view all applications associated with their student identifier.

Endpoint:

GET /me/holds

Required header:

X-Student-Id

Example response:
[
  {
    "holdId": "h_789",
    "classId": "c_123",
    "className": "Class 6 - Morning Shift",
    "status": "HELD",
    "expiresAt": "2026-11-01T04:10:00Z"
  }
]

Requirements:
- Show the student's applications.
- Display class names.
- Display application statuses.
- Display reservation expiration information.
5. Business Rules (Critical)
These rules are mandatory and should receive special attention during development.
BR-01: Seat Capacity Restriction
At no point should:

Held Seats + Confirmed Seats > Total Seats

This must remain true even when thousands of requests arrive simultaneously.
BR-02: Reservation Expiration
When a reservation expires:
- Its status must become EXPIRED.
- Its held seat must be released.
- Available seats must increase.
- The student must no longer be able to confirm that expired reservation.
Important: The system must automatically recognize expired reservations without requiring someone to call an API manually.
BR-03: One Active Application Per Student Per Class
A student cannot have more than one active application for the same class at the same time.

An active application means:
- HELD
- CONFIRMED
However, a student may apply for a different class.
BR-04: Confirmation Idempotency
Repeated confirmation requests must not increase confirmed seats multiple times.

For example:

First request:

HELD → CONFIRMED

Second request:

CONFIRMED → CONFIRMED

Confirmed seat count must increase only once.
BR-05: Persistent Data
Application data must survive:
- Backend restart.
- Frontend restart.
- Docker container restart.
- docker compose down.
- Subsequent docker compose up.
No admission information should disappear after restarting the application.
BR-06: Error Handling
Invalid input must return appropriate HTTP 4xx responses.

The application must never return HTTP 500 for normal user input errors.

Examples:
- Invalid student identifier.
- Nonexistent class.
- No available seats.
- Expired reservation.
- Invalid reservation ownership.
- Invalid request data.
BR-07: Expired Reservation UI
When a reservation expires:
- The UI must show EXPIRED.
- The payment button must become disabled.
- The page must reflect the latest application status.
This must work even without manually refreshing the page.
BR-08: Application Persistence Across Browser Sessions
When a student refreshes the page or closes and reopens the browser, their existing applications must still be visible.

The frontend should retrieve application information from the backend using the student identifier.
6. API Specification
The following API endpoints are explicitly defined in the assignment.
#	Method	Endpoint	Description
1	POST	/classes	Create a new class
2	GET	/classes	List all classes with seat information
3	GET	/classes/{classId}	Get individual class information
4	POST	/classes/{classId}/holds	Reserve a seat
5	GET	/holds/{holdId}	Get reservation status
6	POST	/holds/{holdId}/confirm	Confirm admission
7	DELETE	/holds/{holdId}	Cancel reservation
8	GET	/me/holds	Get student applications


API Requirements
- Backend base URL: http://localhost:8080
- JSON request and response format.
- Correct HTTP status codes.
- Student identity through X-Student-Id where specified.
- Consistent error responses.
- Environment-based database connection configuration.
Important: The frontend must use these API contracts. Do not silently rename or change the specified routes.
7. Frontend Requirements
The assignment requires both School Admin UI and Parent UI.
7.1 Parent Interface
Screen 1: Admission Dashboard
Components:
- Student birth registration number input.
- Class listing.
- Seat availability.
- Application buttons.
Screen 2: Application / Reservation
Display:
- Selected class.
- Student identifier.
- Reservation status.
- Reservation expiration time.
- Confirmation button.
- Cancellation button.
Screen 3: My Applications
Display:
- Class name.
- Reservation ID.
- Application status.
- Expiration time.
- Available actions.
Screen 4: Admission Confirmation
Display:
- Admission details.
- Payment simulation button.
- Confirmation result.
After successful confirmation, show the admission status as CONFIRMED.
7.2 School Admin Interface
Screen 1: Admin Dashboard
Display:
- Total classes.
- Total seats.
- Held seats.
- Confirmed seats.
- Available seats.
Screen 2: Create Class
Form fields:
- Class name.
- Total seats.
Screen 3: Class Management
Display:
- All classes.
- Seat capacity.
- Held seats.
- Confirmed seats.
- Available seats.
7.3 UI/UX Requirements
The assignment explicitly states that visual beauty is not mandatory.

However:
- UI must be functional.
- Navigation must be understandable.
- Buttons must work correctly.
- Errors must be visible.
- Loading states should be handled.
- Empty states should be handled.
- Expired reservations should be clearly identifiable.
- The interface must not display blank error pages.
All functionality must be accessible through the UI.

Direct database operations or Postman must not be necessary for normal application usage.
8. Database Requirements
PostgreSQL is mandatory.

The following is a recommended database structure.
8.1 Classes Table
Column	Type	Description
id	UUID	Primary key
name	VARCHAR	Class/shift name
total_seats	INTEGER	Total seat capacity
created_at	TIMESTAMP	Creation time
updated_at	TIMESTAMP	Update time


8.2 Students Table
Column	Type	Description
id	UUID	Primary key
birth_registration_number	VARCHAR	Student identifier
created_at	TIMESTAMP	Creation time


A separate Students table is an implementation decision, not an explicitly mandated table.
8.3 Holds Table
Column	Type	Description
id	UUID	Primary key
class_id	UUID	Foreign key
student_id	UUID	Student reference
status	VARCHAR	HELD / CONFIRMED / EXPIRED / CANCELLED
expires_at	TIMESTAMP	Reservation expiration
created_at	TIMESTAMP	Creation time
updated_at	TIMESTAMP	Update time


Important Database Constraints
Recommended:
- Primary key constraints.
- Foreign key constraints.
- Positive seat capacity.
- Unique student identifier.
- Indexes on class ID and student ID.
- Indexes on status and expiration time.
- Database-level protection against duplicate active applications.
The final concurrency protection must be enforced at the database/transaction level, not just through frontend validation.
9. Concurrency and Performance Requirements
This is one of the most important sections of the assignment.

Thousands of parents may submit applications simultaneously.

The system must guarantee seat availability correctly.
Required behavior
Suppose:

Class 6 – Morning Shift:
- Total seats: 120
- Held seats: 10
- Confirmed seats: 100
- Available seats: 10
Now imagine 100 students simultaneously attempting to reserve seats.

Expected result:
- Only 10 reservations can succeed.
- Remaining requests must receive 409 Conflict.
- Held + confirmed must never exceed 120.
Recommended implementation
Use:
1. PostgreSQL transactions.
2. Row-level locking.
3. Appropriate isolation and locking strategy.
4. Atomic reservation operations.
5. Database constraints where applicable.
6. Idempotent confirmation logic.
Do not rely on this approach alone:
if (availableSeats > 0)
{
    // Create reservation
}

This is unsafe when multiple requests execute simultaneously.

The availability check and reservation creation must happen atomically.
10. Automated Testing Requirements
The assignment explicitly requires at least three automated tests.
Test 1: Basic Functional Test
Verify that:
- A class can be created.
- Class information can be retrieved.
- A student can reserve a seat.
- The reservation status is correct.
Test 2: Concurrency Test
This is mandatory and particularly important.

Simulate multiple simultaneous reservation requests.

Verify:
- Only available seats can be reserved.
- No additional reservations succeed after capacity is reached.
- No duplicate active applications are created for the same student/class.
- Held + confirmed never exceeds total seats.
Example:
- Total seats: 10.
- Send 100 simultaneous requests from different students.
- Expected successful reservations: 10.
- Expected failed reservations: 90.
Test 3: Confirmation / Idempotency Test
Verify:
- A valid reservation can be confirmed.
- Confirmed seats increase by one.
- Repeated confirmation does not increase the count again.
- Expired reservations cannot be confirmed.
Additional recommended tests
Although only three automated tests are explicitly required, also test:
- Reservation expiration.
- Reservation cancellation.
- Student ownership validation.
- Application history.
- Database persistence after restart.
- Invalid input handling.
11. Docker and Deployment Requirements
This is mandatory.

The entire application must run using Docker Compose.
Required project structure
school-admission-system/
│
├── backend/
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── Entities/
│   ├── DTOs/
│   ├── Data/
│   ├── Tests/
│   ├── Dockerfile
│   └── SchoolAdmission.API.csproj
│
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── services/
│   │   ├── hooks/
│   │   ├── types/
│   │   └── routes/
│   ├── Dockerfile
│   └── package.json
│
├── docker-compose.yml
├── ASSUMPTIONS.md
├── AI_USAGE.md
├── README.md
└── .gitignore

Docker Compose Requirements
The root docker-compose.yml must start:
1. PostgreSQL.
2. Backend.
3. Frontend.
Command:
docker compose up

The reviewer must not need to install .NET, Node.js, PostgreSQL, or other project dependencies manually.
Port requirements
Service	Port
Backend	8080
Frontend	Developer-defined
PostgreSQL	Developer-defined/internal


Backend must be accessible at:

http://localhost:8080
Environment Variables
Database connection must be configurable through environment variables.

Suggested variables:
POSTGRES_DB=school_admission
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres

DATABASE_URL=

HOLD_DURATION_SECONDS=600

Frontend backend URL must also be configurable through an environment variable.
12. Documentation Requirements
Three documentation files are particularly important.
12.1 ASSUMPTIONS.md
The assignment explicitly asks developers to document assumptions where the specification is unclear.

Document:
- Assumption.
- Reason behind the assumption.
- Expected system behavior.
Examples:
- Admin authentication is not required because it is not specified.
- Payment is simulated as successful.
- A student may apply for multiple different classes.
- A student may only have one active application per class.
- Reservation duration defaults to 600 seconds.
- Expired reservations are automatically recognized and released.
Do not leave important design decisions undocumented.
12.2 AI_USAGE.md
AI tools are allowed.

You must document:
- Which AI tools were used.
- Which parts of the project used AI.
- What the AI generated or helped with.
- Mistakes made by AI.
- How those mistakes were identified and corrected.
Be honest about AI usage.
12.3 README.md
Must include:
- Project overview.
- Technology stack.
- Project structure.
- Docker installation/run instructions.
- Environment variables.
- API documentation.
- Frontend URL.
- Backend URL.
- Testing instructions.
A reviewer should be able to run the complete project by following this document.
13. Git and Repository Requirements
The assignment requires:
- A public GitHub repository.
- Complete source code.
- Separate backend and frontend folders.
- Root-level Docker Compose file.
- Commit history.
Important: Do not submit everything in one large commit.

Use meaningful incremental commits.

Suggested commit sequence:
chore: initialize project structure

feat: configure postgres and ef core

feat: implement class management APIs

feat: implement seat reservation system

feat: implement reservation expiration

feat: implement admission confirmation

feat: implement student application history

feat: build parent admission interface

feat: build school admin dashboard

test: add functional and concurrency tests

docs: add project documentation

chore: configure docker compose

14. Environment and Configuration Requirements
The following must be configurable:
Configuration	Variable
Database connection	Environment variable
Reservation duration	HOLD_DURATION_SECONDS
Frontend API URL	Frontend environment variable


The backend must run on port 8080.

The frontend must not contain a hardcoded backend URL.

The application must work after changing the backend URL through configuration.
15. Security and Reliability Requirements
The assignment does not provide a complete security specification, but the following are important implementation considerations.
- Validate all incoming requests.
- Validate student identifiers.
- Verify student ownership before viewing, confirming, or cancelling reservations.
- Prevent SQL injection through parameterized database access.
- Prevent duplicate reservation creation.
- Use database transactions.
- Prevent overbooking.
- Handle concurrent requests safely.
- Return meaningful HTTP errors.
- Avoid exposing sensitive database information.
One important assumption: the provided X-Student-Id mechanism is an assignment-level identity mechanism, not production-grade authentication.
16. Final Deliverables
The following deliverables are required.
#	Deliverable	Priority
1	.NET 8+ backend	Mandatory
2	PostgreSQL database	Mandatory
3	React 18+ frontend	Mandatory
4	School Admin UI	Mandatory
5	Parent UI	Mandatory
6	All 8 specified API endpoints	Mandatory
7	Seat reservation system	Mandatory
8	Reservation expiration	Mandatory
9	Admission confirmation	Mandatory
10	Concurrency protection	Mandatory
11	Minimum 3 automated tests	Mandatory
12	Dockerfiles	Mandatory
13	Root docker-compose.yml	Mandatory
14	ASSUMPTIONS.md	Mandatory
15	AI_USAGE.md	Mandatory
16	README.md	Mandatory
17	Public GitHub repository	Mandatory
18	Meaningful Git commit history	Mandatory


17. Definition of Done (Final Acceptance Criteria)
The project is considered complete only when all the following conditions are satisfied.
ID	Acceptance Criteria	Status
AC-01	Admin can create classes through UI	☐
AC-02	Multiple shifts can exist for the same class	☐
AC-03	Class listing displays correct seat counts	☐
AC-04	Student identification works without login	☐
AC-05	Student can reserve a seat	☐
AC-06	Reservation expiration works	☐
AC-07	Expired seats become available again	☐
AC-08	Student cannot overbook seats	☐
AC-09	Student cannot create duplicate active applications for the same class	☐
AC-10	Admission confirmation works	☐
AC-11	Repeated confirmation does not increase confirmed seats	☐
AC-12	Reservation cancellation works	☐
AC-13	Student can view application history	☐
AC-14	Refreshing the browser preserves application visibility	☐
AC-15	Application data survives Docker restarts	☐
AC-16	At least three automated tests pass	☐
AC-17	Concurrency test proves seat capacity protection	☐
AC-18	All operations work through UI	☐
AC-19	Frontend API URL is configurable	☐
AC-20	Database connection is configurable	☐
AC-21	docker compose up starts the entire application	☐
AC-22	No additional software installation is required on the reviewer's machine	☐
AC-23	ASSUMPTIONS.md is present	☐
AC-24	AI_USAGE.md is present	☐
AC-25	README.md contains execution instructions	☐
AC-26	Public GitHub repository contains complete source code	☐
AC-27	Git commit history shows incremental development	☐


18. Development Priority
I recommend implementing the system in this order.
Phase	Work	Priority
Phase 1	Project initialization and Docker setup	Critical
Phase 2	Database design and EF Core configuration	Critical
Phase 3	Class management APIs	Critical
Phase 4	Reservation and concurrency logic	Critical
Phase 5	Expiration and confirmation logic	Critical
Phase 6	Remaining backend APIs	High
Phase 7	Parent frontend	High
Phase 8	Admin frontend	High
Phase 9	Automated testing	Critical
Phase 10	Documentation and final Docker verification	Critical


Development recommendation: Complete and test the backend's concurrency and reservation logic before spending significant time on frontend styling.
19. Important Things Not to Miss
These are the requirements I would personally treat as the highest-risk parts of the assignment:
1. Concurrency: Never allow held + confirmed seats to exceed total seats.
2. Expiration: Expired reservations must release their seats automatically, without requiring a manual API call.
3. Idempotency: Repeated confirmation must not increase confirmed seats more than once.
4. Student restrictions: One student cannot have multiple active applications for the same class.
5. Persistence: Data must survive docker compose down followed by docker compose up.
6. UI functionality: Both admin and parent must be able to perform their required operations through the UI.
7. Environment configuration: Both database connection and frontend backend URL must be configurable.
8. Testing: The concurrency test is especially important.
9. Documentation: Both ASSUMPTIONS.md and AI_USAGE.md are explicitly requested.
10. Submission: Public GitHub repository, meaningful commit history, and Docker Compose execution.
The original assignment specifies these requirements across its five page