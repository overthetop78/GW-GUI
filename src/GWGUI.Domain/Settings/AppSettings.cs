namespace GWGUI.Domain.Settings;

public enum AppTheme { System, Light, Dark }

public sealed class AppSettings
{
    public int SchemaVersion { get; set; } = SettingsMigrator.CurrentVersion;
    public string Language { get; set; } = "fr";
    public AppTheme Theme { get; set; } = AppTheme.System;
    public string DefaultImagesFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public string? GwExecutablePath { get; set; }
    public string? PreviousGwExecutablePath { get; set; }
    public string? InstalledHostToolsVersion { get; set; }
    public string? AvailableHostToolsVersion { get; set; }
    public DateTimeOffset? LastHostToolsCheckUtc { get; set; }
    public bool ConsoleExpanded { get; set; } = true;
    public double ConsoleHeight { get; set; } = 190;
    public WindowPlacementSettings Window { get; set; } = new();
    public List<ControllerSettings> Controllers { get; set; } = [];
    public List<DriveSettings> Drives { get; set; } = [];
    public ReadUiSettings Read { get; set; } = new();
    public AdvancedUiSettings Write { get; set; } = new();
    public List<ProfileSettings> Profiles { get; set; } = [];
    public ConversionUiSettings Conversion { get; set; } = new();
}

public sealed class ConversionUiSettings
{
    public bool AddTags { get; set; }
    public HashSet<string> SelectedFormats { get; set; } = [];
    public Dictionary<string, HashSet<string>> ExplicitExtensions { get; set; } = new();
    public Dictionary<string, string> OptionValues { get; set; } = new();
    public HashSet<string> EnabledOptions { get; set; } = [];
}

public sealed class AdvancedUiSettings
{
    public Dictionary<string, string> OptionValues { get; set; } = new();
    public HashSet<string> EnabledOptions { get; set; } = [];
}

public sealed class ProfileSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Operation { get; set; } = "Read";
    public string Name { get; set; } = "";
    public Dictionary<string, string> Values { get; set; } = new();
    public HashSet<string> EnabledOptions { get; set; } = [];
}

public sealed class ReadUiSettings
{
    public bool UseKnownFormat { get; set; }
    public string? FormatId { get; set; }
    public bool AutoNumber { get; set; }
    public string SequenceKind { get; set; } = "Numeric";
    public int SequenceWidth { get; set; } = 1;
    public long NextSequence { get; set; } = 1;
    public Dictionary<string, string> OptionValues { get; set; } = new();
    public HashSet<string> EnabledOptions { get; set; } = [];
}

public sealed class WindowPlacementSettings
{
    public double Width { get; set; } = 1360;
    public double Height { get; set; } = 820;
    public double? Left { get; set; }
    public double? Top { get; set; }
    public bool Maximized { get; set; }
}

public sealed class ControllerSettings
{
    public string UsbId { get; set; } = "";
    public string LastPort { get; set; } = "";
    public string Model { get; set; } = "";
    public bool IsAvailable { get; set; }
}

public sealed class DriveSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ControllerUsbId { get; set; } = "";
    public string Selection { get; set; } = "";
    public string Size { get; set; } = "3.5";
    public string Density { get; set; } = "Unknown";
    public int? NominalRpm { get; set; }
}

public interface ISettingsStore
{
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
