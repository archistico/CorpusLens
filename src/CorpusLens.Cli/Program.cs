using System.Globalization;
using CorpusLens.Application.EpubAnalysis;
using CorpusLens.Application.Storage;
using CorpusLens.Application.TextAnalysis;
using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Books;
using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Cli;

public static class Program
{
    private const string DemoText = """
        Hello, Tom.
        Hello, Anna.
        I don't know.
        I don't know what you mean.
        Do you know Anna?
        No, I don't.
        Could you help me, please?
        """;

    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] is "--help" or "-h" or "help")
            {
                WriteHelp();
                return 0;
            }

            string command = args[0];
            string[] commandArgs = args.Skip(1).ToArray();

            return command switch
            {
                "demo" => await RunDemoAsync(commandArgs).ConfigureAwait(false),
                "corpus" => await RunCorpusAsync(commandArgs).ConfigureAwait(false),
                "stats" => await RunStatsAsync(commandArgs).ConfigureAwait(false),
                "analyze-text" => await AnalyzeTextFileAsync(commandArgs).ConfigureAwait(false),
                "analyze-epub" => await AnalyzeEpubAsync(commandArgs).ConfigureAwait(false),
                "analyze-epub-folder" => await AnalyzeEpubFolderAsync(commandArgs).ConfigureAwait(false),
                _ => UnknownCommand(command)
            };
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"ERROR: {exception.Message}");
            return 1;
        }
    }

    private static async Task<int> RunDemoAsync(string[] args)
    {
        CommandLineOptions options = CommandLineOptions.Parse(args);
        string outputDirectory = options.Get("out", "./artifacts/demo");

        AnalyzeTextUseCase useCase = new();
        AnalyzeTextResult result = await useCase.ExecuteAsync(new AnalyzeTextRequest(
            DemoText,
            "demo",
            "CorpusLens Demo",
            "en",
            outputDirectory,
            DefaultSettings()))
            .ConfigureAwait(false);

        WriteResult(result);
        return 0;
    }

    private static async Task<int> RunCorpusAsync(string[] args)
    {
        if (args.Length == 0)
        {
            WriteCorpusHelp();
            return 1;
        }

        string subCommand = args[0];
        string[] subCommandArgs = args.Skip(1).ToArray();

        return subCommand switch
        {
            "create" => await CreateCorpusAsync(subCommandArgs).ConfigureAwait(false),
            "list" => await ListCorporaAsync(subCommandArgs).ConfigureAwait(false),
            _ => UnknownCorpusCommand(subCommand)
        };
    }

    private static async Task<int> CreateCorpusAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Missing corpus name.");
            Console.Error.WriteLine();
            WriteCorpusHelp();
            return 1;
        }

        string name = args[0];
        CommandLineOptions options = CommandLineOptions.Parse(args.Skip(1).ToArray());
        string languageCode = options.Get("language", "en");
        string? description = options.TryGet("description");
        string databasePath = options.Get("db", DefaultDatabasePath());

        CreateCorpusUseCase useCase = new();
        StoredCorpus corpus = await useCase.ExecuteAsync(new CreateCorpusRequest(
            databasePath,
            name,
            languageCode,
            description))
            .ConfigureAwait(false);

        Console.WriteLine("Corpus created.");
        Console.WriteLine($"Id:       {corpus.Id}");
        Console.WriteLine($"Name:     {corpus.Name}");
        Console.WriteLine($"Language: {corpus.LanguageCode}");
        Console.WriteLine($"Database: {databasePath}");
        return 0;
    }

    private static async Task<int> ListCorporaAsync(string[] args)
    {
        CommandLineOptions options = CommandLineOptions.Parse(args);
        string databasePath = options.Get("db", DefaultDatabasePath());

        ListCorporaUseCase useCase = new();
        IReadOnlyList<StoredCorpus> corpora = await useCase.ExecuteAsync(new ListCorporaRequest(databasePath))
            .ConfigureAwait(false);

        Console.WriteLine($"Database: {databasePath}");
        if (corpora.Count == 0)
        {
            Console.WriteLine("No corpora found.");
            return 0;
        }

        Console.WriteLine();
        Console.WriteLine("Id  Language  Name");
        Console.WriteLine("--  --------  ----");
        foreach (StoredCorpus corpus in corpora)
        {
            Console.WriteLine($"{corpus.Id,-2}  {corpus.LanguageCode,-8}  {corpus.Name}");
        }

        return 0;
    }

    private static int UnknownCorpusCommand(string command)
    {
        Console.Error.WriteLine($"Unknown corpus command: {command}");
        Console.Error.WriteLine();
        WriteCorpusHelp();
        return 1;
    }

    private static async Task<int> RunStatsAsync(string[] args)
    {
        if (args.Length == 0)
        {
            WriteStatsHelp();
            return 1;
        }

        string subCommand = args[0];
        string[] subCommandArgs = args.Skip(1).ToArray();

        return subCommand switch
        {
            "runs" => await PrintAnalysisRunsAsync(subCommandArgs).ConfigureAwait(false),
            "summary" => await PrintAnalysisRunSummaryAsync(subCommandArgs).ConfigureAwait(false),
            "words" => await PrintTopWordsAsync(subCommandArgs).ConfigureAwait(false),
            "word" => await PrintWordDetailAsync(subCommandArgs).ConfigureAwait(false),
            "kwic" => await PrintWordContextsAsync(subCommandArgs).ConfigureAwait(false),
            "ngrams" => await PrintTopNGramsAsync(subCommandArgs).ConfigureAwait(false),
            "next" => await PrintTopNextWordsAsync(subCommandArgs).ConfigureAwait(false),
            "categories" => await PrintSentenceCategoriesAsync(subCommandArgs).ConfigureAwait(false),
            _ => UnknownStatsCommand(subCommand)
        };
    }


    private static async Task<int> PrintAnalysisRunsAsync(string[] args)
    {
        CommandLineOptions options = CommandLineOptions.Parse(args);
        SqliteCorpusStore store = new(options.Get("db", DefaultDatabasePath()));
        long? corpusId = TryReadLongOption(options, "corpus-id");
        IReadOnlyList<StoredAnalysisRunSummary> runs = await store
            .ListAnalysisRunsAsync(corpusId, ReadLimit(options))
            .ConfigureAwait(false);

        Console.WriteLine("Run  Corpus               Book/Source                       Started              Words      Sentences  Status");
        Console.WriteLine("---  -------------------  --------------------------------  -------------------  ---------  ---------  ---------");
        foreach (StoredAnalysisRunSummary run in runs)
        {
            Console.WriteLine($"{run.Id,3}  {TrimForColumn(run.CorpusName, 19),-19}  {TrimForColumn(run.BookTitle, 32),-32}  {FormatDateTime(run.StartedAt),-19}  {run.WordTokenCount,9}  {run.SentenceCount,9}  {run.Status}");
        }

        if (runs.Count == 0)
        {
            Console.WriteLine("No analysis runs found.");
        }

        return 0;
    }

    private static async Task<int> PrintAnalysisRunSummaryAsync(string[] args)
    {
        if (!TryReadRunId(args, out long analysisRunId))
        {
            WriteStatsHelp();
            return 1;
        }

        CommandLineOptions options = CommandLineOptions.Parse(args.Skip(1).ToArray());
        SqliteCorpusStore store = new(options.Get("db", DefaultDatabasePath()));
        StoredAnalysisRunSummary? run = await store
            .GetAnalysisRunSummaryAsync(analysisRunId)
            .ConfigureAwait(false);

        if (run is null)
        {
            Console.Error.WriteLine($"Analysis run {analysisRunId} was not found.");
            return 1;
        }

        Console.WriteLine($"Run Id:                   {run.Id}");
        Console.WriteLine($"Corpus:                   {run.CorpusName} ({run.CorpusId})");
        Console.WriteLine($"Book/Source:              {run.BookTitle} ({run.BookId})");
        Console.WriteLine($"Started:                  {FormatDateTime(run.StartedAt)}");
        Console.WriteLine($"Completed:                {FormatDateTime(run.CompletedAt)}");
        Console.WriteLine($"Status:                   {run.Status}");
        Console.WriteLine($"Sentences:                {run.SentenceCount}");
        Console.WriteLine($"Tokens:                   {run.TokenCount}");
        Console.WriteLine($"Word tokens:              {run.WordTokenCount}");
        Console.WriteLine($"Distinct words:           {run.DistinctWordCount}");
        Console.WriteLine($"Avg words per sentence:   {FormatDouble(run.AverageWordsPerSentence)}");
        Console.WriteLine($"Avg chars per word:       {FormatDouble(run.AverageCharactersPerWord)}");
        Console.WriteLine($"Report:                   {run.ReportPath}");
        return 0;
    }

    private static async Task<int> PrintTopWordsAsync(string[] args)
    {
        if (!TryReadRunId(args, out long analysisRunId))
        {
            WriteStatsHelp();
            return 1;
        }

        CommandLineOptions options = CommandLineOptions.Parse(args.Skip(1).ToArray());
        StoredWordFilter filter = ReadWordFilter(options);
        SqliteCorpusStore store = new(options.Get("db", DefaultDatabasePath()));
        IReadOnlyList<StoredWordStatistic> words = await store
            .ListTopWordsAsync(analysisRunId, ReadLimit(options), filter)
            .ConfigureAwait(false);

        Console.WriteLine("Word                 Count  Documents  Per million  Type");
        Console.WriteLine("-------------------  -----  ---------  -----------  --------");
        foreach (StoredWordStatistic word in words)
        {
            Console.WriteLine($"{TrimForColumn(word.Word, 19),-19}  {word.Count,5}  {word.DocumentCount,9}  {FormatDouble(word.FrequencyPerMillion),11}  {WordTypeLabel(word),-8}");
        }

        return 0;
    }

    private static async Task<int> PrintWordDetailAsync(string[] args)
    {
        if (!TryReadRunId(args, out long analysisRunId) || args.Length < 2)
        {
            Console.Error.WriteLine("Usage: stats word <runId> <word> [--limit <n>] [--db <file>]");
            return 1;
        }

        string wordText = args[1];
        CommandLineOptions options = CommandLineOptions.Parse(args.Skip(2).ToArray());
        int limit = ReadLimit(options);
        SqliteCorpusStore store = new(options.Get("db", DefaultDatabasePath()));

        StoredWordStatistic? word = await store
            .GetWordStatisticAsync(analysisRunId, wordText)
            .ConfigureAwait(false);

        if (word is null)
        {
            Console.Error.WriteLine($"Word '{wordText}' was not found in analysis run {analysisRunId}.");
            return 1;
        }

        IReadOnlyList<StoredNextWordStatistic> nextWords = await store
            .ListTopNextWordsAsync(analysisRunId, word.Word, limit)
            .ConfigureAwait(false);
        IReadOnlyList<StoredNextWordStatistic> previousWords = await store
            .ListPreviousWordsAsync(analysisRunId, word.Word, limit)
            .ConfigureAwait(false);

        Console.WriteLine($"Word:          {word.Word}");
        Console.WriteLine($"Type:          {WordTypeLabel(word)}");
        Console.WriteLine($"Count:         {word.Count}");
        Console.WriteLine($"Documents:     {word.DocumentCount}");
        Console.WriteLine($"Per million:   {FormatDouble(word.FrequencyPerMillion)}");
        Console.WriteLine();

        Console.WriteLine("Next words");
        Console.WriteLine("Next word            Count  Probability");
        Console.WriteLine("-------------------  -----  -----------");
        foreach (StoredNextWordStatistic nextWord in nextWords)
        {
            Console.WriteLine($"{TrimForColumn(nextWord.NextWord, 19),-19}  {nextWord.Count,5}  {FormatDouble(nextWord.Probability),11}");
        }

        if (nextWords.Count == 0)
        {
            Console.WriteLine("No next-word statistics found.");
        }

        Console.WriteLine();
        Console.WriteLine("Previous words");
        Console.WriteLine("Previous word        Count  Probability");
        Console.WriteLine("-------------------  -----  -----------");
        foreach (StoredNextWordStatistic previousWord in previousWords)
        {
            Console.WriteLine($"{TrimForColumn(previousWord.Word, 19),-19}  {previousWord.Count,5}  {FormatDouble(previousWord.Probability),11}");
        }

        if (previousWords.Count == 0)
        {
            Console.WriteLine("No previous-word statistics found.");
        }

        return 0;
    }

    private static async Task<int> PrintWordContextsAsync(string[] args)
    {
        if (!TryReadRunId(args, out long analysisRunId) || args.Length < 2)
        {
            Console.Error.WriteLine("Usage: stats kwic <runId> <word> [--limit <n>] [--context <n>] [--db <file>]");
            return 1;
        }

        string wordText = args[1];
        CommandLineOptions options = CommandLineOptions.Parse(args.Skip(2).ToArray());
        int contextWords = TryReadIntOption(options, "context") ?? 8;
        SqliteCorpusStore store = new(options.Get("db", DefaultDatabasePath()));

        IReadOnlyList<StoredWordContext> contexts = await store
            .ListWordContextsAsync(analysisRunId, wordText, ReadLimit(options), contextWords)
            .ConfigureAwait(false);

        Console.WriteLine($"KWIC contexts for '{wordText}' in run {analysisRunId}");
        Console.WriteLine($"Context words per side: {Math.Clamp(contextWords, 1, 30)}");
        Console.WriteLine();
        Console.WriteLine("#    Chapter                         Left context                         Match           Right context");
        Console.WriteLine("---  ------------------------------  -----------------------------------  --------------  -----------------------------------");

        for (int index = 0; index < contexts.Count; index++)
        {
            StoredWordContext context = contexts[index];
            string chapter = string.IsNullOrWhiteSpace(context.ChapterTitle)
                ? $"Chapter {context.ChapterOrderIndex}"
                : context.ChapterTitle;

            Console.WriteLine(
                $"{index + 1,3}  {TrimForColumn(chapter, 30),-30}  {TrimForColumn(context.LeftContext, 35),-35}  {TrimForColumn(context.MatchText, 14),-14}  {TrimForColumn(context.RightContext, 35),-35}");
        }

        if (contexts.Count == 0)
        {
            Console.WriteLine("No contexts found.");
        }

        return 0;
    }


    private static async Task<int> PrintTopNGramsAsync(string[] args)
    {
        if (!TryReadRunId(args, out long analysisRunId))
        {
            WriteStatsHelp();
            return 1;
        }

        CommandLineOptions options = CommandLineOptions.Parse(args.Skip(1).ToArray());
        int? n = TryReadIntOption(options, "n");
        SqliteCorpusStore store = new(options.Get("db", DefaultDatabasePath()));
        IReadOnlyList<StoredNGramStatistic> ngrams = await store
            .ListTopNGramsAsync(analysisRunId, n, ReadLimit(options))
            .ConfigureAwait(false);

        Console.WriteLine("N  Text                             Count  Documents  Per million");
        Console.WriteLine("-  -------------------------------  -----  ---------  -----------");
        foreach (StoredNGramStatistic ngram in ngrams)
        {
            Console.WriteLine($"{ngram.N,1}  {TrimForColumn(ngram.Text, 31),-31}  {ngram.Count,5}  {ngram.DocumentCount,9}  {FormatDouble(ngram.FrequencyPerMillion),11}");
        }

        return 0;
    }

    private static async Task<int> PrintTopNextWordsAsync(string[] args)
    {
        if (!TryReadRunId(args, out long analysisRunId))
        {
            WriteStatsHelp();
            return 1;
        }

        CommandLineOptions options = CommandLineOptions.Parse(args.Skip(1).ToArray());
        string? word = options.TryGet("word");
        SqliteCorpusStore store = new(options.Get("db", DefaultDatabasePath()));
        IReadOnlyList<StoredNextWordStatistic> nextWords = await store
            .ListTopNextWordsAsync(analysisRunId, word, ReadLimit(options))
            .ConfigureAwait(false);

        Console.WriteLine("Word                 Next word            Count  Probability");
        Console.WriteLine("-------------------  -------------------  -----  -----------");
        foreach (StoredNextWordStatistic nextWord in nextWords)
        {
            Console.WriteLine($"{TrimForColumn(nextWord.Word, 19),-19}  {TrimForColumn(nextWord.NextWord, 19),-19}  {nextWord.Count,5}  {FormatDouble(nextWord.Probability),11}");
        }

        return 0;
    }

    private static async Task<int> PrintSentenceCategoriesAsync(string[] args)
    {
        if (!TryReadRunId(args, out long analysisRunId))
        {
            WriteStatsHelp();
            return 1;
        }

        CommandLineOptions options = CommandLineOptions.Parse(args.Skip(1).ToArray());
        SqliteCorpusStore store = new(options.Get("db", DefaultDatabasePath()));
        IReadOnlyList<StoredSentenceCategoryStatistic> categories = await store
            .ListSentenceCategoryStatisticsAsync(analysisRunId)
            .ConfigureAwait(false);

        Console.WriteLine("Category      Count  Percentage");
        Console.WriteLine("------------  -----  ----------");
        foreach (StoredSentenceCategoryStatistic category in categories)
        {
            Console.WriteLine($"{category.Category,-12}  {category.Count,5}  {FormatDouble(category.Percentage),10}");
        }

        return 0;
    }

    private static int UnknownStatsCommand(string command)
    {
        Console.Error.WriteLine($"Unknown stats command: {command}");
        Console.Error.WriteLine();
        WriteStatsHelp();
        return 1;
    }

    private static async Task<int> AnalyzeTextFileAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Missing text file path.");
            Console.Error.WriteLine();
            WriteAnalyzeTextHelp();
            return 1;
        }

        string filePath = args[0];
        CommandLineOptions options = CommandLineOptions.Parse(args.Skip(1).ToArray());

        string languageCode = options.Get("language", "en");
        string? title = options.TryGet("title");
        string outputDirectory = options.Get("out", "./artifacts/text-analysis");

        AnalyzeTextFileUseCase useCase = new();
        AnalyzeTextResult result = await useCase.ExecuteAsync(new AnalyzeTextFileRequest(
            filePath,
            languageCode,
            title,
            outputDirectory,
            DefaultSettings()))
            .ConfigureAwait(false);

        WriteResult(result);
        return 0;
    }

    private static async Task<int> AnalyzeEpubAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Missing EPUB file path.");
            Console.Error.WriteLine();
            WriteAnalyzeEpubHelp();
            return 1;
        }

        string filePath = args[0];
        CommandLineOptions options = CommandLineOptions.Parse(args.Skip(1).ToArray());

        string languageCode = options.Get("language", "en");
        string outputDirectory = options.Get("out", "./artifacts/epub-analysis");
        string? corpusName = options.TryGet("corpus");

        if (string.IsNullOrWhiteSpace(corpusName))
        {
            AnalyzeEpubUseCase useCase = new();
            AnalyzeEpubResult result = await useCase.ExecuteAsync(new AnalyzeEpubRequest(
                filePath,
                languageCode,
                outputDirectory,
                DefaultSettings()))
                .ConfigureAwait(false);

            WriteResult(result);
            return 0;
        }

        string databasePath = options.Get("db", DefaultDatabasePath());
        AnalyzeEpubAndSaveUseCase saveUseCase = new();
        AnalyzeEpubAndSaveResult savedResult = await saveUseCase.ExecuteAsync(new AnalyzeEpubAndSaveRequest(
            filePath,
            languageCode,
            outputDirectory,
            DefaultSettings(),
            databasePath,
            corpusName))
            .ConfigureAwait(false);

        WriteResult(savedResult, databasePath);
        return 0;
    }

    private static async Task<int> AnalyzeEpubFolderAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Missing EPUB folder path.");
            Console.Error.WriteLine();
            WriteAnalyzeEpubFolderHelp();
            return 1;
        }

        string folderPath = args[0];
        CommandLineOptions options = CommandLineOptions.Parse(args.Skip(1).ToArray());

        string languageCode = options.Get("language", "en");
        string outputDirectory = options.Get("out", "./artifacts/epub-folder-analysis");
        string searchPattern = options.Get("pattern", "*.epub");
        bool recursive = options.Has("recursive");
        string? corpusName = options.TryGet("corpus");

        if (string.IsNullOrWhiteSpace(corpusName))
        {
            AnalyzeEpubFolderUseCase useCase = new();
            AnalyzeEpubFolderResult result = await useCase.ExecuteAsync(new AnalyzeEpubFolderRequest(
                folderPath,
                languageCode,
                outputDirectory,
                DefaultSettings(),
                searchPattern,
                recursive))
                .ConfigureAwait(false);

            WriteResult(result);
            return 0;
        }

        string databasePath = options.Get("db", DefaultDatabasePath());
        AnalyzeEpubFolderAndSaveUseCase saveUseCase = new();
        AnalyzeEpubFolderAndSaveResult savedResult = await saveUseCase.ExecuteAsync(new AnalyzeEpubFolderAndSaveRequest(
            folderPath,
            languageCode,
            outputDirectory,
            DefaultSettings(),
            searchPattern,
            recursive,
            databasePath,
            corpusName))
            .ConfigureAwait(false);

        WriteResult(savedResult, databasePath);
        return 0;
    }

    private static AnalysisSettings DefaultSettings()
    {
        return new AnalysisSettings
        {
            NGramMinN = 2,
            NGramMaxN = 5,
            MinNGramCount = 2,
            TopWordsForNextWordAnalysis = 1000,
            MinNextWordPairCount = 2
        };
    }

    private static string DefaultDatabasePath()
    {
        return "./data/corpuslens.db";
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        Console.Error.WriteLine();
        WriteHelp();
        return 1;
    }

    private static void WriteResult(AnalyzeTextResult result)
    {
        Console.WriteLine("CorpusLens analysis completed.");
        Console.WriteLine($"Report:     {result.ReportPath}");
        Console.WriteLine($"Words CSV:  {result.WordsCsvPath}");
        Console.WriteLine($"NGrams CSV: {result.NGramsCsvPath}");
        Console.WriteLine($"Next CSV:   {result.NextWordsCsvPath}");
    }

    private static void WriteResult(AnalyzeEpubResult result)
    {
        Console.WriteLine("CorpusLens EPUB analysis completed.");
        Console.WriteLine($"Title:      {result.Book.Title}");
        Console.WriteLine($"Author:     {result.Book.Author}");
        Console.WriteLine($"Chapters:   {result.Book.Chapters.Count}");
        Console.WriteLine($"Text:       {result.ExtractedTextPath}");
        Console.WriteLine($"Report:     {result.ReportPath}");
        Console.WriteLine($"Words CSV:  {result.WordsCsvPath}");
        Console.WriteLine($"NGrams CSV: {result.NGramsCsvPath}");
        Console.WriteLine($"Next CSV:   {result.NextWordsCsvPath}");
    }

    private static void WriteResult(AnalyzeEpubAndSaveResult result, string databasePath)
    {
        WriteResult(result.AnalysisResult);
        Console.WriteLine("Database save completed.");
        Console.WriteLine($"Database:   {databasePath}");
        Console.WriteLine($"Corpus Id:  {result.Corpus.Id}");
        Console.WriteLine($"Book Id:    {result.Book.Id}");
        Console.WriteLine($"Run Id:     {result.AnalysisRun.Id}");
    }

    private static void WriteResult(AnalyzeEpubFolderResult result)
    {
        Console.WriteLine("CorpusLens EPUB folder analysis completed.");
        Console.WriteLine($"Books:      {result.SourceBooks.Count}");
        Console.WriteLine($"Skipped:    {result.Failures.Count}");
        Console.WriteLine($"Chapters:   {result.Book.Chapters.Count}");
        Console.WriteLine($"Documents:  {result.Analysis.Summary.DocumentCount}");
        Console.WriteLine($"Text:       {result.ExtractedTextPath}");
        Console.WriteLine($"Report:     {result.ReportPath}");
        Console.WriteLine($"Words CSV:  {result.WordsCsvPath}");
        Console.WriteLine($"NGrams CSV: {result.NGramsCsvPath}");
        Console.WriteLine($"Next CSV:   {result.NextWordsCsvPath}");
        Console.WriteLine($"Failures:   {result.ImportFailuresCsvPath}");

        if (result.Failures.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Skipped EPUB files:");
            foreach (EpubImportFailure failure in result.Failures)
            {
                Console.WriteLine($"- {failure.FileName}: {failure.ErrorMessage}");
            }
        }
    }

    private static void WriteResult(AnalyzeEpubFolderAndSaveResult result, string databasePath)
    {
        WriteResult(result.AnalysisResult);
        Console.WriteLine("Database save completed.");
        Console.WriteLine($"Database:   {databasePath}");
        Console.WriteLine($"Corpus Id:  {result.Corpus.Id}");
        Console.WriteLine($"Book Id:    {result.Book.Id}");
        Console.WriteLine($"Run Id:     {result.AnalysisRun.Id}");
    }

    private static void WriteHelp()
    {
        Console.WriteLine("CorpusLens");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  demo [--out <dir>]");
        Console.WriteLine("  corpus create <name> [--language <code>] [--description <text>] [--db <file>]");
        Console.WriteLine("  corpus list [--db <file>]");
        Console.WriteLine("  stats runs [--corpus-id <id>] [--limit <n>] [--db <file>]");
        Console.WriteLine("  stats summary <runId> [--db <file>]");
        Console.WriteLine("  stats words <runId> [--limit <n>] [--content-only] [--function-only] [--db <file>]");
        Console.WriteLine("  stats word <runId> <word> [--limit <n>] [--db <file>]");
        Console.WriteLine("  stats kwic <runId> <word> [--limit <n>] [--context <n>] [--db <file>]");
        Console.WriteLine("  stats ngrams <runId> [--n <n>] [--limit <n>] [--db <file>]");
        Console.WriteLine("  stats next <runId> [--word <word>] [--limit <n>] [--db <file>]");
        Console.WriteLine("  stats categories <runId> [--db <file>]");
        Console.WriteLine("  analyze-text <file> [--language <code>] [--title <title>] [--out <dir>]");
        Console.WriteLine("  analyze-epub <file.epub> [--language <code>] [--out <dir>] [--corpus <name>] [--db <file>]");
        Console.WriteLine("  analyze-epub-folder <dir> [--language <code>] [--out <dir>] [--corpus <name>] [--db <file>] [--recursive]");
        Console.WriteLine();
        WriteCorpusHelp();
        WriteStatsHelp();
        WriteAnalyzeTextHelp();
        WriteAnalyzeEpubHelp();
        WriteAnalyzeEpubFolderHelp();
    }

    private static void WriteCorpusHelp()
    {
        Console.WriteLine("Corpus examples:");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- corpus create \"English Kids\" --language en");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- corpus list");
        Console.WriteLine();
    }

    private static void WriteStatsHelp()
    {
        Console.WriteLine("Stats examples:");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- stats runs --limit 10");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- stats summary 1");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- stats words 1 --limit 25");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- stats words 1 --content-only --limit 25");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- stats words 1 --function-only --limit 25");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- stats word 1 alice --limit 25");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- stats kwic 1 alice --limit 10 --context 8");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- stats ngrams 1 --n 3 --limit 25");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- stats next 1 --word alice --limit 25");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- stats categories 1");
        Console.WriteLine();
    }

    private static void WriteAnalyzeTextHelp()
    {
        Console.WriteLine("Text examples:");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- demo --out ./artifacts/demo");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- analyze-text ./samples/texts/sample_english_short.txt --language en --title \"Sample English\" --out ./artifacts/sample");
        Console.WriteLine();
    }

    private static void WriteAnalyzeEpubHelp()
    {
        Console.WriteLine("EPUB examples:");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- analyze-epub ./samples/epubs/alice.epub --language en --out ./artifacts/alice");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- analyze-epub ./samples/epubs/alice.epub --language en --corpus \"English Kids\" --out ./artifacts/alice");
        Console.WriteLine();
    }

    private static void WriteAnalyzeEpubFolderHelp()
    {
        Console.WriteLine("EPUB folder examples:");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books --language en --out ./artifacts/books");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books --language en --corpus \"English Kids\" --out ./artifacts/books");
        Console.WriteLine("  dotnet run --project src/CorpusLens.Cli -- analyze-epub-folder ./books --language en --recursive --out ./artifacts/books");
        Console.WriteLine();
    }

    private static bool TryReadRunId(string[] args, out long analysisRunId)
    {
        analysisRunId = 0;
        if (args.Length == 0 || !long.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed))
        {
            Console.Error.WriteLine("Missing or invalid analysis run id.");
            return false;
        }

        analysisRunId = parsed;
        return analysisRunId > 0;
    }

    private static int ReadLimit(CommandLineOptions options)
    {
        return TryReadIntOption(options, "limit") ?? 50;
    }

    private static int? TryReadIntOption(CommandLineOptions options, string key)
    {
        string? value = options.TryGet(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
        {
            throw new InvalidOperationException($"Option --{key} must be an integer.");
        }

        return parsed;
    }




    private static StoredWordFilter ReadWordFilter(CommandLineOptions options)
    {
        bool contentOnly = options.Has("content-only");
        bool functionOnly = options.Has("function-only");

        if (contentOnly && functionOnly)
        {
            throw new InvalidOperationException("Use either --content-only or --function-only, not both.");
        }

        if (contentOnly)
        {
            return StoredWordFilter.ContentOnly;
        }

        return functionOnly ? StoredWordFilter.FunctionOnly : StoredWordFilter.All;
    }

    private static string WordTypeLabel(StoredWordStatistic word)
    {
        return word.IsStopWord ? "function" : "content";
    }

    private static long? TryReadLongOption(CommandLineOptions options, string key)
    {
        string? value = options.TryGet(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed))
        {
            throw new InvalidOperationException($"Option --{key} must be an integer.");
        }

        return parsed;
    }

    private static string FormatDouble(double value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatDateTime(DateTimeOffset value)
    {
        return value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private static string TrimForColumn(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";
    }

    private sealed class CommandLineOptions
    {
        private readonly Dictionary<string, string> _values;

        private CommandLineOptions(Dictionary<string, string> values)
        {
            _values = values;
        }

        public static CommandLineOptions Parse(string[] args)
        {
            Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < args.Length; i++)
            {
                string current = args[i];
                if (!current.StartsWith("--", StringComparison.Ordinal))
                {
                    continue;
                }

                string key = current[2..];
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    values[key] = "true";
                    continue;
                }

                values[key] = args[i + 1];
                i++;
            }

            return new CommandLineOptions(values);
        }

        public string Get(string key, string defaultValue)
        {
            return _values.TryGetValue(key, out string? value) ? value : defaultValue;
        }

        public string? TryGet(string key)
        {
            return _values.TryGetValue(key, out string? value) ? value : null;
        }

        public bool Has(string key)
        {
            return _values.ContainsKey(key);
        }
    }
}
