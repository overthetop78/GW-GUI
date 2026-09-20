using GWGUI.MediaEngine.Images.Reading.Documents;
using GWGUI.MediaEngine.Exploration.Interpretation;
using GWGUI.MediaEngine.Contracts.Explorer;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Recognition;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding.Sectors;
using GWGUI.MediaEngine.Constants;

using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection;

/// <summary>Orchestre en parallèle la détection, l'inspection et le classement des candidats SCP.</summary>
internal sealed class ScpAutomaticImageExplorer(IScpReader scpReader, ScpCandidateRegistry candidates, ScpFamilyProbe familyProbe, ScpCandidateInspector inspector, DiskImageDocumentFactory documents)
{
    /// <summary>Explore les candidats dans l'ordre déterministe, conserve les égalités et propage l'annulation.</summary>
    public async Task<ExploredDiskImage> ExploreAsync(
        string path,
        IProgress<ScpExplorationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var scpImage = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var families = await familyProbe.DetectAsync(path, progress, cancellationToken).ConfigureAwait(false);
        if (families.Count == 0) return documents.CreateUnknown(path, scpImage);
        var registrations = candidates.Automatic(families);
        var inspections = new List<ScpCandidateInspection>(registrations.Count);
        for (var index = 0; index < registrations.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var candidate = registrations[index];
            progress?.Report(new(
                ScpExplorationProgressKind.CandidateStarted,
                candidate.DisplayName,
                index,
                registrations.Count));
            var inspection = await inspector.InspectAsync(candidate, path, progress, cancellationToken).ConfigureAwait(false);
            inspections.Add(inspection);
            progress?.Report(new(
                ScpExplorationProgressKind.CandidateCompleted,
                candidate.DisplayName,
                index + 1,
                registrations.Count,
                inspection.Image is not null));
        }
        for (var index = 0; index < inspections.Count; index++)
        {
            var inspection = inspections[index];
            var normalizedImage = ScpCandidateRanker.NormalizeCompatibleGeometry(inspection.Image, scpImage);
            if (normalizedImage is null || ReferenceEquals(normalizedImage, inspection.Image)) continue;
            inspections[index] = inspection with
            {
                Image = normalizedImage,
                Matches = inspector.InspectFileSystems(normalizedImage)
            };
        }
        var ranking = ScpCandidateRanker.Rank(inspections, scpImage);
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
        IProgress<ScpExplorationProgress>? progress,
        CancellationToken cancellationToken)
    {
        scpReader.Remember(path, image);
        return ExploreAsync(path, progress, cancellationToken);
    }

    /// <summary>Conserve les formats étayés par une interprétation crédible et le meilleur décodage physique distinct.</summary>
    internal static IReadOnlyList<SectorImage> CredibleImages(
        ScpCandidateRanker.Result ranking,
        IEnumerable<ExploredFileSystem> detected)
    {
        var recognizedFormatIds = detected
            .Select(match => match.FormatId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (ranking.BestDecoded is null) return [];
        return ranking.DecodedImages
            .Where(image => !recognizedFormatIds.Contains(image.FormatId))
            .GroupBy(image => FileSystemInterpretationIdentity.FormatFamily(image.FormatId), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
    }
}
