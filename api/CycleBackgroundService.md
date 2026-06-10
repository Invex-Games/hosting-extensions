# CycleBackgroundService

```csharp
namespace Invex.Extensions.Hosting.Service;

public abstract class CycleBackgroundService : BackgroundService
```

A `BackgroundService` that performs work in fixed-cadence cycles in the background. Derived classes
specify how often the cycle runs via [`CycleCadenceMs`](#cyclecadencems) and implement the
per-cycle work in [`ExecuteCycleAsync`](#executecycleasync).

The service starts and stops with the `IHost`, like any other `IHostedService`. Register it with
`AddHostedService` or one of the multi-interface overloads in
[`ServiceCollectionExtensions`](ServiceCollectionExtensions.md).

> Conceptual guide: [Cycle background service](../docs/cycle-background-service.md)

---

## Scheduling semantics

- The cadence is **anchored to a fixed schedule** rather than measured from the end of each cycle:
  each cycle is scheduled `CycleCadenceMs` milliseconds after the previous *scheduled* time,
  regardless of how long the cycle's work takes. Timing uses monotonic `Stopwatch` timestamps.
- If a cycle overruns its time slot, subsequent cycles run **back-to-back without delay** until the
  schedule catches up.
- Cycles **never overlap**; the next cycle does not start until the previous one completes.

---

## Members

### CycleCadenceMs

```csharp
protected abstract int CycleCadenceMs { get; }
```

The time interval in milliseconds at which the service should cycle.

**Remarks.** Read at the start of each cycle, so a derived class may vary it between cycles. The
interval measures the time between *scheduled* cycle starts, not the gap between the end of one
cycle and the start of the next.

---

### ExecuteCycleAsync

```csharp
protected abstract Task ExecuteCycleAsync(CancellationToken stoppingToken);
```

Performs the work of the service. Called once per cycle.

| | |
|---|---|
| **Parameters** | `stoppingToken` — triggered when `IHostedService.StopAsync` is called. Pass it to asynchronous operations within the cycle so the service can shut down promptly. |
| **Returns** | A `Task` representing the work of a single cycle. |

**Remarks.** The next cycle is not started until the returned task completes; cycles never overlap.
Unhandled exceptions thrown from this method stop the cycle loop and are handled per the host's
`BackgroundServiceExceptionBehavior` (by default, stopping the host) — catch and handle (or log)
exceptions here if the service should continue cycling after a failure.

---

### ExecuteAsync

```csharp
protected override Task ExecuteAsync(CancellationToken stoppingToken);
```

Runs the cycle loop: waits until the next scheduled cycle time, then invokes `ExecuteCycleAsync`,
repeating until `stoppingToken` is signaled. Derived classes should implement `ExecuteCycleAsync`
rather than overriding this method.

---

## Example

```csharp
public sealed class HeartbeatService : CycleBackgroundService
{
    protected override int CycleCadenceMs => 5_000;

    protected override Task ExecuteCycleAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Still alive!");
        return Task.CompletedTask;
    }
}

// Registration:
builder.Services.AddHostedService<HeartbeatService>();
```

## See also

- [`ServiceCollectionExtensions`](ServiceCollectionExtensions.md) — multi-interface
  `AddHostedService` overloads
- [API index](index.md)


