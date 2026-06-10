# IHostControl

```csharp
namespace Invex.Extensions.Hosting.Control;

public interface IHostControl : IHostApplicationLifetime
```

Provides control over the host application lifetime, extending `IHostApplicationLifetime` with the
ability to stop the application with a specific exit code.

Register this service by calling
[`AddHostControl()`](HostControlHostExtensions.md), then inject it in place of
`IHostApplicationLifetime` wherever the extra control is needed. All members inherited from
`IHostApplicationLifetime` delegate to the host's lifetime implementation, so existing lifetime
behavior is unchanged.

> Conceptual guide: [Host control](../docs/host-control.md)

---

## Methods

### StopApplication(int)

```csharp
void StopApplication(int exitCode = 0);
```

Requests termination of the current application and sets the process exit code.

| | |
|---|---|
| **Parameters** | `exitCode` — the exit code returned to the operating system when the application exits. By convention, `0` indicates success and any non-zero value indicates failure. |
| **Returns** | `void` — returns immediately; does not wait for shutdown to complete. |

**Remarks.** Sets `Environment.ExitCode` to `exitCode` and then calls
`IHostApplicationLifetime.StopApplication()` to begin a graceful shutdown. Hosted services are
stopped via `StopAsync`, and the `ApplicationStopping`/`ApplicationStopped` tokens fire as usual.

---

## Inherited members (from `IHostApplicationLifetime`)

| Member | Description |
|---|---|
| `CancellationToken ApplicationStarted` | Triggered when the host has fully started. |
| `CancellationToken ApplicationStopping` | Triggered when the host is beginning a graceful shutdown. |
| `CancellationToken ApplicationStopped` | Triggered when the host has completed a graceful shutdown. |
| `void StopApplication()` | Requests termination without changing the exit code. |

---

## Example

```csharp
public sealed class Worker(IHostControl hostControl)
{
    public void FailFast() => hostControl.StopApplication(exitCode: 1);
}
```

## See also

- [`HostControlHostExtensions.AddHostControl`](HostControlHostExtensions.md)
- [API index](index.md)

