using CorpusLens.Analysis.StopWords;

namespace CorpusLens.Analysis.Language;

public static class LanguageProfileProvider
{
    private static readonly IReadOnlyDictionary<string, LanguageProfileDefinition> Definitions =
        new Dictionary<string, LanguageProfileDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new(
                "en",
                "English",
                "Germanic",
                7,
                10,
                "internal apostrophes are kept in tokens",
                "Keeps contractions and possessives as single normalized tokens, e.g. don't and alice's."),
            ["it"] = new(
                "it",
                "Italian",
                "Romance",
                8,
                12,
                "internal apostrophes are kept in tokens",
                "Keeps elided forms as single normalized tokens for now, e.g. dall'androne and l'angolo."),
            ["fr"] = new(
                "fr",
                "French",
                "Romance",
                8,
                12,
                "internal apostrophes are kept in tokens",
                "Keeps elided forms as single normalized tokens for now, e.g. l'homme and qu'il."),
            ["de"] = new(
                "de",
                "German",
                "Germanic",
                10,
                14,
                "apostrophes are rare and kept when present",
                "Uses longer default word-length thresholds because compounds are common in German.")
        };

    public static IReadOnlyList<LanguageProfile> ListProfiles()
    {
        return Definitions.Values
            .Select(CreateKnownProfile)
            .OrderBy(profile => profile.Code, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static LanguageProfile GetProfile(string? languageCode)
    {
        string normalizedCode = StopWordProvider.NormalizeLanguageCode(languageCode ?? string.Empty);
        if (Definitions.TryGetValue(normalizedCode, out LanguageProfileDefinition? definition))
        {
            return CreateKnownProfile(definition);
        }

        return new LanguageProfile(
            string.IsNullOrWhiteSpace(normalizedCode) ? "unknown" : normalizedCode,
            "Unknown",
            "Unknown",
            7,
            10,
            "unknown",
            "No dedicated language profile is available; generic defaults are used.",
            0,
            false);
    }

    private static LanguageProfile CreateKnownProfile(LanguageProfileDefinition definition)
    {
        return new LanguageProfile(
            definition.Code,
            definition.Name,
            definition.Family,
            definition.DefaultLongWordLength,
            definition.DefaultVeryLongWordLength,
            definition.ApostropheHandling,
            definition.TokenizationNotes,
            StopWordProvider.CountStopWords(definition.Code),
            true);
    }

    private sealed record LanguageProfileDefinition(
        string Code,
        string Name,
        string Family,
        int DefaultLongWordLength,
        int DefaultVeryLongWordLength,
        string ApostropheHandling,
        string TokenizationNotes);
}
