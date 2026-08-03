using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PackageDelivery.Shared.Models;

namespace PackageDelivery.Api.Configuration
{
    public static class Authentication
    {
        public static TokenProviderOptions AddAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var signingKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(configuration.GetSection("TokenProviderOptions:SecretKey").Value ?? string.Empty));

            var tokenProviderOptions = new TokenProviderOptions
            {
                Path = configuration.GetSection("TokenProviderOptions:TokenPath").Value ?? "/token",
                Audience = configuration.GetSection("TokenProviderOptions:Audience").Value ?? string.Empty,
                Issuer = configuration.GetSection("TokenProviderOptions:Issuer").Value ?? string.Empty,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
            };

            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = signingKey,
                ValidIssuer = configuration.GetSection("TokenProviderOptions:Issuer").Value,
                ValidAudience = configuration.GetSection("TokenProviderOptions:Audience").Value,
                ClockSkew = TimeSpan.Zero,
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Audience = configuration.GetSection("TokenProviderOptions:Audience").Value;
                options.ClaimsIssuer = configuration.GetSection("TokenProviderOptions:Issuer").Value;
                options.TokenValidationParameters = tokenValidationParameters;
                options.SaveToken = true;
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
