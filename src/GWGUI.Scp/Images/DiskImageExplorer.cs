using GWGUI.Scp.Decoding;
using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed record ExploredDiskImage(string SourcePath, SectorImage Image, FileSystemVolume Volume);

public sealed class DiskImageExplorer(
    AdfImageReader adfReader,
    AmigaScpSectorImageReader scpReader,
    FileSystemRegistry fileSystems)
{
    public static DiskImageExplorer CreateDefault() => new(new AdfImageReader(), new AmigaScpSectorImageReader(new ScpReader(), new FluxDecoderRegistry()), new FileSystemRegistry());

    public async Task<ExploredDiskImage> ExploreAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("The disk image does not exist.", path);
        if (formatId is not null && formatId.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase)) formatId = "amigados";
        var extension = Path.GetExtension(path);
        SectorImage image;
        if (extension.Equals(".adf", StringComparison.OrdinalIgnoreCase)) image = await adfReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        else if (extension.Equals(".scp", StringComparison.OrdinalIgnoreCase))
        {
            if (formatId is not null && !formatId.Equals("amigados", StringComparison.OrdinalIgnoreCase)) throw new NotSupportedException($"The selected format '{formatId}' is not supported by the explorer yet.");
            image = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        }
        else throw new NotSupportedException($"The image extension '{extension}' is not supported by the explorer yet.");
        var volume = fileSystems.Read(image, formatId);
        return new(path, image, volume);
    }
}
