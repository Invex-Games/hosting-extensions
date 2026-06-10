# Host Control

`IHostControl` (namespace `Invex.Extensions.Hosting.Control`) extends the framework's
`IHostApplicationLifetime` with one capability it is missing: **stopping the application gracefully
while setting the process exit code**.

## The problem

`IHostApplicationLifetime.StopApplication()` triggers a graceful shutdown, but the process always
exits with code `0` unless you separately set `Environment.ExitCode`. Components that detect fatal
conditions (poisoned queues, unrecoverable configuration errors, repeated health-check failures)
typically need both:

```csharp
// ❌ Easy to forget one half, and the intent is split across two unrelated APIs.
Environment.ExitCode = 1;
hostApplicationLifetime.StopApplication();
```

Exit codes matter to anything supervising the process: container orchestrators, Windows services,
systemd units, CI pipelines, and shell scripts all use them to distinguish success from failure.

## The solution

```csharp
using Invex.Extensions.Hosting.Control;

// Registration
builder.Services.AddHostControl();

// Usage — anywhere you would inject IHostApplicationLifetime
public sealed class PoisonMessageHandler(IHostControl hostControl)
{
    public void OnUnrecoverableError() =>
        hostControl.StopApplication(exitCode: 2); // graceful shutdown, process exits with 2
}
```

`StopApplication(int exitCode)` sets `Environment.ExitCode` and then calls the host lifetime's
`StopApplication()`. Like the framework method, it **returns immediately** — shutdown proceeds
asynchronously, running `StopAsync` on hosted services, firing `ApplicationStopping`/
`ApplicationStopped`, and honoring the host's shutdown timeout.

## A drop-in superset of `IHostApplicationLifetime`

`IHostControl` *inherits* `IHostApplicationLifetime`, and the implementation delegates every
inherited member to the host's real lifetime object. You can therefore inject `IHostControl`
anywhere you currently inject `IHostApplicationLifetime` and lose nothing:

```csharp
public sealed class StartupTasks(IHostControl hostControl)
{
    public void Register()
    {
        hostControl.ApplicationStarted.Register(() => Console.WriteLine("Started"));
        hostControl.ApplicationStopping.Register(() => Console.WriteLine("Stopping"));
        hostControl.ApplicationStopped.Register(() => Console.WriteLine("Stopped"));
    }
}
```

| Member | Source |
|---|---|
| `ApplicationStarted` / `ApplicationStopping` / `ApplicationStopped` | Delegated to `IHostApplicationLifetime` |
| `StopApplication()` | Delegated to `IHostApplicationLifetime` |
| `StopApplication(int exitCode)` | **Added** — sets `Environment.ExitCode`, then stops |

## Exit code conventions

- `0` — success (the default for the optional parameter).
- Non-zero — failure. Pick small positive values; on Unix systems valid exit codes are `0–255`,
  and values `≥ 128` conventionally indicate termination by signal.

## Behavioral notes

- **Last writer wins.** If `StopApplication(exitCode)` is called multiple times before the process
  exits, the most recent `Environment.ExitCode` assignment takes effect.
- **`Main`'s return value can override it.** If your `Main` method returns an `int`, that return
  value supersedes `Environment.ExitCode`. Use an `async Task Main` (non-`int`) entry point, or
  return `Environment.ExitCode` explicitly.
- **Singleton registration.** `AddHostControl()` registers `IHostControl` as a singleton wrapping
  the host's `IHostApplicationLifetime`.

## See also

- [API reference: IHostControl](../api/IHostControl.md)
- [API reference: HostControlHostExtensions](../api/HostControlHostExtensions.md)

