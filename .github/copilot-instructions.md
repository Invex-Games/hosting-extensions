# Copilot Instructions

Guidance for AI agents working in **Invex.Extensions.Hosting** — a small, focused C# library of
utilities for `Microsoft.Extensions.Hosting`: multi-interface DI registration, host control with
exit codes, and fixed-cadence background services. Keep changes focused and defer to the linked
docs for detail.

## What's in the repo

| Project | Role | Target frameworks |
|---------|------|-------------------|
| `Invex.Extensions.Hosting` | The library: `ServiceCollectionExtensions`, `IHostControl` / `HostControl`, `HostControlHostExtensions`, `CycleBackgroundService` | `net10.0;net9.0;net8.0;netstandard2.0` |
| `Invex.Extensions.Hosting.Tests` | NUnit test suite, including a public API surface snapshot test | `net10.0;net9.0;net8.0;net48` |
| `_atom` | Atom build definition (`IBuild.cs`) that generates the GitHub Actions workflows | `net10.0` |

Sources live under `src/`, tests under `tests/`, the Atom build definition under `_atom/`, and the
DocFX documentation site is configured by `docfx.json` with content in `docs/`, `api/`, `index.md`,
and `toc.yml`.

## Build & language specifics

- **.NET 10 SDK** is required (see `global.json`). The library multi-targets down to
  `netstandard2.0` (which also serves .NET Framework 4.8 consumers — the test suite runs on
  `net48` against that build) — code must compile on all four targets. Use `#if NET8_0_OR_GREATER`
  guards for modern-only APIs (e.g. `[DynamicallyAccessedMembers]`), following the existing
  pattern in `ServiceCollectionExtensions.cs`.
- C# `LangVersion` 14, `ImplicitUsings` and `Nullable` enabled, `TreatWarningsAsErrors` on.
- Global usings live in each project's `_usings.cs` — add shared usings there, not per-file.
- `GenerateDocumentationFile` is on; `CS1591` is in the repo-wide `NoWarn`, but the convention is
  that **every public type and member gets full XML doc comments anyway** — keep them accurate to
  the implementation.
- Tests reach internal types (e.g. `HostControl`) via `InternalsVisibleTo` declared in
  `src/Invex.Extensions.Hosting/_usings.cs`.

Build and test the whole solution:

```shell
dotnet build Invex.Extensions.Hosting.slnx
dotnet test Invex.Extensions.Hosting.slnx
```

Build the docs site:

```shell
docfx docfx.json          # add --serve to preview locally
```

## Architecture overview

The entire public surface is four types across three namespaces:

- **`ServiceCollectionExtensions`** (`Invex.Extensions.Hosting`) — `AddSingleton`/`AddScoped`
  overloads for 2–5 service types and `AddHostedService` overloads for 1–5 service types. Each
  registers `TImplementation` once and forwards every service type to it, so all service types
  resolve to the **same instance** (per scope, for scoped).
- **`IHostControl`** (`Invex.Extensions.Hosting.Control`) — extends `IHostApplicationLifetime`
  with `StopApplication(int exitCode = 0)`, which sets `Environment.ExitCode` then triggers a
  graceful shutdown.
- **`HostControlHostExtensions`** (`Invex.Extensions.Hosting.Control`) — `AddHostControl()`
  registers `IHostControl` as a singleton.
- **`CycleBackgroundService`** (`Invex.Extensions.Hosting.Service`) — abstract
  `BackgroundService` running `ExecuteCycleAsync` on a fixed cadence (`CycleCadenceMs`).
- **`HostControl`** is the `internal sealed` implementation of `IHostControl` — consumers depend
  on the interface, never the concrete type.

### Behavioral contracts (do not break these)

- **Shared instance guarantee**: every multi-interface registration overload must yield one
  implementation instance per lifetime — never one per service type.
- **`AddHostedService` overloads** register the implementation as a singleton *and* as the
  `IHostedService`; the running hosted service must remain resolvable via its service interfaces.
- **`StopApplication(exitCode)`** sets `Environment.ExitCode` *before* calling the lifetime's
  `StopApplication()`, and returns immediately without waiting for shutdown.
- **`CycleBackgroundService` cadence is anchored to a fixed schedule** (monotonic `Stopwatch`
  timestamps), not measured from cycle end. Overruns trigger back-to-back catch-up cycles.
  Cycles never overlap. Cancellation during the delay exits promptly without running another cycle.
- `CycleCadenceMs` is read once per cycle, so derived classes may vary it between cycles.

## Key design rules

- Keep the public surface minimal — four public types is a feature. Push back on scope creep.
- Annotate every new public type with `[PublicAPI]` (JetBrains.Annotations).
- On .NET 8+, annotate DI implementation type parameters with
  `[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]` for
  trimming/AOT compatibility.
- New registration overloads must follow the existing forwarding pattern exactly
  (`AddX<TImplementation>()` + factory forwards per service type).

## Atom workflows

The GitHub Actions workflow YAML under `.github/workflows/` (`Validate.yml`, `Build.yml`,
`Dependabot Enable auto-merge.yml`, `Cleanup Prereleases.yml`) is **generated** from the Atom
build definition in `_atom/IBuild.cs`.

Whenever you change anything that affects the workflows — targets, workflow definitions, triggers,
options, or params/secrets — regenerate the YAML:

```shell
atom gen
```

(equivalently `dotnet run --project _atom -- gen`). Commit the regenerated `.github/workflows/`
files alongside your `_atom/` changes; never hand-edit the generated YAML.

A drift between `_atom/IBuild.cs` and the committed YAML should be treated as a missing
`atom gen` run.

Notable targets: `TestProjects` runs the test matrix over `net8.0`/`net9.0`/`net10.0` on
Ubuntu and Windows; `TestFxProjects` runs the test suite on `net48` (Windows only);
`BuildDocs`/`ServeDocs`/`PublishDocs` handle the DocFX site (published to GitHub Pages on stable
releases); `PushToNuget` publishes the package; `CheckPrForBreakingChanges` inspects
`tests/**/*.verified.txt` changes on PRs.

## Conventions

- Add XML doc comments to all public types and members. Match the existing `<summary>` /
  `<param>` / `<returns>` / `<remarks>` / `<example>` style, and keep docs **accurate to the
  implementation** (e.g. exact scheduling and exit-code semantics).
- Use Conventional Commits — the prefix drives versioning (GitVersion, configured in
  `GitVersion.yml`):

  | Prefix | Version bump |
  |--------|--------------|
  | `breaking:` / `major:` | Major |
  | `feat:` / `feature:` / `minor:` | Minor |
  | `fix:` / `patch:` | Patch |
  | `semver-none` / `semver-skip` | No bump |

- When adding user-facing features, update the relevant `docs/` page, the matching `api/` page,
  and `README.md`. Keep the `docs/`, `api/`, and xmldoc content consistent with each other.
- When adding a new docs or API page, add it to the corresponding `toc.yml` (`docs/toc.yml` or
  `api/toc.yml`) so it appears in the DocFX left panel.
- The README is packed into the NuGet package (`Directory.Build.props` packs `readme.md`) — be
  mindful that repo-relative links (e.g. `docs/index.md`, `LICENSE.txt`) won't resolve on
  nuget.org.

## Testing & the Verify workflow

- Tests use **NUnit** with **Shouldly**, **FakeItEasy**, and **Verify** (`Verify.NUnit`) for
  snapshot/approval testing.
- A snapshot test fails when its output differs from the committed `*.verified.txt`. On failure,
  Verify writes a `*.received.txt` next to it.
- If the diff is unintended, fix the code. If the change is valid (expected new output), accept
  it and re-run:
  1. Overwrite the `*.verified.txt` with the contents of the matching `*.received.txt`.
  2. Delete the `*.received.txt`.
  3. Re-run `dotnet test` to confirm the suite is green.
- `PublicApiTests.VerifyPublicApiSurface.verified.txt` tracks the **complete public API**. An
  unexpected diff there signals an unintentional API change — treat it as such and double-check
  before accepting. The Validate workflow's `CheckPrForBreakingChanges` target inspects changes
  to `tests/**/*.verified.txt` on PRs, so API-surface changes must be intentional and committed.

## Adding a new registration overload

1. Follow the existing pattern in `ServiceCollectionExtensions.cs`: register `TImplementation`
   once, forward each service type via a factory, keep the `where` constraints complete, and add
   the `#if NET8_0_OR_GREATER` trimming attribute.
2. Add full XML docs matching the existing style (`<returns>` chaining wording, `<remarks>` on
   instance sharing, `<seealso>` for the lifetime).
3. Add unit tests verifying the shared-instance guarantee.
4. Update `PublicApiTests.VerifyPublicApiSurface.verified.txt` (see the Verify workflow above).
5. Document it in `docs/multi-interface-registration.md` and `api/ServiceCollectionExtensions.md`.

## Defer to the docs

For anything beyond the above, prefer these over duplicating detail:

- `README.md` — package overview and quick start.
- `docs/index.md` — documentation home and design principles.
- `docs/getting-started.md` — installation, supported frameworks, 5-minute tour.
- `docs/multi-interface-registration.md` — forwarded registrations and shared-instance semantics.
- `docs/host-control.md` — `IHostControl`, exit-code conventions, and gotchas.
- `docs/cycle-background-service.md` — scheduling semantics, overrun catch-up, error handling.
- `api/index.md` — the API reference index (one page per public type).

