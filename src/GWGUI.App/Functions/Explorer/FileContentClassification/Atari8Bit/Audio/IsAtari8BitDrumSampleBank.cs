using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitDrumSampleBank(IReadOnlyList<byte>? data) =>
        data is { Count: > 1024 }
        && ContainsAscii(data, "BASS1", 256)
        && ContainsAscii(data, "SNARE1", 256)
        && ContainsAscii(data, "HANDCLAP", 256)
        && ContainsAscii(data, "COWBELL", 256);
}
