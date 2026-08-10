using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.ScpDetection;

internal sealed class ScpCandidateRegistry
{
    private delegate Task<SectorImage> Candidate(string path, string? formatId, CancellationToken cancellationToken);

    private readonly IsoScpSectorImageReader isoReader;
    private readonly IReadOnlyList<(Predicate<string> Matches, Candidate Read)> selectedReaders;
    private readonly IReadOnlyList<Candidate> defaultReaders;
    private readonly IReadOnlyDictionary<ScpFormatFamily, IReadOnlyList<Candidate>> familyReaders;

    public ScpCandidateRegistry(
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
        DecRx02ScpSectorImageReader decReader)
    {
        this.isoReader = isoReader;
        selectedReaders =
        [
            (id => id.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase),
                (path, _, token) => amigaReader.ReadAsync(path, token)),
            (id => id.StartsWith("commodore.", StringComparison.OrdinalIgnoreCase),
                (path, id, token) => commodoreReader.ReadAsync(path, id, token)),
            (id => id.StartsWith("amstrad.", StringComparison.OrdinalIgnoreCase),
                (path, id, token) => amstradReader.ReadAsync(path, id!, token)),
            (id => id.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) || id.Equals("mac.1440", StringComparison.OrdinalIgnoreCase),
                (path, id, token) => ibmReader.ReadAsync(path, id!, token)),
            (id => id.StartsWith("acorn.dfs.", StringComparison.OrdinalIgnoreCase),
                (path, id, token) => bbcReader.ReadAsync(path, id!, token)),
            (id => id.Equals("dec.rx02", StringComparison.OrdinalIgnoreCase),
                (path, _, token) => decReader.ReadAsync(path, token)),
            (id => id.StartsWith("epson.qx10.", StringComparison.OrdinalIgnoreCase),
                (path, id, token) => epsonReader.ReadAsync(path, id!, token)),
            (id => id.Equals("ucsd.ibm.mfm", StringComparison.OrdinalIgnoreCase),
                (path, _, token) => ucsdReader.ReadAsync(path, token)),
            (id => id.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) || id.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase),
                (path, id, token) => atariReader.ReadAsync(path, id, token)),
            (id => id.StartsWith("apple", StringComparison.OrdinalIgnoreCase) || id.StartsWith("mac.", StringComparison.OrdinalIgnoreCase),
                (path, id, token) => appleReader.ReadAsync(path, id, token))
        ];

        var isoCandidates = new List<Candidate>
        {
            (path, _, token) => isoReader.ReadAsync(path, null, token),
            (path, _, token) => isoReader.ReadAsync(path, "acorn.adfs.800", token),
            (path, _, token) => amstradReader.ReadAsync(path, "amstrad.cpc", token),
            (path, _, token) => amstradReader.ReadAsync(path, "amstrad.pcw", token),
            (path, _, token) => ibmReader.ReadAsync(path, "ibm.scan", token),
            (path, _, token) => ucsdReader.ReadAsync(path, token),
            (path, _, token) => commodoreReader.ReadAsync(path, "commodore.1581", token)
        };
        isoCandidates.AddRange(EpsonFormats.Select<string, Candidate>(formatId =>
            (path, _, token) => epsonReader.ReadAsync(path, formatId, token)));

        defaultReaders =
        [
            (path, _, token) => isoReader.ReadAsync(path, null, token),
            (path, _, token) => amigaReader.ReadAsync(path, token),
            (path, _, token) => commodoreReader.ReadAsync(path, "commodore.1581", token),
            (path, _, token) => commodoreReader.ReadAsync(path, null, token),
            (path, _, token) => amstradReader.ReadAsync(path, "amstrad.cpc", token),
            (path, _, token) => amstradReader.ReadAsync(path, "amstrad.pcw", token),
            (path, _, token) => ibmReader.ReadAsync(path, "ibm.scan", token),
            .. EpsonFormats.Select<string, Candidate>(formatId =>
                (path, _, token) => epsonReader.ReadAsync(path, formatId, token)),
            (path, _, token) => appleReader.ReadAsync(path, null, token)
        ];

        familyReaders = new Dictionary<ScpFormatFamily, IReadOnlyList<Candidate>>
        {
            [ScpFormatFamily.Iso] = isoCandidates,
            [ScpFormatFamily.Amiga] = [(path, _, token) => amigaReader.ReadAsync(path, token)],
            [ScpFormatFamily.Commodore] = [(path, _, token) => commodoreReader.ReadAsync(path, null, token)],
            [ScpFormatFamily.Apple] = [(path, _, token) => appleReader.ReadAsync(path, null, token)],
            [ScpFormatFamily.Dec] = [(path, _, token) => decReader.ReadAsync(path, token)]
        };
    }

    public Func<Task<SectorImage>>? Selected(
        string path,
        string? formatId,
        CancellationToken cancellationToken)
    {
        if (formatId is null) return null;
        var registration = selectedReaders.FirstOrDefault(item => item.Matches(formatId));
        var reader = registration.Read ?? ((string candidatePath, string? candidateFormat, CancellationToken token) =>
            isoReader.ReadAsync(candidatePath, candidateFormat, token));
        return () => reader(path, formatId, cancellationToken);
    }

    public IEnumerable<Func<Task<SectorImage>>> Default(string path, CancellationToken cancellationToken) =>
        defaultReaders.Select(reader => (Func<Task<SectorImage>>)(() => reader(path, null, cancellationToken)));

    public IEnumerable<Func<Task<SectorImage>>> Automatic(
        string path,
        IReadOnlySet<ScpFormatFamily> families,
        CancellationToken cancellationToken)
    {
        var selectedFamilies = families.Count == 0 ? familyReaders.Keys : families;
        return selectedFamilies.SelectMany(family => familyReaders[family])
            .Select(reader => (Func<Task<SectorImage>>)(() => reader(path, null, cancellationToken)));
    }

    private static readonly string[] EpsonFormats =
        ["epson.qx10.396", "epson.qx10.399", "epson.qx10.320", "epson.qx10.400", "epson.qx10.logo"];
}
