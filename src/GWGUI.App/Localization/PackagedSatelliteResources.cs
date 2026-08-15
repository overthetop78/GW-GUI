using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace GWGUI.App.Localization;

internal static class PackagedSatelliteResources
{
    private const string SatelliteAssemblyName = "GW GUI.resources";

    public static void Load(CultureInfo culture)
    {
        if (AssemblyLoadContext.Default.Assemblies.Any(assembly =>
                string.Equals(assembly.GetName().Name, SatelliteAssemblyName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(assembly.GetName().CultureName, culture.Name, StringComparison.OrdinalIgnoreCase))) return;
        var path = GetPath(AppContext.BaseDirectory, culture);
        if (File.Exists(path)) AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
    }

    internal static string GetPath(string applicationDirectory, CultureInfo culture) =>
        Path.Combine(applicationDirectory, "Languages", culture.Name, SatelliteAssemblyName + ".dll");
}
