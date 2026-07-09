# Milestone 3 — Persistenza statistiche principali

Stato: implementata nello zip `CorpusLens_Milestone3_statistics_persistence.zip`.

## Obiettivo

La milestone 2 salvava nel database corpus, libro, capitoli estratti e riepilogo della run di analisi.

La milestone 3 aggiunge la persistenza delle statistiche principali, così CorpusLens può interrogare il database senza dover rileggere i CSV prodotti nel filesystem.

## Tabelle aggiunte

```text
WordStatistic
NGramStatistic
NextWordStatistic
SentenceCategoryStatistic
```

## Dati salvati

Per ogni `AnalysisRun` vengono salvati:

- parole più frequenti;
- n-grammi;
- parole successive;
- categorie frase aggregate.

Le statistiche restano collegate a:

```text
AnalysisRunId
CorpusId
BookId
```

Questo permette query future sia per singola run sia per corpus/libro.

## Comandi CLI aggiunti

Dopo aver analizzato un EPUB con `--corpus`, il comando restituisce un `Run Id`.

Esempi:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats words 1 --limit 25

dotnet run --project src/CorpusLens.Cli -- stats ngrams 1 --n 3 --limit 25

dotnet run --project src/CorpusLens.Cli -- stats next 1 --word alice --limit 25

dotnet run --project src/CorpusLens.Cli -- stats categories 1
```

Con database esplicito:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats words 1 --db ./data/test.db --limit 25
```

## Scelte tecniche

Le statistiche vengono salvate nella stessa transazione della `AnalysisRun`.

Se il salvataggio di una statistica fallisce, viene annullata tutta la run. Questo evita run parziali nel database.

Il database continua a essere inizializzato con `CREATE TABLE IF NOT EXISTS`, quindi un database creato nella milestone 2 viene aggiornato automaticamente alla prima operazione.

## Non obiettivi

Questa milestone non salva ancora:

- ogni singolo token;
- ogni singola frase;
- offset puntuali nel testo;
- KWIC;
- confronti tra corpus.

Questi elementi saranno gestiti in milestone successive.
