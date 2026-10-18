using Capitec.Transactions.DisputePortal.Application.Handlers.Exceptions;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.DemoSessions;
using Capitec.Transactions.DisputePortal.Domain.Authentication;
using Capitec.Transactions.DisputePortal.Domain.Customers;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Microsoft.Extensions.Logging;

namespace Capitec.Transactions.DisputePortal.Application.Handlers.V1
{
    public sealed class CreateDemoSessionHandler(
        ILogger<CreateDemoSessionHandler> logger,
        IUnitOfWorkFactory<IUnitOfWork> unitOfWorkFactory,
        IDemoTokenIssuer tokenIssuer) : IRequestHandler<CreateDemoSessionRequest, DemoSessionResponse>
    {
        public async Task<DemoSessionResponse> HandleAsync(
            CreateDemoSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            logger.LogDebug(InfoMessages.MethodStarted);

            try
            {
                using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
                {
                    CustomerSummary? customer = await unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);

                    if (customer is null)
                    {
                        throw new DemoCustomerNotFoundException(ErrorMessages.DemoCustomerNotFound);
                    }

                    DemoToken token = tokenIssuer.Issue(customer.Id);

                    logger.LogDebug(InfoMessages.MethodCompleted);
                    return new DemoSessionResponse
                    {
                        AccessToken = token.AccessToken,
                        DisplayName = customer.DisplayName,
                        ExpiresAt = token.ExpiresAt
                    };
                }
            }
            catch (Exception exception) when (exception is not RequestRejectedException)
            {
                logger.LogError(exception, ErrorMessages.MethodFailed);
                throw;
            }
        }
    }
}
