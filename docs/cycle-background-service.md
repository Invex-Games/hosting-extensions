# Cycle Background Service

`CycleBackgroundService` (namespace `Invex.Extensions.Hosting.Service`) is an abstract
`BackgroundService` for periodic work. You supply the cadence and the per-cycle work; it supplies a
**fixed, drift-free schedule** and clean shutdown handling.

## The problem

The naive periodic-worker loop drifts:

```csharp
// ❌ Actual period = work duration + delay, so cycles slip later and later.
while (!stoppingToken.IsCancellationRequested)
{
    await DoWorkAsync(stoppingToken);
    await Task.Delay(5_000, stoppingToken);
}
```

If each cycle's work takes 800 ms, this loop actually runs every ~5.8 seconds — and the error
accumulates. Getting fixed-rate scheduling, cancellation, and overrun handling right is fiddly
boilerplate that ends up copied between services.

## The solution

```csharp
using Invex.Extensions.Hosting.Service;

public sealed class MetricsFlusher(IMetricsBuffer buffer) : CycleBackgroundService
{
    protected override int CycleCadenceMs => 10_000; // every 10 seconds, on schedule

    protected override async Task ExecuteCycleAsync(CancellationToken stoppingToken) =>
        await buffer.FlushAsync(stoppingToken);
}
```

Register it like any hosted service — or with a
[multi-interface overload](multi-interface-registration.md) if other components need to talk to it:

```csharp
builder.Services.AddHostedService<MetricsFlusher>();
// or
builder.Services.AddHostedService<IMetricsFlusher, MetricsFlusher>();
```

## Scheduling semantics

The cadence is **anchored to a fixed schedule**, not measured from the end of each cycle. Each
cycle is scheduled `CycleCadenceMs` milliseconds after the *previous scheduled time*, using
`Stopwatch` timestamps (monotonic — immune to wall-clock changes).

```text
cadence = 1000 ms, work = 200 ms

scheduled:   0        1000        2000        3000
actual:      [work]   [work]      [work]      [work]
             0-200    1000-1200   2000-2200   3000-3200   ← no drift
```

### Overrun and catch-up

If a cycle takes longer than the cadence, subsequent cycles run **back-to-back without delay**
until the schedule catches up:

```text
cadence = 1000 ms; cycle 1 takes 2500 ms

scheduled:   0           1000   2000   3000
actual:      [cycle 1.........] [c2]   [c3]   [cycle 4]
             0--------2500      2500   2700   3000        ← catches up, then resumes cadence
```

Cycles **never overlap** — the next cycle does not begin until the previous one completes. If your
work can persistently exceed the cadence and catch-up bursts are undesirable, throttle inside
`ExecuteCycleAsync`.

## Members to implement

| Member | Purpose |
|---|---|
| `int CycleCadenceMs { get; }` | Milliseconds between *scheduled* cycle starts. Read at the start of each cycle, so it may vary between cycles. |
| `Task ExecuteCycleAsync(CancellationToken)` | The per-cycle work. Pass the token to async operations so shutdown is prompt. |

## Shutdown behavior

- When the host stops, the in-flight `Task.Delay` is canceled immediately and the loop exits —
  the service never waits out the remainder of a cycle gap.
- If the token fires *during* `ExecuteCycleAsync`, your implementation is responsible for
  honoring it (as with any `BackgroundService`).

## Error handling

Any unhandled exception thrown from `ExecuteCycleAsync` stops the cycle loop, and the host handles
it per `HostOptions.BackgroundServiceExceptionBehavior` (default on modern .NET: **stop the host**).
If the service should survive failures and keep cycling, catch inside the cycle:

```csharp
protected override async Task ExecuteCycleAsync(CancellationToken stoppingToken)
{
    try
    {
        await DoWorkAsync(stoppingToken);
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
    {
        throw; // let shutdown proceed
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Cycle failed; will retry next cycle.");
    }
}
```

> Tip: pair with [`IHostControl`](host-control.md) to stop the application with a failure exit
> code once errors exceed a threshold.

## See also

- [API reference: CycleBackgroundService](../api/CycleBackgroundService.md)
- [Multi-interface registration](multi-interface-registration.md)
- [Host control](host-control.md)

