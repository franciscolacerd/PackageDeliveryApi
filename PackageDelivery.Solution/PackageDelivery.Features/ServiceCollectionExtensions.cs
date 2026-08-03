using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;
using PackageDelivery.Features.Deliveries.CreateDelivery.Repositories;
using PackageDelivery.Features.Deliveries.CreateDelivery.Services;
using PackageDelivery.Features.Deliveries.CreateDelivery.Validators;
using PackageDelivery.Features.Deliveries.GetDeliveries.Repositories;
using PackageDelivery.Features.Deliveries.GetDeliveries.Services;

namespace PackageDelivery.Features
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGetDeliveriesFeature(this IServiceCollection services)
        {
            services.AddScoped<IGetDeliveriesRepository, GetDeliveriesRepository>();
            services.AddScoped<IGetDeliveriesService, GetDeliveriesService>();
            return services;
        }

        public static IServiceCollection AddCreateDeliveryFeature(this IServiceCollection services)
        {
            services.AddScoped<ICreateDeliveryRepository, CreateDeliveryRepository>();
            services.AddScoped<ICreateDeliveryService, CreateDeliveryService>();
            services.AddScoped<IValidator<CreateDeliveryRequest>, CreateDeliveryValidator>();
            return services;
        }

        public static IServiceCollection AddPackageDeliveryServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddGetDeliveriesFeature();
            services.AddCreateDeliveryFeature();

            return services;
        }
    }
}
