# Milestone 18.1 — Avalonia skeleton

Adds the first desktop project, `CorpusLens.Desktop`.

The shell is intentionally small:

- top bar with database path placeholder;
- left run list placeholder;
- main dashboard placeholder;
- bottom status bar;
- minimal `ViewModelBase` and `RelayCommand`.

The initial shell is built in C# code rather than loaded from XAML. This keeps the first Avalonia milestone robust while the project still has a namespace named `CorpusLens.Application`, which can otherwise make early startup diagnostics harder to read.

Database loading starts in Milestone 18.2.
