# PackageDelivery API

A .NET 10 Web API for managing package deliveries, built with a **vertical slice architecture**: each feature owns its models, validation, persistence and services.

## Projects

| Project | Responsibility |
|---------|----------------|
| **PackageDelivery.Api** | Web host: controllers, configuration, middleware, `Program.cs` |
| **PackageDelivery.Features** | Feature slices — one folder per operation (`Models` / `Validators` / `Builders` / `Repositories` / `Services`) |
| **PackageDelivery.Infrastructure** | EF Core `DbContext`, entities and ASP.NET Core Identity |
| **PackageDelivery.Shared** | Cross-cutting building blocks (response models, policies, token options, exceptions) |
| **PackageDelivery.Api.Tests** | End-to-end tests — in-process host (`WebApplicationFactory`) against a real SQL Server (Testcontainers) — NUnit |
| **PackageDelivery.Features.Tests** | Feature/service integration tests (Testcontainers) + validator unit tests — NUnit |
| **PackageDelivery.Infrastructure.Tests** | Persistence integration tests (Testcontainers) — NUnit |
| **PackageDelivery.IntegrationTesting** | Shared SQL Server test container (single container reused across test assemblies) |

## Stack

- **Entity Framework Core 10** + SQL Server (single `PackageDeliveryDbContext`)
- **ASP.NET Core Identity** (`AspNetUser : IdentityUser<long>`)
- **Cookie-based authentication** — access + refresh JWTs in `HttpOnly` + `Secure` + `SameSite=Strict` cookies, with server-side refresh-token rotation and reuse detection (a `Bearer` header is still accepted for non-browser clients)
- **Anti-forgery (CSRF)** on state-changing requests + **CORS** with explicit origins (fail-closed)
- **RFC 7807 ProblemDetails** as the unified error contract (400 binding · 422 validation · 500)
- **FluentValidation** for request validation
- **Serilog** (rolling file, configured in `appsettings.json`)
- **Health checks** (`/health`, `/Healthz`)
- **Rate limiting** — per-IP (global fixed window) and per-user (token bucket)
- Security headers (`NetEscapades.AspNetCore.SecurityHeaders`) and Swagger / OpenAPI

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/authentication/login` | Authenticate (JSON `{ username, password }`) — sets the access + refresh cookies, returns `204` |
| `POST` | `/api/authentication/refresh` | Rotate the refresh token and re-issue the cookies (`204`) |
| `GET`  | `/api/authentication/account` | Profile of the authenticated user |
| `POST` | `/api/authentication/logout` | Revoke the refresh token and clear the cookies |
| `GET`  | `/api/authentication/antiforgery/token` | Issue an anti-forgery request token (sent back in the `X-CSRF-TOKEN` header on unsafe requests) |
| `POST` | `/api/deliveries` | Create a delivery (validates the request, generates a barcode and one package per volume) |
| `GET`  | `/api/deliveries` | List the authenticated user's deliveries, paginated (`?page=1&pageSize=20`) — returns a `PagedResult` (`items` plus `page`, `pageSize`, `totalCount`, `totalPages`, `hasPrevious`, `hasNext`) |

See `PackageDelivery.Solution/PackageDelivery.Api/PackageDelivery.Api.http` for ready-to-run requests.

## Feature slices

Each feature under `PackageDelivery.Features/Deliveries` is self-contained:

- **CreateDelivery** — `Models` (request/response), `Validators` (FluentValidation), `Builders` (`DeliveryBuilder` assembles the `Delivery` aggregate and its packages), `Repositories`, `Services`.
- **GetDeliveries** — `Models` (read model), `Repositories`, `Services`.

Controllers depend directly on the feature services; there is no shared/generic repository.

## Request/response logging

`RequestResponseLoggingMiddleware` writes each request/response to the `ApiRestLogs` table synchronously, using the same `PackageDeliveryDbContext`.

## Commands

```bash
dotnet build PackageDelivery.Solution.slnx
dotnet run --project PackageDelivery.Solution/PackageDelivery.Api
dotnet test PackageDelivery.Solution.slnx
```

## Testing

- **Validator unit tests** run anywhere with no external dependencies.
- **Integration and end-to-end tests need Docker.** A single SQL Server container (Testcontainers) is started once and reused across the test assemblies (`PackageDelivery.IntegrationTesting`); migrations are applied to it automatically. When Docker is unavailable the database-backed tests are skipped rather than failing.
- **End-to-end tests** (`PackageDelivery.Api.Tests`) boot the real pipeline in-process with `WebApplicationFactory<Program>`, seed a test user, and exercise the authenticated cookie flows (login → account → refresh rotation → logout, CSRF, validation) over the actual middleware stack.

CI runs the validator unit tests on Windows and the **full suite on Linux** (where Docker is available), so the authenticated flows are covered on every push.

## Containerization

The API ships as a Linux container. There are two ways to run it, **both reusing the same `Dockerfile`** — they are complementary, not alternatives:

| Way | When | Needs .NET SDK? |
|-----|------|-----------------|
| **docker-compose** (`docker-compose.yml`) | Run anywhere with just Docker — colleagues, CI, demos, servers | No |
| **.NET Aspire** (`PackageDelivery.AppHost`) | Dev inner-loop: dashboard (traces/logs/metrics/health) + automatic connection-string wiring | Yes |

The `Dockerfile` is the common base — compose consumes the image, and Aspire builds it under the hood.

Migrations are applied **automatically on startup when `RunMigrations=true`** (set by both compose and Aspire); otherwise the app never touches the schema (see *Database* below).

### docker-compose (API + SQL Server)

```bash
docker compose up --build
```

- API → `http://localhost:8080` (Swagger at `/swagger`); SQL Server → `localhost:1433`.
- Configuration is overridden through environment variables (connection string, `BaseUrl__Uri`, Serilog output path, `RunMigrations`) so the container never reads the Windows-oriented defaults from `appsettings.json`.

### .NET Aspire

```bash
dotnet run --project PackageDelivery.Solution/PackageDelivery.AppHost
```

The AppHost provisions SQL Server in a container, injects the `PackageDeliveryConnection` string into the API, sets `RunMigrations=true` and opens the Aspire dashboard. The API references `PackageDelivery.ServiceDefaults` (OpenTelemetry, health, service discovery, resilience). `PackageDelivery.AppHost` needs the `Aspire.Hosting.SqlServer` package for `AddSqlServer`/`AddDatabase`.

The `api` resource uses `WaitFor(sql)`, so it starts only once SQL Server is healthy. On the **first run** the SQL Server image (~2.3 GB) is pulled, which can take a few minutes — pre-pull it to make startup deterministic:

```bash
docker pull mcr.microsoft.com/mssql/server:2022-latest
```

> The `C:\`-drive disk-storage health check runs on Windows only, so the container reports healthy on Linux.

## Database (EF Core migrations)

The `InitialCreate` migration is already included (it creates the Identity, delivery and logging tables and seeds the `EventTypes` and `DeliveryAttributes` lookups).

In containers/Aspire the migration is applied automatically at startup (`RunMigrations=true`). To run it by hand instead, set the `PackageDeliveryConnection` connection string in
`PackageDelivery.Solution/PackageDelivery.Api/appsettings.json` and replace the JWT `SecretKey`, then apply the migration:

```bash
dotnet ef database update --context PackageDeliveryDbContext --project PackageDelivery.Solution/PackageDelivery.Infrastructure --startup-project PackageDelivery.Solution/PackageDelivery.Api
```

To create further migrations after changing the model:

```bash
dotnet ef migrations add <Name> --context PackageDeliveryDbContext --project PackageDelivery.Solution/PackageDelivery.Infrastructure --startup-project PackageDelivery.Solution/PackageDelivery.Api
```
