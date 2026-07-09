using CorpusLens.Application.EpubAnalysis;
using CorpusLens.Application.TextAnalysis;
using CorpusLens.Domain.Analysis;

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
                "analyze-text" => await AnalyzeTextFileAsync(commandArgs).ConfigureAwait(false),
                "analyze-epub" => await AnalyzeEpubAsync(commandArgs).ConfigureAwait(false),
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

    private static void WriteHelp()
    {
        Console.WriteLine("CorpusLens");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  demo [--out <dir>]");
        Console.WriteLine("  analyze-text <file> [--language <code>] [--title <title>] [--out <dir>]");
        Console.WriteLine("  analyze-epub <file.epub> [--language <code>] [--out <dir>]");
        Console.WriteLine();
        WriteAnalyzeTextHelp();
        WriteAnalyzeEpubHelp();
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
        Console.WriteLine();
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
    }
}
