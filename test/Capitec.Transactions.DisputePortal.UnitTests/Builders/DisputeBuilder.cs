using System.Reflection;
using Capitec.Transactions.DisputePortal.Domain.Disputes;

namespace Capitec.Transactions.DisputePortal.UnitTests.Builders
{
    public sealed class DisputeBuilder
    {
        private static readonly PropertyInfo IdProperty = typeof(Dispute).GetProperty(nameof(Dispute.Id))!;

        private readonly SubmitDisputeBuilder _command = new();
        private long _id = 1;

        public DisputeBuilder WithId(long id)
        {
            _id = id;
            return this;
        }

        public DisputeBuilder WithExternalId(Guid externalId)
        {
            _command.WithExternalId(externalId);
            return this;
        }

        public DisputeBuilder WithCustomerId(long customerId)
        {
            _command.WithCustomerId(customerId);
            return this;
        }

        public DisputeBuilder WithTransactionId(long transactionId)
        {
            _command.WithTransactionId(transactionId);
            return this;
        }

        public Dispute Build()
        {
            Dispute dispute = new(_command.Build());
            IdProperty.SetValue(dispute, _id);

            return dispute;
        }
    }
}
