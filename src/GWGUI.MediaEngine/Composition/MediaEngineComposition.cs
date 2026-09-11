using GWGUI.MediaEngine.Conversion;
using GWGUI.MediaEngine.Exploration;
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
        ConversionService = new MediaConversionService(
            conversion.Registry,
            writing.Registry,
            writing.WritingService);
    }

    public MediaRecognitionComposition Recognition { get; }

    public MediaExplorationComposition Exploration { get; }

    public MediaConversionComposition Conversion { get; }

    public MediaWritingComposition Writing { get; }

    public MediaVisualizationComposition Visualization { get; }

    public MediaImageReadingService ReadingService => Recognition.ReadingService;

    public MediaExplorer Explorer => Exploration.Explorer;

    public MediaConversionService ConversionService { get; }

    public static MediaEngineComposition CreateDefault()
    {
        var recognition = MediaRecognitionComposition.CreateDefault();
        var exploration = MediaExplorationComposition.CreateDefault();
        var decoding = new ScpSectorDecodingComposition(recognition.ScpReader, exploration.FileSystems);
        var conversion = new MediaConversionComposition(recognition, decoding);
        var writing = MediaWritingComposition.CreateDefault();
        var visualization = MediaVisualizationComposition.CreateDefault();
        return new MediaEngineComposition(recognition, exploration, conversion, writing, visualization);
    }
}
