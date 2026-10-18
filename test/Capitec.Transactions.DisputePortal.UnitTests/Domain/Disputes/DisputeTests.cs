using Capitec.Transactions.DisputePortal.Domain.Disputes;
using Capitec.Transactions.DisputePortal.UnitTests.Builders;

namespace Capitec.Transactions.DisputePortal.UnitTests.Domain.Disputes
{
    public sealed class DisputeTests
    {
        private static readonly DateTimeOffset ChangedAt = new(2026, 10, 18, 11, 0, 0, TimeSpan.Zero);

        [Test]
        public void Constructor_WhenCommandIsValid_ShouldStartSubmittedWithTrimmedText()
        {
            Dispute dispute = new(new SubmitDisputeBuilder().WithReason("  Fraud  ").WithExplanation("  Not mine  ").Build());

            Assert.That(dispute.Status, Is.EqualTo(DisputeStatus.Submitted));
            Assert.That(dispute.Reason, Is.EqualTo("Fraud"));
            Assert.That(dispute.Explanation, Is.EqualTo("Not mine"));
            Assert.That(dispute.UpdatedAt, Is.EqualTo(dispute.CreatedAt));
        }

        [TestCase(null)]
        [TestCase("   ")]
        public void Constructor_WhenExplanationIsBlank_ShouldStoreNull(string? explanation)
        {
            Dispute dispute = new(new SubmitDisputeBuilder().WithExplanation(explanation).Build());

            Assert.That(dispute.Explanation, Is.Null);
        }

        [Test]
        public void Constructor_WhenCommandIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Dispute(null!));
        }

        [Test]
        public void Constructor_WhenReasonIsBlank_ShouldThrowArgumentException()
        {
            SubmitDispute command = new SubmitDisputeBuilder().WithReason(" ").Build();

            Assert.Throws<ArgumentException>(() => new Dispute(command));
        }

        [Test]
        public void Constructor_WhenCustomerIdIsNotPositive_ShouldThrowArgumentOutOfRangeException()
        {
            SubmitDispute command = new SubmitDisputeBuilder().WithCustomerId(0).Build();

            Assert.Throws<ArgumentOutOfRangeException>(() => new Dispute(command));
        }

        [Test]
        public void Constructor_WhenTransactionIdIsNotPositive_ShouldThrowArgumentOutOfRangeException()
        {
            SubmitDispute command = new SubmitDisputeBuilder().WithTransactionId(0).Build();

            Assert.Throws<ArgumentOutOfRangeException>(() => new Dispute(command));
        }

        [TestCase(DisputeStatus.Resolved)]
        [TestCase(DisputeStatus.Rejected)]
        public void Process_WhenTransitionsAreAllowed_ShouldUpdateStatus(DisputeStatus finalStatus)
        {
            Dispute dispute = new(new SubmitDisputeBuilder().Build());

            DisputeStatusChange review = dispute.Process(new ChangeDisputeStatus { Status = DisputeStatus.UnderReview, ChangedAt = ChangedAt });
            DisputeStatusChange final = dispute.Process(new ChangeDisputeStatus { Status = finalStatus, ChangedAt = ChangedAt });

            Assert.That(review.PreviousStatus, Is.EqualTo(DisputeStatus.Submitted));
            Assert.That(final.PreviousStatus, Is.EqualTo(DisputeStatus.UnderReview));
            Assert.That(final.CurrentStatus, Is.EqualTo(finalStatus));
            Assert.That(final.ChangedAt, Is.EqualTo(ChangedAt));
            Assert.That(dispute.UpdatedAt, Is.EqualTo(ChangedAt));
        }

        [Test]
        public void Process_WhenMovingFromSubmittedToResolved_ShouldThrowInvalidOperationException()
        {
            Dispute dispute = new(new SubmitDisputeBuilder().Build());

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => dispute.Process(new ChangeDisputeStatus { Status = DisputeStatus.Resolved, ChangedAt = ChangedAt }));

            Assert.That(exception.Message, Is.EqualTo("A dispute cannot move from Submitted to Resolved."));
        }

        [Test]
        public void Process_WhenCommandIsNull_ShouldThrowArgumentNullException()
        {
            Dispute dispute = new(new SubmitDisputeBuilder().Build());

            Assert.Throws<ArgumentNullException>(() => dispute.Process(null!));
        }
    }
}
