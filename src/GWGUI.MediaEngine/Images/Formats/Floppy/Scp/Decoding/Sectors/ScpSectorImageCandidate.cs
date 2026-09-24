namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding.Sectors;

using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Recognition;
using GWGUI.MediaEngine.Images.Models.Sectors;

/// <summary>Décrit un reconstructeur SCP nommé, sa famille et sa fonction de lecture réutilisable.</summary>
internal sealed record ScpSectorImageCandidate(
    string Id,
    ScpFormatFamily Family,
    Func<string, string?, CancellationToken, Task<SectorImage>> ReadAsync,
    Func<string, string?, IProgress<GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection.ScpExplorationProgress>?, CancellationToken, Task<SectorImage>>? ProgressiveReadAsync = null)
{
    public string DisplayName => Id switch
    {
        ScpCandidateIds.Amiga => "Amiga",
        ScpCandidateIds.IsoAutomatic => "ISO MFM/FM",
        ScpCandidateIds.Atari or ScpCandidateIds.AtariSt720 => "Atari ST",
        ScpCandidateIds.CommodoreAutomatic => "Commodore",
        ScpCandidateIds.Commodore1581 => "Commodore 1581",
        ScpCandidateIds.Apple => "Apple",
        ScpCandidateIds.Dec => "DEC RX02",
        _ when Id.Contains("ibm", StringComparison.OrdinalIgnoreCase) => "IBM PC",
        _ when Id.Contains("amstrad.cpc", StringComparison.OrdinalIgnoreCase) => "Amstrad CPC",
        _ when Id.Contains("amstrad.pcw", StringComparison.OrdinalIgnoreCase) => "Amstrad PCW",
        _ when Id.Contains("acorn", StringComparison.OrdinalIgnoreCase) => "Acorn",
        _ when Id.Contains("epson", StringComparison.OrdinalIgnoreCase) => "Epson QX-10",
        _ when Id.Contains("ucsd", StringComparison.OrdinalIgnoreCase) => "UCSD p-System",
        _ => Family.ToString()
    };

    public Task<SectorImage> ReadWithProgressAsync(
        string path,
        string? formatId,
        IProgress<GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection.ScpExplorationProgress>? progress,
        CancellationToken cancellationToken) =>
        ProgressiveReadAsync is null
            ? ReadAsync(path, formatId, cancellationToken)
            : ProgressiveReadAsync(path, formatId, progress, cancellationToken);
}
