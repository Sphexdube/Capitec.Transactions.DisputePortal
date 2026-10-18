namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records
{
    public sealed class TransactionRecord
    {
        public long Id { get; private set; }

        public Guid ExternalId { get; private set; }

        public long CustomerId { get; private set; }

        public DateTimeOffset OccurredAt { get; private set; }

        public string Merchant { get; private set; } = default!;

        public string Description { get; private set; } = default!;

        public long AmountMinor { get; private set; }

        public string CurrencyCode { get; private set; } = default!;

        public string State { get; private set; } = default!;

        public TransactionRecord(
            Guid externalId,
            long customerId,
            DateTimeOffset occurredAt,
            string merchant,
            string description,
            long amountMinor,
            string currencyCode,
            string state)
        {
            ExternalId = externalId;
            CustomerId = customerId;
            OccurredAt = occurredAt;
            Merchant = merchant;
            Description = description;
            AmountMinor = amountMinor;
            CurrencyCode = currencyCode;
            State = state;
        }
    }
}