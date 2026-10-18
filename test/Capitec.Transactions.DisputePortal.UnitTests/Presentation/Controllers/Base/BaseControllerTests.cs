using Capitec.Transactions.DisputePortal.Api.Controllers.Base;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1;
using Capitec.Transactions.DisputePortal.UnitTests.Setup;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Capitec.Transactions.DisputePortal.UnitTests.Presentation.Controllers.Base
{
    public sealed class BaseControllerTests
    {
        [Test]
        public void ProcessRequest_WhenDelegatesAreNull_ShouldThrowArgumentNullException()
        {
            ProbeController controller = new() { ControllerContext = ControllerContextFactory.Create(1) };

            Assert.ThrowsAsync<ArgumentNullException>(() => controller.ValidateWith(null!));
            Assert.ThrowsAsync<ArgumentNullException>(() => controller.HandleWith(null!, Ok));
            Assert.ThrowsAsync<ArgumentNullException>(() => controller.HandleWith((request, token) => Task.FromResult(request), null!));
        }

        private static IActionResult Ok(ApiSuccessResponse<string> envelope)
        {
            return new OkObjectResult(envelope);
        }

        private sealed class ProbeController : BaseController
        {
            public Task<IActionResult> ValidateWith(Func<string, CancellationToken, Task<ValidationResult>> validate)
            {
                return ProcessRequest(validate, (request, token) => Task.FromResult(request), "value");
            }

            public Task<IActionResult> HandleWith(
                Func<string, CancellationToken, Task<string>> handle,
                Func<ApiSuccessResponse<string>, IActionResult> createResult)
            {
                return ProcessRequest(handle, "value", createResult);
            }
        }
    }
}
