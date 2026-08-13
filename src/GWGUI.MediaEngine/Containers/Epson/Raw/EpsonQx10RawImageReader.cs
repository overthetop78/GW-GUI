using GWGUI.MediaEngine.Geometries.Epson;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Epson.Raw;

/// <summary>Relit une image Epson QX-10 brute selon le profil explicitement sélectionné.</summary>
public sealed class EpsonQx10RawImageReader
{
    /// <summary>Découpe les octets selon la géométrie Epson demandée.</summary>
    public async Task<SectorImage> ReadAsync(string path, string formatId, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var geometry = EpsonQx10GeometryCatalog.Resolve(formatId);
        var expectedLength = geometry.AllTracks.Sum(track => track.Count * track.SectorSize);
        if (bytes.Length != expectedLength) throw new InvalidDataException($"Epson image length is {bytes.Length}; expected {expectedLength} bytes for '{formatId}'.");
        var blocks = new List<SectorBlock>();
        var offset = 0;
        var maximumSectors = 0;
        var sizes = new HashSet<int>();
        for (var cylinder = 0; cylinder < geometry.Cylinders; cylinder++)
        {
            for (var head = 0; head < geometry.Heads; head++)
            {
                var track = geometry.Track(cylinder, head);
                maximumSectors = Math.Max(maximumSectors, track.Count);
                sizes.Add(track.SectorSize);
                for (var index = 0; index < track.Count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var data = bytes.AsSpan(offset, track.SectorSize).ToArray();
                    blocks.Add(new(blocks.Count, new(cylinder, head, track.FirstSector + index), data));
                    offset += track.SectorSize;
                }
            }
        }
        var blockSize = sizes.OrderByDescending(size => geometry.AllTracks.Where(track => track.SectorSize == size).Sum(track => track.Count)).First();
        return new(formatId, blockSize, geometry.Cylinders, geometry.Heads, maximumSectors, blocks, sizes.Count > 1, bytes.Length, blocks.Count);
    }
}
