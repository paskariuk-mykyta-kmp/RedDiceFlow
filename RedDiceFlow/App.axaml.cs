using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Material.Icons.Avalonia;
using RedDiceFlow.ViewModels;
using RedDiceFlow.Views;

namespace RedDiceFlow
{
    public partial class App : Application
    {
        public static bool IsUkrainianStatic { get; set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            Styles.Add(new MaterialIconStyles(null));
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }
            base.OnFrameworkInitializationCompleted();
        }

        public static void SwitchTheme(bool isLight)
        {
            var app = (App)Current!;
            var dicts = app.Resources.MergedDictionaries;
            dicts.Clear();

            var uri = isLight
                ? new Uri("avares://RedDiceFlow/Resources/LightTheme.axaml")
                : new Uri("avares://RedDiceFlow/Resources/DarkTheme.axaml");

            dicts.Add(new ResourceInclude(uri) { Source = uri });
        }

        public static void SwitchLanguage(bool isUkrainian)
        {
            var app = (App)Current!;
            var uri = isUkrainian
                ? new Uri("avares://RedDiceFlow/Resources/Lang_ua.axaml")
                : new Uri("avares://RedDiceFlow/Resources/Lang_en.axaml");

            var langDict = new ResourceInclude(uri) { Source = uri };

            // Find and replace the language dictionary (second merged dict after theme)
            var dicts = app.Resources.MergedDictionaries;
            var themeDict = dicts.Count > 0 ? dicts[0] : null;
            dicts.Clear();
            if (themeDict != null)
                dicts.Add(themeDict);
            dicts.Add(langDict);
        }
    }
}
