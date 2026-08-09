using GWGUI.Scp.FileSystems;
using GWGUI.Scp.Images.ScpDetection;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed class ScpImageExplorationService
{
    private readonly ScpCandidateRegistry candidates;
    private readonly ScpFamilyProbe familyProbe;
    private readonly FileSystemRegistry fileSystems;
    private readonly DiskImageInterpretationService interpretations;

    internal ScpImageExplorationService(
        ScpCandidateRegistry candidates,
        ScpFamilyProbe familyProbe,
        FileSystemRegistry fileSystems)
    {
        this.candidates = candidates;
        this.familyProbe = familyProbe;
        this.fileSystems = fileSystems;
        interpretations = new(fileSystems);
    }

    public async Task<ExploredDiskImage> ExploreAutomaticallyAsync(string path, CancellationToken cancellationToken)
    {
        SectorImage? bestDecoded = null;
        SectorImage? bestRecognized = null;
        ExploredFileSystem? bestRecognizedFileSystem = null;
        double bestRecognizedScore = -1;
        var detected = new List<ExploredFileSystem>();
        var decodedFormatIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        async Task InspectAsync(IEnumerable<Func<Task<SectorImage>>> candidateReaders)
        {
            var inspections = await Task.WhenAll(candidateReaders.Select(async read =>
            {
                try
                {
                    var candidate = await read().ConfigureAwait(false);
                    var matches = new List<(ExploredFileSystem Match, SectorImage Image)>();
                    foreach (var match in fileSystems.ReadAll(candidate))
                    {
                        var recognizedImage = interpretations.NormalizeRecognizedImage(candidate, match.ReaderId, match.Volume);
                        var recognizedVolume = ReferenceEquals(recognizedImage, candidate) ||
                            !fileSystems.TryRead(recognizedImage, match.ReaderId, out var normalizedVolume)
                            ? match.Volume : normalizedVolume;
                        matches.Add((new(recognizedImage.FormatId, match.ReaderId, recognizedVolume), recognizedImage));
                    }
                    foreach (var interpretation in interpretations.AdditionalFileSystemInterpretations(candidate))
                        if (fileSystems.TryRead(interpretation, interpretation.FormatId, out var volume))
                            matches.Add((new(interpretation.FormatId, interpretation.FormatId, volume), interpretation));
                    return (Image: candidate, Matches: (IReadOnlyList<(ExploredFileSystem Match, SectorImage Image)>)matches);
                }
                catch (InvalidDataException)
                {
                    return (Image: (SectorImage?)null,
                        Matches: (IReadOnlyList<(ExploredFileSystem Match, SectorImage Image)>)[]);
                }
            })).ConfigureAwait(false);

            foreach (var inspection in inspections)
            {
                if (inspection.Image is null) continue;
                if (bestDecoded is null ||
                    DiskImageInterpretationService.DecodeScore(inspection.Image) >
                    DiskImageInterpretationService.DecodeScore(bestDecoded))
                    bestDecoded = inspection.Image;
                if (DiskImageInterpretationService.DecodeScore(inspection.Image) >= .5)
                    decodedFormatIds.Add(inspection.Image.FormatId);
                foreach (var recognized in inspection.Matches)
                {
                    var score = DiskImageInterpretationService.DecodeScore(recognized.Image);
                    if (bestRecognized is null || score > bestRecognizedScore)
                    {
                        bestRecognized = recognized.Image;
                        bestRecognizedFileSystem = recognized.Match;
                        bestRecognizedScore = score;
                    }
                    var key = DiskImageInterpretationService.InterpretationIdentity(recognized.Match);
                    if (keys.Add(key)) detected.Add(recognized.Match);
                }
            }
        }

        var families = await familyProbe.DetectAsync(path, cancellationToken).ConfigureAwait(false);
        await InspectAsync(candidates.Automatic(path, families, cancellationToken)).ConfigureAwait(false);
        if (bestDecoded is null) return interpretations.Unknown(path);
        if (bestRecognizedFileSystem is null)
            return interpretations.CreateDocument(path, bestRecognized ?? bestDecoded, detected, decodedFormatIds.ToArray());
        var primaryIdentity = DiskImageInterpretationService.InterpretationIdentity(bestRecognizedFileSystem);
        var orderedDetected = new[] { bestRecognizedFileSystem }.Concat(
            detected.Where(match => DiskImageInterpretationService.InterpretationIdentity(match) != primaryIdentity &&
                                    DiskImageInterpretationService.IsCredibleAlternative(match.Volume))).ToArray();
        return interpretations.CreateDocument(path, bestRecognized ?? bestDecoded, orderedDetected, decodedFormatIds.ToArray());
    }

    public async Task<SectorImage> ReadAsync(string path, string? formatId, CancellationToken cancellationToken)
    {
        var selected = candidates.Selected(path, formatId, cancellationToken);
        if (selected is not null) return await selected().ConfigureAwait(false);

        SectorImage? firstDecoded = null;
        foreach (var read in candidates.Default(path, cancellationToken))
        {
            try
            {
                var candidate = await read().ConfigureAwait(false);
                firstDecoded ??= candidate;
                if (fileSystems.TryRead(candidate, null, out _)) return candidate;
            }
            catch (InvalidDataException) { }
        }
        return firstDecoded ?? throw new InvalidDataException("No supported sectors could be decoded from the SCP image.");
    }
}
