# Milestone 12.2 — Phrase mining polish

Adds small CLI filters that make phrase mining easier to validate on real corpora.

## Commands

```powershell
make stats-phrases RUN=1 MIN_N=2 MAX_N=5 MIN_COUNT=3 LIMIT=30
make stats-phrases-content-boundary RUN=1 MIN_N=2 MAX_N=5 MIN_COUNT=3 MIN_CHAPTERS=2 LIMIT=30
make stats-phrases-content-boundary RUN=1 MIN_N=2 MAX_N=5 MIN_COUNT=3 LONGEST_ONLY=--longest-only LIMIT=30
```

Equivalent CLI:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats phrases 1 --min-n 2 --max-n 5 --min-count 3 --min-chapters 2 --content-boundary --limit 30 --db ./data/corpuslens.db

dotnet run --project src/CorpusLens.Cli -- stats phrases 1 --min-n 2 --max-n 5 --min-count 3 --content-boundary --longest-only --limit 30 --db ./data/corpuslens.db
```

## New options

- `--min-chapters <n>` keeps only phrases that occur in at least `n` chapters.
- `--longest-only` removes redundant shorter phrases when a longer phrase has the same count and chapter count.

Example:

```text
piazza del
piazza del duomo
```

If both have the same count and chapter count, `--longest-only` keeps only:

```text
piazza del duomo
```

This is intentionally conservative: shorter phrases are removed only when the longer phrase has the same count and chapter coverage.

## Output changes

The `stats phrases` header now includes:

```text
Minimum chapters
Nested phrases
Fetched candidates
Matched phrases
Shown phrases
```

`Fetched candidates` is useful because phrase filters are applied after a larger candidate set is retrieved from the stored chapter text.
