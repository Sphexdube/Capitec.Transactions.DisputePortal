using Capitec.Transactions.DisputePortal.Domain.Customers;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Repositories
{
    public sealed class CustomerRepository(DbSet<CustomerRecord> customers) : ICustomerRepository
    {
        public async Task<IReadOnlyList<CustomerSummary>> GetAllAsync(CancellationToken cancellationToken)
        {
            List<CustomerSummary> summaries = await customers
                .AsNoTracking()
                .OrderBy(customer => customer.Id)
                .Select(customer => new CustomerSummary(customer.Id, customer.DisplayName))
                .ToListAsync(cancellationToken);

            return summaries;
        }

        public Task<CustomerSummary?> GetByIdAsync(long customerId, CancellationToken cancellationToken)
        {
            return customers
                .AsNoTracking()
                .Where(customer => customer.Id == customerId)
                .Select(customer => new CustomerSummary(customer.Id, customer.DisplayName))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
