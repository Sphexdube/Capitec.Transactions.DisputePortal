using System.Globalization;
using Asp.Versioning;
using Capitec.Transactions.DisputePortal.Api.Controllers.Base;
using Capitec.Transactions.DisputePortal.Application.Handlers;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Transactions;
using Capitec.Transactions.DisputePortal.Application.Paging;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capitec.Transactions.DisputePortal.Api.Controllers.V1
{
    /// <summary>Lists customer transactions and accepts disputes against them.</summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/transaction")]
    public sealed class TransactionController(
        IRequestHandler<GetTransactionsRequest, GetTransactionsResponse> getTransactionsHandler,
        IValidator<GetTransactionsRequest> getTransactionsValidator,
        IRequestHandler<GetTransactionRequest, TransactionSummaryResponse> getTransactionHandler,
        IRequestHandler<SubmitDisputeRequest, SubmitDisputeResponse> submitDisputeHandler,
        IValidator<SubmitDisputeRequest> submitDisputeValidator) : BaseController
    {
        private const string DisputeLocationFormat = "/api/v1/dispute/{0}";

        /// <summary>Returns one page of the signed-in customer's transactions, newest first.</summary>
        /// <param name="cursor">The cursor from the previous page, or empty for the first page.</param>
        /// <param name="pageSize">The number of transactions in the page, from 1 to 50.</param>
        /// <param name="from">The optional earliest transaction time to include.</param>
        /// <param name="to">The optional latest transaction time to include.</param>
        /// <returns>The transaction page.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResponse<GetTransactionsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] string? cursor,
            [FromQuery] int pageSize = PageLimits.DefaultPageSize,
            [FromQuery] DateTimeOffset? from = null,
            [FromQuery] DateTimeOffset? to = null)
        {
            if (!TryGetCustomerId(out long customerId))
            {
                return CustomerSessionRequired();
            }

            GetTransactionsRequest request = new()
            {
                CustomerId = customerId,
                Cursor = cursor,
                PageSize = pageSize,
                From = from,
                To = to
            };

            return await ProcessRequest(getTransactionsValidator.ValidateAsync, getTransactionsHandler.HandleAsync, request);
        }

        /// <summary>Returns one of the signed-in customer's transactions.</summary>
        /// <param name="transactionId">The external id of the transaction.</param>
        /// <returns>The transaction, or not found when another customer owns it.</returns>
        [HttpGet("{transactionId:guid}")]
        [ProducesResponseType(typeof(ApiSuccessResponse<TransactionSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransaction(Guid transactionId)
        {
            if (!TryGetCustomerId(out long customerId))
            {
                return CustomerSessionRequired();
            }

            return await ProcessRequest(
                getTransactionHandler.HandleAsync,
                new GetTransactionRequest { TransactionId = transactionId, CustomerId = customerId });
        }

        /// <summary>Submits a dispute against one of the signed-in customer's transactions.</summary>
        /// <param name="transactionId">The external id of the disputed transaction.</param>
        /// <param name="idempotencyKey">The client key that makes a retried submission return the original dispute.</param>
        /// <param name="details">The dispute reason and optional explanation.</param>
        /// <returns>The accepted dispute.</returns>
        [HttpPost("{transactionId:guid}/dispute")]
        [ProducesResponseType(typeof(ApiSuccessResponse<SubmitDisputeResponse>), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> SubmitDispute(
            Guid transactionId,
            [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
            [FromBody] SubmitDisputeDetails details)
        {
            if (!TryGetCustomerId(out long customerId))
            {
                return CustomerSessionRequired();
            }

            SubmitDisputeRequest request = new()
            {
                TransactionId = transactionId,
                CustomerId = customerId,
                IdempotencyKey = idempotencyKey,
                Reason = details.Reason,
                Explanation = details.Explanation
            };

            return await ProcessRequest(
                submitDisputeValidator.ValidateAsync,
                submitDisputeHandler.HandleAsync,
                request,
                envelope => Accepted(
                    string.Format(CultureInfo.InvariantCulture, DisputeLocationFormat, envelope.Data.DisputeId),
                    envelope));
        }
    }
}
