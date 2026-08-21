using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PackageDelivery.Shared.Models;
using System.Text;

namespace PackageDelivery.Api.Configuration
{
    public static class Authentication
    {
        public static TokenProviderOptions AddAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var signingKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(configuration.GetSection("TokenProviderOptions:SecretKey").Value ?? string.Empty));

            var section = configuration.GetSection("TokenProviderOptions");

            var tokenProviderOptions = new TokenProviderOptions
            {
                Path = section["TokenPath"] ?? "/token",
                Audience = section["Audience"] ?? string.Empty,
                Issuer = section["Issuer"] ?? string.Empty,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512),
                CookieName = section["CookieName"] ?? "access_token",
                Expiration = TimeSpan.FromMinutes(section.GetValue("Expiration", 30)),
                RefreshTokenExpiration = TimeSpan.FromDays(section.GetValue("RefreshTokenExpirationDays", 7)),
                CookieSecure = section.GetValue("CookieSecure", true)
            };

            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = signingKey,
                ValidIssuer = section["Issuer"] ?? string.Empty,
                ValidAudience = section["Audience"] ?? string.Empty,
                ClockSkew = TimeSpan.Zero,
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Audience = section["Audience"] ?? string.Empty;
                options.ClaimsIssuer = section["Issuer"] ?? string.Empty;
                options.TokenValidationParameters = tokenValidationParameters;
                options.SaveToken = true;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        if (ctx.Request.Cookies.TryGetValue(tokenProviderOptions.CookieName, out var token))
                            ctx.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            });

            return tokenProviderOptions;
        }
    }
}