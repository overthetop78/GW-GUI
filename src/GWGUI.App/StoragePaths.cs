using System.IO;

namespace GWGUI.App;

public static class StoragePaths
{
    private static string? _configuredEmulationStorageDirectory;
    public static bool IsPortable => File.Exists(Path.Combine(AppContext.BaseDirectory, "portable.flag"));
    public static string DataDirectory => ResolveDataDirectory(AppContext.BaseDirectory, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
    public static string LogsDirectory => Path.Combine(DataDirectory, "Logs");
    public static string EmulationDirectory => Path.Combine(DataDirectory, "Emulation");
    public static string EmulationStorageDirectory => string.IsNullOrWhiteSpace(_configuredEmulationStorageDirectory)
        ? Path.Combine(DataDirectory, "Emulation", "Storage")
        : Path.GetFullPath(_configuredEmulationStorageDirectory);
    public static string AmigaHardDisksDirectory => Path.Combine(EmulationStorageDirectory, "HDD", "Amiga");
    public static string AmigaFloppyImagesDirectory => Path.Combine(EmulationStorageDirectory, "Floppies", "Amiga");
    public static string AmigaCompactDiscsDirectory => Path.Combine(EmulationStorageDirectory, "CD", "Amiga");
    public static string AmigaDirectory => Path.Combine(EmulationDirectory, "Machines", "Amiga");
    public static string AmigaFirmwareDirectory => Path.Combine(AmigaDirectory, "Firmware");
    public static string AmigaConfigurationsDirectory => Path.Combine(AmigaDirectory, "Configurations");
    public static string AmigaSessionsDirectory => Path.Combine(AmigaDirectory, "Sessions");
    public static string AmigaCoreDirectory => Path.Combine(AmigaDirectory, "Core");

    public static void ConfigureEmulationStorageDirectory(string? directory) =>
        _configuredEmulationStorageDirectory = string.IsNullOrWhiteSpace(directory) ? null : directory.Trim();
    public static string HostToolsDirectory
    {
        get
        {
            var preferred = IsPortable
                ? Path.Combine(DataDirectory, "Greaseweazle")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GW GUI", "Greaseweazle");
            var legacy = IsPortable
                ? Path.Combine(DataDirectory, "host-tools")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GW GUI", "host-tools");
            MigrateHostToolsDirectory(legacy, preferred);
            return preferred;
        }
    }

    public static string? NormalizeHostToolsPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return path;
        var legacy = IsPortable
            ? Path.Combine(DataDirectory, "host-tools")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GW GUI", "host-tools");
        var relative = Path.GetRelativePath(legacy, path);
        if (relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar)) return path;
        var migrated = Path.Combine(HostToolsDirectory, relative);
        return File.Exists(migrated) ? migrated : path;
    }

    public static string ResolveDataDirectory(string applicationDirectory, string roamingDirectory) =>
        File.Exists(Path.Combine(applicationDirectory, "portable.flag"))
            ? Path.Combine(applicationDirectory, "Data")
            : Path.Combine(roamingDirectory, "GW GUI");

    public static void MigrateHostToolsDirectory(string legacy, string preferred)
    {
        if (!Directory.Exists(legacy)) return;
        if (!Directory.Exists(preferred)) { Directory.Move(legacy, preferred); return; }
        MoveMissingEntries(legacy, preferred);
        if (!Directory.EnumerateFileSystemEntries(legacy).Any()) Directory.Delete(legacy);
    }

    private static void MoveMissingEntries(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var entry in Directory.EnumerateFileSystemEntries(source).ToArray())
        {
            var target = Path.Combine(destination, Path.GetFileName(entry));
            if (Directory.Exists(entry))
            {
                if (Directory.Exists(target)) { MoveMissingEntries(entry, target); if (!Directory.EnumerateFileSystemEntries(entry).Any()) Directory.Delete(entry); }
                else Directory.Move(entry, target);
            }
            else if (!File.Exists(target)) File.Move(entry, target);
        }
    }
}
