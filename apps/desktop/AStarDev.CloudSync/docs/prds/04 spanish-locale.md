# Spanish locale

Source: [feature-ideas.md](../feature-ideas.md), item #4 (Tier 1). Value M,
effort L. Status: Draft. Date: 2026-11-18.

## Problem

The UI has one culture (`en-GB`), so Spanish speakers cannot use it, and every
new string deepens the gap. Classification seeds are English only, so Spanish
file names get no categories.

## Goals

- UI available in Spanish, selectable by the user.
- Spanish file names classify into categories, and a file can match in either
  language.

## Non-goals

- Languages beyond English and Spanish (but adding one must be data-only).
- Regional Spanish variants (`es` is neutral).
- Translating place names (`Seed/place.json`).
- Right-to-left layout.

## Current state

- `DisplayPreferencesFactory.SupportedLocales = [DefaultLocale]`; unknown
  locales fall back to the default.
- All text from `Assets/Localization/en-GB.json` (~30 KB) via
  `ILocalizationService`; view models derive from `LocalizedObservableObject`.
- Classification seeds: `Classification/Seed/*.json` (car, categories, clothing,
  colour, extensions, folders, object, place 2 MB), loaded by
  `EmbeddedWordListsFactory`; vocabulary is not keyed by language.
- No culture picker in Display settings.

## Requirements

1. Must: add `Assets/Localization/es.json` with every key present in
   `en-GB.json`.
1. Must: `SupportedLocales` includes `es`; Display settings has a language
   picker, in all three layouts, listing languages by their own name.
1. Must: choosing a language applies immediately, without restart, and is
   remembered.
1. Must: dates, numbers and sizes format for the selected culture.
1. Must: classification terms carry a language; seeds exist for Spanish for
   categories and word lists (car, clothing, colour, object, folders,
   extensions); `place.json` stays shared.
1. Must: a file matches terms of either language regardless of the UI language,
   and the UI shows category names in the UI language.
1. Must: a pseudo-locale test fails when a key is missing from any locale, has
   mismatched format placeholders, or when a view uses hard-coded text.
1. Must: existing English user data and terms keep working unchanged (migration
   for the language column defaults existing rows to English).
1. Should: fall back to English for a missing key at runtime and log it.

## Acceptance criteria

- Given Spanish selected, then every window in A, B and C shows Spanish text
  without restart.
- Given `es.json` lacks a key or has a different placeholder set, the locale
  test fails.
- Given the file `camisa-roja.jpg`, the Spanish seeds classify it (clothing,
  colour); `red-shirt.jpg` still classifies in English with the UI in Spanish.
- Given an unsupported saved locale, the app uses English.
- Given an existing database, upgrade keeps all terms and links.

## Constraints and decisions

Owner (feature-ideas): second UI language is Spanish; do before more strings
pile up; classification vocabulary is per language (terms keyed by language so a
file can match in either); all layouts stay and each feature must reach A, B and
C.

Interview 2026-11-18:

- Locale code `es` (neutral).
- Claude drafts the Spanish text; a Spanish speaker reviews before release.
- Language switch is live.
- Seeds: translate categories and word lists, skip place names.

## Risks

- Machine-drafted text may be wrong or awkward; release is gated on owner
  review.
- Live switching must refresh every cached string (menus, tray, notifications).
- Language column on terms is a schema change in the classification database.
- String length differences can break layouts; check all three.

## Open questions

None outstanding.
