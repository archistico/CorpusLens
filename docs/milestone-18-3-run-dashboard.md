# Milestone 18.3 — Run dashboard / corpus profile

This milestone turns the first Avalonia database view into a compact run dashboard.

## Added

- The desktop app now loads the same compact corpus profile used by `stats profile`.
- The selected run dashboard shows:
  - core metrics;
  - corpus profile and language profile;
  - difficulty summary;
  - top content words;
  - top function words;
  - recurring phrases;
  - token-index health;
  - query path status.

## Architecture

The desktop app still does not query SQLite directly. It uses:

```text
CorpusLens.Desktop
  -> CorpusLens.Application.Queries
     -> CorpusLens.Infrastructure.Storage
```

The dashboard is read-only. Import, word explorer, KWIC tables, collocations and phrase explorers remain CLI-first until later UI milestones.

## Validation

Recommended checks:

```powershell
dotnet build
dotnet test
make desktop
```

Then open an existing `corpuslens.db`, select a run and verify that the dashboard matches the CLI profile:

```powershell
make stats-profile RUN=1 LIMIT=10 PHRASE_LIMIT=10
make stats-health RUN=1
```
