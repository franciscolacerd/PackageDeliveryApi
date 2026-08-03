using Microsoft.Extensions.Options;
using Serilog;
using System.Text.Json;
using PackageDelivery.Api.Configuration;
using PackageDelivery.Api.Middleware;
using PackageDelivery.Features;
using PackageDelivery.Infrastructure;
using PackageDelivery.Infrastructure.Context;
using PackageDelivery.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfiguration();

var tokenProviderOptions = Authentication.AddAuthentication(builder.Services, builder.Configuration);

builder.Services.AddPackageDeliveryServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCorsPolicies(builder.Configuration);
builder.Services.AddRateLimiting(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddEndpointsApiExplorer();
builder.Services.UseHealthChecks<PackageDeliveryDbContext>();
builder.Services.AddSwagger();
builder.Services.AddSecurity();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUIConfig();

app.UseApiLoggingMiddleware();
app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseCors(Policies.CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TokenProviderMiddleware>(Options.Create(tokenProviderOptions));

app.UseExceptionMiddleware();
app.MapHealthChecks();
app.UseSecurity();

app.MapControllers();

await app.RunAsync();
