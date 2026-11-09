# AI Usage Report

## Tools Used
- **Claude Code (Anthropic)**: Primary agent for architecture, implementation, and orchestration.

## AI Contribution
AI was used throughout the entire lifecycle of the project:
1. **Planning**: Translating the development roadmap into a technical implementation plan.
2. **Backend Implementation**: Generating the .NET 8 API structure, EF Core configurations, and business logic for seat allocation.
3. **Concurrency Logic**: Implementing the `SELECT FOR UPDATE` pattern to handle race conditions.
4. **Frontend Development**: Building the React/TypeScript interface and managing state for the reservation countdown.
5. **DevOps**: Generating the Dockerfiles and Docker Compose configuration.

## Review & Modification Process
Every piece of AI-generated code underwent the following process:
1. **Static Analysis**: Code was reviewed for adherence to the provided technology stack (.NET 8, React 18).
2. **Logic Verification**: The concurrency logic was specifically audited to ensure it prevented overbooking.
3. **Testing**: Logic was verified using xUnit tests with `InMemoryDatabase` (for unit logic) and planned for PostgreSQL (for locking logic).
4. **Correction**: 
    - Modified the initial `AdmissionService` to fix a variable shadowing bug in the expiry loop.
    - Adjusted the frontend `App.tsx` to properly handle routing and navigation.
    - Fixed project reference issues during the test suite setup.
