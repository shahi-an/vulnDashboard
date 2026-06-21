# VulnTrack — TODO

> Last updated: 2026-06-21 (session 5)
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

### ✅ Completed (session 2–3)

- `GetAssetsQuery` + handler + `GetAssetByIdQuery` + handler
- `CreateAssetCommand` + handler + `CreateAssetCommandValidator`
- `UpdateAssetCommand` + handler + `UpdateAssetCommandValidator`
- `DeleteAssetCommand` + handler
- `AssetsController` — GET list, GET by id, POST, PUT, DELETE (fully wired)
- `SearchUsersQuery` + handler (wraps `IGraphService.SearchUsersAsync`)
- `UsersController` — `GET /api/users/search?q=`
- `GetVulnerabilitiesQuery` — `CreatedAfter`/`CreatedBefore`/`FollowUpDueBefore` params
- `GetVulnerabilitiesQueryHandler` — applies all three date filters
- `VulnerabilitiesController` — accepts all date params
- `CreateVulnerabilityCommand` — added optional `TeamId`
- `AssignVulnerabilityCommand` — `Email` is now nullable (team-only assignment allowed)
- `AssignVulnerabilityCommandValidator` — requires email OR teamId
- `VulnerabilityAssignedEvent` — `AssignedToEmail` now nullable
- `Asset.Update()` domain method — includes `description` param

### ✅ Completed (session 4)

- `UploadBatch` pipeline — `CreateUploadBatchCommandHandler` now publishes to `vulnerability-events` ServiceBus queue after blob upload
- `SearchUsersQuery` — Application handler tests (3 scenarios)
- `AssetHandlerTests` — 12 Application-layer tests covering all 5 handlers
- `AssetTests` — 5 Domain entity unit tests
- `AssetsControllerTests` — 10 Api integration tests covering all 5 endpoints
- Upload batch status polling — `FileVulnerabilityPage` polls `GET /api/upload-batches/{id}` every 3 s until terminal status; shows progress bar, counts, and error summary

### ✅ Completed (session 5 — all tests green)

- `DependencyInjection.cs` — `AddValidatorsFromAssembly` now passes `includeInternalTypes: true` so all `internal sealed class` validators are registered (fixes `ValidationBehaviorTests` 4 failures)
- `TestDbContext.SaveChangesAsync` — fixed EF Core InMemory cascade-state quirk: new entities added to private backing-field collections of a `Modified` principal are cascade-marked `Modified` instead of `Added` because the non-sentinel Guid PK confuses EF Core. Fix detects these (state = Modified, no property actually changed) and resets to `Added` before save (fixes `UpdateVulnerabilityStatusCommandHandlerTests` 3 failures; `ProcessDueRemindersCommandHandlerTests` 3 failures already fixed in session 4)
- `VulnTrackWebApplicationFactory` — added `ResponseBodyPipeWriterFixFilter` / `CompatResponseBodyFeature` to replace the test host's `ResponseBodyPipeWriter` (which throws on `UnflushedBytes` in .NET 10 runtime via `RollForward=LatestMajor`) with a `StreamPipeWriter`-backed feature that properly implements `UnflushedBytes` (fixes 13 Api test failures)
- `VulnerabilitiesControllerTests` — fixed Location header assertion to use `ContainEquivalentOf` (case-insensitive) matching `/api/Vulnerabilities/` from `[controller]` route token
- **All 94 tests pass**: Domain.Tests 37/37, Application.Tests 36/36, Api.Tests 21/21

### ⬜ Pending (all low priority)

| Task | Priority | Effort |
|---|---|---|
| `SearchUsersQuery` — Application-layer query wrapping `IGraphService.SearchUsersAsync` for use from controllers | ✅ | — |
| `GET /api/users/search?q=` endpoint exposing Graph user search to frontend | ✅ | — |

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

### ✅ Completed (session 1–3)

- Team and Follow-up Due columns in `VulnerabilityTable`
- Team filter + Follow-up Due date filter in `VulnerabilityFilters`
- Add-comment form in `CommentsTab`
- Schedule-reminder form in `RemindersTab` + `vulnerabilityService.scheduleReminder()`
- `FileVulnerabilityPage` — Manual entry + Excel upload tabs
- `uploadBatchService` — `create`, `getAll`
- "File Vulnerability" nav item + `/file-vulnerability` route
- `AssetsPage` — table, search/type filters, create/edit/delete modals
- `assetService` — `getAll`, `getById`, `create`, `update`, `delete`
- Assets nav item + `/assets` route
- `userService` — user search via Graph
- `UserSearchPicker` — typeahead component backed by `/api/users/search`
- Assign button + modal on `VulnerabilityDetailPage` (user picker + team)
- `CreateVulnerabilityModal` — added Team dropdown + Discovered At date
- Dashboard team / source / date-range filters + global search box (navigates to inventory)
- `VulnerabilitiesPage` reads initial search from dashboard navigation state

### ⬜ Pending (low priority)

| Task | Priority | Effort |
|---|---|---|
| `UploadBatch` processing pipeline — publish ServiceBus message after blob upload | 🟡 | L |
| Upload batch status polling on upload result page | 🟢 | M |
| ~~Add Assigned To field to `CreateVulnerabilityModal`~~ | ✅ | S |

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
