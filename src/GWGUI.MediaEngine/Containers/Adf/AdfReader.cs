using GWGUI.MediaEngine.Geometries.Acorn;
using GWGUI.MediaEngine.Geometries.Amiga;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Adf;

/// <summary>Lit les conteneurs ADF Acorn et Amiga dont la géométrie est déterminée par la taille exacte.</summary>
public sealed class AdfReader
{
    private static readonly int[] AcceptedSizes = [AcornAdfGeometry.Capacity, AcornAdfGeometry.PaddedCapacity, AmigaAdfGeometry.DoubleDensityCapacity, AmigaAdfGeometry.HighDensityCapacity];

    /// <summary>Lit le fichier et reconstruit ses secteurs avec une numérotation commençant à zéro.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (data.Length == AcornAdfGeometry.Capacity) return RegularSectorImageBuilder.Create(data, AcornAdfGeometry.Geometry, cancellationToken);
        if (data.Length == AcornAdfGeometry.PaddedCapacity) return RegularSectorImageBuilder.Create(data, AcornAdfGeometry.Geometry, cancellationToken, AcornAdfGeometry.PaddedTrailingByteCount);
        if (data.Length == AmigaAdfGeometry.DoubleDensity.Capacity) return RegularSectorImageBuilder.Create(data, AmigaAdfGeometry.DoubleDensity, cancellationToken);
        if (data.Length == AmigaAdfGeometry.HighDensity.Capacity) return RegularSectorImageBuilder.Create(data, AmigaAdfGeometry.HighDensity, cancellationToken);
        throw AdfExceptions.InvalidSize(data.Length, AcceptedSizes);
    }
}
