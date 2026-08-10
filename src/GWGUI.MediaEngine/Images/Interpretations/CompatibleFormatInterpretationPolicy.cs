using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

internal sealed class CompatibleFormatInterpretationPolicy : IAdditionalImageInterpretationPolicy
{
    public IEnumerable<SectorImage> Create(SectorImage image)
    {
        var formatIds = image.BlockSize switch
        {
            512 => (IReadOnlyList<string>)["ucsd.ibm.mfm", "commodore900.coherent", "epson.qx10.396",
                "epson.qx10.399", "epson.qx10.logo"],
            256 => ["acorn.dfs.ss", "acorn.dfs.ss80", "acorn.dfs.ds", "acorn.dfs.ds80", "epson.qx10.320"],
            1024 => ["epson.qx10.400"],
            _ => []
        };
        foreach (var formatId in formatIds)
            if (!formatId.Equals(image.FormatId, StringComparison.OrdinalIgnoreCase))
                yield return SectorImageInterpretation.Retag(image, formatId);
    }
}
