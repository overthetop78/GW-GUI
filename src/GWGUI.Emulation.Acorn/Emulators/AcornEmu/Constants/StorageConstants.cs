namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Identifiants persistants des lecteurs de l'émulateur interne et choix des dialogues partagés.</summary>
internal static class StorageConstants
{
    internal const string FloppyPrefix = "acornemu.floppy.", DfsFirmware = "acornemu.dfs";
    internal const string ControllerOption = FloppyPrefix + "controller", Intel8271 = "i8271", AcornWd1770 = "acorn-wd1770";
    internal const string DefaultModel = "ds80", Single40 = "ss40", Double40 = "ds40", Single80 = "ss80", Double80 = "ds80";
    internal const int DriveCount = 2, SecondDriveLed = 4;
    internal const string HardDiskPrefix = "acornemu.hdd.", ScsiInterface = "scsi", HardDiskFormat = "hdd.acorn-winchester";
    internal const int HardDiskCount = AcornScsiConstants.UnitCount, HardDiskLedStart = 8;
    internal const long MaximumHardDiskBytes = 512L * 1024 * 1024;
    internal static IReadOnlyList<string> HardDiskExtensions { get; } = Array.AsReadOnly<string>([".dat"]);
    internal static IReadOnlyList<string> Extensions { get; } = Array.AsReadOnly<string>([".ssd", ".dsd"]);
    internal static IReadOnlyList<string> DoubleDensityExtensions { get; } = Array.AsReadOnly<string>([".ssd", ".dsd", ".ads", ".adm", ".adl", ".adf"]);
    internal static IReadOnlyList<string> Speeds { get; } = Array.AsReadOnly<string>(["100", "200", "400", "800", "0"]);
    internal static IReadOnlyList<FloppyDriveModelChoice> Models { get; } = Array.AsReadOnly<FloppyDriveModelChoice>([
        new(Double80, "", "5.25″ DS · 80", 409600) { BlankFormats = new Dictionary<string, string> { [".dsd"] = "acorn.dfs.ds80" } },
        new(Single80, "", "5.25″ SS · 80", 204800) { BlankFormats = new Dictionary<string, string> { [".ssd"] = "acorn.dfs.ss80" } },
        new(Double40, "", "5.25″ DS · 40", 204800) { BlankFormats = new Dictionary<string, string> { [".dsd"] = "acorn.dfs.ds" } },
        new(Single40, "", "5.25″ SS · 40", 102400) { BlankFormats = new Dictionary<string, string> { [".ssd"] = "acorn.dfs.ss" } }]);
    internal static string Key(int drive, string name) => FloppyPrefix + drive + "." + name;
    internal static string HardDiskKey(int unit, string name) => HardDiskPrefix + unit + "." + name;
}
