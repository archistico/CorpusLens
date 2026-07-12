# Windows x64 distribution

## Prerequisites for maintainers

- .NET 10 SDK;
- PowerShell 5.1 or later;
- restored NuGet dependencies;
- a clean build and test run.

## Build the package

```powershell
dotnet build
dotnet test
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\publish-win-x64.ps1
```

The publication is:

- Release configuration;
- `win-x64` runtime;
- self-contained;
- single-file;
- not trimmed, to preserve Avalonia and reflection-based behavior;
- published without PDB files.

## Output

```text
dist/CorpusLens-win-x64-18.15.0.zip
dist/CorpusLens-win-x64-18.15.0.zip.sha256
```

Validate the checksum with:

```powershell
Get-FileHash .\dist\CorpusLens-win-x64-18.15.0.zip -Algorithm SHA256
```

## Clean-package policy

The publication script fails when it detects local corpus material. The package must never contain:

- SQLite databases or sidecar files;
- EPUB files;
- `extracted_text.txt`;
- import diagnostics;
- analysis artifacts from development runs.

The application writes settings and logs to `%LOCALAPPDATA%\CorpusLens`, so an installed application folder remains replaceable.

## Installation

CorpusLens is distributed as a portable ZIP:

1. extract the ZIP into a user-writable directory;
2. run `CorpusLens.exe`;
3. optionally create a Start-menu or desktop shortcut.

Administrator privileges are not required. Windows may display a reputation warning for unsigned binaries; code signing is outside Milestone 18.15.

## Upgrade

Close CorpusLens and replace the application directory with the new package. Keep corpus databases and artifacts in separate data directories. User preferences survive upgrades in `%LOCALAPPDATA%\CorpusLens`.

## Uninstall

Delete the extracted application directory. To remove preferences and logs as well, delete:

```text
%LOCALAPPDATA%\CorpusLens
```
