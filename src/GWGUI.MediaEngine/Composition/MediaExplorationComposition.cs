using GWGUI.MediaEngine.Images.Formats.Tape;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Sequential;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaFileSystems.FileSystems.Iso9660;
using GWGUI.MediaFileSystems.FileSystems.Udf;
using GWGUI.MediaFileSystems.Exploration;
using GWGUI.MediaFileSystems.Exploration.Partitioning;
using GWGUI.MediaFileSystems.Exploration.Sequential;

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
            MediaFileSystemsReaderAdapter.CreateDefaultCatalog(),
            [
                new SequentialContentDecoderAdapter(sequentialMedia.Decoders),
                new MediaFileSystemsOpticalReaderAdapter(new UdfFileSystemReader()),
                new MediaFileSystemsOpticalReaderAdapter(new JolietExtensionReader()),
                new MediaFileSystemsOpticalReaderAdapter(new RockRidgeExtensionReader()),
                new MediaFileSystemsOpticalReaderAdapter(new Iso9660FileSystemReader())
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
