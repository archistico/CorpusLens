# Milestone 17.3 — Collocations from token index

`stats collocations` now uses the persistent `TokenOccurrence` index when it is available for the selected run.

## Behavior

- Indexed runs use `TokenOccurrence` for target lookup and collocation windows.
- Legacy runs without token index automatically fall back to the previous chapter-text implementation.
- CLI arguments and output remain unchanged.
- Collocations still do not cross chapter boundaries.
- Dice ranking, raw counts, left/right counts and average distance are preserved.

## Notes

The token-index path calculates collocation windows directly from chapter token positions. It no longer needs to tokenize each chapter text at query time.

The query still uses the same normalized token form as KWIC, word detail and phrase mining.
