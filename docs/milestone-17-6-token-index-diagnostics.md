# Milestone 17.6 — Token index diagnostics and performance

This milestone consolidates the persistent token index introduced in Milestone 17.

## Changes

- `stats token-index` now reports diagnostics, not only raw counts:
  - database size;
  - expected word tokens from `AnalysisRun`;
  - indexed word tokens;
  - coverage percentage;
  - token delta;
  - indexed chapters vs expected chapters;
  - indexed books and indexed source books;
  - run position range and gap count;
  - which query families use token index or fallback.
- Added SQLite indexes for token-index backed queries:
  - `(AnalysisRunId, BookId, NormalizedToken)` for `stats word-books`;
  - `(AnalysisRunId, ChapterId, ChapterPosition)` for context-window queries;
  - `(AnalysisRunId, ChapterId, NormalizedToken, ChapterPosition)` for KWIC/collocation lookup.

## Notes

`TokenOccurrence` currently indexes word tokens only. Therefore `Tokens` and `Word tokens` are expected to be identical.

Legacy runs without token rows remain supported. Query commands fall back to stored chapter text when the token index is missing.
