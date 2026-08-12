using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les secteurs Lisa FileWare/Twiggy avec le codage 6-and-2 de la famille IWM, l'octet de format Lisa et sa géométrie zonée de 46 pistes sur deux faces.</summary>
public sealed class AppleLisaFileWareGcrTrackEncoder : AppleMacGcrTrackEncoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.AppleLisaFileWareGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.AppleLisaFileWareGcr;
    /// <summary>Conserve la définition « Default Format » utilisée par ce codec.</summary>
    protected override byte DefaultFormat => AppleLisaFileWareGcrFormat.DefaultFormat;
}
