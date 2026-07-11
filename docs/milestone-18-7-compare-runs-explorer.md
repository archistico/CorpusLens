# Milestone 18.7 — Compare runs explorer

Adds a desktop comparison panel for two analysis runs.

## Features

- Select a left run from the main run list.
- Select a right run from the comparison panel.
- Optional single-word comparison.
- Word-difference table with all/content/function filters.
- Presence filter: all, shared only, exclusive only.
- Difficulty comparison between the selected runs.
- Cross-language note when the two runs have different language codes.
- Async loading with the existing desktop progress/status pattern.

## Architecture

The desktop app calls `RunComparisonQueryService` in `CorpusLens.Application.Queries`.
The UI does not query SQLite directly.

```text
CorpusLens.Desktop
  -> CorpusLens.Application.Queries
    -> CorpusLens.Infrastructure.Storage
```

## Scope

This milestone does not add translated/semantic cross-language comparison.
Comparisons are lexical and based on normalized word forms already present in the database.
