# Agent Instructions

Guidance for AI agents working in **Invex.Extensions.Hosting**. Keep changes focused; the
README documents consumer-facing usage.

## Repository layout

| Path | Purpose |
|---|---|
| `src/Invex.Extensions.Hosting/` | The library source |
| `tests/Invex.Extensions.Hosting.Tests/` | NUnit tests and Verify snapshots |
| `_atom/` | Atom build definition that generates CI workflows |
| `docs/` | Task-oriented DocFX guides |
| `api/` | DocFX API pages |
| `.github/workflows/` | Generated GitHub Actions workflows |

The library's four public types are:

- `ServiceCollectionExtensions`: shared-instance `AddSingleton`, `AddScoped`, and
  `AddHostedService` overloads.
- `Control.IHostControl`: `IHostApplicationLifetime` plus
  `StopApplication(int exitCode = 0)`.
- `Control.HostControlHostExtensions`: the `AddHostControl()` registration extension.
- `Service.CycleBackgroundService`: fixed-cadence `BackgroundService` base class.

`HostControl` itself is internal. Consumers depend on `IHostControl`, not the implementation.

## Build, test, and documentation

The repository requires the .NET 10 SDK selected by `global.json`. From the repository root:

```powershell
dotnet build Invex.Extensions.Hosting.slnx
dotnet test Invex.Extensions.Hosting.slnx
docfx docfx.json
```

The test project targets `net10.0`, `net9.0`, `net8.0`, and `net48`. The library targets
`net10.0`, `net9.0`, `net8.0`, and `netstandard2.0`; code must compile for all four library
targets. Use `#if NET8_0_OR_GREATER` for APIs unavailable on older targets, following the existing
pattern in `ServiceCollectionExtensions.cs`.

After code changes, run ReSharper code cleanup over the solution's C# files. Point `--toolset-path`
at the SDK's `MSBuild.dll`; this avoids the VS BuildTools MSBuild
`MSB4236 Microsoft.NET.SDK.WorkloadAutoImportPropsLocator` error:

```powershell
$sdk = dotnet --version
jb cleanupcode Invex.Extensions.Hosting.slnx --include="**.cs" `
  --toolset-path="C:\Program Files\dotnet\sdk\$sdk\MSBuild.dll"
```

If `jb` is unavailable, install it with:

```powershell
dotnet tool install -g JetBrains.ReSharper.GlobalTools
```

Cleanup honors `.editorconfig` and team-shared `*.DotSettings` automatically.

## Code conventions

- `ImplicitUsings`, `Nullable`, and `TreatWarningsAsErrors` are enabled in
  `Directory.Build.props`; keep builds warning-free.
- Shared global usings belong in the relevant project `_usings.cs`, not individual source files.
- Add complete XML documentation to every new public type and member, even though `CS1591` is
  suppressed. Keep docs exact about null behavior, return values, timing, and errors.
- Annotate every new public type with `[PublicAPI]`.
- On .NET 8 and later, annotate DI implementation type parameters with
  `DynamicallyAccessedMembers(PublicConstructors)` for trimming and AOT compatibility.
- Keep internal helpers `internal` and preserve the intentionally small public API.
- Follow existing patterns, formatting, and localization. Add comments only where behavior is not
  clear from the code.

## Behavioral contracts

Preserve these guarantees when changing the implementation:

- Multi-interface registrations create one implementation instance per lifetime, not one instance
  per service type.
- `AddHostedService` registers the implementation as a singleton, as `IHostedService`, and through
  every requested service interface, all pointing to the same instance.
- `StopApplication(exitCode)` sets `Environment.ExitCode` before invoking the lifetime's parameterless
  `StopApplication()` and does not wait for shutdown.
- `CycleBackgroundService` uses monotonic fixed scheduling. Cycles never overlap; overruns cause
  catch-up cycles; cancellation during the delay does not start another cycle.
- `CycleCadenceMs` is read once per cycle and may vary between cycles.

## Atom-generated workflows

`.github/workflows/` and `.github/dependabot.yml` are generated from `_atom/IBuild.cs`. Never
hand-edit generated files. When changing workflow triggers, targets, matrices, options, parameters,
secrets, or related Atom definitions, regenerate them:

```powershell
atom gen
# or:
dotnet run --project _atom -- gen
```

Commit regenerated files with the Atom change. The workflows validate and test the project on
`net8.0`, `net9.0`, `net10.0`, and `net48`; package and documentation publishing occurs in the
Build workflow. Notable Atom targets include `PackProjects`, `TestProjects`, `TestFxProjects`,
`BuildDocs`, `PublishDocs`, and `CheckPrForBreakingChanges`.

## Verify snapshots and public API

Tests use NUnit, Shouldly, FakeItEasy, and Verify.NUnit. Verify snapshots are stored as
`tests/**/*.verified.txt` and must use LF line endings. A mismatch creates a matching
`.received.txt` file:

1. Fix the implementation if the change is unintended.
2. If the change is intentional, replace the relevant `.verified.txt` with the `.received.txt`
   content and delete the `.received.txt`.
3. Run `dotnet test Invex.Extensions.Hosting.slnx` again.

`PublicApiTests.VerifyPublicApiSurface.verified.txt` records the complete public API. Treat any
change to it as an intentional API change and review it for compatibility; the PR validation
workflow checks verified-file changes for breaking changes.

## Versioning and change checklist

Use Conventional Commits. GitVersion derives version increments from these prefixes:

| Prefix | Version bump |
|---|---|
| `breaking:`, `major:` | Major |
| `feat:`, `feature:`, `minor:` | Minor |
| `fix:`, `patch:` | Patch |
| `semver-none`, `semver-skip` | No bump |

For a code change:

1. Follow the existing implementation and test patterns.
2. Add XML docs and `[PublicAPI]` for new public API.
3. Add or update focused tests, including snapshots when the public surface changes.
4. Update the relevant `README.md`, `docs/`, and `api/` pages for user-facing behavior.
5. Run build, tests, and `jb cleanupcode`.
6. Run `atom gen` when the Atom build definition affects generated files.

Do not add planning or tracking markdown files to the repository. Do not revert unrelated
worktree changes.
