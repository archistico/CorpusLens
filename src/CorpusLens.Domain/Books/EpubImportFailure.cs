namespace CorpusLens.Domain.Books;

public sealed record EpubImportFailure(
    string FilePath,
    string FileName,
    string ErrorMessage,
    string ExceptionType);
