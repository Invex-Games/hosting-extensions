# Invex.Extensions.Hosting

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)

Small, dependency-light utilities for
[`Microsoft.Extensions.Hosting`](https://learn.microsoft.com/dotnet/core/extensions/generic-host):

- Register one implementation under multiple dependency-injection service types without creating
  duplicate instances.
- Stop a generic host gracefully while setting the process exit code.
- Run background work on a fixed, monotonic cadence without scheduling drift.

## Installation

```powershell
dotnet add package Invex.Extensions.Hosting
```

The package targets .NET 10, .NET 9, .NET 8, and .NET Standard 2.0. The .NET Standard build can
be consumed by compatible runtimes, including .NET Framework 4.8. The only runtime dependency is
`Microsoft.Extensions.Hosting.Abstractions`.

## Quick start

```csharp
using Invex.Extensions.Hosting;
using Invex.Extensions.Hosting.Control;
using Invex.Extensions.Hosting.Service;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostControl();
builder.Services.AddHostedService<IWorkerStatus, Worker>();

await builder.Build().RunAsync();

public interface IWorkerStatus
{
    bool IsBusy { get; }
}

public sealed class Worker(IHostControl hostControl) : CycleBackgroundService, IWorkerStatus
{
    public bool IsBusy { get; private set; }

    protected override int CycleCadenceMs => 5_000;

    protected override async Task ExecuteCycleAsync(CancellationToken stoppingToken)
    {
        IsBusy = true;

        try
        {
            await DoWorkAsync(stoppingToken);
        }
        catch (FatalException)
        {
            hostControl.StopApplication(exitCode: 1);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static Task DoWorkAsync(CancellationToken stoppingToken) => Task.CompletedTask;
}
```

`Worker` is registered once. It is both the running `IHostedService` and the instance returned when
another component injects `IWorkerStatus`.

## Multi-interface dependency injection

`ServiceCollectionExtensions` provides `AddSingleton` and `AddScoped` overloads for two to five
service types:

```csharp
services.AddSingleton<IFoo, IBar, MyService>();
services.AddScoped<IRequestState, IRequestMetrics, RequestState>();
```

The implementation type is registered once, and each listed service type forwards to that
registration. Resolving any listed service type or the implementation type returns the same object.
Singletons share one object for the application lifetime; scoped registrations share one object per
scope. Registrations are appended like the standard `AddSingleton` and `AddScoped` methods; they
are not conditional `TryAdd` registrations.

The `AddHostedService` overloads support one to five service types:

```csharp
services.AddHostedService<IQueueMonitor, QueueWorker>();
services.AddHostedService<IStatus, IAdminOperations, Worker>();
```

They register the implementation as a singleton, expose it through each listed service type, and
also register that same instance as `IHostedService`. This is useful when application components
need to observe or control a running hosted service.

## Host control and exit codes

Register `IHostControl` once with `AddHostControl()`:

```csharp
builder.Services.AddHostControl();

public sealed class FailureHandler(IHostControl hostControl)
{
    public void StopForFailure() => hostControl.StopApplication(exitCode: 2);
}
```

`IHostControl` inherits `IHostApplicationLifetime`, so its lifetime tokens and parameterless
`StopApplication()` behave like the framework service. `StopApplication(exitCode)` first assigns
`Environment.ExitCode`, then requests graceful shutdown, and returns immediately; it does not wait
for hosted services to finish stopping. Exit code `0` means success by convention, while a
non-zero value conventionally indicates failure. If it is called more than once, the last assigned
exit code wins.

## Fixed-cadence background services

Derive from `CycleBackgroundService` and implement two members:

```csharp
public sealed class MetricsFlusher(IMetricsBuffer buffer) : CycleBackgroundService
{
    protected override int CycleCadenceMs => 10_000;

    protected override Task ExecuteCycleAsync(CancellationToken stoppingToken) =>
        buffer.FlushAsync(stoppingToken);
}
```

The cadence is anchored to monotonic `Stopwatch` timestamps, so cycle duration does not accumulate
drift. Cycles never overlap. If work overruns its scheduled slot, overdue cycles run back-to-back
until the schedule catches up. The cadence is read once at the start of each cycle, so derived
classes may change it between cycles.

Cancellation during the inter-cycle delay exits promptly without starting another cycle. Cancellation
during `ExecuteCycleAsync` must be honored by the derived implementation. An unhandled cycle
exception stops the loop and is handled according to the host's
`HostOptions.BackgroundServiceExceptionBehavior`; catch and handle failures inside the cycle if the
service should continue.

## Documentation

| Guide | Contents |
|---|---|
| [Documentation home](docs/index.md) | Overview and design principles |
| [Getting started](docs/getting-started.md) | Installation and a guided example |
| [Multi-interface registration](docs/multi-interface-registration.md) | Shared-instance DI registrations |
| [Host control](docs/host-control.md) | Graceful shutdown and exit-code behavior |
| [Cycle background service](docs/cycle-background-service.md) | Fixed-cadence scheduling and errors |
| [API reference](api/index.md) | Complete public API |

## License

[MIT](LICENSE.txt) © Declan Smith
