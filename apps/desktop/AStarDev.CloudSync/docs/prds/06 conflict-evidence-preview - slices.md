# Conflict evidence preview: slices

PRD: [06 conflict-evidence-preview.md](<06 conflict-evidence-preview.md>).
Status: Draft plan. Date: 2026-11-18.

GitHub: epic #22, S1 #23, S2 #24, S3 #25.

## Epic

Title: Epic: Conflict evidence preview.

Each conflict row expands to show local vs cloud size, modified time and whether
the content is the same, with buttons to open the local file and the cloud copy
in the provider's web page.

Out of scope: A content diff, downloading cloud copies, changing strategies.

Pairs with epic #1 (conflict auto-close); reuses its QuickXor hashing.

Definition of done: all slices merged, PRD acceptance criteria pass, unit tests
green, behaviour checked in Layouts A, B and C.

## Order

| Wave | Slice | Blocked by | Parallel with |
|------|-------|------------|---------------|
| 1 | S1 Provider web link | none | S2 |
| 1 | S2 Evidence detail | none | S1 |
| 2 | S3 Open local and cloud | S1, S2 | none |

## Slices

### S1: Provider web link capability (OneDrive)

Why separate: Enabling slice for S3: a provider-neutral capability so Google
Drive can implement it later.

PRD requirements: 4 (cloud link), 5.

In scope:

- New provider-neutral capability returning a browser URL for a remote item, in
  the provider seam.
- OneDrive implementation using the Graph item's web URL.
- A typed result when the item is gone or has no link.
- Registration; no UI.

Out of scope: Any view or button.

Acceptance criteria:

- OneDrive returns a web URL for an existing item.
- A missing item or no link returns a typed failure.

Verification (tests in `...Sync.OneDrive.TestsUnit`; names proposed, write them
first and see them fail):

| Criterion | Check |
|-----------|-------|
| OneDrive returns URL | `OneDriveWebLinkShould.return_url` |
| Missing item | `return_failure_when_missing` |
| No link | `return_failure_when_no_link` |

Run the tests above, then the gate: `dotnet build AStarDev.slnx` and `dotnet
test AStarDev.slnx --filter "FullyQualifiedName!~Integration"`.

Estimate: about 10 files. Hotspots: `DeltaItemMapper.cs`, provider registration.

Definition of done:

- [ ] Failing tests written first, now pass.
- [ ] Build and unit gate pass; no new warnings.
- [ ] Only files this slice needs are changed.
- [ ] PR description pastes the test run output and lists criteria covered.

### S2: Evidence detail on the conflict row

PRD requirements: 1, 2, 6, 7, 8, 9.

In scope:

- Expandable detail on each conflict row in the shared conflict list: for local
  and cloud, presence, size and last modified, from the stored evidence
  (`SyncAttentionEntity` already keeps it).
- Same content / different / unknown: hash the local file with QuickXor when the
  row expands, in the background with "checking", skipping files above a size
  cap (about 256 MB) as unknown.
- Show the detection time so stale evidence is visible.
- Sizes and dates format for the UI culture; text in `en-GB.json`.

Out of scope: Open buttons (S3).

Acceptance criteria:

- BothModified: expanding shows both sizes and times and "different".
- Identical hashes show "same content".
- Evidence is still shown after an app restart.
- Large files show "unknown" without hashing.

Verification (tests in `...ViewModels.TestsUnit`; names proposed, write them
first and see them fail):

| Criterion | Check |
|-----------|-------|
| Fields shown | `ConflictItemShould.show_evidence` |
| Different content | `show_different_when_hash_differs` |
| Same content | `show_same_when_hash_matches` |
| Size cap | `show_unknown_above_cap` |
| Background and cancel | `hash_in_background_and_cancel` |
| In A, B and C | Manual, steps below |

Manual: create a BothModified conflict, open the conflict list in each layout,
expand the row, expect sizes, times and the content result. Record the result in
the PR.

Run the tests above, then the gate: `dotnet build AStarDev.slnx` and `dotnet
test AStarDev.slnx --filter "FullyQualifiedName!~Integration"`.

Estimate: about 18 files. Hotspots: `ConflictItemViewModel.cs`,
`ConflictListContent.axaml`, `en-GB.json`.

Definition of done:

- [ ] Failing tests written first, now pass.
- [ ] Build and unit gate pass; no new warnings.
- [ ] Only files this slice needs are changed.
- [ ] PR description pastes the test run output and lists criteria covered.
- [ ] Manual check done and recorded.

### S3: Open local and cloud copies from the row

PRD requirements: 3, 4.

In scope:

- "Open local" opens the file in the OS default application; disabled with a
  reason when the file is absent.
- "Open in cloud" opens the provider web page through the S1 capability;
  disabled with a reason when the item is gone or has no link.
- OS open service (Linux, Windows, macOS); localized text.

Out of scope: Evidence detail (S2).

Acceptance criteria:

- Local file deleted: Open local is disabled with a reason.
- Cloud item without a link: Open in cloud is disabled with a reason.
- Both buttons work when available.

Verification (tests in `...ViewModels.TestsUnit`; names proposed, write them
first and see them fail):

| Criterion | Check |
|-----------|-------|
| Local absent disabled | `disable_open_local_when_absent` |
| No cloud link disabled | `disable_open_cloud_without_link` |
| Open calls service | Tests with the OS call faked |
| In A, B and C | Manual, steps below |

Manual: expand a conflict row in each layout, click both buttons, expect the
file and the web page to open. Record the result in the PR.

Run the tests above, then the gate: `dotnet build AStarDev.slnx` and `dotnet
test AStarDev.slnx --filter "FullyQualifiedName!~Integration"`.

Estimate: about 12 files. Hotspots: `ConflictItemViewModel.cs`, `en-GB.json`.

Definition of done:

- [ ] Failing tests written first, now pass.
- [ ] Build and unit gate pass; no new warnings.
- [ ] Only files this slice needs are changed.
- [ ] PR description pastes the test run output and lists criteria covered.
- [ ] Manual check done and recorded.

## Epic acceptance

Closing checklist, ticked by the author of the last slice:

- [ ] Expanding a conflict shows both sizes and times and the content result. S2
- [ ] Same content shows when hashes match. S2
- [ ] Evidence is still shown after restart. S2
- [ ] Open local is disabled with a reason when the file is absent. S3
- [ ] Open in cloud is disabled with a reason when there is no link. S1, S3
- [ ] Works in Layouts A, B and C. S2, S3 manual checks

Independent verifier prompt: "Check each item in the Epic acceptance list
against the merged code and tests. Report pass or fail per item with evidence
(file and test name, test output). Change nothing."

## Assumptions and decisions

- Evidence is an expandable row detail; cloud copy opens in the provider web
  page; the local QuickXor is computed on expand in the background with a size
  cap (owner, 2026-11-18).
- Stored conflict evidence already exists (`SyncAttentionEntity`), so no schema
  change is expected.
- S1 and S2 start together; S3 needs both.
- File counts are estimates.
