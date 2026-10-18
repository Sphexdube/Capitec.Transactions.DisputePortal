using Capitec.Transactions.DisputePortal.Domain.Disputes;

namespace Capitec.Transactions.DisputePortal.UnitTests.Builders
{
    public sealed class SubmitDisputeBuilder
    {
        private Guid _externalId = Guid.NewGuid();
        private long _customerId = 1;
        private long _transactionId = 1;
        private string _reason = "Card purchase not recognised";
        private string? _explanation;
        private DateTimeOffset _submittedAt = new(2026, 10, 18, 10, 0, 0, TimeSpan.Zero);

        public SubmitDisputeBuilder WithExternalId(Guid externalId)
        {
            _externalId = externalId;
            return this;
        }

        public SubmitDisputeBuilder WithCustomerId(long customerId)
        {
            _customerId = customerId;
            return this;
        }

        public SubmitDisputeBuilder WithTransactionId(long transactionId)
        {
            _transactionId = transactionId;
            return this;
        }

        public SubmitDisputeBuilder WithReason(string reason)
        {
            _reason = reason;
            return this;
        }

        public SubmitDisputeBuilder WithExplanation(string? explanation)
        {
            _explanation = explanation;
            return this;
        }

        public SubmitDispute Build()
        {
            return new SubmitDispute
            {
                ExternalId = _externalId,
                CustomerId = _customerId,
                TransactionId = _transactionId,
                Reason = _reason,
                Explanation = _explanation,
                SubmittedAt = _submittedAt
            };
        }
    }
}
