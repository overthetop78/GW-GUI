namespace GWGUI.MediaEngine.Decoding;

/// <summary>Decodes Lisa FileWare/Twiggy GCR sectors using their Lisa format identity.</summary>
public sealed class AppleLisaFileWareGcrDecoder : AppleMacGcrDecoder
{
    public override string Id => "applelisa.fileware.gcr";
    public override string DisplayName => "Apple Lisa FileWare GCR";
}
