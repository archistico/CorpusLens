using Avalonia;
using CorpusLens.Desktop.Services;

namespace CorpusLens.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        IDesktopDiagnosticLog diagnosticLog = DesktopRuntime.DiagnosticLog;
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            Exception exception = eventArgs.ExceptionObject as Exception
                ?? new InvalidOperationException($"Unhandled non-exception object: {eventArgs.ExceptionObject}");
            diagnosticLog.WriteError("Unhandled application-domain exception.", exception);
            DesktopFatalErrorReporter.Report(exception.Message, diagnosticLog.CurrentLogPath);
        };
        TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
        {
            diagnosticLog.WriteError("Unobserved task exception.", eventArgs.Exception);
            eventArgs.SetObserved();
        };

        try
        {
            diagnosticLog.WriteInformation("CorpusLens Desktop starting.");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            diagnosticLog.WriteInformation("CorpusLens Desktop stopped normally.");
        }
        catch (Exception exception)
        {
            diagnosticLog.WriteError("CorpusLens Desktop terminated because of a critical startup/runtime error.", exception);
            DesktopFatalErrorReporter.Report(exception.Message, diagnosticLog.CurrentLogPath);
            Environment.ExitCode = 1;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}
