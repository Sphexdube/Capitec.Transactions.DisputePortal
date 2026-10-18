using Asp.Versioning;
using Capitec.Transactions.DisputePortal.Api.Controllers.Base;
using Capitec.Transactions.DisputePortal.Application.Handlers;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Confirmations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capitec.Transactions.DisputePortal.Api.Controllers.V1
{
    /// <summary>Reads the submission confirmation of a dispute.</summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/confirmation")]
    public sealed class ConfirmationController(
        IRequestHandler<GetConfirmationRequest, GetConfirmationResponse> getConfirmationHandler) : BaseController
    {
        /// <summary>Returns the submission confirmation of one dispute of the signed-in customer.</summary>
        /// <param name="disputeId">The external id of the dispute.</param>
        /// <returns>The confirmation, pending until the background worker records it.</returns>
        [HttpGet("{disputeId:guid}")]
        [ProducesResponseType(typeof(ApiSuccessResponse<GetConfirmationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConfirmation(Guid disputeId)
        {
            if (!TryGetCustomerId(out long customerId))
            {
                return CustomerSessionRequired();
            }

            return await ProcessRequest(
                getConfirmationHandler.HandleAsync,
                new GetConfirmationRequest { DisputeId = disputeId, CustomerId = customerId });
        }
    }
}
