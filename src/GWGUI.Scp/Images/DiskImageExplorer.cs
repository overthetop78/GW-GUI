using GWGUI.Scp.Decoding;
using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed record ExploredFileSystem(string FormatId, string ReaderId, FileSystemVolume Volume);
public sealed record ExploredDiskImage(
    string SourcePath,
    SectorImage Image,
    FileSystemVolume Volume,
    bool FileSystemRecognized = true,
    IReadOnlyList<ExploredFileSystem>? DetectedFileSystems = null,
    IReadOnlyList<string>? DetectedImageFormatIds = null)
{
    public DiskImageMetadata Metadata
    {
        get
        {
            var recognized = DetectedFileSystems?.Select(item => item.FormatId).ToArray() ?? [];
            return DiskImageMetadata.From(Image, recognized.Length > 0 ? recognized : [Image.FormatId]);
        }
    }
}

public sealed class DiskImageExplorer(
    AdfImageReader adfReader,
    AtariStImageReader stReader,
    MsaImageReader msaReader,
    AtrImageReader atrReader,
    CommodoreD64ImageReader d64Reader,
    CommodoreD71ImageReader d71Reader,
    CommodoreD81ImageReader d81Reader,
    AmstradDskImageReader amstradDskReader,
    MsxImageReader msxReader,
    IbmPcImageReader ibmPcReader,
    AppleDiskImageReader appleReader,
    BbcDfsImageReader bbcReader,
    CoherentImageReader coherentReader,
    DecRx02ImageReader decRx02Reader,
    Td0ImageReader td0Reader,
    I86fImageReader i86fReader,
    Cp2ImageReader cp2Reader,
    ImdImageReader imdReader,
    AmigaScpSectorImageReader amigaScpReader,
    AtariScpSectorImageReader atariScpReader,
    CommodoreScpSectorImageReader commodoreScpReader,
    AppleScpSectorImageReader appleScpReader,
    DecRx02ScpSectorImageReader decRx02ScpReader,
    FileSystemRegistry fileSystems,
    IScpReader scpReader,
    FluxDecoderRegistry decoders)
{
    public IReadOnlySet<string> SupportedFormatIds => fileSystems.SupportedFormatIds;

    public static DiskImageExplorer CreateDefault()
    {
        var scp = new ScpReader(); var decoders = new FluxDecoderRegistry();
        return new(new AdfImageReader(), new AtariStImageReader(), new MsaImageReader(), new AtrImageReader(),
            new CommodoreD64ImageReader(), new CommodoreD71ImageReader(), new CommodoreD81ImageReader(),
            new AmstradDskImageReader(), new MsxImageReader(), new IbmPcImageReader(), new AppleDiskImageReader(), new BbcDfsImageReader(),
            new CoherentImageReader(), new DecRx02ImageReader(), new Td0ImageReader(), new I86fImageReader(decoders), new Cp2ImageReader(), new ImdImageReader(), new AmigaScpSectorImageReader(scp, decoders), new AtariScpSectorImageReader(scp, decoders),
            new CommodoreScpSectorImageReader(scp, decoders), new AppleScpSectorImageReader(scp, decoders),
            new DecRx02ScpSectorImageReader(scp, decoders), new FileSystemRegistry(), scp, decoders);
    }

    public async Task<ExploredDiskImage> ExploreAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("The disk image does not exist.", path);
        var extension = Path.GetExtension(path);
        if (extension.Equals(".scp", StringComparison.OrdinalIgnoreCase) && formatId is null)
            return await ExploreScpAutomaticallyAsync(path, cancellationToken).ConfigureAwait(false);
        SectorImage image;
        try
        {
        if (extension.Equals(".adf", StringComparison.OrdinalIgnoreCase)) image = await adfReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".ssd", StringComparison.OrdinalIgnoreCase) || extension.Equals(".dsd", StringComparison.OrdinalIgnoreCase)) image = await bbcReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".bin", StringComparison.OrdinalIgnoreCase) && CoherentImageReader.LooksLikeCoherent(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false))) image = await coherentReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".img", StringComparison.OrdinalIgnoreCase) &&
                 (formatId?.Equals("dec.rx02", StringComparison.OrdinalIgnoreCase) == true ||
                  DecRx02ImageReader.LooksLikeRt11(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false))))
            image = await decRx02Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".st", StringComparison.OrdinalIgnoreCase)) image = await stReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".msa", StringComparison.OrdinalIgnoreCase)) image = await msaReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".atr", StringComparison.OrdinalIgnoreCase)) image = await atrReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".d64", StringComparison.OrdinalIgnoreCase)) image = await d64Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".d71", StringComparison.OrdinalIgnoreCase)) image = await d71Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".d81", StringComparison.OrdinalIgnoreCase)) image = await d81Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase)
            && (formatId?.StartsWith("apple", StringComparison.OrdinalIgnoreCase) == true || AppleDiskImageReader.LooksLikeAppleImage(path)))
            image = await appleReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".do", StringComparison.OrdinalIgnoreCase) || extension.Equals(".po", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".2mg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".image", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".d13", StringComparison.OrdinalIgnoreCase) || extension.Equals(".dc42", StringComparison.OrdinalIgnoreCase) || extension.Equals(".nib", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".woz", StringComparison.OrdinalIgnoreCase)) image = await appleReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase)
            && (formatId?.StartsWith("msx.", StringComparison.OrdinalIgnoreCase) == true
                || MsxImageReader.LooksLikeMsx(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false))))
            image = await msxReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase) || extension.Equals(".edsk", StringComparison.OrdinalIgnoreCase)) image = await amstradDskReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".img", StringComparison.OrdinalIgnoreCase) &&
                 (formatId?.StartsWith("mac.", StringComparison.OrdinalIgnoreCase) == true || AppleDiskImageReader.LooksLikeAppleImage(path)))
            image = await appleReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".img", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
            var hasFatBpb = IbmPcImageReader.HasValidBpbGeometry(bytes);
            if (!hasFatBpb && FileSystems.Readers.AmstradCpmFileSystemReader.LooksLikeCpcRawImage(bytes))
                image = Retag(IbmPcImageReader.Create(bytes, cancellationToken), "amstrad.cpc");
            else if (!hasFatBpb && FileSystems.Readers.AmstradCpmFileSystemReader.LooksLikePcwDiskSpecification(bytes))
                image = Retag(IbmPcImageReader.Create(bytes, cancellationToken), "amstrad.pcw");
            else image = IbmPcImageReader.Create(bytes, cancellationToken);
        }
        else if (extension.Equals(".ima", StringComparison.OrdinalIgnoreCase)) image = await ibmPcReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".td0", StringComparison.OrdinalIgnoreCase)) image = await td0Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".86f", StringComparison.OrdinalIgnoreCase)) image = await i86fReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".cp2", StringComparison.OrdinalIgnoreCase)) image = await cp2Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".imd", StringComparison.OrdinalIgnoreCase)) image = await imdReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".scp", StringComparison.OrdinalIgnoreCase))
        {
            if (formatId is not null && !SupportedFormatIds.Contains(formatId)) throw new NotSupportedException($"The selected format '{formatId}' is not supported by the explorer yet.");
            image = await ReadScpAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        }
        else throw new NotSupportedException($"The image extension '{extension}' is not supported by the explorer yet.");
        }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
        {
            return Unknown(path);
        }

        var detected = new List<ExploredFileSystem>();
        if (formatId is null)
        {
            foreach (var match in fileSystems.ReadAll(image)) detected.Add(new(image.FormatId, match.ReaderId, match.Volume));
            if (detected.Count == 0)
            {
                foreach (var interpretation in AdditionalFileSystemInterpretations(image))
                {
                    if (!fileSystems.TryRead(interpretation, interpretation.FormatId, out var volume)) continue;
                    image = interpretation;
                    detected.Add(new(interpretation.FormatId, interpretation.FormatId, volume));
                    break;
                }
            }
        }
        else
        {
            var selectedImage = image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase) ? image : Retag(image, formatId);
            if (fileSystems.TryRead(selectedImage, formatId, out var selectedVolume) || fileSystems.TryRead(selectedImage, null, out selectedVolume))
                detected.Add(new(formatId, formatId, selectedVolume));
        }
        var unique = detected.GroupBy(match => $"{match.FormatId}\0{match.ReaderId}\0{match.Volume.Name}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First()).ToArray();
        return CreateDocument(path, image, unique, [image.FormatId]);
    }

    private async Task<ExploredDiskImage> ExploreScpAutomaticallyAsync(string path, CancellationToken cancellationToken)
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
                        var recognizedImage = NormalizeRecognizedImage(candidate, match.ReaderId, match.Volume);
                        var recognizedVolume = ReferenceEquals(recognizedImage, candidate) ||
                            !fileSystems.TryRead(recognizedImage, match.ReaderId, out var normalizedVolume)
                            ? match.Volume : normalizedVolume;
                        matches.Add((new(recognizedImage.FormatId, match.ReaderId, recognizedVolume), recognizedImage));
                    }
                    foreach (var interpretation in AdditionalFileSystemInterpretations(candidate))
                        if (fileSystems.TryRead(interpretation, interpretation.FormatId, out var volume))
                            matches.Add((new(interpretation.FormatId, interpretation.FormatId, volume), interpretation));
                    return (Image: candidate, Matches: (IReadOnlyList<(ExploredFileSystem Match, SectorImage Image)>)matches);
                }
                catch (InvalidDataException) { return (Image: (SectorImage?)null, Matches: (IReadOnlyList<(ExploredFileSystem Match, SectorImage Image)>)[]); }
            })).ConfigureAwait(false);
            foreach (var inspection in inspections)
            {
                if (inspection.Image is null) continue;
                if (bestDecoded is null || DecodeScore(inspection.Image) > DecodeScore(bestDecoded)) bestDecoded = inspection.Image;
                if (DecodeScore(inspection.Image) >= .5) decodedFormatIds.Add(inspection.Image.FormatId);
                foreach (var recognized in inspection.Matches)
                {
                    var match = recognized.Match;
                    var recognizedScore = DecodeScore(recognized.Image);
                    if (bestRecognized is null || recognizedScore > bestRecognizedScore)
                    {
                        bestRecognized = recognized.Image;
                        bestRecognizedFileSystem = match;
                        bestRecognizedScore = recognizedScore;
                    }
                    var key = FileSystemIdentity(match.Volume);
                    if (!keys.Add(key)) continue;
                    detected.Add(match);
                }
            }
        }

        var families = await ProbeScpFamiliesAsync(path, cancellationToken).ConfigureAwait(false);
        await InspectAsync(AllScpCandidates(path, families, cancellationToken)).ConfigureAwait(false);
        if (bestDecoded is null) return Unknown(path);
        if (bestRecognizedFileSystem is null)
            return CreateDocument(path, bestRecognized ?? bestDecoded, detected, decodedFormatIds.ToArray());
        var primaryIdentity = FileSystemIdentity(bestRecognizedFileSystem.Volume);
        var orderedDetected = new[] { bestRecognizedFileSystem }.Concat(
            detected.Where(match => FileSystemIdentity(match.Volume) != primaryIdentity && IsCredibleAlternative(match.Volume))).ToArray();
        return CreateDocument(path, bestRecognized ?? bestDecoded, orderedDetected, decodedFormatIds.ToArray());
    }

    private static bool IsCredibleAlternative(FileSystemVolume volume) =>
        volume.Warnings.Count <= Math.Max(3, volume.Entries.Count);

    private static double DecodeScore(SectorImage image) =>
        image.AvailableBlocks.Count / (double)Math.Max(1, image.BlockCount);

    private static SectorImage NormalizeRecognizedImage(SectorImage image, string readerId, FileSystemVolume volume)
    {
        if ((readerId.Equals("mac-hfs", StringComparison.OrdinalIgnoreCase) ||
             readerId.Equals("mac-mfs", StringComparison.OrdinalIgnoreCase)) &&
            image.BlockSize == 512 && image.BlockCount == 2880 &&
            !image.FormatId.Equals("mac.1440", StringComparison.OrdinalIgnoreCase))
            return Retag(image, "mac.1440");
        if (readerId.Equals("fat12", StringComparison.OrdinalIgnoreCase) &&
            TryCreateMsxInterpretation(image, out var msxInterpretation))
            return msxInterpretation;
        if (readerId.Equals("fat12", StringComparison.OrdinalIgnoreCase) &&
            image.FormatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase) &&
            TryReadFatGeometry(image, out var cylinders, out var heads, out var sectorsPerTrack, out var totalSectors) &&
            totalSectors < image.BlockCount)
        {
            var blocks = image.AvailableBlocks.Where(block => block.LogicalBlock < totalSectors).ToArray();
            return new($"atarist.{totalSectors / 2}", 512, cylinders, heads, sectorsPerTrack, blocks,
                capacity: totalSectors * 512L, logicalBlockCount: totalSectors);
        }
        if (readerId.Equals("fat12", StringComparison.OrdinalIgnoreCase) &&
            image.FormatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) &&
            ContainsAtariStProgram(volume.Entries))
            return Retag(image, $"atarist.{image.Capacity / 1024}");
        return image;
    }

    private static bool ContainsAtariStProgram(IEnumerable<FileSystemEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Kind == FileSystemEntryKind.File)
            {
                var extension = Path.GetExtension(entry.Name);
                if (extension.Equals(".ttp", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".tos", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".acc", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".gtp", StringComparison.OrdinalIgnoreCase) ||
                    entry.Content is { Count: >= 2 } && entry.Content[0] == 0x60 && entry.Content[1] == 0x1a)
                    return true;
            }
            if (ContainsAtariStProgram(entry.Children)) return true;
        }
        return false;
    }

    private static bool TryReadFatGeometry(SectorImage image, out int cylinders, out int heads,
        out int sectorsPerTrack, out int totalSectors)
    {
        cylinders = heads = sectorsPerTrack = totalSectors = 0;
        if (image.BlockSize != 512 || !image.TryGetBlock(0, out var boot) || boot.Data.Count < 36) return false;
        var bytes = boot.Data;
        var bytesPerSector = bytes[11] | bytes[12] << 8;
        totalSectors = bytes[19] | bytes[20] << 8;
        if (totalSectors == 0)
            totalSectors = bytes[32] | bytes[33] << 8 | bytes[34] << 16 | bytes[35] << 24;
        sectorsPerTrack = bytes[24] | bytes[25] << 8;
        heads = bytes[26] | bytes[27] << 8;
        if (bytesPerSector != 512 || totalSectors <= 0 || sectorsPerTrack <= 0 || heads <= 0 ||
            totalSectors > image.BlockCount || totalSectors % (sectorsPerTrack * heads) != 0)
            return false;
        cylinders = totalSectors / (sectorsPerTrack * heads);
        return cylinders > 0;
    }

    private static string FileSystemIdentity(FileSystemVolume volume)
    {
        static IEnumerable<string> Entries(IEnumerable<FileSystemEntry> entries, string prefix = "")
        {
            foreach (var entry in entries.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase))
            {
                var path = prefix + entry.Name;
                yield return $"{path}\0{entry.Kind}\0{entry.Size}";
                foreach (var child in Entries(entry.Children, path + "/")) yield return child;
            }
        }
        return $"{volume.Name}\0{string.Join('\u001f', Entries(volume.Entries))}";
    }

    private IEnumerable<Func<Task<SectorImage>>> AllScpCandidates(string path, IReadOnlySet<ScpFamily> families, CancellationToken cancellationToken)
    {
        var exhaustive = families.Count == 0;
        if (exhaustive || families.Contains(ScpFamily.Iso))
        {
            yield return () => atariScpReader.ReadAsync(path, null, cancellationToken);
            yield return () => atariScpReader.ReadAsync(path, "acorn.adfs.800", cancellationToken);
            yield return () => atariScpReader.ReadAsync(path, "amstrad.cpc", cancellationToken);
            yield return () => atariScpReader.ReadAsync(path, "amstrad.pcw", cancellationToken);
            yield return () => atariScpReader.ReadAsync(path, "ucsd.ibm.mfm", cancellationToken);
            yield return () => commodoreScpReader.ReadAsync(path, "commodore.1581", cancellationToken);
            yield return () => atariScpReader.ReadAsync(path, "epson.qx10.396", cancellationToken);
            yield return () => atariScpReader.ReadAsync(path, "epson.qx10.399", cancellationToken);
            yield return () => atariScpReader.ReadAsync(path, "epson.qx10.320", cancellationToken);
            yield return () => atariScpReader.ReadAsync(path, "epson.qx10.400", cancellationToken);
            yield return () => atariScpReader.ReadAsync(path, "epson.qx10.logo", cancellationToken);
        }
        if (exhaustive || families.Contains(ScpFamily.Amiga)) yield return () => amigaScpReader.ReadAsync(path, cancellationToken);
        if (exhaustive || families.Contains(ScpFamily.Commodore)) yield return () => commodoreScpReader.ReadAsync(path, null, cancellationToken);
        if (exhaustive || families.Contains(ScpFamily.Apple)) yield return () => appleScpReader.ReadAsync(path, null, cancellationToken);
        if (exhaustive || families.Contains(ScpFamily.Dec)) yield return () => decRx02ScpReader.ReadAsync(path, cancellationToken);
    }

    private async Task<IReadOnlySet<ScpFamily>> ProbeScpFamiliesAsync(string path, CancellationToken cancellationToken)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (scp.Tracks.Count == 0) return new HashSet<ScpFamily>();
        var sampleCount = Math.Min(6, scp.Tracks.Count);
        var samples = Enumerable.Range(0, sampleCount)
            .Select(index => scp.Tracks[index * (scp.Tracks.Count - 1) / Math.Max(1, sampleCount - 1)])
            .DistinctBy(track => track.TrackNumber)
            .Where(track => track.Revolutions.Count > 0)
            .ToArray();
        var found = new System.Collections.Concurrent.ConcurrentDictionary<ScpFamily, byte>();
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

    private enum ScpFamily { Iso, Amiga, Commodore, Apple, Dec }

    private IEnumerable<SectorImage> AdditionalFileSystemInterpretations(SectorImage image)
    {
        var iso = image.FormatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase)
            || image.FormatId.StartsWith("atari.", StringComparison.OrdinalIgnoreCase)
            || image.FormatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase)
            || image.FormatId.StartsWith("acorn.dfs.", StringComparison.OrdinalIgnoreCase)
            || image.FormatId.Equals("imd", StringComparison.OrdinalIgnoreCase);
        if (!iso) yield break;
        if (image.BlockSize == 512)
        {
            if (TryCreateIbmInterpretation(image, out var ibm)) yield return ibm;
            if (TryCreateMsxInterpretation(image, out var msx)) yield return msx;
            foreach (var id in new[] { "ucsd.ibm.mfm", "commodore900.coherent",
                         "epson.qx10.396", "epson.qx10.399", "epson.qx10.logo" })
                if (!id.Equals(image.FormatId, StringComparison.OrdinalIgnoreCase)) yield return Retag(image, id);
        }
        else if (image.BlockSize == 256)
        {
            foreach (var id in new[] { "acorn.dfs.ss", "acorn.dfs.ss80", "acorn.dfs.ds", "acorn.dfs.ds80", "epson.qx10.320" })
                if (!id.Equals(image.FormatId, StringComparison.OrdinalIgnoreCase)) yield return Retag(image, id);
        }
        else if (image.BlockSize == 1024 && !image.FormatId.Equals("epson.qx10.400", StringComparison.OrdinalIgnoreCase))
            yield return Retag(image, "epson.qx10.400");
    }

    private bool TryCreateIbmInterpretation(SectorImage image, out SectorImage interpretation)
    {
        interpretation = null!;
        if (image.FormatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) ||
            !image.TryGetBlock(0, out var boot) || boot.Data.Count != 512)
            return false;
        var fatMedia = image.TryGetBlock(1, out var fat) && fat.Data.Count > 0 ? fat.Data[0] : (byte)0;
        if (!IbmPcImageReader.TryDetectFluxGeometry(boot.Data.ToArray(), fatMedia, out var geometry)) return false;
        var formatId = geometry.FormatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) &&
            fileSystems.SupportedFormatIds.Contains(geometry.FormatId)
            ? geometry.FormatId : "ibm.scan";
        interpretation = Retag(image, formatId);
        return true;
    }

    private static bool TryCreateMsxInterpretation(SectorImage image, out SectorImage interpretation)
    {
        interpretation = null!;
        if (image.FormatId.StartsWith("msx.", StringComparison.OrdinalIgnoreCase) ||
            !image.TryGetBlock(0, out var boot) || boot.Data.Count != 512 ||
            !MsxImageReader.LooksLikeMsx(boot.Data.ToArray()))
            return false;
        var formatId = image.BlockCount switch
        {
            360 => "msx.1d",
            720 when boot.Data.Count > 21 && boot.Data[21] == 0xf8 => "msx.1dd",
            720 => "msx.2d",
            1440 => "msx.2dd",
            _ => string.Empty
        };
        if (formatId.Length == 0) return false;
        interpretation = Retag(image, formatId);
        return true;
    }

    private static string FormatFamily(string formatId)
    {
        var separator = formatId.IndexOf('.');
        return separator < 0 ? formatId : formatId[..separator];
    }

    private static SectorImage Retag(SectorImage image, string formatId) => new(formatId, image.BlockSize,
        image.Cylinders, image.Heads, image.SectorsPerTrack, image.AvailableBlocks,
        image.AvailableBlocks.Any(block => block.Data.Count != image.BlockSize), image.Capacity, image.BlockCount);

    private static ExploredDiskImage CreateDocument(string path, SectorImage image, IReadOnlyList<ExploredFileSystem> detected,
        IReadOnlyList<string>? detectedImageFormatIds = null)
    {
        if (detected.Count > 0) return new(path, image, detected[0].Volume, true, detected, detectedImageFormatIds);
        var physicalTracks = image.AvailableBlocks
            .GroupBy(block => (block.Address.Cylinder, block.Address.Head))
            .OrderBy(group => group.Key.Cylinder).ThenBy(group => group.Key.Head)
            .Select(group => new FileSystemEntry($"T{group.Key.Cylinder:D2} H{group.Key.Head}", FileSystemEntryKind.Directory,
                group.Sum(block => (long)block.Data.Count), null, string.Empty, 0, 0,
                group.All(block => block.IntegrityValid != false),
                group.OrderBy(block => block.Address.Number).Select(block => new FileSystemEntry(
                    $"S{block.Address.Number:D2}.bin", FileSystemEntryKind.File, block.Data.Count, null,
                    string.Empty, 0, block.LogicalBlock, block.IntegrityValid != false, [], block.Data.ToArray())).ToArray()))
            .ToArray();
        var physical = new FileSystemVolume(Path.GetFileNameWithoutExtension(path), image.FormatId,
            image.Capacity, 0, null, null, physicalTracks, []);
        return new(path, image, physical, false, [], detectedImageFormatIds);
    }

    private static ExploredDiskImage Unknown(string path)
    {
        var capacity = new FileInfo(path).Length;
        var image = new SectorImage("unknown", 1, 1, 1, 1, [], capacity: capacity, logicalBlockCount: 1);
        return CreateDocument(path, image, []);
    }

    private async Task<SectorImage> ReadScpAsync(string path, string? formatId, CancellationToken cancellationToken)
    {
        if (formatId?.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase) == true)
            return await amigaScpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (formatId?.StartsWith("commodore.", StringComparison.OrdinalIgnoreCase) == true)
            return await commodoreScpReader.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        if (formatId?.StartsWith("amstrad.", StringComparison.OrdinalIgnoreCase) == true)
            return await atariScpReader.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        if (formatId?.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) == true)
            return await atariScpReader.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        if (formatId?.StartsWith("acorn.dfs.", StringComparison.OrdinalIgnoreCase) == true)
            return await atariScpReader.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        if (formatId?.Equals("dec.rx02", StringComparison.OrdinalIgnoreCase) == true)
            return await decRx02ScpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (formatId?.Equals("mac.1440", StringComparison.OrdinalIgnoreCase) == true)
            return await atariScpReader.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        if (formatId?.StartsWith("apple", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith("mac.", StringComparison.OrdinalIgnoreCase) == true)
            return await appleScpReader.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        if (formatId is not null)
            return await atariScpReader.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        SectorImage? firstDecoded = null;
        foreach (var read in new Func<Task<SectorImage>>[]
        {
            () => atariScpReader.ReadAsync(path, null, cancellationToken),
            () => amigaScpReader.ReadAsync(path, cancellationToken),
            () => commodoreScpReader.ReadAsync(path, "commodore.1581", cancellationToken),
            () => commodoreScpReader.ReadAsync(path, null, cancellationToken),
            () => atariScpReader.ReadAsync(path, "amstrad.cpc", cancellationToken),
            () => atariScpReader.ReadAsync(path, "amstrad.pcw", cancellationToken),
            () => atariScpReader.ReadAsync(path, "ibm.scan", cancellationToken),
            () => atariScpReader.ReadAsync(path, "epson.qx10.396", cancellationToken),
            () => atariScpReader.ReadAsync(path, "epson.qx10.399", cancellationToken),
            () => atariScpReader.ReadAsync(path, "epson.qx10.320", cancellationToken),
            () => atariScpReader.ReadAsync(path, "epson.qx10.400", cancellationToken),
            () => atariScpReader.ReadAsync(path, "epson.qx10.logo", cancellationToken),
            () => appleScpReader.ReadAsync(path, null, cancellationToken)
        })
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
