using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using IMediaImageDocument = global::GWGUI.MediaFileSystems.Interfaces.IMediaImageDocument;
using GWGUI.MediaFileSystems;

namespace GWGUI.MediaFileSystems.Interfaces.Exploration;

/// <summary>Probes and reads a file system from an addressable volume without assuming a physical media family.</summary>
public interface IMediaFileSystemReader
{
    string Id { get; }

    bool CanRead(IMediaImageDocument document, MediaVolumeDescriptor volume);

    FileSystemVolume Read(IMediaImageDocument document, MediaVolumeDescriptor volume);
}
