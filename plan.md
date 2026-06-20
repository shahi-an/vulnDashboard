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

### Application layer — ~75%
**Complete:**
- Common: `IApplicationDbContext`, `ICurrentUserService`, `IBlobStorageService`, `IServiceBusPublisher`, `IGraphService`
- Common: `PagedResult<T>`, `Result`, `NotFoundException`, `ForbiddenAccessException`, `DomainEventNotification<T>`
- Common: `ValidationBehavior<,>`, `LoggingBehavior<,>`
- **Vulnerabilities feature** — 100%: 12 commands + handlers, 4 queries + handlers, 9 validators, 3 event handlers
- **Teams feature** — commands + queries + validators created; **HANDLERS MISSING**
- **Sources feature** — commands + queries + validators created; **HANDLERS MISSING**

**Missing (see Remaining section):**
- Team handlers (5 files)
- Source handlers (4 files)
- UploadBatches feature (commands, queries, handlers)
- `ProcessDueRemindersCommand` + handler (used by SlaReminderTimer)
- `GetPendingRemindersQuery` + handler (used by RemindersController)
- `CancelReminderCommand` + handler (used by RemindersController)

### Infrastructure layer — 100%
- `ApplicationDbContext` with domain-event dispatch in `SaveChangesAsync`
- EF configurations for all 9 entities (Fluent API, soft-delete filters, string-backed enums)
- `BlobStorageService` — user delegation SAS (Managed Identity safe), `BlobUriBuilder` for authenticated client
- `ServiceBusPublisher` — `ConcurrentDictionary` sender cache, `IAsyncDisposable`
- `GraphService`, `CurrentUserService`
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

### Priority 1 — Application layer gaps (blockers: API already wired, Functions uses ProcessDueReminders)

#### 1a. Team handlers (5 files in `Application/Features/Teams/`)
- `CreateTeamCommandHandler.cs`
- `UpdateTeamCommandHandler.cs`
- `DeleteTeamCommandHandler.cs`
- `GetTeamsQueryHandler.cs`
- `GetTeamByIdQueryHandler.cs`

#### 1b. Source handlers (4 files in `Application/Features/Sources/`)
- `CreateVulnerabilitySourceCommandHandler.cs`
- `UpdateVulnerabilitySourceCommandHandler.cs`
- `ToggleSourceActiveCommandHandler.cs`
- `GetVulnerabilitySourcesQueryHandler.cs`

#### 1c. Reminder handlers (in `Application/Features/Vulnerabilities/` or a new `Reminders/` feature)
- `GetPendingRemindersQuery.cs` + handler — used by `GET /api/reminders?dueBefore=...`
- `CancelReminderCommand.cs` + handler — used by `DELETE /api/reminders/{id}`
- `ProcessDueRemindersCommand.cs` + handler — used by `SlaReminderTimer`; queries DB for pending reminders where ScheduledFor <= now, sends emails via GraphService, marks Sent/Failed, returns count

#### 1d. UploadBatch feature (new `Application/Features/UploadBatches/`)
- `GetUploadBatchesQuery.cs` + handler — paginated list with optional sourceId filter
- `GetUploadBatchByIdQuery.cs` + handler
- `CreateUploadBatchCommand.cs` + handler — accepts file stream, uploads raw file to Blob, creates UploadBatch record, publishes to ServiceBus for async processing

### Priority 2 — Tests (none exist yet)

Test projects are scaffolded in the solution (`tests/`) but have zero test files.

#### 2a. `VulnTrack.Domain.Tests`
- Entity factory tests (Vulnerability.Create, UploadBatch state machine, ScheduledReminder transitions)
- Domain event raising tests

#### 2b. `VulnTrack.Application.Tests`
- Handler unit tests with mocked IApplicationDbContext (Moq + EF InMemory)
- ValidationBehavior pipeline tests
- Focus: CreateVulnerabilityCommandHandler, UpdateVulnerabilityStatusCommandHandler, ProcessDueRemindersCommandHandler

#### 2c. `VulnTrack.Api.Tests`
- WebApplicationFactory integration tests with EF InMemory
- Happy-path + error-case tests for VulnerabilitiesController

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

### ProcessDueRemindersCommand — critical details
The SLA timer calls: `await mediator.Send(new ProcessDueRemindersCommand(), ct)`  
Handler must:
1. Query `ScheduledReminders WHERE Status = 'Pending' AND ScheduledFor <= DateTimeOffset.UtcNow` (ignoring global filter since ScheduledReminder has no soft-delete)
2. For each: call `IGraphService.SendEmailAsync(recipientEmail, subject, body)`
3. On success: call `reminder.MarkSent()` + `db.SaveChangesAsync()`
4. On failure: call `reminder.MarkFailed(reason)` + save
5. Return `int` count of processed reminders

### IGraphService.SendEmailAsync
`GraphService` at `Infrastructure/Services/Graph/GraphService.cs` wraps the Microsoft Graph SDK. Ensure this method exists and is wired in DI.

### Upload batch flow
1. Controller receives `IFormFile`, calls `CreateUploadBatchCommand`
2. Handler uploads raw file to blob: `await blobService.UploadAsync(stream, fileName, contentType, containerName, ct)`
3. Creates `UploadBatch` record with `Status = Queued` and `RawFileBlobUri`
4. Publishes `UploadBatchCreatedEvent` to ServiceBus queue `vulnerability-events`
5. `VulnerabilityEventProcessor` (Functions) consumes the event and processes the file asynchronously

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

Please start with Priority 1 from the plan: implement the missing Application layer handlers.
Begin with 1a (Team handlers), then 1b (Source handlers), then 1c (Reminder handlers
including ProcessDueRemindersCommand), then 1d (UploadBatch feature).
Follow the exact same patterns as the existing Vulnerability handlers in:
src/backend/VulnTrack.Application/Features/Vulnerabilities/Commands/
```

---

## Commit history
- `d2344d1` — Initial Repository Setup
- (multiple commits across 2 sessions — application layer, infrastructure fixes, frontend, migrations)
- `7e8eee9` — EF Core initial migration (handwritten)

All code is on `main`. No branches.
