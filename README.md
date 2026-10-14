# Invex.Extensions.Hosting

[![Build](https://github.com/Invex-Games/hosting-extensions/actions/workflows/Build.yml/badge.svg)](https://github.com/Invex-Games/hosting-extensions/actions/workflows/Build.yml)
[![NuGet](https://img.shields.io/nuget/v/Invex.Extensions.Hosting.svg)](https://www.nuget.org/packages/Invex.Extensions.Hosting)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/Invex-Games/hosting-extensions/blob/main/LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%20Standard%202.0-512BD4.svg)](https://dotnet.microsoft.com/)

Small, dependency-light utilities for
[`Microsoft.Extensions.Hosting`](https://learn.microsoft.com/dotnet/core/extensions/generic-host) that remove
the boilerplate around shared-instance registrations, exit codes on shutdown and fixed-rate background work.

## Features

- **Multi-interface registration.** Register one implementation under several service types without creating duplicate instances.
- **Hosted services you can talk to.** `AddHostedService` overloads make the running hosted service resolvable through its own interfaces.
- **Host control with exit codes.** Stop the generic host gracefully while setting the process exit code.
- **Fixed-cadence background services.** Run background work on a monotonic schedule that never drifts and never overlaps.

## Installation

```powershell
dotnet add package Invex.Extensions.Hosting
```

The package targets .NET 10 and .NET Standard 2.0. The .NET Standard build can be consumed by
compatible runtimes, including .NET Framework 4.8 and older .NET versions. The only runtime dependency is
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

public sealed class FatalException : Exception;
```

`Worker` is registered once. It is both the running `IHostedService` and the instance returned when
another component injects `IWorkerStatus`.
## Usage

### Register one implementation under several service types

`ServiceCollectionExtensions` provides `AddSingleton` and `AddScoped` overloads for two to five
service types, and `AddHostedService` overloads for one to five:

```csharp
services.AddSingleton<IFoo, IBar, MyService>();
services.AddScoped<IRequestState, IRequestMetrics, RequestState>();
services.AddHostedService<IQueueMonitor, QueueWorker>();
```

The implementation type is registered once and each listed service type forwards to it, so resolving any of
them returns the same object (per scope for scoped registrations). The hosted-service overloads also register
that instance as `IHostedService`. See [Multi-Interface Registration](https://github.com/Invex-Games/hosting-extensions/blob/main/docs/multi-interface-registration.md).

### Stop the host with an exit code

Register `IHostControl` once with `AddHostControl()`, then inject it wherever you would use
`IHostApplicationLifetime`:

```csharp
builder.Services.AddHostControl();

public sealed class FailureHandler(IHostControl hostControl)
{
    public void StopForFailure() => hostControl.StopApplication(exitCode: 2);
}
```

`StopApplication(exitCode)` assigns `Environment.ExitCode`, requests graceful shutdown and returns
immediately. See [Host Control](https://github.com/Invex-Games/hosting-extensions/blob/main/docs/host-control.md).

### Run work on a fixed cadence

Derive from `CycleBackgroundService` and implement two members:

```csharp
public sealed class MetricsFlusher(IMetricsBuffer buffer) : CycleBackgroundService
{
    protected override int CycleCadenceMs => 10_000;

    protected override Task ExecuteCycleAsync(CancellationToken stoppingToken) =>
        buffer.FlushAsync(stoppingToken);
}
```

Cycles are scheduled from the previous *scheduled* start, never overlap, and catch up back-to-back after an
overrun. See [Cycle Background Service](https://github.com/Invex-Games/hosting-extensions/blob/main/docs/cycle-background-service.md).

## Documentation

Full documentation: [https://invex-games.github.io/hosting-extensions/](https://invex-games.github.io/hosting-extensions/)

| Guide | Contents |
|---|---|
| [Introduction](https://github.com/Invex-Games/hosting-extensions/blob/main/docs/introduction.md) | What the library is for, key concepts and design principles |
| [Getting Started](https://github.com/Invex-Games/hosting-extensions/blob/main/docs/getting-started.md) | Installation, supported frameworks and a guided example |
| [Multi-Interface Registration](https://github.com/Invex-Games/hosting-extensions/blob/main/docs/multi-interface-registration.md) | Shared-instance DI registrations |
| [Host Control](https://github.com/Invex-Games/hosting-extensions/blob/main/docs/host-control.md) | Graceful shutdown and exit-code behavior |
| [Cycle Background Service](https://github.com/Invex-Games/hosting-extensions/blob/main/docs/cycle-background-service.md) | Fixed-cadence scheduling, shutdown and errors |
| [API Reference](https://invex-games.github.io/hosting-extensions/api/index.html) | Complete public API |

## Development

The repository requires the .NET 10 SDK selected by `global.json`. From the repository root:

```shell
dotnet build Invex.Extensions.Hosting.slnx
dotnet test Invex.Extensions.Hosting.slnx
```

The documentation site is built with DocFX through the Atom build in `_atom/`:

```shell
atom BuildDocs   # build the site into _site/
atom ServeDocs   # build and serve it locally
```

## License

[MIT](https://github.com/Invex-Games/hosting-extensions/blob/main/LICENSE.txt) © Declan Smith
