namespace GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants;

internal static class ContentConstants
{
    internal const string CommandFileName = "startup.cmd";
    internal const string ExecutableName = "np21";
    internal const string EmptyDriveArgument = "-";
    internal const char ArgumentSeparator = ' ';
    internal const char Quote = '"';
    internal const char Assignment = '=';
    internal const int CommandBufferLimit = 512;
    internal static IReadOnlyList<string> HardDiskKeys { get; } = ["HDD1FILE", "HDD2FILE"];
    internal static IReadOnlyDictionary<string, string> Profiles { get; } = new Dictionary<string, string>
    {
        ["np2.cfg"] = "[NekoProjectII]",
    };
}
