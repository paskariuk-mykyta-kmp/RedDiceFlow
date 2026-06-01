using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
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
    app.RequestedThemeVariant = isLight ? ThemeVariant.Light : ThemeVariant.Dark;

    var dicts = app.Resources.MergedDictionaries;
    var themeUri = isLight
        ? new Uri("avares://RedDiceFlow/Resources/LightTheme.axaml")
        : new Uri("avares://RedDiceFlow/Resources/DarkTheme.axaml");

    var newDict = new ResourceInclude(themeUri) { Source = themeUri };
    if (dicts.Count > 0)
        dicts[0] = newDict;
    else
        dicts.Add(newDict);
}

        public static void SwitchLanguage(bool isUkrainian)
        {
            var app = (App)Current!;
            var dicts = app.Resources.MergedDictionaries;
            var langUri = isUkrainian
                ? new Uri("avares://RedDiceFlow/Resources/Lang_ua.axaml")
                : new Uri("avares://RedDiceFlow/Resources/Lang_en.axaml");

            var newDict = new ResourceInclude(langUri) { Source = langUri };
            if (dicts.Count > 1)
                dicts[1] = newDict;
            else
                dicts.Add(newDict);
        }
    }
}
