# API Reference

The `Invex.Extensions.Hosting` package exposes four public types; this reference is generated from their XML
documentation, and the public surface is locked by snapshot tests (`PublicApiTests`).

## Invex.Extensions.Hosting

| Type | Kind | Description |
|---|---|---|
| [`ServiceCollectionExtensions`](xref:Invex.Extensions.Hosting.ServiceCollectionExtensions) | static class | `AddSingleton`, `AddScoped` and `AddHostedService` overloads that register one implementation under several service types sharing a single instance. |

## Invex.Extensions.Hosting.Control

| Type | Kind | Description |
|---|---|---|
| [`IHostControl`](xref:Invex.Extensions.Hosting.Control.IHostControl) | interface | Extends `IHostApplicationLifetime` with `StopApplication(int exitCode)`. |
| [`HostControlHostExtensions`](xref:Invex.Extensions.Hosting.Control.HostControlHostExtensions) | static class | `AddHostControl()` registration extension for `IHostControl`. |

## Invex.Extensions.Hosting.Service

| Type | Kind | Description |
|---|---|---|
| [`CycleBackgroundService`](xref:Invex.Extensions.Hosting.Service.CycleBackgroundService) | abstract class | `BackgroundService` base class that runs work on a fixed, drift-free cadence. |

## Typical flow

```csharp
builder.Services.AddHostControl();                              // IHostControl
builder.Services.AddHostedService<IWorkerStatus, Worker>();     // Worker : CycleBackgroundService, IWorkerStatus
```

For task-oriented guides, start with the [Introduction](../docs/introduction.md).
