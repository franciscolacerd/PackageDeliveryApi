using FluentValidation;
using Microsoft.Extensions.Logging;
using PackageDelivery.Features.Deliveries.CreateDelivery.Builders;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;
using PackageDelivery.Features.Deliveries.CreateDelivery.Repositories;

namespace PackageDelivery.Features.Deliveries.CreateDelivery.Services
{
    public class CreateDeliveryService : ICreateDeliveryService
    {
        private readonly ICreateDeliveryRepository _repository;
        private readonly IValidator<CreateDeliveryRequest> _validator;
        private readonly ILogger<CreateDeliveryService> _logger;

        public CreateDeliveryService(
            ICreateDeliveryRepository repository,
            IValidator<CreateDeliveryRequest> validator,
            ILogger<CreateDeliveryService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CreateDeliveryResponse> CreateAsync(CreateDeliveryRequest request, long userId, CancellationToken cancellationToken = default)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);

            if (!validation.IsValid)
            {
                _logger.LogWarning("CreateDelivery validation failed for user {UserId}", userId);
                return new CreateDeliveryResponse
                {
                    Success = false,
                    Message = "Delivery was not created due to validation errors.",
                    Errors = validation.Errors.Select(e => e.ErrorMessage).ToList(),
                    ValidationErrors = validation.Errors
                          .GroupBy(e => e.PropertyName)
                          .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                };
            }

            var delivery = new DeliveryBuilder()
                .ForUser(userId)
                .WithDetails(request.Details)
                .WithSender(request.Sender)
                .WithReceiver(request.Receiver)
                .WithAttributes(request.Attributes)
                .Build();

            await _repository.AddAsync(delivery, cancellationToken);

            _logger.LogInformation("Delivery {BarCode} created for user {UserId} with {Volumes} volume(s)",
                delivery.BarCode, userId, delivery.NumberOfVolumes);

            return new CreateDeliveryResponse
            {
                Success = true,
                BarCode = delivery.BarCode,
                Message = "Delivery created."
            };
        }
    }
}