using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>
/// Encodes the Lisa FileWare/Twiggy sector layout. FileWare uses the same
/// 6-and-2 sector payload coding as Apple's IWM family, with Lisa's format
/// byte and its own zoned 46-track, double-sided geometry.
/// </summary>
public sealed class AppleLisaFileWareGcrTrackEncoder : AppleMacGcrTrackEncoder
{
    public override string Id => FluxCodecIds.AppleLisaFileWareGcr;
    public override string DisplayName => FluxCodecDisplayNames.AppleLisaFileWareGcr;
    protected override byte DefaultFormat => AppleLisaFileWareGcrFormat.DefaultFormat;
}
