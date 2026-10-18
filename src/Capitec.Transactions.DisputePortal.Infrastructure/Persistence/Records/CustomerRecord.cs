namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Records
{
    public sealed class CustomerRecord
    {
        public long Id { get; private set; }

        public Guid ExternalId { get; private set; }

        public string DisplayName { get; private set; } = default!;

        public CustomerRecord(Guid externalId, string displayName)
        {
            ExternalId = externalId;
            DisplayName = displayName;
        }
    }
}