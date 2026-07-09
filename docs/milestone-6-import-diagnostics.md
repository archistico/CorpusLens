# Milestone 6 — Import diagnostics

Questa milestone aggiunge un report diagnostico dell'import EPUB. Il report non modifica l'analisi linguistica: serve a capire se i testi importati sono puliti o se contengono ancora front matter, indici, licenze, copyright, boilerplate Gutenberg/Liber Liber o capitoli sospetti.

## Output generati

Durante `analyze-epub-folder` viene generato:

```text
import_diagnostics.md
```

insieme a:

```text
extracted_text.txt
report.md
words.csv
ngrams.csv
next_words.csv
import_failures.csv
```

## Nuovo comando CLI

È possibile generare una diagnostica anche da una run già salvata nel database:

```powershell
dotnet run --project src/CorpusLens.Cli -- inspect run 1 --db ./data/corpuslens.db
```

Output personalizzato:

```powershell
dotnet run --project src/CorpusLens.Cli -- inspect run 1 --out ./artifacts/diagnostics/import_diagnostics.md --db ./data/corpuslens.db
```

## Makefile

```powershell
make inspect-run RUN=1
```

Percorso personalizzato:

```powershell
make inspect-run RUN=1 DIAGNOSTICS_OUT=./artifacts/it/import_diagnostics_from_db.md
```

## Contenuto del report

Il report contiene:

- riepilogo libri/capitoli/errori;
- capitoli vuoti;
- capitoli corti;
- capitoli molto lunghi;
- termini sospetti da boilerplate;
- capitoli sospetti;
- tabella dei capitoli più corti;
- tabella dei capitoli più lunghi;
- errori di importazione EPUB.

## Termini sospetti iniziali

Esempi:

```text
project gutenberg
gutenberg
copyright
license
licence
donation
ebook
e-book
transcriber
publisher
archive
liber liber
liberliber
licenza
questo e-book
indice
sommario
prefazione
editore
www
https
```

## Criteri di accettazione

- `analyze-epub-folder` genera `import_diagnostics.md`;
- i file EPUB corrotti restano segnalati in `import_failures.csv`;
- `inspect run` genera una diagnostica da una run già salvata;
- il report evidenzia residui di boilerplate senza bloccare l'analisi;
- il report è leggibile in Markdown;
- build e test passano.

## Follow-up: Milestone 6.1

The Italian corpus diagnostics showed that some Liber Liber metadata pages were still imported as standalone chapters. Milestone 6.1 extends `EpubBoilerplateCleaner` to skip metadata-only chapters with Liber Liber/license/e-book markers, without treating normal narrative chapters containing words such as `indice` as front matter.
