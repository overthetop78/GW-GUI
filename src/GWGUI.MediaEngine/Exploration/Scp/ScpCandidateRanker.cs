using GWGUI.MediaEngine.Exploration.Interpretation;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Scoring;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Scp;

/// <summary>Classe les inspections SCP dans leur ordre et conserve le premier résultat en cas d'égalité.</summary>
internal static class ScpCandidateRanker
{
    /// <summary>Résultat agrégé du classement automatique.</summary>
    internal sealed record Result(SectorImage? BestDecoded, SectorImage? BestRecognized, ExploredFileSystem? BestFileSystem, IReadOnlyList<ExploredFileSystem> Detected, IReadOnlyList<string> DecodedFormatIds, IReadOnlyList<ScpCandidateInspection> Rejected);

    /// <summary>Calcule une fois chaque score, déduplique les systèmes et conserve l'ordre des formats et diagnostics.</summary>
    public static Result Rank(IEnumerable<ScpCandidateInspection> inspections)
    {
        SectorImage? bestDecoded = null;
        SectorImage? bestRecognized = null;
        ExploredFileSystem? bestFileSystem = null;
        var bestDecodedScore = ScpExplorationThresholds.NoRecognizedScore;
        var bestRecognizedScore = ScpExplorationThresholds.NoRecognizedScore;
        var detected = new List<ExploredFileSystem>();
        var identities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var formatIds = new List<string>();
        var formatIdentity = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var rejected = new List<ScpCandidateInspection>();
        foreach (var inspection in inspections)
        {
            if (inspection.Image is null) { rejected.Add(inspection); continue; }
            var decodedScore = DiskImageDecodeScore.Calculate(inspection.Image);
            if (decodedScore > bestDecodedScore) { bestDecoded = inspection.Image; bestDecodedScore = decodedScore; }
            if (decodedScore >= ScpExplorationThresholds.MinimumDecodedFormatScore && formatIdentity.Add(inspection.Image.FormatId)) formatIds.Add(inspection.Image.FormatId);
            foreach (var recognized in inspection.Matches)
            {
                var score = DiskImageDecodeScore.Calculate(recognized.Image);
                if (score > bestRecognizedScore) { bestRecognized = recognized.Image; bestFileSystem = recognized.Match; bestRecognizedScore = score; }
                if (identities.Add(FileSystemInterpretationIdentity.Create(recognized.Match))) detected.Add(recognized.Match);
            }
        }
        return new(bestDecoded, bestRecognized, bestFileSystem, detected, formatIds, rejected);
    }
}
