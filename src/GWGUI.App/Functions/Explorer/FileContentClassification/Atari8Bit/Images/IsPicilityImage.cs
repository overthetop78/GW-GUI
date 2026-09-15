using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsPicilityImage(IReadOnlyList<byte>? data)
    {
        if (data is { Count: 15872 })
            return data.Take(1024).All(value => value == 0xff)
                && data.TakeLast(1024).All(value => value == 0x00)
                && data.Skip(1024).Take(data.Count - 2048).Any(value => value is not 0x00 and not 0xff);

        return data is { Count: 767 }
            && data[0] == 0x04 && data[1] == 0x1e
            && data.Count(value => value == 0x55) >= data.Count / 3;
    }
}
