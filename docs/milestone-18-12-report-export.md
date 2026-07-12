# Milestone 18.12 — Report ed export

## Obiettivo

Rendere consultabili dalla UI desktop gli artefatti già generati da una run, senza rigenerarli, modificarli o duplicare l'accesso SQLite nel progetto Avalonia.

## Artefatti gestiti

Percorsi persistiti nella tabella `AnalysisRun`:

* `report.md`;
* `words.csv`;
* `ngrams.csv`;
* `next_words.csv`;
* `extracted_text.txt`.

Artefatti opzionali rilevati nella cartella output:

* `import_diagnostics.md`;
* `import_failures.csv`.

I due file diagnostici non hanno ancora una colonna dedicata nello schema e vengono quindi individuati per nome quando la cartella output può essere risolta.

## Stati di disponibilità

* **Available**: il percorso risolto esiste e può essere aperto;
* **Missing**: la run contiene un percorso non vuoto, ma il file non esiste più;
* **Not generated**: il percorso non è stato registrato oppure l'artefatto opzionale non è presente nella cartella output.

Questa distinzione impedisce di presentare come “mai generato” un export che risulta invece registrato nel database e successivamente rimosso o spostato.

## Risoluzione dei percorsi

Per un percorso assoluto viene usato il valore normalizzato. Per un percorso relativo vengono valutate, in ordine, le seguenti basi:

1. cartella di lavoro corrente;
2. cartella padre della directory che contiene il database, utile per il layout `data/` + `artifacts/`;
3. directory che contiene il database.

Il primo file esistente determina il percorso effettivo. Se nessun candidato esiste, il primo percorso normalizzato resta visibile come percorso atteso e viene marcato `Missing`.

## Architettura

```text
MainWindow
  -> MainWindowViewModel
    -> ArtifactExplorerViewModel
      -> ArtifactExplorerQueryService
        -> AnalysisRunQueryService.GetRunAsync
          -> SqliteCorpusStore.GetAnalysisRunAsync
```

L'apertura del percorso è delegata a `SystemPathLauncher`, che:

* accetta separatamente file e directory;
* normalizza il percorso;
* verifica l'esistenza prima dell'apertura;
* usa `ProcessStartInfo.UseShellExecute` per l'applicazione predefinita;
* non concatena comandi shell;
* non modifica il file.

## UI

Il pannello mostra:

* riepilogo dei conteggi per stato;
* elenco selezionabile degli artefatti;
* dettagli del percorso registrato e risolto;
* stato e spiegazione diagnostica;
* cartella output risolta;
* azioni **Open selected**, **Open output folder** e **Refresh availability**.

## Test

I test coprono:

* lettura completa della run;
* risoluzione di percorsi relativi rispetto alla cartella padre del database;
* distinzione `Available` / `Missing` / `NotGenerated`;
* rilevamento dei diagnostici opzionali;
* selezione automatica del primo artefatto disponibile;
* apertura tramite launcher iniettato, senza avviare processi reali durante i test;
* rifiuto dell'apertura di un file mancante.

## Criteri di accettazione

* ogni artefatto esistente può essere aperto dalla UI;
* i file mancanti non provocano crash;
* un percorso registrato ma rimosso è distinto da un artefatto non generato;
* l'apertura usa l'applicazione predefinita del sistema operativo;
* nessun file esportato viene modificato;
* build e test completati senza errori.
