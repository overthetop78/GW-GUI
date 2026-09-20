using GWGUI.MediaEngine.Images.Formats.Tape;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaFileSystems.FileSystems.Iso9660;
using GWGUI.MediaFileSystems.FileSystems.Udf;
using GWGUI.MediaFileSystems.Exploration;
using GWGUI.MediaFileSystems.Exploration.Partitioning;
using GWGUI.MediaFileSystems.Exploration.Sequential;
using FileSystemsMediaExplorer = GWGUI.MediaFileSystems.Exploration.MediaExplorer;
using FileSystemsReader = GWGUI.MediaFileSystems.Interfaces.Exploration.IMediaFileSystemReader;
using MediaExplorer = GWGUI.MediaEngine.Images.Reading.MediaExplorer;
using FileSystemRegistry = GWGUI.MediaFileSystems.Exploration.SectorFileSystemRegistry;

namespace GWGUI.MediaEngine.Composition;

/// <summary>Provides the registered file-system readers and the common media explorer.</summary>
public sealed class MediaExplorationComposition
{
    private MediaExplorationComposition(
        FileSystemRegistry fileSystems,
        MediaVolumeDetectorRegistry volumeDetectors,
        FileSystemsMediaExplorer mediaFileSystems)
    {
        FileSystems = fileSystems;
        VolumeDetectors = volumeDetectors;
        MediaFileSystems = mediaFileSystems;
        Explorer = new MediaExplorer(mediaFileSystems);
    }

    public FileSystemRegistry FileSystems { get; }

    public MediaVolumeDetectorRegistry VolumeDetectors { get; }

    public FileSystemsMediaExplorer MediaFileSystems { get; }

    public MediaExplorer Explorer { get; }

    public static MediaExplorationComposition CreateDefault(SequentialMediaComposition sequentialMedia)
    {
        ArgumentNullException.ThrowIfNull(sequentialMedia);
        var volumeDetectors = new MediaVolumeDetectorRegistry(
        [
            new SequentialContentVolumeDetector(),
            new OpticalTrackVolumeDetector(),
            new GptVolumeDetector(),
            new MbrVolumeDetector(),
            new WholeMediaVolumeDetector()
        ]);
        var readers = FileSystemReaderCatalog.CreateDefault()
            .Cast<FileSystemsReader>()
            .Concat(new FileSystemsReader[]
            {
                new SequentialContentDecoderAdapter(sequentialMedia.Decoders),
                new UdfFileSystemReader(),
                new JolietExtensionReader(),
                new RockRidgeExtensionReader(),
                new Iso9660FileSystemReader()
            });
        return new(
            new FileSystemRegistry(),
            volumeDetectors,
            new FileSystemsMediaExplorer(readers, volumeDetectors));
    }
}
