using System.Reflection;
using System.IO;
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
        ComponentVersions.ItemsSource = GetComponentVersions();
    }

    private static IReadOnlyList<ComponentVersion> GetComponentVersions()
    {
        var components = new List<ComponentVersion> { new(".NET Desktop Runtime", Environment.Version.ToString()) };
        foreach (var (label, assemblyName) in new[]
        {
            ("WPF", "PresentationFramework"), ("SkiaSharp", "SkiaSharp"), ("NAudio", "NAudio.Core"),
            ("Newtonsoft.Json", "Newtonsoft.Json"), ("OpenTK", "OpenTK.Core"), ("Veldrid", "Veldrid"),
            ("Veldrid.SPIRV", "Veldrid.SPIRV"), ("Vortice.Direct3D11", "Vortice.Direct3D11"),
            ("GLWpfControl", "GLWpfControl")
        })
        {
            try
            {
                var assembly = Assembly.Load(new AssemblyName(assemblyName));
                var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                components.Add(new(label, informational?.Split('+')[0] ?? assembly.GetName().Version?.ToString() ?? "?"));
            }
            catch (FileNotFoundException) { }
        }
        return components;
    }

    private sealed record ComponentVersion(string Name, string Version);
}
