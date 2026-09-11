using GWGUI.MediaEngine.Acquisition;
using GWGUI.MediaEngine.Conversion;
using GWGUI.MediaEngine.Conversion.Optical;
using GWGUI.MediaEngine.Conversion.Sequential;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.PhysicalWriting;
using GWGUI.MediaEngine.Reading;

namespace GWGUI.MediaEngine.Composition;

/// <summary>Assembles the shared media engine services without implementing format algorithms.</summary>
public sealed class MediaEngineComposition
{
    private MediaEngineComposition(
        MediaRecognitionComposition recognition,
        MediaExplorationComposition exploration,
        MediaConversionComposition conversion,
        MediaWritingComposition writing,
        MediaVisualizationComposition visualization)
    {
        Recognition = recognition;
        Exploration = exploration;
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

    public MediaExplorationComposition Exploration { get; }

    public MediaConversionComposition Conversion { get; }

    public MediaWritingComposition Writing { get; }

    public MediaVisualizationComposition Visualization { get; }

    public FloppyFluxAcquisitionService FloppyFluxAcquisitionService { get; }

    public FloppyMediaWritePlanningService FloppyMediaWritePlanningService { get; }

    public MediaImageReadingService ReadingService => Recognition.ReadingService;

    public MediaExplorer Explorer => Exploration.Explorer;

    public MediaConversionService ConversionService { get; }

    public OpticalImageConversionService OpticalConversionService { get; }

    public SequentialMediaConversionService SequentialConversionService { get; }

    public static MediaEngineComposition CreateDefault()
    {
        var recognition = MediaRecognitionComposition.CreateDefault();
        var sequentialMedia = SequentialMediaComposition.CreateDefault();
        var exploration = MediaExplorationComposition.CreateDefault(sequentialMedia);
        var decoding = new ScpSectorDecodingComposition(recognition.ScpReader, exploration.FileSystems);
        var conversion = new MediaConversionComposition(recognition, decoding, sequentialMedia);
        var writing = MediaWritingComposition.CreateDefault();
        var visualization = MediaVisualizationComposition.CreateDefault();
        return new MediaEngineComposition(recognition, exploration, conversion, writing, visualization);
    }
}
