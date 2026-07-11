# Milestone 17.4 — Phrase mining da token index

`stats phrases` now uses the persisted `TokenOccurrence` table when a run has a token index.

The CLI interface is unchanged:

```powershell
make stats-phrases RUN=1 MIN_N=2 MAX_N=5 MIN_COUNT=3 LIMIT=30
make stats-phrases-content-boundary RUN=1 MIN_N=2 MAX_N=5 MIN_COUNT=3 MIN_CHAPTERS=2 LONGEST_ONLY=--longest-only LIMIT=30
```

## Behaviour

- Indexed runs use `TokenOccurrence` ordered by chapter/token position.
- Legacy runs without token occurrences automatically fall back to the previous `Chapter.CleanText` path.
- Phrase mining still does not cross punctuation: token offsets are checked against chapter text and only whitespace-separated consecutive words are joined.
- Output and filtering are unchanged.

## Why this matters

This completes the first query migration set after introducing the token index:

- KWIC uses token index.
- Collocations use token index.
- Phrase mining uses token index.

Word-book distribution still uses chapter text and can be migrated separately if needed.
