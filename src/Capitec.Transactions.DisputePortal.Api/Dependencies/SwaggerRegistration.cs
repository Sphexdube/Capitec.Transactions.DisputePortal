using System.Reflection;
using Capitec.Transactions.DisputePortal.Api.Swagger;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Capitec.Transactions.DisputePortal.Api.Dependencies
{
    public static class SwaggerRegistration
    {
        public const string BearerSchemeName = "Bearer";
        private const string BearerDescription = "Paste the access token from POST /api/v1/demo-session.";
        private const string ApplicationXmlFile = "Capitec.Transactions.DisputePortal.Application.xml";

        public static void Register(SwaggerGenOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            string[] xmlFiles = [$"{Assembly.GetExecutingAssembly().GetName().Name}.xml", ApplicationXmlFile];

            foreach (string xmlFile in xmlFiles)
            {
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }
            }

            options.AddSecurityDefinition(BearerSchemeName, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = BearerDescription
            });
            options.OperationFilter<ApiOperationFilter>();
        }
    }
}
