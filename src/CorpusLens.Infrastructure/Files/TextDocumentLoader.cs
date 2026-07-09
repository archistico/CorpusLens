using CorpusLens.Domain.Text;

namespace CorpusLens.Infrastructure.Files;

public sealed class TextDocumentLoader
{
    public async Task<TextDocument> LoadAsync(
        string filePath,
        string languageCode,
        string? title,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        string fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Text file not found.", fullPath);
        }

        string content = await File.ReadAllTextAsync(fullPath, cancellationToken).ConfigureAwait(false);
        string documentTitle = string.IsNullOrWhiteSpace(title)
            ? Path.GetFileNameWithoutExtension(fullPath)
            : title.Trim();

        return new TextDocument(fullPath, documentTitle, languageCode.Trim(), content);
    }
}
