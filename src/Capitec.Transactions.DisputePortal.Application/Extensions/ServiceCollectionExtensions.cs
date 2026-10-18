using Capitec.Transactions.DisputePortal.Application.Handlers;
using Capitec.Transactions.DisputePortal.Application.Handlers.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Request.V1;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Confirmations;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.DemoSessions;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Disputes;
using Capitec.Transactions.DisputePortal.Application.Models.Response.V1.Transactions;
using Capitec.Transactions.DisputePortal.Application.Validators.V1;
using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Capitec.Transactions.DisputePortal.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.TryAddSingleton(TimeProvider.System);

            services.AddScoped<IRequestHandler<SubmitDisputeRequest, SubmitDisputeResponse>, SubmitDisputeHandler>();
            services.AddScoped<IRequestHandler<GetDisputeRequest, GetDisputeResponse>, GetDisputeHandler>();
            services.AddScoped<IRequestHandler<GetDisputesRequest, GetDisputesResponse>, GetDisputesHandler>();
            services.AddScoped<IRequestHandler<GetDisputeHistoryRequest, IReadOnlyList<DisputeHistoryResponse>>, GetDisputeHistoryHandler>();
            services.AddScoped<IRequestHandler<GetConfirmationRequest, GetConfirmationResponse>, GetConfirmationHandler>();
            services.AddScoped<IRequestHandler<GetTransactionsRequest, GetTransactionsResponse>, GetTransactionsHandler>();
            services.AddScoped<IRequestHandler<GetTransactionRequest, TransactionSummaryResponse>, GetTransactionHandler>();
            services.AddScoped<IRequestHandler<GetDemoCustomersRequest, IReadOnlyList<DemoCustomerOptionResponse>>, GetDemoCustomersHandler>();
            services.AddScoped<IRequestHandler<CreateDemoSessionRequest, DemoSessionResponse>, CreateDemoSessionHandler>();
            services.AddScoped<IDisputeConfirmationProcessor, DisputeConfirmationProcessor>();

            services.AddScoped<IValidator<SubmitDisputeRequest>, SubmitDisputeRequestValidator>();
            services.AddScoped<IValidator<GetDisputesRequest>, GetDisputesRequestValidator>();
            services.AddScoped<IValidator<GetTransactionsRequest>, GetTransactionsRequestValidator>();

            return services;
        }
    }
}
