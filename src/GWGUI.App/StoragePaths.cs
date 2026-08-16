using System.IO;
using GWGUI.Emulation.Atari;

namespace GWGUI.App;

public static class StoragePaths
{
    private static string? _configuredEmulationStorageDirectory;
    private static string? _configuredEmulationStateDirectory;
    private static string? _configuredEmulationCaptureDirectory;
    private static string? _configuredAmigaHardDisksDirectory;
    public static bool IsPortable => File.Exists(Path.Combine(AppContext.BaseDirectory, "portable.flag"));
    public static string DataDirectory => ResolveDataDirectory(AppContext.BaseDirectory, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
    public static string LogsDirectory => Path.Combine(DataDirectory, "Logs");
    public static string EmulationDirectory => Path.Combine(DataDirectory, "Emulation");
    public static string EmulationStorageDirectory => string.IsNullOrWhiteSpace(_configuredEmulationStorageDirectory)
        ? Path.Combine(DataDirectory, "Emulation", "Storage")
        : Path.GetFullPath(_configuredEmulationStorageDirectory);
    public static string AmigaHardDisksDirectory => ResolveConfiguredDirectory(
        _configuredAmigaHardDisksDirectory, Path.Combine(EmulationStorageDirectory, "HDD", "Amiga"));
    public static string AmigaFloppyImagesDirectory => Path.Combine(EmulationStorageDirectory, "Floppies", "Amiga");
    public static string AmigaCompactDiscsDirectory => Path.Combine(EmulationStorageDirectory, "CD", "Amiga");
    public static string AmigaDirectory => Path.Combine(EmulationDirectory, "Machines", "Amiga");
    public static string AmigaFirmwareDirectory => Path.Combine(AmigaDirectory, "Firmware");
    public static string AmigaConfigurationsDirectory => Path.Combine(AmigaDirectory, "Configurations");
    public static string AmigaSessionsDirectory => Path.Combine(AmigaDirectory, "Sessions");
    public static string AmigaCoreDirectory => Path.Combine(AmigaDirectory, "Core");
    public static string AtariDirectory => Path.Combine(EmulationDirectory, StoragePathConstants.MachinesDirectory,
        StoragePathConstants.AtariDirectory);
    public static string AtariCoreDirectory => Path.Combine(AtariDirectory, StoragePathConstants.CoreDirectory);
    public static string AtariConfigurationsDirectory =>
        Path.Combine(AtariDirectory, StoragePathConstants.ConfigurationsDirectory);
    public static string AtariSessionsDirectory =>
        Path.Combine(AtariDirectory, StoragePathConstants.SessionsDirectory);
    public static string AtariSavesDirectory =>
        Path.Combine(AtariDirectory, StoragePathConstants.SavesDirectory);
    public static string AtariSharedStorageDirectory =>
        Path.Combine(EmulationStorageDirectory, StoragePathConstants.AtariDirectory);
    public static string AtariFloppyImagesDirectory =>
        Path.Combine(EmulationStorageDirectory, StoragePathConstants.FloppiesDirectory,
            StoragePathConstants.AtariDirectory);
    public static string AtariCassetteImagesDirectory =>
        Path.Combine(EmulationStorageDirectory, StoragePathConstants.CassettesDirectory,
            StoragePathConstants.AtariDirectory);
    public static string AtariCartridgeImagesDirectory =>
        Path.Combine(EmulationStorageDirectory, StoragePathConstants.CartridgesDirectory,
            StoragePathConstants.AtariDirectory);
    public static string AtariCompactDiscsDirectory =>
        Path.Combine(EmulationStorageDirectory, StoragePathConstants.CompactDiscsDirectory,
            StoragePathConstants.AtariDirectory);
    public static string AtariHardDisksDirectory =>
        Path.Combine(EmulationStorageDirectory, StoragePathConstants.HardDisksDirectory,
            StoragePathConstants.AtariDirectory);
    public static string EmulationStateDirectory => ResolveConfiguredDirectory(
        _configuredEmulationStateDirectory,
        Path.Combine(EmulationStorageDirectory, StoragePathConstants.StatesDirectory));
    public static string EmulationCaptureDirectory => ResolveConfiguredDirectory(
        _configuredEmulationCaptureDirectory,
        Path.Combine(EmulationStorageDirectory, StoragePathConstants.CapturesDirectory));
    public static string AtariStatesDirectory =>
        Path.Combine(EmulationStateDirectory, StoragePathConstants.AtariDirectory);
    public static string AtariCapturesDirectory =>
        Path.Combine(EmulationCaptureDirectory, StoragePathConstants.AtariDirectory);
    public static string AtariFirmwareDirectory =>
        Path.Combine(AtariDirectory, AtariFirmwareConstants.FirmwareDirectoryName);
    public static string AtariStFirmwareDirectory =>
        Path.Combine(AtariFirmwareDirectory, AtariFirmwareConstants.StFamilyDirectoryName);
    public static string AtariEightBitFirmwareDirectory =>
        Path.Combine(AtariFirmwareDirectory, AtariFirmwareConstants.EightBitFamilyDirectoryName);
    public static string Atari5200FirmwareDirectory =>
        Path.Combine(AtariFirmwareDirectory, AtariFirmwareConstants.Atari5200FamilyDirectoryName);
    public static string Atari2600FirmwareDirectory =>
        Path.Combine(AtariFirmwareDirectory, AtariFirmwareConstants.Atari2600FamilyDirectoryName);
    public static string Atari7800FirmwareDirectory =>
        Path.Combine(AtariFirmwareDirectory, AtariFirmwareConstants.Atari7800FamilyDirectoryName);
    public static string AtariLynxFirmwareDirectory =>
        Path.Combine(AtariFirmwareDirectory, AtariFirmwareConstants.LynxFamilyDirectoryName);
    public static string AtariJaguarFirmwareDirectory =>
        Path.Combine(AtariFirmwareDirectory, AtariFirmwareConstants.JaguarFamilyDirectoryName);

    public static void ConfigureEmulationStorageDirectory(string? directory) =>
        _configuredEmulationStorageDirectory = string.IsNullOrWhiteSpace(directory) ? null : directory.Trim();

    public static void ConfigureEmulationStateDirectory(string? directory) =>
        _configuredEmulationStateDirectory = NormalizeConfiguredDirectory(directory);

    public static void ConfigureEmulationCaptureDirectory(string? directory) =>
        _configuredEmulationCaptureDirectory = NormalizeConfiguredDirectory(directory);

    public static void ConfigureAmigaHardDisksDirectory(string? directory) =>
        _configuredAmigaHardDisksDirectory = NormalizeConfiguredDirectory(directory);

    private static string ResolveConfiguredDirectory(string? configured, string fallback) =>
        string.IsNullOrWhiteSpace(configured) ? fallback : Path.GetFullPath(configured);

    private static string? NormalizeConfiguredDirectory(string? directory) =>
        string.IsNullOrWhiteSpace(directory) ? null : directory.Trim();
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
