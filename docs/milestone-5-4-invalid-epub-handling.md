# Milestone 5.4 — Invalid EPUB handling

## Obiettivo

L'analisi di una cartella EPUB non deve fallire completamente se un singolo file EPUB è corrotto, incompleto o non realmente valido.

Questo caso è emerso analizzando `books/it`, dove un EPUB scaricato da fonti public domain produceva l'errore:

```text
A local file header is corrupt.
```

## Comportamento precedente

Il comando:

```powershell
make analyze-it
```

si interrompeva al primo EPUB non leggibile, impedendo l'analisi degli altri file validi presenti nella cartella.

## Nuovo comportamento

Durante l'analisi cartella:

```powershell
make analyze-it
```

oppure:

```powershell
dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books/it --language it --corpus "Italian Literature" --db ./data/corpuslens.db --out ./artifacts/it
```

CorpusLens ora:

1. prova a leggere ogni EPUB;
2. importa gli EPUB validi;
3. registra gli EPUB falliti;
4. continua l'analisi se almeno un EPUB è valido;
5. mostra in console quanti EPUB sono stati saltati;
6. produce `import_failures.csv` nella cartella di output.

## Output aggiunto

```text
artifacts/<lingua>/import_failures.csv
```

Colonne:

```text
file_name
file_path
exception_type
error_message
```

## Esempio output console

```text
CorpusLens EPUB folder analysis completed.
Books:      7
Skipped:    1
Chapters:   124
Documents:  7
Text:       ./artifacts/it/extracted_text.txt
Report:     ./artifacts/it/report.md
Words CSV:  ./artifacts/it/words.csv
NGrams CSV: ./artifacts/it/ngrams.csv
Next CSV:   ./artifacts/it/next_words.csv
Failures:   ./artifacts/it/import_failures.csv

Skipped EPUB files:
- libro_corrotto.epub: A local file header is corrupt.
```

## Caso limite

Se tutti gli EPUB della cartella falliscono, CorpusLens interrompe l'analisi con errore.

Questo comportamento è voluto: non avrebbe senso salvare una run senza documenti validi.

## Criteri di accettazione

- un EPUB corrotto non blocca l'intera analisi cartella;
- gli EPUB validi vengono comunque analizzati;
- i file falliti sono visibili in console;
- i file falliti sono salvati in `import_failures.csv`;
- se tutti gli EPUB falliscono, viene mostrato un errore chiaro;
- l'analisi singolo EPUB resta strict e continua a fallire se il file passato è corrotto.
