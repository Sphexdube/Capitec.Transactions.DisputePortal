using Capitec.Transactions.DisputePortal.Api.Controllers.V1;
using Capitec.Transactions.DisputePortal.Application.Handlers;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Confirmations;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Application.Validators.V1;
using Capitec.Transactions.DisputePortal.UnitTests.Setup;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Capitec.Transactions.DisputePortal.UnitTests.Presentation.Controllers.V1
{
    public sealed class DisputeControllerTests
    {
        private readonly Guid _disputeId = Guid.NewGuid();
        private IRequestHandler<GetDisputesRequest, GetDisputesResponse> _getDisputes = null!;
        private IRequestHandler<GetDisputeRequest, GetDisputeResponse> _getDispute = null!;
        private IRequestHandler<GetDisputeHistoryRequest, IReadOnlyList<DisputeHistoryResponse>> _getHistory = null!;
        private IRequestHandler<GetConfirmationRequest, GetConfirmationResponse> _getConfirmation = null!;

        [SetUp]
        public void SetUp()
        {
            _getDisputes = Substitute.For<IRequestHandler<GetDisputesRequest, GetDisputesResponse>>();
            _getDispute = Substitute.For<IRequestHandler<GetDisputeRequest, GetDisputeResponse>>();
            _getHistory = Substitute.For<IRequestHandler<GetDisputeHistoryRequest, IReadOnlyList<DisputeHistoryResponse>>>();
            _getConfirmation = Substitute.For<IRequestHandler<GetConfirmationRequest, GetConfirmationResponse>>();
        }

        [Test]
        public async Task Actions_WhenCustomerIsSignedIn_ShouldPassCustomerAndReturnOk()
        {
            DisputeController disputes = CreateDisputeController(6);
            ConfirmationController confirmations = CreateConfirmationController(6);

            IActionResult list = await disputes.GetDisputes("", 10);
            IActionResult detail = await disputes.GetDispute(_disputeId);
            IActionResult history = await disputes.GetDisputeHistory(_disputeId);
            IActionResult confirmation = await confirmations.GetConfirmation(_disputeId);

            Assert.That(new[] { list, detail, history, confirmation }, Is.All.InstanceOf<OkObjectResult>());
            await _getDisputes.Received(1).HandleAsync(Arg.Is<GetDisputesRequest>(request => request.CustomerId == 6 && request.PageSize == 10), Arg.Any<CancellationToken>());
            await _getDispute.Received(1).HandleAsync(Arg.Is<GetDisputeRequest>(request => request.CustomerId == 6 && request.DisputeId == _disputeId), Arg.Any<CancellationToken>());
            await _getHistory.Received(1).HandleAsync(Arg.Is<GetDisputeHistoryRequest>(request => request.CustomerId == 6 && request.DisputeId == _disputeId), Arg.Any<CancellationToken>());
            await _getConfirmation.Received(1).HandleAsync(Arg.Is<GetConfirmationRequest>(request => request.CustomerId == 6 && request.DisputeId == _disputeId), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Actions_WhenCustomerClaimIsMissing_ShouldReturnUnauthorized()
        {
            DisputeController disputes = CreateDisputeController(null);
            ConfirmationController confirmations = CreateConfirmationController(null);

            IActionResult[] results =
            [
                await disputes.GetDisputes(null),
                await disputes.GetDispute(_disputeId),
                await disputes.GetDisputeHistory(_disputeId),
                await confirmations.GetConfirmation(_disputeId)
            ];

            Assert.That(results, Is.All.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task GetDisputes_WhenCursorIsInvalid_ShouldReturnBadRequest()
        {
            IActionResult result = await CreateDisputeController(6).GetDisputes("bad");

            Assert.That(((ApiErrorResponse)((BadRequestObjectResult)result).Value!).Errors.Single().Message, Does.StartWith("cursor:"));
        }

        private DisputeController CreateDisputeController(long? customerId)
        {
            return new DisputeController(_getDisputes, new GetDisputesRequestValidator(), _getDispute, _getHistory)
            {
                ControllerContext = ControllerContextFactory.Create(customerId)
            };
        }

        private ConfirmationController CreateConfirmationController(long? customerId)
        {
            return new ConfirmationController(_getConfirmation)
            {
                ControllerContext = ControllerContextFactory.Create(customerId)
            };
        }
    }
}
