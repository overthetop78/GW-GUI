using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Epson;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Reconstruction.Amiga;
using GWGUI.MediaEngine.Reconstruction.Apple;
using GWGUI.MediaEngine.Reconstruction.Atari;
using GWGUI.MediaEngine.Reconstruction.Commodore;
using GWGUI.MediaEngine.Reconstruction.Dec;
using GWGUI.MediaEngine.Reconstruction.Iso;

namespace GWGUI.MediaEngine.Images.ScpDetection;

/// <summary>Associe les familles et identifiants de formats aux reconstructeurs SCP.</summary>
internal sealed class ScpCandidateRegistry
{
    /// <summary>Reconstructeur différé d'une image sectorielle candidate.</summary>
    private delegate Task<SectorImage> Candidate(string path, string? formatId, CancellationToken cancellationToken);

    /// <summary>Reconstructeur ISO utilisé comme repli pour une sélection explicite.</summary>
    private readonly IsoScpSectorImageReader isoReader;
    /// <summary>Routage des identifiants explicitement demandés.</summary>
    private readonly IReadOnlyList<(Predicate<string> Matches, Candidate Read)> selectedReaders;
    /// <summary>Reconstructeurs essayés lorsqu'aucune famille n'est connue.</summary>
    private readonly IReadOnlyList<Candidate> defaultReaders;
    /// <summary>Reconstructeurs regroupés par famille détectable.</summary>
    private readonly IReadOnlyDictionary<ScpFormatFamily, IReadOnlyList<Candidate>> familyReaders;

    /// <summary>Initialise le registre avec les reconstructeurs spécialisés.</summary>
    public ScpCandidateRegistry(
        AmigaScpSectorImageReader amigaReader,
        IsoScpSectorImageReader isoReader,
        AtariScpSectorImageReader atariReader,
        CommodoreScpSectorImageReader commodoreReader,
        AppleScpSectorImageReader appleReader,
        DecRx02ScpSectorImageReader decReader)
    {
        this.isoReader = isoReader;
        selectedReaders =
        [
            (id => id.StartsWith(DiskImageFormatIds.AmigaPrefix, StringComparison.OrdinalIgnoreCase),
                (path, _, token) => amigaReader.ReadAsync(path, token)),
            (id => id.StartsWith(DiskImageFormatIds.CommodorePrefix, StringComparison.OrdinalIgnoreCase),
                (path, id, token) => commodoreReader.ReadAsync(path, id, token)),
            (id => id.StartsWith(DiskImageFormatIds.AmstradPrefix, StringComparison.OrdinalIgnoreCase),
                (path, id, token) => isoReader.ReadAsync(path, id, token)),
            (id => id.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) || id.Equals(DiskImageFormatIds.Mac1440, StringComparison.OrdinalIgnoreCase),
                (path, id, token) => isoReader.ReadAsync(path, id, token)),
            (id => id.StartsWith(DiskImageFormatIds.AcornDfsPrefix, StringComparison.OrdinalIgnoreCase),
                (path, id, token) => isoReader.ReadAsync(path, id, token)),
            (id => id.StartsWith(DiskImageFormatIds.AcornAdfsPrefix, StringComparison.OrdinalIgnoreCase),
                (path, id, token) => isoReader.ReadAsync(path, id, token)),
            (id => id.Equals(DiskImageFormatIds.DecRx02, StringComparison.OrdinalIgnoreCase),
                (path, _, token) => decReader.ReadAsync(path, token)),
            (id => id.StartsWith(DiskImageFormatIds.EpsonQx10Prefix, StringComparison.OrdinalIgnoreCase),
                (path, id, token) => isoReader.ReadAsync(path, id, token)),
            (id => id.Equals(DiskImageFormatIds.UcsdIbmMfm, StringComparison.OrdinalIgnoreCase),
                (path, id, token) => isoReader.ReadAsync(path, id, token)),
            (id => id.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase) || id.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase),
                (path, id, token) => atariReader.ReadAsync(path, id, token)),
            (IsAppleFormat,
                (path, id, token) => appleReader.ReadAsync(path, id, token))
        ];

        var isoCandidates = new List<Candidate>
        {
            (path, _, token) => isoReader.ReadAsync(path, null, token),
            (path, _, token) => isoReader.ReadAsync(path, DiskImageFormatIds.AcornAdfs800, token),
            (path, _, token) => isoReader.ReadAsync(path, DiskImageFormatIds.AmstradCpc, token),
            (path, _, token) => isoReader.ReadAsync(path, DiskImageFormatIds.AmstradPcw, token),
            (path, _, token) => isoReader.ReadAsync(path, DiskImageFormatIds.IbmScan, token),
            (path, _, token) => isoReader.ReadAsync(path, DiskImageFormatIds.UcsdIbmMfm, token),
            (path, _, token) => commodoreReader.ReadAsync(path, DiskImageFormatIds.Commodore1581, token)
        };
        isoCandidates.AddRange(EpsonQx10GeometryCatalog.ScpCandidateFormatIds.Select<string, Candidate>(formatId =>
            (path, _, token) => isoReader.ReadAsync(path, formatId, token)));

        defaultReaders =
        [
            (path, _, token) => isoReader.ReadAsync(path, null, token),
            (path, _, token) => amigaReader.ReadAsync(path, token),
            (path, _, token) => commodoreReader.ReadAsync(path, DiskImageFormatIds.Commodore1581, token),
            (path, _, token) => commodoreReader.ReadAsync(path, null, token),
            (path, _, token) => isoReader.ReadAsync(path, DiskImageFormatIds.AmstradCpc, token),
            (path, _, token) => isoReader.ReadAsync(path, DiskImageFormatIds.AmstradPcw, token),
            (path, _, token) => isoReader.ReadAsync(path, DiskImageFormatIds.IbmScan, token),
            .. EpsonQx10GeometryCatalog.ScpCandidateFormatIds.Select<string, Candidate>(formatId =>
                (path, _, token) => isoReader.ReadAsync(path, formatId, token)),
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

    /// <summary>Retourne le reconstructeur correspondant à une sélection explicite.</summary>
    public Func<Task<SectorImage>>? Selected(string path, string? formatId, CancellationToken cancellationToken)
    {
        if (formatId is null) return null;
        var registration = selectedReaders.FirstOrDefault(item => item.Matches(formatId));
        var reader = registration.Read ?? ((string candidatePath, string? candidateFormat, CancellationToken token) =>
            isoReader.ReadAsync(candidatePath, candidateFormat, token));
        return () => reader(path, formatId, cancellationToken);
    }

    /// <summary>Énumère les reconstructeurs essayés sans indication de famille.</summary>
    public IEnumerable<Func<Task<SectorImage>>> Default(string path, CancellationToken cancellationToken) =>
        defaultReaders.Select(reader => (Func<Task<SectorImage>>)(() => reader(path, null, cancellationToken)));

    /// <summary>Énumère les reconstructeurs correspondant aux familles détectées.</summary>
    public IEnumerable<Func<Task<SectorImage>>> Automatic(string path, IReadOnlySet<ScpFormatFamily> families, CancellationToken cancellationToken)
    {
        var selectedFamilies = families.Count == 0 ? familyReaders.Keys : families;
        return selectedFamilies.SelectMany(family => familyReaders[family])
            .Select(reader => (Func<Task<SectorImage>>)(() => reader(path, null, cancellationToken)));
    }

    /// <summary>Indique si un identifiant appartient à une famille de formats Apple prise en charge.</summary>
    private static bool IsAppleFormat(string formatId) => formatId.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.MacPrefix, StringComparison.OrdinalIgnoreCase);
}
