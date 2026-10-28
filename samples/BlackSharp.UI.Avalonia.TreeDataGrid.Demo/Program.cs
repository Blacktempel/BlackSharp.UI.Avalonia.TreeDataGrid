using Avalonia;
using System.Diagnostics;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo
{
    internal class Program
    {
        #region Fields

        public static Stopwatch? Stopwatch;

        #endregion

        #region Public

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            try
            {
                Stopwatch = Stopwatch.StartNew();
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                Environment.ExitCode = 1;

                try
                {
                    File.WriteAllText(Path.Combine(Path.GetTempPath(), "BlackSharp.UI.Avalonia.TreeDataGrid.Demo-error.log"), exception.ToString());
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();

        #endregion
    }
}
