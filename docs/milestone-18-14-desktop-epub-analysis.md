# Milestone 18.14 — Desktop EPUB analysis

## Goal

Run the complete persisted EPUB-folder workflow from the Avalonia desktop application without duplicating the CLI pipeline.

## User workflow

1. Open an existing CorpusLens SQLite database.
2. Select one specific corpus in the left navigator.
3. Choose the folder containing EPUB files.
4. Choose the artifact output folder.
5. Optionally enable recursive scanning.
6. Confirm the persistent operation.
7. Start the analysis and follow stage, percentage and import counters.
8. Cancel cooperatively when required.
9. Inspect the automatically selected new run and open its output or diagnostics.

The corpus language is authoritative. The desktop form displays it and the Application use case rejects a request whose language does not match the persisted corpus.

## Architecture

```text
MainWindow
  -> MainWindowViewModel
    -> EpubAnalysisViewModel
      -> AnalyzeEpubFolderAndSaveUseCase
        -> AnalyzeEpubFolderUseCase
          -> EPUB reader / corpus analyzer / report writers
        -> SqliteCorpusStore
```

The Desktop layer owns picker interaction, confirmation and presentation state. It does not open SQLite connections and does not reproduce the import or analysis algorithms.

`EpubAnalysisDefaults` supplies the shared default `AnalysisSettings` and EPUB search pattern to both CLI and Desktop.

## Progress and responsiveness

`EpubAnalysisProgress` reports:

- current stage;
- normalized percentage;
- descriptive message;
- processed/total EPUB count;
- successfully imported count;
- skipped/failed count.

Progress covers discovery, per-file import, corpus analysis, artifact generation, database persistence and token-index construction. The application pipeline continues away from the UI synchronization context; `Progress<T>` marshals updates back to Avalonia.

The in-memory corpus-analysis stage is currently one coarse progress phase. Cancellation is checked immediately before and after it and throughout I/O and database loops.

## Persistence consistency

Folder extraction and report generation complete before database writes begin. During persistence, every newly inserted aggregate/source book id is tracked. If cancellation or an exception occurs before the workflow returns successfully, `SqliteCorpusStore.DeleteBooksAsync` removes those books in one transaction. Foreign-key cascades remove dependent chapters, analysis runs, statistics, run-book links and token occurrences.

Filesystem artifacts already written before cancellation are intentionally not deleted: they may contain useful import diagnostics and are outside the database consistency boundary.

## Completion behavior

After a successful save, the desktop reloads corpora and runs, preserves the selected corpus and selects the new analysis-run id. Dashboard, source books, chapters and report/export explorer are loaded through the existing selection path.

The result panel shows imported and skipped EPUB counts and exposes the latest output folder, `import_diagnostics.md` and `import_failures.csv` through the existing safe operating-system launcher.

## Validation

Run:

```powershell
dotnet build
dotnet test
```

Manual smoke test:

1. open a database containing a corpus;
2. select that corpus;
3. analyze a small EPUB folder;
4. verify live progress and a responsive window;
5. verify that the new run is selected automatically;
6. open diagnostics and the output folder;
7. start a larger analysis, cancel it, refresh, and verify that no partial run appears.
