# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### JavaScript / TypeScript (pnpm workspaces)

```bash
pnpm install                                        # install all workspace deps
pnpm --filter @animetracker/ci test                 # run ci-tools tests (no coverage)
pnpm --filter @animetracker/ci test:coverage        # run with v8 coverage report
pnpm --filter @animetracker/repo-checks test        # run monorepo structure checks
```

Run a single test file:
```bash
pnpm --filter @animetracker/ci exec vitest run affected.spec.ts
```

### .NET (services/core)

```bash
dotnet restore services/core/AnimeTracker.sln
dotnet build  services/core/AnimeTracker.sln --configuration Release
dotnet test   services/core/AnimeTracker.sln
dotnet test   services/core/AnimeTracker.sln --collect:"XPlat Code Coverage" --results-directory TestResults
```

Run a single test project:
```bash
dotnet test services/core/tests/Catalog/AnimeTracker.Catalog.UnitTests
```

### CI — detect affected jobs locally

```bash
CHANGED_FILES="apps/spa/src/app.ts tools/ci/affected.ts" node --experimental-strip-types tools/ci/compute-matrix.ts
```

## Architecture

### Monorepo layout

```
apps/spa/          Angular SPA (HU-00.04 — skeleton only)
apps/bff/          NestJS BFF / API Gateway (HU-00.03 — skeleton only)
apps/extension/    Browser extension auto-tracker (HU-00.05 — skeleton only)
services/core/     .NET 10 Modular Monolith — business logic
contracts/         OpenAPI contract files (API-first; HU-00.07)
infra/             Docker Compose, K8s, deploy scripts
tools/ci/          Affected-jobs helper (@animetracker/ci, Vitest)
tools/repo-checks/ Monorepo structure tests (@animetracker/repo-checks, Vitest)
docs/              Planning (ARD, PRD, épicas) and implementation stories
```

The pnpm workspace covers `apps/*` and `tools/*`. `services/core` is a standalone .NET solution and is not part of the pnpm workspace.

### .NET Modular Monolith (services/core)

Three business modules — **Catalog**, **Tracking**, **Calendar** — each split into five layers:

```
Module/
  <Module>.Domain/          Entities, ValueObjects, AggregateRoot<TId>, domain events
  <Module>.Contracts/       Public DTOs and MediatR IRequest/INotification records
  <Module>.Application/     MediatR handlers, IServiceCollection extension
  <Module>.Infrastructure/  EF Core, external adapters (empty stubs now)
  <Module>.Api/             Minimal-API registration, HealthCheck, IServiceCollection/IEndpointRouteBuilder extension
```

`SharedKernel` provides: `Entity<TId>`, `AggregateRoot<TId>` (with domain-event list), `ValueObject`, `Result<T>`, `IDomainEvent`, `IClock`.

The `Host` project (`AnimeTracker.Host`) wires everything: it calls `Add<Module>Module()` + `Map<Module>Module()` for each of the three modules. **Never add business logic to Host.**

Cross-module communication goes through `Contracts` (MediatR queries/commands), never through direct project references between modules.

Central package management: all NuGet versions live in `services/core/Directory.Packages.props`; individual `.csproj` files omit version attributes. Build settings (`TreatWarningsAsErrors=true`, `Nullable=enable`, `LangVersion=latest`) are in `Directory.Build.props` and apply to the entire solution.

Test stack: **xUnit + Shouldly** for assertions, **coverlet** for coverage, **NetArchTest.Rules** for architecture tests (`AnimeTracker.ArchitectureTests`).

### CI pipeline (.github/workflows/ci.yml)

1. `detect` — runs `node --experimental-strip-types tools/ci/compute-matrix.ts` to produce the JSON matrix of affected jobs from `CHANGED_FILES`.
2. `run-job` — matrix job that dispatches to the correct steps (core, repo-checks, bff, spa, extension, contracts) based on `matrix.job`.
3. `ci-ok` — aggregator; the **only required branch-protection check**. Fails if `detect` failed or any `run-job` was not `success|skipped`.

Coverage threshold: **80 % lines** for both .NET (Python script over `coverage.cobertura.xml`) and TypeScript (`thresholds` in `tools/ci/vitest.config.ts`).

### Branch and PR flow

PRs always follow: `feature/story-*` → **`dev`** → **`staging`** → **`main`**. Never open a story PR directly to `staging` or `main`.

## Commits

Conventional Commits are enforced by commitlint (Husky hook). Format:

```
<type>(<scope>): <lowercase description>
```

Allowed types: `feat fix docs style refactor test chore ci perf`  
Allowed scopes: `spa bff ext core contracts infra ci docs`

The subject must be lowercase (commitlint `subject-case` rule).

## Key invariants

- **`run:` steps in GitHub Actions** must use block-scalar (`run: |`) whenever the string contains `: ` (colon + space); plain scalars terminate at that sequence.
- `compute-matrix.ts` is **excluded** from Vitest coverage thresholds (it writes to `GITHUB_OUTPUT`; guard with `if (process.env.GITHUB_OUTPUT)` before any `appendFileSync` call).
- Each module's `.Api` project is the **only** dependency that `Host` takes; it must not reference `.Application` or `.Infrastructure` directly.
- `docs/implementation/sprint_status.yml` tracks story states. Valid progression: `backlog → ready-for-dev → in-progress → review → pull-request → done`.
