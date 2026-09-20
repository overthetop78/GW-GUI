namespace GWGUI.MediaFileSystems.Interfaces;

/// <summary>Exposes a decoded logical sector to file-system readers.</summary>
public interface IMediaSectorBlock
{
    int LogicalBlock { get; }

    int Cylinder { get; }

    int Head { get; }

    int PhysicalSectorNumber { get; }

    IReadOnlyList<byte> Data { get; }

    bool? IntegrityValid { get; }

    IReadOnlyList<byte>? Tag { get; }
}
