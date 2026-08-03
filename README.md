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
| `GET`  | `/api/deliveries` | List the authenticated user's deliveries |

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

## Database (EF Core migrations)

The `InitialCreate` migration is already included (it creates the Identity, delivery and logging tables and seeds the `EventTypes` and `DeliveryAttributes` lookups).

Set the `PackageDeliveryConnection` connection string in
`PackageDelivery.Solution/PackageDelivery.Api/appsettings.json` and replace the JWT `SecretKey`, then apply the migration:

```bash
dotnet ef database update --context PackageDeliveryDbContext --project PackageDelivery.Solution/PackageDelivery.Infrastructure --startup-project PackageDelivery.Solution/PackageDelivery.Api
```

To create further migrations after changing the model:

```bash
dotnet ef migrations add <Name> --context PackageDeliveryDbContext --project PackageDelivery.Solution/PackageDelivery.Infrastructure --startup-project PackageDelivery.Solution/PackageDelivery.Api
```
