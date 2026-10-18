using Asp.Versioning.ApiExplorer;

namespace Capitec.Transactions.DisputePortal.Api.Dependencies
{
    public static class SwaggerUiRegistration
    {
        public const string Route = "/swagger";

        public static void UseVersionedSwaggerUi(WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (ApiVersionDescription description in app.DescribeApiVersions())
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });
        }
    }
}
