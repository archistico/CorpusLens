using CorpusLens.Analysis.Language;
using CorpusLens.Analysis.StopWords;

namespace CorpusLens.Application.Storage;

public static class CorpusLanguageCatalog
{
    private static readonly IReadOnlyList<CorpusLanguageOption> Languages =
        LanguageProfileProvider.ListProfiles()
            .Where(profile => profile.IsKnown)
            .Select(profile => new CorpusLanguageOption(profile.Code, profile.Name))
            .OrderBy(language => language.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public static IReadOnlyList<CorpusLanguageOption> ListSupportedLanguages()
    {
        return Languages;
    }

    public static bool TryNormalizeSupportedCode(string? languageCode, out string normalizedCode)
    {
        normalizedCode = string.Empty;
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return false;
        }

        string candidate = StopWordProvider.NormalizeLanguageCode(languageCode);
        CorpusLanguageOption? language = Languages.FirstOrDefault(
            item => string.Equals(item.Code, candidate, StringComparison.OrdinalIgnoreCase));
        if (language is null)
        {
            return false;
        }

        normalizedCode = language.Code;
        return true;
    }

    public static string NormalizeSupportedCode(string? languageCode)
    {
        if (TryNormalizeSupportedCode(languageCode, out string normalizedCode))
        {
            return normalizedCode;
        }

        string supported = string.Join(", ", Languages.Select(language => language.Code));
        throw new ArgumentException(
            $"Unsupported corpus language '{languageCode?.Trim()}'. Supported codes: {supported}.",
            nameof(languageCode));
    }
}
