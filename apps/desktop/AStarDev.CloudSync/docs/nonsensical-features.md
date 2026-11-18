# CloudSync: things that make no sense

Read-only review. Paths are relative to `apps/desktop/AStarDev.CloudSync/`. Line numbers from the working tree on 2026-11-18.
Not exhaustive: static grep/read only, nothing was run.

## High

| # | Finding | Evidence | Why it makes no sense | Suggested action |
|---|---------|----------|-----------------------|------------------|
| 1 | Classification page exists only in Layout A | `AStarDev.CloudSync.ViewModels/Layouts/LayoutAViewModel.cs:34,61,82`; zero "Classification" hits in `LayoutBViewModel.cs`, `LayoutCViewModel.cs`, `Layouts/LayoutBView.axaml`, `LayoutCView.axaml` | A user who picks "Grouped by provider" or "Sync pairs first" in settings silently loses terms, suggestions and Reclassify. Layout choice (cosmetic) changes feature set. Also the `RerunRecommended` banner (`en-GB.json:241`) can never be seen. | DECIDED: classification = search and browse, all layouts stay. This is a bug: move classification out of Layout A into every layout. |
| 2 | Classification has no user-visible outcome | Links written by `ClassifyingSyncBaselineWriter.cs`, `ClassifyingUploadRecorder.cs`, `ClassificationReclassifier.cs`. Only reader in UI is the "Files" count column (`ClassificationTermListViewModel.cs`). `GetFilesForTermAsync`, `GetTermsForFileAsync`, `AssignUserLinkAsync` (`Classification/IFileClassificationStore.cs:28-37`) are called only from tests. | Whole subsystem (2 MB `Seed/place.json`, 8 seed files, DB migrations, Reclassify run state, suggestions) tags files that nothing can search, filter, browse or act on. Search page (`ViewModels/Search`) does not use tags. | DECIDED: wire tags into search and browse (`ViewModels/Search`). |
| 3 | "Added by you" source / user links cannot happen | `en-GB.json:202` `Classification.Terms.Source.User`; `TermSourceFactory.CreateUser()` only used in `ClassificationSuggestions.cs` and tests; no UI to add a term or assign a link (`AssignUserLinkAsync` test-only) | Labels and store API describe user-authored terms/links, no path creates them (except accepting a suggestion, if that is `User`; verify). Doc comment "Links the user assigned stay" guards a state that is unreachable. | Confirm; delete user-link API or add the UI. |
| 4 | Three user-selectable main layouts that are design experiments | `DisplaySettings/MainLayout.cs`, `ViewModels/DisplaySettingsViewModel.cs:30-34` ("Layouts A, B and C"), `ViewModels/MainWindowLayouts.cs`, `MainWindowViewModel.cs:30-32,91-93`, `AccountViewModel.cs:96-102` (`LayoutA/B/CFailureText`) | Three parallel UIs (~1.2k lines axaml, ~650 lines VMs, 3 snapshot suites) for one data set, with feature drift (finding 1). View model names carry layout letters (`LayoutAAccountCardViewModel`), the mono-letter names mean nothing. All three VMs are always constructed and injected (`MainWindowLayouts`) even when only one is shown. Layout A hosts `MainWindow.axaml:34` error text for all layouts via `LayoutA.Overview`. | DECIDED: all layouts stay. Remaining: remove feature drift (finding 1), share failure text (6), name layouts by purpose, construct lazily. |

## Medium

| # | Finding | Evidence | Why it makes no sense | Suggested action |
|---|---------|----------|-----------------------|------------------|
| 5 | ~~Update UI strings with no updater~~ RETRACTED | Owner confirmed auto-update exists, with deferral. Updater is Velopack (`AStarDev.CloudSync/Updates/CloudSyncUpdateApplyGate.cs`, `Autostart/VelopackAutostartLauncher.cs`). Original grep missed it. | Not an issue. `Update.*` keys are live; verify they are referenced by the update UI. | None. |
| 6 | Failure text triplicated per layout | `StatusViewModel.cs:47` `FailureMessage(string layout)` builds `Failure.{A\|B\|C}.{Reason}` keys; `en-GB.json:29-40`; `AccountViewModel.cs:96-102` | Same 4 failure reasons, 12 strings, three wordings. Layout C wording ("Check the log", "Free cloud space") is a button-like label, not a message. Layout letter is a string passed into localisation key construction. Also `Status.Failed.*` (`:25-28`) is a fourth wording of the same reasons. | One message per reason; layouts differ in presentation only. |
| 7 | Misleading label "Change local Sync folder" (uncommitted edit) | `git diff` of `Assets/Localization/en-GB.json`: `Account.SyncFolder` and `AccountDialog.SyncFolder.Title` both renamed. Used: `AccountViewModel.cs:126` (button), `AccountDialogViewModel.cs:311` (dialog title) | Dialog title is also shown when nothing is set yet (`AccountDialog.SyncFolder.None`, "No sync folder has been set yet.") where "Change" is wrong. Capital "S" inconsistent with other labels. | Use "Sync folder" for the title, "Change sync folder" for the button. |
| 8 | Locale plumbing for a locale that does not exist | `DisplaySettings/DisplayPreferences.cs:17-18` ("Only en-GB exists today"), `DisplayPreferencesFactory.cs:9-12` `SupportedLocales = [DefaultLocale]`; `DisplaySettingsViewModel.cs:41` | Stored, validated and round-tripped setting that can only ever be one value; `SelectLayout`/`SelectTheme` re-pass it each save. | Not an issue: Spanish is planned as second locale. Add the UI picker with it. |
| 9 | "Providers" tile counts one provider | `SyncOverviewViewModel.cs:135` `Stats.Providers`; only OneDrive exists (`Provider.OneDrive.Code`, `Provider.Unknown.Code`) | A permanently "1 provider" statistic; "Grouped by provider" layout groups one group. `Provider.Unknown.Code` has no code path that creates an unknown provider (verify). | More providers planned: keep, or hide until a second one lands. |

## Low (developer-facing)

| # | Finding | Evidence | Suggested action |
|---|---------|----------|------------------|
| 10 | Fixed-count "pass-through" decorators wired by concrete type | `Overview/InstrumentedSyncOverviewSource.cs:8` takes `ThrottleHintingSyncOverviewSource`, which takes `AccountStoreSyncOverviewSource` (concrete, not `ISyncOverviewSource`) | Decorator chain is not substitutable; `NoThroughputReader.cs` is a null object for a case DI always satisfies (verify). Inline or depend on the interface. |
| 11 | Sync-pair vs folder vs selection-rule: three names for one idea | UI says "Synced folders"/"+ Add folder" (`en-GB.json:265-266`), code says `SyncPair`, `SyncPairViewModel`, `SelectionRule`, `SqliteSelectionRuleStore` | Reader cannot tell if a "pair" is a rule. Pick one term. |
| 12 | Plural key families without callers by literal name | 65 localisation keys have no literal reference (`.One/.Many`, `Days7..365`, `Direction.*.Glyph`, `Notification.*`, `Failure.*`) | Most are built dynamically (`CountKey.For`, `$"{DirectionKey}.Glyph"`), so this is not dead code, and none is confirmed dead. Add a test that every key is reachable, else rot goes unseen. |
| 13 | Project count vs content | `AStarDev.CloudSync.Overview` (726 lines, 12 files incl. 3 decorators), `AStarDev.CloudSync.DisplaySettings` (~230 lines), `Accounts` / `Accounts.OneDrive` / `Accounts.Sqlite` | Tiny assemblies each with an `*ServiceCollectionExtensions`, two-file DU+Factory+Extensions triples per enum (`AppTheme`, `AppThemeFactory`, `AppThemeExtensions`). Heavy ceremony for a two-value choice. Consider merging into fewer projects. |

## Verified not an issue
- No unreferenced public types found (only extension/DI/design-time classes had a single mention).
- `HasConflicts` pattern precedence in `StatusViewModel.cs:62` is correct (`is A or B` binds as a pattern).

## Reviewer take (second pass, 2026-11-18)
- Real bugs/gaps: #1 (feature missing from layouts B/C) and #2 (classification has no outcome). Both resolved by the decided search-and-browse work in `feature-ideas.md` #1 and #2. #3 resolves with it.
- Worth doing: #6 (one failure message per reason) and #7 (label fix, trivial).
- Churn, do only if touching the code anyway: #10 (decorator chain), #11 (naming drift), #13 (project merging). Low payoff against risk.
- #5 retracted, #8 and #9 not issues (planned work).
- Verify before acting on any finding: the agent worked from static reading only, and one finding was wrong.
