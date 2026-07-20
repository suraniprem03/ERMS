# ERMS MASTER CONTEXT

## Project Overview
The **Expense Reimbursement Management System (ERMS)** is a modern, full-stack application built to track and manage employee expense claims (Travel, Food, Hotel, Recharge).

## Architecture 
**Backend:** .NET 8 Web API
**Database:** PostgreSQL (with EF Core)
**Design Pattern:** Clean Architecture (API, Application, Domain, Infrastructure). 
**Rules:**
- **NO Repository Pattern**. Direct EF Core `DbContext` usage via standard Services.
- **NO MediatR / CQRS**. Standard Service classes with Interfaces injected via DI.
- **Entity Configurations**: Fluent API ONLY. No DataAnnotations on Entities.
- **Naming Conventions**: PostgreSQL `snake_case` database naming via `EFCore.NamingConventions`.

## Security & Auth
- JWT Bearer Authentication.
- All users log in via `Email`.
- Employees are strictly tied to a `User` entity for system access. (Automatically created when Admin creates Employee).
- Passwords are auto-generated as `P@ssw0rd123!` for new Employees, with `IsPasswordChangeRequired` flag logic supported.

## Core Modules & API Structure
All controllers wrap responses in standard `ApiResponse<T>` wrappers.
1. **Auth (`/api/auth`)**: Login and Token Management.
2. **Employees (`/api/employees`)**: Manages Employee records and auto-syncs `User` credentials.
3. **States (`/api/states`) & Areas (`/api/areas`)**: Master tables for geographical data. Deactivation rules prevent deactivating active parent objects.
4. **Expense Categories (`/api/expense-categories`)**: Drives the polymorphic claim logic. (e.g. TRAVEL, FOOD).
5. **Claims (`/api/claims`)**: The core engine. 
   - Strongly-typed JSON structures for different claim types.
   - State machine: `Draft (1) -> Submitted (2) -> Approved (4) / Rejected (5) / Returned (3)`.
   - Immutable audit trail via `ClaimStatusHistory`.
6. **Attachments (`/api/attachments`)**: Supports `.png`, `.jpg`, `.jpeg`, and `.pdf` local filesystem uploads tied directly to Draft/Returned claims.
7. **Reports (`/api/reports`)**: Supports exporting all claims to Excel (`.xlsx`) via ClosedXML.

## Frontend Future Directives
When building the frontend (e.g., React or Angular):
- Make sure to fetch `ExpenseCategories` first to render dynamic forms. 
- The payload sent to `/api/claims` changes dynamically based on the selected category (e.g., if Category = TRAVEL, you must nest the data inside `travelDetails: {}`).
- Attachments should use `FormData` (multipart/form-data) via `POST /api/attachments/claim/{claimId}`.
