using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Domain.Observability.Messages;
using Capitec.Transactions.DisputePortal.Infrastructure.BackgroundJobs.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Capitec.Transactions.DisputePortal.Infrastructure.BackgroundJobs.Jobs
{
    internal sealed class DisputeConfirmationWorker(
        IServiceScopeFactory scopeFactory,
        ConfirmationWorkerConfiguration configuration,
        TimeProvider timeProvider,
        ILogger<DisputeConfirmationWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogDebug(InfoMessages.MethodStarted);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    int processedCount = await ProcessBatchAsync(stoppingToken);

                    if (processedCount == 0)
                    {
                        await Task.Delay(configuration.IdleDelay, timeProvider, stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, ErrorMessages.OutboxProcessingFailed);
                    await DelayAfterFailureAsync(stoppingToken);
                }
            }
        }

        private async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
        {
            using (IServiceScope scope = scopeFactory.CreateScope())
            {
                IDisputeConfirmationProcessor processor = scope.ServiceProvider.GetRequiredService<IDisputeConfirmationProcessor>();

                return await processor.ProcessBatchAsync(configuration.BatchSize, cancellationToken);
            }
        }

        private async Task DelayAfterFailureAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(configuration.FailureDelay, timeProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}
