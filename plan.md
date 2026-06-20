# VulnTrack — Build Plan

> Last updated: 2026-06-20  
> Working directory: `C:\Projects\vulnerabilityApp`  
> Git branch: `main` (all work committed here)

---

## Project at a glance

Cloud-native Vulnerability Management Portal. Clean Architecture + CQRS (MediatR). React frontend + ASP.NET Core 8 API + Azure Functions. All Azure services via Managed Identity (DefaultAzureCredential). No secrets in code — placeholder tokens only.

**Tech stack:**
- Backend: ASP.NET Core 8, EF Core 8 / Azure SQL, Azure Blob Storage, Azure Service Bus, Microsoft Graph, Entra ID
- Frontend: React 18 + TypeScript + Vite + TanStack Query v5 + Tailwind CSS + MSAL
- Functions: Azure Functions v4 isolated worker
- Auth: Microsoft.Identity.Web (API) + MSAL Auth Code PKCE (frontend)
- Observability: OpenTelemetry → Azure Monitor

---

## Layer structure

```
src/
  backend/
    VulnTrack.Domain          — entities, enums, domain events (zero NuGet deps)
    VulnTrack.Application     — MediatR CQRS handlers, FluentValidation, AutoMapper
    VulnTrack.Infrastructure  — EF Core, Azure SDK services, DI wiring
    VulnTrack.Api             — ASP.NET Core Web API, controllers, middleware
    VulnTrack.Functions       — Azure Functions (ServiceBus + Timer triggers)
  frontend/
    src/                      — React app (types, services, components, pages)
tests/
  VulnTrack.Domain.Tests
  VulnTrack.Application.Tests
  VulnTrack.Api.Tests
```

---

## COMPLETED ✅

### Domain layer — 100%
- `BaseEntity` (Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, domain events)
- `AuditableEntity` (+ IsDeleted, DeletedAt, DeletedBy)
- Entities: `Vulnerability`, `Team`, `VulnerabilitySource`, `UploadBatch`, `Asset`, `StatusUpdate`, `Attachment`, `ScheduledReminder`, `VulnerabilityComment`
- Enums: `Severity`, `VulnerabilityStatus`, `VulnerabilityType`, `RemediationPriority`, `UploadBatchStatus`, `ReminderStatus`, `AssetType`
- Domain events: `VulnerabilityCreatedEvent`, `VulnerabilityStatusChangedEvent`, `VulnerabilityAssignedEvent`, `VulnerabilityEcdUpdatedEvent`, `UploadBatchCompletedEvent`, `ReminderScheduledEvent`

### Application layer — 100%
- Common: `IApplicationDbContext`, `ICurrentUserService`, `IBlobStorageService`, `IServiceBusPublisher`, `IGraphService` (+ `SendEmailAsync`)
- Common: `PagedResult<T>`, `Result`, `NotFoundException`, `ForbiddenAccessException`, `DomainEventNotification<T>`
- Common: `ValidationBehavior<,>`, `LoggingBehavior<,>`
- **Vulnerabilities feature** — 100%: 12 commands + handlers, 4 queries + handlers, 9 validators, 3 event handlers
- **Teams feature** — 100%: 3 commands + handlers, 2 queries + handlers, 2 validators (handlers co-located in command/query files)
- **Sources feature** — 100%: 3 commands + handlers, 1 query + handler, 2 validators (handlers co-located)
- **Reminders feature** — 100%: `GetPendingRemindersQuery`, `CancelReminderCommand`, `ProcessDueRemindersCommand` (sends email via `IGraphService.SendEmailAsync` directly)
- **UploadBatches feature** — 100%: `GetUploadBatchesQuery`, `GetUploadBatchByIdQuery`, `CreateUploadBatchCommand`

### Infrastructure layer — 100%
- `ApplicationDbContext` with domain-event dispatch in `SaveChangesAsync`
- EF configurations for all 9 entities (Fluent API, soft-delete filters, string-backed enums)
- `BlobStorageService` — user delegation SAS (Managed Identity safe), `BlobUriBuilder` for authenticated client
- `ServiceBusPublisher` — `ConcurrentDictionary` sender cache, `IAsyncDisposable`
- `GraphService` — `GetUserAsync`, `SearchUsersAsync`, `SendEmailAsync` (app-permission `Users[senderEmail].SendMail`)
- `CurrentUserService`, `GraphSettings` (requires `Graph:SenderEmail` in config)
- `DependencyInjection.cs` with explicit config-missing exceptions
- **EF Core initial migration** — handwritten (no SDK installed), all 9 tables, 26 indexes, FK constraints

### API layer — 100%
- `ExceptionHandlingMiddleware` (NotFoundException→404, ForbiddenAccess→403, ValidationException→400)
- Controllers: `VulnerabilitiesController` (14 endpoints), `TeamsController` (5), `SourcesController` (5), `UploadBatchesController` (3), `RemindersController` (2), `AssetsController`
- `Program.cs` — Entra ID auth, Swagger, OpenTelemetry, CORS, health checks (SQL + Blob + ServiceBus)

### Functions app — 100%
- `VulnerabilityEventProcessor` — ServiceBus trigger on `vulnerability-events` queue
- `SlaReminderTimer` — Timer trigger (daily 08:00 UTC), sends `ProcessDueRemindersCommand` via MediatR

### Frontend — 100%
- Auth: MSAL + Entra ID, Axios interceptor for silent token acquisition
- Types: `VulnerabilityListItem`, `VulnerabilityDetail`, `Team`, `VulnerabilitySource`, `PagedResult<T>`, all DTOs
- Services: `vulnerabilityService`, `sourceService`, `teamService`
- Components: `AppLayout`, `SeverityBadge`, `StatusBadge`, `VulnerabilityFilters` (debounced), `VulnerabilityTable`, `CreateVulnerabilityModal` (react-hook-form + zod), `UpdateStatusModal`, `Spinner`, `Pagination`, `Modal`
- Pages: `DashboardPage` (4 stat cards + severity bar), `VulnerabilitiesPage` (paginated + filtered), `VulnerabilityDetailPage` (4 tabs: history, comments, attachments, reminders)

### EF Migrations — 100%
- `20260620000000_Initial.cs` — creates all 9 tables in FK-dependency order
- `ApplicationDbContextModelSnapshot.cs` — full EF 8.0.6 model snapshot

---

## REMAINING ❌

### Priority 1 — COMPLETE ✅ (done session 2026-06-20)

All Application layer handlers implemented. Email delivery wired: `ProcessDueRemindersCommand`
calls `IGraphService.SendEmailAsync` → Graph `Users[senderEmail].SendMail` with app permissions.
Requires `Graph:SenderEmail` config value in appsettings / env vars on both API and Functions.

### Priority 2 — COMPLETE ✅ (done session 2026-06-20)

#### 2a. `VulnTrack.Domain.Tests` ✅
- `VulnerabilityTests.cs` — Create factory, priority derivation, domain events, UpdateStatus, Assign, SetEcd, ScheduleReminder
- `UploadBatchTests.cs` — state machine (Queued→Processing→Completed/CompletedWithErrors/Failed/Cancelled), UploadBatchCompletedEvent
- `ScheduledReminderTests.cs` — MarkSent, MarkFailed, Cancel, Skip transitions

#### 2b. `VulnTrack.Application.Tests` ✅
- `Common/TestDbContext.cs` — EF Core InMemory context implementing IApplicationDbContext
- `Common/TestServiceProvider.cs` — DI builder: MediatR + TestDbContext + mocked external deps
- `Handlers/CreateVulnerabilityCommandHandlerTests.cs`
- `Handlers/UpdateVulnerabilityStatusCommandHandlerTests.cs`
- `Handlers/ProcessDueRemindersCommandHandlerTests.cs` — email success, skip (remediated), email failure, no-op
- `Behaviours/ValidationBehaviorTests.cs` — valid pass, empty server name, invalid IP, bad CVSS, bad CVE format
- Also added `Microsoft.EntityFrameworkCore` to `VulnTrack.Application.csproj` (was missing for IApplicationDbContext)

#### 2c. `VulnTrack.Api.Tests` ✅
- `Infrastructure/TestAuthHandler.cs` — replaces JWT Bearer in tests (auto-authenticates)
- `Infrastructure/VulnTrackWebApplicationFactory.cs` — overrides EF (InMemory), mocks Azure services, overrides auth
- `Controllers/VulnerabilitiesControllerTests.cs` — per-test factory isolation; 11 scenarios covering GET/POST/PATCH + 400/404 error paths
- Added `Moq` package to `VulnTrack.Api.Tests.csproj`

### Priority 3 — Azure infrastructure (deploy to cloud)

Nothing exists yet for infrastructure-as-code.

#### 3a. Bicep templates (`infra/` directory)
Resources needed:
- Azure SQL Server + Database (Basic tier for dev)
- Storage Account + Blob container (`vulnerability-attachments`, private)
- Service Bus namespace + queue (`vulnerability-events`)
- App Service Plan + Web App (API)
- Function App (consumption plan)
- Application Insights
- Managed Identity + role assignments:
  - API/Functions identity → SQL `db_owner` via AAD auth
  - API/Functions identity → Storage `Storage Blob Data Contributor`
  - API/Functions identity → Service Bus `Azure Service Bus Data Owner`
  - API identity → Graph API `Mail.Send` (for email notifications)

#### 3b. Entra ID app registrations (manual or scripted)
- API app registration (exposes `VulnTrack.Api` scope)
- Frontend SPA app registration (redirect URIs, `accessTokenAcceptedVersion: 2`)
- Update `appsettings.json` tokens: `__AZURE_TENANT_ID__`, `__API_CLIENT_ID__`, `__FRONTEND_CLIENT_ID__`

### Priority 4 — CI/CD

No pipeline files exist.

#### 4a. GitHub Actions workflows (`.github/workflows/`)
- `build.yml` — on PR: restore, build all projects, run all tests
- `deploy-api.yml` — on push to main: build + publish API, deploy to Azure Web App
- `deploy-functions.yml` — on push to main: build + publish Functions, deploy to Function App
- `deploy-frontend.yml` — on push to main: `npm run build`, deploy to Azure Static Web Apps (or Blob + CDN)

---

## Key implementation notes for new session

### Missing handler patterns (copy from existing handlers)
All handlers follow this pattern:
```csharp
// Query handler example
internal sealed class GetTeamsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetTeamsQuery, List<TeamDto>>
{
    public async Task<List<TeamDto>> Handle(GetTeamsQuery request, CancellationToken ct)
    {
        return await db.Teams
            .AsNoTracking()
            .Select(t => new TeamDto { ... })
            .ToListAsync(ct);
    }
}
```

### ProcessDueRemindersCommand — IMPLEMENTED
Handler in `Application/Features/Reminders/Commands/ProcessDueRemindersCommand.cs`:
1. Queries `ScheduledReminders WHERE Status = Pending AND ScheduledFor <= now` (include Vulnerability)
2. If `Vulnerability.Status == Remediated` → `reminder.Skip()`
3. Else → `IGraphService.SendEmailAsync(recipientEmail, subject, htmlBody)` → `reminder.MarkSent()`
4. On exception → `reminder.MarkFailed(ex.Message)`
5. Returns count of processed reminders (sent + skipped)

### IGraphService.SendEmailAsync — IMPLEMENTED
`GraphService.SendEmailAsync` calls `graphClient.Users[senderEmail].SendMail.PostAsync(...)`.
Requires **application permission** `Mail.Send` on the Managed Identity.
Configure sender mailbox via `Graph:SenderEmail` in appsettings.

### Upload batch flow — IMPLEMENTED
1. Controller receives `IFormFile` → `CreateUploadBatchCommand(sourceId, fileName, stream, contentType)`
2. Handler: uploads to Blob via `IBlobStorageService.UploadAsync` → creates `UploadBatch` (Status=Queued) with `RawFileBlobUri`
3. No synchronous ServiceBus publish — async processing by Functions `VulnerabilityEventProcessor` on `vulnerability-events` (wire via domain event or manual trigger in next iteration)

### Enum-as-string pattern (all enum columns)
```csharp
builder.Property(x => x.Status)
    .HasConversion<string>()
    .HasMaxLength(20);
```

### dotnet SDK not installed
`dotnet --version` returns "not found". To run migrations on a real database, install the SDK first:
```
winget install Microsoft.DotNet.SDK.8
```
Then: `cd src/backend && dotnet ef database update --project VulnTrack.Infrastructure --startup-project VulnTrack.Api`

---

## How to continue in a new Claude Code session

Open Claude Code in `C:\Projects\vulnerabilityApp` and paste:

```
Read plan.md — this is our VulnTrack project build plan.

Priority 1 is complete. Please tackle Priority 2: write tests.
Start with 2a (Domain.Tests — entity factory tests and domain event tests),
then 2b (Application.Tests — handler unit tests with Moq + EF InMemory),
then 2c (Api.Tests — WebApplicationFactory integration tests).
```

---

## Commit history
- `d2344d1` — Initial Repository Setup
- (multiple commits across 2 sessions — application layer, infrastructure fixes, frontend, migrations)
- `7e8eee9` — EF Core initial migration (handwritten)

All code is on `main`. No branches.
