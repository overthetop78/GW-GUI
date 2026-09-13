using GWGUI.MediaEngine.Exploration.Interpretation;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Decoding.Scp.Sectors;

namespace GWGUI.MediaEngine.Exploration.Scp;

/// <summary>Reconstruit un candidat SCP et recherche ses systèmes de fichiers réels.</summary>
internal sealed class ScpCandidateInspector(FileSystemRegistry fileSystems, DiskImageInterpretationService interpretations)
{
    /// <summary>Inspecte un candidat, relit les images normalisées avec le même Reader et conserve son diagnostic de rejet.</summary>
    public async Task<ScpCandidateInspection> InspectAsync(
        ScpSectorImageCandidate candidate,
        string path,
        IProgress<ScpExplorationProgress>? progress,
        CancellationToken cancellationToken)
    {
        try
        {
            var image = await candidate.ReadWithProgressAsync(path, null, progress, cancellationToken).ConfigureAwait(false);
            return new(candidate.Id, image, InspectFileSystems(image), null);
        }
        catch (InvalidDataException exception)
        {
            return new(candidate.Id, null, [], exception);
        }
    }

    /// <summary>Lit et met en mémoire toutes les interprétations de système de fichiers d'une image déjà reconstruite.</summary>
    internal IReadOnlyList<ExploredFileSystem> InspectFileSystems(GWGUI.MediaEngine.Representations.Sectors.SectorImage image)
    {
            var matches = new List<ExploredFileSystem>();
            foreach (var match in fileSystems.ReadCandidates(image, image.FormatId).Matches)
            {
                var normalized = interpretations.NormalizeRecognizedImage(image, match.ReaderId, match.Volume);
                ExploredFileSystem recognized;
                if (ReferenceEquals(normalized, image))
                {
                    recognized = new(match.ReaderId, image, match.Volume);
                }
                else if (fileSystems.TryRead(normalized, match.ReaderId, out var normalizedMatch))
                {
                    recognized = new(match.ReaderId, normalized, normalizedMatch.Volume);
                }
                else
                {
                    recognized = new(match.ReaderId, image, match.Volume);
                }

                matches.Add(recognized);
                foreach (var interpretation in interpretations.AdditionalFileSystemInterpretations(recognized.Image))
                {
                    if (!fileSystems.TryRead(interpretation, interpretation.FormatId, out var interpretedMatch)) continue;
                    matches.Add(new(interpretedMatch.ReaderId, interpretation, interpretedMatch.Volume));
                }
            }
            return matches;
    }
}
