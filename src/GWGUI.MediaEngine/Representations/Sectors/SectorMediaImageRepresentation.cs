using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Representations.Sectors;

/// <summary>Exposes a sector image directly without synthesizing a flux representation.</summary>
public sealed class SectorMediaImageRepresentation : IMediaImageRepresentation
{
    public SectorMediaImageRepresentation(SectorImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        Image = image;
    }

    public MediaRepresentationKind RepresentationKind => MediaRepresentationKind.Sectors;

    public long? LogicalLength => Image.Capacity;

    public bool SupportsRandomAccess => true;

    public bool SupportsSequentialAccess => true;

    public SectorImage Image { get; }
}
