using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>
/// Encodes the Lisa FileWare/Twiggy sector layout. FileWare uses the same
/// 6-and-2 sector payload coding as Apple's IWM family, with Lisa's format
/// byte and its own zoned 46-track, double-sided geometry.
/// </summary>
public sealed class AppleLisaFileWareGcrTrackEncoder : AppleMacGcrTrackEncoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.AppleLisaFileWareGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.AppleLisaFileWareGcr;
    /// <summary>Conserve la définition « Default Format » utilisée par ce codec.</summary>
    protected override byte DefaultFormat => AppleLisaFileWareGcrFormat.DefaultFormat;
}
