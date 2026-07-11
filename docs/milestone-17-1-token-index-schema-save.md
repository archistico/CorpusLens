# Milestone 17.1 — Token index schema + save

This milestone adds the first persistent token index.

## Scope

- Adds `TokenOccurrence` to SQLite.
- Saves word-token occurrences when an analysis run is saved.
- Adds `stats token-index <runId>` to inspect whether a run is indexed.
- Keeps existing query commands on their current paths for now.

## Token fields

Each stored token occurrence includes:

- analysis run, corpus, book and chapter ids;
- chapter order;
- global run position;
- chapter-local position;
- original token text;
- normalized token text;
- stopword flag;
- start/end offsets in the chapter clean text.

## Notes

For folder runs, the index currently follows the aggregate run book, which is also what existing KWIC, collocation and phrase commands use. Query migration to the token index will be handled in later milestones.
