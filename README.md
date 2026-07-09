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
