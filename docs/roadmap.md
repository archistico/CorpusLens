# CorpusLens — Roadmap nuove milestone

Versione: 0.2
Stato: pianificazione operativa
Ambito: milestone successive alla persistenza statistiche, analisi cartella EPUB, stopword profile, word detail e KWIC.

---

## 1. Stato attuale del progetto

CorpusLens dispone già di una base funzionante per:

* analisi di singolo file EPUB;
* analisi aggregata di una cartella EPUB;
* estrazione testo pulito da EPUB;
* rimozione boilerplate Project Gutenberg;
* rimozione front matter / indice iniziale nei casi noti;
* analisi TXT;
* report Markdown;
* export CSV;
* persistenza SQLite minima;
* persistenza statistiche principali;
* navigazione delle run salvate;
* summary delle run;
* Makefile per comandi ricorrenti in ambiente Windows/PowerShell;
* prime viste su parole, n-grammi, parole successive e categorie frase;
* stopword profile iniziali;
* distinzione tra parole contenuto e parole funzione;
* dettaglio parola;
* KWIC base;
* gestione EPUB corrotti durante l'analisi cartella tramite skip e `import_failures.csv`.

Le ultime milestone prodotte devono ancora essere consolidate localmente:

```text
Milestone 5   — Stopword profiles
Milestone 5.1 — Word detail
Milestone 5.2 — KWIC contexts
Milestone 5.4 — Invalid EPUB handling
```

La priorità immediata è quindi verificare e stabilizzare queste funzioni prima di introdurre nuove feature strutturali.

---

# Fase 1 — Consolidamento delle funzioni recenti

## Milestone 5-Final — Consolidamento stopword, word detail e KWIC

### Obiettivo

Verificare, correggere e chiudere definitivamente le funzionalità introdotte nelle milestone 5, 5.1 e 5.2.

### Funzioni coinvolte

* classificazione stopword;
* `IsStopWord` in `WordFrequency`;
* salvataggio `IsStopWord` in `WordStatistic`;
* query `stats words --content-only`;
* query `stats words --function-only`;
* comando `stats word`;
* comando `stats kwic`;
* target Makefile collegati.

### Comandi da verificare

```powershell
dotnet restore
dotnet build
dotnet test
```

```powershell
make check
make corpus-create CORPUS="English Literature" LANG=en
make analyze-books BOOKS=./books OUT=./artifacts/books CORPUS="English Literature"
```

```powershell
make stats-words RUN=1 LIMIT=25
make stats-content RUN=1 LIMIT=25
make stats-function RUN=1 LIMIT=25
make stats-word RUN=1 WORD=alice LIMIT=25
make stats-word RUN=1 WORD=said LIMIT=25
make stats-kwic RUN=1 WORD=alice LIMIT=10 CONTEXT=8
make stats-kwic RUN=1 WORD=said LIMIT=10 CONTEXT=8
```

### Criteri di accettazione

* build completata senza errori;
* test completati senza errori;
* `stats-content` mostra parole lessicali utili;
* `stats-function` mostra articoli, pronomi, preposizioni, ausiliari e altre parole funzione;
* `stats-word` mostra frequenza, document count, frequenza per milione, parole successive e parole precedenti;
* `stats-kwic` mostra contesti leggibili;
* `report.md` contiene sezioni distinte per:

  * top words;
  * top content words;
  * top function words;
* `words.csv` contiene la colonna `is_stop_word`;
* database SQLite coerente con la nuova colonna `IsStopWord`.

### Note

Le stopword non devono mai essere eliminate dai dati. Devono solo essere classificate, filtrabili e consultabili.

---

# Fase 2 — Qualità dell’import e del testo estratto

## Milestone 6 — Import diagnostics

### Stato

IMPLEMENTATA nella milestone 6.

### Obiettivo

Aggiungere un report diagnostico per valutare la qualità dell’import EPUB e individuare eventuali residui non linguistici nei dati.

Questa milestone non modifica l’analisi linguistica. Serve a capire se i dati importati sono puliti.

### Nuovi output

```text
artifacts/<run>/import_diagnostics.md
```

### Nuovi comandi ipotizzati

```powershell
dotnet run --project src/CorpusLens.Cli -- inspect run 1 --db ./data/corpuslens.db
```

oppure:

```powershell
make inspect-run RUN=1
```

### Informazioni da includere

* numero libri importati;
* numero capitoli estratti;
* numero documenti analizzati;
* capitoli molto brevi;
* capitoli molto lunghi;
* libri con numero anomalo di capitoli;
* possibili residui Project Gutenberg;
* possibili residui di copyright;
* possibili residui di indice;
* parole sospette da boilerplate;
* dimensione media capitolo;
* capitoli saltati perché riconosciuti come front matter;
* capitoli saltati perché riconosciuti come table of contents;
* elenco dei capitoli più corti;
* elenco dei capitoli più lunghi.

### Parole sospette iniziali

Per inglese:

```text
gutenberg
copyright
license
donation
contents
ebook
publisher
transcriber
archive
```

Per italiano:

```text
indice
copyright
licenza
editore
prefazione
sommario
```

### Criteri di accettazione

* il report diagnostico viene generato senza bloccare l’analisi;
* eventuali residui Project Gutenberg vengono evidenziati;
* eventuali capitoli sospetti vengono elencati;
* l’utente può capire rapidamente se il corpus è pulito;
* nessun testo viene rimosso automaticamente in questa fase, salvo le pulizie già esistenti;
* il report è leggibile in Markdown.

---

## Milestone 7 — Tokenizer inglese migliorato

### Obiettivo

Migliorare la tokenizzazione inglese, soprattutto per contrazioni e possessivi.

### Casi da supportare

```text
don't
doesn't
didn't
can't
couldn't
won't
wouldn't
I'm
I'll
I've
I'd
you're
you'll
he's
she's
it's
Alice's
Queen's
Rabbit's
```

### Decisione iniziale

Nella prima versione migliorata, le contrazioni e i possessivi vengono mantenuti come token singoli normalizzati.

Esempi:

```text
Don't      → don't
I'm        → i'm
Alice's    → alice's
Queen's    → queen's
```

Non si divide ancora in:

```text
do + n't
I + 'm
Alice + 's
```

Questa divisione potrà essere valutata in una fase linguistica più avanzata.

### Componenti coinvolti

* `Tokenizer`;
* `TextNormalizer`;
* `WordFrequencyAnalyzer`;
* `NGramAnalyzer`;
* `StopWordProvider`;
* test del tokenizer.

### Test da aggiungere

```text
"I don't know."
"Alice's sister was reading."
"I'm sure you'll see."
"The Queen's voice was loud."
"He couldn't believe it."
```

### Criteri di accettazione

* `don't` resta una parola;
* `alice's` resta una parola;
* `i'm` resta una parola;
* le contrazioni non vengono spezzate in token inutili;
* i n-grammi con contrazioni sono leggibili;
* `stats-word RUN=1 WORD=don't` funziona;
* `stats-kwic RUN=1 WORD=don't` funziona;
* la modifica non peggiora la tokenizzazione di parole semplici.

---

## Milestone 8 — Sentence splitter migliorato

### Stato

IMPLEMENTATA nella milestone 8.

### Obiettivo

Migliorare la divisione in frasi, soprattutto per dialoghi, abbreviazioni e titoli.

### Casi problematici

```text
"Who are you?" said the Caterpillar.
"Oh dear!" cried Alice.
Mr. Rabbit went away.
Dr. Smith arrived.
Prof. Brown spoke.
CHAPTER I.
CHAPTER II.
e.g. this example
i.e. this explanation
```

### Regole iniziali

Il sentence splitter deve gestire:

* punto finale;
* punto interrogativo;
* punto esclamativo;
* virgolette finali;
* parentesi finali;
* abbreviazioni comuni;
* titoli di capitolo;
* sequenze di dialogo.

### Abbreviazioni inglesi iniziali

```text
Mr.
Mrs.
Ms.
Dr.
Prof.
St.
Jr.
Sr.
e.g.
i.e.
etc.
```

### Criteri di accettazione

* `"Who are you?" said the Caterpillar.` non viene spezzata male;
* `Mr.` non chiude una frase da solo;
* `Dr.` non chiude una frase da solo;
* `CHAPTER I.` può essere riconosciuto come titolo;
* domande con `?"` vengono classificate come `Question`;
* esclamazioni con `!"` vengono classificate come `Exclamation`;
* i test esistenti continuano a passare.

---

# Fase 3 — Modello dati più corretto


## Milestone 8.1 — CLI output polish

Micro-milestone di rifinitura dopo il sentence splitter migliorato.

Obiettivi:

- stampare le probabilità CLI come percentuali leggibili;
- evitare valori apparentemente pari a `0` quando la probabilità è piccola;
- pulire i bordi dei contesti KWIC rimuovendo punteggiatura e virgolette iniziali/finali;
- non modificare database, analisi o CSV.

Criteri di accettazione:

- `stats word` mostra probabilità come `15.02%`;
- `stats next` mostra probabilità come percentuale;
- `stats kwic` non mostra contesti destri che iniziano con `:`, `,`, virgolette o parentesi;
- i test continuano a passare.

## Milestone 9 — Database model v2: run aggregate con libri reali ✅

### Problema attuale

L’analisi aggregata di una cartella EPUB viene salvata nel database come un singolo libro sintetico:

```text
EPUB folder: books
```

Questa soluzione è accettabile per il prototipo, ma limita le query future.

### Obiettivo

Consentire a una `AnalysisRun` di riferirsi a più libri reali.

### Nuove tabelle proposte

```text
AnalysisRunBook
---------------
Id
AnalysisRunId
BookId
OrderIndex
```

### Modello aggiornato

```text
Corpus
Book
Chapter
AnalysisRun
AnalysisRunBook
WordStatistic
NGramStatistic
NextWordStatistic
SentenceCategoryStatistic
```

### Comportamento atteso

Analisi singolo EPUB:

```text
AnalysisRun
  └── 1 Book
```

Analisi cartella EPUB:

```text
AnalysisRun
  ├── Book 1
  ├── Book 2
  ├── Book 3
  └── ...
```

### Vantaggi

Questa struttura permette query future come:

* in quali libri compare questa parola;
* quali parole sono presenti in tutti i libri;
* quali parole sono specifiche di un solo libro;
* confronta libro contro corpus;
* confronta corpus contro corpus;
* mostra distribuzione di una parola tra libri.

### Migrazione

Per database esistenti:

* mantenere compatibilità con run vecchie;
* non rompere `stats summary`;
* se una run non ha righe in `AnalysisRunBook`, usare il vecchio `BookId` come fallback.

### Criteri di accettazione

* analisi singolo EPUB salva un libro reale;
* analisi cartella salva tutti i libri reali;
* `AnalysisRunBook` contiene un record per ogni libro analizzato;
* `stats summary` mostra anche `Books: N`;
* `DocumentCount` continua a essere corretto;
* i report e i CSV non cambiano formato inutilmente;
* i test coprono run singola e run aggregata.

---

# Fase 4 — Query linguistiche avanzate

## Milestone 10 — Query per libro e dispersione

### Obiettivo

Aggiungere interrogazioni per capire se una parola è distribuita in tutto il corpus o concentrata in pochi libri.

### Nuovi comandi ipotizzati

```powershell
make books
make book-summary BOOK=1
make stats-word-books RUN=1 WORD=alice
make stats-dispersion RUN=1 WORD=alice
```

Oppure:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats word-books 1 alice
dotnet run --project src/CorpusLens.Cli -- stats dispersion 1 alice
```

### Output desiderato

Per una parola concentrata in un libro:

```text
Word: alice

Total count: 386
Books: 1 / 26

Book                              Count   Per million
Alice's Adventures in Wonderland   386     ...
Other book                           0     0
```

Per una parola distribuita:

```text
Word: the

Books: 26 / 26
```

### Metriche

* count totale;
* numero libri in cui compare;
* percentuale libri;
* frequenza per milione per libro;
* massimo count in un libro;
* dispersione semplice;
* concentrazione semplice.

### Criteri di accettazione

* mostra count per libro;
* mostra frequenza per milione per libro;
* distingue parole concentrate da parole distribuite;
* funziona con run aggregate;
* funziona anche con run su singolo libro;
* output leggibile da CLI.

---

## Milestone 11 — Collocazioni e associazioni forti

### Obiettivo

Andare oltre le semplici parole successive e individuare associazioni linguistiche più forti.

Le coppie frequenti non sono sempre interessanti. Per esempio:

```text
of the
in the
to the
```

sono frequenti, ma non sempre utili come collocazioni.

### Metriche candidate

* PMI;
* Lift;
* Dice coefficient.

### Scelta iniziale consigliata

Partire con:

```text
Count
Lift
Dice coefficient
```

PMI può essere aggiunta dopo, perché tende a favorire coppie rare se non filtrata bene.

### Nuovi comandi

```powershell
make stats-collocations RUN=1 LIMIT=50
make stats-collocations RUN=1 WORD=said LIMIT=30
```

Oppure:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats collocations 1 --limit 50
dotnet run --project src/CorpusLens.Cli -- stats collocations 1 --word said --limit 30
```

### Filtri minimi

```text
min_count >= 5
min_document_count >= 2
exclude_pairs_with_only_stopwords = true
```

### Output desiderato

```text
Word       Partner      Count   Documents   Lift    Dice
mock       turtle       55      1           ...     ...
white      rabbit       35      1           ...     ...
march      hare         40      1           ...     ...
said       alice        116     1           ...     ...
```

### Criteri di accettazione

* non mostra solo coppie come `of the`, `in the`, `to the`;
* emergono coppie linguisticamente interessanti;
* applica soglia minima di count;
* può filtrare per parola;
* può filtrare content-only;
* produce report leggibile;
* dati esportabili in CSV.

---

## Milestone 12 — Phrase mining base

### Obiettivo

Estrarre espressioni ricorrenti più utili dei semplici n-grammi meccanici.

### Esempi desiderati

```text
I don't know
as soon as
a great deal
one of the
said the king
mock turtle
white rabbit
march hare
```

### Differenza rispetto agli n-grammi

Gli n-grammi sono sequenze grezze. Le phrase devono essere filtrate.

### Regole iniziali

Una phrase candidata:

* ha lunghezza tra 2 e 5 parole;
* ha count minimo;
* non è composta solo da stopword;
* non inizia e finisce con punteggiatura;
* non contiene solo numeri;
* può contenere stopword interne;
* deve avere almeno una parola contenuto, salvo eccezioni configurate.

### Nuovi output

```text
phrases.csv
```

### Nuovi comandi

```powershell
make stats-phrases RUN=1 LIMIT=50
make stats-phrases RUN=1 CONTENT_ONLY=true LIMIT=50
```

Oppure:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats phrases 1 --limit 50
dotnet run --project src/CorpusLens.Cli -- stats phrases 1 --content-only --limit 50
```

### Criteri di accettazione

* le phrase sono leggibili;
* il report non è dominato da frammenti inutili;
* vengono applicate soglie configurabili;
* viene generato `phrases.csv`;
* il report Markdown include una sezione `Top phrases`;
* i test coprono casi semplici e casi da scartare.

---

# Fase 5 — Confronto tra corpora

## Milestone 13 — Corpus comparison v1

### Obiettivo

Confrontare due corpus e individuare differenze lessicali e strutturali.

### Esempi di corpus

```text
English Kids
English Literature
English Mystery
English Technical
Italian Novels
German Technical Manuals
```

### Nuovi comandi

```powershell
dotnet run --project src/CorpusLens.Cli -- compare corpora "English Kids" "English Literature"
```

oppure:

```powershell
make compare CORPUS_A="English Kids" CORPUS_B="English Literature"
```

### Output principali

* parole più distintive del corpus A;
* parole più distintive del corpus B;
* n-grammi più distintivi del corpus A;
* n-grammi più distintivi del corpus B;
* parole comuni ad alta frequenza;
* parole contenuto condivise;
* differenza lunghezza media frase;
* differenza ricchezza lessicale;
* differenza percentuale parole funzione/contenuto.

### Metriche iniziali

Per ogni parola:

```text
CountA
CountB
PerMillionA
PerMillionB
DocumentCountA
DocumentCountB
Difference
Ratio
```

### Output file

```text
compare_report.md
compare_words.csv
compare_ngrams.csv
```

### Criteri di accettazione

* confronta due corpus esistenti;
* genera report Markdown;
* genera CSV;
* distingue parole più tipiche del corpus A e del corpus B;
* non fallisce se una parola esiste solo in uno dei due corpus;
* permette filtro content-only.

---

## Milestone 14 — Difficoltà relativa testi/corpus

### Obiettivo

Creare un profilo di difficoltà relativa, senza pretendere di classificare ufficialmente il livello CEFR.

### Non obiettivo

Non bisogna dire:

```text
Questo corpus è B1.
```

Bisogna dire:

```text
Questo corpus è più semplice/complesso rispetto ad altri corpus analizzati, secondo metriche interne.
```

### Metriche iniziali

* media parole per frase;
* media caratteri per parola;
* numero parole distinte;
* type-token ratio;
* content word ratio;
* function word ratio;
* rare word ratio;
* repeated phrase ratio;
* percentuale domande;
* percentuale esclamazioni;
* percentuale negazioni;
* percentuale richieste;
* densità lessicale.

### Nuovi comandi

```powershell
make stats-difficulty RUN=1
```

oppure:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats difficulty 1
```

### Output desiderato

```text
Corpus difficulty profile

Metric                         Value
Average words per sentence      17.57
Average chars per word           4.34
Distinct words               69,231
Content word ratio              ...
Function word ratio             ...
Rare word ratio                 ...
Questions ratio                 ...
Negations ratio                 ...
```

### Criteri di accettazione

* non produce livelli CEFR fittizi;
* produce metriche chiare;
* confrontabile tra run;
* esportabile in Markdown e CSV;
* utile per scegliere testi più facili o più difficili.

---

# Fase 6 — Multilingua strutturato

## Milestone 15 — Language profiles v2

### Obiettivo

Riorganizzare le regole linguistiche per lingua in profili dedicati.

### Problema attuale

Stopword, domande, negazioni, richieste e regole di tokenizzazione rischiano di essere sparse in componenti diversi.

### Nuova struttura proposta

```text
LanguageProfile
EnglishLanguageProfile
ItalianLanguageProfile
FrenchLanguageProfile
GermanLanguageProfile
```

Ogni profilo deve contenere:

* codice lingua;
* stopword;
* parole interrogative;
* parole di negazione;
* saluti;
* pattern di richiesta;
* abbreviazioni;
* regole tokenizer;
* regole sentence splitter;
* regole normalizzazione apostrofi;
* regole classificazione frasi.

### Lingue iniziali

```text
en
it
fr
de
```

### Criteri di accettazione

* il profilo inglese non rompe l’italiano;
* il profilo italiano gestisce apostrofi come `l'uomo`, `dell'acqua`, `c'è`;
* il profilo tedesco mantiene parole composte;
* il profilo francese gestisce forme come `l'homme`, `qu'il`, `c'est`, `n'est`;
* le regole sono testabili separatamente;
* il codice resta leggibile e non sovraingegnerizzato.

---

## Milestone 16 — Primo corpus italiano

### Obiettivo

Validare CorpusLens su un corpus italiano.

### Tipologia consigliata

Per la prima prova evitare testi tecnici complessi. Meglio:

* narrativa italiana semplice;
* racconti;
* libri di pubblico dominio;
* testi con dialoghi;
* testi con lingua non troppo arcaica, se possibile.

### Casi da verificare

```text
non
che
di
la
il
un
una
l'
dell'
c'è
perché
qual è
com'è
```

### Comandi

```powershell
make corpus-create CORPUS="Italian Literature" LANG=it
make analyze-books BOOKS=./books-it OUT=./artifacts/books-it CORPUS="Italian Literature" LANG=it
make stats-words RUN=2 LIMIT=25
make stats-content RUN=2 LIMIT=25
make stats-function RUN=2 LIMIT=25
```

### Criteri di accettazione

* tokenizzazione italiana accettabile;
* stopword italiane funzionanti;
* negazioni riconosciute;
* domande riconosciute;
* report coerente;
* content words utili;
* KWIC funzionante con apostrofi;
* nessun errore su caratteri accentati.

---

# Fase 7 — Persistenza dettagliata opzionale

## Milestone 17 — Token index persistente

### Obiettivo

Salvare nel database le occorrenze dei token per consentire ricerche più precise e veloci.

### Problema attuale

KWIC cerca nei testi puliti dei capitoli. Funziona, ma è meno preciso rispetto a un indice token.

### Nuova tabella proposta

```text
TokenOccurrence
---------------
Id
AnalysisRunId
BookId
ChapterId
SentenceIndex
TokenIndex
Text
NormalizedText
TokenType
IsStopWord
StartOffset
EndOffset
```

### Vantaggi

Permette:

* KWIC preciso;
* ricerca veloce;
* contesti precedenti/successivi precisi;
* frasi più comuni più affidabili;
* query per frase;
* query per capitolo;
* esportazione Anki;
* future analisi grammaticali.

### Rischio

Su un corpus di 26 EPUB sono già presenti circa:

```text
3.8 milioni di token
```

Il salvataggio è fattibile, ma va progettato con attenzione.

### Requisiti tecnici

* inserimenti batch;
* transazioni;
* indici SQLite ragionati;
* possibilità di disabilitare il salvataggio token;
* test su corpus piccolo;
* misurazione prestazioni.

### Indici possibili

```text
AnalysisRunId, NormalizedText
AnalysisRunId, BookId
AnalysisRunId, ChapterId
BookId, ChapterId, SentenceIndex
```

### Criteri di accettazione

* import non diventa eccessivamente lento;
* database resta gestibile;
* KWIC usa token invece di regex;
* query `stats kwic` resta veloce;
* test coprono ricerca token e contesti.

---

# Fase 8 — UI desktop

## Milestone 18 — Prima UI Avalonia

### Obiettivo

Creare una prima interfaccia desktop solo quando il motore sarà abbastanza stabile.

### Tecnologia proposta

```text
Avalonia
```

### Schermate iniziali

```text
Dashboard
Corpora
Runs
Books
Words
Word detail
KWIC
N-grams
Compare
Settings
```

### Funzioni minime UI

* creare corpus;
* scegliere cartella EPUB;
* lanciare analisi;
* vedere run salvate;
* vedere summary run;
* vedere top words;
* vedere top content words;
* vedere top function words;
* cercare una parola;
* vedere dettaglio parola;
* vedere KWIC;
* aprire report Markdown;
* aprire CSV esportati.

### Non obiettivi della prima UI

* grafici complessi;
* editing avanzato;
* cloud;
* multiutente;
* sincronizzazione;
* machine learning;
* traduzione;
* classificazione semantica avanzata.

### Criteri di accettazione

* UI semplice e stabile;
* usa i servizi già esistenti;
* non duplica logica del core;
* non rompe CLI;
* gestione errori leggibile;
* usabile per analizzare una cartella EPUB senza terminale.

---

# Roadmap sintetica

Ordine consigliato:

```text
5-Final  Consolidamento stopword / word detail / KWIC
6        Import diagnostics
7        Tokenizer inglese migliorato
8        Sentence splitter migliorato
9        Database model v2 con libri reali nelle run aggregate
10       Query per libro e dispersione
11       Collocazioni
12       Phrase mining
13       Confronto tra corpora
14       Difficoltà relativa
15       Language profiles v2
16       Primo corpus italiano
17       Token index persistente
18       Prima UI Avalonia
```

---

# Priorità operative immediate

Le prossime tre milestone da sviluppare realmente sono:

## Priorità 1

```text
Milestone 5-Final — Consolidamento stopword / word detail / KWIC
```

Motivo:

Le ultime funzioni devono essere testate e corrette prima di costruirci sopra.

---

## Priorità 2

```text
Milestone 6 — Import diagnostics
```

Motivo:

Prima di fare analisi più sofisticate, bisogna sapere se i testi importati sono puliti.

---

## Priorità 3

```text
Milestone 7 — Tokenizer inglese migliorato
```

Motivo:

L’inglese narrativo usa molte contrazioni e possessivi. Se il tokenizer non li gestisce bene, parole, n-grammi, phrase mining e KWIC risultano meno affidabili.

---

# Regola progettuale da mantenere

CorpusLens non deve cancellare informazione linguistica utile.

In particolare:

```text
Le stopword non devono essere eliminate.
Devono essere classificate.
```

Il sistema deve offrire viste diverse:

* tutte le parole;
* parole contenuto;
* parole funzione;
* parole successive;
* parole precedenti;
* collocazioni;
* phrase;
* contesti KWIC;
* confronto tra corpus.

Questa scelta è importante perché, per imparare una lingua, le parole funzione sono fondamentali quanto le parole contenuto.

---

# Direzione generale

CorpusLens deve evolvere in questo ordine:

```text
1. dati puliti;
2. tokenizzazione affidabile;
3. persistenza corretta;
4. query utili;
5. confronto tra corpus;
6. multilingua;
7. UI.
```

La UI deve arrivare solo dopo che il motore produce dati affidabili, interrogabili e linguisticamente sensati.

---

## Nota operativa — Milestone 6.2

Dopo il primo giro di diagnostica sul corpus italiano, la pulizia metadata è stata raffinata ulteriormente:

- capitoli metadata-only italiani con `QUESTO E-BOOK`, `LICENZA`, `Liber Liber`, URL e donazioni vengono riconosciuti meglio;
- i capitoli lunghi reali non vengono più segnalati come sospetti solo perché contengono parole deboli come `indice`, `sommario`, `prefazione` o `editore`;
- i conteggi dei termini sospetti restano comunque visibili nel report diagnostico.


### Milestone 9 note

Aggiunto collegamento `AnalysisRunBook` tra run aggregate e libri EPUB reali. Nuovo comando: `stats books <runId>`.

### Milestone 10 note

Aggiunto comando `stats word-books <runId> <word>` / `make stats-word-books` per vedere la distribuzione di una parola sui libri reali collegati a una run aggregata. La query usa i capitoli puliti già salvati nel DB; il token index persistente resta previsto per una milestone successiva.

### Milestone 10.1 note

Uniformata l'intestazione del comando `stats word-books`: ora stampa sempre `Source books`, `Matched books`, `Coverage`, `Total count` e `Shown books`, anche quando la parola è presente in molti libri o non viene trovata.


### Milestone 11 note

Aggiunto comando `stats collocations <runId> <word>` / `make stats-collocations` per calcolare collocazioni a finestra intorno a una parola target. La query usa i capitoli puliti già salvati nel DB e restituisce conteggio, sinistra/destra, frequenza per occorrenza target e distanza media. PMI/log-likelihood e token index persistente restano fuori da questa prima milestone.

### Milestone 11.1 note

Aggiunti filtri `--content-only` e `--function-only` al comando `stats collocations`, con colonna `Type` e target Makefile dedicati. Le collocazioni restano calcolate a frequenza grezza su finestra, ma l'output content-only permette di vedere subito collocati lexicalmente più utili.

---

## Implemented note — Milestone 11.2

Collocation output now includes a lightweight Dice ranking score. Raw counts remain visible, but default ordering favors more characteristic collocates over very common words.


## Milestone 11.3 — Collocation thresholds

Aggiunti `--min-count` e `--min-dice` al comando `stats collocations`, con parametri Makefile `MIN_COUNT` e `MIN_DICE`. L'output ora mostra soglie applicate, collocati filtrati e righe mostrate dopo `--limit`.


## Milestone 12 implemented

Phrase mining command added with n-range, min-count and optional content-word boundary filtering.

## Maintenance notes

- Milestone 12.1 fixed single-run `stats books`, sentence-classification punctuation edge cases, and n-gram contiguity with `MinWordLength`.

## Milestone 12.2 — Phrase mining polish

- Added `--min-chapters` for `stats phrases`.
- Added `--longest-only` to suppress conservative nested phrase duplicates.
- Added Makefile support for `MIN_CHAPTERS` and `LONGEST_ONLY`.

## Milestone 13 — Corpus comparison

- Added `stats compare-word <leftRunId> <rightRunId> <word>`.
- Added `stats compare-words <leftRunId> <rightRunId>` with `--min-count`, `--content-only` and `--function-only`.
- Added Makefile targets for run comparison.
- Ranking is currently based on absolute per-million frequency difference; stronger keyness metrics remain future work.

## Milestone 13.1 — Corpus comparison polish

- Added `--shared-only` and `--exclusive-only` to `stats compare-words`.
- Added language mismatch note for lexical cross-language comparisons.
- Improved formatting for very small non-zero ratios.
- Added Makefile support for `SHARED_ONLY` and `EXCLUSIVE_ONLY`.

## Milestone 14 — Relative difficulty

Added `stats difficulty` and `stats compare-difficulty` with a transparent heuristic based on sentence length, word length, long-word share, content-word share and lexical diversity.

## Milestone 14.1 — Difficulty output polish

Polished `stats difficulty` output with language, thresholds and clearer comparison notes. No scoring formula changes.

## Milestone 15 — Language profiles v2

Added explicit language profiles for `en`, `it`, `fr` and `de`. Difficulty thresholds now default from the run language profile unless overridden on the CLI.

## Milestone 16 — Corpus profile / Italian corpus validation

Added `stats profile <runId>` and `make stats-profile` as a compact run validation view. It combines run metadata, source books, core metrics, difficulty profile, top content/function words and recurring phrases with conservative phrase filters. The command is useful for quickly checking Italian and English folder runs after import.

## Milestone 17.1 — Token index schema + save

Added persistent `TokenOccurrence` storage and `stats token-index <runId>` for validation. Existing KWIC, collocation, phrase and word-book queries still use the proven chapter-text paths; later milestones can migrate them one at a time.


## Milestone 17.2 — KWIC da token index

`stats kwic` now uses `TokenOccurrence` when a run has a token index, with automatic fallback to the previous chapter-text implementation for legacy runs. Output remains unchanged; only the occurrence lookup path changed.

## Milestone 17.3 — Collocations da token index

`stats collocations` now uses `TokenOccurrence` when a run has a token index, with automatic fallback to the previous chapter-text implementation for legacy runs. Output remains unchanged; the query path now uses persisted token positions and keeps collocation windows inside chapter boundaries.

## Milestone 17.4 — Phrase mining da token index

`stats phrases` now uses `TokenOccurrence` when available, with fallback to the previous `CleanText` implementation for legacy runs. The command keeps the same output and filters, while phrase candidates are derived from persisted token positions and still checked against chapter text so phrases do not cross punctuation.
