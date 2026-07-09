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
dotnet run --project src/CorpusLens.Cli -- analyze-epub ./books/en/alice.epub --language en --out ./artifacts/alice
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


## Language-specific book folders

The recommended local structure is now language-based:

```text
books/
├── en/
└── it/
```

Use `books/en` for English EPUB files and `books/it` for Italian EPUB files. This avoids accidentally mixing languages in the same corpus analysis.

Create the folders from PowerShell with:

```powershell
make setup-books
```

Typical English workflow:

```powershell
make check
make corpus-create-en
make analyze-en
make stats-runs LIMIT=10
make stats-summary RUN=1
make stats-content RUN=1 LIMIT=30
```

Typical Italian workflow:

```powershell
make check
make corpus-create-it
make analyze-it
make stats-runs LIMIT=10
make stats-summary RUN=1
make stats-content RUN=1 LIMIT=30
make stats-function RUN=1 LIMIT=30
```

If both corpora are needed in the same database, do not run `make check` between the two analyses because `make check` removes `./data`. Instead use:

```powershell
make clean
make corpus-create-en
make corpus-create-it
make analyze-en
make analyze-it
make stats-runs LIMIT=10
```

The generic targets still work and can be overridden explicitly:

```powershell
make analyze-books BOOKS=./books/it LANG=it CORPUS="Italian Literature" OUT=./artifacts/it
```

## Milestone 4 — Makefile and folder analysis

This version adds a root `Makefile` with the most common development and analysis commands.

`make check` is intended for the local development database: it removes `./data` and `./artifacts` before restore/build/test, so repeated runs start from a clean state. Use `make clean-data` or `make clean` when you want to reset the local SQLite database explicitly.

```powershell
make check
make clean-data
make clean
make corpus-create-en
make corpus-create-it
make corpus-list
make analyze-book BOOK=./books/en/alice.epub OUT=./artifacts/alice CORPUS="English Literature" LANG=en
make analyze-en
make analyze-it
make stats-words RUN=1 LIMIT=25
```

CorpusLens can also analyze all EPUB files in a folder as a single aggregate corpus run:

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books/en --language en --out ./artifacts/en
```

To save the aggregate run in SQLite:

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books/en --language en --corpus "English Literature" --out ./artifacts/en
```

Recursive folder scan:

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books/en --language en --recursive --out ./artifacts/en
```

The aggregate analysis treats each EPUB as a separate document, so `DocumentCount` in word and n-gram statistics represents the number of books in which the item appears.

### Corrupt or invalid EPUB files

When analyzing a folder, CorpusLens now skips EPUB files that cannot be read and continues with the valid files. This is useful for mixed folders downloaded from public-domain sources, where one file may be damaged or may not be a real EPUB.

The console output shows the number of skipped files and lists their names. The output folder also contains:

```text
import_failures.csv
```

Example:

```powershell
make analyze-it
```

If one Italian EPUB is corrupt, the command still analyzes the readable EPUB files and writes the failure details to:

```text
artifacts/it/import_failures.csv
```

If all EPUB files fail, the command stops with an error because there is no valid corpus to analyze.

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


## Stopword e parole contenuto

CorpusLens non elimina le stopword. Le classifica come parole funzionali, così puoi vedere sia le frequenze complete sia il lessico più informativo del corpus.

```powershell
make stats-words RUN=1 LIMIT=25
make stats-content RUN=1 LIMIT=25
make stats-function RUN=1 LIMIT=25
```

La colonna `Type` della CLI distingue `content` e `function`. Nel CSV `words.csv` viene esportata anche la colonna `is_stop_word`.


## Word detail lookup

After saving an analysis run to SQLite, inspect one word with its main statistics, following words and previous words:

```powershell
make stats-word RUN=1 WORD=alice LIMIT=25
make stats-kwic RUN=1 WORD=alice LIMIT=10 CONTEXT=8
make inspect-run RUN=1
```

Equivalent CLI command:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats word 1 alice --limit 25 --db ./data/corpuslens.db
dotnet run --project src/CorpusLens.Cli -- stats kwic 1 alice --limit 10 --context 8 --db ./data/corpuslens.db
```

These commands do not recompute the analysis. They read the already persisted statistics and clean chapter text from SQLite.

## Italian EPUB cleanup note

Italian EPUBs from Liber Liber / Progetto Manuzio may contain front matter such as metadata, license text, donation notes and table of contents pages. CorpusLens includes a first cleanup pass for common Liber Liber markers. If `make analyze-it` produces `artifacts/it/import_failures.csv`, inspect that file: invalid EPUBs are skipped so the valid books can still be analyzed.


## Import diagnostics

Durante l'analisi di una cartella EPUB CorpusLens genera anche:

```text
import_diagnostics.md
```

Il file segnala import falliti, capitoli sospetti, capitoli molto corti/lunghi e possibili residui di boilerplate come Gutenberg, Liber Liber, licenze, indici e copyright.

Puoi rigenerare la diagnostica da una run salvata:

```powershell
make inspect-run RUN=1
```

## English tokenizer notes

CorpusLens keeps common English contractions and possessives as single word tokens in the current analysis model.

Examples:

```text
don't
I'm
I'll
won't
couldn't
Alice's
Queen's
```

The tokenizer normalizes typographic apostrophes to plain apostrophes for statistics, so `Alice’s` and `Alice's` are counted together as `alice's`.

Useful checks:

```powershell
make stats-word RUN=1 WORD="don't" LIMIT=25
make stats-kwic RUN=1 WORD="don't" LIMIT=10 CONTEXT=8
make stats-word RUN=1 WORD="alice's" LIMIT=25
make stats-kwic RUN=1 WORD="alice's" LIMIT=10 CONTEXT=8
```


## Milestone 8 — Sentence splitter migliorato

Il sentence splitter gestisce meglio narrativa inglese e dialoghi:

- abbreviazioni come `Mr.`, `Mrs.`, `Dr.`, `Prof.`, `e.g.`, `i.e.`;
- numeri decimali come `3.14`;
- dialoghi con attribuzione successiva, ad esempio `"Who are you?" said the Caterpillar.`;
- titoli come `CHAPTER I.` come unità autonoma.

Dettagli: `docs/milestone-8-sentence-splitter.md`.

## Milestone 8.1 — CLI output polish

Questa micro-milestone migliora solo la leggibilità dell'output CLI.

Le probabilità nelle viste `stats word` e `stats next` sono ora stampate come percentuali con due decimali, per evitare valori apparentemente pari a `0` quando la probabilità è piccola.

Esempio:

```text
0.48%
```

Il KWIC rimuove inoltre la punteggiatura ai bordi dei contesti sinistro/destro. Per esempio, un caso come:

```text
said: “I have a view”
```

viene mostrato come contesto destro:

```text
I have a view
```

invece di:

```text
: “I have a view
```

Dettagli: `docs/milestone-8-1-cli-output-polish.md`.

## Run source books

When an EPUB folder is analyzed with `--corpus`, CorpusLens stores the aggregate run and also links the run to the real EPUB books included in the folder.

List the real books behind a run:

```powershell
make stats-books RUN=1
```

or directly:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats books 1 --db ./data/corpuslens.db
```


## Word distribution by source book

For aggregate EPUB-folder runs, inspect where a word occurs across the real source books:

```powershell
make stats-word-books RUN=1 WORD="whale" LIMIT=30
make stats-word-books RUN=1 WORD="alice" LIMIT=30
make stats-word-books RUN=1 WORD="don't" LIMIT=30
```

Equivalent raw command:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats word-books 1 "whale" --limit 30 --db ./data/corpuslens.db
```

This reads stored clean chapter text and shows only books where the word appears. The command always prints source books, matched books, coverage, total count and shown books. Details: `docs/milestone-10-word-book-distribution.md` and `docs/milestone-10-1-word-books-output-consistency.md`.
