using GWGUI.MediaEngine.Images.Formats.Tape;
using GWGUI.MediaEngine.Images.Visualization;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Reading;
using GWGUI.MediaEngine.Images.Conversion;
using GWGUI.MediaEngine.Images.Conversion.Optical;
using GWGUI.MediaEngine.Images.Conversion.Sequential;
using GWGUI.MediaEngine.PhysicalMedia.Writing;
using GWGUI.MediaEngine.Images.Reading;
using MediaExplorer = GWGUI.MediaEngine.Images.Reading.MediaExplorer;
using FileSystemsMediaExplorer = GWGUI.MediaFileSystems.Exploration.MediaExplorer;
using FileSystemRegistry = GWGUI.MediaFileSystems.Exploration.SectorFileSystemRegistry;

namespace GWGUI.MediaEngine;

/// <summary>Assembles the shared media engine services without implementing format algorithms.</summary>
public sealed class MediaEngineComposition
{
    private MediaEngineComposition(
        MediaRecognitionComposition recognition,
        MediaExplorer explorer,
        MediaConversionComposition conversion,
        MediaWritingComposition writing,
        MediaVisualizationComposition visualization)
    {
        Recognition = recognition;
        Explorer = explorer;
        Conversion = conversion;
        Writing = writing;
        Visualization = visualization;
        FloppyFluxAcquisitionService = new FloppyFluxAcquisitionService();
        FloppyMediaWritePlanningService = new FloppyMediaWritePlanningService(DiskImageExplorer.CreateDefault());
        ConversionService = new MediaConversionService(
            conversion.Registry,
            writing.Registry,
            writing.WritingService);
        OpticalConversionService = conversion.CreateOpticalImageConverter(writing);
        SequentialConversionService = conversion.CreateSequentialMediaConverter(writing);
    }

    public MediaRecognitionComposition Recognition { get; }

    public MediaConversionComposition Conversion { get; }

    public MediaWritingComposition Writing { get; }

    public MediaVisualizationComposition Visualization { get; }

    public FloppyFluxAcquisitionService FloppyFluxAcquisitionService { get; }

    public FloppyMediaWritePlanningService FloppyMediaWritePlanningService { get; }

    public MediaImageReadingService ReadingService => Recognition.ReadingService;

    public MediaExplorer Explorer { get; }

    public MediaConversionService ConversionService { get; }

    public OpticalImageConversionService OpticalConversionService { get; }

    public SequentialMediaConversionService SequentialConversionService { get; }

    public static MediaEngineComposition CreateDefault()
    {
        var recognition = MediaRecognitionComposition.CreateDefault();
        var sequentialMedia = SequentialMediaComposition.CreateDefault();
        var mediaFileSystems = FileSystemsMediaExplorer.CreateDefault(
            [new SequentialContentDecoderAdapter(sequentialMedia.Decoders)]);
        var explorer = new MediaExplorer(mediaFileSystems);
        var decoding = new ScpSectorDecodingComposition(recognition.ScpReader, new FileSystemRegistry());
        var conversion = new MediaConversionComposition(recognition, decoding, sequentialMedia);
        var writing = MediaWritingComposition.CreateDefault();
        var visualization = MediaVisualizationComposition.CreateDefault();
        return new MediaEngineComposition(recognition, explorer, conversion, writing, visualization);
    }
}
