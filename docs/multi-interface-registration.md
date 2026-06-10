# Multi-Interface Registration

`Invex.Extensions.Hosting.ServiceCollectionExtensions` provides `AddSingleton`, `AddScoped`, and
`AddHostedService` overloads that register **one implementation type under multiple service types**,
while guaranteeing all service types resolve to the **same instance** (per lifetime).

## The problem

With the standard `Microsoft.Extensions.DependencyInjection` API, registering the same
implementation under two service types creates **two independent instances**:

```csharp
// ❌ Two separate MyService instances — IFoo and IBar do NOT share state.
services.AddSingleton<IFoo, MyService>();
services.AddSingleton<IBar, MyService>();
```

The usual workaround is verbose and easy to get wrong:

```csharp
// ✅ Works, but boilerplate that must be repeated for every service type.
services.AddSingleton<MyService>();
services.AddSingleton<IFoo>(sp => sp.GetRequiredService<MyService>());
services.AddSingleton<IBar>(sp => sp.GetRequiredService<MyService>());
```

## The solution

```csharp
using Invex.Extensions.Hosting;

// ✅ One MyService instance, resolvable as IFoo, IBar, or MyService itself.
services.AddSingleton<IFoo, IBar, MyService>();
```

Under the hood this performs exactly the manual pattern above: the implementation type is
registered once, and each service type is registered as a *forward* to it.

## Available overloads

All overloads follow the pattern `Add{Lifetime}<TService1, …, TServiceN, TImplementation>(services)`
where `TImplementation` must implement every listed service type.

| Method | Service types | Lifetime semantics |
|---|---|---|
| `AddSingleton<TService1..N, TImplementation>` | 2–5 | One shared instance for the application lifetime. |
| `AddScoped<TService1..N, TImplementation>` | 2–5 | One shared instance **per scope**; different scopes get different instances. |
| `AddHostedService<TService, TImplementation>` | 1 | Singleton + registered as `IHostedService`. |
| `AddHostedService<TService1..N, TImplementation>` | 2–5 | Singleton + registered as `IHostedService`. |

In every case the implementation type itself (`TImplementation`) is also resolvable directly.

## Hosted services with extra interfaces

The `AddHostedService` overloads are the highlight: the standard
`services.AddHostedService<TImplementation>()` registers the implementation *only* as
`IHostedService`, making the running instance invisible to the rest of the application. These
overloads make the **live hosted service instance** resolvable through its other interfaces:

```csharp
public interface IQueueMonitor { int Depth { get; } }

public sealed class QueueWorker : BackgroundService, IQueueMonitor
{
    public int Depth { get; private set; }
    protected override Task ExecuteAsync(CancellationToken ct) => /* ... */ Task.CompletedTask;
}

// The hosted service and the IQueueMonitor are the SAME object.
services.AddHostedService<IQueueMonitor, QueueWorker>();

// Elsewhere, e.g. a minimal-API endpoint:
app.MapGet("/queue/depth", (IQueueMonitor monitor) => monitor.Depth);
```

## Behavioral notes

- **Shared instance guarantee.** Resolving any of the registered service types (or
  `TImplementation` itself) yields the same instance — per application for singletons, per scope
  for scoped registrations.
- **Disposal.** The container owns the instance and disposes it once (it disposes the
  `TImplementation` registration; the forwards are factory registrations that don't trigger a
  second disposal).
- **Not "TryAdd".** Registrations are appended unconditionally, matching the behavior of the
  framework's own `AddSingleton`/`AddScoped`.
- **Trimming/AOT.** On .NET 8+, `TImplementation` is annotated with
  `[DynamicallyAccessedMembers(PublicConstructors)]` so its constructors survive trimming.

## See also

- [API reference: ServiceCollectionExtensions](../api/ServiceCollectionExtensions.md)
- [Cycle background service](cycle-background-service.md) — a natural pairing with
  `AddHostedService` overloads.

