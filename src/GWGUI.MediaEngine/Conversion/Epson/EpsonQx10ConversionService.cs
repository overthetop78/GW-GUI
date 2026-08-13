using GWGUI.MediaEngine.Containers.Epson.Raw;
using GWGUI.MediaEngine.Containers.ImageDisk;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Epson;
using GWGUI.MediaEngine.Reconstruction.Iso;

namespace GWGUI.MediaEngine.Conversion.Epson;

/// <summary>Convertit les captures et images Epson QX-10 vers IMG ou IMD sans passer par gw.exe.</summary>
public sealed class EpsonQx10ConversionService(IsoScpSectorImageReader scpReader, EpsonQx10RawImageReader rawReader, EpsonQx10RawImageWriter rawWriter, ImdReader imdReader, ImdWriter imdWriter)
{
    /// <summary>Indique si la cible Epson et son extension sont prises en charge.</summary>
    public static bool CanCreate(string formatId, string extension) => EpsonQx10GeometryCatalog.All.ContainsKey(formatId) && (extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Imd, StringComparison.OrdinalIgnoreCase));

    /// <summary>Relit ou reconstruit la source puis produit le conteneur demandé.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string targetFormatId, CancellationToken cancellationToken = default)
    {
        var outputExtension = Path.GetExtension(outputPath);
        if (!CanCreate(targetFormatId, outputExtension)) throw new NotSupportedException($"Epson target '{targetFormatId}' with extension '{outputExtension}' is not supported.");
        var sourceExtension = Path.GetExtension(sourcePath);
        ImdImage? detailed = null;
        var image = sourceExtension.Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase)
            ? await scpReader.ReadAsync(sourcePath, targetFormatId, cancellationToken).ConfigureAwait(false)
            : sourceExtension.Equals(DiskImageFileExtensions.Imd, StringComparison.OrdinalIgnoreCase)
                ? (detailed = await imdReader.ReadDetailedAsync(sourcePath, cancellationToken).ConfigureAwait(false)).SectorImage
                : await rawReader.ReadAsync(sourcePath, targetFormatId, cancellationToken).ConfigureAwait(false);
        if (!image.FormatId.Equals(targetFormatId, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException($"Epson source format '{image.FormatId}' does not match '{targetFormatId}'.");
        if (outputExtension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase))
            await rawWriter.WriteAsync(image, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
        else
            await imdWriter.WriteAsync(detailed ?? ImdImageBuilder.BuildEpson(image), outputPath, cancellationToken).ConfigureAwait(false);
    }
}
