namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les secteurs GCR Lisa FileWare/Twiggy en conservant l'identité du format Lisa.</summary>
public sealed class AppleLisaFileWareGcrDecoder : AppleIwmGcrDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.AppleLisaFileWareGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.AppleLisaFileWareGcr;
}
