# Milestone 10 — Word distribution by source book

This milestone adds the first per-book query for aggregate analysis runs.

## Goal

An aggregate run still keeps one corpus-level statistics set, but users can now inspect how a word is distributed across the real EPUB books linked to the run through `AnalysisRunBook`.

## New command

```powershell
make stats-word-books RUN=1 WORD="whale" LIMIT=30
```

Equivalent raw command:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats word-books 1 "whale" --limit 30 --db ./data/corpuslens.db
```

## Output

The command prints source books where the word occurs, ordered by descending count.

Columns:

- `Book`
- `Author`
- `Count`
- `Per million`
- `Word tokens`

The command also prints a simple dispersion indicator: source book count, matched book count and matched-book coverage percentage.

## Notes

- The query does not require a persistent token index yet.
- Counts are computed from stored clean chapter text.
- Apostrophes and dash variants are normalized consistently with KWIC lookup.
- Books with zero matches are omitted.

A future token index milestone can make this query faster and extend it with dispersion scores, chapter-level coverage and richer filtering.
