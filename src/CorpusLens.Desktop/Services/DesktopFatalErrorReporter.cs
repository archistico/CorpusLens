using System.Runtime.InteropServices;

namespace CorpusLens.Desktop.Services;

public static partial class DesktopFatalErrorReporter
{
    public static void Report(string message, string logPath)
    {
        string details = string.Join(
            Environment.NewLine,
            message,
            string.Empty,
            "CorpusLens must close.",
            $"Diagnostic log: {logPath}");

        if (OperatingSystem.IsWindows())
        {
            _ = MessageBox(IntPtr.Zero, details, "CorpusLens — Critical error", 0x00000010U);
            return;
        }

        Console.Error.WriteLine(details);
    }

    [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int MessageBox(IntPtr windowHandle, string text, string caption, uint type);
}
