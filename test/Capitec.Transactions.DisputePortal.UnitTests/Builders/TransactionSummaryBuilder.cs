using Capitec.Transactions.DisputePortal.Domain.Transactions;

namespace Capitec.Transactions.DisputePortal.UnitTests.Builders
{
    public sealed class TransactionSummaryBuilder
    {
        private long _id = 1;
        private Guid _externalId = Guid.NewGuid();
        private DateTimeOffset _occurredAt = new(2026, 10, 17, 9, 30, 0, TimeSpan.Zero);
        private long _amountMinor = 125050;

        public TransactionSummaryBuilder WithId(long id)
        {
            _id = id;
            return this;
        }

        public TransactionSummaryBuilder WithExternalId(Guid externalId)
        {
            _externalId = externalId;
            return this;
        }

        public TransactionSummaryBuilder WithOccurredAt(DateTimeOffset occurredAt)
        {
            _occurredAt = occurredAt;
            return this;
        }

        public TransactionSummaryBuilder WithAmountMinor(long amountMinor)
        {
            _amountMinor = amountMinor;
            return this;
        }

        public TransactionSummary Build()
        {
            return new TransactionSummary(_id, _externalId, "Example Market", "Card purchase", _occurredAt, _amountMinor, "ZAR", "Completed");
        }
    }
}
