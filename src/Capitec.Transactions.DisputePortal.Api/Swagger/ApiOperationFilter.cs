using Capitec.Transactions.DisputePortal.Api.Dependencies;
using Capitec.Transactions.DisputePortal.Api.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Capitec.Transactions.DisputePortal.Api.Swagger
{
    internal sealed class ApiOperationFilter : IOperationFilter
    {
        private const string CorrelationIdDescription = "The correlation id of the request. Send it to trace a request; the API generates one when it is missing.";

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentNullException.ThrowIfNull(context);

            AddCorrelationHeader(operation);

            bool allowsAnonymous = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();

            if (!allowsAnonymous)
            {
                operation.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(SwaggerRegistration.BearerSchemeName, context.Document)] = []
                    }
                ];
            }
        }

        private static void AddCorrelationHeader(OpenApiOperation operation)
        {
            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = CorrelationIdMiddleware.HeaderName,
                In = ParameterLocation.Header,
                Required = false,
                Description = CorrelationIdDescription,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String, MaxLength = CorrelationIdMiddleware.MaximumLength }
            });

            foreach (IOpenApiResponse response in operation.Responses?.Values ?? Enumerable.Empty<IOpenApiResponse>())
            {
                if (response is OpenApiResponse concreteResponse)
                {
                    concreteResponse.Headers ??= new Dictionary<string, IOpenApiHeader>();
                    concreteResponse.Headers[CorrelationIdMiddleware.HeaderName] = new OpenApiHeader
                    {
                        Description = CorrelationIdDescription,
                        Schema = new OpenApiSchema { Type = JsonSchemaType.String }
                    };
                }
            }
        }
    }
}
