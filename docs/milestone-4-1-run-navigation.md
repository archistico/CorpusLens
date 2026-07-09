# Milestone 4.1 — Run navigation and CLI quality

## Obiettivo

Questa micro-milestone migliora l'uso quotidiano dopo avere salvato una o più analisi nel database SQLite.

Aggiunge:

- elenco delle run di analisi salvate;
- riepilogo dettagliato di una singola run;
- target Makefile dedicati;
- formattazione numerica più leggibile nella CLI.

## Nuovi comandi CLI

Elenco run:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats runs --limit 10
```

Filtro per corpus:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats runs --corpus-id 1 --limit 10
```

Riepilogo run:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats summary 1
```

## Nuovi target Makefile

```powershell
make stats-runs LIMIT=10
make stats-summary RUN=1
```

## Nota

La CLI ora arrotonda i valori `double` a due decimali per rendere più leggibili frequenze per milione, probabilità e percentuali.
