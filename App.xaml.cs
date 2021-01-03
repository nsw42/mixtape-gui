using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MixtapeGui.ViewModels;
using MixtapeGui.Views;

namespace MixtapeGui
{
    public class App : Application
    {
        public static string TempDirectory = Path.Join(Path.GetTempPath(), "mixtape");

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (!Directory.Exists(App.TempDirectory))
            {
                Directory.CreateDirectory(App.TempDirectory);
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
