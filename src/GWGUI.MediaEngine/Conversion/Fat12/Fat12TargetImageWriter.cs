using GWGUI.MediaEngine.Containers.Atari.St;
using GWGUI.MediaEngine.Containers.Ibm.Raw;
using GWGUI.MediaEngine.Containers.Msx.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Fat12;

public sealed class Fat12TargetImageWriter(
    AtariStWriter atariWriter,
    IbmRawImageWriter ibmWriter,
    MsxRawImageWriter msxWriter)
{
    public Task WriteAsync(SectorImage image, string path, string targetFormatId, CancellationToken cancellationToken = default)
    {
        var target = image.FormatId.Equals(targetFormatId, StringComparison.OrdinalIgnoreCase) ? image : image.WithFormatId(targetFormatId);
        if (targetFormatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase)) return atariWriter.WriteAsync(target, path, cancellationToken);
        if (targetFormatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase)) return ibmWriter.WriteAsync(target, path, targetFormatId, cancellationToken);
        if (targetFormatId.StartsWith(DiskImageFormatIds.MsxPrefix, StringComparison.OrdinalIgnoreCase)) return msxWriter.WriteAsync(target, path, targetFormatId, cancellationToken);
        throw Fat12ReinterpretationExceptions.UnsupportedTarget(targetFormatId);
    }
}
