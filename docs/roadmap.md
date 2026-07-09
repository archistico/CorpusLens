# CorpusLens — Roadmap operativa

## Milestone 0 — Setup e primo motore testabile

Stato: incluso in questo zip.

Obiettivi:

- solution .NET;
- progetti separati;
- primi test;
- CLI minimale;
- report Markdown/CSV su testo semplice.

Criteri di completamento:

- `dotnet build` senza errori;
- `dotnet test` senza errori;
- `demo` genera report e CSV.

## Milestone 1 — Import EPUB

Obiettivi:

- integrare lettura EPUB;
- estrarre metadata base;
- estrarre capitoli HTML/XHTML;
- convertire HTML in testo pulito;
- collegare l'import al motore già esistente.

Criteri di completamento:

- almeno un EPUB public domain importato;
- testo pulito leggibile;
- report generato da EPUB;
- test su EPUB campione.

## Milestone 2 — Persistenza SQLite

Obiettivi:

- salvare corpus, libri, capitoli, frasi e token;
- salvare analysis run;
- rendere riproducibili gli output.

## Milestone 3 — Analisi linguistica estesa

Obiettivi:

- parole precedenti;
- frasi ripetute;
- categorie frase più robuste;
- report più leggibili.

## Milestone 4 — Confronto corpora

Obiettivi:

- confrontare vocabolario;
- parole distintive;
- n-grammi distintivi;
- indicatori di difficoltà relativa.

## Milestone 5 — UI

Obiettivi:

- interfaccia desktop Avalonia;
- import guidato;
- dashboard;
- navigazione risultati.
