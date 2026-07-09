# Milestone 9 — Database model v2: run aggregate with real books

## Goal

Folder analysis still produces one aggregate analysis run, but the database now preserves the real EPUB books that contributed to that run.

This keeps existing aggregate commands stable while enabling later per-book and dispersion queries.

## Design

The existing aggregate run remains unchanged:

- `AnalysisRun.BookId` points to the synthetic aggregate source, for example `EPUB folder: en`.
- Aggregate statistics remain stored in `WordStatistic`, `NGramStatistic`, `NextWordStatistic`, and `SentenceCategoryStatistic`.
- KWIC still reads aggregate chapters, so existing commands do not regress.

A new bridge table links the run to the actual source books:

```sql
AnalysisRunBook
- Id
- AnalysisRunId
- BookId
- OrderIndex
```

Each real EPUB is also persisted as a normal `Book` with its own `Chapter` rows.

## New CLI command

```powershell
dotnet run --project src/CorpusLens.Cli -- stats books 1 --db ./data/corpuslens.db
```

Make target:

```powershell
make stats-books RUN=1
```

Expected output shape:

```text
Source books for run 1

#    Book                              Author                    Chapters  Characters
---  --------------------------------  ------------------------  --------  ----------
  1  Alice's Adventures in Wonderl...  Lewis Carroll                    12      144395
  2  A Room with a View                E. M. Forster                    20      374211
```

## Why this matters

This milestone prepares these later features without implementing them yet:

- word distribution across books;
- book-specific word frequencies;
- dispersion metrics;
- word present in all/most/few books;
- comparison between one book and the aggregate corpus.

## Non-goals

This milestone does not yet persist every token occurrence and does not yet compute per-book statistics.
