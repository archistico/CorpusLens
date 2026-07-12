# CorpusLens — Documento tecnico iniziale

Versione: 0.1
Stato: bozza iniziale
Obiettivo: definire visione, architettura, milestone e modello dati minimo del progetto.

---

## 1. Visione del progetto

**CorpusLens** è un’applicazione per analizzare testi provenienti da libri digitali in formato EPUB, con l’obiettivo di comprendere meglio il funzionamento pratico di una lingua attraverso dati estratti da testi reali.

Il progetto non nasce come semplice contatore di parole, ma come uno strumento per costruire e analizzare **corpora linguistici personali**.

Un corpus può rappresentare, ad esempio:

* libri per bambini in inglese;
* romanzi gialli in italiano;
* testi tecnici in tedesco;
* narrativa semplice in francese;
* manuali professionali;
* testi scolastici;
* libri per differenti livelli di competenza linguistica.

L’utente deve poter caricare più EPUB, assegnarli a un corpus, analizzarli e ottenere report utili per capire:

* quali parole sono più frequenti;
* quali parole compaiono insieme;
* quali parole seguono più spesso altre parole;
* quali frasi o strutture ricorrono;
* quanto un testo è semplice o complesso;
* quali differenze ci sono tra corpora diversi;
* quali pattern linguistici sono tipici di un certo genere, livello o dominio.

---

## 2. Principi guida

Il progetto deve seguire alcuni principi già applicati in altri software sviluppati insieme:

1. **Chiarezza prima di tutto**
   Il codice deve essere leggibile, esplicito e testabile.

2. **Core separato dalla UI**
   Il motore di analisi deve funzionare prima da console o test automatici. L’interfaccia grafica arriverà dopo.

3. **Milestone piccole e verificabili**
   Ogni fase deve produrre un risultato concreto e testabile.

4. **Niente sovraingegnerizzazione iniziale**
   L’architettura deve essere pulita, ma non inutilmente complessa.

5. **Dati riproducibili**
   A parità di EPUB, impostazioni e versione del motore, l’analisi deve produrre gli stessi risultati.

6. **Pipeline esplicita**
   Ogni passaggio deve essere separato: importazione, pulizia, segmentazione, tokenizzazione, analisi, report.

7. **Supporto multilingua progressivo**
   Si parte da una lingua semplice da gestire, preferibilmente inglese, poi si estende a italiano, tedesco, francese e altre lingue.

8. **Uso locale e rispettoso dei testi**
   Il progetto può analizzare libri personali o di pubblico dominio, ma non deve favorire l’esportazione massiva di testi protetti da copyright.

---

## 3. Obiettivi principali

CorpusLens deve permettere di:

* importare libri EPUB;
* estrarre testo pulito dai contenuti HTML/XHTML;
* organizzare i libri in corpora;
* indicare lingua, genere, livello e altre informazioni descrittive;
* segmentare il testo in frasi;
* tokenizzare il testo in parole;
* normalizzare parole e punteggiatura;
* calcolare frequenze;
* calcolare n-grammi;
* calcolare parole precedenti e successive;
* individuare frasi ricorrenti;
* classificare alcune tipologie semplici di frasi;
* esportare report in formati leggibili;
* confrontare corpora diversi.

---

## 4. Non obiettivi iniziali

Nelle prime versioni non si vuole ancora realizzare:

* una UI completa;
* traduzione automatica;
* sintesi vocale;
* riconoscimento perfetto di lemmi e parti del discorso;
* classificazione semantica avanzata tramite AI;
* valutazione certificata del livello CEFR A1/A2/B1/B2;
* supporto completo a lingue senza separazione esplicita tra parole, come cinese o giapponese;
* gestione cloud o multiutente;
* sincronizzazione online;
* pubblicazione di contenuti testuali protetti.

Questi aspetti potranno essere valutati in milestone future.

---

## 5. Formati supportati

### 5.1 Formato iniziale

Il formato iniziale supportato sarà:

```text
EPUB
```

L’EPUB verrà trattato come contenitore da cui estrarre:

* metadata;
* titolo;
* autore;
* lingua dichiarata;
* sequenza dei file HTML/XHTML;
* testo dei capitoli;
* eventuale struttura interna del libro.

### 5.2 Formati futuri possibili

In futuro si potranno aggiungere:

```text
TXT
PDF testuale
HTML
DOCX
Markdown
```

Il PDF non dovrebbe essere il primo formato, perché l’estrazione testuale può essere più sporca e meno prevedibile rispetto all’EPUB.

---

## 6. Concetti principali

### 6.1 Book

Un `Book` rappresenta un singolo libro importato.

Contiene:

* titolo;
* autore;
* lingua;
* percorso del file;
* hash del file;
* metadata EPUB;
* data importazione;
* capitoli estratti;
* stato dell’importazione.

---

### 6.2 Corpus

Un `Corpus` rappresenta un insieme di libri analizzati insieme.

Esempi:

```text
English Kids Books
Italian Detective Novels
German Technical Manuals
French Easy Readers
```

Un corpus ha:

* nome;
* lingua principale;
* descrizione;
* tag;
* libri associati;
* impostazioni di analisi.

---

### 6.3 Chapter

Un `Chapter` rappresenta una sezione testuale estratta da un EPUB.

Non è detto che corrisponda sempre a un capitolo editoriale reale, perché alcuni EPUB dividono il contenuto in file tecnici. Per questo il concetto va trattato in modo flessibile.

---

### 6.4 Sentence

Una `Sentence` rappresenta una frase individuata dal motore.

Contiene:

* testo originale;
* testo normalizzato;
* libro di origine;
* capitolo di origine;
* posizione nel capitolo;
* numero di token;
* eventuale categoria rilevata.

---

### 6.5 Token

Un `Token` rappresenta una singola unità del testo.

Può essere:

* parola;
* numero;
* punteggiatura;
* simbolo;
* abbreviazione;
* altro elemento riconosciuto.

Esempio:

```text
"I don't know."
```

Token possibili:

```text
I
do
n't
know
.
```

Oppure, in una prima versione più semplice:

```text
I
don't
know
.
```

La scelta esatta verrà definita nella fase di tokenizzazione.

---

### 6.6 WordForm

Una `WordForm` rappresenta una parola così come compare nel testo dopo normalizzazione.

Esempio:

```text
Running
running
RUNNING
```

possono essere ricondotte alla stessa forma normalizzata:

```text
running
```

---

### 6.7 Lemma

Un `Lemma` rappresenta la forma base di una parola.

Esempio:

```text
go
goes
went
gone
going
```

possono essere ricondotte al lemma:

```text
go
```

La lemmatizzazione non è obbligatoria nella prima versione. Deve essere prevista nell’architettura, ma può essere implementata dopo.

---

### 6.8 NGram

Un `NGram` rappresenta una sequenza ricorrente di N token.

Esempi:

```text
I don't
I don't know
as soon as
in order to
non lo so
a un certo punto
ich weiß nicht
```

CorpusLens dovrà calcolare almeno:

* bigrammi;
* trigrammi;
* 4-grammi;
* 5-grammi.

---

### 6.9 PhraseCategory

Una `PhraseCategory` rappresenta una classificazione semplice di una frase.

Categorie iniziali possibili:

* domanda;
* negazione;
* saluto;
* richiesta;
* esclamazione;
* dialogo;
* imperativo probabile;
* frase dichiarativa;
* altra categoria.

Nella prima versione la classificazione sarà basata su regole, non su machine learning.

---

## 7. Pipeline di analisi

La pipeline iniziale sarà:

```text
EPUB
  ↓
lettura struttura interna
  ↓
estrazione contenuti HTML/XHTML
  ↓
rimozione markup
  ↓
pulizia testo
  ↓
normalizzazione
  ↓
segmentazione in frasi
  ↓
tokenizzazione
  ↓
salvataggio dati intermedi
  ↓
calcolo statistiche
  ↓
generazione report
```

Ogni fase deve essere testabile separatamente.

---

## 8. Pulizia del testo

La pulizia del testo è una delle parti più importanti del progetto.

Gli EPUB possono contenere:

* indici;
* note;
* copyright;
* numeri di pagina;
* intestazioni ripetute;
* piè di pagina;
* sillabazioni;
* caratteri tipografici speciali;
* spazi multipli;
* righe vuote;
* virgolette diverse;
* apostrofi diversi;
* trattini di dialogo;
* contenuti non narrativi;
* testo promozionale dell’editore.

La pulizia deve essere progressiva e configurabile.

Non deve esserci una singola funzione enorme che prova a risolvere tutto.

Esempi di componenti separati:

```text
HtmlTextExtractor
WhitespaceNormalizer
QuoteNormalizer
DashNormalizer
PageNumberRemover
HeaderFooterDetector
NoteRemover
ChapterTitleDetector
```

Nelle prime milestone si implementano solo le parti essenziali.

---

## 9. Statistiche iniziali

### 9.1 Statistiche generali

Per ogni corpus:

* numero libri;
* numero capitoli;
* numero frasi;
* numero token;
* numero parole;
* numero parole distinte;
* lunghezza media frase;
* lunghezza media parola;
* distribuzione lunghezza frasi;
* distribuzione lunghezza parole.

---

### 9.2 Frequenze parole

Per ogni parola:

* forma originale;
* forma normalizzata;
* conteggio assoluto;
* frequenza relativa;
* frequenza per milione di parole;
* numero di libri in cui compare;
* numero di capitoli in cui compare;
* prima occorrenza;
* ultima occorrenza;
* dispersione nel corpus.

Esempio output:

```text
Word      Count   PerMillion   Books   Chapters
the       15420   53210.44     12      184
and        8421   29061.32     12      176
little     2210    7625.11      9       88
```

---

### 9.3 Parole successive

Per le parole più frequenti, CorpusLens deve calcolare quali parole le seguono più spesso.

Esempio:

```text
Word: I

Next word     Count   Probability
am            214     0.18
don't         181     0.15
was           166     0.14
have          121     0.10
think          84     0.07
```

Questa analisi deve avere soglie minime per evitare risultati rumorosi.

Parametri iniziali:

```text
top_words = 1000
min_pair_count = 3
min_probability = opzionale
```

---

### 9.4 Parole precedenti

Analisi simile alle parole successive, ma invertita.

Esempio:

```text
Word: know

Previous word     Count
don't             181
I                 120
you                84
to                 77
```

---

### 9.5 N-grammi

CorpusLens deve calcolare sequenze frequenti di parole.

Tipi iniziali:

```text
2-gram
3-gram
4-gram
5-gram
```

Filtri minimi:

```text
min_count >= 3
no solo numeri
no solo punteggiatura
no sequenze vuote
```

In futuro i filtri potranno diventare più raffinati.

---

### 9.6 Frasi ricorrenti

CorpusLens deve individuare frasi uguali o molto simili.

Prima versione:

* confronto su frase normalizzata;
* conteggio;
* libri in cui compare;
* capitoli in cui compare.

Esempio:

```text
Sentence              Count
I don't know.         42
What do you mean?     31
Come here.            18
```

---

### 9.7 Classificazione frasi base

La classificazione iniziale sarà rule-based.

Esempi:

#### Domande

Regole possibili:

* frase termina con `?`;
* in inglese inizia con `what`, `why`, `where`, `when`, `how`, `do`, `does`, `did`, `can`, `could`, `would`;
* in italiano contiene pattern come `che cosa`, `perché`, `dove`, `quando`, `come`;
* in tedesco contiene parole interrogative come `wer`, `was`, `wo`, `wann`, `warum`, `wie`.

#### Negazioni

Esempi:

```text
en: not, don't, doesn't, didn't, never, no
it: non, mai, nessuno, niente
de: nicht, kein, keine, niemals
fr: ne, pas, jamais, rien
```

#### Saluti

Esempi:

```text
en: hello, hi, good morning, good evening
it: ciao, buongiorno, buonasera, salve
de: hallo, guten morgen, guten abend
fr: bonjour, salut, bonsoir
```

#### Richieste

Esempi:

```text
en: can you, could you, would you, please
it: puoi, potresti, per favore
de: können Sie, kannst du, bitte
fr: pouvez-vous, peux-tu, s'il vous plaît
```

Le regole dovranno essere specifiche per lingua.

---

## 10. Report iniziali

La prima versione deve generare report semplici ma utili.

Formati consigliati:

```text
Markdown
CSV
JSON
```

### 10.1 Report Markdown

Pensato per lettura umana.

Contenuto:

* riepilogo corpus;
* lista libri;
* statistiche generali;
* top parole;
* top bigrammi;
* top trigrammi;
* parole successive;
* frasi ricorrenti;
* note su eventuali errori di importazione.

---

### 10.2 Export CSV

Pensato per analisi esterna con Excel, LibreOffice, Python o altri strumenti.

File possibili:

```text
words.csv
sentences.csv
ngrams.csv
next_words.csv
previous_words.csv
books.csv
chapters.csv
```

---

### 10.3 Export JSON

Pensato per usi futuri:

* UI;
* API;
* importazione in altri strumenti;
* confronto automatico.

---

## 11. Architettura software

Struttura proposta:

```text
CorpusLens
│
├── src
│   ├── CorpusLens.Domain
│   ├── CorpusLens.Application
│   ├── CorpusLens.Analysis
│   ├── CorpusLens.Infrastructure
│   ├── CorpusLens.Cli
│   └── CorpusLens.Desktop
│
├── tests
│   ├── CorpusLens.Domain.Tests
│   ├── CorpusLens.Application.Tests
│   ├── CorpusLens.Analysis.Tests
│   ├── CorpusLens.Infrastructure.Tests
│   └── CorpusLens.Integration.Tests
│
├── docs
│   ├── technical-design.md
│   ├── roadmap.md
│   └── analysis-rules.md
│
├── samples
│   ├── epubs
│   └── reports
│
└── README.md
```

La cartella `CorpusLens.Desktop` può essere creata più avanti. Nelle prime milestone bastano core, infrastruttura, analisi e CLI.

---

## 12. Progetti principali

### 12.1 CorpusLens.Domain

Contiene il modello di dominio puro.

Non deve dipendere da:

* filesystem;
* database;
* librerie EPUB;
* UI;
* librerie esterne pesanti.

Classi principali:

```text
Corpus
Book
BookMetadata
Chapter
Sentence
Token
WordForm
NGram
AnalysisRun
PhraseCategory
LanguageProfile
```

---

### 12.2 CorpusLens.Application

Contiene i casi d’uso.

Esempi:

```text
ImportBookUseCase
ImportFolderUseCase
CreateCorpusUseCase
AnalyzeCorpusUseCase
GenerateReportUseCase
CompareCorporaUseCase
```

Coordina Domain, Analysis e Infrastructure.

---

### 12.3 CorpusLens.Analysis

Contiene gli algoritmi di analisi.

Servizi iniziali:

```text
SentenceSplitter
Tokenizer
WordFrequencyAnalyzer
NGramAnalyzer
NextWordAnalyzer
PreviousWordAnalyzer
RepeatedSentenceAnalyzer
PhraseClassifier
CorpusStatisticsCalculator
```

---

### 12.4 CorpusLens.Infrastructure

Contiene implementazioni tecniche.

Esempi:

```text
EpubBookReader
SqliteCorpusRepository
FileSystemStorage
MarkdownReportWriter
CsvReportWriter
JsonReportWriter
```

---

### 12.5 CorpusLens.Cli

Prima interfaccia utente del progetto.

Comandi possibili:

```text
corpuslens corpus create "English Kids"
corpuslens import ./books --corpus "English Kids" --language en
corpuslens analyze "English Kids"
corpuslens report "English Kids" --format markdown --out ./reports
corpuslens export "English Kids" --format csv --out ./exports
```

---

### 12.6 CorpusLens.Desktop

Interfaccia desktop Avalonia read-only per l'esplorazione delle run già persistite.

Funzioni disponibili:

* apertura di un database CorpusLens;
* elenco e selezione delle run;
* dashboard corpus e diagnostica token index;
* Books explorer;
* Word explorer con KWIC e distribuzione per libro;
* collocazioni e phrase mining;
* confronto lessicale e di difficoltà tra run.

La struttura desktop mantiene `MainWindowViewModel` come coordinatore e delega le singole aree a ViewModel dedicati:

```text
MainWindowViewModel
├── DashboardViewModel
├── BooksExplorerViewModel
├── WordExplorerViewModel
├── CollocationsExplorerViewModel
├── PhraseExplorerViewModel
├── CompareRunsViewModel
└── DesktopOperationStateViewModel
```

Le query restano nel livello Application. I ViewModel desktop ricevono funzioni di query iniettabili per consentire test unitari senza database reale.

```text
CorpusLens.Desktop
  -> CorpusLens.Application.Queries
    -> CorpusLens.Infrastructure.Storage
```

Il code-behind programmatico Avalonia è suddiviso per area funzionale. Le operazioni globali condividono busy state, messaggi e cancellazione coordinata. Import EPUB, gestione corpus ed esportazione dalla UI restano milestone successive.

---

## 13. Modello dati minimo

Il database iniziale può essere SQLite.

### 13.1 Corpus

```text
Corpus
------
Id
Name
LanguageCode
Description
CreatedAt
UpdatedAt
```

---

### 13.2 Book

```text
Book
----
Id
CorpusId
Title
Author
LanguageCode
OriginalFilePath
FileHash
ImportedAt
Status
ErrorMessage
```

`Status` può assumere valori come:

```text
Imported
Analyzed
Failed
```

---

### 13.3 Chapter

```text
Chapter
-------
Id
BookId
OrderIndex
Title
SourcePath
RawText
CleanText
CharacterCount
```

Nella prima versione `RawText` e `CleanText` possono essere salvati direttamente. In futuro, se il database cresce troppo, si potrà valutare storage su filesystem.

---

### 13.4 Sentence

```text
Sentence
--------
Id
BookId
ChapterId
OrderIndex
Text
NormalizedText
TokenCount
CharacterCount
Category
```

---

### 13.5 Token

```text
Token
-----
Id
BookId
ChapterId
SentenceId
OrderIndex
Text
NormalizedText
TokenType
StartOffset
EndOffset
```

`TokenType` iniziali:

```text
Word
Number
Punctuation
Symbol
Other
```

---

### 13.6 WordStatistic

```text
WordStatistic
-------------
Id
AnalysisRunId
CorpusId
Word
NormalizedWord
Count
DocumentCount
ChapterCount
FrequencyPerMillion
FirstOccurrenceTokenId
LastOccurrenceTokenId
```

---

### 13.7 NGramStatistic

```text
NGramStatistic
--------------
Id
AnalysisRunId
CorpusId
N
Text
NormalizedText
Count
DocumentCount
FrequencyPerMillion
```

---

### 13.8 NextWordStatistic

```text
NextWordStatistic
-----------------
Id
AnalysisRunId
CorpusId
Word
NextWord
Count
Probability
```

---

### 13.9 PreviousWordStatistic

```text
PreviousWordStatistic
---------------------
Id
AnalysisRunId
CorpusId
Word
PreviousWord
Count
Probability
```

---

### 13.10 SentenceStatistic

```text
SentenceStatistic
-----------------
Id
AnalysisRunId
CorpusId
NormalizedText
ExampleText
Count
DocumentCount
Category
```

---

### 13.11 AnalysisRun

```text
AnalysisRun
-----------
Id
CorpusId
StartedAt
CompletedAt
Status
EngineVersion
SettingsJson
ErrorMessage
```

Questo permette di sapere con quali impostazioni è stata prodotta un’analisi.

---

## 14. Impostazioni di analisi

Ogni corpus dovrebbe avere impostazioni modificabili.

Esempio:

```json
{
  "language": "en",
  "minWordLength": 1,
  "lowercaseWords": true,
  "keepStopWords": true,
  "includePunctuationTokens": false,
  "ngramMinN": 2,
  "ngramMaxN": 5,
  "minNGramCount": 3,
  "topWordsForNextWordAnalysis": 1000,
  "minNextWordPairCount": 3,
  "classifySentences": true
}
```

Le stopword non devono essere eliminate automaticamente. Per chi studia una lingua, parole molto comuni come articoli, preposizioni, pronomi e ausiliari sono importanti.

---

## 15. Milestone

### Milestone 0 — Setup progetto e documento tecnico

Obiettivi:

* creare repository;
* creare struttura solution;
* aggiungere README iniziale;
* aggiungere documento tecnico;
* aggiungere roadmap;
* impostare test;
* impostare formattazione codice.

Output atteso:

```text
Solution compilabile
Test vuoti o minimi funzionanti
README iniziale
Documento tecnico v0.1
```

---

### Milestone 1 — Import EPUB e testo pulito

Obiettivi:

* leggere file EPUB;
* estrarre metadata base;
* estrarre contenuti testuali;
* rimuovere markup HTML/XHTML;
* produrre testo pulito per capitolo;
* salvare libro e capitoli in SQLite;
* generare un report minimo.

Output atteso:

```text
Book imported
Chapters extracted
Clean text available
Import report generated
```

Test:

* EPUB valido;
* EPUB con metadata incompleti;
* EPUB con più file XHTML;
* EPUB con immagini;
* EPUB non leggibile;
* cartella con più EPUB.

---

### Milestone 2 — Segmentazione frasi e tokenizzazione

Obiettivi:

* dividere testo in frasi;
* dividere frasi in token;
* riconoscere parole, numeri e punteggiatura;
* normalizzare maiuscole/minuscole;
* salvare frasi e token;
* calcolare statistiche base.

Output atteso:

```text
Sentences table populated
Tokens table populated
Basic corpus statistics available
```

Test:

* frasi con punto;
* frasi con punto interrogativo;
* frasi con punto esclamativo;
* abbreviazioni semplici;
* dialoghi;
* apostrofi;
* numeri;
* punteggiatura multipla.

---

### Milestone 3 — Frequenze parole

Obiettivi:

* calcolare parole più frequenti;
* calcolare frequenza assoluta;
* calcolare frequenza per milione;
* calcolare document frequency;
* esportare `words.csv`;
* generare sezione Markdown delle top parole.

Output atteso:

```text
Top words report
words.csv
WordStatistic populated
```

---

### Milestone 4 — N-grammi

Obiettivi:

* calcolare bigrammi;
* calcolare trigrammi;
* calcolare 4-grammi;
* calcolare 5-grammi;
* applicare soglia minima;
* esportare `ngrams.csv`;
* generare report Markdown.

Output atteso:

```text
Top n-grams report
ngrams.csv
NGramStatistic populated
```

---

### Milestone 5 — Parole precedenti e successive

Obiettivi:

* calcolare parole successive per le top parole;
* calcolare parole precedenti per le top parole;
* calcolare probabilità condizionata;
* applicare soglie minime;
* esportare `next_words.csv`;
* esportare `previous_words.csv`.

Output atteso:

```text
Next word analysis
Previous word analysis
CSV exports
Markdown report
```

---

### Milestone 6 — Frasi ricorrenti e categorie semplici

Obiettivi:

* individuare frasi ripetute;
* classificare domande;
* classificare negazioni;
* classificare saluti;
* classificare richieste;
* classificare esclamazioni;
* esportare risultati.

Output atteso:

```text
Repeated sentence report
Sentence categories report
SentenceStatistic populated
```

---

### Milestone 7 — Confronto tra corpora

Obiettivi:

* confrontare due corpora;
* individuare parole distintive;
* individuare n-grammi distintivi;
* confrontare lunghezza media frasi;
* confrontare ricchezza lessicale;
* generare report comparativo.

Output atteso:

```text
Corpus comparison report
Distinctive words
Distinctive n-grams
Relative difficulty indicators
```

---

### Milestone 8 — Interfaccia grafica

Obiettivi:

* creare UI desktop;
* importare EPUB da interfaccia;
* visualizzare corpora;
* visualizzare statistiche;
* navigare parole e frasi;
* esportare report.

Questa milestone non deve iniziare prima che il motore sia stabile.

---

## 16. Testing

Il progetto deve avere test automatici fin dall’inizio.

Categorie di test:

### 16.1 Unit test

Per:

* normalizzazione testo;
* segmentazione frasi;
* tokenizzazione;
* conteggio parole;
* n-grammi;
* parole successive;
* classificazione frasi.

---

### 16.2 Integration test

Per:

* import EPUB reale;
* salvataggio SQLite;
* analisi completa corpus;
* generazione report.

---

### 16.3 Golden file test

Per alcuni EPUB o testi campione, si salvano output attesi.

Esempio:

```text
sample_english_short_text.expected.words.csv
sample_english_short_text.expected.ngrams.csv
sample_english_short_text.expected.report.md
```

Questi test servono a intercettare modifiche involontarie nel motore di analisi.

---

## 17. Dataset di test iniziali

Per evitare problemi di copyright, i primi test dovrebbero usare:

* testi brevi creati manualmente;
* EPUB di pubblico dominio;
* libri campione molto piccoli;
* testi sintetici costruiti apposta per verificare casi specifici.

Esempio testo sintetico:

```text
Hello, Tom.
Hello, Anna.
I don't know.
I don't know what you mean.
Do you know Anna?
No, I don't.
```

Questo permette di verificare:

* saluti;
* frasi ripetute;
* domande;
* negazioni;
* parole successive;
* bigrammi;
* trigrammi.

---

## 18. Possibili metriche future

Dopo le prime milestone si potranno aggiungere:

* lemmi;
* parti del discorso;
* verbi più frequenti;
* sostantivi più frequenti;
* aggettivi più frequenti;
* collocazioni;
* KWIC;
* parole rare;
* parole distintive per corpus;
* livello stimato di difficoltà;
* percentuale di vocabolario noto;
* confronto con liste di frequenza esterne;
* estrazione di espressioni idiomatiche;
* riconoscimento dialoghi;
* riconoscimento tempi verbali;
* analisi dei personaggi nei romanzi;
* clustering di frasi simili;
* esportazione Anki.

---

## 19. KWIC futuro

Una funzione molto utile sarà KWIC, cioè Key Word In Context.

Esempio per la parola `however`:

```text
... he was tired. however, he continued ...
... the result, however, was different ...
... however hard he tried ...
```

Questa funzione permette di studiare una parola nei suoi contesti reali.

Non è necessaria nella prima versione, ma va tenuta presente nel modello dati perché richiede token, frasi e posizioni.

---

## 20. Decisioni tecniche iniziali

Decisioni proposte:

```text
Linguaggio: C#
Runtime: .NET LTS corrente
Database: SQLite
Prima UI: CLI
UI futura: Avalonia
Formato input iniziale: EPUB
Export iniziali: Markdown, CSV, JSON
Test framework: xUnit
Approccio NLP iniziale: regole semplici + algoritmi interni
Lemmatizzazione: prevista ma non obbligatoria nella prima fase
Machine learning: escluso dalle prime milestone
```

---

## 21. Comandi CLI iniziali

Comandi minimi desiderati:

```text
corpuslens corpus create "English Kids" --language en

corpuslens import ./books --corpus "English Kids"

corpuslens analyze "English Kids"

corpuslens report "English Kids" --out ./reports

corpuslens export "English Kids" --format csv --out ./exports
```

Comandi futuri:

```text
corpuslens compare "English Kids" "English Technical"

corpuslens word "English Kids" --word "know"

corpuslens kwic "English Kids" --word "however"

corpuslens phrases "English Kids" --category questions
```

---

## 22. Prima versione utile

La prima versione realmente utile sarà la 0.1.

### CorpusLens 0.1

Funzioni:

* creazione corpus;
* import EPUB da cartella;
* estrazione testo;
* salvataggio SQLite;
* segmentazione frasi semplice;
* tokenizzazione semplice;
* top parole;
* top bigrammi;
* top trigrammi;
* parole successive per top 100 parole;
* report Markdown;
* export CSV.

Questa versione deve essere piccola, verificabile e già utile.

---

## 23. Criteri di completamento della versione 0.1

La versione 0.1 si considera completata quando:

* la solution compila senza errori;
* i test principali passano;
* almeno un EPUB viene importato correttamente;
* il testo pulito è leggibile;
* le frasi vengono segmentate in modo accettabile;
* le parole vengono tokenizzate in modo coerente;
* viene prodotto un report Markdown;
* viene prodotto almeno `words.csv`;
* viene prodotto almeno `ngrams.csv`;
* il README spiega come usare il programma;
* il comportamento è riproducibile.

---

## 24. Rischi principali

### 24.1 EPUB sporchi

Gli EPUB reali possono essere molto diversi tra loro.

Mitigazione:

* test con EPUB differenti;
* import robusto;
* log chiari;
* gestione errori per libro;
* pipeline di pulizia modulare.

---

### 24.2 Segmentazione frasi imperfetta

Abbreviazioni, dialoghi e punteggiatura possono creare errori.

Mitigazione:

* iniziare semplice;
* aggiungere test progressivi;
* evitare di promettere precisione assoluta;
* permettere miglioramenti per lingua.

---

### 24.3 Tokenizzazione multilingua

Ogni lingua ha particolarità diverse.

Mitigazione:

* partire dall’inglese;
* creare `LanguageProfile`;
* introdurre regole specifiche per lingua solo quando necessario.

---

### 24.4 Troppi dati

Salvare token e frasi può far crescere il database.

Mitigazione:

* usare SQLite inizialmente;
* evitare dati duplicati inutili;
* valutare archiviazione testo su filesystem se necessario;
* aggiungere indici solo quando servono.

---

### 24.5 Analisi troppo ambiziose

Classificare semanticamente le frasi può diventare complesso.

Mitigazione:

* partire con regole semplici;
* indicare i risultati come “probabili”;
* rimandare AI e machine learning a fasi future.

---

## 25. Direzione consigliata immediata

La prossima attività dovrebbe essere:

```text
Milestone 0 — Setup progetto
```

Contenuto:

* creare repository `CorpusLens`;
* creare solution .NET;
* creare progetti principali;
* creare primi test;
* aggiungere questo documento in `docs/technical-design.md`;
* aggiungere README iniziale;
* definire roadmap;
* preparare un testo sintetico di test.

Dopo questa fase si può iniziare la Milestone 1:

```text
Import EPUB → estrazione testo pulito → salvataggio capitoli
```

---

## 26. Sintesi finale

CorpusLens deve crescere come uno strumento tecnico, pulito e verificabile per analizzare testi reali e trasformarli in conoscenza linguistica.

La priorità non è avere subito molte funzioni, ma costruire un motore affidabile.

Ordine corretto:

```text
1. Import EPUB
2. Testo pulito
3. Frasi
4. Token
5. Frequenze
6. N-grammi
7. Parole successive/precedenti
8. Frasi ricorrenti
9. Classificazioni semplici
10. Confronto corpora
11. UI
```

La prima versione deve essere piccola, ma già utile per rispondere a domande concrete come:

* quali sono le parole più frequenti in questo insieme di libri?
* quali frasi si ripetono?
* quali parole seguono più spesso una parola importante?
* quali pattern linguistici emergono?
* questo corpus è più semplice o più complesso di un altro?

Questo è il nucleo di CorpusLens.


## Desktop chapter explorer

Milestone 18.10 extends the read-only desktop query path without moving storage concerns into the UI:

```text
MainWindowViewModel
  -> ChaptersExplorerViewModel
    -> ChapterExplorerQueryService
      -> SqliteCorpusStore.ListChaptersAsync
```

The application service derives word/sentence counts and conservative quality flags from the persisted `Chapter.CleanText`. The Desktop ViewModel owns selection, preview search and match navigation. No chapter text is edited or re-persisted.

## Desktop n-gram explorer

Milestone 18.11 exposes persisted `NGramStatistic` rows through the established read-only layering:

```text
MainWindowViewModel
  -> NGramExplorerViewModel
    -> NGramExplorerQueryService
      -> SqliteCorpusStore.ListNGramsAsync
```

Storage-level filters handle run id, n size, minimum count, exact contained term and deterministic ordering. The Application layer resolves source-book languages and uses `StopWordProvider` to derive `C`/`F` composition patterns and language-aware content/function filters. The Desktop layer owns option state, cancellable loading and presentation only. No n-gram is recomputed or persisted by the explorer.

## Desktop reports and exports explorer

Milestone 18.12 exposes the files already produced by an analysis run without moving filesystem resolution or SQLite access into the Avalonia view:

```text
MainWindowViewModel
  -> ArtifactExplorerViewModel
    -> ArtifactExplorerQueryService
      -> AnalysisRunQueryService.GetRunAsync
        -> SqliteCorpusStore.GetAnalysisRunAsync
```

`StoredAnalysisRun` remains the authoritative source for the report, word CSV, n-gram CSV, next-word CSV and extracted-text paths. The Application query service resolves absolute paths and the relative-path layouts normally produced by the CLI. A non-empty recorded path whose file no longer exists is classified as `Missing`; an empty path is classified as `NotGenerated`. The two EPUB-folder diagnostics are legacy optional artifacts that are discovered by filename in the resolved output directory because their paths are not currently persisted in `AnalysisRun`.

The Desktop layer owns only selection, display state and requests to open a target. `SystemPathLauncher` validates existence, passes the full path directly to `ProcessStartInfo` with `UseShellExecute = true`, and supplies no shell command or user-controlled arguments. Exported files are never rewritten by the explorer.
