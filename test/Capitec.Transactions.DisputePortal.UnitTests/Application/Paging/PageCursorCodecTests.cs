using System.Text;
using Capitec.Transactions.DisputePortal.Application.Paging;

namespace Capitec.Transactions.DisputePortal.UnitTests.Application.Paging
{
    public sealed class PageCursorCodecTests
    {
        [Test]
        public void Decode_WhenCursorWasEncoded_ShouldPreservePositionInUtc()
        {
            PageCursor expected = new(new DateTimeOffset(2026, 10, 18, 17, 30, 0, TimeSpan.FromHours(2)), 12345);

            PageCursor? actual = PageCursorCodec.Decode(PageCursorCodec.Encode(expected));

            Assert.That(actual, Is.EqualTo(new PageCursor(expected.Position.ToUniversalTime(), expected.Id)));
            Assert.That(actual!.Position.Offset, Is.EqualTo(TimeSpan.Zero));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("a")]
        [TestCase("not-a-cursor")]
        [TestCase("!!!!")]
        public void Decode_WhenCursorIsMalformed_ShouldReturnNull(string? value)
        {
            Assert.That(PageCursorCodec.Decode(value), Is.Null);
        }

        [TestCase("1:2:3")]
        [TestCase("x:2")]
        [TestCase("1:x")]
        [TestCase("1:0")]
        [TestCase("9999999999999999999:1")]
        [TestCase("3155378976000000000:1")]
        public void Decode_WhenPayloadIsInvalid_ShouldReturnNull(string payload)
        {
            string value = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)).TrimEnd('=');

            Assert.That(PageCursorCodec.Decode(value), Is.Null);
        }

        [Test]
        public void Decode_WhenCursorIsOversized_ShouldReturnNull()
        {
            Assert.That(PageCursorCodec.Decode(new string('a', 257)), Is.Null);
        }

        [Test]
        public void IsValidOrEmpty_WhenCursorIsEmptyOrValid_ShouldReturnTrue()
        {
            string valid = PageCursorCodec.Encode(new PageCursor(DateTimeOffset.UnixEpoch, 1));

            Assert.That(PageCursorCodec.IsValidOrEmpty(null), Is.True);
            Assert.That(PageCursorCodec.IsValidOrEmpty(valid), Is.True);
            Assert.That(PageCursorCodec.IsValidOrEmpty("bad"), Is.False);
        }

        [Test]
        public void Encode_WhenCursorIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => PageCursorCodec.Encode(null!));
        }
    }
}
