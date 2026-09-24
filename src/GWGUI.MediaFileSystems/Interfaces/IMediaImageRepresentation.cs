namespace GWGUI.MediaFileSystems.Interfaces;

/// <summary>Exposes the common read-only capabilities of a media image representation.</summary>
public interface IMediaImageRepresentation
{
    long? LogicalLength { get; }

    bool SupportsRandomAccess { get; }

    bool SupportsSequentialAccess { get; }
}
