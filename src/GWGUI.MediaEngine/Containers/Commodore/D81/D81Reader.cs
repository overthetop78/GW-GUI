using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.MediaEngine.Containers.Commodore.D81;

/// <summary>Lit un conteneur Commodore D81 dans sa vue logique de 3 200 blocs.</summary>
public sealed class D81Reader
{
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;

    public D81Reader() : this(File.ReadAllBytesAsync) { }

    internal D81Reader(Func<string, CancellationToken, Task<byte[]>> readBytes)
    {
        this.readBytes = readBytes ?? throw new ArgumentNullException(nameof(readBytes));
    }

    /// <summary>Lit un D81 de longueur exacte et reconstruit ses blocs logiques.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await readBytes(path, cancellationToken).ConfigureAwait(false);
        if (data.Length != D81Layout.ImageLength) throw D81Exceptions.InvalidLength(data.Length, D81Layout.ImageLength);
        var geometry = new LinearSectorImageGeometry(Commodore1581Geometry.LogicalBlockSize, Commodore1581Geometry.LogicalCylinderCount, Commodore1581Geometry.LogicalHeadCount, Commodore1581Geometry.LogicalBlocksPerTrack, SectorNumbering.ZeroBased);
        return LinearSectorImageBuilder.Create(data, DiskImageFormatIds.Commodore1581, geometry, cancellationToken);
    }
}
