# Milestone 18.4 — Word explorer

Adds the first interactive word exploration panel to the Avalonia desktop app.

## Scope

The desktop app can now search a word in the selected run and show:

- word summary;
- next words;
- previous words;
- KWIC contexts;
- distribution by source book.

The UI uses `CorpusLens.Application.Queries.WordExplorerQueryService`; it does not query SQLite directly.

## Loading model

Word searches run asynchronously on a background task. The status bar and progress bar are reused from the dashboard loading flow.

## Limits

The first UI version uses fixed compact limits:

- next words: 10;
- previous words: 10;
- KWIC contexts: 10;
- book distribution rows: 10;
- KWIC context words per side: 8.

Future milestones can add editable limits, filters and richer tables.
