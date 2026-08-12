using GWGUI.MediaEngine.FileSystems.Coherent;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.MediaEngine.Containers.Coherent;

/// <summary>Lit les dumps sectoriels bruts des disquettes COHERENT, notamment ceux du Commodore 900.</summary>
public sealed class CoherentRawImageReader
{
    /// <summary>Lit puis valide un dump COHERENT depuis son chemin.</summary>
    /// <param name="path">Chemin du dump à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle reconstruite selon la géométrie du Commodore 900.</returns>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default) => Read(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false), cancellationToken);

    /// <summary>Valide et reconstruit un dump COHERENT déjà chargé en mémoire.</summary>
    /// <param name="bytes">Contenu intégral du dump en lecture seule.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la reconstruction.</param>
    /// <returns>Image sectorielle reconstruite selon la géométrie du Commodore 900.</returns>
    public Task<SectorImage> ReadAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default) => Task.FromResult(Read(bytes.Span, cancellationToken));

    /// <summary>Valide le superbloc et reconstruit les blocs sectoriels du dump.</summary>
    private static SectorImage Read(ReadOnlySpan<byte> bytes, CancellationToken cancellationToken)
    {
        if (!CoherentFormat.LooksLikeCoherent(bytes)) throw CoherentRawImageExceptions.ContentNotCoherent(bytes.Length);
        if (bytes.Length % Commodore900Geometry.SectorSize != 0) throw CoherentRawImageExceptions.NonSectorAlignedLength(bytes.Length, Commodore900Geometry.SectorSize);
        var availableBlocks = bytes.Length / Commodore900Geometry.SectorSize;
        var declaredBlocks = CoherentFormat.ReadDeclaredFileSystemBlockCount(bytes);
        if (declaredBlocks < 3 || declaredBlocks > availableBlocks) throw CoherentRawImageExceptions.InvalidDeclaredBlockCount(declaredBlocks, availableBlocks);
        if (availableBlocks > Commodore900Geometry.BlockCount) throw CoherentRawImageExceptions.GeometryCapacityExceeded(availableBlocks, Commodore900Geometry.BlockCount);
        return Commodore900SectorImageBuilder.Create(bytes, cancellationToken);
    }
}
