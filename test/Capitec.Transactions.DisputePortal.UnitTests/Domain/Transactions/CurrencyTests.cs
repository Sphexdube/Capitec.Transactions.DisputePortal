using Capitec.Transactions.DisputePortal.Domain.Transactions;

namespace Capitec.Transactions.DisputePortal.UnitTests.Domain.Transactions
{
    public sealed class CurrencyTests
    {
        [Test]
        public void Constructor_WhenCodeIsLowerCase_ShouldNormaliseToUpperCase()
        {
            Currency currency = new("zar", 2);

            Assert.That(currency.Code, Is.EqualTo("ZAR"));
            Assert.That(currency.MinorUnitExponent, Is.EqualTo(2));
        }

        [TestCase("")]
        [TestCase("ZA")]
        [TestCase("ZA1")]
        public void Constructor_WhenCodeIsInvalid_ShouldThrowArgumentException(string code)
        {
            Assert.Throws<ArgumentException>(() => new Currency(code, 2));
        }

        [TestCase(-1)]
        [TestCase(5)]
        public void Constructor_WhenExponentIsOutOfRange_ShouldThrowArgumentOutOfRangeException(int exponent)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Currency("ZAR", exponent));
        }
    }
}
