# School Admission System

A complete school admission system built with .NET 8, React, and PostgreSQL, designed to handle high-concurrency seat reservations with first-come-first-served logic and automatic expiry.

## 🚀 Features
- **Admin Dashboard**: Create and manage class offerings and track overall seat availability.
- **Guardian Interface**: Identify students, browse available classes, and reserve seats.
- **Reservation System**: 10-minute holds on seats to prevent overbooking during the confirmation process.
- **Concurrency Protection**: Row-level locking (`SELECT FOR UPDATE`) to ensure seat counts are always accurate.
- **Automatic Expiry**: Seats from expired reservations are automatically released back to the pool.
- **Idempotent Confirmations**: Safe, repeated confirmation requests without duplicating seat counts.

## 🛠 Technology Stack
- **Backend**: .NET 8 Web API, Entity Framework Core
- **Database**: PostgreSQL
- **Frontend**: React 18, TypeScript, Vite, Tailwind CSS, Axios
- **Testing**: xUnit with EF Core InMemory unit tests and an opt-in PostgreSQL concurrency test
- **Infrastructure**: Docker, Docker Compose

## 📁 Project Structure
```
/
├── AdmissionSystem.Api/    # Backend Web API
│   ├── Controllers/        # API Endpoints
│   ├── Services/           # Business Logic
│   ├── Data/               # DB Context & Migrations
│   └── Entities/           # Database Models
├── AdmissionSystem.Tests/  # Automated Tests
├── frontend/               # React Frontend
│   ├── src/api/            # API Client
│   ├── src/components/     # Reusable UI
│   └── src/pages/          # Application Pages
└── docker-compose.yml      # Orchestration
```

## 🛠 Setup & Execution

### Prerequisites
- [Docker](https://www.docker.com/products/docker-desktop/) installed and running.

### Startup Instructions
1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd school-admission-system
   ```

2. Start the entire stack:
   ```bash
   cp .env.example .env
   docker compose up -d
   ```

3. Access the application:
   - **Frontend**: `http://localhost:5173`
   - **Backend API**: `http://localhost:8080`
   - **Swagger UI**: `http://localhost:8080/swagger`

## 📡 API Documentation
All requests to hold/confirm/cancel endpoints require the `X-Student-Id` header.

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/classes` | Create a new class offering |
| `GET` | `/classes` | List all classes and availability |
| `GET` | `/classes/{id}` | Get specific class details |
| `POST` | `/classes/{id}/holds` | Reserve a seat (10m hold) |
| `GET` | `/holds/{id}` | Check reservation status |
| `POST` | `/holds/{id}/confirm` | Confirm admission |
| `DELETE` | `/holds/{id}` | Cancel reservation |
| `GET` | `/me/holds` | View all student applications |

## 🧪 Testing
Run automated tests using the .NET CLI:
```bash
dotnet test AdmissionSystem.Tests/AdmissionSystem.Tests.csproj
```

## ⚙️ Environment Variables
| Variable | Default | Description |
| :--- | :--- | :--- |
| `DATABASE_URL` | `Host=db;Database=admission_db;...` | PostgreSQL connection string used by Compose |
| `VITE_API_URL` | `http://localhost:8080` | Frontend API endpoint |
| `HOLD_DURATION_SECONDS`| `600` | Duration of seat reservations |

`VITE_API_URL` is a frontend build-time setting. Set it in `.env` before running `docker compose up --build` when the API is hosted at a different browser-accessible URL. `DATABASE_URL` in the Compose environment should use the `db` service hostname (as in `.env.example`).

### Automated tests

Run the unit tests with `dotnet test AdmissionSystem.Tests/AdmissionSystem.Tests.csproj`. The PostgreSQL row-locking concurrency test runs when `TEST_DATABASE_URL` points to a disposable PostgreSQL database; otherwise xUnit reports it as skipped.
