using GWGUI.MediaEngine.Constants;

using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;

using GWGUI.MediaEngine.Images.Formats.Floppy.St;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Conversion.Fat12;

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
