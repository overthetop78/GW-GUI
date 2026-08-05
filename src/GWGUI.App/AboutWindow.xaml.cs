using System.Reflection;
using System.Windows;
using GWGUI.App.Localization;

namespace GWGUI.App;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
        VersionText.Text = LocExtension.Get("About.Version", version.Split('+')[0]);
    }
}
