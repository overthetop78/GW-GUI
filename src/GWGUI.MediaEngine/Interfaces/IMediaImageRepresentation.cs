using GWGUI.Domain.Enums;

namespace GWGUI.MediaEngine.Interfaces;

/// <summary>Exposes the common read-only capabilities of a media image representation.</summary>
public interface IMediaImageRepresentation
{
    MediaRepresentationKind RepresentationKind { get; }

    long? LogicalLength { get; }

    bool SupportsRandomAccess { get; }

    bool SupportsSequentialAccess { get; }
}
