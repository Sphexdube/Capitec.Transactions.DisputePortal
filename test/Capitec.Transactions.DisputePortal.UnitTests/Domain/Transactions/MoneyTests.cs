using Capitec.Transactions.DisputePortal.Domain.Transactions;

namespace Capitec.Transactions.DisputePortal.UnitTests.Domain.Transactions
{
    public sealed class MoneyTests
    {
        private static readonly Currency Zar = new("ZAR", 2);
        private static readonly Currency Jpy = new("JPY", 0);

        [Test]
        public void Add_WhenCurrenciesMatch_ShouldReturnExactMinorUnits()
        {
            Money result = new Money(10, Zar).Add(new Money(20, Zar));

            Assert.That(result.AmountMinor, Is.EqualTo(30));
            Assert.That(result.Currency, Is.EqualTo(Zar));
        }

        [Test]
        public void Add_WhenCurrenciesDiffer_ShouldThrowInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new Money(10, Zar).Add(new Money(10, Jpy)));
        }

        [Test]
        public void Add_WhenMinorUnitsOverflow_ShouldThrowOverflowException()
        {
            Assert.Throws<OverflowException>(() => new Money(long.MaxValue, Zar).Add(new Money(1, Zar)));
        }

        [Test]
        public void Add_WhenOtherIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Money(1, Zar).Add(null!));
        }

        [Test]
        public void Constructor_WhenCurrencyIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Money(1, null!));
        }
    }
}
