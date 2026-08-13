using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Exploration.Documents;
using GWGUI.MediaEngine.Exploration.Interpretation;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Recognition.Scp;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Scp;

namespace GWGUI.MediaEngine.Exploration.Scp;

/// <summary>Orchestre en parallèle la détection, l'inspection et le classement des candidats SCP.</summary>
internal sealed class ScpAutomaticImageExplorer(IScpReader scpReader, ScpCandidateRegistry candidates, ScpFamilyProbe familyProbe, ScpCandidateInspector inspector, DiskImageDocumentFactory documents)
{
    /// <summary>Explore les candidats dans l'ordre déterministe, conserve les égalités et propage l'annulation.</summary>
    public async Task<ExploredDiskImage> ExploreAsync(string path, CancellationToken cancellationToken)
    {
        var scpImage = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var families = await familyProbe.DetectAsync(path, cancellationToken).ConfigureAwait(false);
        var registrations = candidates.Automatic(families);
        var inspections = await Task.WhenAll(registrations.Select(candidate => inspector.InspectAsync(candidate, path, cancellationToken))).ConfigureAwait(false);
        var ranking = ScpCandidateRanker.Rank(inspections);
        if (ranking.BestDecoded is null)
        {
            return documents.CreateUnknown(path, scpImage);
        }
        if (ranking.BestFileSystem is null)
        {
            return documents.Create(
                path,
                ranking.BestRecognized ?? ranking.BestDecoded,
                ranking.Detected,
                CredibleImages(ranking, ranking.Detected),
                scpImage);
        }
        var primaryIdentity = FileSystemInterpretationIdentity.Create(ranking.BestFileSystem);
        var ordered = new[] { ranking.BestFileSystem }.Concat(ranking.Detected.Where(match => FileSystemInterpretationIdentity.Create(match) != primaryIdentity && FileSystemAlternativePolicy.IsCredible(match.Volume))).ToArray();
        return documents.Create(
            path,
            ranking.BestRecognized ?? ranking.BestDecoded,
            ordered,
            CredibleImages(ranking, ordered),
            scpImage);
    }

    /// <summary>Explore une capture déjà en mémoire sans relire le fichier qui vient d'être produit.</summary>
    public Task<ExploredDiskImage> ExploreAsync(
        string path,
        ScpImage image,
        CancellationToken cancellationToken)
    {
        scpReader.Remember(path, image);
        return ExploreAsync(path, cancellationToken);
    }

    /// <summary>Conserve les formats étayés par une interprétation crédible et le meilleur décodage physique distinct.</summary>
    private static IReadOnlyList<SectorImage> CredibleImages(
        ScpCandidateRanker.Result ranking,
        IEnumerable<ExploredFileSystem> detected)
    {
        var recognizedFormatIds = detected
            .Select(match => match.FormatId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (recognizedFormatIds.Count > 1)
        {
            return [];
        }

        var physicalImage = ranking.DecodedImages.FirstOrDefault(image =>
            !recognizedFormatIds.Contains(image.FormatId));
        if (physicalImage is null)
        {
            return [];
        }

        return [physicalImage];
    }
}
