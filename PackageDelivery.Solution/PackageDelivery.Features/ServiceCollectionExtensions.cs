using Microsoft.Extensions.DependencyInjection;
using PackageDelivery.Features.Deliveries.GetDeliveries.Repositories;
using PackageDelivery.Features.Deliveries.GetDeliveries.Services;

namespace PackageDelivery.Features
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDeliveryServices(this IServiceCollection services)
        {
            services.AddScoped<IDeliveryRepository, DeliveryRepository>();
            services.AddScoped<IDeliveryService, DeliveryService>();
            return services;
        }

        public static IServiceCollection AddPackageDeliveryServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddDeliveryServices();

            return services;
        }
    }
}
