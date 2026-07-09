# Milestone 5 — Stopword profiles e content/function words

Questa milestone aggiunge una classificazione leggera delle parole in:

- **function words**: stopword/parole funzionali, come articoli, pronomi, preposizioni, ausiliari;
- **content words**: parole contenuto, utili per vedere lessico, temi e dominio del corpus.

Le stopword non vengono eliminate. Vengono salvate e restano interrogabili.

## Funzioni aggiunte

- profili stopword iniziali per `en`, `it`, `fr`, `de`;
- proprietà `IsStopWord` su `WordFrequency`;
- colonna `IsStopWord` in `WordStatistic`;
- migrazione automatica per database esistenti;
- `words.csv` con colonna `is_stop_word`;
- report Markdown con sezioni `Top content words` e `Top function words`;
- CLI `stats words` con `--content-only` e `--function-only`;
- Makefile con `stats-content` e `stats-function`.

## Comandi

```powershell
make stats-words RUN=1 LIMIT=25
make stats-content RUN=1 LIMIT=25
make stats-function RUN=1 LIMIT=25
```

Oppure direttamente:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats words 1 --content-only --limit 25 --db ./data/corpuslens.db
dotnet run --project src/CorpusLens.Cli -- stats words 1 --function-only --limit 25 --db ./data/corpuslens.db
```

## Nota

La lista stopword è volutamente interna e semplice. In futuro potrà diventare configurabile per corpus o caricabile da file.
