using Avalonia;
using Avalonia.Headless;
using BlackSharp.UI.Avalonia.TreeDataGrid.Tests;

[assembly: AvaloniaTestApplication(typeof(TestApplication))]

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Tests
{
    public class TestApplication : Application
    {
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true });
    }
}
