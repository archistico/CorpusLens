# Milestone 13 — Corpus comparison

Adds first-run comparison commands without changing the database schema.

## Commands

Compare one word between two analysis runs:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats compare-word 1 2 "love" --db ./data/corpuslens.db
```

Compare top lexical differences between two runs:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats compare-words 1 2 --content-only --min-count 5 --limit 30 --db ./data/corpuslens.db
```

Makefile targets:

```powershell
make stats-compare-word RUN_A=1 RUN_B=2 WORD="love"
make stats-compare-words RUN_A=1 RUN_B=2 LIMIT=30 MIN_COUNT=5
make stats-compare-words-content RUN_A=1 RUN_B=2 LIMIT=30 MIN_COUNT=5
make stats-compare-words-function RUN_A=1 RUN_B=2 LIMIT=30 MIN_COUNT=5
```

## Metrics

`stats compare-word` shows absolute counts, document counts, per-million frequency, share of combined count, per-million difference and left/right ratio.

`stats compare-words` fetches top words from both runs, merges them, filters by count and optional word type, then ranks by absolute per-million difference.

This is intentionally lightweight. It is useful for first inspection, while later milestones can add stronger keyness metrics such as log-likelihood or dispersion-aware scoring.
