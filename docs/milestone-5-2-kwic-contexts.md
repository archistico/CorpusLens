# Milestone 5.2 — KWIC contexts

Status: draft implementation

## Goal

Add a first KWIC-style lookup so CorpusLens can show short real contexts for a word stored in a saved analysis run.

KWIC means **Key Word In Context**. It is useful because frequency alone does not explain how a word behaves. A learner can inspect repeated real usages without opening the full extracted text manually.

## Added command

```powershell
dotnet run --project src/CorpusLens.Cli -- stats kwic 1 alice --limit 10 --context 8 --db ./data/corpuslens.db
```

Makefile shortcut:

```powershell
make stats-kwic RUN=1 WORD=alice LIMIT=10 CONTEXT=8
```

## Behavior

The command:

1. loads the selected analysis run;
2. finds the stored book/source for that run;
3. scans the stored clean chapter texts;
4. finds case-insensitive word matches;
5. prints a small left/right context window around each match.

The command does not require token rows in the database. It uses the already stored chapter text.

## Output shape

```text
KWIC contexts for 'alice' in run 1
Context words per side: 8

#    Chapter                         Left context                         Match           Right context
---  ------------------------------  -----------------------------------  --------------  -----------------------------------
  1  CHAPTER I. Down the Rabbit-Hole  down the rabbit-hole                 Alice           was beginning to get very tired
```

## Notes

This first implementation is intentionally simple:

- it returns short snippets only;
- it does not save contexts permanently;
- it uses word-like token matching instead of raw substring search;
- it scans stored `Chapter.CleanText` on demand.

Future improvements may include:

- sentence-aware KWIC;
- export to CSV/Markdown;
- filtering by book/chapter;
- random samples instead of first occurrences;
- context sorting by following/previous word.
