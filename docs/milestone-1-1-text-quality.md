# CorpusLens — Milestone 1.1 Text quality

Versione: 0.1.1

## Obiettivo

Migliorare la qualità del testo estratto dagli EPUB prima dell'analisi statistica.

Questa micro-milestone non introduce SQLite e non cambia il motore di analisi. Interviene solo sulla pulizia del testo EPUB e sull'ordinamento degli export.

## Modifiche

- Rimozione del boilerplate Project Gutenberg già presente nella Milestone 1.
- Rimozione del front matter iniziale quando contiene un indice duplicato prima del vero primo capitolo.
- Esclusione di file EPUB che sembrano solo indici o table of contents.
- Ordinamento di `next_words.csv` per conteggio decrescente, coerente con il report Markdown.

## Caso Alice

Nel file `alice.epub` il testo iniziale conteneva:

```text
Alice's Adventures in Wonderland
by Lewis Carroll
THE MILLENNIUM FULCRUM EDITION 3.0
Contents
CHAPTER I.
...
CHAPTER XII.
...
CHAPTER I.
Down the Rabbit-Hole
Alice was beginning...
```

La parte prima del secondo `CHAPTER I.` è front matter/indice e non deve incidere sulle statistiche linguistiche.

Dopo questa micro-milestone il testo estratto deve iniziare dal vero primo capitolo.

## Criteri di verifica

- `dotnet build` senza errori.
- `dotnet test` senza errori.
- Analizzando Alice, `extracted_text.txt` non deve più iniziare con `Contents` o con l'elenco duplicato dei capitoli.
- In `words.csv`, parole come `millennium`, `fulcrum`, `edition` non devono più comparire se provenivano solo dal front matter.
- In `ngrams.csv`, i bigrammi `chapter i`, `chapter ii`, ecc. non devono più essere gonfiati dall'indice duplicato.
- `next_words.csv` deve essere ordinato per `count` decrescente.
