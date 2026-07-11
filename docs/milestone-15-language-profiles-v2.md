# Milestone 15 — Language profiles v2

This milestone introduces explicit language profiles for the languages currently supported by CorpusLens.

## Scope

- Add language profiles for `en`, `it`, `fr` and `de`.
- Expose profile metadata from the CLI.
- Keep stop-word classification compatible with the existing implementation.
- Use language-profile default thresholds for relative difficulty when the user does not pass explicit thresholds.

## Language profile fields

Each profile describes:

- language code;
- language name;
- language family;
- default long-word threshold;
- default very-long-word threshold;
- stop-word count;
- apostrophe handling note;
- tokenization note.

## CLI

List supported profiles:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats language-profiles
```

Show one profile:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats language-profile it
```

Difficulty now uses profile thresholds by default:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats difficulty 1
```

Explicit thresholds still override the profile:

```powershell
dotnet run --project src/CorpusLens.Cli -- stats difficulty 1 --long-word-length 8 --very-long-word-length 12
```

## Notes

This milestone does not change tokenization or the database schema. Italian apostrophized forms such as `dall'androne` and `l'angolo` are still kept as single tokens for now.
