using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Repositories
{
    public sealed class ConfirmationRepository(DbSet<ConfirmationRecord> confirmations) : IConfirmationRepository
    {
        public Task<DisputeConfirmation?> GetByDisputeIdAsync(long disputeId, CancellationToken cancellationToken)
        {
            return confirmations
                .AsNoTracking()
                .Where(confirmation => confirmation.DisputeId == disputeId)
                .OrderByDescending(confirmation => confirmation.CreatedAt)
                .Select(confirmation => new DisputeConfirmation(
                    confirmation.DisputeId,
                    confirmation.OutboxEventId,
                    confirmation.Message,
                    confirmation.CreatedAt))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public void Add(DisputeConfirmation confirmation)
        {
            ArgumentNullException.ThrowIfNull(confirmation);

            confirmations.Add(new ConfirmationRecord(
                confirmation.DisputeId,
                confirmation.OutboxEventId,
                confirmation.Message,
                confirmation.CreatedAt));
        }
    }
}
