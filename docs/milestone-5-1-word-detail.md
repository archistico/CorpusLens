# Milestone 5.1 — Word detail lookup

This milestone adds a small database-backed inspection command for a single word.

## Command

```powershell
dotnet run --project src/CorpusLens.Cli -- stats word 1 alice --limit 25 --db ./data/corpuslens.db
```

Makefile shortcut:

```powershell
make stats-word RUN=1 WORD=alice LIMIT=25
```

## Output

The command prints:

- total count;
- document count;
- frequency per million;
- content/function classification;
- most common next words;
- most common previous words.

The previous-word table is derived from `NextWordStatistic` by querying rows where `NextWord` equals the selected word.

## Scope

This milestone does not add KWIC yet. KWIC requires token positions or sentence/token persistence and will be handled later.
