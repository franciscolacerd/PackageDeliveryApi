using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PackageDelivery.Infrastructure.Context;
using PackageDelivery.Infrastructure.Entities;

namespace PackageDelivery.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddIdentityCore<AspNetUser>(options =>
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
            })
            .AddRoles<AspNetRole>()
            .AddEntityFrameworkStores<PackageDeliveryDbContext>()
            .AddDefaultTokenProviders();

            services.AddDbContext<PackageDeliveryDbContext>(opts => opts.UseSqlServer(
                configuration.GetConnectionString("PackageDeliveryConnection"),
                options => options.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), null)),
                ServiceLifetime.Scoped);

            services.AddDataProtection();

            return services;
        }
    }
}
