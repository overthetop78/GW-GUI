namespace GWGUI.MediaEngine.Encoding;

/// <summary>Spécialise l'encodeur GCR IWM commun avec l'identité du format Lisa FileWare/Twiggy.</summary>
public sealed class AppleLisaFileWareGcrTrackEncoder : AppleMacGcrTrackEncoder
{
    /// <summary>Obtient l'identifiant technique central du codec Lisa FileWare.</summary>
    public override string Id => FluxCodecIds.AppleLisaFileWareGcr;
    /// <summary>Obtient le nom affiché central du codec Lisa FileWare.</summary>
    public override string DisplayName => FluxCodecDisplayNames.AppleLisaFileWareGcr;
}
