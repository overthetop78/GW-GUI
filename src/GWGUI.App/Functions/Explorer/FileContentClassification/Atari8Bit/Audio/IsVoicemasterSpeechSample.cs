using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsVoicemasterSpeechSample(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".spe", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: >= 260 } data
            || data[0] != 0x00 || data[1] != 0x56)
            return false;

        var declaredPayloadLength = data[2] | data[3] << 8;
        return declaredPayloadLength == data.Count - 4
            && data.Skip(4).Distinct().Take(32).Count() == 32;
    }
}
