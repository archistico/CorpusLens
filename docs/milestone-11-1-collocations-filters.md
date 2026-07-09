# Milestone 11.1 — Collocations filters and ranking polish

## Scope

This milestone keeps the first collocation implementation simple, but makes the CLI output useful for everyday inspection.

## Changes

- `stats collocations` now accepts `--content-only`.
- `stats collocations` now accepts `--function-only`.
- The output includes a `Type` column with `content` or `function`.
- Numbering is based on displayed rows after filtering.
- The command prints the selected filter and how many collocates are shown.
- Added Makefile targets:
  - `stats-collocations-content`
  - `stats-collocations-function`

## Examples

```powershell
make stats-collocations RUN=1 WORD="whale" WINDOW=4 LIMIT=30
make stats-collocations-content RUN=1 WORD="whale" WINDOW=4 LIMIT=30
make stats-collocations-function RUN=1 WORD="love" WINDOW=4 LIMIT=30
```

Raw CLI equivalents:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats collocations 1 "whale" --window 4 --limit 30 --db ./data/corpuslens.db
dotnet run --project src/CorpusLens.Cli -- stats collocations 1 "whale" --content-only --window 4 --limit 30 --db ./data/corpuslens.db
dotnet run --project src/CorpusLens.Cli -- stats collocations 1 "love" --function-only --window 4 --limit 30 --db ./data/corpuslens.db
```

## Design note

Filtering is applied in the CLI using the existing stop-word profiles. The underlying collocation query still computes raw frequency-window collocations from the cleaned chapter text.

When a filter is active, the CLI fetches more rows internally and then applies the selected filter, so `--content-only --limit 30` is less likely to return only a handful of rows after removing high-frequency function words.

## Out of scope

- PMI.
- Log-likelihood.
- Dice coefficient.
- Persistent token index.
- Lemmatized collocations.
