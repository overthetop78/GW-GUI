using GWGUI.App.Localization;
using GWGUI.Domain.Settings;

namespace GWGUI.App;

public sealed class HardwareRow(string? driveId, string port, string usbId, string readerLabel, string size, string density, string rpm, bool available, bool configured, string configurationState)
{
    public string? DriveId { get; } = driveId;
    public string Port { get; } = port;
    public string UsbId { get; } = usbId;
    public string ReaderLabel { get; } = readerLabel;
    public string Size { get; set; } = size;
    public string Density { get; set; } = density;
    public string Rpm { get; set; } = rpm;
    public bool Available { get; } = available;
    public string AvailabilityState => LocExtension.Get(Available ? "Hardware.AvailableState" : "Hardware.UnavailableState");
    public bool Configured { get; } = configured;
    public string ConfigurationState { get; } = configurationState;
}

public static class HardwareChoices
{
    public const string UnknownSpeed = "—";
    public static IReadOnlyList<string> Sizes { get; } = ["3", "3.5", "5.25", "8"];
    public static IReadOnlyList<string> Densities { get; } = ["Unknown", "DD", "HD", "ED"];
    public static IReadOnlyList<string> Speeds { get; } = [UnknownSpeed, "300 RPM", "360 RPM"];
}

public sealed record ProfileOptionRow(string Id, string Operation, string Name, bool IsSystem)
{
    public string OperationLabel => Operation switch
    {
        "Read" => LocExtension.Get("Tab.Read"),
        "Write" => LocExtension.Get("Tab.Write"),
        "Convert" => LocExtension.Get("Tab.Convert"),
        _ => Operation
    };
}

public sealed record TagPresetOption(string Label, string Pattern);
public sealed record TagVariableOption(string Token, string Description);
public sealed record LogOptionRow(string Action, string Label, ActionLogSettings Settings);
public sealed record RecentTagPatternOption(int Number, string? Pattern)
{
    public string Display => string.IsNullOrWhiteSpace(Pattern) ? "—" : Pattern;
}
