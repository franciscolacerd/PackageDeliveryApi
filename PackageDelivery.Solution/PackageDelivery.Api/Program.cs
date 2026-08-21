using Microsoft.EntityFrameworkCore;
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
builder.Services.AddSingleton(tokenProviderOptions);

builder.Services.AddPackageDeliveryServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCorsPolicies(builder.Configuration);
builder.Services.AddRateLimiting(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddControllersWithViews(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()))
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddEndpointsApiExplorer();
builder.Services.UseHealthChecks<PackageDeliveryDbContext>(builder.Configuration);
builder.Services.AddSwagger();
builder.Services.AddAntiforgeryProtection();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("RunMigrations"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PackageDeliveryDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUIConfig();
app.UseApiLoggingMiddleware();
app.UseHttpsRedirection();
app.UseRouting();
app.UseExceptionHandler();
app.UseCors(Policies.CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseRateLimiter();
app.MapHealthChecks();
app.UseSecurity();
app.MapControllers();

await app.RunAsync();

public partial class Program { }