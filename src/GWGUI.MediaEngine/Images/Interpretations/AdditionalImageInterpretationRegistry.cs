using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

/// <summary>Coordonne les politiques produisant des interprétations supplémentaires.</summary>
/// <param name="fileSystems">Registre utilisé par la politique IBM pour valider ses formats.</param>
internal sealed class AdditionalImageInterpretationRegistry
{
    /// <summary>Politiques examinées dans leur ordre de priorité.</summary>
    private readonly IReadOnlyList<IAdditionalImageInterpretationPolicy> policies;

    /// <summary>Crée le registre avec les politiques supplémentaires par défaut.</summary>
    /// <param name="fileSystems">Registre requis par la politique IBM.</param>
    public AdditionalImageInterpretationRegistry(FileSystemRegistry fileSystems) : this([new IbmAdditionalImageInterpretationPolicy(fileSystems), new MsxAdditionalImageInterpretationPolicy(), new CompatibleFormatInterpretationPolicy()]) { }

    /// <summary>Crée le registre avec une copie ordonnée des politiques fournies.</summary>
    /// <param name="policies">Politiques à copier dans leur ordre d'exécution.</param>
    public AdditionalImageInterpretationRegistry(IEnumerable<IAdditionalImageInterpretationPolicy> policies) => this.policies = Array.AsReadOnly(policies.ToArray());

    /// <summary>Énumère les interprétations supplémentaires d'une image compatible ISO.</summary>
    /// <param name="image">Image source à interpréter.</param>
    /// <returns>Interprétations produites dans l'ordre des politiques.</returns>
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
