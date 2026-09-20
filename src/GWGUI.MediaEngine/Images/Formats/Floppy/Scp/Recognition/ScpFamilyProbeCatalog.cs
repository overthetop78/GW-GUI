using GWGUI.MediaEngine.Images.Reading.Decoding;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Recognition;

/// <summary>Expose les sondes de familles SCP dans leur ordre d'affichage et d'exécution.</summary>
internal static class ScpFamilyProbeCatalog
{
    public static IReadOnlyList<ScpFamilyProbeDefinition> Definitions { get; } = Array.AsReadOnly(
        new[]
        {
            new ScpFamilyProbeDefinition(ScpFormatFamily.Amiga, "Amiga", [FluxCodecIds.AmigaMfm]),
            new ScpFamilyProbeDefinition(ScpFormatFamily.Iso, "IBM PC / Atari ST / ISO", [FluxCodecIds.IsoMfm, FluxCodecIds.IsoFm]),
            new ScpFamilyProbeDefinition(ScpFormatFamily.Commodore, "Commodore", [FluxCodecIds.CommodoreGcr]),
            new ScpFamilyProbeDefinition(ScpFormatFamily.Apple, "Apple", [FluxCodecIds.AppleIIGcr, FluxCodecIds.AppleRwts18, FluxCodecIds.AppleMacGcr]),
            new ScpFamilyProbeDefinition(ScpFormatFamily.Dec, "DEC", [FluxCodecIds.DecRx02])
        });
}
