# Milestone 5.3 — Language-specific book folders

CorpusLens now assumes a language-based local EPUB layout for day-to-day analysis:

```text
books/
├── en/
└── it/
```

The goal is to avoid mixing languages in the same corpus by mistake. The generic CLI still accepts any folder, but the Makefile now provides explicit targets for English and Italian.

## Makefile additions

```powershell
make setup-books
make corpus-create-en
make corpus-create-it
make analyze-en
make analyze-it
make analyze-en-recursive
make analyze-it-recursive
```

## Default folders

```text
BOOKS_EN = ./books/en
BOOKS_IT = ./books/it
OUT_EN   = ./artifacts/en
OUT_IT   = ./artifacts/it
```

## English workflow

```powershell
make check
make corpus-create-en
make analyze-en
make stats-runs LIMIT=10
make stats-content RUN=1 LIMIT=30
```

## Italian workflow

```powershell
make check
make corpus-create-it
make analyze-it
make stats-runs LIMIT=10
make stats-content RUN=1 LIMIT=30
make stats-function RUN=1 LIMIT=30
```

## Running both languages in one database

`make check` removes `./data` and `./artifacts`, so it should not be used between two analyses that must remain in the same SQLite database.

Use this sequence instead:

```powershell
make clean
make corpus-create-en
make corpus-create-it
make analyze-en
make analyze-it
make stats-runs LIMIT=10
```

The returned run ids can then be used for language-specific queries.
