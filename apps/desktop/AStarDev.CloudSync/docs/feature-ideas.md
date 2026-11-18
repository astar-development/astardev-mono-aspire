# CloudSync feature ideas

Read-only survey, 2026-11-18. Value H/M/L, effort S/M/L. Ranked best first.

## What exists today

- OneDrive only (Graph + MSAL): accounts, delta change feed, resumable fragment upload, quota refresh, sign-in/reauth. Provider seam exists (`IProviderCapability<T>`, `KeyedProviderCapability`, `CloudProvider`), but only `Provider.OneDrive` is registered.
- Sync (`AStarDev.CloudSync.Sync`): per-account lock, baseline + remote index in SQLite, upload/download planners, selective sync rules (`SelectionRule*`, `FolderSelection`), schedule (`SyncSchedule` = Every N min or ManualOnly), pause per account, mass-deletion guard + held deletions, local trash with retention, throttle gate/hints, transfer concurrency per account, throughput samples, interrupted-run recovery, network monitor, disk-space probe.
- Conflicts: 5+1 kinds, strategies `LocalWins|RemoteWins|KeepBoth|Skip`, bulk apply, per-kind default, kind filter.
- Classification: seeded categories/terms (car, clothing, colour, object, place...), reclassify runs, suggestions (promote/ignore words), term lists. Standalone: nothing in sync, search or UI-browse consumes the result yet.
- UI/VMs: 3 layouts (A/B/C), overview, activity, log viewer, file audit, tray + notifications, search (accounts and folders only), display settings (theme, layout, schedule, autostart, per-account verbose logging), update prompt. All text from `Assets/Localization/en-GB.json` (single culture).
- Tests: per-project `*.TestsUnit` / `*.TestsIntegration` plus `TestHelpers.Ui`. Telemetry via `IWorkflowTelemetry`, `Instrumented*` decorators.

## Ranked ideas

| # | Idea | Grounded in | Value | Effort |
|---|------|-------------|-------|--------|
| 1 | **Ignore patterns (gitignore-style, per pair)**. Today only fixed skips (Hidden, Temporary, OfficeLock, SystemFile, SymbolicLink). Users cannot exclude `node_modules`, `*.iso`, etc. | `ILocalFileFilter`, `LocalFileFilter`, `FileSkipReason`, `SyncSkipFilter`; new rule table beside `SelectionRule` | H | M |
| 2 | **Classification-powered search and browse**: filter files by category/term, show category on file rows. Classification data already stored but not queryable from UI. | `ISearchResults`/`SearchResultsViewModel` (accounts+folders only), `SqliteFileClassificationStore`, `ClassificationCandidate` | H | M |
| 3 | **Bandwidth limit + quiet hours**. Only concurrency is configurable; schedule is interval-only. Add KB/s cap and time-window schedule (e.g. pause 09-17, or manual-only on weekdays). | `TransferLimits`, `TransferConcurrency`, `ThroughputRecorder`, `SyncSchedule`, `SyncScheduler.OnTick`, Settings.Schedule | H | M |
| 4 | **Metered / offline awareness**. `INetworkMonitor` exposes only availability; add metered-connection and battery gate (skip scheduled passes, allow manual). | `INetworkMonitor`, `NetworkAvailability`, `SystemNetworkMonitor`, `SyncScheduler` | M | S |
| 5 | **Auto-resolve conflict policy**. Per pair or kind default applied without prompting (e.g. BothAdded identical hash -> auto-close; BothModified -> KeepBoth). Hashing exists, so identical-content conflicts can self-heal. | `ConflictClassifier`, `ConflictResolutionPlanner`, `IContentHasher`/`QuickXorHash`, `ConflictBulkOptionViewModel` default-per-kind | H | M |
| 6 | **Conflict preview/diff metadata**: show size, mtime, hash match, and open-both-files buttons before choosing. | `ConflictEvidence`, `ConflictSides`, `SideSnapshot`, `ConflictItemViewModel`, ConflictListContent.axaml | M | S |
| 7 | **Second provider (Google Drive or Dropbox)**. Seams are in place: `IRemoteChangeFeed`, `IRemoteFileUploader/Downloader`, `IRemoteNamePolicy`, `ProviderError`. Biggest scope item, so ranked mid. Needs a new `Sync.<Provider>` + `Accounts.<Provider>` project pair mirroring the OneDrive ones. | all `Sync.OneDrive` / `Accounts.OneDrive` types | H | L |
| 8 | **Per-file sync status in activity/search + retry queue UI**. Show failed files with reason and a Retry button; failures are tracked as attention items but retry/backoff policy is not user-visible. | `SyncAttentionItem`, `ISyncAttentionReader`, `PathQuarantine*`, `ActivityListViewModel` | M | M |
| 9 | **Diagnostics bundle**: one-click zip of logs (redacted), db health, last N runs, throughput. Pieces exist separately. | `LogViewerViewModel`, `ILogFileLocator`, `IDatabaseHealthCheck`, `ISyncRunJournal`, `LogFileSweeper` | M | S |
| 10 | **OpenTelemetry export for sync workflows**. `IWorkflowTelemetry`/`TelemetryTags` already model workflows; AppHost already maps CloudSync OTel keys. Add counters (bytes, files, throttle events, conflicts) so Aspire dashboard shows sync health. | `WorkflowTelemetry`, `Instrumented*`, AppHost OTel mapping | M | S |
| 11 | **Notification controls**: per-account mute, digest, only-on-failure. Policy exists as an interface. | `ISyncNotificationPolicy`, `PassNotificationDecider`, `SyncNotifier` | M | S |
| 12 | **Localization beyond en-GB**: add second culture file, culture picker in display settings, pseudo-locale test to catch hard-coded strings and overflow. `ILocalizationService` already live-switchable. | `Assets/Localization/en-GB.json` (484 keys), `LocalizedObservableObject`, DisplayPreferences | M | M |
| 13 | **Accessibility pass**: automation names exist in 20 axaml files; add keyboard-only conflict resolve, high-contrast theme (`AppTheme`), focus order tests via `TestHelpers.Ui`. | `AppTheme`, `OverlayBehavior`, Layout*View.axaml | M | M |
| 14 | **Token cache hardening**: plaintext fallback needs explicit consent today; add periodic re-prompt/secret-service detection status in Settings, and a "wipe local tokens" action. | `TokenCacheInitialiser`, `IPlaintextCacheConsentStore`, `MsalCacheHelperStorage`, `MicrosoftSignOut` | M | S |
| 15 | **Classification: confidence UI + manual override**. `ClassificationConfidence` High/Low exists; let user confirm/override a file's category and feed back as terms. Add multi-word phrases/regex terms. | `ClassificationCombiner`, `ClassificationSuggestionsViewModel` (promote/ignore only), `ClassificationTokeniser` | M | M |
| 16 | **Classification-based auto-organise** (move/tag files into category folders, opt-in, dry-run first). Depends on #2 and #15. | `ClassificationReclassifier`, `ILocalFileOperations`, `IRemoteItemRenamer` | M | L |
| 17 | **Selective sync by size/age** (skip files >N MB, files older than N days, "online only" placeholder). | `SelectionRulePlanner`, `DownloadPlanner`, `FileAuditor` counts | L | M |
| 18 | **Test hooks**: property tests for `RelativeSyncPath`/`UnicodeText`/case-rule edge cases; fault-injection fake for `IRemoteChangeFeed` (throttle, 410 resync); golden tests for seed JSON (dupes across categories). | `Sync.TestsUnit`, `TestHelpers`, `Classification/Seed/*.json` | M | S |

## Suggested first slice

#1 ignore patterns, #4 metered gate, #6 conflict evidence in UI, #9 diagnostics bundle, #10 OTel counters. All small to medium, no schema surprises beyond one migration (#1).

## Not checked

Did not read every axaml, did not run the app or tests, and did not verify whether OneDrive "files on demand"/placeholders are out of scope by design. Effort numbers are guesses from file layout.

## Owner decisions (2026-11-18)
- More providers will be added: second-provider seams are in scope.
- Classification = search and browse. Rank classification-powered search/browse top; auto-organise is not wanted.
- Single device: metered/battery gate and bandwidth cap are lower priority.
- Files on demand (placeholder sync): wanted. Large effort, needs OS integration (Linux FUSE, Windows Cloud Files API); needs its own design.
- Second UI language: Spanish (es).
- All layouts (A/B/C) stay, so features must reach every layout.
- Auto-update already exists (Velopack, with deferral): do not propose.
