# ServiceCollectionExtensions

```csharp
namespace Invex.Extensions.Hosting;

public static class ServiceCollectionExtensions
```

Provides extension methods for `IServiceCollection` that register a single implementation type
under multiple service types ("forwarded" registrations) while sharing one instance per lifetime.

The standard `AddSingleton`/`AddScoped` methods create an *independent* instance per service type
when the same implementation is registered multiple times. The overloads in this class instead
register the implementation type once and forward each service type to it, so resolving any of the
service types yields the same instance (per scope, for scoped registrations).

> Conceptual guide: [Multi-interface registration](../docs/multi-interface-registration.md)

---

## Methods

### AddSingleton

```csharp
public static IServiceCollection AddSingleton<TService1, TService2, TImplementation>(
    this IServiceCollection services)
    where TService1 : class
    where TService2 : class
    where TImplementation : class, TService1, TService2;
```

Overloads exist for **2, 3, 4, and 5** service types
(`AddSingleton<TService1, …, TService5, TImplementation>`), each with the corresponding
`where TImplementation : class, TService1, …, TServiceN` constraint.

Adds a singleton service of each specified service type with a single shared implementation
instance.

| | |
|---|---|
| **Type parameters** | `TService1…N` — the service types to add. `TImplementation` — the implementation type; must implement all service types. |
| **Parameters** | `services` — the `IServiceCollection` to add the service to. |
| **Returns** | The `services` instance so that additional calls can be chained. |

**Remarks.** A single registration is created for `TImplementation`, and each service type is
registered as a forward to it, so all service types (and `TImplementation` itself) resolve to the
same shared instance.

```csharp
services.AddSingleton<IFoo, IBar, MyService>();

// All three resolve to the same object:
var a = provider.GetRequiredService<IFoo>();
var b = provider.GetRequiredService<IBar>();
var c = provider.GetRequiredService<MyService>();
```

---

### AddScoped

```csharp
public static IServiceCollection AddScoped<TService1, TService2, TImplementation>(
    this IServiceCollection services)
    where TService1 : class
    where TService2 : class
    where TImplementation : class, TService1, TService2;
```

Overloads exist for **2, 3, 4, and 5** service types.

Adds a scoped service of each specified service type with a single implementation instance shared
**within each scope**.

| | |
|---|---|
| **Type parameters** | `TService1…N` — the service types to add. `TImplementation` — the implementation type; must implement all service types. |
| **Parameters** | `services` — the `IServiceCollection` to add the service to. |
| **Returns** | The `services` instance so that additional calls can be chained. |

**Remarks.** All service types resolve to the same instance within a given scope; different scopes
receive different instances.

---

### AddHostedService

```csharp
public static IServiceCollection AddHostedService<TService, TImplementation>(
    this IServiceCollection serviceCollection)
    where TService : class
    where TImplementation : class, TService, IHostedService;
```

Overloads exist for **1, 2, 3, 4, and 5** service types
(`AddHostedService<TService1, …, TService5, TImplementation>`); `TImplementation` must additionally
implement `IHostedService`.

Adds a hosted service of each specified service type with a single shared implementation instance
that is also registered as the `IHostedService`.

| | |
|---|---|
| **Type parameters** | `TService1…N` — the service types to add. `TImplementation` — the implementation type; must implement all service types and `IHostedService`. |
| **Parameters** | `serviceCollection` — the `IServiceCollection` to add the service to. |
| **Returns** | The `serviceCollection` instance so that additional calls can be chained. |

**Remarks.** `TImplementation` is registered as a singleton and as a hosted service. Every service
type and the hosted service resolve to the same shared instance, allowing other components to
interact with the running hosted service through its service interfaces.

```csharp
services.AddHostedService<IWorkerStatus, MyWorker>();

// The running hosted service is the same object as:
var status = provider.GetRequiredService<IWorkerStatus>();
```

---

## Trimming / AOT

On .NET 8 and later, the `TImplementation` type parameter of every overload is annotated with
`[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]`, so the
constructors required by the DI container are preserved under trimming.

## See also

- [`IHostControl`](IHostControl.md)
- [`CycleBackgroundService`](CycleBackgroundService.md)
- [API index](index.md)

