using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Partitioning;
using GWGUI.MediaEngine.Exploration.Sequential;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Iso9660;
using GWGUI.MediaEngine.FileSystems.Udf;

namespace GWGUI.MediaEngine.Composition;

/// <summary>Provides the registered file-system readers and the common media explorer.</summary>
public sealed class MediaExplorationComposition
{
    private MediaExplorationComposition(
        FileSystemRegistry fileSystems,
        MediaVolumeDetectorRegistry volumeDetectors)
    {
        FileSystems = fileSystems;
        VolumeDetectors = volumeDetectors;
        Explorer = new MediaExplorer(fileSystems, volumeDetectors);
    }

    public FileSystemRegistry FileSystems { get; }

    public MediaVolumeDetectorRegistry VolumeDetectors { get; }

    public MediaExplorer Explorer { get; }

    public static MediaExplorationComposition CreateDefault(SequentialMediaComposition sequentialMedia)
    {
        ArgumentNullException.ThrowIfNull(sequentialMedia);
        return new(
        new FileSystemRegistry(
            FileSystemReaderCatalog.CreateDefault(),
            [
                new SequentialContentFileSystemReader(sequentialMedia.Decoders),
                new UdfFileSystemReader(),
                new JolietExtensionReader(),
                new RockRidgeExtensionReader(),
                new Iso9660FileSystemReader()
            ]),
        new MediaVolumeDetectorRegistry(
        [
            new SequentialContentVolumeDetector(),
            new OpticalTrackVolumeDetector(),
            new GptVolumeDetector(),
            new MbrVolumeDetector(),
            new WholeMediaVolumeDetector()
        ]));
    }
}
