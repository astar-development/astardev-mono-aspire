# Safe conflict auto-close: slices

PRD: [05 conflict-auto-close.md](<05 conflict-auto-close.md>). Status: Draft
plan. Date: 2026-11-18.

## Epic

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
| 1 | S1 BothModified auto-close | none | none | ~10 |
| 2 | S2 Activity summary | S1 | none | ~8 |

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

## Assumptions and decisions

- BothAdded identical-content linking already exists in `FirstSyncLinker`, so it
  is not rebuilt. It joins the summary count only.
- Two slices, sequential, chosen over one or three (owner, 2026-11-18).
- File counts are estimates from a grep of the Sync project. I did not read
  `DownloadPlanner`, the baseline writer or the activity view models in full.
- Epic links 06 as related only (no dependency).
