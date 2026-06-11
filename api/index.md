# API Reference — Invex.Extensions.Hosting

Assembly: `Invex.Extensions.Hosting` · Package: [`Invex.Extensions.Hosting`](https://www.nuget.org/packages/Invex.Extensions.Hosting)

The public API surface consists of exactly four types. It is locked by snapshot tests
(`PublicApiTests` in the test project), so changes here are always deliberate.

## Namespaces and types

### `Invex.Extensions.Hosting`

| Type | Kind | Description |
|---|---|---|
| [`ServiceCollectionExtensions`](ServiceCollectionExtensions.md) | static class | `AddSingleton`/`AddScoped`/`AddHostedService` overloads that register one implementation under multiple service types sharing a single instance. |

### `Invex.Extensions.Hosting.Control`

| Type | Kind | Description |
|---|---|---|
| [`IHostControl`](IHostControl.md) | interface | Extends `IHostApplicationLifetime` with `StopApplication(int exitCode)`. |
| [`HostControlHostExtensions`](HostControlHostExtensions.md) | static class | `AddHostControl()` registration extension. |

### `Invex.Extensions.Hosting.Service`

| Type | Kind | Description |
|---|---|---|
| [`CycleBackgroundService`](CycleBackgroundService.md) | abstract class | `BackgroundService` base class that runs work on a fixed, drift-free cadence. |

## Conceptual documentation

For task-oriented guides, see the [`docs/`](../docs/index.md) directory:

- [Getting started](../docs/getting-started.md)
- [Multi-interface registration](../docs/multi-interface-registration.md)
- [Host control](../docs/host-control.md)
- [Cycle background service](../docs/cycle-background-service.md)

