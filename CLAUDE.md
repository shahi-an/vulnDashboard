# CLAUDE.md — VulnTrack Project Memory

> This file is the authoritative reference for all Claude Code sessions working on this repository.
> Read this file before doing any implementation work. Do not re-architect existing decisions
> unless the user explicitly requests it.

---

## Project Purpose

VulnTrack is a cloud-native **Vulnerability Management Portal** for tracking, triaging, and remediating server and infrastructure vulnerabilities across teams. It centralises vulnerability data from multiple scanning sources, assigns ownership, tracks remediation progress through status workflows, and sends automated SLA reminder emails when estimated completion dates approach. Users authenticate via Microsoft Entra ID (formerly Azure AD) and interact through a React single-page application backed by an ASP.NET Core 8 REST API.

---

## Business Requirements

### Navigation
- **Home** — Dashboard landing page with KPI summary
- **Inventory** — Paginated, filterable list of all vulnerabilities
- **Search** — Free-text search across server name, IP, and vulnerability number
- **File Vulnerability** — Entry point for creating vulnerabilities (manual form and Excel upload)

### Inventory columns
Server Name · Server IP · Vulnerability Type · Severity · Status · Assigned To · Team · Last Updated · Follow-up Due · ECD

### Vulnerability Detail
All data fields · Status history timeline · Comments (internal and external) · Attachments (upload/download) · Reminders (schedule and view)

### File Vulnerability
- Manual entry form — create a single vulnerability with validation
- Excel upload — batch ingest via file upload processed asynchronously

### Integrations
- **Entra ID** — authentication and identity for all users
- **Microsoft Graph user lookup** — search and resolve users when assigning vulnerabilities
- **Email reminders** — automated SLA reminder emails sent via Microsoft Graph when vulnerabilities are due

### Dashboard
- KPI cards — Total, Open, Critical/High Open, Remediated counts
- Severity distribution bar — Critical / High / Other breakdown of open items
- Filters — filter KPIs by team, source, or date range *(pending)*
- Search *(pending)*

---

## Technology Stack

### Backend
| Technology | Version | Role |
|---|---|---|
| ASP.NET Core | 8 | Web API host |
| .NET | 8 | Runtime |
| Entity Framework Core | 8 | ORM / migrations |
| MediatR | latest | CQRS dispatcher |
| FluentValidation | latest | Request validation |
| AutoMapper | latest | Object mapping (available; used selectively) |
| Microsoft.Identity.Web | latest | JWT Bearer auth (Entra ID) |
| Azure.Storage.Blobs | latest | Blob Storage SDK |
| Azure.Messaging.ServiceBus | latest | Service Bus SDK |
| Microsoft.Graph | latest | Graph SDK (user lookup, email) |
| Azure.Identity | latest | `DefaultAzureCredential` |
| Azure.Monitor.OpenTelemetry.AspNetCore | latest | Telemetry export to Azure Monitor |
| Swashbuckle | latest | OpenAPI / Swagger |
| AspNetCore.HealthChecks.* | latest | Health check endpoints |

### Azure Functions
| Technology | Version | Role |
|---|---|---|
| Azure Functions | v4 isolated worker | Background processing host |
| .NET | 8 | Isolated worker runtime |

### Frontend
| Technology | Version | Role |
|---|---|---|
| React | 18 | UI framework |
| TypeScript | latest | Type safety |
| Vite | latest | Build tool; dev proxy to `https://localhost:7001` |
| TanStack Query | v5 | Server-state management, caching, invalidation |
| React Router | latest | Client-side routing |
| MSAL (`@azure/msal-react`) | latest | Entra ID Auth Code PKCE |
| Axios | latest | HTTP client with bearer token interceptor |
| react-hook-form | latest | Form state management |
| Zod | latest | Form schema validation |
| Tailwind CSS | latest | Utility-first styling |
| date-fns | latest | Date formatting |

### Testing
| Technology | Role |
|---|---|
| xUnit | Test runner for all backend test projects |
| FluentAssertions | Readable assertion DSL |
| Moq | Mock creation for external dependencies |
| EF Core InMemory | In-process database for unit/integration tests |
| WebApplicationFactory | Full API integration tests without a real server |

---

## Architecture Decisions

### Clean Architecture with vertical slices
The solution is organised into four layers with a strict dependency rule — outer layers depend on inner layers, never the reverse.

```
Domain → Application → Infrastructure
                     → Api
                     → Functions
```

- **Domain** — zero NuGet dependencies; contains entities, enums, and domain events only
- **Application** — depends on Domain; defines interfaces (`IApplicationDbContext`, etc.) that Infrastructure implements; contains all CQRS handlers
- **Infrastructure** — implements Application interfaces; all Azure SDK and EF Core code lives here
- **Api / Functions** — entry points only; delegate immediately to MediatR

### CQRS via MediatR
Every user-initiated operation is a MediatR `IRequest<T>`. There are no service classes. Handlers are in `Application/Features/{Feature}/Commands/` or `Queries/`. Handlers are `internal sealed` classes; commands/queries are `public sealed record`s. Commands that mutate state return `Result<T>` or `Result`; queries return the DTO directly.

### Feature-folder organisation
```
Application/Features/
  Vulnerabilities/
    Commands/        ← Command record + handler in same file
    Queries/         ← Query record + handler in same file
    Validators/      ← One validator per command (separate file)
    EventHandlers/   ← Domain event notification handlers
  Teams/
  Sources/
  Reminders/
  UploadBatches/
```

### Result pattern
Commands return `Result<T>` or `Result`. Never throw for expected business failures. Throw only for unexpected infrastructure failures (which the middleware catches). The `ExceptionHandlingMiddleware` maps `NotFoundException → 404`, `ForbiddenAccessException → 403`, `ValidationException → 400`.

### Domain events
Entities raise domain events by appending to an internal `List<IDomainEvent>`. `ApplicationDbContext.SaveChangesAsync` dispatches all events as `DomainEventNotification<T>` via MediatR and then clears them. `IDomainEvent` is a plain marker interface — no MediatR reference in Domain.

### No secrets in source
All Azure connection strings, tenant IDs, client IDs, and email addresses are stored as `__PLACEHOLDER_TOKEN__` strings in `appsettings.json`. Real values live only in App Service / Function App configuration in Azure (or `appsettings.*.local.json` which is gitignored).

### DefaultAzureCredential everywhere
All Azure SDK clients (`BlobServiceClient`, `ServiceBusClient`, `GraphServiceClient`) are constructed with `DefaultAzureCredential`. This works with local developer identity (`az login`) in development and Managed Identity in production. No connection strings or client secrets are used for Azure services.

### Soft delete
All entities inheriting `AuditableEntity` support soft delete via `IsDeleted`, `DeletedAt`, `DeletedBy`. EF global query filters exclude soft-deleted rows automatically.

### Enum storage
All enum columns use `.HasConversion<string>().HasMaxLength(20)`. Enums are stored as their string names, not integer ordinals, for human-readable data and safe reordering.

### Frontend data fetching
All server state goes through TanStack Query. Each resource has a consistent `queryKey` array (e.g., `['vulnerabilities', filters]`). After mutations, `queryClient.invalidateQueries` is called to refetch affected lists. No global state manager (Redux, Zustand) is used.

### Frontend auth
MSAL `AuthenticatedTemplate` / `UnauthenticatedTemplate` gates all routes. The Axios `apiClient` silently acquires a fresh access token via `msalInstance.acquireTokenSilent` before every request. The token scope is `api://{CLIENT_ID}/access_as_user`.

---

## Azure Services

| Service | Config key | Why it exists |
|---|---|---|
| **Azure SQL Database** | `ConnectionStrings:DefaultConnection` | Primary relational store for all entities. EF Core with Managed Identity auth (no SQL password). |
| **Azure Blob Storage** | `AzureStorage:AccountName`, `AzureStorage:AttachmentsContainer` | Stores vulnerability attachment files and raw Excel upload files. Container is private; files are served via short-lived user-delegation SAS URLs generated by `BlobStorageService`. |
| **Azure Service Bus** | `ServiceBus:Namespace`, `ServiceBus:VulnerabilityEventsQueue` | Decouples the API from the async Excel batch processing pipeline. The API enqueues a message; the Functions app processes it. Also has a `notifications` queue. |
| **Microsoft Graph** | `MicrosoftGraph:BaseUrl`, `Graph:SenderEmail` | Two uses: (1) user lookup and search when assigning vulnerabilities (`Users.Read` delegated); (2) sending SLA reminder emails from a shared mailbox (`Mail.Send` application permission). |
| **Microsoft Entra ID** | `AzureAd:TenantId`, `AzureAd:ClientId` | Identity provider. API validates JWT Bearer tokens issued by Entra. Frontend uses Auth Code PKCE to acquire tokens. |
| **Azure Functions** | — | Hosts two background functions: `VulnerabilityEventProcessor` (Service Bus trigger — processes uploaded Excel batches) and `SlaReminderTimer` (Timer trigger, daily 08:00 UTC — sends due reminder emails). |
| **Azure Monitor / Application Insights** | `ApplicationInsights:ConnectionString` | Receives OpenTelemetry traces, logs, and metrics from the API via `UseAzureMonitor()`. |
| **Managed Identity** | — | Both the API Web App and Function App use system-assigned Managed Identity. No passwords or client secrets for Azure service-to-service auth. Required roles: SQL `db_owner`, Storage `Blob Data Contributor`, Service Bus `Data Owner`, Graph `Mail.Send`. |

---

## Coding Standards

### C# patterns

**Command / query records** — always `public sealed record`:
```csharp
public sealed record CreateTeamCommand(string Name, string? Description) : IRequest<Result<Guid>>;
```

**Handlers** — always `internal sealed class`, primary constructor injection:
```csharp
internal sealed class CreateTeamCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateTeamCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTeamCommand request, CancellationToken ct) { ... }
}
```

**Validators** — one per command, in `Validators/` folder, `internal sealed class`:
```csharp
internal sealed class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
```

**Result pattern** — never throw for expected failures:
```csharp
return Result<Guid>.Success(entity.Id);
return Result<Guid>.Failure("Name already exists.");
```

**EF queries** — always `AsNoTracking()` in query handlers; only track in command handlers where `SaveChangesAsync` is called.

**CancellationToken** — always pass through as the last parameter, named `ct` or `cancellationToken`.

**Enum EF config** — always:
```csharp
builder.Property(x => x.Status)
    .HasConversion<string>()
    .HasMaxLength(20);
```

**Comments** — write no comments unless the reason is non-obvious (hidden constraint, workaround, subtle invariant). Never document what the code obviously does.

### React / TypeScript patterns

**Service functions** — plain objects exporting async functions; never classes:
```ts
export const vulnerabilityService = {
  getAll: (params) => apiClient.get('/api/vulnerabilities', { params }).then(r => r.data),
};
```

**Forms** — always `react-hook-form` + `zodResolver`. Define the Zod schema first, infer `FormValues` from it.

**Query keys** — always arrays: `['vulnerabilities', filters]`, `['vulnerability', id]`, `['teams']`.

**Mutations** — call `queryClient.invalidateQueries` in `onSuccess` to keep the cache consistent.

**Components** — functional only; no class components. Co-locate sub-components in the same file when they are only used by one parent.

**Tailwind** — no custom CSS files. Use `cn()` utility for conditional class merging.

---

## Database Design

All entities inherit from `BaseEntity` (Id: Guid, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, domain events list). Entities that support deletion inherit `AuditableEntity` (adds IsDeleted, DeletedAt, DeletedBy).

| Entity | Key relationships | Purpose |
|---|---|---|
| `Vulnerability` | belongs to `VulnerabilitySource`, optionally `Team`; has many `StatusUpdate`, `Attachment`, `VulnerabilityComment`, `ScheduledReminder` | Core aggregate. Tracks server/IP, type, severity, status, ECD, CVE, CVSS, assignment. |
| `Team` | has many `Vulnerability` | Organisational unit owning a group of vulnerabilities. |
| `VulnerabilitySource` | has many `Vulnerability`, `UploadBatch` | Named scanning source (e.g., Nessus, manual). Can be active or inactive. |
| `StatusUpdate` | belongs to `Vulnerability` | Immutable audit record of every status transition with optional comment and actor. |
| `Attachment` | belongs to `Vulnerability`; optionally `UploadBatch` | File metadata + Blob URI for files attached to a vulnerability. |
| `UploadBatch` | belongs to `VulnerabilitySource`; has many `Attachment` | Tracks a single Excel file upload. State machine: Queued → Processing → Completed / CompletedWithErrors / Failed / Cancelled. |
| `ScheduledReminder` | belongs to `Vulnerability` | A single email reminder event. State machine: Pending → Sent / Failed / Skipped / Cancelled. |
| `VulnerabilityComment` | belongs to `Vulnerability` | Free-text comment; flagged as internal or external. |
| `Asset` | standalone | Registered server/infrastructure asset. Currently no Application layer handlers — see Remaining Work. |

### Migration
One handwritten initial migration (`20260620000000_Initial.cs`). To apply: install .NET 8 SDK, then:
```
cd src/backend
dotnet ef database update --project VulnTrack.Infrastructure --startup-project VulnTrack.Api
```

---

## Current Status

### Fully complete
- **Domain layer** — all 9 entities, 7 enums, 6 domain events, `BaseEntity`, `AuditableEntity`, `IDomainEvent`
- **Application layer** — all CQRS handlers for Vulnerabilities (12 commands, 4 queries, 9 validators, 3 event handlers), Teams (3 commands, 2 queries), Sources (3 commands, 1 query), Reminders (`ProcessDueRemindersCommand` with Graph email, `GetPendingRemindersQuery`, `CancelReminderCommand`), UploadBatches (1 command, 2 queries); common models, interfaces, and pipeline behaviours
- **Infrastructure layer** — `ApplicationDbContext`, 9 EF Fluent configurations, `BlobStorageService`, `ServiceBusPublisher`, `GraphService`, `CurrentUserService`, `DependencyInjection`
- **EF migration** — all 9 tables, 26 indexes, FK constraints
- **API** — `VulnerabilitiesController` (15 endpoints), `TeamsController` (5), `SourcesController` (5), `UploadBatchesController` (3), `RemindersController` (2); `ExceptionHandlingMiddleware`, `Program.cs` with Entra ID auth, Swagger, CORS, health checks, OpenTelemetry
- **Azure Functions** — `VulnerabilityEventProcessor` (Service Bus trigger), `SlaReminderTimer` (Timer trigger, daily 08:00 UTC, dispatches `ProcessDueRemindersCommand`)
- **Frontend** — Auth flow (MSAL, Axios interceptor), `DashboardPage` (4 KPI cards, severity bar), `VulnerabilitiesPage` (paginated + filtered list, create modal), `VulnerabilityDetailPage` (all fields, 4 read-only tabs: history/comments/attachments/reminders), `AppLayout` (sidebar nav), all UI components (`SeverityBadge`, `StatusBadge`, `Spinner`, `Pagination`, `Modal`), `vulnerabilityService`, `sourceService`, `teamService`
- **Tests** — `VulnTrack.Domain.Tests` (3 files), `VulnTrack.Application.Tests` (5 files), `VulnTrack.Api.Tests` (3 files, 11 integration scenarios)

---

## Remaining Work

See `TODO.md` in the repository root for the full prioritised list with effort estimates and dependencies. Key gaps are summarised below.

### Feature gaps (RTM)
- **Inventory table** — Team column and Follow-up Due column not rendered in `VulnerabilityTable`
- **Add comment** — `CommentsTab` on detail page is read-only; no submit form
- **Schedule reminder** — `RemindersTab` on detail page is read-only; no schedule button or form
- **Excel upload** — backend fully implemented; no frontend UI exists (no file picker, no upload page, no `uploadBatchService`, no nav item)
- **Dashboard filters and search** — `DashboardPage` shows aggregate KPIs only; no filter or search controls
- **Assets feature** — entity, EF config, and controller stub exist; `IApplicationDbContext` does not expose `DbSet<Asset>`; no Application handlers; frontend `AssetsPage` is a blank placeholder

### Infrastructure gaps
- Entra ID app registrations not provisioned; all config values are placeholder tokens
- `infra/` and `.github/workflows/` directories exist but are empty — no Bicep templates and no CI/CD pipelines

---

## Future Sessions Instructions

1. **Read this file first** before writing any code in a new session. Also check `TODO.md` for current priority and `plan.md` for historical context.

2. **Do not re-architect** existing decisions (Clean Architecture layers, CQRS via MediatR, `Result<T>` pattern, `DefaultAzureCredential`, TanStack Query, react-hook-form + zod) unless the user explicitly requests a change.

3. **Follow existing patterns exactly**:
   - New commands → `public sealed record` + `internal sealed class` handler in the same file, validator in `Validators/`
   - New queries → same structure, `AsNoTracking()` in the handler
   - New controllers → `[Authorize]`, primary constructor injection of `IMediator`, return `IActionResult`
   - New frontend pages → functional component, TanStack Query for data, `vulnerabilityService`-style service object
   - New forms → `react-hook-form` + `zodResolver`, Zod schema defined first

4. **Before adding a new entity**: update `IApplicationDbContext` with the new `DbSet<T>`, add a Fluent API configuration file in `Infrastructure/Data/Configurations/`, and create a new EF migration.

5. **No secrets in code**. New config values go into `appsettings.json` as `__TOKEN_NAME__` placeholders. Document the token name here and in `TODO.md`.

6. **Minimise complexity**. Do not add abstractions, base classes, or helpers unless the same pattern appears in three or more places. Do not add error handling for scenarios that cannot occur. Do not add feature flags.

7. **Update this file** if a new major architecture decision is made, a new Azure service is added, or the project status changes significantly.
