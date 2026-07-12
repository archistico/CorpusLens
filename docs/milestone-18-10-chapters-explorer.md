# Milestone 18.10 — Chapters explorer

Adds read-only chapter navigation to the Avalonia desktop application.

## Navigation flow

```text
Run → Source book → Chapter → Persisted clean-text preview
```

Selecting a run still loads its dashboard and source books. The first source book is selected automatically and its chapters are loaded in their original `OrderIndex` sequence. Selecting another book starts a cancellable background load for that book only.

## Application query service

`ChapterExplorerQueryService` reuses `SqliteCorpusStore.ListChaptersAsync`; the Desktop project contains no SQL. The service returns the persisted chapter fields together with derived preview metrics:

- character count;
- word-token count using the existing `Tokenizer`;
- sentence count using the existing `SentenceSplitter`;
- empty, very-short and very-long indicators;
- a conservative potentially-suspicious indicator for strong boilerplate or contents markers.

The clean text is not re-imported or rewritten. The preview displays the `Chapter.CleanText` value already stored in SQLite.

## Desktop ViewModel

`ChaptersExplorerViewModel` owns:

- the ordered chapter list;
- selected-chapter details;
- the clean-text preview;
- chapter-level aggregate metrics;
- quality-warning summaries;
- case-insensitive in-preview search;
- previous/next match navigation with wrap-around;
- selection offsets used by the read-only Avalonia `TextBox` to highlight the active match.

The query delegate remains injectable so chapter loading and search behavior can be tested without a physical database.

## Quality indicators

The initial diagnostics are intentionally transparent and deterministic:

- **empty**: no persisted readable text or zero characters;
- **very short**: fewer than 200 persisted characters;
- **very long**: at least 100,000 persisted characters;
- **potentially suspicious**: strong contents, copyright, ISBN or Project Gutenberg markers.

These indicators do not alter the corpus. They only help identify chapters worth reviewing.

## Tests

Desktop ViewModel tests cover:

- ordering chapters by the original EPUB position;
- automatic selection of the first chapter;
- aggregate and warning summaries;
- case-insensitive search;
- active-match offsets;
- next/previous navigation and wrap-around behavior.

## Scope

This milestone is read-only. It does not edit chapter text, re-run import cleaning, or change the SQLite schema.
