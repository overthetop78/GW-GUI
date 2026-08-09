using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Visualization;

internal sealed class AtariVisualizationPolicy : SectorImageVisualizationPolicy
{
    public override bool CanHandle(SectorImage image) =>
        image.FormatId.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase);

    public override string EncoderId(SectorImage image) =>
        image.FormatId.Equals("atari.90", StringComparison.OrdinalIgnoreCase) ? "iso.fm" : "iso.mfm";

    public override SectorAddress VisualAddress(SectorImage image, SectorAddress address)
    {
        if (!image.FormatId.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) ||
            image.SectorsPerTrack != 1 || image.Cylinders <= 80) return address;
        var sectorsPerTrack = image.FormatId.Equals("atari.130", StringComparison.OrdinalIgnoreCase) ? 26 : 18;
        var logical = address.Cylinder;
        return new(logical / sectorsPerTrack, 0, logical % sectorsPerTrack + 1);
    }
}
