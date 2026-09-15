using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsScreenAidedManagementPrinterProfile(IReadOnlyList<byte>? data) =>
        data is { Count: 18 }
        && data[0] == (byte)'S'
        && data.Skip(6).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'A' })
        && data.Skip(10).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'L' })
        && data.Skip(15).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'U' });
}
