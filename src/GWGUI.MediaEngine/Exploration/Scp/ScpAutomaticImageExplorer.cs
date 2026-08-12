using GWGUI.MediaEngine.Exploration.Documents;
using GWGUI.MediaEngine.Exploration.Interpretation;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Recognition.Scp;
using GWGUI.MediaEngine.SectorImages.Scp;

namespace GWGUI.MediaEngine.Exploration.Scp;

/// <summary>Orchestre en parallèle la détection, l'inspection et le classement des candidats SCP.</summary>
internal sealed class ScpAutomaticImageExplorer(ScpCandidateRegistry candidates, ScpFamilyProbe familyProbe, ScpCandidateInspector inspector, DiskImageDocumentFactory documents)
{
    /// <summary>Explore les candidats dans l'ordre déterministe, conserve les égalités et propage l'annulation.</summary>
    public async Task<ExploredDiskImage> ExploreAsync(string path, CancellationToken cancellationToken)
    {
        var families = await familyProbe.DetectAsync(path, cancellationToken).ConfigureAwait(false);
        var registrations = candidates.Automatic(families);
        var inspections = await Task.WhenAll(registrations.Select(candidate => inspector.InspectAsync(candidate, path, cancellationToken))).ConfigureAwait(false);
        var ranking = ScpCandidateRanker.Rank(inspections);
        if (ranking.BestDecoded is null) return documents.CreateUnknown(path);
        if (ranking.BestFileSystem is null) return documents.Create(path, ranking.BestRecognized ?? ranking.BestDecoded, ranking.Detected, CredibleFormatIds(ranking, ranking.Detected));
        var primaryIdentity = FileSystemInterpretationIdentity.Create(ranking.BestFileSystem);
        var ordered = new[] { ranking.BestFileSystem }.Concat(ranking.Detected.Where(match => FileSystemInterpretationIdentity.Create(match) != primaryIdentity && FileSystemAlternativePolicy.IsCredible(match.Volume))).ToArray();
        return documents.Create(path, ranking.BestRecognized ?? ranking.BestDecoded, ordered, CredibleFormatIds(ranking, ordered));
    }

    /// <summary>Conserve les formats étayés par une interprétation crédible et le meilleur décodage physique distinct.</summary>
    private static IReadOnlyList<string> CredibleFormatIds(ScpCandidateRanker.Result ranking, IEnumerable<ExploredFileSystem> detected)
    {
        var formats = detected.Select(match => match.FormatId).ToList();
        var physical = ranking.DecodedFormatIds.FirstOrDefault();
        if (formats.Count <= 1 && physical is not null && !formats.Contains(physical, StringComparer.OrdinalIgnoreCase)) formats.Add(physical);
        return formats.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
