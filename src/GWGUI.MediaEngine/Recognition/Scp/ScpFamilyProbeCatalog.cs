using GWGUI.MediaEngine.Decoding;

namespace GWGUI.MediaEngine.Recognition.Scp;

/// <summary>Expose l'ordre immuable des huit sondes de familles SCP.</summary>
internal static class ScpFamilyProbeCatalog
{
    /// <summary>Sondes ordonnées ISO, Amiga, Commodore, Apple puis DEC.</summary>
    public static IReadOnlyList<ScpFamilyProbeDefinition> Definitions { get; } = Array.AsReadOnly(new[] { new ScpFamilyProbeDefinition(ScpFormatFamily.Iso, FluxCodecIds.IsoMfm), new(ScpFormatFamily.Iso, FluxCodecIds.IsoFm), new(ScpFormatFamily.Amiga, FluxCodecIds.AmigaMfm), new(ScpFormatFamily.Commodore, FluxCodecIds.CommodoreGcr), new(ScpFormatFamily.Apple, FluxCodecIds.AppleIIGcr), new(ScpFormatFamily.Apple, FluxCodecIds.AppleRwts18), new(ScpFormatFamily.Apple, FluxCodecIds.AppleMacGcr), new(ScpFormatFamily.Dec, FluxCodecIds.DecRx02) });
}
