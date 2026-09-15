using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariMidiSequencerFile(IReadOnlyList<byte>? data) =>
        data is { Count: >= 4 }
        && data[0] == 0xb3 && data[1] == 0xa5 && data[2] == 0xb1;
}
