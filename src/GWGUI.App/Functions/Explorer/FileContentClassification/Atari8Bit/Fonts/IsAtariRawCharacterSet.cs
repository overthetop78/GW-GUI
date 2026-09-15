using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariRawCharacterSet(IReadOnlyList<byte>? data) =>
        data is { Count: 1024 or 1025 }
        && data.Take(8).All(value => value == 0)
        && data.Skip(8).Take(1016).Any(value => value != 0)
        && (data.Count == 1024 || data[1024] is 0 or 1);
}
