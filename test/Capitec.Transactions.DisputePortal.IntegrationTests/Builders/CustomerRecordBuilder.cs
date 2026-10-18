using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records;

namespace Capitec.Transactions.DisputePortal.IntegrationTests.Builders
{
    public sealed class CustomerRecordBuilder
    {
        private Guid _externalId = Guid.NewGuid();
        private string _displayName = "Test Customer";

        public CustomerRecordBuilder WithDisplayName(string displayName)
        {
            _displayName = displayName;
            return this;
        }

        public CustomerRecord Build()
        {
            return new CustomerRecord(_externalId, _displayName);
        }
    }
}
