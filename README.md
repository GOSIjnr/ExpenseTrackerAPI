# ExpenseTracker

ExpenseTracker is being rebuilt around a layered ASP.NET Core backend. The active backend currently keeps only identity, session, authentication, and user-management capabilities. The previous backend implementation has been moved to `backend/legacy` so the remaining expense-tracker domains can be reintroduced gradually.

## Backend Layout

```text
backend/
  ExpenseTracker.Api/             ASP.NET Core API and endpoint modules
  ExpenseTracker.Application/     CQRS handlers, validation, responses, services
  ExpenseTracker.Domain/          User/session entities and domain components
  ExpenseTracker.Infrastructure/  Hashing, encryption, caching, mediator pipeline
  ExpenseTracker.Persistence/     EF Core DbContext and user/session mappings
  legacy/                         Previous backend code
```

## Active API Surface

- Authentication: register, login, refresh, logout
- Sessions: list current user sessions, revoke one session, revoke all sessions
- Users: profile, update profile/password, admin lock/unlock/promote/demote/delete flows
- Base info endpoint: `GET /api`

## Configuration

Key backend settings live in `backend/ExpenseTracker.Api/appsettings.json`.

- `ConnectionStrings:Database`
- `DataEncryptionOptions`
- `HashingOptions`
- `SessionLifetimeOptions`
- `CacheTimeToLiveOptions`
- `SuperAdminSeedOptions`
- `Cors:AllowedOrigins`

For local development:

```bash
cd backend/ExpenseTracker.Api
dotnet user-secrets set "ConnectionStrings:Database" "Host=localhost;Port=5432;Database=expense_tracker;Username=postgres;Password=postgres"
dotnet user-secrets set "SuperAdminSeedOptions:FirstName" "Admin"
dotnet user-secrets set "SuperAdminSeedOptions:MiddleName" ""
dotnet user-secrets set "SuperAdminSeedOptions:LastName" "User"
dotnet user-secrets set "SuperAdminSeedOptions:UserName" "admin"
dotnet user-secrets set "SuperAdminSeedOptions:Email" "admin@example.com"
dotnet user-secrets set "SuperAdminSeedOptions:Password" "ChangeMe!123"
```

## Run

```bash
dotnet restore expense-tracker.slnx
dotnet build expense-tracker.slnx
dotnet run --project backend/ExpenseTracker.Api/ExpenseTracker.Api.csproj
```

In development, OpenAPI is available at `/openapi/v1.json` and Scalar at `/scalar`.
