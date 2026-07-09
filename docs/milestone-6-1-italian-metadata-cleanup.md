# Milestone 6.1 — Italian metadata-only chapter cleanup

## Goal

Use the import diagnostics report to improve EPUB cleanup for Italian corpora, especially Liber Liber EPUBs that expose license/front-matter pages as standalone chapters.

## Problem found

The Italian diagnostics showed many suspicious terms still present in extracted chapters:

- `www`
- `indice`
- `licenza`
- `e-book`
- `liberliber`
- `https`
- `liber liber`
- `questo e-book`

Several short chapters were metadata-only pages rather than narrative content.

## Change

`EpubBoilerplateCleaner.IsLikelyFrontMatterOnly()` now detects standalone Liber Liber metadata chapters even when they do not contain a full table of contents.

The detection uses a conservative combination of:

- Liber Liber markers;
- license / e-book / URL markers;
- metadata lines;
- small total word count;
- few long prose lines.

This keeps normal narrative chapters that merely contain words like `indice` in real prose.

## Validation

Added tests for:

- standalone Liber Liber metadata chapter detection;
- narrative text mentioning `indice` without being treated as front matter.

## Expected effect

After regenerating an Italian corpus, `import_diagnostics.md` should show lower counts for:

- `licenza`;
- `e-book`;
- `liberliber`;
- `liber liber`;
- `www`;
- `https`.

Some occurrences may remain if they appear inside real notes, appendices or source-specific content.
