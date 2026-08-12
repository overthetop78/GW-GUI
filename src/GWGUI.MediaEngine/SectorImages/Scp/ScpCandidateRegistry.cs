namespace GWGUI.MediaEngine.SectorImages.Scp;

using GWGUI.MediaEngine.Recognition.Scp;

/// <summary>Associe les familles et formats explicites à des candidats SCP nommés et immuables.</summary>
internal sealed class ScpCandidateRegistry
{
    private readonly IReadOnlyList<ScpFormatSelection> selections;
    private readonly IReadOnlyList<ScpSectorImageCandidate> defaultCandidates;
    private readonly IReadOnlyDictionary<ScpFormatFamily, IReadOnlyList<ScpSectorImageCandidate>> familyCandidates;
    private readonly IReadOnlyList<ScpFormatFamily> familyOrder;
    private readonly ScpSectorImageCandidate fallback;

    /// <summary>Copie toutes les inscriptions et l'ordre déterministe des familles.</summary>
    public ScpCandidateRegistry(IEnumerable<ScpFormatSelection> selections, IEnumerable<ScpSectorImageCandidate> defaultCandidates, IEnumerable<KeyValuePair<ScpFormatFamily, IReadOnlyList<ScpSectorImageCandidate>>> familyCandidates, IEnumerable<ScpFormatFamily> familyOrder, ScpSectorImageCandidate fallback)
    {
        this.selections = Array.AsReadOnly(selections.ToArray());
        this.defaultCandidates = Array.AsReadOnly(defaultCandidates.ToArray());
        this.familyCandidates = new System.Collections.ObjectModel.ReadOnlyDictionary<ScpFormatFamily, IReadOnlyList<ScpSectorImageCandidate>>(familyCandidates.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<ScpSectorImageCandidate>)Array.AsReadOnly(pair.Value.ToArray())));
        this.familyOrder = Array.AsReadOnly(familyOrder.ToArray());
        this.fallback = fallback;
    }

    /// <summary>Retourne le candidat spécialisé du format explicite, ou le candidat ISO de repli.</summary>
    public ScpSectorImageCandidate? Selected(string? formatId)
    {
        if (formatId is null) return null;
        return selections.FirstOrDefault(selection => selection.Matches(formatId))?.Candidate ?? fallback;
    }

    /// <summary>Retourne les candidats par défaut dans leur ordre d'inscription.</summary>
    public IReadOnlyList<ScpSectorImageCandidate> Default() => defaultCandidates;

    /// <summary>Retourne les candidats des familles détectées en respectant l'ordre global et en ignorant les familles non inscrites.</summary>
    public IReadOnlyList<ScpSectorImageCandidate> Automatic(IReadOnlySet<ScpFormatFamily> families)
    {
        var selectedFamilies = familyOrder.Where(family => families.Count == 0 || families.Contains(family));
        return selectedFamilies.Where(familyCandidates.ContainsKey).SelectMany(family => familyCandidates[family]).ToArray();
    }
}
