namespace Invex.Extensions.Hosting.Tests.Service;

[SuppressMessage("ReSharper", "MethodSupportsCancellation")]
[SuppressMessage("Reliability", "CA2016:Forward the \'CancellationToken\' parameter to methods")]
public class TestCycleBackgroundService : CycleBackgroundService
{
    public int ExecuteCycleAsyncCallCount { get; private set; }

    public bool ThrowImmediateException { get; init; }

    public bool ThrowDelayedException { get; init; }

    protected override int CycleCadenceMs => 200;

    protected override async Task ExecuteCycleAsync(CancellationToken stoppingToken)
    {
        if (ThrowImmediateException)
            throw new("Test exception");

        if (ThrowDelayedException)
        {
            await Task.Delay(100);

            throw new("Test exception");
        }

        ExecuteCycleAsyncCallCount++;
    }
}
