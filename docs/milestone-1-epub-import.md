# Milestone 1 — EPUB import

Stato: fix 2

## Obiettivo

Aggiungere un primo import EPUB senza introdurre ancora SQLite o UI desktop.

La pipeline della milestone è:

```text
EPUB
  ↓
VersOne.Epub
  ↓
HTML/XHTML content
  ↓
HtmlToTextConverter
  ↓
EpubBoilerplateCleaner
  ↓
CorpusAnalyzer
  ↓
Markdown + CSV
```

## Funzioni implementate

- Lettura EPUB singolo.
- Estrazione metadata base: titolo e autore, quando disponibili.
- Estrazione contenuti dal reading order EPUB.
- Conversione HTML/XHTML in testo leggibile.
- Rimozione `script`, `style` e `noscript`.
- Normalizzazione spazi e righe vuote.
- Rimozione boilerplate Project Gutenberg quando sono presenti i marker START/END.
- Analisi del testo estratto con il motore esistente.
- Generazione di:
  - `extracted_text.txt`
  - `report.md`
  - `words.csv`
  - `ngrams.csv`
  - `next_words.csv`

## Comando CLI

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub ./books/alice.epub --language en --out ./artifacts/alice
```

## Correzioni fix 2

La prova su `alice.epub` ha evidenziato due problemi utili:

1. Il testo Project Gutenberg entrava nelle statistiche, producendo risultati come `project gutenberg` tra gli n-grammi più frequenti.
2. La sezione `Next words` del report mostrava quasi solo coppie associate alla parola `a`, perché il report prendeva i primi risultati ordinati per parola.
3. Alcune domande o esclamazioni chiuse da virgolette non venivano classificate correttamente.

Correzioni applicate:

- aggiunto `EpubBoilerplateCleaner`;
- applicata pulizia Project Gutenberg nell'import EPUB;
- ordinata la sezione `Next words` del report per conteggio decrescente;
- migliorato `SimplePhraseClassifier` per ignorare virgolette e parentesi finali quando controlla `?` e `!`;
- aggiunti test dedicati.

## Non incluso in questa milestone

- Import cartella EPUB.
- SQLite.
- Titoli reali dei capitoli.
- Deduplicazione indice/contents.
- Filtri configurabili da CLI.
- UI desktop.

