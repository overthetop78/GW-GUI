namespace GWGUI.MediaFileSystems.Interfaces;

/// <summary>Exposes the sector image of an already decoded media representation.</summary>
public interface IMediaSectorRepresentation : IMediaImageRepresentation
{
    IMediaSectorImage Image { get; }
}
