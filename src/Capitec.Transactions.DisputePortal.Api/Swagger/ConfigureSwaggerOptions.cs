using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Capitec.Transactions.DisputePortal.Api.Swagger
{
    internal sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
    {
        private const string Title = "Capitec Transaction Dispute Portal API";
        private const string Description = "Lets a signed-in customer list transactions, submit disputes and follow their status.";

        public void Configure(SwaggerGenOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = Title,
                    Version = description.ApiVersion.ToString(),
                    Description = Description
                });
            }
        }
    }
}
