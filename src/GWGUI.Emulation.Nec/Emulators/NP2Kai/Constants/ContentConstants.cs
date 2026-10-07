namespace GWGUI.Emulation.Nec.Emulators.NP2Kai.Constants;

internal static class ContentConstants
{
    internal const string CommandFileName = "startup.cmd";
    internal const string ExecutableName = "np2kai";
    internal const string EmptyDriveArgument = "-";
    internal const char ArgumentSeparator = ' ';
    internal const char Quote = '"';
    internal const char Assignment = '=';
    internal const int CommandBufferLimit = 1024;
    internal static IReadOnlyList<string> HardDiskKeys { get; } = ["HDD1FILE", "HDD2FILE"];
    internal static IReadOnlyDictionary<string, string> Profiles { get; } = new Dictionary<string, string>
    {
        ["np21kai.cfg"] = "[NekoProject21kai]",
        ["np2kai.cfg"] = "[NekoProjectIIkai]",
    };
}
