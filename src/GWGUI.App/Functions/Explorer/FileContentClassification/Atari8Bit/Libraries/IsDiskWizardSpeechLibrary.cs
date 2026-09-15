using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsDiskWizardSpeechLibrary(IReadOnlyList<byte>? data) =>
        data is { Count: 145 }
        && data[0] == 0xf4 && data[1] == 0x14
        && data[2] == 0xa6 && data[3] == 0xd2;
}
