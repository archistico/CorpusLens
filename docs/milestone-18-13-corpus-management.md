# Milestone 18.13 — Gestione corpus

## Obiettivo

Consentire la gestione iniziale dei corpus direttamente dall'applicazione Avalonia, senza usare la CLI e senza spostare SQL o regole di persistenza nel progetto Desktop.

## Funzioni introdotte

* elenco dei corpus presenti nel database aperto;
* filtro **All corpora**;
* filtro delle run per corpus selezionato;
* dettaglio con ID, nome, lingua, descrizione, date e numero di run;
* creazione di un nuovo corpus con nome, lingua e descrizione opzionale;
* selezione automatica del corpus appena creato;
* conferma esplicita obbligatoria prima della scrittura;
* messaggi chiari per input vuoti, duplicati e lingue non supportate.

## Lingue supportate

Il catalogo applicativo espone i profili già supportati dal motore:

```text
en  English
it  Italian
fr  French
de  German
```

I codici regionali vengono normalizzati alla lingua base, per esempio:

```text
it-IT -> it
fr_FR -> fr
```

Codici privi di un profilo CorpusLens, come `es`, vengono rifiutati prima di creare o modificare il database.

## Architettura

```text
MainWindow
  -> MainWindowViewModel
    -> CorpusManagementViewModel
      -> ListCorporaUseCase
      -> CreateCorpusUseCase
        -> SqliteCorpusStore
```

Il Desktop non contiene query SQL. Il ViewModel usa funzioni iniettabili per rendere caricamento e creazione verificabili senza database reale.

## Scrittura persistente

La creazione richiede che l'utente selezioni la conferma:

```text
I confirm creation of this corpus in the open database.
```

Anche in presenza della conferma, l'operazione viene bloccata se:

* il database non è stato aperto;
* il nome è vuoto;
* esiste già un corpus con lo stesso nome, senza distinzione tra maiuscole e minuscole;
* la lingua non appartiene al catalogo supportato.

`SqliteCorpusStore.CreateCorpusAsync` esegue l'`INSERT` dentro una transazione esplicita. Il vincolo univoco SQLite resta l'ultima protezione da duplicati concorrenti.

## Run associate

`MainWindowViewModel` mantiene due collezioni:

* `Runs`: insieme completo delle run caricate, usato anche dal confronto tra corpora;
* `VisibleRuns`: sottoinsieme mostrato nel navigatore in base al corpus selezionato.

La selezione **All corpora** ripristina l'elenco completo. Il limite di caricamento desktop è stato aumentato da 100 a 1000 run.

## Coerenza linguistica

`CorpusManagementViewModel.IsSelectedCorpusLanguageCompatible` confronta la lingua del corpus selezionato con una lingua di analisi normalizzata. La Milestone 18.14 userà questo contratto per impedire l'avvio di un'analisi EPUB con lingua incompatibile.

## Rinomina

La rinomina è stata rinviata. Non è necessaria per creare e analizzare corpus e richiederebbe una policy esplicita per aggiornamento, audit e gestione degli errori. In questa milestone le operazioni sono quindi limitate alla lettura e alla creazione append-only.

## Test

I test aggiunti coprono:

* normalizzazione dei codici lingua regionali;
* rifiuto di una lingua non supportata prima della creazione del database;
* caricamento dei corpus e conteggio delle run associate;
* validazione case-insensitive dei nomi duplicati;
* creazione tramite funzione applicativa iniettata;
* selezione automatica del corpus creato;
* obbligo della conferma esplicita nel coordinatore desktop;
* rollback e integrità dei dati in caso di nome duplicato.

## Criteri di accettazione

* un corpus può essere creato dalla UI senza terminale;
* le run possono essere filtrate per corpus;
* nomi vuoti, duplicati e lingue non supportate sono rifiutati;
* nessun SQL è presente nel progetto Desktop;
* la scrittura è transazionale;
* le operazioni asincrone continuano a usare stato busy e cancellazione centralizzati;
* le viste già esistenti continuano a usare la run selezionata senza modifiche alla logica di analisi;
* build e test completati senza errori.
