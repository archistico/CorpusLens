namespace CorpusLens.Application.Storage;

public sealed record CorpusLanguageOption(
    string Code,
    string DisplayName)
{
    public override string ToString()
    {
        return $"{DisplayName} ({Code})";
    }
}
