namespace GWGUI.Emulation.Snk.Emulators.MednafenNgp.Constants;

internal static class RuntimeOptionConstants
{
    internal const string Language = "ngp_language";
    internal const string English = "english";
    internal const string Japanese = "japanese";
    internal const string SaveExtension = ".flash";
    internal const bool NeedsFullPath = true;
    internal static readonly IReadOnlyList<string> CartridgeExtensions = Array.AsReadOnly(new[] { ".ngp", ".ngc", ".ngpc", ".npc" });
}
