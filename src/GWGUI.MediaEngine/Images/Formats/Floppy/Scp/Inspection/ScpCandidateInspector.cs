using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Contracts.Explorer;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding.Sectors;
using FileSystemRegistry = GWGUI.MediaFileSystems.Exploration.SectorFileSystemRegistry;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection;

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
    internal IReadOnlyList<ExploredFileSystem> InspectFileSystems(GWGUI.MediaEngine.Images.Models.Sectors.SectorImage image)
    {
            var matches = new List<ExploredFileSystem>();
            foreach (var match in fileSystems.ReadCandidates(image, image.FormatId).Matches)
            {
                var volume = FileSystemVolumeMapper.ConvertVolume(match.Volume, image.FormatId);
                var normalized = interpretations.NormalizeRecognizedImage(image, match.ReaderId);
                ExploredFileSystem recognized;
                if (ReferenceEquals(normalized, image))
                {
                    recognized = new(match.ReaderId, image, volume);
                }
                else if (fileSystems.TryRead(normalized, match.ReaderId, out var normalizedMatch))
                {
                    recognized = new(match.ReaderId, normalized,
                        FileSystemVolumeMapper.ConvertVolume(normalizedMatch.Volume, normalized.FormatId));
                }
                else
                {
                    recognized = new(match.ReaderId, image, volume);
                }

                matches.Add(recognized);
                foreach (var interpretation in interpretations.AdditionalImageCandidates(recognized.Image))
                {
                    if (!fileSystems.TryRead(interpretation, interpretation.FormatId, out var interpretedMatch)) continue;
                    matches.Add(new(interpretedMatch.ReaderId, interpretation,
                        FileSystemVolumeMapper.ConvertVolume(interpretedMatch.Volume, interpretation.FormatId)));
                }
            }
            return matches;
    }
}
