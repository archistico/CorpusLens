# Milestone 13.1 — Corpus comparison polish

This milestone refines the first corpus-comparison commands without changing the database schema.

## Added

- `stats compare-words --shared-only`
- `stats compare-words --exclusive-only`
- clearer ratio formatting for very small non-zero ratios (`<0.01`)
- language note when two runs have different languages
- Makefile support for `SHARED_ONLY` and `EXCLUSIVE_ONLY`

## Examples

```powershell
make stats-compare-word RUN_A=1 RUN_B=2 WORD="amore"
make stats-compare-words-content RUN_A=1 RUN_B=2 LIMIT=30 MIN_COUNT=5 SHARED_ONLY=--shared-only
make stats-compare-words-content RUN_A=1 RUN_B=2 LIMIT=30 MIN_COUNT=5 EXCLUSIVE_ONLY=--exclusive-only
```

## Notes

Corpus comparison is lexical. When comparing different languages, CorpusLens does not translate equivalent concepts such as `love` and `amore`; it compares the surface forms stored in each run.

`--shared-only` keeps words with counts on both sides.

`--exclusive-only` keeps words that appear on only one side of the comparison.
