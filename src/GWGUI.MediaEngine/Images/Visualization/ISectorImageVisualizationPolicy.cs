using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Visualization;

internal interface ISectorImageVisualizationPolicy
{
    bool CanHandle(SectorImage image);
    string EncoderId(SectorImage image);
    SectorAddress VisualAddress(SectorImage image, SectorAddress address);
    IReadOnlyList<TrackSector> CreateTrackSectors(SectorImage image,
        IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items);
    IReadOnlyDictionary<string, int>? TrackAttributes(SectorImage image, int sectorCount);
    uint BitCellTicks(SectorImage image, int cylinder);
}
