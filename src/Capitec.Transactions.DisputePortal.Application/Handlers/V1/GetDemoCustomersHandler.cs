using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.DemoSessions;
using Capitec.Transactions.DisputePortal.Domain.Customers;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Microsoft.Extensions.Logging;

namespace Capitec.Transactions.DisputePortal.Application.Handlers.V1
{
    public sealed class GetDemoCustomersHandler(
        ILogger<GetDemoCustomersHandler> logger,
        IUnitOfWorkFactory<IUnitOfWork> unitOfWorkFactory)
        : IRequestHandler<GetDemoCustomersRequest, IReadOnlyList<DemoCustomerOptionResponse>>
    {
        public async Task<IReadOnlyList<DemoCustomerOptionResponse>> HandleAsync(
            GetDemoCustomersRequest request,
            CancellationToken cancellationToken = default)
        {
            logger.LogDebug(InfoMessages.MethodStarted);

            try
            {
                using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
                {
                    IReadOnlyList<CustomerSummary> customers = await unitOfWork.Customers.GetAllAsync(cancellationToken);

                    logger.LogDebug(InfoMessages.MethodCompleted);
                    return customers
                        .Select(customer => new DemoCustomerOptionResponse
                        {
                            CustomerId = customer.Id,
                            DisplayName = customer.DisplayName
                        })
                        .ToArray();
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ErrorMessages.MethodFailed);
                throw;
            }
        }
    }
}
