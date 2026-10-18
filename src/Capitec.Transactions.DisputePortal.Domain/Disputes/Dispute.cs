using System.Globalization;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;

namespace Capitec.Transactions.DisputePortal.Domain.Disputes
{
    public sealed class Dispute
    {
        public long Id { get; private set; }

        public Guid ExternalId { get; private set; }

        public long CustomerId { get; private set; }

        public long TransactionId { get; private set; }

        public string Reason { get; private set; } = default!;

        public string? Explanation { get; private set; }

        public DisputeStatus Status { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        private Dispute()
        {
        }

        public Dispute(SubmitDispute command)
        {
            Process(command);
        }

        public DisputeStatusChange Process(ChangeDisputeStatus command)
        {
            ArgumentNullException.ThrowIfNull(command);

            if (!IsAllowedTransition(Status, command.Status))
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    ErrorMessages.DisputeTransitionNotAllowedFormat,
                    Status,
                    command.Status));
            }

            DisputeStatus previousStatus = Status;
            Status = command.Status;
            UpdatedAt = command.ChangedAt;

            return new DisputeStatusChange(previousStatus, Status, UpdatedAt);
        }

        private void Process(SubmitDispute command)
        {
            ArgumentNullException.ThrowIfNull(command);

            if (command.CustomerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(command), command.CustomerId, ValidationMessages.CustomerRequired);
            }

            if (command.TransactionId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(command), command.TransactionId, ValidationMessages.TransactionRequired);
            }

            if (string.IsNullOrWhiteSpace(command.Reason))
            {
                throw new ArgumentException(ValidationMessages.DisputeReasonRequired, nameof(command));
            }

            ExternalId = command.ExternalId;
            CustomerId = command.CustomerId;
            TransactionId = command.TransactionId;
            Reason = command.Reason.Trim();
            Explanation = string.IsNullOrWhiteSpace(command.Explanation) ? null : command.Explanation.Trim();
            Status = DisputeStatus.Submitted;
            CreatedAt = command.SubmittedAt;
            UpdatedAt = command.SubmittedAt;
        }

        private static bool IsAllowedTransition(DisputeStatus currentStatus, DisputeStatus nextStatus)
        {
            return (currentStatus, nextStatus) switch
            {
                (DisputeStatus.Submitted, DisputeStatus.UnderReview) => true,
                (DisputeStatus.UnderReview, DisputeStatus.Resolved) => true,
                (DisputeStatus.UnderReview, DisputeStatus.Rejected) => true,
                _ => false
            };
        }
    }
}
