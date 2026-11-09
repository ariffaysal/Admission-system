School Admission System — Complete Development Roadmap
Required technology stack
First, these are the technologies the assignment wants:
Layer	Technology
Backend	C# / ASP.NET Core Web API (.NET 8+)
Database	PostgreSQL
ORM	Entity Framework Core (recommended; ORM is your choice)
Frontend	React 18+
Frontend language	TypeScript (JavaScript is also allowed)
Frontend tooling	Vite (recommended)
UI	Tailwind CSS (your choice)
API communication	Axios
Backend testing	xUnit
Containerization	Docker
Orchestration	Docker Compose


Important URLs and configuration:
- Backend: http://localhost:8080
- Frontend: configurable API URL through an environment variable.
- Database connection: environment variable.
- Hold duration: HOLD_DURATION_SECONDS, default 600.
- Submission: Public GitHub repository.
- Deadline: 11 November 2026, 11:59:59 PM.
PART 1 — Requirement Analysis, Project Planning & Database Foundation
Goal: Understand the requirements, plan the system, design the database and establish the backend foundation.
1.1 — Understand the PRD
- [ ] Read the complete assignment.
- [ ] Identify all functional requirements.
- [ ] Identify all non-functional requirements.
- [ ] Identify the two user types: School Admin and Guardian.
- [ ] Understand the first-come-first-served admission process.
- [ ] Understand how seat reservations work.
- [ ] Understand reservation expiry.
- [ ] Understand admission confirmation.
- [ ] Understand cancellation.
- [ ] Understand the concurrency requirements.
- [ ] Identify all eight required API endpoints.
- [ ] Identify the three mandatory automated test categories.
- [ ] Identify all Docker requirements.
- [ ] Identify all documentation requirements.
- [ ] Create a list of unclear requirements.
- [ ] Decide reasonable behavior for unspecified requirements.
- [ ] Document those decisions in ASSUMPTIONS.md.
1.2 — Plan the application architecture
- [ ] Decide backend architecture.
- [ ] Decide frontend architecture.
- [ ] Decide database architecture.
- [ ] Decide API communication structure.
- [ ] Decide how the frontend identifies a student.
- [ ] Decide how the backend handles reservation expiry.
- [ ] Decide how to protect against overbooking.
- [ ] Decide how to handle duplicate requests.
- [ ] Create a basic system architecture diagram.
- [ ] Create a database relationship diagram (ERD).
1.3 — Design the database
Create these two primary tables:

Table 1: class_offerings
- [ ] Create id (UUID, primary key).
- [ ] Create name.
- [ ] Create total_seats.
- [ ] Create held_seats.
- [ ] Create confirmed_seats.
- [ ] Create created_at.
- [ ] Create updated_at.
- [ ] Add a constraint preventing negative seat counts.
- [ ] Add a constraint preventing held + confirmed seats from exceeding total seats.
Table 2: admission_holds
- [ ] Create id (UUID, primary key).
- [ ] Create class_id (foreign key).
- [ ] Create student_id.
- [ ] Create status.
- [ ] Create expires_at.
- [ ] Create created_at.
- [ ] Create updated_at.
- [ ] Define allowed statuses:
  - HELD
  - CONFIRMED
  - EXPIRED
  - CANCELLED
- [ ] Add a foreign key relationship.
- [ ] Add a partial unique index for one active application per student per class.
- [ ] Add an index for finding applications by student.
- [ ] Add an index for finding applications by class.
- [ ] Add an index for finding expired holds.
1.4 — Set up the backend
- [ ] Install the .NET 8 SDK.
- [ ] Create the ASP.NET Core Web API project.
- [ ] Configure the project to listen on port 8080.
- [ ] Install Entity Framework Core.
- [ ] Install the PostgreSQL EF Core provider.
- [ ] Install the required PostgreSQL driver.
- [ ] Configure the database connection string using environment variables.
- [ ] Create AdmissionDbContext.
- [ ] Create ClassOffering entity.
- [ ] Create AdmissionHold entity.
- [ ] Configure entity relationships.
- [ ] Configure database constraints and indexes.
- [ ] Create the initial EF Core migration.
- [ ] Apply the migration.
- [ ] Verify the database tables.
- [ ] Test database connectivity.
Part 1 completion checkpoint
You should have:
- A documented PRD and assumptions.
- A database ERD.
- A working PostgreSQL database.
- Two primary database tables.
- EF Core migrations.
- A working .NET API connected to PostgreSQL.
PART 2 — Backend Development & Business Logic
Goal: Build all required APIs and implement the admission business rules.
2.1 — Backend project structure
- [ ] Create Controllers folder.
- [ ] Create Services folder.
- [ ] Create Entities folder.
- [ ] Create DTOs folder.
- [ ] Create Data folder.
- [ ] Create Middleware folder.
- [ ] Create Interfaces folder if using service interfaces.
- [ ] Configure dependency injection.
- [ ] Configure CORS for the React frontend.
- [ ] Configure global exception handling.
- [ ] Configure request validation.
- [ ] Configure JSON serialization.
- [ ] Configure environment-based settings.
2.2 — Create class API
Endpoint: POST /classes
- [ ] Create CreateClassRequest DTO.
- [ ] Accept class/shift name.
- [ ] Accept total seat count.
- [ ] Validate class name.
- [ ] Validate seat count is greater than zero.
- [ ] Implement ClassService.
- [ ] Save the class in PostgreSQL.
- [ ] Return the created class.
- [ ] Return HTTP 201 Created.
- [ ] Handle invalid requests with appropriate 4xx responses.
2.3 — Get all classes API
Endpoint: GET /classes
- [ ] Retrieve all class offerings.
- [ ] Calculate available seats.
- [ ] Return total seats.
- [ ] Return held seats.
- [ ] Return confirmed seats.
- [ ] Return available seats.
- [ ] Handle empty results.
- [ ] Return HTTP 200 OK.
Available seats calculation:
availableSeats =
totalSeats - heldSeats - confirmedSeats

2.4 — Get individual class API
Endpoint: GET /classes/{classId}
- [ ] Accept class ID.
- [ ] Validate UUID.
- [ ] Find class in database.
- [ ] Return class information.
- [ ] Calculate available seats.
- [ ] Return HTTP 404 if not found.
2.5 — Implement reservation API
Endpoint: POST /classes/{classId}/holds
- [ ] Read X-Student-Id header.
- [ ] Validate student ID.
- [ ] Validate class ID.
- [ ] Find the requested class.
- [ ] Begin database transaction.
- [ ] Lock the relevant class row.
- [ ] Process expired reservations for that class.
- [ ] Check whether the student already has an active application.
- [ ] Check seat availability.
- [ ] Reject reservation if no seat is available.
- [ ] Calculate expiry time.
- [ ] Create a new HELD reservation.
- [ ] Increase held_seats.
- [ ] Commit transaction.
- [ ] Return reservation ID.
- [ ] Return reservation status.
- [ ] Return expiry timestamp.
- [ ] Handle duplicate applications.
- [ ] Handle database concurrency conflicts.
Expected response:
{
  "holdId": "h_789",
  "classId": "c_123",
  "status": "HELD",
  "expiresAt": "2026-11-01T04:10:00Z"
}

2.6 — Get reservation status API
Endpoint: GET /holds/{holdId}
- [ ] Accept reservation ID.
- [ ] Find reservation.
- [ ] Check expiry if status is HELD.
- [ ] Update expired status when necessary.
- [ ] Return reservation status.
- [ ] Return expiry timestamp.
- [ ] Return related class information.
- [ ] Return appropriate 404 response.
2.7 — Implement admission confirmation
Endpoint: POST /holds/{holdId}/confirm
- [ ] Accept reservation ID.
- [ ] Read X-Student-Id.
- [ ] Verify student ownership.
- [ ] Begin database transaction.
- [ ] Lock the relevant records.
- [ ] Check reservation status.
- [ ] Check whether reservation has expired.
- [ ] Reject expired reservations.
- [ ] Reject cancelled reservations.
- [ ] Handle already-confirmed reservations idempotently.
- [ ] Change status from HELD to CONFIRMED.
- [ ] Decrease held_seats.
- [ ] Increase confirmed_seats.
- [ ] Commit transaction.
- [ ] Return confirmation response.
Important: This assignment uses simulated successful payment. You do not need to integrate a real payment gateway.
2.8 — Implement cancellation
Endpoint: DELETE /holds/{holdId}
- [ ] Accept reservation ID.
- [ ] Read X-Student-Id.
- [ ] Verify ownership.
- [ ] Find reservation.
- [ ] Check whether cancellation is allowed.
- [ ] Begin transaction.
- [ ] Change status to CANCELLED.
- [ ] Release the held seat if the previous status was HELD.
- [ ] Commit transaction.
- [ ] Return HTTP 204 No Content on success.
Document the cancellation rules in ASSUMPTIONS.md.
2.9 — Get student's applications
Endpoint: GET /me/holds
- [ ] Read X-Student-Id.
- [ ] Validate student ID.
- [ ] Find all applications belonging to that student.
- [ ] Include class name.
- [ ] Include reservation ID.
- [ ] Include status.
- [ ] Include expiry timestamp.
- [ ] Ensure expired reservations are represented correctly.
- [ ] Return applications.
Part 2 completion checkpoint
All eight required API endpoints should work.

You should be able to complete the entire admission workflow using API requests.
PART 3 — Reservation Reliability, Expiry & Automated Testing
Goal: Ensure the system works correctly even when thousands of guardians use it simultaneously.

This part is especially important because the assignment explicitly tests concurrency and idempotency.
3.1 — Concurrency protection
- [ ] Understand database transactions.
- [ ] Understand PostgreSQL row-level locking.
- [ ] Implement SELECT ... FOR UPDATE where appropriate.
- [ ] Lock the class row during seat allocation.
- [ ] Check availability inside the transaction.
- [ ] Create the reservation inside the same transaction.
- [ ] Update seat counters inside the same transaction.
- [ ] Ensure failed reservations do not consume seats.
- [ ] Ensure concurrent requests cannot overbook.
- [ ] Handle unique-index violations.
- [ ] Return appropriate conflict responses.
3.2 — Reservation expiry system
- [ ] Read HOLD_DURATION_SECONDS from environment variables.
- [ ] Use 600 seconds when no value is configured.
- [ ] Calculate expiry using UTC timestamps.
- [ ] Create a background expiry service.
- [ ] Find expired HELD reservations.
- [ ] Change their status to EXPIRED.
- [ ] Decrease held seat counters.
- [ ] Ensure expired seats become available again.
- [ ] Prevent expired reservations from being confirmed.
- [ ] Process expired holds during reservation requests too.
- [ ] Ensure an expiry operation cannot release the same seat twice.
3.3 — Idempotency
- [ ] Ensure repeated confirmation doesn't increment counters twice.
- [ ] Ensure an already-confirmed reservation remains confirmed.
- [ ] Ensure expired reservations cannot be confirmed.
- [ ] Ensure cancellation doesn't release a seat twice.
- [ ] Ensure database updates happen only when the status transition is valid.
3.4 — Automated testing
Create a test project.

Recommended tools:
- xUnit
- ASP.NET Core integration testing
- Testcontainers for PostgreSQL, if practical
Functional test
- [ ] Create a class.
- [ ] Reserve a seat.
- [ ] Verify reservation status is HELD.
- [ ] Confirm admission.
- [ ] Verify status becomes CONFIRMED.
- [ ] Verify seat counters are correct.
Concurrency test
- [ ] Create a class with limited capacity.
- [ ] Send many simultaneous reservation requests.
- [ ] Wait for all requests to finish.
- [ ] Count successful reservations.
- [ ] Check held + confirmed seats.
- [ ] Verify the total never exceeds capacity.
Example:
Total seats: 10

Concurrent requests: 100

Expected:
Successful holds <= 10
Held + Confirmed <= 10

Idempotency test
- [ ] Create a reservation.
- [ ] Confirm it once.
- [ ] Confirm it again.
- [ ] Confirm it multiple times.
- [ ] Verify confirmed seat count increases only once.
Additional recommended tests
- [ ] Invalid class ID.
- [ ] Invalid student ID.
- [ ] Duplicate active application.
- [ ] Reservation expiry.
- [ ] Confirmation after expiry.
- [ ] Cancellation.
- [ ] Cancellation after confirmation.
- [ ] Student ownership validation.
- [ ] No available seats.
- [ ] Database persistence after restart.
Part 3 completion checkpoint
- Concurrency protection works.
- Expiration works.
- Confirmation is idempotent.
- At least three required automated test categories pass.
PART 4 — React Frontend Development
Goal: Build the complete Admin and Guardian interfaces.
4.1 — Frontend project setup
- [ ] Create React project using Vite.
- [ ] Use TypeScript.
- [ ] Install Axios.
- [ ] Install Tailwind CSS.
- [ ] Set up routing.
- [ ] Create reusable components folder.
- [ ] Create pages folder.
- [ ] Create API services folder.
- [ ] Create hooks folder.
- [ ] Create TypeScript types.
- [ ] Configure backend URL through environment variables.
- [ ] Create Axios instance.
- [ ] Configure global error handling.
- [ ] Create a loading component.
- [ ] Create an error component.
- [ ] Create a reusable notification/toast component.
4.2 — Guardian interface
Page 1: Admission dashboard
- [ ] Display available classes.
- [ ] Display class/shift names.
- [ ] Display total seats.
- [ ] Display available seats.
- [ ] Display confirmed seats.
- [ ] Display held seats.
- [ ] Add a reserve button.
- [ ] Disable reservation when no seats are available.
- [ ] Display appropriate error messages.
Page 2: Student identification
The assignment uses X-Student-Id.
- [ ] Provide a student ID input.
- [ ] Validate the input.
- [ ] Store the selected student ID appropriately.
- [ ] Include the header in relevant API requests.
- [ ] Ensure the student ID remains available after browser refresh.
Remember: this is an assignment-specific identification mechanism, not a real authentication system.
Page 3: Reservation details
- [ ] Display reservation ID.
- [ ] Display class name.
- [ ] Display reservation status.
- [ ] Display expiry time.
- [ ] Display countdown timer.
- [ ] Display confirmation button.
- [ ] Display cancellation button.
- [ ] Handle expired reservations.
Page 4: My applications
- [ ] Fetch /me/holds.
- [ ] Display all applications.
- [ ] Display class names.
- [ ] Display statuses.
- [ ] Display expiry timestamps.
- [ ] Display confirmation information.
- [ ] Allow users to view application details.
- [ ] Refresh application data when appropriate.
Page 5: Confirmation
- [ ] Display admission details.
- [ ] Display the ভর্তি ফি দিন button.
- [ ] Call confirmation API.
- [ ] Show loading state.
- [ ] Show success notification.
- [ ] Show failure notification.
- [ ] Prevent accidental repeated clicks.
- [ ] Handle already-confirmed reservations.
4.3 — Admin interface
Page 1: Admin dashboard
- [ ] Display total class offerings.
- [ ] Display total seats.
- [ ] Display held seats.
- [ ] Display confirmed seats.
- [ ] Display available seats.
Page 2: Create class/shift
- [ ] Create a class creation form.
- [ ] Add class/shift name input.
- [ ] Add total seat input.
- [ ] Validate form fields.
- [ ] Submit to POST /classes.
- [ ] Show success notification.
- [ ] Show validation errors.
- [ ] Refresh class listing.
Page 3: Class management
- [ ] Display all classes.
- [ ] Display seat statistics.
- [ ] Display individual class details.
- [ ] Refresh seat availability.
- [ ] Show empty states.
- [ ] Handle API failures.
The assignment requires an Admin UI but doesn't explicitly require a full admin authentication system. Document your decision about admin access in ASSUMPTIONS.md.
4.4 — UI reliability
- [ ] Make the UI responsive.
- [ ] Add loading states.
- [ ] Add empty states.
- [ ] Add error states.
- [ ] Add success notifications.
- [ ] Handle network failures.
- [ ] Handle server errors.
- [ ] Handle unavailable seats.
- [ ] Display EXPIRED clearly.
- [ ] Disable ভর্তি ফি দিন after expiry.
- [ ] Ensure expiry updates without requiring a manual page reload.
- [ ] Refresh application status from the backend.
- [ ] Preserve the student's ongoing applications after browser refresh.
- [ ] Test the UI on desktop and mobile.
Important: A frontend countdown is not the authority for expiry. The backend must decide whether a reservation is still valid.
Part 4 completion checkpoint
You should be able to complete every required operation through the browser:
- Admin creates classes.
- Guardian identifies a student.
- Guardian browses classes.
- Guardian reserves a seat.
- Guardian confirms admission.
- Guardian cancels a reservation.
- Guardian views applications.
No Postman or direct database access should be needed by the reviewer.
PART 5 — Docker, Documentation, GitHub & Final Submission
Goal: Prepare the complete project for evaluation.
5.1 — Backend Docker configuration
- [ ] Create backend Dockerfile.
- [ ] Configure .NET restore.
- [ ] Configure build.
- [ ] Configure publish.
- [ ] Configure runtime image.
- [ ] Expose port 8080.
- [ ] Configure environment variables.
- [ ] Configure database connection.
- [ ] Ensure migrations are applied appropriately at startup.
5.2 — Frontend Docker configuration
- [ ] Create frontend Dockerfile.
- [ ] Configure Node.js build stage.
- [ ] Install dependencies.
- [ ] Build React application.
- [ ] Configure production web server.
- [ ] Expose frontend port.
- [ ] Configure frontend API URL.
- [ ] Verify API URL works inside the Docker environment and browser.
5.3 — PostgreSQL Docker configuration
- [ ] Configure PostgreSQL image.
- [ ] Configure database name.
- [ ] Configure username.
- [ ] Configure password.
- [ ] Configure environment variables.
- [ ] Create persistent Docker volume.
- [ ] Add health check.
- [ ] Verify backend waits for database readiness.
5.4 — Docker Compose
Create root-level:
docker-compose.yml

- [ ] Add PostgreSQL service.
- [ ] Add backend service.
- [ ] Add frontend service.
- [ ] Configure service dependencies.
- [ ] Configure persistent database volume.
- [ ] Configure environment variables.
- [ ] Configure port mapping.
- [ ] Configure networking.
- [ ] Configure health checks.
- [ ] Ensure all three services start together.
Mandatory deployment test
Run:
docker compose up

Verify:
- [ ] PostgreSQL starts.
- [ ] Backend starts.
- [ ] Frontend starts.
- [ ] Backend is accessible at http://localhost:8080.
- [ ] Frontend is accessible.
- [ ] Frontend communicates with backend.
- [ ] Database migrations run.
- [ ] Application works without installing additional software on the reviewer's machine beyond Docker.
Then test persistence:
docker compose down
docker compose up

- [ ] Existing classes remain.
- [ ] Existing applications remain.
- [ ] Confirmed admissions remain.
- [ ] Database data is not lost.
Do not use docker compose down -v during this persistence test because that removes the database volume.
5.5 — Documentation
README.md
- [ ] Project introduction.
- [ ] Project features.
- [ ] Technology stack.
- [ ] Project architecture.
- [ ] Folder structure.
- [ ] Prerequisites.
- [ ] Docker startup instructions.
- [ ] Application URLs.
- [ ] Environment variables.
- [ ] API endpoint documentation.
- [ ] Database information.
- [ ] Automated testing instructions.
- [ ] Example student ID header.
- [ ] Known limitations.
ASSUMPTIONS.md
- [ ] Document decisions not explicitly specified.
- [ ] Explain admin access approach.
- [ ] Explain student identification approach.
- [ ] Explain cancellation behavior.
- [ ] Explain reservation expiry behavior.
- [ ] Explain payment simulation.
- [ ] Explain class/shift representation.
- [ ] Explain any other implementation decisions.
AI_USAGE.md
- [ ] List AI tools used.
- [ ] Explain where AI was used.
- [ ] Describe generated or assisted code.
- [ ] Explain how AI-generated code was reviewed.
- [ ] Explain corrections or modifications you made.
5.6 — GitHub and commits
- [ ] Create Git repository.
- [ ] Create .gitignore.
- [ ] Exclude secrets.
- [ ] Exclude build artifacts.
- [ ] Exclude unnecessary local files.
- [ ] Commit project initialization.
- [ ] Commit database setup.
- [ ] Commit backend APIs.
- [ ] Commit reservation logic.
- [ ] Commit concurrency protection.
- [ ] Commit automated tests.
- [ ] Commit frontend.
- [ ] Commit Docker configuration.
- [ ] Commit documentation.
- [ ] Push to GitHub.
- [ ] Make repository public.
- [ ] Verify repository can be cloned.
- [ ] Verify Docker instructions work from a clean clone.
5.7 — Final quality checklist
- [ ] All eight APIs work.
- [ ] Admin interface works.
- [ ] Guardian interface works.
- [ ] Seat capacity is never exceeded.
- [ ] Duplicate active applications are prevented.
- [ ] Expired reservations release seats.
- [ ] Expired reservations cannot be confirmed.
- [ ] Repeated confirmation is idempotent.
- [ ] Cancellation works according to documented rules.
- [ ] Browser refresh preserves ongoing applications.
- [ ] Errors are understandable.
- [ ] Invalid input returns 4xx, not 500.
- [ ] Required automated tests pass.
- [ ] Docker Compose starts everything.
- [ ] PostgreSQL data persists.
- [ ] All required documentation exists.
- [ ] GitHub repository is public.
- [ ] Final repository has meaningful commit history.
Final project tracking sheet
This is the order I recommend following:
Part	Main goal	Completion condition
1	PRD, architecture, database, backend setup	Database and API foundation working
2	All backend APIs and business logic	Eight endpoints working
3	Concurrency, expiry and testing	Required tests passing
4	React Admin and Guardian UI	Complete workflow works through UI
5	Docker, documentation and submission	Clean Docker deployment and public GitHub repo


Suggested Git commit milestones
Keep commits small and meaningful. For example:
01 - Initialize solution and backend project
02 - Configure PostgreSQL and EF Core
03 - Add admission database entities
04 - Add initial database migration
05 - Implement class management APIs
06 - Implement seat reservation logic
07 - Add reservation expiry service
08 - Implement admission confirmation
09 - Implement cancellation and student applications
10 - Add concurrency protection
11 - Add automated backend tests
12 - Initialize React frontend
13 - Implement guardian interface
14 - Implement admin interface
15 - Add reservation countdown and expiry UI
16 - Configure Dockerfiles
17 - Add Docker Compose
18 - Add project documentation
19 - Final integration and bug fixes