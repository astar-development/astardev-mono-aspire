# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

.NET 11 / .NET Aspire 13.6 template solution (`AStarDev.slnx`). Blazor Server frontend calls minimal-API backend; Aspire AppHost orchestrates both.

## Commands

```bash
dotnet build AStarDev.slnx
dotnet run --project src/AStarDev.AppHost          # or `aspire run` (see aspire.config.json); starts api + web + dashboard
dotnet test AStarDev.slnx                          # all tests
dotnet test AStarDev.slnx --filter "FullyQualifiedName!~Integration"   # unit only (what CI runs)
dotnet test tests/AStarDev.Web.TestsUnit --filter "FullyQualifiedName~WeatherApiClientShould.<method>"   # single test
```

Integration tests boot Aspire app via `DistributedApplicationTestingBuilder`, so need container runtime/Aspire prerequisites, slower. CI (`.github/workflows/main.yml`) excludes `Integration` and `EndToEnd` from `dotnet test`.

## Architecture

- `src/AStarDev.AppHost` — Aspire orchestrator. Registers `apiservice` and `webfrontend`; web references api via service discovery (`https+http://apiservice` in `AStarDev.Web/Program.cs`), waits for it. Both expose `/health`.
- `src/AStarDev.ApiService` — minimal API backend.
- `src/AStarDev.Web` — Blazor interactive-server UI; typed `HttpClient` (`WeatherApiClient`) to api.
- `src/AStarDev.ServiceDefaults` — shared OpenTelemetry, health checks, service discovery, resilience (`AddServiceDefaults` / `MapDefaultEndpoints`). Every service must call both.
- `tests/` — four projects: `{ApiService,Web}.TestsUnit` and `{ApiService,Web}.TestsIntegration`. Stack: xUnit, Shouldly, Testably.Abstractions.Testing; integration adds `Aspire.Hosting.Testing`.

## Conventions

- Test classes named `<Subject>Should`, methods snake_case (e.g. `WebRootShould.return_ok_status_code`); IDE1006 suppressed in integration projects.
- No `Directory.Build.props`/`global.json`: package versions pinned per csproj.
- `.github/` holds community files; `_repo-setup-scripts/configure-gh-repo.sh` configures GitHub repo.