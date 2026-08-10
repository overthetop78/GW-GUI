using System.Buffers.Binary;
using GWGUI.Scp.Recognition.Definitions;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed class MsaImageReader : ISectorImageReader
{
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Msa, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var source = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (source.Length < 10 || ReadWord(source, 0) != 0x0e0f) throw new InvalidDataException("The MSA header is invalid.");
        var sectors = ReadWord(source, 2); var heads = ReadWord(source, 4) + 1;
        var startCylinder = ReadWord(source, 6); var endCylinder = ReadWord(source, 8);
        if (sectors is < 1 or > 36 || heads is < 1 or > 2 || endCylinder < startCylinder || endCylinder > 255) throw new InvalidDataException("The MSA geometry is invalid.");
        var trackBytes = checked(sectors * 512); var position = 10;
        var blocks = new List<SectorBlock>();
        for (var cylinder = startCylinder; cylinder <= endCylinder; cylinder++)
        for (var head = 0; head < heads; head++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (position + 2 > source.Length) throw new InvalidDataException("The MSA track table is truncated.");
            var packedLength = ReadWord(source, position); position += 2;
            if (position + packedLength > source.Length) throw new InvalidDataException("An MSA track is truncated.");
            var track = packedLength == trackBytes ? source.AsSpan(position, packedLength).ToArray() : Unpack(source.AsSpan(position, packedLength), trackBytes);
            position += packedLength;
            for (var sector = 0; sector < sectors; sector++)
            {
                var logical = (cylinder * heads + head) * sectors + sector;
                blocks.Add(new(logical, new(cylinder, head, sector + 1), track.AsSpan(sector * 512, 512).ToArray()));
            }
        }
        return new($"atarist.{((endCylinder + 1) * heads * sectors * 512) / 1024}", 512, endCylinder + 1, heads, sectors, blocks);
    }

    private static byte[] Unpack(ReadOnlySpan<byte> packed, int expected)
    {
        var output = new byte[expected]; var input = 0; var written = 0;
        while (input < packed.Length && written < output.Length)
        {
            if (packed[input] != 0xe5) { output[written++] = packed[input++]; continue; }
            if (input + 4 > packed.Length) throw new InvalidDataException("An MSA compressed run is truncated.");
            var value = packed[input + 1]; var count = BinaryPrimitives.ReadUInt16BigEndian(packed[(input + 2)..]); input += 4;
            if (count == 0 || written + count > output.Length) throw new InvalidDataException("An MSA compressed run exceeds its track.");
            output.AsSpan(written, count).Fill(value); written += count;
        }
        if (input != packed.Length || written != expected) throw new InvalidDataException("The decompressed MSA track has an invalid length.");
        return output;
    }

    private static int ReadWord(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);
}
