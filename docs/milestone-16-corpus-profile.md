# Milestone 16 — Corpus profile / Italian corpus validation

This milestone adds a compact run profile command intended for quick validation after importing a real corpus, especially the first Italian corpus.

## Commands

```powershell
make stats-profile RUN=1 LIMIT=10 PHRASE_LIMIT=10
```

Equivalent CLI:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats profile 1 --limit 10 --phrase-limit 10 --db ./data/corpuslens.db
```

## Output sections

`stats profile` prints:

- run metadata;
- language profile;
- source-book and chapter counts;
- core metrics;
- relative difficulty summary;
- top content words;
- top function words;
- recurring phrases with content-word boundaries.

## Phrase filters

The phrase preview uses conservative defaults:

- n-gram length: 2 to 5;
- minimum count: 3;
- minimum chapters: 2;
- content-word boundary;
- longest-only deduplication.

These defaults keep the profile readable and avoid showing many local one-chapter repetitions.

## Notes

The command does not add new analysis data and does not change the database schema. It combines existing persisted statistics and query APIs into a single validation screen.
