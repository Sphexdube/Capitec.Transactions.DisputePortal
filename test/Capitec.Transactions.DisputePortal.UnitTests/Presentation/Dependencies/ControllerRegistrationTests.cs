using Capitec.Transactions.DisputePortal.Api.Dependencies;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Capitec.Transactions.DisputePortal.UnitTests.Presentation.Dependencies
{
    public sealed class ControllerRegistrationTests
    {
        [Test]
        public void InvalidModelState_WhenFieldsAreInvalid_ShouldReturnFieldErrorsWithoutJsonPaths()
        {
            ActionContext context = CreateContext("$", "details", "$.reason", "Idempotency-Key", "$.reason");

            ApiErrorResponse response = Invoke(context);

            Assert.That(response.Errors.Select(error => error.Message), Is.EquivalentTo(new[] { "reason: Invalid value.", "Idempotency-Key: Invalid value." }));
            Assert.That(response.Errors.Select(error => error.Code), Is.All.EqualTo("VALIDATION_ERROR"));
        }

        [Test]
        public void InvalidModelState_WhenOnlyTheBodyIsInvalid_ShouldReturnRequestInvalid()
        {
            ApiErrorResponse response = Invoke(CreateContext("$", "details"));

            Assert.That(response.Errors.Single().Message, Is.EqualTo("The request is invalid."));
        }

        private static ApiErrorResponse Invoke(ActionContext context)
        {
            ServiceCollection services = new();
            ControllerRegistration.Register(services);

            using (ServiceProvider provider = services.BuildServiceProvider())
            {
                ApiBehaviorOptions options = provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
                BadRequestObjectResult result = (BadRequestObjectResult)options.InvalidModelStateResponseFactory(context);

                return (ApiErrorResponse)result.Value!;
            }
        }

        private static ActionContext CreateContext(params string[] invalidKeys)
        {
            ActionDescriptor descriptor = new()
            {
                Parameters =
                [
                    new Microsoft.AspNetCore.Mvc.Abstractions.ParameterDescriptor { Name = "details", BindingInfo = new BindingInfo { BindingSource = BindingSource.Body } },
                    new Microsoft.AspNetCore.Mvc.Abstractions.ParameterDescriptor { Name = "transactionId" }
                ]
            };
            ActionContext context = new(new DefaultHttpContext(), new RouteData(), descriptor);

            foreach (string key in invalidKeys)
            {
                context.ModelState.AddModelError(key, "Invalid.");
            }

            return context;
        }
    }
}
