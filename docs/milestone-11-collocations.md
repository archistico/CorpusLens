# Milestone 11 — Collocazioni

Questa milestone introduce una prima query operativa per le collocazioni.

## Obiettivo

Dato un `AnalysisRunId` e una parola target, CorpusLens calcola quali parole compaiono più spesso vicino alla target entro una finestra configurabile.

Esempio:

```powershell
make stats-collocations RUN=1 WORD="whale" WINDOW=4 LIMIT=30
```

Comando diretto:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats collocations 1 "whale" --window 4 --limit 30 --db ./data/corpuslens.db
```

## Metodo

Per ogni occorrenza della parola target, il comando guarda:

```text
N parole a sinistra + parola target + N parole a destra
```

Poi conta ogni parola collocata nella finestra.

La query usa il testo pulito dei capitoli già salvato nel database. Non introduce ancora un indice persistente dei token.

## Output

Il comando mostra:

- collocato;
- conteggio totale;
- conteggio a sinistra della target;
- conteggio a destra della target;
- occorrenze medie per occorrenza della target (`Per target`);
- distanza media dalla target.

Esempio indicativo:

```text
Collocations for 'whale' in run 1
Window: 4 words per side

#    Collocate            Count  Left  Right  Per target  Avg distance
---  -------------------  -----  ----  -----  ----------  ------------
  1  white                   52    12     40        0.05          1.42
  2  sperm                   48    31     17        0.05          1.81
```

## Limiti intenzionali

Questa è una prima collocation query semplice.

Non fa ancora:

- PMI;
- log-likelihood;
- Dice coefficient;
- filtro content/function words;
- collocazioni persistite in tabella;
- token index persistente.

Questi passi restano previsti per milestone successive.

## Follow-up — Milestone 11.1

Milestone 11.1 adds output filtering:

```powershell
make stats-collocations-content RUN=1 WORD="whale" WINDOW=4 LIMIT=30
make stats-collocations-function RUN=1 WORD="love" WINDOW=4 LIMIT=30
```

Use `--content-only` to inspect lexical collocates and `--function-only` to inspect grammatical patterns.
