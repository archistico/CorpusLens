# Milestone 18.8 — Books explorer

Adds a desktop source-books explorer for the selected analysis run.

## Features

- Loads source books asynchronously when a run is selected.
- Shows aggregate book, chapter, character and language counts.
- Lists source books in their run order.
- Selects the first source book automatically.
- Shows title, author, language, database ID, chapter count, character count, original file path and file hash for the selected book.
- Supports both aggregate folder runs and legacy/single-book runs through the existing query fallback.
- Disables the book list while the desktop app is busy.

## Architecture

The desktop app uses `AnalysisRunQueryService.ListRunBooksAsync` from `CorpusLens.Application.Queries`.
The UI does not access SQLite directly and does not duplicate the existing run-book query.

```text
CorpusLens.Desktop
  -> CorpusLens.Application.Queries
    -> CorpusLens.Infrastructure.Storage
```

## Scope

This milestone is read-only. It does not yet add chapter browsing, opening the original EPUB, corpus creation or analysis from the desktop UI.
