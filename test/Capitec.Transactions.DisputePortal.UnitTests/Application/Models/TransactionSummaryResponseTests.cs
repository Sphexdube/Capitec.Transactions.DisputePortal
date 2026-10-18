using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Transactions;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Models
{
    public sealed class TransactionSummaryResponseTests
    {
        [Test]
        public void From_WhenTransactionIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => TransactionSummaryResponse.From(null!));
        }
    }
}
