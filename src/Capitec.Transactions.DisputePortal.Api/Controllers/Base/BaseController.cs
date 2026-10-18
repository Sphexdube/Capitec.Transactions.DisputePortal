using System.Security.Claims;
using Capitec.Transactions.DisputePortal.Api.Authentication;
using Capitec.Transactions.DisputePortal.Api.Results;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1;
using Capitec.Transactions.DisputePortal.Domain.Errors;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Capitec.Transactions.DisputePortal.Api.Controllers.Base
{
    [ApiController]
    [Produces("application/json")]
    public abstract class BaseController : ControllerBase
    {
        protected async Task<IActionResult> ProcessRequest<TRequest, TResponse>(
            Func<TRequest, CancellationToken, Task<ValidationResult>> validate,
            Func<TRequest, CancellationToken, Task<TResponse>> handle,
            TRequest request)
        {
            return await ProcessRequest(validate, handle, request, Ok);
        }

        protected async Task<IActionResult> ProcessRequest<TRequest, TResponse>(
            Func<TRequest, CancellationToken, Task<ValidationResult>> validate,
            Func<TRequest, CancellationToken, Task<TResponse>> handle,
            TRequest request,
            Func<ApiSuccessResponse<TResponse>, IActionResult> createResult)
        {
            ArgumentNullException.ThrowIfNull(validate);

            ValidationResult validationResult = await validate(request, HttpContext.RequestAborted);

            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponseFactory.CreateErrors(
                    HttpContext,
                    ApiResponseFactory.CreateValidationErrors(validationResult.Errors)));
            }

            return await ProcessRequest(handle, request, createResult);
        }

        protected async Task<IActionResult> ProcessRequest<TRequest, TResponse>(
            Func<TRequest, CancellationToken, Task<TResponse>> handle,
            TRequest request)
        {
            return await ProcessRequest(handle, request, Ok);
        }

        protected async Task<IActionResult> ProcessRequest<TRequest, TResponse>(
            Func<TRequest, CancellationToken, Task<TResponse>> handle,
            TRequest request,
            Func<ApiSuccessResponse<TResponse>, IActionResult> createResult)
        {
            ArgumentNullException.ThrowIfNull(handle);
            ArgumentNullException.ThrowIfNull(createResult);

            TResponse response = await handle(request, HttpContext.RequestAborted);

            return createResult(ApiResponseFactory.CreateSuccess(HttpContext, response));
        }

        protected bool TryGetCustomerId(out long customerId)
        {
            return long.TryParse(User.FindFirstValue(CustomerClaimTypes.CustomerId), out customerId) && customerId > 0;
        }

        protected IActionResult CustomerSessionRequired()
        {
            return Unauthorized(ApiResponseFactory.CreateError(
                HttpContext,
                ApiErrorCodes.Unauthenticated,
                ErrorMessages.CustomerSessionRequired));
        }
    }
}
