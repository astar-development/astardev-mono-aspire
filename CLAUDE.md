# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

.NET 11 / .NET Aspire 17.0 solution (`AStarDev.slnx`). Aspire AppHost orchestrates the Blazor Server sites (`AStarDev.Web`, `Fab4Kids.Web`) and the CloudSync desktop app.

## Commands

```bash
dotnet build AStarDev.slnx
dotnet run --project apps/Aspire/AStarDev.AppHost  # or `aspire run` (see aspire.config.json); starts web sites + CloudSync + dashboard
dotnet test AStarDev.slnx                          # all tests
dotnet test AStarDev.slnx --filter "FullyQualifiedName!~Integration"   # unit only (what CI runs)
dotnet test tests/web/AStarDev.Web.TestsUnit --filter "FullyQualifiedName~<Subject>Should.<method>"   # single test
```

Integration tests boot Aspire app via `DistributedApplicationTestingBuilder`, so need container runtime/Aspire prerequisites, slower. CI (`.github/workflows/main.yml`) excludes `Integration` and `EndToEnd` from `dotnet test`.

## Architecture

- `apps/Aspire/AStarDev.AppHost` — Aspire orchestrator (`Aspire.AppHost.Sdk/17.0.0`, obsolete-SDK warning ASPIRE012 suppressed). Registers `AStarDev-Web`, `Fab4Kids-Web`, `CloudSync` via `AddDotnetProject`; project paths resolve from `AppHost.cs`'s own location (`[CallerFilePath]`) because relative paths otherwise follow the cwd. Web sites expose `/health`.
- `apps/web/{AStarDev.Web,Fab4Kids}` — Blazor Server sites.
- `apps/desktop/AStarDev.CloudSync` — desktop app; AppHost maps its OTel config keys.
- `apps/Aspire/AStarDev.ServiceDefaults` — shared OpenTelemetry, health checks, service discovery, resilience (`AddServiceDefaults` / `MapDefaultEndpoints`). Every web service must call both.
- `tests/web`, `tests/desktop` — `*.TestsUnit` and `*.TestsIntegration` per app. Stack: xUnit, Shouldly, Testably.Abstractions.Testing; integration adds `Aspire.Hosting.Testing`.

## Conventions

- Test classes named `<Subject>Should`, methods snake_case (e.g. `WebRootShould.return_ok_status_code`); IDE1006 suppressed in integration projects.
- `.github/` holds community files; `_repo-setup-scripts/configure-gh-repo.sh` configures GitHub repo.
