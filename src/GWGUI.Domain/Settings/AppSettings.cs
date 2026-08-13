namespace GWGUI.Domain.Settings;

public sealed class AppSettings
{
    public int SchemaVersion { get; set; } = SettingsMigrator.CurrentVersion;
    public string Language { get; set; } = "";
    public AppTheme Theme { get; set; } = AppTheme.System;
    public string DefaultImagesFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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
