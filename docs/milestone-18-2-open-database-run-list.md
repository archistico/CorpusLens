# Milestone 18.2 — Open database and run list

This milestone connects the Avalonia desktop shell to an existing CorpusLens SQLite database.

## Added

- `Open database` button using Avalonia storage picker.
- Run loading through `CorpusLens.Application.Queries.AnalysisRunQueryService`.
- Left-side run list.
- Run selection.
- Basic run summary in the main area:
  - sentences
  - token counts
  - word counts
  - average words per sentence
  - average characters per word
  - report path
- Token-index health summary through `TokenIndexHealthService`.
- Query-path summary for KWIC, collocations, phrases and word-books.
- Refresh button for reloading the selected database.

## Not included yet

- Importing EPUB folders from the UI.
- Word explorer.
- KWIC table.
- Collocation and phrase explorers.
- Run comparison UI.

Those remain future milestones.

## Design note

The desktop project does not query SQLite directly. It uses the Application query services introduced in Milestone 18.0. This keeps the UI as an orchestration and presentation layer.
