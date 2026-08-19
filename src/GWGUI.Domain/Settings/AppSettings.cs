namespace GWGUI.Domain.Settings;

public sealed class AppSettings
{
    public int SchemaVersion { get; set; } = SettingsMigrator.CurrentVersion;
    public string Language { get; set; } = "";
    public AppTheme Theme { get; set; } = AppTheme.System;
    public string DefaultImagesFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public string EmulationStorageFolder { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GW GUI", "Emulation");
    public string EmulationCaptureFolder { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GW GUI", "Emulation", "Captures");
    public string EmulationStateFolder { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GW GUI", "Emulation", "States");
    public string AmigaHardDisksFolder { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GW GUI", "Emulation", "HDD", "Amiga");
    public Dictionary<string, string> EmulationShortcuts { get; set; } = EmulationShortcutDefaultFunctions.Create();
    public List<EmulationMediaFolderSettings> EmulationMediaFolders { get; set; } = [];
    public bool CreateEmulationFoldersAutomatically { get; set; } = true;
    public string? LastDiskImageFolder { get; set; }
    public string? GwExecutablePath { get; set; }
    public string? PreviousGwExecutablePath { get; set; }
    public string? InstalledHostToolsVersion { get; set; }
    public string? AvailableHostToolsVersion { get; set; }
    public DateTimeOffset? LastHostToolsCheckUtc { get; set; }
    public bool ConsoleExpanded { get; set; } = true;
    public double ConsoleHeight { get; set; } = 190;
    public OperationLogSettings Logging { get; set; } = new();
    public WindowPlacementSettings Window { get; set; } = new();
    public List<ControllerSettings> Controllers { get; set; } = [];
    public List<ControllerSettings> UnconfiguredControllers { get; set; } = [];
    public List<DriveSettings> Drives { get; set; } = [];
    public EngineSettings Engines { get; set; } = new();
    public ReadUiSettings Read { get; set; } = new();
    public AdvancedUiSettings Write { get; set; } = new();
    public List<ProfileSettings> Profiles { get; set; } = [];
    public ConversionUiSettings Conversion { get; set; } = new();
}

public enum EmulationMediaFolderFamily { Amiga, Atari }

public enum EmulationMediaFolderType { Floppy, CompactDisc, HardDisk, Cartridge, Cassette, Directory }

public sealed class EmulationMediaFolderSettings
{
    public EmulationMediaFolderFamily Family { get; set; }
    public string Model { get; set; } = "";
    public EmulationMediaFolderType Type { get; set; }
    public string Folder { get; set; } = "";
}
