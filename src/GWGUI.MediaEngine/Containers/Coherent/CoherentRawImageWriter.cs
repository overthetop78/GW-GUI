using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Containers.Storage;

namespace GWGUI.MediaEngine.Containers.Coherent;

/// <summary>Écrit un dump sectoriel COHERENT dans l'ordre zoné du Commodore 900.</summary>
public sealed class CoherentRawImageWriter(IAtomicImageFileWriter? fileWriter = null)
{
    private readonly IAtomicImageFileWriter files = fileWriter ?? new AtomicImageFileWriter();
    /// <summary>Valide chaque bloc et écrit atomiquement le dump BIN ou IMG.</summary>
    public async Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default)
    {
        ValidateImage(image);
        var bytes = new byte[checked(image.BlockCount * Commodore900Geometry.SectorSize)];
        for (var logicalBlock = 0; logicalBlock < image.BlockCount; logicalBlock++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!image.TryGetBlock(logicalBlock, out var block)) throw new InvalidDataException($"Le bloc Commodore 900 {logicalBlock} est absent.");
            if (block.Address != Commodore900Geometry.AddressOf(logicalBlock)) throw new InvalidDataException($"Le bloc Commodore 900 {logicalBlock} ne correspond pas à l'ordre physique documenté.");
            if (block.Data.Count != Commodore900Geometry.SectorSize) throw new InvalidDataException($"Le bloc Commodore 900 {logicalBlock} ne contient pas {Commodore900Geometry.SectorSize} octets.");
            block.Data.ToArray().CopyTo(bytes, logicalBlock * Commodore900Geometry.SectorSize);
        }
        await files.WriteAsync(path, (output, token) => output.WriteAsync(bytes, token).AsTask(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Contrôle l'identifiant, la géométrie et la longueur logique de l'image.</summary>
    private static void ValidateImage(SectorImage image)
    {
        if (!image.FormatId.Equals(DiskImageFormatIds.Commodore900Coherent, StringComparison.OrdinalIgnoreCase) || image.BlockSize != Commodore900Geometry.SectorSize || image.Cylinders != Commodore900Geometry.CylinderCount || image.Heads != Commodore900Geometry.HeadCount || image.SectorsPerTrack != Commodore900Geometry.MaximumSectorsPerTrack || image.BlockCount > Commodore900Geometry.BlockCount) throw new InvalidDataException("L'image sectorielle n'utilise pas la géométrie zonée du Commodore 900.");
    }

}
