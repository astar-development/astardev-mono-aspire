# Safe conflict auto-close

Source: [feature-ideas.md](../feature-ideas.md), item #5 (Tier 1). Value M,
effort S. Status: Draft. Date: 2026-11-18.

## Problem

When both sides end up with identical content, a conflict is still raised and
the user must resolve something that loses nothing either way.

## Goals

- Conflicts whose local and cloud content are identical close without user
  action.
- The user can still see that it happened.

## Non-goals

- Auto-resolving BothModified when content differs (data-loss risk; stays with
  the user, per the idea doc).
- Any other conflict kind (delete vs modify, move target taken, precondition
  failed).
- Changing conflict strategies.

## Current state

- `ConflictClassifier.Classify(ConflictEvidence)` decides the kind. BothAdded:
  both sides present, no baseline. BothModified: local hash differs from
  `ConflictBaseline.LocalHash` and remote tag differs from `RemoteTag`.
- `SideSnapshot.Present` carries size, last modified, optional `ContentHash`,
  optional `ContentTag`.
- Local hash is SHA-256 (`IContentHasher`, baseline keeps it); the cloud hash is
  the provider's (OneDrive QuickXorHash). They cannot be compared directly.
- `QuickXorHash` (internal) already computes the local QuickXor in
  `FirstSyncLinker`, which links identical both-added files today by size +
  QuickXor. A cloud item without a hash is never linked.

## Requirements

1. Must: for BothAdded and BothModified, when sizes match and the
   provider-comparable hash of the local file equals the cloud hash, no conflict
   is raised; the baseline is updated as if synced.
1. Must: no auto-close when the cloud item has no hash, the local file cannot be
   read, or it changed during hashing; the conflict is raised as today.
1. Must: differing content always raises the conflict, never auto-resolved.
1. Must: no file is written, deleted or moved by an auto-close.
1. Must: each run records one activity-log summary with the count of auto-closed
   items (none when zero).
1. Should: the hash is computed once per path per run and shared with
   FirstSyncLinker's existing read.

## Acceptance criteria

- Given a file added on both sides with equal size and hash, no conflict appears
  and the activity log shows "1 identical conflict closed".
- Given BothModified with identical content on both sides, same result.
- Given equal size but different hash, the BothModified conflict is raised.
- Given a cloud item with no hash, the conflict is raised.
- Given a file locked locally, the conflict is raised or the path retried, never
  closed.
- Unit tests on the classifier/planner (`<Subject>Should`, snake_case) cover
  each case above; no UI change in A, B or C beyond the new activity entry.

## Constraints and decisions

Owner (feature-ideas): do NOT auto-resolve BothModified with differing content;
hash match only; all layouts show the activity log.

Interview 2026-11-18:

- Applies to BothAdded and BothModified.
- Visibility: one activity-log summary per run, no per-file entries.

Code: compare provider-comparable hashes (OneDrive: QuickXor), keep the provider
seam neutral so Google Drive can supply its own hash.

## Risks

- Hashing large files twice costs I/O; share the read (requirement 6).
- Hash equality is trusted; QuickXor collisions are theoretically possible, so
  size must match as well.
- Provider with no content hash never auto-closes (accepted).

## Open questions

None outstanding.
