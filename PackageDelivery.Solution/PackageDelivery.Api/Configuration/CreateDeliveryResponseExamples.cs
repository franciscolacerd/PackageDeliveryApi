using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using PackageDelivery.Api.Controllers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PackageDelivery.Api.Configuration
{
    public class CreateDeliveryResponseExamples : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.Name != nameof(DeliveriesController.CreateDelivery) || operation.Responses is null)
                return;

            SetJsonExample(operation, "200", new JsonObject
            {
                ["success"] = true,
                ["barCode"] = "481920371650283",
                ["message"] = "Delivery created.",
                ["errors"] = new JsonArray()
            });

            SetJsonExample(operation, "422", new JsonObject
            {
                ["success"] = false,
                ["barCode"] = null,
                ["message"] = "Delivery was not created due to validation errors.",
                ["errors"] = new JsonArray(
                    "'Details.NumberOfVolumes' must be greater than '0'.",
                    "'Sender.Address.ZipCode' must be valid.")
            });
        }

        private static void SetJsonExample(OpenApiOperation operation, string statusCode, JsonObject example)
        {
            if (operation.Responses is not null
                && operation.Responses.TryGetValue(statusCode, out var response)
                && response.Content is not null
                && response.Content.TryGetValue("application/json", out var media))
            {
                media.Example = example;
            }
        }
    }
}
