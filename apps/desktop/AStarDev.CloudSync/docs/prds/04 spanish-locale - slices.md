# Spanish locale: slices

PRD: [04 spanish-locale.md](<04 spanish-locale.md>). Status: Draft plan. Date:
2026-11-18.

## Epic

Title: Epic: Spanish locale.

The UI is available in Spanish (`es`) through a language picker that applies
live, and classification matches Spanish file names. Terms and category names
carry a language; seeds exist for Spanish except place names. A pseudo-locale
test guards missing keys.

Out of scope: Other languages (but adding one must be data only), regional
variants, translating place names, right-to-left layout.

Every epic that adds `en-GB.json` keys needs matching `es.json` keys once S4
lands.

Definition of done: all slices merged, PRD acceptance criteria pass, unit tests
green, behaviour checked in Layouts A, B and C.

## Order

| Wave | Slice | Blocked by | Parallel with |
|------|-------|------------|---------------|
| 1 | S1 Scaffold, picker, live switch | none | S2 |
| 1 | S2 Classification language | none | S1 |
| 2 | S3 Spanish seeds | S2 | S4 |
| 2 | S4 Spanish UI text | S1 | S3 |

## Slices

### S1: Locale scaffold, picker, live switch and key test

PRD requirements: 2, 3, 4, 7, 9.

In scope:

- Add `es` to the supported locales; unknown saved locales fall back to English.
- `es.json` scaffold with every `en-GB.json` key (English placeholders; real
  Spanish comes in S4).
- A language picker in Display settings in all three layouts, listing languages
  by their own name; the choice is saved.
- Live switch: every view model, the tray and notifications refresh without
  restart.
- Dates, numbers and sizes format for the selected culture.
- Pseudo-locale test: fails for a missing key or mismatched placeholders in any
  locale marked complete (English now, Spanish from S4), and for hard-coded text
  in views.
- Fall back to English for a missing key and log it.

Out of scope: Real Spanish text (S4), classification (S2, S3).

Acceptance criteria:

- Choosing Spanish updates every window in A, B and C without restart.
- A missing key or placeholder mismatch fails the test.
- An unsupported saved locale uses English.

Verification (tests in `...DisplaySettings.TestsUnit`,
`...ViewModels.TestsUnit`; names proposed, write them first and see them fail):

| Criterion | Check |
|-----------|-------|
| Picker saves and loads | `DisplayPreferencesShould.*` |
| Live switch | `LocalizationShould.refresh_on_change` |
| Key and placeholder test | `LocaleFilesShould.match_keys` |
| Unsupported falls back | `fall_back_to_english` |
| Picker and refresh in A, B, C | Manual, steps below |

Manual: in each layout open Display settings, pick Spanish, expect every visible
window to update with no restart (still English placeholders until S4). Record
the result in the PR.

Run the tests above, then the gate: `dotnet build AStarDev.slnx` and `dotnet
test AStarDev.slnx --filter "FullyQualifiedName!~Integration"`.

Estimate: about 20 files. Hotspots: `DisplayPreferencesFactory.cs`,
`en-GB.json`, `es.json`, layout Display settings views.

Definition of done:

- [ ] Failing tests written first, now pass.
- [ ] Build and unit gate pass; no new warnings.
- [ ] Only files this slice needs are changed.
- [ ] PR description pastes the test run output and lists criteria covered.
- [ ] Manual check done and recorded.

### S2: Classification language: terms and category names

Why separate: Enabling slice for S3: terms and categories need a language before
Spanish seeds can load.

PRD requirements: 5 (schema and matching), 6, 8.

In scope:

- Add a language to terms and a name per language to categories, with a
  migration defaulting existing rows to English.
- Matching: a file matches terms of either language regardless of the UI
  language.
- The UI shows category names in the UI language, falling back to English.
- Existing English data and links keep working.

Out of scope: Spanish seed data (S3), UI text (S4).

Acceptance criteria:

- An existing database upgrades with all terms and links intact.
- A term in either language matches the file.
- Category names show in the UI language.

Verification (tests in `...Classification.TestsUnit`,
`...Classification.Sqlite.TestsIntegration`; names proposed, write them first
and see them fail):

| Criterion | Check |
|-----------|-------|
| Migration keeps data | Integration test |
| Match either language | `match_terms_in_any_language` |
| Name by culture | `show_category_in_ui_language` |
| English fallback | `fall_back_to_english_name` |

Run the tests above, then the gate: `dotnet build AStarDev.slnx` and `dotnet
test AStarDev.slnx --filter "FullyQualifiedName!~Integration"`.

Estimate: about 18 files. Hotspots: `SqliteTermStore.cs`,
`SqliteCategoryStore.cs`, `ClassificationDbContext.cs`, migrations.

Definition of done:

- [ ] Failing tests written first, now pass.
- [ ] Build and unit gate pass; no new warnings.
- [ ] Only files this slice needs are changed.
- [ ] PR description pastes the test run output and lists criteria covered.

### S3: Spanish classification seeds

PRD requirements: 5 (data).

In scope:

- Spanish categories and word lists for car, clothing, colour, object, folders
  and extensions; `place.json` stays shared.
- Loader reads seeds by language (`EmbeddedWordListsFactory`).
- Duplicate and format check over all seed JSON.
- Draft by Claude; a Spanish speaker reviews before release.

Out of scope: Place names, UI text.

Acceptance criteria:

- `camisa-roja.jpg` classifies as clothing and colour; `red-shirt.jpg` still
  classifies with the UI in Spanish.
- Seed files have no duplicates.

Verification (tests in `...Classification.TestsUnit`; names proposed, write them
first and see them fail):

| Criterion | Check |
|-----------|-------|
| Spanish file name | `SeedClassificationShould.classify_spanish` |
| English unaffected | `classify_english_in_spanish_ui` |
| Seed duplicates | `SeedFilesShould.have_no_duplicates` |

Run the tests above, then the gate: `dotnet build AStarDev.slnx` and `dotnet
test AStarDev.slnx --filter "FullyQualifiedName!~Integration"`.

Estimate: about 12 files. Hotspots: `Seed/*.json`,
`EmbeddedWordListsFactory.cs`.

Definition of done:

- [ ] Failing tests written first, now pass.
- [ ] Build and unit gate pass; no new warnings.
- [ ] Only files this slice needs are changed.
- [ ] PR description pastes the test run output and lists criteria covered.

### S4: Spanish UI text

PRD requirements: 1, 9.

In scope:

- Translate every key in `es.json` (Claude draft).
- Mark Spanish complete so the key test enforces it, so every later `en-GB.json`
  key needs an `es.json` key.
- Check string length in Layouts A, B and C.
- A Spanish speaker reviews the text before release.

Out of scope: Seeds, scaffolding.

Acceptance criteria:

- Every window in A, B and C shows Spanish text.
- The key test passes for `es`.

Verification (tests in `...ViewModels.TestsUnit`; names proposed, write them
first and see them fail):

| Criterion | Check |
|-----------|-------|
| All keys translated | `LocaleFilesShould.match_keys` |
| No untranslated text | Test comparing `es` to `en-GB` |
| Layouts fit | Manual, steps below |

Manual: select Spanish, open every window in A, B and C, expect Spanish text
with no clipped labels. Record the result in the PR.

Run the tests above, then the gate: `dotnet build AStarDev.slnx` and `dotnet
test AStarDev.slnx --filter "FullyQualifiedName!~Integration"`.

Estimate: about 6 files (large data files). Hotspots: `en-GB.json`, `es.json`.

Definition of done:

- [ ] Failing tests written first, now pass.
- [ ] Build and unit gate pass; no new warnings.
- [ ] Only files this slice needs are changed.
- [ ] PR description pastes the test run output and lists criteria covered.
- [ ] Manual check done and recorded.

## Epic acceptance

Closing checklist, ticked by the author of the last slice:

- [ ] Spanish selected: every window in A, B and C is Spanish, no restart. S1,
  S4
- [ ] A missing key or placeholder mismatch fails the test. S1, S4
- [ ] `camisa-roja.jpg` classifies; `red-shirt.jpg` still does. S2, S3
- [ ] An unsupported saved locale uses English. S1
- [ ] An existing database upgrades with terms and links. S2
- [ ] Category names show in the UI language. S2

Independent verifier prompt: "Check each item in the Epic acceptance list
against the merged code and tests. Report pass or fail per item with evidence
(file and test name, test output). Change nothing."

## Assumptions and decisions

- Locale `es` (neutral); live switch; Claude drafts and a Spanish speaker
  reviews; seeds translated except place names; names stored per language
  (owner, 2026-11-18).
- S1 and S2 start together; S3 needs S2; S4 needs S1.
- Cross-epic: the key test makes every other epic's new `en-GB.json` key need an
  `es.json` key once Spanish is marked complete in S4.
- File counts are estimates. S4 is mostly data, so its count is small but its
  review is large.
