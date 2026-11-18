# Ignore patterns

Source: [feature-ideas.md](../feature-ideas.md), item #3 (Tier 1). Value H,
effort M. Status: Draft. Date: 2026-11-18.

## Problem

Only fixed skips exist (Hidden, Temp, OfficeLock, System, Symlink). Users cannot
exclude `node_modules`, `*.iso` or similar, so large or junk items sync.

## Goals

- Per sync pair, the user lists patterns that are never synced in either
  direction.
- Same behaviour in Layouts A, B and C.

## Non-goals

- A global (all pairs) list; per pair only.
- Deleting already-synced copies when a pattern starts matching.
- Size/age rules (folded into files on demand).
- Changing the fixed skips, which stay.

## Current state

- `ILocalFileFilter` (`LocalFileFilter`, `FileSkipReason`) skips Hidden,
  Temporary, OfficeLock, SystemFile, SymbolicLink; `ILocalFolderFilter` skips
  hidden, dot-prefixed, linked and temp folders. `LocalTreeWalker` uses both
  after `ISyncSelectionEvaluator`.
- `SelectionRule` / `SelectionRuleKind` (Include direction, Exclude) rules per
  folder, stored by `SqliteSelectionRuleStore`, edited through the
  folder-selection dialog (`FolderSelectionViewModel`, hosted for all layouts).
- No pattern concept; the remote side (change feed, download) does not consult
  the local filters.

## Requirements

1. Must: each sync pair has an ordered list of patterns, stored with the pair.
1. Must: syntax is gitignore-style: `*`, `?`, `**`; trailing `/` matches folders
   only; leading `/` anchors to the pair root; `#` comment lines and blank lines
   ignored; `!` re-includes.
1. Must: a matching file or folder is neither uploaded nor downloaded; it is
   also skipped by the local scan and watcher.
1. Must: a matching item already in the baseline stops syncing; local and cloud
   copies are kept and no deletion is planned or held.
1. Must: changing the list takes effect on the next sync run without restart;
   removing a pattern lets matching items sync again.
1. Must: the list is edited in the folder-selection dialog, with an
   add/remove/reorder list and inline validation of each line.
1. Must: an invalid pattern line is rejected with a localized message and never
   silently dropped.
1. Must: all new text uses `en-GB.json` keys.
1. Should: a pattern tester shows whether a typed path would be ignored.
1. Should: the activity log shows ignored counts per run, not one entry per
   item.

## Acceptance criteria

- Given `*.iso`, an `.iso` file is not uploaded and a cloud `.iso` is not
  downloaded.
- Given `node_modules/`, a folder of that name at any depth is skipped; a file
  named `node_modules` is not.
- Given `/build`, only `build` at the pair root matches.
- Given `*.log` and `!keep.log`, `keep.log` syncs.
- Given a synced file that now matches, a run neither deletes nor re-uploads it
  and no held deletion appears.
- Given the pattern is removed, the item syncs again.
- Given `[` (invalid), the dialog shows a localized error and the list is
  unchanged.
- Unit tests (`<Subject>Should`, snake_case) cover the matcher, planner
  integration (local and remote) and the dialog view model in each layout.

## Constraints and decisions

Owner (feature-ideas): all layouts stay and every feature must reach A, B and C;
More providers will be added.

Interview 2026-11-18:

- Per pair, edited in the folder-selection dialog.
- gitignore-style core syntax.
- Newly ignored synced files stay on both sides.
- Patterns apply in both directions.

Code: matching must be provider-neutral (operate on relative paths) so Google
Drive reuses it; paths follow existing case rules.

## Risks

- Both-direction ignoring needs the remote planner to consult patterns; a missed
  path lets ignored items download.
- Newly ignored baseline entries must not look like deletions to the
  mass-deletion guard or `LocalDeletionFinder`.
- Case sensitivity differs by OS (Fedora vs Windows); matching must follow the
  pair's existing case rules.
- Large trees: matcher runs per item per scan; must stay compiled/cached.

## Open questions

None outstanding.
