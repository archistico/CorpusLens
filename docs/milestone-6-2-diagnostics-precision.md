# Milestone 6.2 — Diagnostics precision and Italian metadata cleanup

## Goal

Refine the import diagnostics after testing the Italian corpus. The previous diagnostics were useful, but they still reported too many suspicious chapters only because a long real chapter contained weak words such as `indice`, `sommario`, `prefazione`, or `editore`.

This milestone keeps the diagnostic term counts, but improves chapter-level suspicious detection.

## Changes

- Strengthen detection of short/medium Italian metadata-only chapters containing `QUESTO E-BOOK`, `LICENZA`, `Liber Liber`, URLs, and donation text.
- Keep long narrative chapters from being flagged only because they mention weak terms such as `indice`.
- Split suspicious terms internally into:
  - strong boilerplate terms;
  - weak front-matter terms.
- Continue reporting all suspicious term counts in the diagnostics summary.

## Acceptance criteria

- Metadata-only Italian chapters are skipped more reliably.
- A long real narrative chapter mentioning `indice` is not listed as suspicious only for that word.
- Strong boilerplate terms such as `e-book`, `licenza`, `liberliber`, `www`, and `https` still flag suspicious chapters.
- Existing import diagnostics output format remains compatible.
