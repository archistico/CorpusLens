# Milestone 18.11 — N-gram explorer

Adds read-only n-gram exploration to the Avalonia desktop application.

## Query path

```text
MainWindowViewModel
  -> NGramExplorerViewModel
    -> NGramExplorerQueryService
      -> SqliteCorpusStore.ListNGramsAsync
        -> NGramStatistic
```

The Desktop project contains no SQL. Filtering that belongs to persisted statistics is performed by the Infrastructure query; language-aware content/function classification is performed by the Application service.

## Persisted-statistic filters

`SqliteCorpusStore.ListNGramsAsync` supports:

- analysis run;
- optional exact n size;
- minimum occurrence count;
- optional exact contained word or contiguous phrase;
- result limit;
- ordering by count, frequency per million, document count or text.

The contained-term condition uses padded n-gram text, so searching `he` does not accidentally match the token `the`. Search input is whitespace-normalized and lower-cased before being passed as a SQLite parameter.

## Language-aware composition

The query service reads the languages of the run's source books and classifies each word with the existing `StopWordProvider`. Every result exposes a transparent pattern:

```text
C-C     content/content
F-C     function/content
C-F-C   content/function/content
```

Available filters are:

- all n-grams;
- content words only;
- function words only;
- content-word boundary, where the first and final words are content words.

The pattern is descriptive only. No token or stored statistic is modified.

## Desktop behavior

The panel provides quick size selectors for all stored sizes, bigrams, trigrams, 4-grams and 5-grams. Users can also set a minimum count, result limit, contained term, composition filter and sort order.

Loading runs through the centralized cancellable desktop operation coordinator. Results display:

- n-gram text;
- n size;
- raw count;
- document count;
- frequency per million;
- content/function pattern.

The result control is read-only but supports normal text selection and `Ctrl+C` copying.

## Tests

Tests cover:

- parameterized SQLite filtering by n, count and contained term;
- case-normalized contained-term search;
- forwarding ViewModel size, threshold, filter and sort options;
- formatted counts and word patterns;
- language-aware content-boundary filtering through a real temporary SQLite database.

## Scope

This milestone reads existing `NGramStatistic` rows. It does not recompute n-grams, modify the database schema or add a new export format.
