# Safe conflict auto-close: slices

PRD: [05 conflict-auto-close.md](<05 conflict-auto-close.md>). Status: Draft
plan. Date: 2026-11-18.

## Epic

GitHub: epic #1, S1 #2, S2 #3.

Title: Epic: Safe conflict auto-close.

Close BothModified conflicts silently when local and cloud content are
identical, and tell the user through one activity-log summary per run that
identical conflicts (BothAdded and BothModified) were closed. Differing content
is never auto-resolved.

Out of scope: any other conflict kind, changing strategies, per-file log
entries, and the evidence preview.

Related, not blocking: [06
conflict-evidence-preview.md](<06 conflict-evidence-preview.md>) may reuse the
hash comparison.

Definition of done: all slices merged, PRD acceptance criteria pass, unit tests
green, activity entry shows in Layouts A, B and C.

## Order

| Wave | Slice | Blocked by | Parallel with | Est. files |
|------|-------|------------|---------------|------------|
| 1 | S1 (#2) BothModified auto-close | none | none | ~10 |
| 2 | S2 (#3) Activity summary | S1 | none | ~8 |

Critical path: S1 then S2. Nothing runs in parallel: S2 needs the count that S1
produces, and both touch the upload and download executors.

## Slices

### S1: BothModified auto-close in the sync engine

Goal: when a BothModified conflict has equal size and the local QuickXor equals
the cloud hash, no conflict is raised and the baseline is updated as if synced.

PRD requirements: 1 (BothModified half), 2, 3, 4, 6.

In scope:

- Extend `FirstSyncLinker` (today BothAdded only) or add a sibling so
  BothModified findings are linked when sizes and hashes match. Existing
  BothAdded behaviour is unchanged.
- Update the baseline entry (new local SHA-256, remote tag) without writing,
  deleting or moving any file.
- No auto-close when the cloud item has no hash, the local file cannot be read,
  or it changed during hashing; the conflict is raised as before.
- Share the single file read for hashes (existing `HashOnceAsync`).
- Count closed items per pass and expose the count to the run (no UI).

Out of scope: the activity entry (S2).

Acceptance criteria:

- Equal size and hash on both sides, BothModified: no conflict, baseline
  updated.
- Equal size, different hash: conflict raised.
- Cloud item without a hash: conflict raised.
- Locked or changed local file: conflict raised or path retried, never closed.
- Tests follow `<Subject>Should` with snake_case names.

Estimated files: about 10 (`FirstSyncLinker`, `UploadItemExecutor`,
`DownloadExecutor`, `ConflictFinding` or related types, baseline writer usage,
pass tally, and 3 or 4 test files). Estimate from a grep, not a full read.

Conflict hotspots: `UploadItemExecutor.cs` and `DownloadExecutor.cs` (S2 also
reads the count there).

Verification (project `tests/desktop/AStarDev.CloudSync.Sync.TestsUnit`; test
names are proposed, none found for `FirstSyncLinker` today):

| Criterion | Check |
|-----------|-------|
| Equal hash closes | Test `close_both_modified_when_hash_matches` |
| Different hash raises | Test `raise_conflict_when_hash_differs` |
| No cloud hash raises | Test `raise_conflict_when_cloud_hash_missing` |
| Locked or changed file never closes | Test `not_close_when_file_locked` |
| Executors use it | Tests on `UploadItemExecutor` and `DownloadExecutor` |
| Nothing written or deleted | Test asserts no file operations |

Write the tests first and see them fail. Run: `dotnet test
tests/desktop/AStarDev.CloudSync.Sync.TestsUnit`, then the gate `dotnet build
AStarDev.slnx` and `dotnet test AStarDev.slnx --filter
"FullyQualifiedName!~Integration"`.

Definition of done:

- [ ] Failing tests written first, now pass.
- [ ] Build and the unit gate pass; `mdl` and build warnings clean.
- [ ] Only the files this slice needs are changed.
- [ ] PR description pastes the test run output and lists criteria covered.

### S2: Activity-log summary

Goal: one activity entry per run saying how many identical conflicts were
closed, none when zero.

PRD requirements: 5. Acceptance: "1 identical conflict closed" in the log.

In scope:

- New `ActivityKind` case and factory, carrying the count.
- Write the entry once per run from the count S1 exposes.
- Localized text in `en-GB.json` with plural handling, shown in the shared
  activity list so Layouts A, B and C all show it.
- View-model tests for the text and for the zero case.

Out of scope: per-file entries.

Estimated files: about 8 (`ActivityKind`, `ActivityKindFactory`, run/pass
journal writer, activity view-model mapping, `en-GB.json`, 3 test files).

Conflict hotspots: `en-GB.json` (a parallel Spanish locale PR would add a
matching `es.json` key; coordinate), `ActivityKind.cs`.

Verification (projects `...Sync.TestsUnit` and `...ViewModels.TestsUnit`; names
proposed):

| Criterion | Check |
|-----------|-------|
| "1 identical conflict closed" | Test `say_one_identical_conflict_closed` |
| Plural and zero | Tests for N > 1 and no entry when zero |
| One entry per run | Test on the pass journal writer |
| Shows in A, B, C | Manual, steps below the table |

Manual steps: run a sync with an identical conflict, open the activity list
in each layout, expect the entry in each.

Write the tests first and see them fail. Same gate commands as S1.

Definition of done: same checklist as S1, plus the manual check recorded in the
PR (layout and result for A, B and C).

## Epic acceptance

Closing checklist, ticked by the author of the last slice:

- [ ] Identical BothAdded closes silently (already true; confirm unchanged).
  Proven by S1 tests.
- [ ] Identical BothModified closes silently. S1.
- [ ] Differing content always raises the conflict. S1.
- [ ] No cloud hash, locked or changed file never closes. S1.
- [ ] No file written, deleted or moved by an auto-close. S1.
- [ ] One activity summary per run, none when zero. S2.
- [ ] Summary visible in Layouts A, B and C. S2 manual check.

Independent verifier prompt: "Check each item in the Epic acceptance list
against the merged code and tests. Report pass or fail per item with evidence
(file and test name, test output). Change nothing."

## Assumptions and decisions

- BothAdded identical-content linking already exists in `FirstSyncLinker`, so it
  is not rebuilt. It joins the summary count only.
- Two slices, sequential, chosen over one or three (owner, 2026-11-18).
- File counts are estimates from a grep of the Sync project. I did not read
  `DownloadPlanner`, the baseline writer or the activity view models in full.
- Epic links 06 as related only (no dependency).
