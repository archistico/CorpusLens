# Milestone 11.3 — Collocation thresholds

This milestone adds threshold controls to collocation queries.

## CLI

```powershell
dotnet run --project src/CorpusLens.Cli -- stats collocations 1 "piazza" --window 4 --limit 30 --min-count 3 --db ./data/corpuslens.db

dotnet run --project src/CorpusLens.Cli -- stats collocations 1 "piazza" --content-only --window 4 --limit 30 --min-count 3 --min-dice 0.02 --db ./data/corpuslens.db
```

## Makefile

```powershell
make stats-collocations RUN=1 WORD="piazza" WINDOW=4 LIMIT=30 MIN_COUNT=3
make stats-collocations-content RUN=1 WORD="piazza" WINDOW=4 LIMIT=30 MIN_COUNT=3 MIN_DICE=0.02
make stats-collocations-function RUN=1 WORD="love" WINDOW=4 LIMIT=30 MIN_COUNT=3
```

## Output

The collocation header now reports:

```text
Minimum count: 3
Minimum Dice: 0.02
Matched collocates: 12
Shown collocates: 12 of 12
```

`Matched collocates` is calculated after applying filter and thresholds to an expanded candidate set. `Shown collocates` reports how many rows are displayed after `--limit`.

## Scope

No schema change. No token index. Thresholds are applied in the CLI after retrieving ranked collocations from the stored cleaned chapters.
