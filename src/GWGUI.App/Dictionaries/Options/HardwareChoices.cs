namespace GWGUI.App.Dictionaries.Options;

public static class HardwareChoices
{
    public const string UnknownSpeed = "—";
    public static IReadOnlyList<string> Sizes { get; } = ["3", "3.5", "5.25", "8"];
    public static IReadOnlyList<string> Densities { get; } = ["Unknown", "DD", "HD", "ED"];
    public static IReadOnlyList<string> Speeds { get; } = [UnknownSpeed, "300 RPM", "360 RPM"];
}
