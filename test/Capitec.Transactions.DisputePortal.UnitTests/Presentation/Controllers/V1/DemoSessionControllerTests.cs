using Capitec.Transactions.DisputePortal.Api.Authentication;
using Capitec.Transactions.DisputePortal.Api.Controllers.V1;
using Capitec.Transactions.DisputePortal.Application.Handlers;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.DemoSessions;
using Capitec.Transactions.DisputePortal.UnitTests.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Capitec.Transactions.DisputePortal.UnitTests.Presentation.Controllers.V1
{
    public sealed class DemoSessionControllerTests
    {
        private IRequestHandler<GetDemoCustomersRequest, IReadOnlyList<DemoCustomerOptionResponse>> _getCustomers = null!;
        private IRequestHandler<CreateDemoSessionRequest, DemoSessionResponse> _createSession = null!;

        [SetUp]
        public void SetUp()
        {
            _getCustomers = Substitute.For<IRequestHandler<GetDemoCustomersRequest, IReadOnlyList<DemoCustomerOptionResponse>>>();
            _createSession = Substitute.For<IRequestHandler<CreateDemoSessionRequest, DemoSessionResponse>>();
        }

        [TestCase("Development")]
        [TestCase("Local")]
        public async Task Actions_WhenEnabledInLocalEnvironment_ShouldReturnOk(string environmentName)
        {
            DemoSessionController controller = CreateController(true, environmentName);

            IActionResult customers = await controller.GetCustomers();
            IActionResult session = await controller.CreateSession(new CreateDemoSessionRequest { CustomerId = 1 });

            Assert.That(customers, Is.InstanceOf<OkObjectResult>());
            Assert.That(session, Is.InstanceOf<OkObjectResult>());
        }

        [TestCase(false, "Development")]
        [TestCase(true, "Production")]
        public async Task Actions_WhenDisabledOrNotLocal_ShouldReturnNotFound(bool enabled, string environmentName)
        {
            DemoSessionController controller = CreateController(enabled, environmentName);

            IActionResult customers = await controller.GetCustomers();
            IActionResult session = await controller.CreateSession(new CreateDemoSessionRequest { CustomerId = 1 });

            Assert.That(customers, Is.InstanceOf<NotFoundObjectResult>());
            Assert.That(session, Is.InstanceOf<NotFoundObjectResult>());
            await _createSession.DidNotReceive().HandleAsync(Arg.Any<CreateDemoSessionRequest>(), Arg.Any<CancellationToken>());
        }

        private DemoSessionController CreateController(bool enabled, string environmentName)
        {
            IWebHostEnvironment environment = Substitute.For<IWebHostEnvironment>();
            environment.EnvironmentName.Returns(environmentName);

            return new DemoSessionController(new DemoAuthenticationConfiguration { Enabled = enabled }, environment, _getCustomers, _createSession)
            {
                ControllerContext = ControllerContextFactory.Create(null)
            };
        }
    }
}
