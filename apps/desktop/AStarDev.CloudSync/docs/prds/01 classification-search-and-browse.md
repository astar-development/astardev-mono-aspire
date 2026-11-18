
# Classification search and browse

Source: [feature-ideas.md](../feature-ideas.md), item #1 (Tier 1). Value H,
effort M. Status: Draft. Date: 2026-11-18.

## Problem

Files are classified into categories/terms but nothing uses the result. Search
finds only accounts and folders, so users cannot find files by what they are.
Classification is also reachable only from Layout A; Layouts B and C have no
entry point.

## Goals

- Find files by category or term.
- See a file's category wherever files are listed.
- Same capability in Layouts A, B and C.

## Non-goals

- Confirming or correcting a category (idea #2, paired but separate).
- Auto-organising or moving files (dropped in the idea doc: risky).
- Spanish vocabulary (idea #4); design must not block per-language terms.
- Changing classification engine or seeds.

## Current state

- `IFileClassificationStore.GetFilesForTermAsync(TermId)` and
  `GetTermsForFileAsync` exist (`SqliteFileClassificationStore`); no production
  caller of `GetFilesForTermAsync`.
- `GetFileCountsAsync` used only by `ClassificationTermListViewModel`.
- `ClassifiableFile` holds only `AccountId` + `RelativeSyncPath`; no name, size,
  category.
- `SearchResultsViewModel` matches accounts and sync pairs only
  (`MatchesSearch`); hosted in `LayoutAViewModel.Search` /
  `SearchResultsContent` (A only).
- Layout B: sidebar `SearchText` filter over accounts/pairs. Layout C:
  `CommandSearchBox` bound to `SearchText`. Neither references classification.
- Classification page (`ClassificationPageViewModel`, tabs for
  terms/suggestions/reclassify) wired in Layout A only.
- No file-row view model exists anywhere in the UI.

## Requirements

1. Must: search results include files, in a new section beside accounts and
   folders.
1. Must: user can filter files by category and by term.
1. Must: each file row shows category, name and size.
1. Must: a term in the Classification term list can open its matching files.
1. Must: all of the above work in Layouts A, B and C, each using its own idiom
   (A search page, B sidebar, C command box).
1. Must: category/term filters are chips; selecting a chip filters files,
   selecting it again clears it.
1. Must: Layouts B and C each get a Classification page with the same tabs as
   Layout A (reuse `ClassificationPageViewModel`).
1. Must: file rows show category, name and size; clicking a row reveals the file
   in the local folder.
1. Must: all new text comes from `Assets/Localization/en-GB.json` keys.
1. Must: file queries run off the UI thread and show loading/empty/failed
   states; a `ClassificationFailure` never crashes the page.
1. Should: free-text search matches file path as well as category/term text.
1. Should: results are bounded or paged so large libraries stay responsive.

## Acceptance criteria

- Given a classified file, when the user filters by its term, then the file
  appears with its category in A, B and C.
- Given no matches, the empty state shows localized text.
- Given the store returns `ClassificationFailure.Unavailable`, a localized error
  state appears and the rest of search still works.
- Given a blank search and no filter, accounts/folders list as today (no
  regression); file section is empty or hidden, not a full dump.
- Unit tests per layout view model (`<Subject>Should`, snake_case) cover filter,
  empty, failure.

## Constraints and decisions

- Owner: classification = search and browse; all layouts stay and every feature
  must reach A, B and C; vocabulary is per language; single device per account
  holder.
- Provider-neutral: no OneDrive-specific data in rows.
- Store lookups are by `TermId` or file today; category/path queries need new
  store methods.

## Risks

- Large baselines: unindexed term/path queries could be slow in SQLite.
- Stale results after reclassify runs; rows must refresh on completion.

## Decided (2026-11-18)

- File hits live in the existing search box, as a new Files section.
- Clicking a file reveals it in the local folder (for now).
- A file row shows category, name and size.
- Category/term filter is a row of chips under the search box.
- Layouts B and C each get their own Classification page (term list,
  suggestions, reclassify), matching Layout A.

## Open questions

None outstanding.
