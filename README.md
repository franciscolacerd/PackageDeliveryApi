# PackageDelivery API

Boilerplate de API .NET 10 em **vertical slice architecture**, gerado a partir do template do projecto `Tibi - Api` mas adaptado ao contexto **PackageDelivery**. Acesso a dados **100% Entity Framework Core** (sem Dapper).

## Camadas (7 projectos)

| Projecto | Papel |
|----------|-------|
| **PackageDelivery.Api** | Host web: Controllers, Configuration, Middleware, `Program.cs` |
| **PackageDelivery.Features** | Vertical slices (`Models`/`Repositories`/`Services` por feature) |
| **PackageDelivery.Infrastructure** | EF Core `DbContext`, entidades, Identity, factories |
| **PackageDelivery.Shared** | OperationResponse, Policies, BaseService, TokenProviderOptions, exceptions |
| **PackageDelivery.Api.Tests** | Testes end-to-end (HttpClient) — NUnit |
| **PackageDelivery.Features.Tests** | Testes de integração dos serviços — NUnit |
| **PackageDelivery.Infrastructure.Tests** | Testes de integração de persistência — NUnit |

## Stack / NuGets

- **EF Core 10** + SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`)
- **ASP.NET Core Identity** (`AspNetUser : IdentityUser<long>`) + JWT Bearer + refresh token
- **FluentValidation** 12
- **Serilog** (ficheiro, rotação diária, 7 dias) em `C:/Logs/PackageDelivery.Api/`
- **HealthChecks** (`/health`, `/Healthz`) — SQL, DbContext, Swagger, disco
- **Rate limiting** (fixed / token bucket / per-ip)
- **NWebsec** (headers de segurança), CORS, Polly (retry com jitter)
- Swagger / OpenAPI

> Observabilidade (métricas / OpenTelemetry) foi deliberadamente deixada de fora — será tratada mais tarde de outra forma.

## Funcionalidades incluídas

- **Auth**: `POST /token` (`grant_type=password` e `refresh_token`) → JWT
- **DeliveriesController**: `GET /api/deliveries` — slice de exemplo `Deliveries/GetDeliveries` (entregas do utilizador autenticado, via EF Core). Controller com DI normal a chamar o serviço da feature.
- **Logging de request/response**: gravado **de imediato na BD** (tabela `ApiRestLogs`, BD de logging) pelo `RequestResponseLoggingMiddleware` — sem fila nem background service

## Comandos

```bash
dotnet build PackageDelivery.Solution.slnx
dotnet run --project PackageDelivery.Solution/PackageDelivery.Api
dotnet test PackageDelivery.Solution.slnx
```

## Base de dados (EF Core migrations)

Configurar a connection string `PackageDeliveryConnection` em
`PackageDelivery.Api/appsettings.json`. Depois:

```bash
dotnet ef migrations add InitialCreate --context PackageDeliveryDbContext --project PackageDelivery.Solution/PackageDelivery.Infrastructure --startup-project PackageDelivery.Solution/PackageDelivery.Api
dotnet ef database update --context PackageDeliveryDbContext --project PackageDelivery.Solution/PackageDelivery.Infrastructure --startup-project PackageDelivery.Solution/PackageDelivery.Api
```

> **Nota:** o boilerplate ainda não inclui migrations — as entidades (`AspNet*`, `Delivery`, `ApiRestLog`) estão todas mapeadas no `PackageDeliveryDbContext` (context único) prontas para gerar a primeira migration. Substituir a `SecretKey` do JWT e a connection string antes de usar.
