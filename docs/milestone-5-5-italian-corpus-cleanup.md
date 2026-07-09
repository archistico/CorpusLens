# Milestone 5.5 — Italian corpus cleanup

## Obiettivo

Migliorare la prima analisi su corpus italiano, soprattutto per EPUB provenienti da Liber Liber / Progetto Manuzio.

## Problemi osservati

Dall'analisi di `books/it` sono emersi due aspetti:

- alcuni EPUB sono corrotti o incompleti e vengono correttamente saltati;
- molti EPUB validi contengono front matter Liber Liber, licenze, indice, metadati editoriali e note tecniche che entrano ancora nelle statistiche.

Esempi di parole spurie osservate:

```text
liber
liberliber
licenza
indice
www
https
note
```

## Modifiche

### Pulizia Liber Liber

`EpubBoilerplateCleaner` ora riconosce e rimuove un blocco iniziale tipico Liber Liber quando contiene marker come:

```text
Informazioni
QUESTO E-BOOK:
LICENZA:
Liber Liber
Indice
```

La rimozione cerca poi il primo inizio narrativo plausibile, ad esempio:

```text
I
NELLA CONIGLIERA
Alice cominciava...
```

oppure marker come:

```text
CAPITOLO I.
PARTE PRIMA
```

### Stopword italiane

Il profilo italiano è stato ampliato con parole funzione che erano emerse erroneamente tra le content words:

```text
i
è
me
egli
quel
fu
```

e altre forme di pronomi, dimostrativi e verbi ausiliari frequenti.

## Criteri di accettazione

- `dotnet build` passa;
- `dotnet test` passa;
- `make analyze-it` continua anche se alcuni EPUB sono corrotti;
- `import_failures.csv` elenca gli EPUB non importabili;
- `extracted_text.txt` non inizia più con front matter Liber Liber;
- `stats-content` non mostra più `i` ed `è` tra le prime parole contenuto;
- parole come `liber`, `liberliber`, `licenza`, `indice`, `www`, `https` calano sensibilmente o spariscono dalle statistiche principali.
