using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Interpretation.Contracts;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Interpretation;

/// <summary>Coordonne une copie ordonnée des politiques produisant des interprétations supplémentaires.</summary>
internal sealed class AdditionalImageInterpretationRegistry
{
    private readonly IReadOnlyList<IAdditionalImageInterpretationPolicy> policies;

    /// <summary>Construit le registre en copiant les politiques dans leur ordre d'exécution.</summary>
    public AdditionalImageInterpretationRegistry(IEnumerable<IAdditionalImageInterpretationPolicy> policies) => this.policies = Array.AsReadOnly(policies.ToArray());

    /// <summary>Énumère de façon différée les candidats des politiques pour une famille source autorisée.</summary>
    public IEnumerable<SectorImage> Create(SectorImage image)
    {
        if (!SupportsAdditionalInterpretations(image.FormatId)) yield break;
        foreach (var policy in policies)
            foreach (var candidate in policy.CreateCandidates(image)) yield return candidate;
    }

    /// <summary>Indique si le format source appartient aux cinq familles sectorielles historiquement réinterprétées.</summary>
    private static bool SupportsAdditionalInterpretations(string formatId) => formatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.AcornDfsPrefix, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.Imd, StringComparison.OrdinalIgnoreCase);
}
