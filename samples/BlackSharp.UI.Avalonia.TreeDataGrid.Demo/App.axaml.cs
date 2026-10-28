using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo
{
    public class App : Application
    {
        #region Public

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            Bogus.Randomizer.Seed = new System.Random(0);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            this.AttachDeveloperTools();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        #endregion
    }
}
