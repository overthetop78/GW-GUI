using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool ContainsSequence(IReadOnlyList<byte> data, IReadOnlyList<byte> sequence)
    {
        if (sequence.Count == 0 || sequence.Count > data.Count) return false;
        for (var offset = 0; offset <= data.Count - sequence.Count; offset++)
        {
            var matches = true;
            for (var index = 0; index < sequence.Count; index++)
            {
                if (data[offset + index] == sequence[index]) continue;
                matches = false;
                break;
            }
            if (matches) return true;
        }
        return false;
    }
}
