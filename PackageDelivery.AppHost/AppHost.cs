var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

// nome do recurso = nome da connection string que a API lê
var db = sql.AddDatabase("PackageDeliveryConnection", databaseName: "PackageDelivery");

builder.AddProject<Projects.PackageDelivery_Api>("api")
    .WithReference(db)
    .WaitFor(db)
    .WithEnvironment("RunMigrations", "true");

builder.Build().Run();