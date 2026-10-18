using Asp.Versioning;
using Capitec.Transactions.DisputePortal.Api.Authentication;
using Capitec.Transactions.DisputePortal.Api.Controllers.Base;
using Capitec.Transactions.DisputePortal.Api.Results;
using Capitec.Transactions.DisputePortal.Application.Handlers;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.DemoSessions;
using Capitec.Transactions.DisputePortal.Domain.Errors;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capitec.Transactions.DisputePortal.Api.Controllers.V1
{
    /// <summary>Starts demo customer sessions in Local and Development only.</summary>
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/demo-session")]
    public sealed class DemoSessionController(
        DemoAuthenticationConfiguration configuration,
        IWebHostEnvironment environment,
        IRequestHandler<GetDemoCustomersRequest, IReadOnlyList<DemoCustomerOptionResponse>> getDemoCustomersHandler,
        IRequestHandler<CreateDemoSessionRequest, DemoSessionResponse> createDemoSessionHandler) : BaseController
    {
        /// <summary>Returns the demo customers a session can start for.</summary>
        /// <returns>The demo customers.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResponse<IReadOnlyList<DemoCustomerOptionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomers()
        {
            if (!IsEnabled())
            {
                return DemoAuthenticationDisabled();
            }

            return await ProcessRequest(getDemoCustomersHandler.HandleAsync, new GetDemoCustomersRequest());
        }

        /// <summary>Starts a demo session and returns a signed bearer token for the customer.</summary>
        /// <param name="request">The demo customer to sign in as.</param>
        /// <returns>The demo session.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiSuccessResponse<DemoSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateSession([FromBody] CreateDemoSessionRequest request)
        {
            if (!IsEnabled())
            {
                return DemoAuthenticationDisabled();
            }

            return await ProcessRequest(createDemoSessionHandler.HandleAsync, request);
        }

        private bool IsEnabled()
        {
            return configuration.IsAvailableIn(environment);
        }

        private NotFoundObjectResult DemoAuthenticationDisabled()
        {
            return NotFound(ApiResponseFactory.CreateError(HttpContext, ApiErrorCodes.NotFound, ErrorMessages.ResourceNotFoundDetail));
        }
    }
}
