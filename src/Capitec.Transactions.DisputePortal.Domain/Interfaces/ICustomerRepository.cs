using Capitec.Transactions.DisputePortal.Domain.Customers;

namespace Capitec.Transactions.DisputePortal.Domain.Interfaces
{
    public interface ICustomerRepository
    {
        public Task<IReadOnlyList<CustomerSummary>> GetAllAsync(CancellationToken cancellationToken);

        public Task<CustomerSummary?> GetByIdAsync(long customerId, CancellationToken cancellationToken);
    }
}
