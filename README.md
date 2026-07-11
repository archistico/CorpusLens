# CorpusLens

CorpusLens is a small C#/.NET tool for building and exploring language corpora from EPUB and text files.

It is CLI-first, testable, and stores analysis data in SQLite so that corpora can be queried after import.

## Features

- EPUB and plain-text analysis
- English, Italian, French and German stop-word profiles
- word frequencies, content/function word views, n-grams and next-word statistics
- word detail with previous/next words
- KWIC contexts
- source-book lists for aggregate corpus runs
- word distribution by book
- word comparison between analysis runs
- relative difficulty profiles for analysis runs
- collocations with content/function filters
- repeated phrase mining
- import diagnostics for EPUB folders

## Requirements

- .NET 10 SDK
- `make` available from PowerShell/Git Bash/MSYS2 or similar

## Project layout

```text
books/
  en/        English EPUB files, ignored by git
  it/        Italian EPUB files, ignored by git
src/         application source
tests/       xUnit tests
docs/        technical notes and roadmap
data/        local SQLite database, generated
artifacts/   reports and extracted text, generated
```

Generated folders are intentionally not versioned.

## Quick start

```powershell
make check
make setup-books
```

Put EPUB files in `books/en` or `books/it`, then create and analyze a corpus:

```powershell
make corpus-create-en
make analyze-en
```

For Italian:

```powershell
make corpus-create-it
make analyze-it
```

`make check` deletes `./data` and `./artifacts`. Do not run it between two analyses if you want both corpora in the same SQLite database.

To analyze English and Italian into the same database:

```powershell
make clean
make corpus-create-en
make corpus-create-it
make analyze-en
make analyze-it
```

## Common commands

```powershell
make stats-runs LIMIT=10
make stats-summary RUN=1
make stats-books RUN=1
make stats-words RUN=1 LIMIT=25
make stats-content RUN=1 LIMIT=25
make stats-function RUN=1 LIMIT=25
```

Word-level queries:

```powershell
make stats-word RUN=1 WORD="alice" LIMIT=25
make stats-word-books RUN=1 WORD="whale" LIMIT=30
make stats-kwic RUN=1 WORD="alice" LIMIT=10 CONTEXT=8
make stats-next RUN=1 WORD="don't" LIMIT=25
```

Compare runs:

```powershell
make stats-compare-word RUN_A=1 RUN_B=2 WORD="love"
make stats-compare-words-content RUN_A=1 RUN_B=2 LIMIT=30 MIN_COUNT=5
make stats-compare-words-content RUN_A=1 RUN_B=2 LIMIT=30 MIN_COUNT=5 SHARED_ONLY=--shared-only
make stats-compare-words-content RUN_A=1 RUN_B=2 LIMIT=30 MIN_COUNT=5 EXCLUSIVE_ONLY=--exclusive-only
```

Comparisons are lexical. If two runs use different languages, CorpusLens prints a note and does not translate equivalent concepts.

Difficulty:

```powershell
make stats-difficulty RUN=1
make stats-compare-difficulty RUN_A=1 RUN_B=2
```

Difficulty is a relative heuristic based on sentence length, word length, long-word share, content-word share and lexical diversity. It is useful for comparing similar corpora, not as an absolute reading-grade formula.

Collocations:

```powershell
make stats-collocations RUN=1 WORD="whale" WINDOW=4 LIMIT=30
make stats-collocations-content RUN=1 WORD="whale" WINDOW=4 LIMIT=30 MIN_COUNT=3
make stats-collocations-function RUN=1 WORD="love" WINDOW=4 LIMIT=30 MIN_COUNT=3
```

Collocations are ranked with a lightweight Dice score while still showing raw counts. Use `MIN_COUNT` and `MIN_DICE` to hide weak low-frequency matches.

Phrases:

```powershell
make stats-phrases RUN=1 MIN_N=2 MAX_N=5 MIN_COUNT=3 LIMIT=30
make stats-phrases-content-boundary RUN=1 MIN_N=2 MAX_N=5 MIN_COUNT=3 MIN_CHAPTERS=2 LIMIT=30
make stats-phrases-content-boundary RUN=1 MIN_N=2 MAX_N=5 MIN_COUNT=3 LONGEST_ONLY=--longest-only LIMIT=30
```

Import diagnostics:

```powershell
make inspect-run RUN=1
```

## Direct CLI examples

```powershell
dotnet run --project src/CorpusLens.Cli -- corpus create "English Literature" --language en --db ./data/corpuslens.db

dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books/en --language en --corpus "English Literature" --db ./data/corpuslens.db --out ./artifacts/en

dotnet run --project src/CorpusLens.Cli -- stats word-books 1 "whale" --limit 30 --db ./data/corpuslens.db

dotnet run --project src/CorpusLens.Cli -- stats compare-words 1 2 --content-only --min-count 5 --limit 30 --db ./data/corpuslens.db

dotnet run --project src/CorpusLens.Cli -- stats difficulty 1 --db ./data/corpuslens.db
```

## Development

```powershell
dotnet restore
dotnet build
dotnet test
```

or simply:

```powershell
make check
```

## Documentation

- `docs/technical-design.md`
- `docs/analysis-rules.md`
- `docs/roadmap.md`
