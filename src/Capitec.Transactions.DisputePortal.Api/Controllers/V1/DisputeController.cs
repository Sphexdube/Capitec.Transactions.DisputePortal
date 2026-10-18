using Asp.Versioning;
using Capitec.Transactions.DisputePortal.Api.Controllers.Base;
using Capitec.Transactions.DisputePortal.Application.Handlers;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Application.Paging;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capitec.Transactions.DisputePortal.Api.Controllers.V1
{
    /// <summary>Reads the disputes of the signed-in customer.</summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/dispute")]
    public sealed class DisputeController(
        IRequestHandler<GetDisputesRequest, GetDisputesResponse> getDisputesHandler,
        IValidator<GetDisputesRequest> getDisputesValidator,
        IRequestHandler<GetDisputeRequest, GetDisputeResponse> getDisputeHandler,
        IRequestHandler<GetDisputeHistoryRequest, IReadOnlyList<DisputeHistoryResponse>> getDisputeHistoryHandler) : BaseController
    {
        /// <summary>Returns one page of the signed-in customer's disputes, newest first.</summary>
        /// <param name="cursor">The cursor from the previous page, or empty for the first page.</param>
        /// <param name="pageSize">The number of disputes in the page, from 1 to 50.</param>
        /// <returns>The dispute page.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResponse<GetDisputesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDisputes(
            [FromQuery] string? cursor,
            [FromQuery] int pageSize = PageLimits.DefaultPageSize)
        {
            if (!TryGetCustomerId(out long customerId))
            {
                return CustomerSessionRequired();
            }

            GetDisputesRequest request = new()
            {
                CustomerId = customerId,
                Cursor = cursor,
                PageSize = pageSize
            };

            return await ProcessRequest(getDisputesValidator.ValidateAsync, getDisputesHandler.HandleAsync, request);
        }

        /// <summary>Returns one dispute of the signed-in customer with its transaction.</summary>
        /// <param name="disputeId">The external id of the dispute.</param>
        /// <returns>The dispute.</returns>
        [HttpGet("{disputeId:guid}")]
        [ProducesResponseType(typeof(ApiSuccessResponse<GetDisputeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDispute(Guid disputeId)
        {
            if (!TryGetCustomerId(out long customerId))
            {
                return CustomerSessionRequired();
            }

            return await ProcessRequest(
                getDisputeHandler.HandleAsync,
                new GetDisputeRequest { DisputeId = disputeId, CustomerId = customerId });
        }

        /// <summary>Returns the status history of one dispute of the signed-in customer, oldest first.</summary>
        /// <param name="disputeId">The external id of the dispute.</param>
        /// <returns>The status history.</returns>
        [HttpGet("{disputeId:guid}/history")]
        [ProducesResponseType(typeof(ApiSuccessResponse<IReadOnlyList<DisputeHistoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDisputeHistory(Guid disputeId)
        {
            if (!TryGetCustomerId(out long customerId))
            {
                return CustomerSessionRequired();
            }

            return await ProcessRequest(
                getDisputeHistoryHandler.HandleAsync,
                new GetDisputeHistoryRequest { DisputeId = disputeId, CustomerId = customerId });
        }
    }
}
