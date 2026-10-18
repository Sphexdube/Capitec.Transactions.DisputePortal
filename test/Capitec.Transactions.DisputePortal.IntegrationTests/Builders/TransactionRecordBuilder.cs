using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;

namespace Capitec.Transactions.DisputePortal.IntegrationTests.Builders
{
    public sealed class TransactionRecordBuilder
    {
        private Guid _externalId = Guid.NewGuid();
        private long _customerId;
        private DateTimeOffset _occurredAt = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
        private long _amountMinor = 10000;

        public TransactionRecordBuilder WithCustomerId(long customerId)
        {
            _customerId = customerId;
            return this;
        }

        public TransactionRecordBuilder WithOccurredAt(DateTimeOffset occurredAt)
        {
            _occurredAt = occurredAt;
            return this;
        }

        public TransactionRecordBuilder WithAmountMinor(long amountMinor)
        {
            _amountMinor = amountMinor;
            return this;
        }

        public TransactionRecord Build()
        {
            return new TransactionRecord(_externalId, _customerId, _occurredAt, "Test Merchant", "Test purchase", _amountMinor, "ZAR", "Completed");
        }
    }
}
