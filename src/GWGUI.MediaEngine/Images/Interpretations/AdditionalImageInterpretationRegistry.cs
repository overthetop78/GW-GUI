using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

/// <summary>Coordonne les politiques produisant des interprétations supplémentaires.</summary>
/// <param name="fileSystems">Registre utilisé par la politique IBM pour valider ses formats.</param>
internal sealed class AdditionalImageInterpretationRegistry(FileSystemRegistry fileSystems)
{
    /// <summary>Politiques examinées dans leur ordre de priorité.</summary>
    private readonly IReadOnlyList<IAdditionalImageInterpretationPolicy> policies =
    [
        new IbmAdditionalImageInterpretationPolicy(fileSystems),
        new MsxAdditionalImageInterpretationPolicy(),
        new CompatibleFormatInterpretationPolicy()
    ];

    /// <summary>Énumère les interprétations supplémentaires d'une image compatible ISO.</summary>
    public IEnumerable<SectorImage> Create(SectorImage image)
    {
        if (!IsIsoCompatible(image.FormatId)) yield break;
        foreach (var policy in policies)
        {
            foreach (var interpretation in policy.Create(image)) yield return interpretation;
        }
    }

    /// <summary>Indique si le format peut partager les interprétations sectorielles ISO.</summary>
    private static bool IsIsoCompatible(string formatId) => formatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.AcornDfsPrefix, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.Imd, StringComparison.OrdinalIgnoreCase);
}
