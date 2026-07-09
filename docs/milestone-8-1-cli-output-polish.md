# Milestone 8.1 — CLI output polish

## Obiettivo

Migliorare la leggibilità dell'output console senza modificare il modello dati, le statistiche salvate o la logica di analisi.

Questa milestone è volutamente piccola e non introduce nuove funzionalità linguistiche.

## Correzioni incluse

### Probabilità in percentuale

Prima le probabilità molto piccole venivano formattate con due decimali come valori decimali.

Esempio:

```text
0
```

Ora vengono stampate come percentuali con due decimali:

```text
0.48%
```

Questo riguarda:

```text
stats word <runId> <word>
stats next <runId>
stats next <runId> --word <word>
```

### KWIC più leggibile

I contesti KWIC ora rimuovono punteggiatura e virgolette ai bordi del contesto.

Esempio sorgente:

```text
The old man said: “I have a view,” and smiled.
```

Prima il contesto destro poteva iniziare così:

```text
: “I have a view
```

Ora viene mostrato così:

```text
I have a view
```

## Non obiettivi

Questa milestone non modifica:

- tokenizzazione;
- sentence splitter;
- database;
- CSV;
- report Markdown;
- calcolo delle probabilità.

Cambia solo la presentazione CLI e la pulizia visuale dei contesti KWIC.

## Verifiche consigliate

```powershell
make stats-word RUN=1 WORD="don't" LIMIT=10
make stats-next RUN=1 WORD="don't" LIMIT=10
make stats-kwic RUN=1 WORD="said" LIMIT=10 CONTEXT=8
```

## Criteri di accettazione

- le probabilità non appaiono più come `0` quando sono piccole;
- `stats word` mostra percentuali leggibili;
- `stats next` mostra percentuali leggibili;
- KWIC non mostra più `:`, `,`, virgolette o parentesi come primo carattere del contesto destro;
- i test esistenti continuano a passare.
