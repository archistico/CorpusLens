# Milestone 17.7 — Token index consolidation

This milestone consolidates the token-index work without changing the analysis model.

## Added

- `stats health <runId>` as a compact run health check.
- `make stats-health RUN=1`.
- README and analysis-rules notes for token-index query paths and fallback behavior.

## Health check

`stats health` reports:

- run existence and completion status;
- token-index presence;
- token coverage vs expected word tokens;
- chapter and source-book coverage;
- run-position gaps;
- query path for KWIC, collocations, phrases and word-books.

It returns an error only when the run is missing or arguments are invalid. Warnings are printed for incomplete/missing indexes, so legacy runs remain usable through fallback query paths.

## No behavior changes

The following query behavior is unchanged:

- KWIC uses token index when available, otherwise fallback.
- Collocations use token index when available, otherwise fallback.
- Phrase mining uses token index when available, otherwise fallback.
- Word-books use token index when source-book rows are available, otherwise fallback.

