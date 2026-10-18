using Capitec.Transactions.DisputePortal.Domain.Authentication;

namespace Capitec.Transactions.DisputePortal.Domain.Interfaces
{
    public interface IDemoTokenIssuer
    {
        public DemoToken Issue(long customerId);
    }
}
