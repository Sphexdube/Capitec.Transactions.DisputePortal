using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Infrastructure.BackgroundJobs.Configuration;
using Capitec.Transactions.DisputePortal.Infrastructure.BackgroundJobs.Extensions;
using Capitec.Transactions.DisputePortal.Infrastructure.BackgroundJobs.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Capitec.Transactions.DisputePortal.UnitTests.Infrastructure.BackgroundJobs
{
    public sealed class DisputeConfirmationWorkerTests
    {
        private IDisputeConfirmationProcessor _processor = null!;
        private ServiceProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            _processor = Substitute.For<IDisputeConfirmationProcessor>();
            ServiceCollection services = new();
            services.AddScoped(_ => _processor);
            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        [Test]
        public async Task ExecuteAsync_WhenBatchesSucceed_ShouldPollUntilStopped()
        {
            TaskCompletionSource idle = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _processor.ProcessBatchAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(
                _ => Task.FromResult(2),
                _ => Task.FromResult(0),
                _ =>
                {
                    idle.TrySetResult();
                    return Task.FromResult(0);
                });
            DisputeConfirmationWorker worker = CreateWorker(new ConfirmationWorkerConfiguration { IdleDelay = TimeSpan.Zero, BatchSize = 5 });

            await RunUntilAsync(worker, idle.Task);

            await _processor.Received().ProcessBatchAsync(5, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ExecuteAsync_WhenStoppedDuringIdleDelay_ShouldStopCleanly()
        {
            TaskCompletionSource called = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _processor.ProcessBatchAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(_ =>
            {
                called.TrySetResult();
                return Task.FromResult(0);
            });
            DisputeConfirmationWorker worker = CreateWorker(new ConfirmationWorkerConfiguration { IdleDelay = TimeSpan.FromHours(1) });

            await RunUntilAsync(worker, called.Task);

            Assert.That(worker.ExecuteTask!.IsCompletedSuccessfully, Is.True);
        }

        [Test]
        public async Task ExecuteAsync_WhenBatchFails_ShouldRetryAfterFailureDelay()
        {
            TaskCompletionSource retried = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _processor.ProcessBatchAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(
                _ => throw new TimeoutException(),
                call => WaitForStopAsync(retried, call.Arg<CancellationToken>()));
            DisputeConfirmationWorker worker = CreateWorker(new ConfirmationWorkerConfiguration { FailureDelay = TimeSpan.Zero });

            await RunUntilAsync(worker, retried.Task);

            Assert.That(worker.ExecuteTask!.IsCompletedSuccessfully, Is.True);
        }

        [Test]
        public async Task ExecuteAsync_WhenStoppedDuringFailureDelay_ShouldStopCleanly()
        {
            TaskCompletionSource failed = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _processor.ProcessBatchAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(_ =>
            {
                failed.TrySetResult();
                return Task.FromException<int>(new TimeoutException());
            });
            DisputeConfirmationWorker worker = CreateWorker(new ConfirmationWorkerConfiguration { FailureDelay = TimeSpan.FromHours(1) });

            await RunUntilAsync(worker, failed.Task);

            Assert.That(worker.ExecuteTask!.IsCompletedSuccessfully, Is.True);
        }

        [TestCase(true, 1)]
        [TestCase(false, 0)]
        public void AddBackgroundJobs_WhenWorkerIsToggled_ShouldRegisterHostedServiceOnlyWhenEnabled(bool enabled, int expectedCount)
        {
            ServiceCollection services = new();

            services.AddBackgroundJobs(new ConfirmationWorkerConfiguration { Enabled = enabled });

            Assert.That(services.Count(descriptor => descriptor.ServiceType == typeof(IHostedService)), Is.EqualTo(expectedCount));
            Assert.Throws<ArgumentNullException>(() => services.AddBackgroundJobs(null!));
        }

        private DisputeConfirmationWorker CreateWorker(ConfirmationWorkerConfiguration configuration)
        {
            return new DisputeConfirmationWorker(
                _provider.GetRequiredService<IServiceScopeFactory>(),
                configuration,
                TimeProvider.System,
                NullLogger<DisputeConfirmationWorker>.Instance);
        }

        private static async Task<int> WaitForStopAsync(TaskCompletionSource signal, CancellationToken cancellationToken)
        {
            signal.TrySetResult();
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return 0;
        }

        private static async Task RunUntilAsync(DisputeConfirmationWorker worker, Task condition)
        {
            await worker.StartAsync(CancellationToken.None);
            await condition.WaitAsync(TimeSpan.FromSeconds(10));
            await worker.StopAsync(CancellationToken.None);
            worker.Dispose();
        }
    }
}
