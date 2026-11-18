# Classification search and browse: slices

PRD: [01
classification-search-and-browse.md](<01 classification-search-and-browse.md>).
Status: Draft plan. Date: 2026-11-18.

## Epic

GitHub: epic #9, S1 #10, S2 #11, S3 #12, S4 #13.

Title: Epic: Classification search and browse.

Users find files by category or term and see each file's categories, in all
three layouts. Files appear as a new section of the shared search results, with
category and term chips, a "Show more" button after 200 results, and "reveal in
folder" on click. Layouts B and C also get a Classification page.

Out of scope: confirming or correcting categories (PRD 02), moving files,
Spanish vocabulary, engine or seed changes.

Definition of done: all slices merged, PRD acceptance criteria pass, unit tests
green, behaviour checked by hand in Layouts A, B and C.

## Order

| Wave | Slice | Blocked by | Parallel with | Est. files |
|------|-------|------------|---------------|------------|
| 1 | S1 File query and row model | none | none | ~14 |
| 2 | S2 Shared Files search, Layout A | S1 | none | ~20 |
| 3 | S3 Layout B | S2 | S4 | ~16 |
| 3 | S4 Layout C | S2 | S3 | ~16 |

Critical path: S1, S2, then S3 or S4 (whichever is longer). S3 and S4 run in
parallel: each edits only its own layout view model and view.

## Slices

### S1: File query and row model

Why a separate first slice: today the store returns only account and path
(`ClassifiableFile`), with no name, size or category. S2 needs a file row query.
It is an enabling slice: new read side only, no UI, no behaviour change.

PRD requirements: 11, 12 (store side), 10 (typed failures).

In scope:

- A file search read model: account, path, file name, size, and the distinct
  category names of its terms.
- A query by free text (path, term text, category name), term ids and category
  ids, with offset and limit; a bounded page plus a "has more" flag.
- Typed failure results (`ClassificationFailure`), never exceptions.
- Sqlite implementation joining the classification links to the sync baseline
  for size, with indexes if needed (migration only if required).
- DI registration.

Out of scope: any view model or view.

Acceptance criteria: a classified file is returned with name, size and
categories; filters by term and by category narrow results; limit and offset
page results with a correct has-more flag; a database failure returns
`Unavailable`.

Verification (`...Classification.TestsUnit` and
`...Classification.Sqlite.TestsIntegration`; names proposed):

| Criterion | Check |
|-----------|-------|
| Row has name, size, categories | `FileSearchShould.return_row_fields` |
| Filter by term and category | Tests per filter and combined |
| Free text matches path | `match_free_text_in_path` |
| Paging and has-more | `page_results_and_flag_more` |
| Failure is typed | `return_unavailable_on_failure` |
| Real Sqlite query | Integration test on seeded data |

Write the tests first and see them fail. Run `dotnet test
tests/desktop/AStarDev.CloudSync.Classification.TestsUnit`, then the gate
`dotnet build AStarDev.slnx` and `dotnet test AStarDev.slnx --filter
"FullyQualifiedName!~Integration"`. Run the Sqlite integration tests locally.

Estimated files: about 14 (read model and query types 4, interface, Sqlite
implementation, DI, possible migration three files, tests 4). I did not read
`SqliteFileClassificationStore` or the baseline schema, so the join and the file
count are guesses. Hotspots: `SqliteFileClassificationStore.cs`, DI
registration.

Definition of done:

- [ ] Failing tests written first, now pass.
- [ ] Build and unit gate pass; Sqlite integration tests pass locally.
- [ ] Only files this slice needs are changed.
- [ ] PR description pastes the test output and lists criteria covered.

### S2: Shared Files search, wired into Layout A

PRD requirements: 1, 2, 3, 4, 6, 8, 9, 10, 12 (UI), in Layout A.

In scope:

- Extend `SearchResultsViewModel` and `SearchResultsContent` with a Files
  section: rows showing name, size and categories.
- Category and term chips under the search box; click selects, click again
  clears; they filter the Files query.
- Loading, empty and failed states; queries run off the UI thread; a failure
  never crashes the page.
- First 200 results with a "Show more" button.
- Click a row to reveal the file in the local folder (OS service: file manager
  on Linux, Windows and macOS).
- "Open files" on a term in the Classification term list: switches Layout A to
  Search with that term's chip selected.
- Blank search and no chip: accounts and folders as today, Files section hidden.
- Text in `en-GB.json`; Layout A wiring.

Out of scope: Layouts B and C, Classification pages for B and C.

Acceptance criteria: the PRD cases for Layout A: filter by term shows the file
with its category; empty and failure states are localized; blank search is
unchanged; the term list link opens the filtered Files.

Verification (`...ViewModels.TestsUnit`, `...TestsUnit`; names proposed):

| Criterion | Check |
|-----------|-------|
| Filter by term shows file | `SearchResultsShould.list_files_for_term` |
| Chip toggles on and off | `toggle_chip_filter` |
| Empty and failed states | Tests per state |
| Blank search unchanged | `not_list_files_for_blank_search` |
| Show more after 200 | `load_next_page` |
| Term list opens files | Test on the term list view model |
| Reveal in folder | Test on the reveal service (OS call faked) |
| Looks right in Layout A | Manual, steps below |

Manual: in Layout A, search a word that matches a classified file, pick a chip,
expect the row with name, size and categories; click it, expect the folder to
open; open the Classification page and use "open files" on a term.

Write the tests first and see them fail. Same gate commands as S1.

Estimated files: about 20 (search view model and view, row view model, chip view
model, reveal service and per-OS implementation 3, term list view model and
view, Layout A view model, `en-GB.json`, DI, tests 6). If it reaches about 25,
split the reveal service into its own small slice before S2. Hotspots:
`en-GB.json`, `SearchResultsViewModel.cs`, DI registration.

Definition of done:

- [ ] Failing tests written first, now pass.
- [ ] Build and unit gate pass; no new warnings.
- [ ] Manual check done in Layout A and recorded.
- [ ] Only files this slice needs are changed.
- [ ] PR description pastes the test output and lists criteria covered.

### S3: Layout B

PRD requirements: 5 and 7 for Layout B.

In scope:

- While the sidebar search box has text, show the shared `SearchResultsContent`
  in B's main area; its text and the shared view model's text stay in sync.
- A Classification page in B (reuse `ClassificationPageViewModel` and
  `ClassificationPageContent`) with a navigation entry in B's idiom.
- Text for the new navigation entry in `en-GB.json`.

Out of scope: any change to the shared search view model (S2).

Acceptance criteria: in B, search shows accounts, folders and Files with chips;
the Classification page opens with its three tabs; "open files" from the term
list works.

Verification (`...ViewModels.TestsUnit`; names proposed):

| Criterion | Check |
|-----------|-------|
| Search text drives results | `LayoutBViewModelShould` tests |
| Classification page shown | Test on page selection |
| Term list link works in B | Test |
| Looks right in Layout B | Manual, steps below |

Manual: in Layout B, type a classified word in the sidebar box, expect the
results panel with Files; open Classification from the navigation; use "open
files" on a term.

Write the tests first and see them fail. Same gate commands as S1.

Estimated files: about 16 (B view model and view, navigation entry, page
hosting, `en-GB.json`, DI or factory, tests 5 or 6). I did not read B's view or
how its pages are hosted. Hotspots: `LayoutBViewModel.cs`, `LayoutBView.axaml`,
`en-GB.json`.

Definition of done: same checklist as S2, with the manual check in Layout B.

### S4: Layout C

PRD requirements: 5 and 7 for Layout C. Same scope, acceptance and verification
as S3, for C: the command box drives the shared results panel, and a
Classification page is added with a navigation entry in C's idiom.

Estimated files: about 16. I did not read C's view or its page hosting.
Hotspots: `LayoutCViewModel.cs`, `LayoutCView.axaml`, `en-GB.json`.

Definition of done: same checklist as S2, with the manual check in Layout C.

## Epic acceptance

Closing checklist, ticked by the author of the last slice:

- [ ] A classified file appears, with its categories, when filtered by its term
  in Layouts A, B and C. S2, S3, S4
- [ ] Each file row shows category, name and size; clicking reveals the file. S2
- [ ] Filter chips toggle on and off. S2
- [ ] A term in the term list opens its matching files. S2, S3, S4
- [ ] Empty and failed states show localized text and the rest of search works.
  S2
- [ ] A blank search and no chip lists accounts and folders as before. S2
- [ ] Results stop at 200 with "Show more". S1, S2
- [ ] Layouts B and C each have a Classification page. S3, S4
- [ ] All new text comes from `en-GB.json`. S2, S3, S4

Independent verifier prompt: "Check each item in the Epic acceptance list
against the merged code and tests. Report pass or fail per item with evidence
(file and test name, test output). Change nothing."

## Assumptions and decisions

Interview 2026-11-18:

- B and C reuse the shared search results panel for Files.
- Shared core first, then one slice per layout; S3 and S4 in parallel.
- A row shows all distinct categories, shortened with an ellipsis.
- Results are bounded at 200 with a "Show more" button.
- File counts are estimates from a grep and reads of the search view model, the
  search view and `LayoutAPage`. I did not read the store implementation, B or C
  views, or how pages are hosted.
- The PRD file starts with a blank first line; harmless, not edited.
