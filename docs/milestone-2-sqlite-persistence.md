# Milestone 2 — Persistenza SQLite minima

Versione: 0.2

## Obiettivo

Introdurre una persistenza locale minima per trasformare CorpusLens da semplice pipeline file → report in uno strumento capace di ricordare corpus, libri importati, capitoli estratti e run di analisi.

Questa milestone non salva ancora tutti i token e tutte le statistiche dettagliate nel database. La scelta è intenzionale: prima si stabilizza il modello minimo, poi si amplia.

## Database predefinito

Percorso predefinito:

```text
./data/corpuslens.db
```

È possibile cambiarlo con:

```text
--db <file>
```

## Tabelle create

```text
Corpus
Book
Chapter
AnalysisRun
```

### Corpus

Contiene i corpus creati dall'utente.

Campi principali:

```text
Id
Name
LanguageCode
Description
CreatedAt
UpdatedAt
```

### Book

Contiene i libri EPUB importati in un corpus.

Campi principali:

```text
Id
CorpusId
Title
Author
LanguageCode
OriginalFilePath
FileHash
ImportedAt
Status
ErrorMessage
```

### Chapter

Contiene i capitoli/sezioni testuali estratti dall'EPUB.

Campi principali:

```text
Id
BookId
OrderIndex
Title
SourcePath
CleanText
CharacterCount
```

### AnalysisRun

Contiene il riepilogo di una run di analisi e i percorsi dei report generati.

Campi principali:

```text
Id
CorpusId
BookId
StartedAt
CompletedAt
Status
EngineVersion
SettingsJson
SentenceCount
TokenCount
WordTokenCount
DistinctWordCount
AverageWordsPerSentence
AverageCharactersPerWord
ReportPath
WordsCsvPath
NGramsCsvPath
NextWordsCsvPath
ExtractedTextPath
ErrorMessage
```

## Comandi CLI aggiunti

### Creare un corpus

```powershell
dotnet run --project src/CorpusLens.Cli -- corpus create "English Kids" --language en
```

Con database esplicito:

```powershell
dotnet run --project src/CorpusLens.Cli -- corpus create "English Kids" --language en --db ./data/corpuslens.db
```

### Elencare i corpus

```powershell
dotnet run --project src/CorpusLens.Cli -- corpus list
```

### Analizzare un EPUB e salvarlo nel database

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub ./samples/epubs/alice.epub --language en --corpus "English Kids" --out ./artifacts/alice
```

Il comando continua a produrre i file:

```text
extracted_text.txt
report.md
words.csv
ngrams.csv
next_words.csv
```

In più salva nel database:

```text
Book
Chapter
AnalysisRun
```

## Scelta tecnica

La persistenza usa `Microsoft.Data.Sqlite + SQLitePCLRaw.bundle_e_sqlite3 3.0.3`, senza Entity Framework.

Motivi:

- controllo esplicito dello schema;
- dipendenza leggera;
- SQL visibile e facile da modificare;
- nessuna migrazione complessa nelle prime versioni;
- coerenza con l'obiettivo di non sovraingegnerizzare.

## Cosa non è ancora incluso

Non vengono ancora salvati nel database:

```text
WordStatistic
NGramStatistic
NextWordStatistic
SentenceStatistic completa
TokenOccurrence completa
```

Questi dati restano esportati in CSV. Verranno portati nel database quando il modello minimo sarà stabile.


## Nota test su Windows

Le connessioni SQLite vengono aperte con `Pooling = false` per evitare che i test che usano database temporanei trovino ancora il file `.db` bloccato durante la cancellazione della cartella temporanea.
