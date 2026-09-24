namespace GWGUI.MediaFileSystems.Interfaces;

/// <summary>Exposes the source details needed to locate volumes in a decoded image.</summary>
public interface IMediaSourceDescriptor
{
    string PrimaryPath { get; }
    long? KnownLength { get; }
}
