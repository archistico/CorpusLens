# Milestone 11.2 — Collocation ranking score

Adds a simple association score to collocation output.

## What changed

`stats collocations` still shows raw counts, side counts and average distance, but now also shows:

```text
Dice
```

The score is used as the default ranking key so that characteristic collocates are promoted above very frequent function words.

## Why

Raw frequency alone tends to rank words like `the`, `a`, `and`, `of` too highly. The score keeps counts visible but makes outputs such as these easier to read:

```text
sperm whale
white whale
right whale
said alice
don't know
```

## Notes

The score is intentionally lightweight and is computed from the text already stored in `Chapter.CleanText`. No persistent token index is introduced in this milestone.

## Dice note

The displayed `Count` column is the raw number of target/collocate window co-occurrences. The `Dice` score is bounded with the target and collocate corpus frequencies before scoring, so manual calculations from the displayed raw `Count` alone may not match exactly when one token occurrence falls inside multiple target windows.

