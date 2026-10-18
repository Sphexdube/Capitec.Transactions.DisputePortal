using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;

namespace Capitec.Transactions.DisputePortal.UnitTests.Builders
{
    public sealed class SubmitDisputeRequestBuilder
    {
        private Guid _transactionId = Guid.NewGuid();
        private long _customerId = 17;
        private string _idempotencyKey = Guid.NewGuid().ToString("N");
        private string _reason = "  Unrecognized transaction  ";
        private string? _explanation = "  Not mine  ";

        public SubmitDisputeRequestBuilder WithTransactionId(Guid transactionId)
        {
            _transactionId = transactionId;
            return this;
        }

        public SubmitDisputeRequestBuilder WithCustomerId(long customerId)
        {
            _customerId = customerId;
            return this;
        }

        public SubmitDisputeRequestBuilder WithIdempotencyKey(string idempotencyKey)
        {
            _idempotencyKey = idempotencyKey;
            return this;
        }

        public SubmitDisputeRequestBuilder WithReason(string reason)
        {
            _reason = reason;
            return this;
        }

        public SubmitDisputeRequestBuilder WithExplanation(string? explanation)
        {
            _explanation = explanation;
            return this;
        }

        public SubmitDisputeRequest Build()
        {
            return new SubmitDisputeRequest
            {
                TransactionId = _transactionId,
                CustomerId = _customerId,
                IdempotencyKey = _idempotencyKey,
                Reason = _reason,
                Explanation = _explanation
            };
        }
    }
}
