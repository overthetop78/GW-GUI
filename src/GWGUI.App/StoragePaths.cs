using System.IO;

namespace GWGUI.App;

public static class StoragePaths
{
    public static bool IsPortable => File.Exists(Path.Combine(AppContext.BaseDirectory, "portable.flag"));
    public static string DataDirectory => ResolveDataDirectory(AppContext.BaseDirectory, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
    public static string HostToolsDirectory => IsPortable
        ? Path.Combine(DataDirectory, "host-tools")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GW GUI", "host-tools");

    public static string ResolveDataDirectory(string applicationDirectory, string roamingDirectory) =>
        File.Exists(Path.Combine(applicationDirectory, "portable.flag"))
            ? Path.Combine(applicationDirectory, "Data")
            : Path.Combine(roamingDirectory, "GW GUI");
}
