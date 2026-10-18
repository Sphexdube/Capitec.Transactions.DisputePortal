using System.Reflection;
using Capitec.Transactions.DisputePortal.Api.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using NSubstitute;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Capitec.Transactions.DisputePortal.UnitTests.Presentation.Swagger
{
    public sealed class ApiOperationFilterTests
    {
        private readonly ApiOperationFilter _filter = new();

        [Test]
        public void Apply_WhenEndpointRequiresAuthentication_ShouldAddBearerAndCorrelationHeaders()
        {
            OpenApiOperation operation = new()
            {
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse { Description = "OK" },
                    ["404"] = new OpenApiResponseReference("NotFound")
                }
            };

            _filter.Apply(operation, CreateContext());

            Assert.That(operation.Security, Has.Count.EqualTo(1));
            Assert.That(operation.Parameters!.Single().Name, Is.EqualTo("CorrelationId"));
            Assert.That(((OpenApiResponse)operation.Responses["200"]).Headers!.ContainsKey("CorrelationId"), Is.True);
        }

        [Test]
        public void Apply_WhenEndpointAllowsAnonymous_ShouldNotAddSecurity()
        {
            OpenApiOperation operation = new() { Parameters = [], Responses = null };

            _filter.Apply(operation, CreateContext(new AllowAnonymousAttribute()));

            Assert.That(operation.Security, Is.Null.Or.Empty);
            Assert.That(operation.Parameters, Has.Count.EqualTo(1));
        }

        [Test]
        public void Apply_WhenArgumentsAreNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _filter.Apply(null!, CreateContext()));
            Assert.Throws<ArgumentNullException>(() => _filter.Apply(new OpenApiOperation(), null!));
        }

        private static OperationFilterContext CreateContext(params object[] metadata)
        {
            ApiDescription description = new() { ActionDescriptor = new ActionDescriptor { EndpointMetadata = metadata.ToList() } };
            MethodInfo method = typeof(ApiOperationFilterTests).GetMethod(nameof(CreateContext), BindingFlags.NonPublic | BindingFlags.Static)!;

            return new OperationFilterContext(description, Substitute.For<ISchemaGenerator>(), new SchemaRepository(), new OpenApiDocument(), method);
        }
    }
}
