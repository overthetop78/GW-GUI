namespace GWGUI.Emulation.Sinclair.Emulators.EightyOne.Constants;

internal static class MediaRuntimeConstants
{
    internal static readonly IReadOnlyList<string> TapeExtensions = Array.AsReadOnly(new[] { ".p", ".tzx", ".t81" });
    internal const bool NeedsFullPath = false;
}
