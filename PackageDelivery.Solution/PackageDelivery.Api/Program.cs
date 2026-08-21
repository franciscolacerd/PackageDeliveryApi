using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PackageDelivery.Api.Configuration;
using PackageDelivery.Api.Middleware;
using PackageDelivery.Features;
using PackageDelivery.Infrastructure;
using PackageDelivery.Infrastructure.Context;
using PackageDelivery.Shared.Models;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddConfiguration();

var tokenProviderOptions = Authentication.AddAuthentication(builder.Services, builder.Configuration);

builder.Services.AddPackageDeliveryServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCorsPolicies(builder.Configuration);
builder.Services.AddRateLimiting(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddEndpointsApiExplorer();
builder.Services.UseHealthChecks<PackageDeliveryDbContext>(builder.Configuration);
builder.Services.AddSwagger();
builder.Services.AddSecurity();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("RunMigrations"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PackageDeliveryDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUIConfig();

app.UseApiLoggingMiddleware();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(Policies.CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseMiddleware<TokenProviderMiddleware>(Options.Create(tokenProviderOptions));

app.UseExceptionMiddleware();
app.MapHealthChecks();
app.UseSecurity();

app.MapControllers();

await app.RunAsync();