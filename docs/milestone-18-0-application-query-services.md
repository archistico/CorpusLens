# Milestone 18.0 — Application query services

This milestone prepares the Avalonia UI without adding any UI code yet.

## Goal

Move the first read-only query orchestration out of `CorpusLens.Cli` and into reusable services in `CorpusLens.Application`.

The CLI remains the reference surface, but future UI code can call the same services instead of duplicating SQL orchestration or console-specific logic.

## Added services

```text
CorpusLens.Application/Queries/AnalysisRunQueryService.cs
CorpusLens.Application/Queries/CorpusProfileQueryService.cs
CorpusLens.Application/Queries/TokenIndexHealthService.cs
```

Supporting DTOs:

```text
CorpusProfileRequest
CorpusProfileResult
TokenIndexHealthResult
DifficultyThresholds
```

## Current coverage

The first extracted services cover the screens needed by the first UI milestones:

```text
- run list
- run summary
- source books
- compact corpus profile
- token index health
```

## CLI integration

The CLI now uses the new services for:

```text
stats runs
stats summary
stats profile
stats health
```

Other commands still use the existing direct store calls and can be migrated gradually when the UI needs them.

## Non-goals

This milestone intentionally does not add Avalonia, XAML, windows, dialogs, or import workflow changes.
