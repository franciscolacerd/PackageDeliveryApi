using Microsoft.OpenApi;
using PackageDelivery.Api.Controllers;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace PackageDelivery.Api.Configuration
{
    public class CreateDeliveryResponseExamples : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.Name != nameof(DeliveriesController.CreateDeliveryAsync) || operation.Responses is null)
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
                ["type"] = "https://tools.ietf.org/html/rfc4918#section-11.2",
                ["title"] = "One or more validation errors occurred.",
                ["status"] = 422,
                ["errors"] = new JsonObject
                {
                    ["Details.NumberOfVolumes"] = new JsonArray("'Details.NumberOfVolumes' must be greater than '0'."),
                    ["Sender.Address.ZipCode"] = new JsonArray("'Sender.Address.ZipCode' must be valid.")
                }
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