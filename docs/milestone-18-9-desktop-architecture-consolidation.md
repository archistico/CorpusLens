# Milestone 18.9 — Desktop architecture consolidation

Consolidates the Avalonia desktop architecture before the chapter and n-gram explorers are added.

## ViewModel structure

`MainWindowViewModel` now coordinates database selection, the run list and global operations. Feature state and query behavior are delegated to dedicated ViewModels:

- `DashboardViewModel`
- `BooksExplorerViewModel`
- `WordExplorerViewModel`
- `CollocationsExplorerViewModel`
- `PhraseExplorerViewModel`
- `CompareRunsViewModel`
- `DesktopOperationStateViewModel`

The public compatibility surface of `MainWindowViewModel` is preserved so the validated UI behavior does not change.

## Async operation model

Global desktop operations use one coordinator that:

- updates the status message and busy indicator together;
- links caller cancellation tokens;
- cancels a superseded operation;
- prevents an older operation from overwriting the status of a newer one;
- applies a consistent cancellation and error path.

Dashboard health/profile queries still tolerate partial failures, and book loading continues to fail independently without discarding a valid dashboard.

## Testability

Each feature ViewModel accepts optional query delegates. Production uses the existing application query services; tests inject deterministic in-memory results without opening SQLite.

The new `CorpusLens.Desktop.Tests` project covers:

- source-book loading and automatic selection;
- word-result formatting;
- collocation filter propagation;
- phrase filter propagation;
- default comparison-run selection;
- coordinated busy/status state.

## View organization

The programmatic Avalonia window remains visually unchanged, but its code is separated into feature-specific partial files for shell, bindings, books, words, collocations, phrases and comparisons.

## Architecture boundary

The dependency flow remains:

```text
CorpusLens.Desktop
  -> CorpusLens.Application.Queries
    -> CorpusLens.Infrastructure.Storage
```

No SQLite query or persistence logic was moved into the desktop project.

## Scope

This milestone is an architectural refactor. It does not add new corpus-analysis behavior or new visible explorer functions. Chapter browsing starts in Milestone 18.10.
