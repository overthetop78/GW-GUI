using System.Collections.Concurrent;
using GWGUI.Scp.Decoding;
using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed class ScpImageExplorationService(
    AmigaScpSectorImageReader amigaReader,
    IsoScpSectorImageReader isoReader,
    AtariScpSectorImageReader atariReader,
    AmstradScpSectorImageReader amstradReader,
    BbcScpSectorImageReader bbcReader,
    IbmPcScpSectorImageReader ibmReader,
    EpsonQx10ScpSectorImageReader epsonReader,
    UcsdScpSectorImageReader ucsdReader,
    CommodoreScpSectorImageReader commodoreReader,
    AppleScpSectorImageReader appleReader,
    DecRx02ScpSectorImageReader decReader,
    FileSystemRegistry fileSystems,
    IScpReader scpReader,
    FluxDecoderRegistry decoders)
{
    private readonly DiskImageInterpretationService interpretations = new(fileSystems);

    public async Task<ExploredDiskImage> ExploreAutomaticallyAsync(string path, CancellationToken cancellationToken)
    {
        SectorImage? bestDecoded = null;
        SectorImage? bestRecognized = null;
        ExploredFileSystem? bestRecognizedFileSystem = null;
        double bestRecognizedScore = -1;
        var detected = new List<ExploredFileSystem>();
        var decodedFormatIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        async Task InspectAsync(IEnumerable<Func<Task<SectorImage>>> candidates)
        {
            var inspections = await Task.WhenAll(candidates.Select(async read =>
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
                    var key = DiskImageInterpretationService.FileSystemIdentity(recognized.Match.Volume);
                    if (keys.Add(key)) detected.Add(recognized.Match);
                }
            }
        }

        var families = await ProbeFamiliesAsync(path, cancellationToken).ConfigureAwait(false);
        await InspectAsync(AllCandidates(path, families, cancellationToken)).ConfigureAwait(false);
        if (bestDecoded is null) return interpretations.Unknown(path);
        if (bestRecognizedFileSystem is null)
            return interpretations.CreateDocument(path, bestRecognized ?? bestDecoded, detected, decodedFormatIds.ToArray());
        var primaryIdentity = DiskImageInterpretationService.FileSystemIdentity(bestRecognizedFileSystem.Volume);
        var orderedDetected = new[] { bestRecognizedFileSystem }.Concat(
            detected.Where(match => DiskImageInterpretationService.FileSystemIdentity(match.Volume) != primaryIdentity &&
                                    DiskImageInterpretationService.IsCredibleAlternative(match.Volume))).ToArray();
        return interpretations.CreateDocument(path, bestRecognized ?? bestDecoded, orderedDetected, decodedFormatIds.ToArray());
    }

    public async Task<SectorImage> ReadAsync(string path, string? formatId, CancellationToken cancellationToken)
    {
        var selected = CreateSelectedReader(path, formatId, cancellationToken);
        if (selected is not null) return await selected().ConfigureAwait(false);

        SectorImage? firstDecoded = null;
        foreach (var read in DefaultCandidates(path, cancellationToken))
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

    private Func<Task<SectorImage>>? CreateSelectedReader(
        string path,
        string? formatId,
        CancellationToken cancellationToken)
    {
        if (formatId is null) return null;
        if (formatId.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase))
            return () => amigaReader.ReadAsync(path, cancellationToken);
        if (formatId.StartsWith("commodore.", StringComparison.OrdinalIgnoreCase))
            return () => commodoreReader.ReadAsync(path, formatId, cancellationToken);
        if (formatId.StartsWith("amstrad.", StringComparison.OrdinalIgnoreCase))
            return () => amstradReader.ReadAsync(path, formatId, cancellationToken);
        if (formatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase))
            return () => ibmReader.ReadAsync(path, formatId, cancellationToken);
        if (formatId.StartsWith("acorn.dfs.", StringComparison.OrdinalIgnoreCase))
            return () => bbcReader.ReadAsync(path, formatId, cancellationToken);
        if (formatId.Equals("dec.rx02", StringComparison.OrdinalIgnoreCase))
            return () => decReader.ReadAsync(path, cancellationToken);
        if (formatId.Equals("mac.1440", StringComparison.OrdinalIgnoreCase))
            return () => ibmReader.ReadAsync(path, formatId, cancellationToken);
        if (formatId.StartsWith("epson.qx10.", StringComparison.OrdinalIgnoreCase))
            return () => epsonReader.ReadAsync(path, formatId, cancellationToken);
        if (formatId.Equals("ucsd.ibm.mfm", StringComparison.OrdinalIgnoreCase))
            return () => ucsdReader.ReadAsync(path, cancellationToken);
        if (formatId.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) ||
            formatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase))
            return () => atariReader.ReadAsync(path, formatId, cancellationToken);
        if (formatId.StartsWith("apple", StringComparison.OrdinalIgnoreCase) ||
            formatId.StartsWith("mac.", StringComparison.OrdinalIgnoreCase))
            return () => appleReader.ReadAsync(path, formatId, cancellationToken);
        return () => isoReader.ReadAsync(path, formatId, cancellationToken);
    }

    private IEnumerable<Func<Task<SectorImage>>> DefaultCandidates(string path, CancellationToken cancellationToken)
    {
        yield return () => isoReader.ReadAsync(path, null, cancellationToken);
        yield return () => amigaReader.ReadAsync(path, cancellationToken);
        yield return () => commodoreReader.ReadAsync(path, "commodore.1581", cancellationToken);
        yield return () => commodoreReader.ReadAsync(path, null, cancellationToken);
        yield return () => amstradReader.ReadAsync(path, "amstrad.cpc", cancellationToken);
        yield return () => amstradReader.ReadAsync(path, "amstrad.pcw", cancellationToken);
        yield return () => ibmReader.ReadAsync(path, "ibm.scan", cancellationToken);
        foreach (var formatId in EpsonFormats)
            yield return () => epsonReader.ReadAsync(path, formatId, cancellationToken);
        yield return () => appleReader.ReadAsync(path, null, cancellationToken);
    }

    private IEnumerable<Func<Task<SectorImage>>> AllCandidates(
        string path,
        IReadOnlySet<ScpFamily> families,
        CancellationToken cancellationToken)
    {
        var exhaustive = families.Count == 0;
        if (exhaustive || families.Contains(ScpFamily.Iso))
        {
            yield return () => isoReader.ReadAsync(path, null, cancellationToken);
            yield return () => isoReader.ReadAsync(path, "acorn.adfs.800", cancellationToken);
            yield return () => amstradReader.ReadAsync(path, "amstrad.cpc", cancellationToken);
            yield return () => amstradReader.ReadAsync(path, "amstrad.pcw", cancellationToken);
            yield return () => ucsdReader.ReadAsync(path, cancellationToken);
            yield return () => commodoreReader.ReadAsync(path, "commodore.1581", cancellationToken);
            foreach (var formatId in EpsonFormats)
                yield return () => epsonReader.ReadAsync(path, formatId, cancellationToken);
        }
        if (exhaustive || families.Contains(ScpFamily.Amiga))
            yield return () => amigaReader.ReadAsync(path, cancellationToken);
        if (exhaustive || families.Contains(ScpFamily.Commodore))
            yield return () => commodoreReader.ReadAsync(path, null, cancellationToken);
        if (exhaustive || families.Contains(ScpFamily.Apple))
            yield return () => appleReader.ReadAsync(path, null, cancellationToken);
        if (exhaustive || families.Contains(ScpFamily.Dec))
            yield return () => decReader.ReadAsync(path, cancellationToken);
    }

    private async Task<IReadOnlySet<ScpFamily>> ProbeFamiliesAsync(string path, CancellationToken cancellationToken)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (scp.Tracks.Count == 0) return new HashSet<ScpFamily>();
        var sampleCount = Math.Min(6, scp.Tracks.Count);
        var samples = Enumerable.Range(0, sampleCount)
            .Select(index => scp.Tracks[index * (scp.Tracks.Count - 1) / Math.Max(1, sampleCount - 1)])
            .DistinctBy(track => track.TrackNumber)
            .Where(track => track.Revolutions.Count > 0)
            .ToArray();
        var found = new ConcurrentDictionary<ScpFamily, byte>();
        await Task.WhenAll(samples.Select(track => Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var revolution = track.Revolutions[0];
            Probe(ScpFamily.Iso, "iso.mfm", revolution);
            Probe(ScpFamily.Iso, "iso.fm", revolution);
            Probe(ScpFamily.Amiga, "amiga.mfm", revolution);
            Probe(ScpFamily.Commodore, "commodore.gcr", revolution);
            Probe(ScpFamily.Apple, "apple2.gcr", revolution);
            Probe(ScpFamily.Apple, "apple2.rwts18", revolution);
            Probe(ScpFamily.Apple, "applemac.gcr", revolution);
            Probe(ScpFamily.Dec, "dec.rx02", revolution);
        }, cancellationToken))).ConfigureAwait(false);
        return found.Keys.ToHashSet();

        void Probe(ScpFamily family, string decoderId, ScpRevolution revolution)
        {
            if (found.ContainsKey(family)) return;
            var result = decoders.Decode(decoderId, revolution);
            if ((result.Sectors ?? []).Any(sector => sector.Data is not null && sector.IntegrityValid == true))
                found.TryAdd(family, 0);
        }
    }

    private static readonly string[] EpsonFormats =
        ["epson.qx10.396", "epson.qx10.399", "epson.qx10.320", "epson.qx10.400", "epson.qx10.logo"];

    private enum ScpFamily { Iso, Amiga, Commodore, Apple, Dec }
}
