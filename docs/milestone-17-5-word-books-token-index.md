# Milestone 17.5 — Word-books da token index

`stats word-books` now prefers the persistent `TokenOccurrence` index when the run has token rows attached to the source books.

## Behaviour

- Folder runs now replace the initial aggregate-book token index with token rows attached to each real source book.
- `stats word-books <runId> <word>` uses `TokenOccurrence` when source-book token rows are available.
- Legacy runs, or runs whose token index only covers the aggregate book, automatically fall back to the previous `CleanText` path.
- Output is unchanged.

## Why this matters

Word distribution by source book no longer has to rescan chapter text for newly generated folder runs. The token index now supports:

- KWIC;
- collocations;
- phrase mining;
- word distribution by source book.

## Validation

Regenerate the run after applying this milestone:

```powershell
make clean
make corpus-create-it
make analyze-it
make stats-token-index RUN=1
make stats-word-books RUN=1 WORD="piazza" LIMIT=30
```

For a folder run, `stats-token-index` should report the source-book chapter count, and `stats word-books` should return the same distribution semantics as before.
