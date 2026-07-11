# Milestone 12 — Phrase mining

Adds an exploratory phrase mining query over stored chapter text.

## Command

```powershell
make stats-phrases RUN=1 MIN_N=2 MAX_N=5 MIN_COUNT=3 LIMIT=30
make stats-phrases-content-boundary RUN=1 MIN_N=2 MAX_N=5 MIN_COUNT=3 LIMIT=30
```

Equivalent CLI:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats phrases 1 --min-n 2 --max-n 5 --min-count 3 --limit 30 --db ./data/corpuslens.db
dotnet run --project src/CorpusLens.Cli -- stats phrases 1 --min-n 2 --max-n 5 --min-count 3 --content-boundary --limit 30 --db ./data/corpuslens.db
```

## Behavior

- Mines repeated consecutive word phrases from stored chapter text.
- Supports fixed `--n` or a range with `--min-n` / `--max-n`.
- Supports `--min-count` and `--limit`.
- Does not join phrases across punctuation such as commas, colons, periods or quotes.
- Optional `--content-boundary` keeps phrases whose first and last words are content words while allowing function words inside.

Examples preserved by content-boundary filtering:

```text
piazza del duomo
piazza san giovanni
white whale
```

Examples usually removed by content-boundary filtering:

```text
di lui
in mezzo
per la
```

This milestone does not add persistent token indexes or statistical association scores for phrases.
