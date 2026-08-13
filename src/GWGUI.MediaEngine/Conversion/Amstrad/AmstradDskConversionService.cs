using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Iso;

namespace GWGUI.MediaEngine.Conversion.Amstrad;

/// <summary>Convertit les captures SCP et conteneurs CPCEMU vers DSK ou EDSK pour Amstrad CPC et PCW.</summary>
public sealed class AmstradDskConversionService(IsoScpSectorImageReader scpReader, CpcDskReader reader, CpcDskWriter writer)
{
    /// <summary>Indique si le format et l'extension décrivent une cible CPCEMU prise en charge.</summary>
    public static bool CanCreate(string formatId, string extension) => IsAmstrad(formatId) && (extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Edsk, StringComparison.OrdinalIgnoreCase));

    /// <summary>Reconstruit une capture ou relit un conteneur puis écrit la disposition demandée.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string targetFormatId, CancellationToken cancellationToken = default)
    {
        var outputExtension = Path.GetExtension(outputPath);
        if (!CanCreate(targetFormatId, outputExtension)) throw new NotSupportedException($"Amstrad target '{targetFormatId}' with extension '{outputExtension}' is not supported.");
        var kind = outputExtension.Equals(DiskImageFileExtensions.Edsk, StringComparison.OrdinalIgnoreCase) ? CpcDskContainerKind.Extended : CpcDskContainerKind.Standard;
        CpcDskImage container;
        if (Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase))
        {
            var image = await scpReader.ReadAsync(sourcePath, targetFormatId, cancellationToken).ConfigureAwait(false);
            container = CpcDskImageBuilder.Build(image, kind);
        }
        else
        {
            var source = await reader.ReadDetailedAsync(sourcePath, cancellationToken).ConfigureAwait(false);
            container = source with { Kind = kind };
        }
        await writer.WriteAsync(container, outputPath, cancellationToken).ConfigureAwait(false);
    }

    private static bool IsAmstrad(string formatId) => formatId.Equals(DiskImageFormatIds.AmstradCpc, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AmstradPcw, StringComparison.OrdinalIgnoreCase);
}
