namespace GWGUI.Domain.Settings;

public enum AppTheme { System, Light, Dark }

public sealed class AppSettings
{
    public int SchemaVersion { get; set; } = 1;
    public string Language { get; set; } = "fr";
    public AppTheme Theme { get; set; } = AppTheme.System;
    public string DefaultImagesFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public string? GwExecutablePath { get; set; }
    public bool ConsoleExpanded { get; set; } = true;
    public double ConsoleHeight { get; set; } = 190;
    public WindowPlacementSettings Window { get; set; } = new();
    public List<ControllerSettings> Controllers { get; set; } = [];
    public List<DriveSettings> Drives { get; set; } = [];
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
