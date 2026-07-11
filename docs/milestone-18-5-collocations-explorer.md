# Milestone 18.5 — Desktop collocations explorer

Adds a Collocations explorer section to the Avalonia desktop UI.

## Scope

- Search collocations for the selected run.
- Configure target word, window, minimum count, minimum Dice and limit.
- Switch between all words, content words only and function words only.
- Load results asynchronously without blocking the UI.
- Reuse `CorpusLens.Application.Queries` rather than querying SQLite directly from the desktop project.

## Query path

`CorpusLens.Desktop` calls `CollocationExplorerQueryService`, which delegates to `SqliteCorpusStore.ListCollocationsAsync`.

If the selected run has a healthy token index, collocations are resolved through `TokenOccurrence`; otherwise the existing legacy fallback is used by the infrastructure layer.

## Not included

- Charts.
- Export from UI.
- Advanced ranking controls.
- Phrase explorer UI.
