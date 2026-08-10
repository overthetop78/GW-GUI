using GWGUI.MediaEngine.SectorImages;

using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Images.Visualization;

internal sealed class AtariVisualizationPolicy : SectorImageVisualizationPolicy
{
    public override bool CanHandle(SectorImage image) =>
        image.FormatId.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase);

    public override string EncoderId(SectorImage image) =>
        image.FormatId.Equals(DiskImageFormatIds.Atari90, StringComparison.OrdinalIgnoreCase) ? "iso.fm" : "iso.mfm";

    public override SectorAddress VisualAddress(SectorImage image, SectorAddress address)
    {
        if (!image.FormatId.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase) ||
            image.SectorsPerTrack != 1 || image.Cylinders <= DiskGeometryConstants.EightyTrackCylinderCount) return address;
        var sectorsPerTrack = image.FormatId.Equals(DiskImageFormatIds.Atari130, StringComparison.OrdinalIgnoreCase) ? 26 : 18;
        var logical = address.Cylinder;
        return new(logical / sectorsPerTrack, 0, logical % sectorsPerTrack + 1);
    }
}
