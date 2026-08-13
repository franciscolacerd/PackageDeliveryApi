# PackageDelivery API

A .NET 10 Web API for managing package deliveries, built with a **vertical slice architecture**: each feature owns its models, validation, persistence and services.

## Projects

| Project | Responsibility |
|---------|----------------|
| **PackageDelivery.Api** | Web host: controllers, configuration, middleware, `Program.cs` |
| **PackageDelivery.Features** | Feature slices — one folder per operation (`Models` / `Validators` / `Builders` / `Repositories` / `Services`) |
| **PackageDelivery.Infrastructure** | EF Core `DbContext`, entities and ASP.NET Core Identity |
| **PackageDelivery.Shared** | Cross-cutting building blocks (response models, policies, token options, exceptions) |
| **PackageDelivery.Api.Tests** | End-to-end tests (HttpClient) — NUnit |
| **PackageDelivery.Features.Tests** | Feature/service integration tests — NUnit |
| **PackageDelivery.Infrastructure.Tests** | Persistence integration tests — NUnit |

## Stack

- **Entity Framework Core 10** + SQL Server (single `PackageDeliveryDbContext`)
- **ASP.NET Core Identity** (`AspNetUser : IdentityUser<long>`) with JWT bearer authentication and refresh tokens
- **FluentValidation** for request validation
- **Serilog** (rolling file, configured in `appsettings.json`)
- **Health checks** (`/health`, `/Healthz`)
- **Rate limiting** (fixed window, token bucket, per-IP)
- Security headers, CORS and Swagger / OpenAPI

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/token` | Authenticate (`grant_type=password`) or renew (`grant_type=refresh_token`) — returns a JWT |
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
dotnet run --project PackageDelivery.AppHost
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
