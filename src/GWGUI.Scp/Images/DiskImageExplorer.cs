using GWGUI.Scp.Decoding;
using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed record ExploredDiskImage(string SourcePath, SectorImage Image, FileSystemVolume Volume);

public sealed class DiskImageExplorer(
    AdfImageReader adfReader,
    AtariStImageReader stReader,
    MsaImageReader msaReader,
    AtrImageReader atrReader,
    AmigaScpSectorImageReader amigaScpReader,
    AtariScpSectorImageReader atariScpReader,
    FileSystemRegistry fileSystems)
{
    public IReadOnlySet<string> SupportedFormatIds => fileSystems.SupportedFormatIds;

    public static DiskImageExplorer CreateDefault()
    {
        var scp = new ScpReader(); var decoders = new FluxDecoderRegistry();
        return new(new AdfImageReader(), new AtariStImageReader(), new MsaImageReader(), new AtrImageReader(),
            new AmigaScpSectorImageReader(scp, decoders), new AtariScpSectorImageReader(scp, decoders), new FileSystemRegistry());
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
        else if (extension.Equals(".scp", StringComparison.OrdinalIgnoreCase))
        {
            if (formatId is not null && !SupportedFormatIds.Contains(formatId)) throw new NotSupportedException($"The selected format '{formatId}' is not supported by the explorer yet.");
            image = await ReadScpAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        }
        else throw new NotSupportedException($"The image extension '{extension}' is not supported by the explorer yet.");
        var volume = fileSystems.Read(image, formatId);
        return new(path, image, volume);
    }

    private async Task<SectorImage> ReadScpAsync(string path, string? formatId, CancellationToken cancellationToken)
    {
        if (formatId?.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase) == true)
            return await amigaScpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (formatId is not null)
            return await atariScpReader.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        try { return await atariScpReader.ReadAsync(path, null, cancellationToken).ConfigureAwait(false); }
        catch (InvalidDataException) { return await amigaScpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false); }
    }
}
