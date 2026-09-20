using System.Collections.Frozen;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Interfaces.Exploration;
using MediaVolumeDescriptor = GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using FileSystemsReader = GWGUI.MediaFileSystems.Interfaces.Exploration.IMediaFileSystemReader;

namespace GWGUI.MediaEngine.Images.Reading;

/// <summary>Transmet les pistes optiques décodées au lecteur de système de fichiers.</summary>
internal sealed class MediaFileSystemsOpticalReaderAdapter(FileSystemsReader reader) : IMediaFileSystemReader
{
    private static readonly IReadOnlySet<MediaRepresentationKind> OpticalTracks =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();

    public string Id => reader.Id;

    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => OpticalTracks;

    public bool CanRead(MediaImageDocument document, MediaVolumeDescriptor volume) => reader.CanRead(document, volume);

    public FileSystemVolume Read(MediaImageDocument document, MediaVolumeDescriptor volume) =>
        MediaFileSystemsReaderAdapter.ConvertVolume(reader.Read(document, volume));
}
