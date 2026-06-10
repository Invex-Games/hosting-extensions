# Invex Hosting Extensions

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)

Useful utilities for [`Microsoft.Extensions.Hosting`](https://learn.microsoft.com/dotnet/core/extensions/generic-host) —
a tiny, dependency-light library that fills three common gaps in the generic host programming model.

## Features

- **Multi-interface registration** — `AddSingleton`, `AddScoped`, and `AddHostedService` overloads
  that register one implementation under up to five service types, all resolving to the **same
  instance**. Particularly useful for exposing a running hosted service through its other
  interfaces.
- **Host control** — `IHostControl`, a drop-in superset of `IHostApplicationLifetime` that adds
  `StopApplication(int exitCode)` for graceful shutdown **with a process exit code**.
- **Cycle background service** — `CycleBackgroundService`, a `BackgroundService` base class that
  runs periodic work on a **fixed, drift-free cadence** with overrun catch-up and clean shutdown.

## Installation

```powershell
dotnet add package Invex.Extensions.Hosting
```

Targets `net10.0`, `net9.0`, `net8.0`, `net48`, and `netstandard2.0`. The only runtime dependency
is `Microsoft.Extensions.Hosting.Abstractions`.

## Quick start

```csharp
using Invex.Extensions.Hosting;
using Invex.Extensions.Hosting.Control;
using Invex.Extensions.Hosting.Service;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostControl();                          // IHostControl
builder.Services.AddHostedService<IWorkerStatus, Worker>(); // hosted service + interface, one instance

await builder.Build().RunAsync();

public interface IWorkerStatus { bool IsBusy { get; } }

public sealed class Worker(IHostControl hostControl) : CycleBackgroundService, IWorkerStatus
{
    public bool IsBusy { get; private set; }

    protected override int CycleCadenceMs => 5_000; // every 5 seconds, on a fixed schedule

    protected override async Task ExecuteCycleAsync(CancellationToken stoppingToken)
    {
        IsBusy = true;
        try { await DoWorkAsync(stoppingToken); }
        catch (FatalException) { hostControl.StopApplication(exitCode: 1); }
        finally { IsBusy = false; }
    }
}
```

## Documentation

| | |
|---|---|
| 📖 [Documentation home](docs/index.md) | Overview and table of contents |
| 🚀 [Getting started](docs/getting-started.md) | Installation and a 5-minute tour |
| 🧩 [Multi-interface registration](docs/multi-interface-registration.md) | Shared-instance DI registrations |
| 🛑 [Host control](docs/host-control.md) | Graceful shutdown with exit codes |
| 🔁 [Cycle background service](docs/cycle-background-service.md) | Drift-free periodic workers |
| 📚 [API reference](api/index.md) | Full public API documentation |

## License

[MIT](LICENSE.txt) © Declan Smith
