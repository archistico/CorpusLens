# CorpusLens Desktop — User guide

## Start the application

For development:

```powershell
make desktop
```

For a published build, extract the Windows ZIP and run `CorpusLens.exe`. The self-contained package does not require the .NET SDK.

## Open a database

Select **Open database** and choose a CorpusLens SQLite file. The application loads corpora and runs in the background. The most recently opened database is restored at the next startup when it is still available.

The recent-database selector keeps up to eight paths. Choose one and select **Open recent**. Missing paths are removed automatically when opened.

## Manage corpora

The left panel contains **All corpora** and each persisted corpus. Selecting one filters the run list. Use the corpus-creation panel to create a new corpus with a unique name and one of the supported languages:

```text
en  English
it  Italian
fr  French
de  German
```

Persistent creation requires the confirmation checkbox.

## Explore an analysis run

Select a run from the left panel. CorpusLens loads:

- dashboard metrics and language profile;
- source books and chapters;
- reports and exported files;
- word, n-gram, collocation and phrase explorers;
- run comparison tools.

Long operations can be cancelled from the status bar.

## Analyze an EPUB folder

1. Select one specific corpus, not **All corpora**.
2. Choose the EPUB input folder.
3. Choose an output folder for reports, CSV files and extracted text.
4. Enable recursive scanning when EPUB files are stored in subfolders.
5. Select the confirmation checkbox.
6. Select **Analyze EPUB folder**.

The selected corpus language is used automatically. Progress reports the active phase and file counters. When analysis completes, the new run is selected and its output folder and diagnostics can be opened directly.

Cancellation is cooperative. Partial database rows are removed; files already written to the output directory may remain and can be deleted manually.

## Reports and exports

The reports panel distinguishes:

- **Available** — the file exists;
- **Missing** — a persisted path no longer resolves to a file;
- **Not generated** — the optional artifact was not produced.

CorpusLens opens existing files with the operating-system default application and never edits them from this panel.

## Local settings

Non-sensitive UI preferences are stored in:

```text
%LOCALAPPDATA%\CorpusLens\settings.json
```

Delete this file while CorpusLens is closed to reset recent databases, folder preferences and window size.

## Diagnostic logs

Select **Logs** in the top bar. Daily logs are stored in:

```text
%LOCALAPPDATA%\CorpusLens\logs
```

When reporting a defect, include the relevant log and a description of the operation. Do not attach databases, EPUB files or extracted text unless you are legally allowed and intentionally choose to share them.

## Updating a portable installation

1. close CorpusLens;
2. keep databases and artifacts outside the application folder;
3. extract the new package to a new folder or replace the old application files;
4. start the new `CorpusLens.exe`.

Settings and recent paths are retained because they live under `%LOCALAPPDATA%`, not in the application folder.
