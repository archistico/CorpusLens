# Milestone 8 — Sentence splitter migliorato

## Obiettivo

Migliorare la segmentazione delle frasi, soprattutto per narrativa inglese con dialoghi, abbreviazioni e titoli.

## Casi gestiti

- abbreviazioni comuni inglesi: `Mr.`, `Mrs.`, `Ms.`, `Dr.`, `Prof.`, `St.`, `Jr.`, `Sr.`, `e.g.`, `i.e.`, `etc.`;
- abbreviazioni puntate come `e.g.` e `i.e.` senza spezzare dopo la prima lettera;
- numeri decimali come `3.14`;
- dialoghi con attribuzione successiva, ad esempio `"Who are you?" said the Caterpillar.`;
- esclamazioni con attribuzione successiva, ad esempio `"Oh dear!" cried Alice.`;
- titoli tipo `CHAPTER I.` lasciati come unità autonoma.

## Scelte progettuali

La milestone non introduce ancora POS tagging, lemmatizzazione o analisi grammaticale. Rimane una regola euristica semplice e testabile.

Le frasi dialogate con attribuzione in minuscolo vengono tenute insieme:

```text
"Who are you?" said the Caterpillar.
```

Se dopo la chiusura del dialogo inizia una nuova frase con maiuscola, la frase viene invece separata.

## Test

Sono stati aggiunti test per:

- abbreviazioni prima di nomi;
- abbreviazioni puntate;
- dialoghi interrogativi;
- dialoghi esclamativi;
- titoli di capitolo;
- numeri decimali.
