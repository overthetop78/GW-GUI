namespace GWGUI.MediaEngine.Decoding;

/// <summary>Decodes Lisa FileWare/Twiggy GCR sectors using their Lisa format identity.</summary>
public sealed class AppleLisaFileWareGcrDecoder : AppleMacGcrDecoder
{
    public override string Id => FluxCodecIds.AppleLisaFileWareGcr;
    public override string DisplayName => FluxCodecDisplayNames.AppleLisaFileWareGcr;
}
