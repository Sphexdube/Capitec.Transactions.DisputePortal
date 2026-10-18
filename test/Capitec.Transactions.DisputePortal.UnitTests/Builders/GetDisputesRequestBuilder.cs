using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;

namespace Capitec.Transactions.DisputePortal.UnitTests.Builders
{
    public sealed class GetDisputesRequestBuilder
    {
        private long _customerId = 1;
        private string? _cursor;
        private int _pageSize = 2;

        public GetDisputesRequestBuilder WithCustomerId(long customerId)
        {
            _customerId = customerId;
            return this;
        }

        public GetDisputesRequestBuilder WithCursor(string? cursor)
        {
            _cursor = cursor;
            return this;
        }

        public GetDisputesRequestBuilder WithPageSize(int pageSize)
        {
            _pageSize = pageSize;
            return this;
        }

        public GetDisputesRequest Build()
        {
            return new GetDisputesRequest
            {
                CustomerId = _customerId,
                Cursor = _cursor,
                PageSize = _pageSize
            };
        }
    }
}
