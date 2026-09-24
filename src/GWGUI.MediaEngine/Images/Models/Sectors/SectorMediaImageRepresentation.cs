using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces;
using IMediaSectorImage = global::GWGUI.MediaFileSystems.Interfaces.IMediaSectorImage;
using IMediaSectorRepresentation = global::GWGUI.MediaFileSystems.Interfaces.IMediaSectorRepresentation;

namespace GWGUI.MediaEngine.Images.Models.Sectors;

/// <summary>Exposes a sector image directly without synthesizing a flux representation.</summary>
public sealed class SectorMediaImageRepresentation : IMediaSectorRepresentation, IMediaImageRepresentation
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

    IMediaSectorImage IMediaSectorRepresentation.Image => Image;
}
