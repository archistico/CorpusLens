# Milestone 17.2 — KWIC da token index

Questa milestone migra la ricerca KWIC al token index persistente introdotto nella milestone 17.1.

## Obiettivo

`stats kwic` usa `TokenOccurrence` quando la run è indicizzata. Se la run è stata creata prima dell'introduzione del token index, oppure non ha righe in `TokenOccurrence`, il comando continua a usare il vecchio fallback basato su `Chapter.CleanText`.

## Comportamento

- run con token index: ricerca occorrenze tramite `TokenOccurrence.NormalizedToken` e `RunPosition`;
- ricostruzione dei frammenti KWIC tramite offset del token e `Chapter.CleanText`;
- run legacy senza token index: fallback automatico al metodo precedente;
- output CLI invariato.

## Perché è utile

La query KWIC non deve più riesaminare tutti i capitoli per trovare la parola target quando l'indice esiste. Questo prepara le milestone successive, in cui anche collocazioni e phrase mining potranno usare il token index.

## Note

Il token index attuale contiene solo token parola. Punteggiatura e token strutturali non sono ancora indicizzati; per questo la ricostruzione del contesto leggibile usa ancora il testo pulito del capitolo.
