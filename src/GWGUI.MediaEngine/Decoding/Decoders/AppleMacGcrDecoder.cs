namespace GWGUI.MediaEngine.Decoding;

/// <summary>Expose le décodage Apple IWM GCR avec l'identité Macintosh.</summary>
public sealed class AppleMacGcrDecoder : AppleIwmGcrDecoder
{
    /// <summary>Obtient l'identifiant technique du codec Macintosh.</summary>
    public override string Id => FluxCodecIds.AppleMacGcr;

    /// <summary>Obtient le nom affiché du codec Macintosh.</summary>
    public override string DisplayName => FluxCodecDisplayNames.AppleMacGcr;
}
