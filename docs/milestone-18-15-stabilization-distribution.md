# Milestone 18.15 — Desktop stabilization and distribution

## Goal

Close the first complete CorpusLens Desktop cycle with durable local preferences, recent-database recovery, diagnostic logging, smoke tests and a repeatable self-contained Windows package.

## Runtime preferences

The desktop application stores only non-sensitive UI preferences in:

```text
%LOCALAPPDATA%\CorpusLens\settings.json
```

The persisted values are:

- last selected database path;
- up to eight recent database paths;
- last EPUB input folder;
- last analysis output folder;
- recursive-search preference;
- last window width and height.

No EPUB content, chapter text, token data, credentials or analysis results are written to the settings file.

`JsonDesktopSettingsStore` normalizes paths, removes duplicate recent entries, caps the list and writes through a temporary file before replacing `settings.json`. Invalid or unreadable JSON falls back to defaults and is recorded in the diagnostic log.

## Startup and recent databases

At startup `MainWindowViewModel.InitializeAsync` attempts to reopen the last database. If the path no longer exists, it is removed from the recent list and the application remains usable. Opening a database moves it to the first position in the recent list.

The top bar contains:

- **Open database** for a file picker;
- the recent-database selector;
- **Open recent**;
- **Refresh**;
- **Logs** for the local diagnostic directory.

## Diagnostic logging and critical errors

Logs are written to:

```text
%LOCALAPPDATA%\CorpusLens\logs\corpuslens-YYYYMMDD.log
```

Recoverable asynchronous operations continue to report concise errors in the status bar and now also write exception details to the log. Application-domain exceptions, unobserved task exceptions and fatal startup/runtime failures are recorded centrally. On Windows, a fatal error also displays a native critical-error dialog containing the log path.

Logging is deliberately best-effort: a logging failure cannot replace or mask the original application error.

## Version and Windows metadata

The desktop executable is versioned as `18.15.0` and includes:

- product and description metadata;
- assembly, file and informational version values;
- a Windows executable icon;
- an `asInvoker` application manifest;
- per-monitor DPI awareness;
- long-path awareness.

## Self-contained Windows publication

Run from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\publish-win-x64.ps1
```

or:

```powershell
make publish-win-x64
```

The script executes a Release, self-contained, single-file `win-x64` publish and creates:

```text
dist/CorpusLens-win-x64-18.15.0.zip
dist/CorpusLens-win-x64-18.15.0.zip.sha256
```

Before packaging, the script rejects any publish tree containing:

- `.db`, `.sqlite` or `.sqlite3` files;
- `.db-wal` or `.db-shm` files;
- EPUB files;
- extracted text;
- import diagnostics or import-failure CSV files.

The `dist/` directory is ignored by Git.

## Tests

The desktop test project now includes stabilization smoke tests for:

- JSON preference round-trip;
- fallback from invalid settings JSON;
- diagnostic-log creation;
- automatic reopening of the last database;
- corpus loading during startup;
- recent-database ordering;
- removal of an unavailable last database;
- persistence of analysis folders, recursive mode and window size.

The existing Desktop and Application tests continue to cover EPUB request validation, progress, persistence, compensation and explorer behavior.

## Acceptance checklist

```powershell
dotnet build
dotnet test
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\publish-win-x64.ps1
```

Then verify on a Windows x64 machine without the .NET SDK:

1. extract the generated ZIP into a normal user-writable folder;
2. start `CorpusLens.exe`;
3. open a database and close the application;
4. start it again and confirm that the database is restored;
5. inspect a run and start a small EPUB-folder analysis;
6. open the diagnostic log folder;
7. confirm that the ZIP contains no local corpus data.
