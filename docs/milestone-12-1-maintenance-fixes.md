# Milestone 12.1 — Maintenance fixes

Small corrective milestone after the first phrase-mining pass.

## Included fixes

- `stats books` now falls back to the run book for single-book runs that do not have `AnalysisRunBook` rows.
- Sentence classification no longer treats every internal `?` or `!` as the category of the whole sentence.
- Leading quoted dialogue such as `"Who are you?" said Alice.` is still classified as a question or exclamation.
- N-gram generation no longer creates artificial adjacency across words removed by `MinWordLength`.
- Removed a duplicated Liber Liber metadata condition.
- Documented that collocation `Count` is raw while Dice is bounded for scoring.

## Not changed

- No database schema change.
- No tokenizer change.
- No CLI command rename.
- No persistent token index yet.
