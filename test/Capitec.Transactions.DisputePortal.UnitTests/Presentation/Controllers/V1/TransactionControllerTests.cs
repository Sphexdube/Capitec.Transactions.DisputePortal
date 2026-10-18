using Capitec.Transactions.DisputePortal.Api.Controllers.V1;
using Capitec.Transactions.DisputePortal.Application.Handlers;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Transactions;
using Capitec.Transactions.DisputePortal.Application.Validators.V1;
using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.UnitTests.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Capitec.Transactions.DisputePortal.UnitTests.Presentation.Controllers.V1
{
    public sealed class TransactionControllerTests
    {
        private IRequestHandler<GetTransactionsRequest, GetTransactionsResponse> _getTransactions = null!;
        private IRequestHandler<SubmitDisputeRequest, SubmitDisputeResponse> _submitDispute = null!;
        private IRequestHandler<GetTransactionRequest, TransactionSummaryResponse> _getTransaction = null!;

        [SetUp]
        public void SetUp()
        {
            _getTransactions = Substitute.For<IRequestHandler<GetTransactionsRequest, GetTransactionsResponse>>();
            _submitDispute = Substitute.For<IRequestHandler<SubmitDisputeRequest, SubmitDisputeResponse>>();
            _getTransaction = Substitute.For<IRequestHandler<GetTransactionRequest, TransactionSummaryResponse>>();
        }

        [Test]
        public async Task GetTransactions_WhenRequestIsValid_ShouldReturnOkEnvelope()
        {
            GetTransactionsResponse page = new() { Items = [], PageSize = 20 };
            _getTransactions.HandleAsync(Arg.Is<GetTransactionsRequest>(request => request.CustomerId == 5), Arg.Any<CancellationToken>()).Returns(page);

            IActionResult result = await CreateController(5).GetTransactions(null);

            ApiSuccessResponse<GetTransactionsResponse> envelope = (ApiSuccessResponse<GetTransactionsResponse>)((OkObjectResult)result).Value!;
            Assert.That(envelope.Data, Is.SameAs(page));
            Assert.That(envelope.CorrelationId, Is.EqualTo(ControllerContextFactory.TraceIdentifier));
        }

        [Test]
        public async Task GetTransactions_WhenPageSizeIsInvalid_ShouldReturnBadRequestEnvelope()
        {
            IActionResult result = await CreateController(5).GetTransactions(null, 51);

            BadRequestObjectResult badRequest = (BadRequestObjectResult)result;
            ApiErrorResponse errors = (ApiErrorResponse)badRequest.Value!;
            Assert.That(errors.Errors.Single().Message, Does.StartWith("pageSize:"));
            await _getTransactions.DidNotReceive().HandleAsync(Arg.Any<GetTransactionsRequest>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetTransaction_WhenCustomerIsSignedIn_ShouldPassOwnershipToHandler()
        {
            Guid transactionId = Guid.NewGuid();

            IActionResult result = await CreateController(5).GetTransaction(transactionId);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            await _getTransaction.Received(1).HandleAsync(
                Arg.Is<GetTransactionRequest>(request => request.TransactionId == transactionId && request.CustomerId == 5),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task SubmitDispute_WhenRequestIsValid_ShouldReturnAcceptedWithLocation()
        {
            Guid disputeId = Guid.NewGuid();
            _submitDispute.HandleAsync(Arg.Any<SubmitDisputeRequest>(), Arg.Any<CancellationToken>())
                .Returns(new SubmitDisputeResponse { DisputeId = disputeId, Status = DisputeStatus.Submitted, CreatedAt = DateTimeOffset.UnixEpoch });

            IActionResult result = await CreateController(5).SubmitDispute(Guid.NewGuid(), "key-1", new SubmitDisputeDetails { Reason = "Fraud" });

            AcceptedResult accepted = (AcceptedResult)result;
            Assert.That(accepted.Location, Is.EqualTo($"/api/v1/dispute/{disputeId}"));
            Assert.That(accepted.StatusCode, Is.EqualTo(StatusCodes.Status202Accepted));
        }

        [Test]
        public async Task Actions_WhenCustomerClaimIsMissing_ShouldReturnUnauthorizedEnvelope()
        {
            TransactionController controller = CreateController(null);

            IActionResult list = await controller.GetTransactions(null);
            IActionResult single = await controller.GetTransaction(Guid.NewGuid());
            IActionResult submit = await controller.SubmitDispute(Guid.NewGuid(), "key", new SubmitDisputeDetails { Reason = "Fraud" });

            Assert.That(list, Is.InstanceOf<UnauthorizedObjectResult>());
            Assert.That(submit, Is.InstanceOf<UnauthorizedObjectResult>());
            Assert.That(single, Is.InstanceOf<UnauthorizedObjectResult>());
            Assert.That(((ApiErrorResponse)((UnauthorizedObjectResult)list).Value!).Errors.Single().Code, Is.EqualTo("UNAUTHENTICATED"));
        }

        private TransactionController CreateController(long? customerId)
        {
            return new TransactionController(_getTransactions, new GetTransactionsRequestValidator(), _getTransaction, _submitDispute, new SubmitDisputeRequestValidator())
            {
                ControllerContext = ControllerContextFactory.Create(customerId)
            };
        }
    }
}
