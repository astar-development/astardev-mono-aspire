# Conflict evidence preview

Source: [feature-ideas.md](../feature-ideas.md), item #6 (Tier 1). Value M,
effort S. Status: Draft. Date: 2026-11-18. Pairs with [05
conflict-auto-close](<05 conflict-auto-close.md>).

## Problem

The conflict list shows a path and a kind, but not what differs. The user must
leave the app to compare sizes and dates and to open both files before choosing
a strategy.

## Goals

- See local vs cloud size, modified time and whether the content matches,
  without leaving the list.
- Open the local copy and the cloud copy to inspect them.

## Non-goals

- A content diff or side-by-side file viewer.
- Downloading cloud copies just to preview them.
- Changing strategies or auto-resolving (see #5).

## Current state

- `ConflictItemViewModel` shows `PathText`, `KindText`, `DetectedText` and
  strategy options; no evidence.
- `ConflictEvidence` / `SideSnapshot.Present` hold size, last modified, hash and
  tag at detection time; the stored conflict record is not known to keep them.
- The conflict list is one shared view model for all layouts.
- No provider API returns a web link for an item; none exists today.

## Requirements

1. Must: each conflict row expands to show, for local and cloud: size, last
   modified and whether each exists.
1. Must: the expanded row shows "same content" / "different" / "unknown" from
   the hashes, using provider-comparable hashes.
1. Must: a button opens the local file in the OS default application; it is
   disabled with a reason when the local file is absent.
1. Must: a button opens the cloud copy in the provider's web page in the
   browser; disabled with a reason when the item is gone or the provider offers
   no link.
1. Must: a provider-neutral "web link for item" capability is added to the
   provider seam; OneDrive implements it.
1. Must: evidence is stored with, or refreshed for, the conflict so it can be
   shown after restart.
1. Must: all text from `en-GB.json` keys (and Spanish when #4 ships).
1. Must: works in Layouts A, B and C through the shared conflict list.
1. Should: sizes and dates format for the UI culture.

## Acceptance criteria

- Given a BothModified conflict, expanding shows both sizes and times and
  "different".
- Given identical hashes (not auto-closed for any reason), it shows "same
  content".
- Given the local file deleted, "Open local" is disabled with a localized
  reason.
- Given a cloud item with no web link, "Open in cloud" is disabled with a
  localized reason.
- Given an app restart, the evidence is still shown.
- View-model tests (`<Subject>Should`, snake_case) cover each state; the same
  list renders in A, B and C.

## Constraints and decisions

Owner (feature-ideas): size, mtime, hash match, open-both buttons; all layouts
must get it; more providers will be added.

Interview 2026-11-18:

- Evidence is an expandable detail on each conflict row.
- Cloud copy opens via the provider's web page, not a download.

Code: the web link must go through the provider seam so Google Drive can
implement it later.

## Risks

- Stored conflicts may not hold evidence; a schema change is possible.
- Evidence can be stale if files change after detection; show the detection
  time.
- Web links need the user signed in at the provider in their browser.

## Open questions

None outstanding.
