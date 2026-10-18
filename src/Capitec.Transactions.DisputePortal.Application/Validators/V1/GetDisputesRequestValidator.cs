using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Paging;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using FluentValidation;

namespace Capitec.Transactions.DisputePortal.Application.Validators.V1
{
    public sealed class GetDisputesRequestValidator : AbstractValidator<GetDisputesRequest>
    {
        public GetDisputesRequestValidator()
        {
            RuleFor(request => request.CustomerId)
                .GreaterThan(0)
                .WithMessage(ValidationMessages.CustomerRequired)
                .OverridePropertyName(FieldNames.CustomerId);

            RuleFor(request => request.PageSize)
                .InclusiveBetween(PageLimits.MinimumPageSize, PageLimits.MaximumPageSize)
                .WithMessage(ValidationMessages.PageSizeOutOfRange)
                .OverridePropertyName(FieldNames.PageSize);

            RuleFor(request => request.Cursor)
                .Must(PageCursorCodec.IsValidOrEmpty)
                .WithMessage(ValidationMessages.DisputeCursorInvalid)
                .OverridePropertyName(FieldNames.Cursor);
        }
    }
}
