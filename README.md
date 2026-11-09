# School Admission System

A Dockerized admission demo for class and shift seat management. Parents can reserve a seat, confirm the simulated admission fee, cancel a hold, and review their applications. Administrators can create offerings and review capacity.

## Stack and structure

- ASP.NET Core 10 Web API with Entity Framework Core and PostgreSQL
- React 19 + TypeScript + Vite
- Docker Compose with persistent PostgreSQL storage

```text
backend/       .NET API and domain model
frontend/      React application
docker-compose.yml
ASSUMPTIONS.md
AI_USAGE.md
```

## Run with Docker

Copy `.env.example` to `.env` if you want to customize settings, then run:

```sh
docker compose up --build
```

Open the UI at <http://localhost:3000> and API at <http://localhost:8080>. PostgreSQL data survives container restarts in `postgres_data`. Avoid `docker compose down -v` when you want to keep that data.

## Configuration

`POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `HOLD_DURATION_SECONDS` (default 600), and `VITE_API_URL` (default `http://localhost:8080`) can be set in `.env`.

## API

JSON API endpoints: `POST /classes`, `GET /classes`, `GET /classes/{classId}`, `POST /classes/{classId}/holds`, `GET /holds/{holdId}`, `POST /holds/{holdId}/confirm`, `DELETE /holds/{holdId}`, and `GET /me/holds`.

Student operations use `X-Student-Id: 20152691234567890`. Invalid/missing student IDs return 400; unavailable seats and duplicate active applications return 409. Class creation accepts `{ "name": "Class 6 - Morning Shift", "totalSeats": 120 }`.

## Local development

Run PostgreSQL separately, set `DATABASE_URL`, then run `dotnet run --project backend`. For the frontend, run `cd frontend`, `npm install`, and `npm run dev`; set `VITE_API_URL` to the API origin.

Automated tests are not included in this initial scaffold yet.
