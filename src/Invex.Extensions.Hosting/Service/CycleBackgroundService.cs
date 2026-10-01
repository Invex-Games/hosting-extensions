namespace Invex.Extensions.Hosting.Service;

/// <summary>
///     A <see cref="BackgroundService" /> that performs work in fixed-cadence cycles in the background.
///     Derived classes specify how often the cycle runs via <see cref="CycleCadenceMs" /> and implement the
///     per-cycle work in <see cref="ExecuteCycleAsync" />.
/// </summary>
/// <remarks>
///     <para>
///         The service starts and stops with the <see cref="Microsoft.Extensions.Hosting.IHost" />, like any other
///         <see cref="Microsoft.Extensions.Hosting.IHostedService" />. Register it with
///         <see
///             cref="ServiceCollectionServiceExtensions" />
///         <c>.AddHostedService</c> or one of the multi-interface overloads in
///         <see cref="Invex.Extensions.Hosting.ServiceCollectionExtensions" />.
///     </para>
///     <para>
///         The cadence is anchored to a fixed schedule rather than measured from the end of each cycle: each cycle is
///         scheduled <see cref="CycleCadenceMs" /> milliseconds after the previous scheduled time, regardless of how
///         long the cycle's work takes. If a cycle overruns its time slot, subsequent cycles run back-to-back without
///         delay until the schedule catches up.
///     </para>
///     <para>
///         Cycles never overlap; the next cycle does not start until the previous one completes. Any unhandled
///         exception thrown from <see cref="ExecuteCycleAsync" /> stops the service and is handled according to the
///         host's <c>BackgroundServiceExceptionBehavior</c> (by default, stopping the host). Handle exceptions inside
///         <see cref="ExecuteCycleAsync" /> if the service should keep cycling after a failure.
///     </para>
/// </remarks>
/// <example>
///     <code>
///         public sealed class HeartbeatService : CycleBackgroundService
///         {
///             protected override int CycleCadenceMs =&gt; 5_000;
///
///             protected override Task ExecuteCycleAsync(CancellationToken stoppingToken)
///             {
///                 Console.WriteLine("Still alive!");
///                 return Task.CompletedTask;
///             }
///         }
///     </code>
/// </example>
[PublicAPI]
public abstract class CycleBackgroundService : BackgroundService
{
    private long _nextCheckTime = Stopwatch.GetTimestamp();

    /// <summary>
    ///     The time interval in milliseconds at which the service should cycle.
    /// </summary>
    /// <remarks>
    ///     This value is read at the start of each cycle, so a derived class may vary it between cycles.
    ///     The interval measures the time between scheduled cycle starts, not the gap between the end of one
    ///     cycle and the start of the next.
    /// </remarks>
    protected abstract int CycleCadenceMs { get; }

    /// <summary>
    ///     Runs the cycle loop: waits until the next scheduled cycle time, then invokes
    ///     <see cref="ExecuteCycleAsync" />, repeating until <paramref name="stoppingToken" /> is signaled.
    /// </summary>
    /// <param name="stoppingToken">Triggered when the host is shutting down or the service is stopped.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> that represents the lifetime of the cycle loop.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await WaitForNextCycle(stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            await ExecuteCycleAsync(stoppingToken);
        }
    }

    private async Task WaitForNextCycle(CancellationToken stoppingToken)
    {
        var currentTime = Stopwatch.GetTimestamp();
        var timeToWaitMs = (_nextCheckTime - currentTime) * 1000 / Stopwatch.Frequency;
        _nextCheckTime += CycleCadenceMs * Stopwatch.Frequency / 1000;

        if (timeToWaitMs > 0)
        {
            try
            {
                await Task.Delay((int)timeToWaitMs, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Ignore
            }
        }
    }

    /// <summary>
    ///     Performs the work of the service. Called once per cycle.
    /// </summary>
    /// <param name="stoppingToken">
    ///     Triggered when
    ///     <see cref="Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.
    ///     Pass this token to any asynchronous operations performed within the cycle so the service can shut down promptly.
    /// </param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> that represents the work of a single cycle.</returns>
    /// <remarks>
    ///     The next cycle is not started until the returned task completes; cycles never overlap.
    ///     Unhandled exceptions thrown from this method stop the cycle loop — catch and handle (or log) exceptions
    ///     here if the service should continue cycling after a failure.
    /// </remarks>
    protected abstract Task ExecuteCycleAsync(CancellationToken stoppingToken);
}
