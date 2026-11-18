# CloudSync feature ideas

Survey 2026-11-18, second pass same day after owner decisions. Value H/M/L, effort S/M/L. Effort is guessed from file layout, not code depth.

## What exists today

- OneDrive only (Graph + MSAL): accounts, delta change feed, resumable fragment upload, quota refresh, sign-in/reauth. Provider seam exists (`IProviderCapability<T>`, `KeyedProviderCapability`, `CloudProvider`), only `Provider.OneDrive` registered.
- Sync (`AStarDev.CloudSync.Sync`): per-account lock, baseline + remote index in SQLite, planners, selective sync rules (`SelectionRule*`, `FolderSelection`), schedule (every N min or manual), pause per account, mass-deletion guard + held deletions, local trash, throttle gate, transfer concurrency, interrupted-run recovery, network monitor, disk-space probe.
- Conflicts: strategies `LocalWins|RemoteWins|KeepBoth|Skip`, bulk apply, per-kind default.
- Classification: seeded English categories/terms, reclassify runs, suggestions. Result is stored but not consumed by search or browse yet.
- UI: 3 layouts (A/B/C, all staying), overview, activity, log viewer, audit, tray + notifications, search (accounts and folders only), display settings, Velopack auto-update with deferral. Text from `Assets/Localization/en-GB.json` (single culture).

## Owner decisions (2026-11-18)

- More providers will be added.
- Classification = search and browse.
- Single device.
- Files on demand: wanted.
- Second UI language: Spanish.
- All layouts stay: every feature must reach A, B and C.
- Auto-update exists: do not propose.

## Recommended order

**Tier 1: do next (small/medium, clear value)**

| # | Idea | Grounded in | Value | Effort |
|---|------|-------------|-------|--------|
| 1 | **Classification search and browse** (decided). Filter by category/term, show category on file rows. Must land in all three layouts, which also fixes the Layout-A-only bug. Pair with #2. | `SearchResultsViewModel`, `SqliteFileClassificationStore`, `IFileClassificationStore.GetFilesForTermAsync` (exists, test-only today) | H | M |
| 2 | **Classification confirm/override**: user confirms or corrects a file's category, stored as a `User` link. Makes "Added by you" real and gives #1 a feedback loop. | `AssignUserLinkAsync` (exists, test-only), `ClassificationSuggestionsViewModel` | M | M |
| 3 | **Ignore patterns** (gitignore-style, per pair). Only fixed skips today (Hidden, Temp, OfficeLock, System, Symlink). Users cannot exclude `node_modules`, `*.iso`. | `ILocalFileFilter`, `FileSkipReason`, rule table beside `SelectionRule` | H | M |
| 4 | **Spanish locale** + culture picker + pseudo-locale test. Do before more strings pile up. Includes per-language classification seeds (Spanish word lists and categories), with terms keyed by language so a file can match in either. | `ILocalizationService`, `DisplayPreferencesFactory.SupportedLocales`, `Classification/Seed/*.json`, `EmbeddedWordListsFactory` | M | L |
| 5 | **Safe conflict auto-close**: identical hash on both sides closes silently. Do NOT auto-resolve BothModified (data-loss risk); leave that to the user. | `ConflictClassifier`, `IContentHasher`/`QuickXorHash` | M | S |
| 6 | **Conflict evidence preview**: size, mtime, hash match, open-both buttons. | `ConflictEvidence`, `ConflictItemViewModel` | M | S |

**Tier 2: worthwhile, schedule when convenient**

| # | Idea | Grounded in | Value | Effort |
|---|------|-------------|-------|--------|
| 7 | Retry/failed-files UI with reason and Retry button | `SyncAttentionItem`, `PathQuarantine*` | M | M |
| 8 | Diagnostics bundle (redacted logs, db health, last runs) | `LogViewerViewModel`, `IDatabaseHealthCheck`, `ISyncRunJournal` | M | S |
| 9 | OTel counters (bytes, files, throttles, conflicts) for the Aspire dashboard | `IWorkflowTelemetry`, AppHost OTel mapping | M | S |
| 10 | Metered/battery gate: skip scheduled passes on battery or metered links, allow manual. Matters for the laptop (Windows). Needs per-OS probes. | `INetworkMonitor`, `SystemNetworkMonitor`, `SyncScheduler` | M | S |
| 10b | Bandwidth cap (KB/s). Quiet-hours schedule only if wanted. | `TransferLimits`, `SyncScheduler` | L | M |
| 11 | Accessibility pass (keyboard conflict resolve, high-contrast theme) | `AppTheme`, Layout*View.axaml | M | M |
| 12 | Test hooks: change-feed fault injection (throttle, 410 resync), property tests for path/case rules, seed JSON duplicate check | `Sync.TestsUnit`, `Classification/Seed/*.json` | M | S |

**Tier 3: big, needs design first**

| # | Idea | Notes | Value | Effort |
|---|------|-------|-------|--------|
| 13 | **Files on demand** (wanted) | Per-OS, not one feature. Fedora (primary): no placeholder API; needs a FUSE virtual filesystem, a different architecture from a synced folder. Windows (wife): Cloud Files API. macOS (maybe later): File Provider. Design spike first, Fedora first: hydration/dehydration model, cache eviction, interaction with baseline index and delete guard, and how it coexists with the sync-folder mode on Windows/macOS. | H | L |
| 14 | **Second provider: Google Drive** (decided) | Seams exist (`IRemoteChangeFeed`, `IRemoteFileUploader/Downloader`, `IRemoteNamePolicy`, `ProviderError`). Do this before files on demand: placeholders must be provider-neutral, and a second provider shows where the seams leak. | H | L |

**Dropped or demoted**

- Classification auto-organise (old #16): removed. Search and browse was chosen, and moving user files automatically is risky. Revisit only if asked.
- Selective sync by size/age (old #17): folded into files on demand.
- Notification controls, token-cache hardening: kept as low-priority backlog, not listed above.

## Platforms and devices (owner, 2026-11-18)

- Fedora desktop (primary), Windows laptop (wife), Mac (maybe later). Each user has their own device, so "single device" means one per account holder, not one machine overall.
- Fedora comes first for anything per-OS (tray, autostart, token storage via secret service, FUSE).
- Classification vocabulary is per language (Spanish included).

- Separate cloud accounts per person: no cross-user conflict handling needed.
- Google Drive: personal Drive only (no Shared drives).
- Windows and macOS are untested. PR CI runs on Linux only; publish runs on Linux, Windows and macOS. Risk: Windows/mac breakage is found only at publish time.

## Added idea

| # | Idea | Notes | Value | Effort |
|---|------|-------|-------|--------|
| 0 | **Run unit tests on Windows (and macOS) in PR CI**. Publish builds all three OSs but PRs only test on Linux, and your wife's Windows laptop is a real user. Likely first failures: path separators/case rules, secret-service token cache vs DPAPI/Keychain, tray, autostart. Do before Google Drive and files on demand. | `.github/workflows/main.yml` matrix; start with `windows-latest`, add macOS later | H | S |

## Open questions

None outstanding.
