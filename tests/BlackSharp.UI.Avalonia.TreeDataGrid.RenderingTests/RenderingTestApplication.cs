using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;

[assembly: AvaloniaTestApplication(typeof(BlackSharp.UI.Avalonia.TreeDataGrid.RenderingTests.RenderingTestApplication))]

namespace BlackSharp.UI.Avalonia.TreeDataGrid.RenderingTests
{
    public class RenderingTestApplication : Application
    {
        #region Public

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<RenderingTestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing     = false,
                ShouldRenderOnUIThread = true,
                FrameBufferFormat     = PixelFormat.Bgra8888,
            })
            .UseSkia()
            .WithInterFont();

        public override void Initialize()
        {
            var assemblyName = typeof(TreeDataGrid).Assembly.GetName().Name;

            Styles.Add(new FluentTheme());
            Styles.Add(new StyleInclude(new Uri($"avares://{assemblyName}/"))
            {
                Source = new Uri($"avares://{assemblyName}/Themes/Fluent.axaml"),
            });
            Bogus.Randomizer.Seed = new Random(0);
        }

        #endregion
    }
}
