using Microsoft.OpenApi;

namespace PackageDelivery.Api.Configuration
{
    public static class Swagger
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            var openApiInfo = new OpenApiInfo
            {
                Version = "v1",
                Title = "PackageDelivery API",
                Description = "API for managing package deliveries."
            };

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(openApiInfo.Version, openApiInfo);

                foreach (var xmlDoc in Directory.GetFiles(AppContext.BaseDirectory, "PackageDelivery.*.xml"))
                    c.IncludeXmlComments(xmlDoc, includeControllerXmlComments: true);

                c.SupportNonNullableReferenceTypes();

                c.OperationFilter<CreateDeliveryResponseExamples>();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme.\r\n" +
                                  "Enter your token in the text input below.\r\n" +
                                  "Example: '12345abcdef'"
                });

                c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer"),
                        new List<string>()
                    }
                });
            });
        }

        public static void UseSwaggerUIConfig(this WebApplication app)
        {
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PackageDelivery Api v1");
                c.DefaultModelsExpandDepth(-1);
            });
        }
    }
}
