# VulnTrack — TODO

> Last updated: 2026-06-20
> Based on RTM audit against original business requirements.

---

## Legend

| Symbol | Meaning |
|---|---|
| ✅ | Completed |
| 🔄 | In Progress (started, not fully done) |
| ⬜ | Pending |
| 🔴 | Priority: High |
| 🟡 | Priority: Medium |
| 🟢 | Priority: Low |

Effort scale: XS < 1 h · S 1–4 h · M 4–8 h · L 1–2 d · XL 3–5 d · XXL 1–2 wk

---

## Backend

### ✅ Completed

- Domain layer — `BaseEntity`, `AuditableEntity`, `IDomainEvent`
- All 9 domain entities: `Vulnerability`, `Team`, `VulnerabilitySource`, `StatusUpdate`, `Attachment`, `UploadBatch`, `ScheduledReminder`, `VulnerabilityComment`, `Asset`
- All 7 enums: `Severity`, `VulnerabilityStatus`, `VulnerabilityType`, `RemediationPriority`, `UploadBatchStatus`, `ReminderStatus`, `AssetType`
- All 6 domain events: `VulnerabilityCreatedEvent`, `VulnerabilityStatusChangedEvent`, `VulnerabilityAssignedEvent`, `VulnerabilityEcdUpdatedEvent`, `UploadBatchCompletedEvent`, `ReminderScheduledEvent`
- Application common: `IApplicationDbContext`, `ICurrentUserService`, `IBlobStorageService`, `IServiceBusPublisher`, `IGraphService`, `PagedResult<T>`, `Result<T>`, `NotFoundException`, `ForbiddenAccessException`, `DomainEventNotification<T>`
- MediatR pipeline: `ValidationBehavior<,>`, `LoggingBehavior<,>`
- Vulnerabilities feature — 12 commands + handlers, 4 queries + handlers, 9 validators, 3 event handlers
- Teams feature — 3 commands + handlers, 2 queries + handlers, 2 validators
- Sources feature — 3 commands + handlers, 1 query + handler, 2 validators
- Reminders feature — `ProcessDueRemindersCommand` (email via Graph), `GetPendingRemindersQuery`, `CancelReminderCommand`
- UploadBatches feature — `CreateUploadBatchCommand` (Blob upload), `GetUploadBatchesQuery`, `GetUploadBatchByIdQuery`
- `ApplicationDbContext` with domain-event dispatch in `SaveChangesAsync`
- EF Fluent configurations for all 9 entities (soft-delete filters, string-backed enums)
- `BlobStorageService` — user-delegation SAS, Managed Identity safe
- `ServiceBusPublisher` — `ConcurrentDictionary` sender cache, `IAsyncDisposable`
- `GraphService` — `GetUserAsync`, `SearchUsersAsync`, `SendEmailAsync`
- `CurrentUserService`, `DependencyInjection.cs`
- `ExceptionHandlingMiddleware` (404 / 403 / 400 mapping)
- `VulnerabilitiesController` — 15 endpoints
- `TeamsController` — 5 endpoints
- `SourcesController` — 5 endpoints
- `UploadBatchesController` — 3 endpoints
- `RemindersController` — 2 endpoints
- `Program.cs` — Entra ID auth, Swagger + OAuth2 PKCE, OpenTelemetry, CORS, health checks
- Azure Functions: `VulnerabilityEventProcessor` (Service Bus trigger), `SlaReminderTimer` (daily 08:00 UTC)
- Domain.Tests — `VulnerabilityTests`, `UploadBatchTests`, `ScheduledReminderTests`
- Application.Tests — `CreateVulnerabilityCommandHandlerTests`, `UpdateVulnerabilityStatusCommandHandlerTests`, `ProcessDueRemindersCommandHandlerTests`, `ValidationBehaviorTests`, `TestDbContext`, `TestServiceProvider`
- Api.Tests — `VulnerabilitiesControllerTests` (11 scenarios), `TestAuthHandler`, `VulnTrackWebApplicationFactory`

### 🔄 In Progress

- `AssetsController` — stub exists (`GET /api/assets` returns empty array); handler wiring pending

### ⬜ Pending

| Task | Priority | Effort | Dependencies |
|---|---|---|---|
| Add `DbSet<Asset> Assets` to `IApplicationDbContext` and `ApplicationDbContext` | 🔴 | XS | None |
| Assets feature — `GetAssetsQuery` + handler | 🔴 | S | `IApplicationDbContext.Assets` |
| Assets feature — `GetAssetByIdQuery` + handler | 🔴 | S | `IApplicationDbContext.Assets` |
| Assets feature — `CreateAssetCommand` + handler + validator | 🔴 | S | `IApplicationDbContext.Assets` |
| Assets feature — `UpdateAssetCommand` + handler + validator | 🟡 | S | `CreateAssetCommand` |
| Assets feature — `DeleteAssetCommand` + handler | 🟡 | S | `CreateAssetCommand` |
| Wire `AssetsController` to Asset CQRS handlers (CRUD) | 🔴 | S | Asset handlers above |
| `SearchUsersQuery` — Application-layer query wrapping `IGraphService.SearchUsersAsync` for use from controllers | 🟡 | S | None |
| `GET /api/users/search?q=` endpoint exposing Graph user search to frontend | 🟡 | S | `SearchUsersQuery` |
| `UploadBatch` processing pipeline — publish `vulnerability-events` ServiceBus message after blob upload so `VulnerabilityEventProcessor` can parse and create vulnerabilities | 🟡 | L | Service Bus wiring, file-parse logic |
| Asset.Tests — unit tests for Asset domain entity | 🟢 | S | Asset entity (done) |
| Assets feature handler tests | 🟢 | M | Asset handlers |
| `AssetsController` integration tests | 🟢 | M | `AssetsController` wiring |

---

## Frontend

### ✅ Completed

- MSAL auth (`msal.ts`), silent token acquisition Axios interceptor (`apiClient.ts`)
- `AppLayout` — sidebar with Dashboard and Vulnerabilities nav links, user info, sign-out
- `DashboardPage` — 4 KPI stat cards (Total, Open, Critical/High, Remediated) + severity distribution bar
- `VulnerabilitiesPage` — paginated list, filter bar, "+ Add vulnerability" button
- `VulnerabilityDetailPage` — full header card (all fields), description/solution panels, 4-tab panel (history, comments, attachments, reminders)
- `VulnerabilityFilters` — debounced text search, Severity dropdown, Status dropdown, clear-all
- `VulnerabilityTable` — columns: VN#, Server Name/IP, Type, Severity, Status, Assigned To, ECD, Updated
- `CreateVulnerabilityModal` — react-hook-form + zod; fields: Server Name, IP, Type, Severity, Description, Source, Solution, CVE ID, CVSS Score
- `UpdateStatusModal` — status transition with optional comment
- `SeverityBadge`, `StatusBadge`, `Spinner`, `Pagination`, `Modal`
- `vulnerabilityService` — `getAll`, `getById`, `create`, `updateStatus`, `assign`, `setEcd`, `addComment`, `getAttachmentDownloadUrl`, `deleteAttachment`, `delete`
- `sourceService`, `teamService`
- `LoginPage`
- Type definitions: `VulnerabilityListItem`, `VulnerabilityDetail`, `Team`, `VulnerabilitySource`, `PagedResult<T>`, all request/response DTOs

### 🔄 In Progress

- `VulnerabilityDetailPage` comments tab — displays comments; add-comment form not yet built
- `VulnerabilityDetailPage` reminders tab — displays reminders; schedule-reminder form not yet built
- `AssetsPage` — file exists; renders heading and placeholder text only

### ⬜ Pending

| Task | Priority | Effort | Dependencies |
|---|---|---|---|
| Add "Team" column to `VulnerabilityTable` | 🔴 | XS | None (`teamName` already in `VulnerabilityListItem` type) |
| Add "Follow-up Due" column to `VulnerabilityTable` | 🔴 | XS | None (`followUpDue` already in type) |
| Add Team filter dropdown to `VulnerabilityFilters` (calls `GET /api/teams`) | 🟡 | S | `teamService.getAll()` (done) |
| Add Follow-up Due date filter to `VulnerabilityFilters` | 🟡 | S | None |
| Add-comment form in `CommentsTab` (textarea + submit button calling `vulnerabilityService.addComment`) | 🔴 | S | None |
| Schedule-reminder form/button in `RemindersTab` (recipient email, date picker, optional message) | 🔴 | M | `vulnerabilityService.scheduleReminder()` (needs adding) |
| Add `scheduleReminder()` method to `vulnerabilityService` | 🔴 | XS | None |
| User search picker component — typeahead backed by `GET /api/users/search` for Assign flow | 🟡 | M | `GET /api/users/search` endpoint |
| Assign-vulnerability UI on detail page (user picker + optional team select) | 🟡 | M | User search picker |
| Add Team, Assigned To, Discovered At fields to `CreateVulnerabilityModal` | 🟡 | S | `teamService.getAll()` (done) |
| Excel upload UI — file picker modal or dedicated page (`/upload`) with source selector and drag-and-drop | 🔴 | L | `uploadBatchService` (below) |
| `uploadBatchService` — `create(sourceId, file)`, `getAll()`, `getById()` | 🔴 | S | None (API endpoints done) |
| "File Vulnerability" nav item in `AppLayout` sidebar | 🔴 | XS | Excel upload page/modal |
| Register `/upload` route in `App.tsx` | 🔴 | XS | Excel upload page |
| Dashboard filters — filter KPIs by Team, Source, or date range | 🟡 | L | `teamService`, `sourceService` (done) |
| Dashboard search — global search box navigating to filtered Inventory | 🟢 | M | None |
| `AssetsPage` — full implementation: asset table, create/edit/delete | 🟡 | XL | Asset API endpoints, `assetService` |
| `assetService` — CRUD methods | 🟡 | S | Asset API endpoints |
| Register `/assets` route in `App.tsx` and add Assets nav item | 🟡 | XS | `AssetsPage` |
| Upload batch status polling — show processing progress on upload result | 🟢 | M | Excel upload UI |

---

## Database

### ✅ Completed

- Handwritten EF Core initial migration (`20260620000000_Initial.cs`) — all 9 tables, 26 indexes, FK constraints
- `ApplicationDbContextModelSnapshot.cs`
- EF Fluent API configurations for all 9 entities
- Soft-delete global query filters (`IsDeleted`)
- Enum-as-string column conversions for all enum properties
- Domain-event dispatch in `SaveChangesAsync`

### ⬜ Pending

| Task | Priority | Effort | Dependencies |
|---|---|---|---|
| Install .NET 8 SDK (`winget install Microsoft.DotNet.SDK.8`) — required to run `dotnet ef` | 🔴 | XS | None |
| Run `dotnet ef database update` against a provisioned Azure SQL instance | 🔴 | XS | Azure SQL provisioned (see Azure section), .NET SDK |
| Migration: add `Assets` DbSet to `IApplicationDbContext` (triggers EF model change) | 🔴 | XS | `IApplicationDbContext` update |
| Verify soft-delete filters are applied consistently in all multi-entity queries (no orphan joins to deleted rows) | 🟡 | S | None |

---

## Azure

### ✅ Completed

- `BlobStorageService` — `DefaultAzureCredential`, user-delegation SAS, `BlobUriBuilder`
- `ServiceBusPublisher` — namespace sender cache, `IAsyncDisposable`
- `GraphService` — user lookup, user search, send mail (app permission)
- `DependencyInjection.cs` — explicit config-missing exceptions for all Azure settings
- `appsettings.json` — all connection values as placeholder tokens (no secrets in repo)
- OpenTelemetry wired to Azure Monitor (`UseAzureMonitor()`)
- Health checks: SQL (`AddDbContextCheck`), Blob (`AddAzureBlobStorage`), Service Bus (`AddAzureServiceBusQueue`)

### ⬜ Pending

| Task | Priority | Effort | Dependencies |
|---|---|---|---|
| Provision Azure SQL Server + Database (Basic tier) | 🔴 | S | Azure subscription |
| Provision Storage Account + `vulnerability-attachments` blob container (private access) | 🔴 | S | Azure subscription |
| Provision Service Bus namespace + `vulnerability-events` queue | 🔴 | S | Azure subscription |
| Provision App Service Plan + Web App (API) | 🔴 | S | Azure subscription |
| Provision Function App (Consumption plan) + storage account for Functions | 🔴 | S | Azure subscription |
| Provision Application Insights resource | 🟡 | XS | Azure subscription |
| Create Managed Identity for API Web App; assign roles: SQL `db_owner`, Storage `Blob Data Contributor`, Service Bus `Data Owner` | 🔴 | M | App Service provisioned |
| Create Managed Identity for Function App; assign same roles + Graph `Mail.Send` | 🔴 | M | Function App provisioned |
| Write Bicep templates in `infra/` for all resources above (parameterized for dev/prod) | 🟡 | XL | All provisioning decisions finalized |
| Populate `appsettings.json` tokens with real values in App Service / Function App configuration (not in source) | 🔴 | S | Azure resources provisioned |

---

## Security

### ✅ Completed

- `[Authorize]` applied to all 6 controllers
- `Microsoft.Identity.Web` JWT Bearer validation
- MSAL Auth Code PKCE on frontend (no implicit flow)
- `DefaultAzureCredential` throughout Infrastructure (no hardcoded secrets)
- All sensitive values stored as placeholder tokens; `appsettings.*.local.json` and `local.settings.json` gitignored
- `ExceptionHandlingMiddleware` — no stack traces leaked in production error responses

### ⬜ Pending

| Task | Priority | Effort | Dependencies |
|---|---|---|---|
| Register Entra ID app — API (`VulnTrack.Api`): expose `access_as_user` scope, set `accessTokenAcceptedVersion: 2` | 🔴 | S | Entra ID tenant access |
| Register Entra ID app — Frontend SPA: add `http://localhost:5173` and production redirect URIs | 🔴 | S | Entra ID tenant access |
| Replace `__TENANT_ID__` and `__CLIENT_ID__` in `appsettings.json` with real values | 🔴 | XS | App registrations above |
| Update frontend `.env` / MSAL config with real Client ID and Tenant ID | 🔴 | XS | App registrations above |
| Grant admin consent for Graph `Mail.Send` application permission on the API/Functions managed identity | 🔴 | S | Managed Identity provisioned, Graph app permission registered |
| Replace `__NOTIFICATION_SENDER_EMAIL__` in config with a licensed M365 mailbox address | 🔴 | XS | M365 mailbox available |
| CORS `AllowedOrigins` — replace `https://vulntrack.example.com` with actual production frontend URL | 🔴 | XS | Frontend hosting URL known |
| Confirm role-based authorization requirements — currently any authenticated user can mutate any record; add role checks if needed | 🟡 | M | Business decision on RBAC model |
| Enable Azure Defender for SQL on provisioned database | 🟢 | XS | Azure SQL provisioned |

---

## Deployment

### ✅ Completed

- `.gitignore` covers `appsettings.*.local.json`, `local.settings.json`, `bin/`, `obj/`
- `VulnTrack.sln` with all 8 projects
- `Directory.Build.props`
- `README.md`

### ⬜ Pending

| Task | Priority | Effort | Dependencies |
|---|---|---|---|
| GitHub Actions: `build.yml` — on PR: restore, build all projects, run all 3 test suites | 🔴 | M | None |
| GitHub Actions: `deploy-api.yml` — on push to `main`: build + publish API, deploy to Azure Web App | 🔴 | M | App Service provisioned, `AZURE_CREDENTIALS` secret in repo |
| GitHub Actions: `deploy-functions.yml` — on push to `main`: build + publish Functions, deploy to Function App | 🔴 | M | Function App provisioned |
| GitHub Actions: `deploy-frontend.yml` — on push to `main`: `npm run build`, deploy to Azure Static Web Apps or Blob + CDN | 🔴 | M | Hosting resource provisioned |
| Choose and provision frontend hosting: Azure Static Web Apps (recommended — built-in CDN, free tier) vs Storage Account + Azure CDN | 🔴 | S | Azure subscription |
| Configure production environment variables in App Service and Function App (connection strings, client IDs) | 🔴 | S | Azure resources + app registrations |
| Configure staging slot on App Service for zero-downtime deployments | 🟢 | S | App Service provisioned |
| Set up Azure Key Vault and reference secrets from App Service config (instead of plaintext app settings) | 🟡 | M | Azure subscription, Managed Identity |

---

## Summary

| Category | Completed | In Progress | Pending |
|---|---|---|---|
| Backend | 47 items | 1 | 13 |
| Frontend | 20 items | 3 | 19 |
| Database | 6 items | 0 | 4 |
| Azure | 8 items | 0 | 10 |
| Security | 6 items | 0 | 9 |
| Deployment | 4 items | 0 | 8 |
| **Total** | **91** | **4** | **63** |

### Critical path to first deployable build

1. Entra ID app registrations (Security)
2. Azure resource provisioning — SQL, Storage, Service Bus, App Service, Function App (Azure)
3. Run EF migration against Azure SQL (Database)
4. GitHub Actions build + deploy pipelines (Deployment)
5. Replace all placeholder config tokens (Security / Azure)

### Critical path to full feature parity (RTM gaps)

1. Assets feature — backend handlers + controller wiring (Backend)
2. Team and Follow-up Due columns in inventory table (Frontend)
3. Add-comment form on detail page (Frontend)
4. Schedule-reminder UI on detail page (Frontend)
5. Excel upload page + `uploadBatchService` (Frontend)
6. "File Vulnerability" nav item (Frontend)
7. Dashboard filters and search (Frontend)
