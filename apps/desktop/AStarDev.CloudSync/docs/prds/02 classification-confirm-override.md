# Classification confirm/override

Source: [feature-ideas.md](../feature-ideas.md), item #2 (Tier 1). Value M,
effort M. Status: Draft. Date: 2026-11-18. Depends on [01
classification-search-and-browse](<01 classification-search-and-browse.md>)
(file rows).

## Problem

Automatic classification is sometimes wrong and the user cannot say so. Wrong
results stay in search and browse, and nothing the user does feeds back. The
"Added by you" label (`Classification.Terms.Source.User`) exists but no user
action produces such a link in the UI.

## Goals

- Confirm a file's category as right, or correct it to another.
- A correction survives reclassify runs.
- Undo any of the user's choices.

## Non-goals

- Creating new terms or categories inline (stays in the Classification page).
- Moving or renaming files (auto-organise was dropped).
- Bulk confirm/correct (single file per action).
- Learning from corrections to change the engine.

## Current state

- `IFileClassificationStore.AssignUserLinkAsync(accountId, path, termId)`
  exists; no production caller. Returns `NotFound`, `FileNotFound` (path not in
  baseline) or `Unavailable`.
- `ReplaceAutoLinksAsync` replaces automatic links and keeps user links.
- No store method to remove a single user link, or to suppress an automatic link
  for a file.
- Category picker UI exists in `ClassificationSuggestionsViewModel`
  (`ChooseCategoryCommand`, `CategoryFilterText`), used for promote/ignore.
- File rows with category come from the #1 PRD; they do not exist yet.

## Requirements

1. Must: each file row in search results has Confirm and Correct actions.
1. Must: Confirm stores the file's current category as a user link, so
   reclassify keeps it.
1. Must: Correct opens a category picker (existing categories only, filterable)
   and stores the chosen category as a user link.
1. Must: Correct hides the file's automatic link(s) to the replaced category,
   and they stay hidden after reclassify runs.
1. Must: a row shows when its category was set by the user ("Added by you")
   using the existing localized label.
1. Must: a row with a user choice has Remove; it deletes the user link and any
   suppression, returning the file to automatic classification.
1. Must: Files is a section of the shared search, so A, B and C all get the
   actions.
1. Must: failures (`NotFound`, `FileNotFound`, `Unavailable`) show a localized
   error on the row and leave the row unchanged.
1. Must: all new text uses `en-GB.json` keys.
1. Should: search/filter results update immediately after a change, without
   rerunning search.

## Acceptance criteria

- Given an auto-classified file, when the user confirms, then a reclassify run
  keeps the category and the row shows "Added by you".
- Given a wrong category, when the user corrects to another, then the file
  matches the new term in search and no longer matches the old one, before and
  after a reclassify.
- Given a user choice, when the user removes it, then the file shows its
  automatic category again.
- Given a corrected file is renamed or moved in the cloud, then the correction
  still applies after the next sync and reclassify.
- Given the path left the baseline, when the user acts, then the row shows a
  localized error and nothing is stored.
- View-model unit tests (`<Subject>Should`, snake_case) cover confirm, correct,
  remove and each failure; store tests cover suppression surviving
  `ReplaceAutoLinksAsync`.

## Constraints and decisions

Owner (feature-ideas): classification = search and browse; all layouts stay and
every feature must reach A, B and C; vocabulary is per language.

Interview 2026-11-18:

- Actions live on the search-result file row.
- Correct replaces the automatic category (suppressed persistently), not added
  alongside.
- Choices are existing categories/terms only.
- Undo is included.
- User links and suppressions are keyed by `RemoteItemId` (plus account), not
  path, so they survive renames and moves.

Code: needs two new store capabilities (remove user link, suppress automatic
link), keyed by account + `RemoteItemId`, plus a persistence schema change in
`SqliteFileClassificationStore`.

## Risks

- Schema migration for suppressions in the Sqlite classification store; existing
  databases must upgrade without losing links.
- Keying by `RemoteItemId` assumes the baseline stores it per file and the
  classification store can resolve path to id; unverified. If not, fall back to
  path keying (corrections then lost on rename/move).
- Existing user links are keyed by path (`AssignUserLinkAsync`); they need
  migrating to `RemoteItemId`, and a file with no id yet cannot be corrected
  (show the `FileNotFound` error).
- Reclassify engine must honour suppressions in every code path.

## Open questions

None outstanding.
