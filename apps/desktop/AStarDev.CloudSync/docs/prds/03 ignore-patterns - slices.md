# Ignore patterns: slices

PRD: [03 ignore-patterns.md](<03 ignore-patterns.md>). Status: Draft plan. Date:
2026-11-18.

## Epic

GitHub: epic #4, S1 #5, S2 #6, S3 #7, S4 #8.

Title: Epic: Ignore patterns.

Per sync pair, the user keeps an ordered list of gitignore-style patterns,
edited in the folder-selection dialog. A matching item is neither uploaded nor
downloaded; items already synced stay on both sides with no deletion. Matching
is case-insensitive on every OS.

Out of scope: a global list, deleting already-synced copies, size or age rules,
changing the fixed skips.

Definition of done: all slices merged, PRD acceptance criteria pass, unit tests
green, dialog works in Layouts A, B and C.

## Order

| Wave | Slice | Blocked by | Parallel with | Est. files |
|------|-------|------------|---------------|------------|
| 1 | S1 (#5) Matcher and store | none | none | ~16 |
| 2 | S2 (#6) Engine integration | S1 | S3 | ~16 |
| 2 | S3 (#7) Dialog editing | S1 | S2 | ~16 |
| 3 | S4 (#8) Tester and counts | S2, S3 | none | ~10 |

Critical path: S1, then S2 or S3 (whichever is longer), then S4. S2 and S3 run
in parallel: S2 changes the Sync project, S3 the view models and views, and
neither edits the other's files.

## Slices

### S1: Pattern matcher, store and migration

Why a separate first slice: the matcher and the store are shared by S2 and S3.
It is an enabling slice, kept minimal and with no behaviour change.

PRD requirements: 1, 2, 7 (validation in the parser).

In scope:

- Pattern parser and matcher: `*`, `?`, `**`, trailing `/` folders only, leading
  `/` anchor, `#` comments, blank lines, `!` re-include, ordered evaluation,
  case-insensitive. Invalid lines return a typed failure with the line number
  and reason.
- `IIgnorePatternStore` (read and replace the ordered list), keyed by account
  plus pair root path (a pair has no row of its own; it is the top-most Include
  rule).
- EF entity, configuration, `SyncDbContext` set, migration and snapshot.
- DI registration. Remove a pair's list when its pair goes (orphan cleanup).

Out of scope: any use of the matcher by the engine or UI.

Acceptance criteria: `*.iso`, `node_modules/`, `/build`, `*.log` with
`!keep.log` behave as the PRD says; `[` is rejected; the list round-trips and
keeps its order; an upgrade keeps existing data.

Verification (new tests in `tests/desktop/AStarDev.CloudSync.Sync.TestsUnit` and
`...Sync.Sqlite.TestsIntegration`; names proposed):

| Criterion | Check |
|-----------|-------|
| Syntax cases | Tests `IgnorePatternMatcherShould.*` per rule |
| Case-insensitive | Test `match_ignoring_case` |
| Invalid line rejected | Test `reject_unbalanced_bracket` |
| Order and round trip | Integration test on the store |
| Migration upgrade | Integration test applying the migration |

Write the tests first and see them fail. Run `dotnet test
tests/desktop/AStarDev.CloudSync.Sync.TestsUnit`, then the gate `dotnet build
AStarDev.slnx` and `dotnet test AStarDev.slnx --filter
"FullyQualifiedName!~Integration"`. Integration tests need the Aspire
prerequisites; run the Sqlite ones locally.

Estimated files: about 16 (matcher types 4, store interface and Sqlite store 3,
entity and configuration 2, migration three files, DbContext, DI, tests 4).
Hotspots: `SyncDbContext.cs`, model snapshot,
`SyncServiceCollectionExtensions.cs`.

Definition of done:

- [ ] Failing tests written first, now pass.
- [ ] Build and unit gate pass; Sqlite integration tests pass locally.
- [ ] Only files this slice needs are changed.
- [ ] PR description pastes the test output and lists criteria covered.

### S2: Engine integration, both directions

PRD requirements: 3, 4, 5.

In scope:

- Load the pair's patterns at the start of each run and pass them to the local
  scan, `UploadPlanner`, `DownloadPlanner` and `LocalDeletionFinder`.
- A matching file or folder is not uploaded, not downloaded, and not descended.
- A matching item already in the baseline stops syncing; no deletion is planned
  or held and the mass-deletion guard is not triggered; both copies stay.
- Removing a pattern lets the item sync again on the next run; no restart.

Out of scope: any UI, activity counts.

Acceptance criteria: the PRD cases for `*.iso` both ways, "synced file that now
matches is neither deleted nor re-uploaded, no held deletion", and removal
resumes sync.

Verification (`...Sync.TestsUnit`; names proposed):

| Criterion | Check |
|-----------|-------|
| Local ignored not uploaded | Test on `UploadPlanner` |
| Cloud ignored not downloaded | Test on `DownloadPlanner` |
| Synced now ignored: no delete | Test on `LocalDeletionFinder` |
| No held deletion or guard | Test on the planners and guard |
| Removal resumes | Test with the list changed between runs |

Write the tests first and see them fail. Same gate commands as S1.

Estimated files: about 16 (planners 2, walker, deletion finder, pass runner or
request types 2, ignore evaluation adapter, tests 6 or 7). Hotspots:
`UploadPlanner.cs`, `DownloadPlanner.cs`, `LocalTreeWalker.cs`. I did not read
the pass runner or the remote index; the count is a guess.

Definition of done: same checklist as S1 without the integration item.

### S3: Dialog editing and validation

PRD requirements: 6, 7 (message), 8 (text).

In scope:

- An ignore-patterns section in the folder-selection dialog: ordered list with
  add, remove and reorder, one pattern per row.
- Inline validation per line using the S1 parser, with a localized message; an
  invalid list is never saved and the stored list is unchanged.
- Save and load through `IIgnorePatternStore`.
- Localized text in `en-GB.json`.

Out of scope: tester, activity counts.

Acceptance criteria: edit, reorder, save and reload keeps the list; `[` shows a
localized error and leaves the list unchanged; works in Layouts A, B and C.

Verification (`...ViewModels.TestsUnit`; names proposed):

| Criterion | Check |
|-----------|-------|
| Add, remove, reorder | Tests on the new view model |
| Invalid line message | Test `show_error_for_invalid_pattern` |
| List unchanged on error | Test `not_save_invalid_list` |
| Dialog in A, B, C | Manual, steps below |

Manual: open the folder-selection dialog from each layout, add `*.iso`, save,
reopen, expect the list; enter `[`, expect the error.

Write the tests first and see them fail. Same gate commands as S1.

Estimated files: about 16 (new view model and row, folder-selection view model,
the dialog view, `en-GB.json`, factories, tests 6). Hotspots: `en-GB.json`,
`FolderSelectionViewModel.cs`. If the Spanish locale PR is in flight, add the
`es.json` keys there too.

Definition of done: same checklist as S1 without the integration item, plus the
manual check recorded for A, B and C.

### S4: Pattern tester and ignored counts

PRD requirements: 9 and 10 (the two Should items).

In scope:

- A tester in the dialog: type a path, see whether it would be ignored and by
  which pattern.
- Activity-log entry per run with the ignored counts, not one per item.
- Localized text.

Acceptance criteria: the tester agrees with the matcher; one activity entry per
run with the count, none when zero.

Verification (`...ViewModels.TestsUnit`, `...Sync.TestsUnit`; names proposed):

| Criterion | Check |
|-----------|-------|
| Tester result and pattern | Tests on the tester view model |
| One entry per run | Test on the journal writer |
| Zero: no entry | Test |
| Entry in A, B, C | Manual, steps below |

Manual: run a sync that ignores a file, open the activity list in each layout,
expect one entry with the count.

Write the tests first and see them fail. Same gate commands as S1.

Estimated files: about 10. Hotspots: `en-GB.json`, `ActivityKind.cs`,
`FolderSelectionViewModel.cs`. Sequence after the epic for conflict auto-close
if both are in flight, since both add an `ActivityKind`.

Definition of done: same as S3.

## Epic acceptance

Closing checklist, ticked by the author of the last slice:

- [ ] `*.iso` is not uploaded and a cloud `.iso` is not downloaded. S2
- [ ] `node_modules/` skips folders at any depth, not a file of that name. S1,
  S2
- [ ] `/build` matches only at the pair root. S1
- [ ] `*.log` with `!keep.log` syncs `keep.log`. S1
- [ ] A synced file that now matches is neither deleted nor re-uploaded; no held
  deletion. S2
- [ ] Removing the pattern syncs the item again. S2
- [ ] `[` shows a localized error and leaves the list unchanged. S3
- [ ] The dialog works in Layouts A, B and C. S3 manual check
- [ ] Tester and per-run ignored counts work. S4

Independent verifier prompt: "Check each item in the Epic acceptance list
against the merged code and tests. Report pass or fail per item with evidence
(file and test name, test output). Change nothing."

## Assumptions and decisions

Interview 2026-11-18:

- Issue 3 read as PRD 03; GitHub issue #3 is a slice of another epic.
- Patterns stored in a new table keyed by account plus pair root path.
- Four slices; S2 and S3 parallel after S1; Should items in S4.
- Matching is case-insensitive on every OS. The PRD's case-rule risk should be
  updated to say so.
- File counts are estimates from a grep and a read of the selection and filter
  types. I did not read the pass runner, remote index, watcher or dialog view.
