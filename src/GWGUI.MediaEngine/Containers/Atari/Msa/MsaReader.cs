using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Reading;

namespace GWGUI.MediaEngine.Containers.Atari.Msa;

public sealed class MsaReader : ISectorImageReader
{
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Msa, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var source = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (source.Length < 10 || ReadWord(source, 0) != 0x0E0F) throw new InvalidDataException("The MSA header is invalid.");
        var sectors = ReadWord(source, 2);
        var heads = ReadWord(source, 4) + 1;
        var startCylinder = ReadWord(source, 6);
        var endCylinder = ReadWord(source, 8);
        if (sectors is < 1 or > 36 || heads is < 1 or > 2 || endCylinder < startCylinder || endCylinder > 255) throw new InvalidDataException("The MSA geometry is invalid.");
        var trackBytes = checked(sectors * 512);
        var position = 10;
        var blocks = new List<SectorBlock>();
        for (var cylinder = startCylinder; cylinder <= endCylinder; cylinder++)
        {
            for (var head = 0; head < heads; head++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (position + 2 > source.Length) throw new InvalidDataException("The MSA track table is truncated.");
                var packedLength = ReadWord(source, position);
                position += 2;
                if (position + packedLength > source.Length) throw new InvalidDataException("An MSA track is truncated.");
                var track = packedLength == trackBytes ? source.AsSpan(position, packedLength).ToArray() : MsaRleDecoder.Unpack(source.AsSpan(position, packedLength), trackBytes);
                position += packedLength;
                for (var sector = 0; sector < sectors; sector++)
                {
                    var logical = (cylinder * heads + head) * sectors + sector;
                    blocks.Add(new(logical, new(cylinder, head, sector + 1), track.AsSpan(sector * 512, 512).ToArray()));
                }
            }
        }
        return new(DiskImageFormatIds.AtariStFromCapacity((endCylinder + 1) * heads * sectors * 512L), 512, endCylinder + 1, heads, sectors, blocks);
    }

    private static int ReadWord(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);
}
