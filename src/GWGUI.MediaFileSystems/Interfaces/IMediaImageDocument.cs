using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;

namespace GWGUI.MediaFileSystems.Interfaces;

/// <summary>Expose un média déjà chargé et reconnu aux lecteurs de volumes et de fichiers.</summary>
public interface IMediaImageDocument
{
    IMediaSourceDescriptor Source { get; }
    string FormatId { get; }
    bool IsTape { get; }
    bool IsHardDisk { get; }
    IMediaImageRepresentation Representation { get; }
    IReadOnlyList<MediaVolumeDescriptor> Volumes { get; }
    IReadOnlyList<string> Diagnostics { get; }
    IReadOnlyDictionary<string, string> Metadata { get; }
}
