# Milestone 10.1 — Word-books output consistency

This micro-milestone polishes the `stats word-books` command output without changing storage, tokenization or word-count logic.

## Goal

The word distribution header must be consistent for every word, including words that occur in many books and words with no matches.

## Output changes

The command now always prints:

- `Source books`
- `Matched books`
- `Coverage`
- `Total count`
- `Shown books`

Example:

```text
Word distribution for 'don't' in run 1
Source books:  26
Matched books: 23 of 26
Coverage:      88.46%
Total count:   2989
Shown books:   23 of 23
```

For a missing word:

```text
Word distribution for 'notaword' in run 1
Source books:  26
Matched books: 0 of 26
Coverage:      0.00%
Total count:   0
Shown books:   0 of 0
No matching source books found.
```

## Notes

- This is CLI-only output polish.
- The underlying `ListWordBookDistributionAsync` query is unchanged.
- The command remains compatible with aggregate runs created in Milestone 9.
