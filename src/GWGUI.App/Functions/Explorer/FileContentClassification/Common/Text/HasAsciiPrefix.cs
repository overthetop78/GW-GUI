using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool HasAsciiPrefix(IReadOnlyList<byte>? data, string signature) =>
        data is not null && data.Count >= signature.Length &&
        data.Take(signature.Length).SequenceEqual(System.Text.Encoding.ASCII.GetBytes(signature));
}
