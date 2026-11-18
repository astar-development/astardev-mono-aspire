# Classification confirm and override: slices

PRD: [02
classification-confirm-override.md](<02 classification-confirm-override.md>).
Status: Draft plan. Date: 2026-11-18.

GitHub: epic #14, S1 #15, S2 #16.

## Epic

Title: Epic: Classification confirm and override.

Users confirm or correct a file's category on the file row in search results. A
correction replaces the wrong automatic category and survives reclassify runs;
every choice can be removed. Choices are keyed by account plus `RemoteItemId`,
so they survive renames and moves.

Out of scope: Creating new terms inline, bulk actions, engine changes, moving
files.

Depends on the file rows from epic #9 (slice #11).

Definition of done: all slices merged, PRD acceptance criteria pass, unit tests
green, behaviour checked in Layouts A, B and C.

## Order

| Wave | Slice | Blocked by | Parallel with |
|------|-------|------------|---------------|
| 1 | S1 Store: user links, suppression | none | none |
| 2 | S2 Row actions in shared search | S1, #11 | none |

## Slices

### S1: User links and suppression keyed by RemoteItemId

Why separate: Enabling slice for the store side: no UI. The classification
database is separate from the sync database, so the id is stored in the link
table.

PRD requirements: 2, 3 (store), 4, 6 (store).

In scope:

- Add `RemoteItemId` to the classification link table with a migration; fill it
  whenever automatic links are written (`ClassifyingSyncBaselineWriter`,
  `ClassifyingUploadRecorder`, `ClassificationReclassifier`).
- Rekey user links by account plus `RemoteItemId`. Nothing in production calls
  `AssignUserLinkAsync` today, so no user data needs migrating.
- New store methods: remove a user link, suppress an automatic link, remove a
  suppression. `ReplaceAutoLinksAsync` and the reads honour suppressions.
- Path to `RemoteItemId` lookup for a file; `FileNotFound` when the file has no
  id yet.

Out of scope: Any view model or view.

Acceptance criteria:

- Confirm stores the current category as a user link that reclassify keeps.
- Correct stores the chosen category and hides the wrong automatic link, before
  and after a reclassify.
- Removing a user choice restores automatic classification.
- A renamed or moved file keeps its choice.
- Unknown path returns `FileNotFound`; a database failure returns `Unavailable`.

Verification (tests in `...Classification.TestsUnit`,
`...Classification.Sqlite.TestsIntegration`; names proposed, write them first
and see them fail):

| Criterion | Check |
|-----------|-------|
| Confirm survives reclassify | `confirm_survives_reclassify` |
| Correct hides automatic link | `correct_suppresses_auto_link` |
| Suppression survives replace | `keep_suppression_on_replace` |
| Remove restores automatic | `remove_user_link_restores_auto` |
| Rename or move keeps choice | `keep_choice_after_move` |
| Failures typed | Tests per failure |
| Migration upgrade | Integration test |

Run the tests above, then the gate: `dotnet build AStarDev.slnx` and `dotnet
test AStarDev.slnx --filter "FullyQualifiedName!~Integration"`.

Estimate: about 20 files. Hotspots: `SqliteFileClassificationStore.cs`,
`FileLinkEntity.cs`, the three auto-link writers, migrations.

Definition of done:

- [ ] Failing tests written first, now pass.
- [ ] Build and unit gate pass; no new warnings.
- [ ] Only files this slice needs are changed.
- [ ] PR description pastes the test run output and lists criteria covered.

### S2: Confirm, correct and remove on the search file row

PRD requirements: 1, 3, 5, 7, 8, 9, 10.

In scope:

- Confirm, Correct and Remove actions on each Files row in the shared search
  results (from epic #9).
- Correct opens the existing category picker (existing categories and terms
  only, filterable).
- Rows set by the user show "Added by you" using the existing localized label;
  the row updates immediately.
- Failures (`NotFound`, `FileNotFound`, `Unavailable`) show a localized error on
  the row and leave it unchanged.
- Text in `en-GB.json`.

Out of scope: Store changes (S1).

Acceptance criteria:

- Confirm: reclassify keeps it and the row shows "Added by you".
- Correct: the file matches the new term and not the old one.
- Remove: the automatic category shows again.
- Failure: localized error, nothing stored.
- Works in Layouts A, B and C through the shared panel.

Verification (tests in `...ViewModels.TestsUnit`; names proposed, write them
first and see them fail):

| Criterion | Check |
|-----------|-------|
| Confirm and remove | `FileRowShould.confirm_and_remove` |
| Correct via picker | `correct_to_chosen_category` |
| Failure messages | Tests per failure |
| Row updates at once | `refresh_row_after_change` |
| In A, B and C | Manual, steps below |

Manual: in each layout search a classified file, confirm it, correct it to
another category, remove the choice; expect the row and search results to change
each time. Layouts B and C need epic #9 slices #12 and #13 merged; check them
then. Record the result in the PR.

Run the tests above, then the gate: `dotnet build AStarDev.slnx` and `dotnet
test AStarDev.slnx --filter "FullyQualifiedName!~Integration"`.

Estimate: about 16 files. Hotspots: `en-GB.json`, `SearchResultsViewModel.cs`,
the file row view model.

Definition of done:

- [ ] Failing tests written first, now pass.
- [ ] Build and unit gate pass; no new warnings.
- [ ] Only files this slice needs are changed.
- [ ] PR description pastes the test run output and lists criteria covered.
- [ ] Manual check done and recorded.

## Epic acceptance

Closing checklist, ticked by the author of the last slice:

- [ ] Confirm keeps the category through a reclassify. S1, S2
- [ ] Correct replaces the wrong category, before and after reclassify. S1, S2
- [ ] Removing a choice restores automatic classification. S1, S2
- [ ] A renamed or moved file keeps its choice. S1
- [ ] Rows show "Added by you" for user choices. S2
- [ ] Failures show a localized error and change nothing. S1, S2
- [ ] Works in Layouts A, B and C. S2 manual check
- [ ] All new text comes from `en-GB.json`. S2

Independent verifier prompt: "Check each item in the Epic acceptance list
against the merged code and tests. Report pass or fail per item with evidence
(file and test name, test output). Change nothing."

## Assumptions and decisions

- Choices are existing categories and terms only; undo is included; actions live
  on the search row (PRD interview).
- Link keyed by `RemoteItemId` stored in the classification link table (owner,
  2026-11-18). Nothing calls `AssignUserLinkAsync` in production, so there is no
  data to migrate.
- S2 depends on epic #9 slice #11; B and C checks wait for #12 and #13.
- File counts are estimates; I read the store interface, the entity list and the
  three auto-link callers by name only.
