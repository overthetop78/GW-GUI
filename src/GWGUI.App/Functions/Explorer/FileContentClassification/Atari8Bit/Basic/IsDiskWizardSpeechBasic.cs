using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsDiskWizardSpeechBasic(IReadOnlyList<byte>? data) =>
        data is { Count: >= 100 }
        && (ContainsAscii(data, "RETURN TO MAIN MENU", data.Count)
            || ContainsAscii(data, "USERS CLUB", data.Count));
}
