# HostControlHostExtensions

```csharp
namespace Invex.Extensions.Hosting.Control;

public static class HostControlHostExtensions
```

Provides extension methods for `IServiceCollection` to add host control capabilities.

> Conceptual guide: [Host control](../docs/host-control.md)

---

## Methods

### AddHostControl

```csharp
public static IServiceCollection AddHostControl(this IServiceCollection services);
```

Adds the [`IHostControl`](IHostControl.md) service to the `IServiceCollection`.

| | |
|---|---|
| **Parameters** | `services` — the `IServiceCollection` to add the `IHostControl` service to. |
| **Returns** | The `services` instance so that additional calls can be chained. |

**Remarks.** `IHostControl` is registered as a **singleton** that wraps the host's
`IHostApplicationLifetime`, adding the ability to stop the application with a specific exit code
via `IHostControl.StopApplication(int)`.

## Example

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostControl();
```

## See also

- [`IHostControl`](IHostControl.md)
- [API index](index.md)

