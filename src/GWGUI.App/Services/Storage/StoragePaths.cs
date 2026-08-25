using GWGUI.App.Constants.Storage;
using System.IO;

namespace GWGUI.App.Services.Storage;

public static class StoragePaths
{
    private static string? _configuredEmulationStorageDirectory;
    private static string? _configuredEmulationStateDirectory;
    private static string? _configuredEmulationCaptureDirectory;
    public static bool IsPortable => File.Exists(Path.Combine(AppContext.BaseDirectory, "portable.flag"));
    public static string DataDirectory => ResolveDataDirectory(AppContext.BaseDirectory, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
    public static string LogsDirectory => Path.Combine(DataDirectory, "Logs");
    public static string EmulationDirectory => Path.Combine(DataDirectory, "Emulation");
    public static string FirmwareCatalogDirectory => Path.Combine(EmulationDirectory, "Catalogs", "Firmware");
    public static string FirmwareCatalogPath(string platform) =>
        Path.Combine(FirmwareCatalogDirectory, $"{platform.ToLowerInvariant()}.json");
    public static string EmulationStorageDirectory => string.IsNullOrWhiteSpace(_configuredEmulationStorageDirectory)
        ? Path.Combine(DataDirectory, "Emulation", "Storage")
        : Path.GetFullPath(_configuredEmulationStorageDirectory);
    public static string EmulationStateDirectory => ResolveConfiguredDirectory(
        _configuredEmulationStateDirectory,
        Path.Combine(EmulationStorageDirectory, StoragePathConstants.StatesDirectory));
    public static string EmulationCaptureDirectory => ResolveConfiguredDirectory(
        _configuredEmulationCaptureDirectory,
        Path.Combine(EmulationStorageDirectory, StoragePathConstants.CapturesDirectory));

    public static void ConfigureEmulationStorageDirectory(string? directory) =>
        _configuredEmulationStorageDirectory = string.IsNullOrWhiteSpace(directory) ? null : directory.Trim();

    public static void ConfigureEmulationStateDirectory(string? directory) =>
        _configuredEmulationStateDirectory = NormalizeConfiguredDirectory(directory);

    public static void ConfigureEmulationCaptureDirectory(string? directory) =>
        _configuredEmulationCaptureDirectory = NormalizeConfiguredDirectory(directory);

    private static string ResolveConfiguredDirectory(string? configured, string fallback) =>
        string.IsNullOrWhiteSpace(configured) ? fallback : Path.GetFullPath(configured);

    private static string? NormalizeConfiguredDirectory(string? directory) =>
        string.IsNullOrWhiteSpace(directory) ? null : directory.Trim();
    public static string HostToolsDirectory => Path.Combine(DataDirectory, "Greaseweazle");

    public static string? NormalizeHostToolsPath(string? path) => path;

    public static string ResolveDataDirectory(string applicationDirectory, string roamingDirectory) =>
        File.Exists(Path.Combine(applicationDirectory, "portable.flag"))
            ? Path.Combine(applicationDirectory, "Data")
            : Path.Combine(roamingDirectory, "GW GUI");

}
