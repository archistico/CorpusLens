# Milestone 18.6 — Desktop phrase explorer

Adds a phrase mining panel to the Avalonia desktop app.

## Included

- `PhraseExplorerQueryService` in `CorpusLens.Application/Queries`.
- Desktop controls for min/max n, min count, min chapters and limit.
- Content-boundary and longest-only options.
- Asynchronous loading with the existing busy/progress status.
- Result table text with phrase, n, count, chapters, per-million frequency and boundary type.

## Not included

- Import from UI.
- Phrase KWIC.
- Dedicated grid/table controls.
- Compare-runs UI.
