# CorpusLens — Regole di analisi iniziali

## Normalizzazione

- converte spazi speciali in spazi normali;
- normalizza apostrofi tipografici;
- normalizza virgolette;
- normalizza trattini lunghi;
- riduce spazi multipli.

## Segmentazione frasi

La prima versione separa le frasi su:

- punto;
- punto interrogativo;
- punto esclamativo.

Sono previste alcune abbreviazioni inglesi comuni per evitare tagli troppo evidenti.

## Tokenizzazione

Tipi token:

- `Word`;
- `Number`;
- `Punctuation`;
- `Symbol`;
- `Other`.

Le parole possono contenere apostrofi interni, quindi `don't` resta un token unico nella prima versione.

## N-grammi

Gli n-grammi sono calcolati solo sui token di tipo `Word` e non attraversano il confine di frase.

## Classificazione frasi

La classificazione iniziale è rule-based e prudente:

- `Question`;
- `Greeting`;
- `Negation`;
- `Request`;
- `Exclamation`;
- `Statement`;
- `Other`.

Le categorie sono da considerare indicative, non linguisticamente perfette.

## Token index and query paths

Saved SQLite runs create a persistent `TokenOccurrence` index for word tokens.

When the index is available, these queries use token positions:

- `stats kwic`
- `stats collocations`
- `stats phrases`
- `stats word-books`

When the index is missing, the same commands fall back to stored chapter text. This keeps legacy databases readable.

Use:

```powershell
make stats-health RUN=1
make stats-token-index RUN=1
```

to check whether a run is indexed and which query path will be used.
