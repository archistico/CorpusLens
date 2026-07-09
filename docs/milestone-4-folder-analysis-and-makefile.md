# Milestone 4 — Folder analysis and Makefile

## Obiettivo

Questa milestone aggiunge due funzioni operative:

1. un `Makefile` con i comandi più ricorrenti;
2. un comando CLI per analizzare tutti gli EPUB presenti in una cartella come un'unica analisi aggregata.

L'obiettivo è rendere più comodo il ciclo quotidiano:

```text
restore → build → test → crea corpus → analizza libri → consulta statistiche
```

## Nuovo comando CLI

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books --language en --out ./artifacts/books
```

Con salvataggio nel database:

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books --language en --corpus "English Literature" --out ./artifacts/books
```

Scansione ricorsiva:

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books --language en --recursive --out ./artifacts/books
```

## Comportamento

Il comando:

1. cerca i file `.epub` nella cartella indicata;
2. li ordina per percorso;
3. importa ogni EPUB;
4. considera ogni EPUB come un documento distinto;
5. produce una singola analisi aggregata;
6. genera `extracted_text.txt`, `report.md`, `words.csv`, `ngrams.csv`, `next_words.csv`;
7. se viene passato `--corpus`, salva la run aggregata in SQLite.

Questa scelta è importante perché le statistiche `DocumentCount` diventano più significative: indicano in quanti libri compare una parola o un n-gramma.

## Makefile

Target principali:

```powershell
make restore
make build
make test
make check
make demo
make corpus-create CORPUS="English Literature" LANG=en
make corpus-list
make analyze-book BOOK=./books/alice.epub OUT=./artifacts/alice CORPUS="English Literature"
make analyze-books BOOKS=./books OUT=./artifacts/books CORPUS="English Literature"
make analyze-books-recursive BOOKS=./books OUT=./artifacts/books CORPUS="English Literature"
make stats-words RUN=1 LIMIT=25
make stats-ngrams RUN=1 LIMIT=25
make stats-trigrams RUN=1 N=3 LIMIT=25
make stats-next RUN=1 WORD=alice LIMIT=25
make stats-categories RUN=1
```

## Limite noto

Per ora la run aggregata salvata in SQLite viene rappresentata come un singolo record `Book` sintetico del tipo:

```text
EPUB folder: books
```

I singoli EPUB sono comunque usati come documenti distinti nell'analisi statistica. In una milestone futura si potrà salvare anche una tabella esplicita `BookSet` o collegare una singola `AnalysisRun` a più libri reali.
