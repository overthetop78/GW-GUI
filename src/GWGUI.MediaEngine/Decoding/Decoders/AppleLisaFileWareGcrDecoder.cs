namespace GWGUI.MediaEngine.Decoding;

/// <summary>Decodes Lisa FileWare/Twiggy GCR sectors using their Lisa format identity.</summary>
public sealed class AppleLisaFileWareGcrDecoder : AppleMacGcrDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.AppleLisaFileWareGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.AppleLisaFileWareGcr;
}
