using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PackageDelivery.Shared.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj, JsonSerializerOptions? options = null)
        {
            if (obj == null)
                return "null";

            var defaultOptions = options ?? new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            return JsonSerializer.Serialize(obj, defaultOptions);
        }
    }
}
