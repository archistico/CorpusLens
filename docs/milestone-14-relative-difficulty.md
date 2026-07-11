# Milestone 14 — Relative difficulty

This milestone adds a first, transparent difficulty profile for analysis runs.

The goal is not to provide an absolute school-grade readability formula. CorpusLens reports a relative heuristic that is useful for comparing similar corpora or different runs of the same language.

## Commands

```powershell
make stats-difficulty RUN=1
make stats-compare-difficulty RUN_A=1 RUN_B=2
```

Direct CLI:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats difficulty 1 --db ./data/corpuslens.db

dotnet run --project src/CorpusLens.Cli -- stats compare-difficulty 1 2 --db ./data/corpuslens.db
```

## Metrics

The profile includes:

- average words per sentence;
- average characters per word;
- long-word share, default `>= 7` characters;
- very-long-word share, default `>= 10` characters;
- content-word share;
- function-word share;
- lexical diversity per 1,000 word tokens.

The thresholds can be changed:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats difficulty 1 --long-word-length 8 --very-long-word-length 12 --db ./data/corpuslens.db
```

## Heuristic score

The score combines the profile metrics with fixed lightweight weights. Higher means relatively harder.

The score is intentionally visible and explainable. It should be used for comparison, not as a certified readability measurement.

## Cross-language comparisons

When runs use different languages, CorpusLens prints the same warning used by lexical comparison commands. Different languages can have different morphology and word-length distributions, so difficulty scores are best compared within the same language or among carefully chosen corpora.
