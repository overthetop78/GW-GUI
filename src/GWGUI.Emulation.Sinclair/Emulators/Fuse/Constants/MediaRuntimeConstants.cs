namespace GWGUI.Emulation.Sinclair.Emulators.Fuse.Constants;

internal static class MediaRuntimeConstants
{
    internal static readonly IReadOnlyList<string> TapeExtensions = Array.AsReadOnly(new[] { ".tap", ".tzx" });
    internal static readonly IReadOnlyList<string> Plus3Extensions = Array.AsReadOnly(new[] { ".dsk", ".ipf" });
    internal static readonly IReadOnlyList<string> Plus3EExtensions = Array.AsReadOnly(new[] { ".dsk" });
    internal static readonly IReadOnlyList<string> TrDosExtensions = Array.AsReadOnly(new[] { ".scl", ".trd" });
    internal static readonly IReadOnlyList<string> CartridgeExtensions = Array.AsReadOnly(new[] { ".dck" });
    internal const bool NeedsFullPath = false;
    internal static readonly IReadOnlyList<string> InitialContentExtensions = Array.AsReadOnly(new[] { ".z80", ".sna", ".szx", ".rzx", ".zip", ".m3u" });
    internal const string PlaylistExtension = ".m3u";
    internal const string InitialContentDirectory = "initial-content";
    internal const string InitialContentFileName = "content";
    internal const int MaximumPlaylistImages = 16;
    internal const int MaximumNativePathLength = 1024;
    internal const char PlaylistComment = '#';
}
