using GWGUI.Scp.Decoding;
using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed record ExploredDiskImage(string SourcePath, SectorImage Image, FileSystemVolume Volume, bool FileSystemRecognized = true);

public sealed class DiskImageExplorer(
    AdfImageReader adfReader,
    AtariStImageReader stReader,
    MsaImageReader msaReader,
    AtrImageReader atrReader,
    CommodoreD64ImageReader d64Reader,
    CommodoreD71ImageReader d71Reader,
    CommodoreD81ImageReader d81Reader,
    AmstradDskImageReader amstradDskReader,
    AmigaScpSectorImageReader amigaScpReader,
    AtariScpSectorImageReader atariScpReader,
    CommodoreScpSectorImageReader commodoreScpReader,
    FileSystemRegistry fileSystems)
{
    public IReadOnlySet<string> SupportedFormatIds => fileSystems.SupportedFormatIds;

    public static DiskImageExplorer CreateDefault()
    {
        var scp = new ScpReader(); var decoders = new FluxDecoderRegistry();
        return new(new AdfImageReader(), new AtariStImageReader(), new MsaImageReader(), new AtrImageReader(),
            new CommodoreD64ImageReader(), new CommodoreD71ImageReader(), new CommodoreD81ImageReader(),
            new AmstradDskImageReader(),
            new AmigaScpSectorImageReader(scp, decoders), new AtariScpSectorImageReader(scp, decoders),
            new CommodoreScpSectorImageReader(scp, decoders), new FileSystemRegistry());
    }

    public async Task<ExploredDiskImage> ExploreAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("The disk image does not exist.", path);
        var extension = Path.GetExtension(path);
        SectorImage image;
        if (extension.Equals(".adf", StringComparison.OrdinalIgnoreCase)) image = await adfReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".st", StringComparison.OrdinalIgnoreCase)) image = await stReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".msa", StringComparison.OrdinalIgnoreCase)) image = await msaReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".atr", StringComparison.OrdinalIgnoreCase)) image = await atrReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".d64", StringComparison.OrdinalIgnoreCase)) image = await d64Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".d71", StringComparison.OrdinalIgnoreCase)) image = await d71Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".d81", StringComparison.OrdinalIgnoreCase)) image = await d81Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase) || extension.Equals(".edsk", StringComparison.OrdinalIgnoreCase)) image = await amstradDskReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".scp", StringComparison.OrdinalIgnoreCase))
        {
            if (formatId is not null && !SupportedFormatIds.Contains(formatId)) throw new NotSupportedException($"The selected format '{formatId}' is not supported by the explorer yet.");
            image = await ReadScpAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        }
        else throw new NotSupportedException($"The image extension '{extension}' is not supported by the explorer yet.");
        if (fileSystems.TryRead(image, formatId, out var volume)) return new(path, image, volume);
        var unknown = new FileSystemVolume(string.Empty, string.Empty, image.Capacity, 0, null, null, [], []);
        return new(path, image, unknown, false);
    }

    private async Task<SectorImage> ReadScpAsync(string path, string? formatId, CancellationToken cancellationToken)
    {
        if (formatId?.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase) == true)
            return await amigaScpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (formatId?.StartsWith("commodore.", StringComparison.OrdinalIgnoreCase) == true)
            return await commodoreScpReader.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        if (formatId?.StartsWith("amstrad.", StringComparison.OrdinalIgnoreCase) == true)
            return await atariScpReader.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
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
            () => atariScpReader.ReadAsync(path, "amstrad.pcw", cancellationToken)
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
