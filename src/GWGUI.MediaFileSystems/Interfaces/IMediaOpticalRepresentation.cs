namespace GWGUI.MediaFileSystems.Interfaces;

/// <summary>Exposes the tracks of an already decoded optical representation.</summary>
public interface IMediaOpticalRepresentation : IMediaImageRepresentation
{
    IReadOnlyList<IMediaOpticalTrack>? Tracks { get; }
}
