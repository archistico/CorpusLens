# Milestone 14.1 — Difficulty output polish

Small CLI polish for relative difficulty reports.

## Changes

- `stats difficulty` now prints the run language codes.
- `stats difficulty` now prints the active long-word thresholds near the top of the report.
- The single-run report explicitly reminds the user that the heuristic is best used for comparable corpora.
- The metric table keeps both `Content word share` and `Function word share` visible, matching the comparison output.

No database schema, scoring formula, tokenizer, or analysis logic changed.
