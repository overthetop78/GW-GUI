using System.Text.Json;

namespace GWGUI.Emulation.Atari;

internal static class AtariStateStoreConstants
{
    internal const string AtariDirectoryName = "Atari";
    internal const string QuickStateName = "quick";
    internal const string StateFileExtension = ".gwats";
    internal const string MetadataFileExtension = ".json";
    internal const string MetadataSearchPattern = "*.json";
    internal const string CaptureFileExtension = ".png";
    internal const string TemporaryFileExtension = ".tmp";
    internal const string MachineIdentifierFormat = "N";
    internal const string CurrentDirectoryName = ".";
    internal const string ParentDirectoryName = "..";
    internal const int MaximumStateNameLength = 80;
    internal const int CopyBufferSize = 81920;
    internal const int MetadataReadRetryCount = 64;
    internal const int MetadataReadRetryDelayMilliseconds = 1;
    internal const int FirstRetryIndex = 0;
    internal const int NextRetryCount = 1;
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
}
