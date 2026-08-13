using GWGUI.MediaEngine.Exploration.Interpretation;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Scp;

namespace GWGUI.MediaEngine.Exploration.Scp;

/// <summary>Reconstruit un candidat SCP et recherche ses systèmes de fichiers réels.</summary>
internal sealed class ScpCandidateInspector(FileSystemRegistry fileSystems, DiskImageInterpretationService interpretations)
{
    /// <summary>Inspecte un candidat, relit les images normalisées avec le même Reader et conserve son diagnostic de rejet.</summary>
    public async Task<ScpCandidateInspection> InspectAsync(ScpSectorImageCandidate candidate, string path, CancellationToken cancellationToken)
    {
        try
        {
            var image = await candidate.ReadAsync(path, null, cancellationToken).ConfigureAwait(false);
            var matches = new List<ExploredFileSystem>();
            foreach (var match in fileSystems.ReadCandidates(image, image.FormatId).Matches)
            {
                var normalized = interpretations.NormalizeRecognizedImage(image, match.ReaderId, match.Volume);
                if (ReferenceEquals(normalized, image))
                {
                    matches.Add(new(image.FormatId, match.ReaderId, image, match.Volume));
                    continue;
                }

                var volume = match.Volume;
                if (fileSystems.TryRead(normalized, match.ReaderId, out var normalizedMatch)) volume = normalizedMatch.Volume;
                matches.Add(new(normalized.FormatId, match.ReaderId, normalized, volume));
            }
            return new(candidate.Id, image, matches, null);
        }
        catch (InvalidDataException exception)
        {
            return new(candidate.Id, null, [], exception);
        }
    }
}
