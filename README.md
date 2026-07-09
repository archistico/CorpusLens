# CorpusLens

CorpusLens is a small, testable C#/.NET project for building and analyzing personal language corpora from books and texts.

The first milestone intentionally focuses on a clean analysis engine and a CLI working on plain text. EPUB import is planned as the next implementation step, while the core model, analyzers and reports are already separated from the UI and infrastructure.

## Current status

Milestone 0 / first slice:

- solution structure;
- domain model;
- text normalization;
- sentence splitting;
- tokenization;
- word frequencies;
- n-grams;
- next-word statistics;
- simple phrase classification;
- Markdown and CSV reports;
- xUnit tests.

## Requirements

- .NET 10 SDK or newer compatible SDK using `global.json` roll-forward.

## Build

```bash
dotnet restore
dotnet build
```

## Test

```bash
dotnet test
```

## Run demo

```bash
dotnet run --project src/CorpusLens.Cli -- demo --out ./artifacts/demo
```

## Analyze a text file

```bash
dotnet run --project src/CorpusLens.Cli -- analyze-text ./samples/texts/sample_english_short.txt --language en --title "Sample English" --out ./artifacts/sample
```

Generated files:

- `report.md`
- `words.csv`
- `ngrams.csv`
- `next_words.csv`

## Documentation

- `docs/technical-design.md`
- `docs/roadmap.md`
- `docs/analysis-rules.md`
