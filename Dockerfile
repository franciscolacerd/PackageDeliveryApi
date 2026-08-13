# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# csproj primeiro → camada de restore cacheável
COPY PackageDelivery.Solution/PackageDelivery.Api/PackageDelivery.Api.csproj                       PackageDelivery.Solution/PackageDelivery.Api/
COPY PackageDelivery.Solution/PackageDelivery.Features/PackageDelivery.Features.csproj             PackageDelivery.Solution/PackageDelivery.Features/
COPY PackageDelivery.Solution/PackageDelivery.Infrastructure/PackageDelivery.Infrastructure.csproj PackageDelivery.Solution/PackageDelivery.Infrastructure/
COPY PackageDelivery.Solution/PackageDelivery.Shared/PackageDelivery.Shared.csproj                 PackageDelivery.Solution/PackageDelivery.Shared/
RUN dotnet restore PackageDelivery.Solution/PackageDelivery.Api/PackageDelivery.Api.csproj

# resto do código → publish
COPY . .
RUN dotnet publish PackageDelivery.Solution/PackageDelivery.Api/PackageDelivery.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/logs
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PackageDelivery.Api.dll"]