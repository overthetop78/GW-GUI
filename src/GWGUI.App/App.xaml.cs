using System.Globalization;
using System.IO;
using System.Windows;
using GWGUI.Infrastructure.Settings;

namespace GWGUI.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GW GUI");
        var settings = new JsonSettingsStore(Path.Combine(directory, "settings.json")).LoadAsync().GetAwaiter().GetResult();
        var culture = CultureInfo.GetCultureInfo(settings.Language == "en" ? "en" : "fr");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        base.OnStartup(e);
        MainWindow = new MainWindow();
        MainWindow.Show();
    }
}
