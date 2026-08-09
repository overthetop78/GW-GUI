using GWGUI.Scp.Decoding;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

/// <summary>Decodes and re-encodes RWTS18 using real Apple II image containers.</summary>
public sealed class AppleRwts18ConversionService
{
    private readonly AppleDiskImageReader _appleReader = new();
    private readonly AppleScpSectorImageReader _scpReader = new(new ScpReader(), new FluxDecoderRegistry());
    private readonly AppleNibbleImageWriter _writer = new();

    public static bool CanCreate(string formatId, string extension) =>
        formatId.Equals("apple2.rwts18", StringComparison.OrdinalIgnoreCase) &&
        extension is ".nib" or ".woz";

    public async Task ConvertAsync(string sourcePath, string outputPath, CancellationToken cancellationToken = default)
    {
        SectorImage image = Path.GetExtension(sourcePath).Equals(".scp", StringComparison.OrdinalIgnoreCase)
            ? await _scpReader.ReadAsync(sourcePath, "apple2.rwts18", cancellationToken).ConfigureAwait(false)
            : await _appleReader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await _writer.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
    }
}
