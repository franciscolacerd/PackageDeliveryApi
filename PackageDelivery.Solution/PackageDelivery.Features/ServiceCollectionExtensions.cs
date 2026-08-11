using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;
using PackageDelivery.Features.Deliveries.CreateDelivery.Repositories;
using PackageDelivery.Features.Deliveries.CreateDelivery.Services;
using PackageDelivery.Features.Deliveries.CreateDelivery.Validators;
using PackageDelivery.Features.Deliveries.GetDeliveries.Repositories;
using PackageDelivery.Features.Deliveries.GetDeliveries.Services;
using PackageDelivery.Shared.Settings;

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

        public static IServiceCollection AddPackageDeliveryServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PagingSettings>(configuration.GetSection("PagingSettings"));

            services.AddHttpContextAccessor();

            services.AddGetDeliveriesFeature();
            services.AddCreateDeliveryFeature();

            return services;
        }
    }
}