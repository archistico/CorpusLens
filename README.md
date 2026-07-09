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

## Milestone 1 — EPUB import

La prima importazione EPUB è disponibile tramite CLI:

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub ./books/alice.epub --language en --out ./artifacts/alice
```

Output generati:

```text
extracted_text.txt
report.md
words.csv
ngrams.csv
next_words.csv
```

In questa milestone l'EPUB viene letto con VersOne.Epub, i contenuti HTML/XHTML vengono convertiti in testo con HtmlAgilityPack e poi analizzati con il motore già esistente.


## Milestone 1 fix 2 notes

The EPUB import pipeline now removes Project Gutenberg boilerplate when standard START/END markers are present. The Markdown report also sorts the `Next words` preview by descending count, so the section shows the most relevant transitions instead of only the first alphabetical word group.


### Milestone 1.1 — Text quality

Questa versione aggiunge una pulizia EPUB più robusta per evitare che front matter, indici duplicati e file di navigazione finiscano nelle statistiche linguistiche.

In particolare, per EPUB Project Gutenberg come Alice, il testo analizzato parte dal vero primo capitolo invece che da titolo, indice e note editoriali.

## Milestone 2 — SQLite minima

CorpusLens può ora creare un database SQLite locale per ricordare corpus, libri EPUB importati, capitoli estratti e run di analisi.

Database predefinito:

```powershell
./data/corpuslens.db
```

Crea un corpus:

```powershell
dotnet run --project src/CorpusLens.Cli -- corpus create "English Kids" --language en
```

Elenca i corpus:

```powershell
dotnet run --project src/CorpusLens.Cli -- corpus list
```

Analizza un EPUB e salva importazione/run nel database:

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub ./samples/epubs/alice.epub --language en --corpus "English Kids" --out ./artifacts/alice
```

Per usare un percorso database esplicito aggiungi:

```powershell
--db ./data/corpuslens.db
```

Dettagli tecnici: `docs/milestone-2-sqlite-persistence.md`.


Note SQLite: the project pins SQLitePCLRaw.bundle_e_sqlite3 3.0.3 to avoid restoring the vulnerable SQLitePCLRaw.lib.e_sqlite3 2.1.11 transitive dependency.


## Database statistics commands

After running `analyze-epub` with `--corpus`, use the returned `Run Id` to query persisted statistics:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats words 1 --limit 25
dotnet run --project src/CorpusLens.Cli -- stats ngrams 1 --n 3 --limit 25
dotnet run --project src/CorpusLens.Cli -- stats next 1 --word alice --limit 25
dotnet run --project src/CorpusLens.Cli -- stats categories 1
```

## Milestone 4 — Makefile and folder analysis

This version adds a root `Makefile` with the most common development and analysis commands.

`make check` is intended for the local development database: it removes `./data` and `./artifacts` before restore/build/test, so repeated runs start from a clean state. Use `make clean-data` or `make clean` when you want to reset the local SQLite database explicitly.

```powershell
make check
make clean-data
make clean
make corpus-create CORPUS="English Literature" LANG=en
make corpus-list
make analyze-book BOOK=./books/alice.epub OUT=./artifacts/alice CORPUS="English Literature"
make analyze-books BOOKS=./books OUT=./artifacts/books CORPUS="English Literature"
make stats-words RUN=1 LIMIT=25
```

CorpusLens can also analyze all EPUB files in a folder as a single aggregate corpus run:

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books --language en --out ./artifacts/books
```

To save the aggregate run in SQLite:

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books --language en --corpus "English Literature" --out ./artifacts/books
```

Recursive folder scan:

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books --language en --recursive --out ./artifacts/books
```

The aggregate analysis treats each EPUB as a separate document, so `DocumentCount` in word and n-gram statistics represents the number of books in which the item appears.

### Makefile on Windows

The Makefile is intended to be used from PowerShell on Windows. Cleanup targets use explicit PowerShell commands, so `make check` removes `./data` and `./artifacts` before restoring, building and testing.


## Milestone 4.1 — Run navigation

After saving multiple analyses in SQLite, you can list and inspect analysis runs directly from the CLI:

```powershell
make stats-runs LIMIT=10
make stats-summary RUN=1
```

Equivalent raw commands:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats runs --limit 10 --db ./data/corpuslens.db
dotnet run --project src/CorpusLens.Cli -- stats summary 1 --db ./data/corpuslens.db
```

The stats CLI now formats decimal values with two decimals, so large frequency tables are easier to read.

Details: `docs/milestone-4-1-run-navigation.md`.
