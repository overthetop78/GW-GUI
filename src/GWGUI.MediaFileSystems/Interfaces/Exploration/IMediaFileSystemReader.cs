using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaFileSystems;

namespace GWGUI.MediaFileSystems.Interfaces.Exploration;

/// <summary>Probes and reads a file system from an addressable volume without assuming a physical media family.</summary>
public interface IMediaFileSystemReader
{
    string Id { get; }

    IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; }

    bool CanRead(MediaImageDocument document, MediaVolumeDescriptor volume);

    FileSystemVolume Read(MediaImageDocument document, MediaVolumeDescriptor volume);
}
