# Getting Started

## Installation

Install the NuGet package into the project that builds your host:

```powershell
dotnet add package Invex.Extensions.Hosting
```

## Supported frameworks

| Target | Notes |
|---|---|
| .NET 10 | |
| .NET 9 | |
| .NET 8 | Trimming/AOT annotations active from .NET 8 onward |
| .NET Framework 4.8 | |
| .NET Standard 2.0 | Any runtime that supports `Microsoft.Extensions.Hosting.Abstractions` |

The only runtime dependency is `Microsoft.Extensions.Hosting.Abstractions`.

## Namespaces

```csharp
using Invex.Extensions.Hosting;          // ServiceCollectionExtensions
using Invex.Extensions.Hosting.Control;  // IHostControl, AddHostControl()
using Invex.Extensions.Hosting.Service;  // CycleBackgroundService
```

## A 5-minute tour

The example below shows all three features working together: a periodic worker that exposes a
status interface, and that can shut the application down with an error exit code when something
goes irrecoverably wrong.

```csharp
using Invex.Extensions.Hosting;
using Invex.Extensions.Hosting.Control;
using Invex.Extensions.Hosting.Service;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// 1. IHostControl: graceful shutdown with an exit code.
builder.Services.AddHostControl();

// 2. Multi-interface hosted service registration: the SAME HealthWorker instance
//    is the running hosted service AND the resolvable IHealthStatus.
builder.Services.AddHostedService<IHealthStatus, HealthWorker>();

await builder.Build().RunAsync();

// --- the worker ---

public interface IHealthStatus
{
    bool IsHealthy { get; }
}

// 3. CycleBackgroundService: runs ExecuteCycleAsync every 5 seconds on a fixed cadence.
public sealed class HealthWorker(IHostControl hostControl) : CycleBackgroundService, IHealthStatus
{
    private int _consecutiveFailures;

    public bool IsHealthy => _consecutiveFailures == 0;

    protected override int CycleCadenceMs => 5_000;

    protected override async Task ExecuteCycleAsync(CancellationToken stoppingToken)
    {
        try
        {
            await CheckSomethingAsync(stoppingToken);
            _consecutiveFailures = 0;
        }
        catch (Exception) when (++_consecutiveFailures >= 3)
        {
            // Give up: stop the host and report failure to the OS / orchestrator.
            hostControl.StopApplication(exitCode: 1);
        }
    }

    private static Task CheckSomethingAsync(CancellationToken ct) => Task.CompletedTask;
}
```

What each piece contributes:

1. **`AddHostControl()`** registers [`IHostControl`](../api/IHostControl.md), a superset of
   `IHostApplicationLifetime` that adds `StopApplication(int exitCode)`.
2. **`AddHostedService<TService, TImplementation>()`** registers `HealthWorker` once and forwards
   `IHealthStatus` *and* `IHostedService` to that single instance — so a health endpoint can inject
   `IHealthStatus` and observe the live worker. See
   [multi-interface registration](multi-interface-registration.md).
3. **`CycleBackgroundService`** handles the timing loop, so the worker only implements the per-cycle
   work. See [cycle background service](cycle-background-service.md).

## Next steps

- [Multi-interface registration](multi-interface-registration.md)
- [Host control](host-control.md)
- [Cycle background service](cycle-background-service.md)
- [API reference](../api/index.md)

