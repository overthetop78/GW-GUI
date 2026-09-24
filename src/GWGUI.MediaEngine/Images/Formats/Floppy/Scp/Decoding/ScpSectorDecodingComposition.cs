using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Images.Reading.Decoding;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding.Sectors;
using FileSystemRegistry = GWGUI.MediaFileSystems.Exploration.SectorFileSystemRegistry;
using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Recognition;
using GWGUI.MediaEngine.Images.Reading.Reconstruction;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Apple;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Atari;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Iso;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding;

/// <summary>Provides the shared SCP decoders, reconstruction candidates, and sector reader.</summary>
internal sealed class ScpSectorDecodingComposition
{
    public ScpSectorDecodingComposition(ScpReader scpReader, FileSystemRegistry fileSystems)
    {
        ArgumentNullException.ThrowIfNull(scpReader);
        ArgumentNullException.ThrowIfNull(fileSystems);
        Decoders = new FluxDecoderRegistry(FluxDecoderCatalog.CreateDefault());
        AmigaReader = new AmigaScpSectorImageReader(scpReader, Decoders);
        AtariReader = new AtariScpSectorImageReader(scpReader, Decoders);
        Candidates = CreateCandidates(scpReader, Decoders, AmigaReader, AtariReader);
        Reader = new ScpSectorImageReader(Candidates, fileSystems);
    }

    public FluxDecoderRegistry Decoders { get; }

    public AmigaScpSectorImageReader AmigaReader { get; }

    public AtariScpSectorImageReader AtariReader { get; }

    public ScpCandidateRegistry Candidates { get; }

    public ScpSectorImageReader Reader { get; }

    private static ScpCandidateRegistry CreateCandidates(
        ScpReader scpReader,
        FluxDecoderRegistry decoders,
        AmigaScpSectorImageReader amigaReader,
        AtariScpSectorImageReader atariReader)
    {
        var isoReader = new IsoScpSectorImageReader(scpReader, decoders);
        var commodoreReader = new CommodoreScpSectorImageReader(scpReader, decoders);
        var appleReader = new AppleScpSectorImageReader(scpReader, decoders);
        var decReader = new DecRx02ScpSectorImageReader(scpReader, decoders);
        var isoAutomatic = new ScpSectorImageCandidate(
            ScpCandidateIds.IsoAutomatic,
            ScpFormatFamily.Iso,
            (path, _, token) => isoReader.ReadAsync(path, null, token),
            (path, _, progress, token) => isoReader.ReadAsync(path, null, progress, token));
        var isoSelected = new ScpSectorImageCandidate(
            ScpCandidateIds.IsoSelected,
            ScpFormatFamily.Iso,
            (path, format, token) => isoReader.ReadAsync(path, format, token));
        var amiga = new ScpSectorImageCandidate(
            ScpCandidateIds.Amiga,
            ScpFormatFamily.Amiga,
            (path, _, token) => amigaReader.ReadAsync(path, token),
            (path, _, progress, token) => amigaReader.ReadAsync(path, progress, token));
        var atari = new ScpSectorImageCandidate(
            ScpCandidateIds.Atari,
            ScpFormatFamily.Iso,
            (path, format, token) => atariReader.ReadAsync(path, format, token));
        var atariSt720 = new ScpSectorImageCandidate(
            ScpCandidateIds.AtariSt720,
            ScpFormatFamily.Iso,
            (path, _, token) => atariReader.ReadAsync(path, DiskImageFormatIds.AtariSt720, token));
        var commodoreAutomatic = new ScpSectorImageCandidate(
            ScpCandidateIds.CommodoreAutomatic,
            ScpFormatFamily.Commodore,
            (path, _, token) => commodoreReader.ReadAsync(path, null, token));
        var commodore1581 = new ScpSectorImageCandidate(
            ScpCandidateIds.Commodore1581,
            ScpFormatFamily.Iso,
            (path, _, token) => commodoreReader.ReadAsync(path, DiskImageFormatIds.Commodore1581, token));
        var apple = new ScpSectorImageCandidate(
            ScpCandidateIds.Apple,
            ScpFormatFamily.Apple,
            (path, format, token) => appleReader.ReadAsync(path, format, token));
        var dec = new ScpSectorImageCandidate(
            ScpCandidateIds.Dec,
            ScpFormatFamily.Dec,
            (path, _, token) => decReader.ReadAsync(path, token));

        ScpSectorImageCandidate Iso(string format) => new(
            ScpCandidateIds.IsoFormat(format),
            ScpFormatFamily.Iso,
            (path, _, token) => isoReader.ReadAsync(path, format, token),
            (path, _, progress, token) => isoReader.ReadAsync(path, format, progress, token));

        var acornAdfs = Iso(DiskImageFormatIds.AcornAdfs800);
        var amstradCpc = Iso(DiskImageFormatIds.AmstradCpc);
        var amstradPcw = Iso(DiskImageFormatIds.AmstradPcw);
        var ibmScan = Iso(DiskImageFormatIds.IbmScan);
        var ucsd = Iso(DiskImageFormatIds.UcsdIbmMfm);
        var epson = EpsonQx10GeometryCatalog.ScpCandidateFormatIds.Select(Iso).ToArray();
        var isoFamily = new[]
            {
                isoAutomatic,
                atariSt720,
                acornAdfs,
                amstradCpc,
                amstradPcw,
                ibmScan,
                ucsd,
                commodore1581
            }
            .Concat(epson)
            .ToArray();
        var defaults = new[]
            {
                isoAutomatic,
                amiga,
                commodore1581,
                commodoreAutomatic,
                amstradCpc,
                amstradPcw,
                ibmScan
            }
            .Concat(epson)
            .Append(apple)
            .ToArray();
        var selections = new[]
        {
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.AmigaPrefix, StringComparison.OrdinalIgnoreCase), amiga),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.CommodorePrefix, StringComparison.OrdinalIgnoreCase), commodoreAutomatic),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.AmstradPrefix, StringComparison.OrdinalIgnoreCase), isoSelected),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) || id.Equals(DiskImageFormatIds.Mac1440, StringComparison.OrdinalIgnoreCase), isoSelected),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.AcornDfsPrefix, StringComparison.OrdinalIgnoreCase) || id.StartsWith(DiskImageFormatIds.AcornAdfsPrefix, StringComparison.OrdinalIgnoreCase), isoSelected),
            new ScpFormatSelection(id => id.Equals(DiskImageFormatIds.DecRx02, StringComparison.OrdinalIgnoreCase), dec),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.EpsonQx10Prefix, StringComparison.OrdinalIgnoreCase) || id.Equals(DiskImageFormatIds.UcsdIbmMfm, StringComparison.OrdinalIgnoreCase), isoSelected),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase) || id.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase), atari),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase) || id.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase) || id.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) || id.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase) || id.StartsWith(DiskImageFormatIds.MacPrefix, StringComparison.OrdinalIgnoreCase), apple)
        };
        KeyValuePair<ScpFormatFamily, IReadOnlyList<ScpSectorImageCandidate>>[] families =
        [
            new(ScpFormatFamily.Iso, isoFamily),
            new(ScpFormatFamily.Amiga, [amiga]),
            new(ScpFormatFamily.Commodore, [commodoreAutomatic]),
            new(ScpFormatFamily.Apple, [apple]),
            new(ScpFormatFamily.Dec, [dec])
        ];
        return new ScpCandidateRegistry(
            selections,
            defaults,
            families,
            [ScpFormatFamily.Amiga, ScpFormatFamily.Iso, ScpFormatFamily.Commodore, ScpFormatFamily.Apple, ScpFormatFamily.Dec],
            isoSelected);
    }
}
