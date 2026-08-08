using System.Buffers.Binary;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

/// <summary>Reads raw Apple II/Macintosh images, NIB/WOZ, 2IMG and DiskCopy 4.2 containers.</summary>
public sealed class AppleDiskImageReader : ISectorImageReader
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
        { ".d13", ".do", ".po", ".2mg", ".image", ".dc42", ".nib", ".woz", ".dsk", ".img" };

    private static readonly int[] ProDosToPhysical = [0, 2, 4, 6, 8, 10, 12, 14, 1, 3, 5, 7, 9, 11, 13, 15];
    private static readonly int[] PhysicalToDos = [0, 7, 14, 6, 13, 5, 12, 4, 11, 3, 10, 2, 9, 1, 8, 15];

    public bool CanRead(string path) => Extensions.Contains(Path.GetExtension(path));

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var extension = Path.GetExtension(path);
        if (bytes.AsSpan().StartsWith("2IMG"u8)) return ReadTwoImg(bytes);
        if (extension.Equals(".image", StringComparison.OrdinalIgnoreCase) || extension.Equals(".dc42", StringComparison.OrdinalIgnoreCase)) return ReadDiskCopy(bytes);
        if (extension.Equals(".nib", StringComparison.OrdinalIgnoreCase)) return AppleNibbleImageDecoder.ReadNib(bytes);
        if (extension.Equals(".woz", StringComparison.OrdinalIgnoreCase)) return AppleNibbleImageDecoder.ReadWoz(bytes);
        return ReadRaw(bytes, extension);
    }

    public static bool LooksLikeAppleImage(string path)
    {
        try
        {
            var extension = Path.GetExtension(path);
            if (extension.Equals(".d13", StringComparison.OrdinalIgnoreCase) || extension.Equals(".do", StringComparison.OrdinalIgnoreCase) || extension.Equals(".po", StringComparison.OrdinalIgnoreCase) || extension.Equals(".2mg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".image", StringComparison.OrdinalIgnoreCase) || extension.Equals(".dc42", StringComparison.OrdinalIgnoreCase) || extension.Equals(".nib", StringComparison.OrdinalIgnoreCase) || extension.Equals(".woz", StringComparison.OrdinalIgnoreCase)) return true;
            if (!extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase)) return false;
            var bytes = File.ReadAllBytes(path);
            return bytes.Length == 143_360 && (LooksLikeDos33(bytes) || LooksLikeProDos(bytes) || LooksLikeProDos(ConvertDosOrderToProDosBlocks(bytes)) || LooksLikeSos(bytes))
                || bytes.Length is 409_600 or 819_200 or 1_474_560 && LooksLikeMac(bytes);
        }
        catch { return false; }
    }

    private static SectorImage ReadTwoImg(byte[] container)
    {
        if (container.Length < 64) throw new InvalidDataException("The 2IMG header is truncated.");
        var headerLength = checked((int)BinaryPrimitives.ReadUInt16LittleEndian(container.AsSpan(8)));
        var imageFormat = BinaryPrimitives.ReadUInt32LittleEndian(container.AsSpan(12));
        var dataOffset = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(container.AsSpan(24)));
        var dataLength = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(container.AsSpan(28)));
        if (headerLength < 64 || dataOffset < headerLength || dataLength <= 0 || dataOffset > container.Length - dataLength)
            throw new InvalidDataException("The 2IMG data range is invalid.");
        if (imageFormat == 2) return AppleNibbleImageDecoder.ReadNib(container.AsSpan(dataOffset, dataLength));
        if (imageFormat > 2) throw new NotSupportedException("The 2IMG image format is not supported.");
        var extension = imageFormat == 0 ? ".do" : ".po";
        return ReadRaw(container.AsSpan(dataOffset, dataLength).ToArray(), extension);
    }

    private static SectorImage ReadDiskCopy(byte[] container)
    {
        const int headerLength = 84;
        if (container.Length < headerLength) throw new InvalidDataException("The DiskCopy header is truncated.");
        var dataLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(container.AsSpan(64)));
        var tagLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(container.AsSpan(68)));
        if (dataLength <= 0 || headerLength + (long)dataLength + tagLength > container.Length)
            throw new InvalidDataException("The DiskCopy payload is invalid.");
        var payload = container.AsSpan(headerLength, dataLength).ToArray();
        if (LooksLikeMac(payload) || LooksLikeProDos(payload)) return ReadRaw(payload, ".image");

        // Lisa DiskCopy images require the 12-byte tag stored alongside every 512-byte page.
        if (tagLength == dataLength / 512 * 12)
        {
            var tags = container.AsSpan(headerLength + dataLength, tagLength);
            var blocks = new SectorBlock[dataLength / 512];
            for (var logical = 0; logical < blocks.Length; logical++)
            {
                blocks[logical] = new(logical, new(logical / 10, 0, logical % 10),
                    payload.AsSpan(logical * 512, 512).ToArray(), Tag: tags.Slice(logical * 12, 12).ToArray());
            }
            return new("applelisa.office", 512, Math.Max(1, blocks.Length / 10), 1, 10, blocks,
                capacity: dataLength, logicalBlockCount: blocks.Length);
        }
        throw new InvalidDataException("The DiskCopy image is neither a recognized Macintosh/ProDOS image nor a tagged Lisa image.");
    }

    private static SectorImage ReadRaw(byte[] data, string extension)
    {
        if (data.Length == 35 * 13 * 256)
            return CreateLinear(data, "apple2.dos32", 256, 35, 1, 13);
        if (data.Length == 143_360)
        {
            // DOS-order images retain 256-byte track/sector addressing. ProDOS-order images
            // are exposed as 512-byte blocks so ProDOS and SOS can read them directly.
            if (extension.Equals(".po", StringComparison.OrdinalIgnoreCase) || LooksLikeProDos(data))
                return CreateLinear(data, "apple2.prodos", 512, 35, 1, 8);
            if (LooksLikeDos33(data)) return CreateLinear(data, "apple2.dos33", 256, 35, 1, 16);

            var prodosBlocks = ConvertDosOrderToProDosBlocks(data);
            // Apple III SOS uses the same block-directory layout as ProDOS. Check the
            // boot signature first so a valid SOS volume is not mislabeled as Apple II.
            if (LooksLikeSos(data))
                return CreateLinear(prodosBlocks, "apple3.sos", 512, 35, 1, 8);
            if (LooksLikeProDos(prodosBlocks))
                return CreateLinear(prodosBlocks, "apple2.prodos", 512, 35, 1, 8);
            return CreateLinear(data, "apple2.dos33", 256, 35, 1, 16);
        }
        if (data.Length is 409_600 or 819_200 or 1_474_560)
        {
            if (LooksLikeMac(data))
            {
                var signature = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(1024));
                return CreateLinear(data, signature == 0xd2d7 ? "applemac.mfs" : "applemac.hfs", 512,
                    data.Length == 409_600 ? 80 : 80, data.Length == 409_600 ? 1 : 2, data.Length / 512 / (data.Length == 409_600 ? 80 : 160));
            }
            // Apple II 3.5-inch and hard-disk utility images use ProDOS blocks.
            if (LooksLikeProDos(data)) return CreateLinear(data, "apple2.prodos", 512, 80, 2, data.Length / 512 / 160);
        }
        throw new InvalidDataException("The Apple disk image has an unsupported size or signature.");
    }

    private static SectorImage CreateLinear(byte[] data, string formatId, int blockSize, int cylinders, int heads, int sectorsPerTrack)
    {
        var count = data.Length / blockSize;
        var blocks = new SectorBlock[count];
        for (var logical = 0; logical < count; logical++)
        {
            var perCylinder = heads * sectorsPerTrack;
            blocks[logical] = new(logical, new(logical / perCylinder, logical / sectorsPerTrack % heads, logical % sectorsPerTrack), data.AsSpan(logical * blockSize, blockSize).ToArray());
        }
        return new(formatId, blockSize, cylinders, heads, sectorsPerTrack, blocks, capacity: data.Length, logicalBlockCount: count);
    }

    internal static SectorImage CreateAppleIIFromDecodedTracks(IEnumerable<(int Track, IReadOnlyList<GWGUI.Scp.Decoding.DecodedSector> Sectors)> decodedTracks)
    {
        var selected = decodedTracks.SelectMany(item => item.Sectors
                .Where(sector => sector.Data is { Count: 256 } && sector.Number is >= 0 and < 16)
                .Select(sector => (item.Track, Sector: sector)))
            .GroupBy(item => (item.Track, item.Sector.Number))
            .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.Sector.IntegrityValid == true).First().Sector);
        var trackCount = Math.Max(35, selected.Count == 0 ? 35 : selected.Keys.Max(key => key.Track) + 1);
        var sectorsPerTrack = selected.Count > 0 && selected.Keys.Max(key => key.Number) < 13 ? 13 : 16;
        var dosBlocks = selected.Where(pair => pair.Key.Number < sectorsPerTrack).Select(pair => new SectorBlock(pair.Key.Track * sectorsPerTrack + pair.Key.Number,
            new(pair.Key.Track, 0, pair.Key.Number), pair.Value.Data!.ToArray(), pair.Value.IntegrityValid)).ToArray();
        if (dosBlocks.Length == 0) return new("apple2.gcr", 256, trackCount, 1, 16, []);

        if (sectorsPerTrack == 13)
            return new("apple2.dos32", 256, trackCount, 1, 13, dosBlocks);

        var prodosBlocks = new List<SectorBlock>();
        for (var track = 0; track < trackCount; track++)
        for (var block = 0; block < 8; block++)
        {
            var first = ProDosToPhysical[block * 2]; var second = ProDosToPhysical[block * 2 + 1];
            if (!selected.TryGetValue((track, first), out var low) || !selected.TryGetValue((track, second), out var high)) continue;
            var data = low.Data!.Concat(high.Data!).ToArray();
            prodosBlocks.Add(new(track * 8 + block, new(track, 0, block), data,
                low.IntegrityValid == true && high.IntegrityValid == true));
        }
        var prodosProbe = new byte[trackCount * 8 * 512];
        foreach (var block in prodosBlocks) block.Data.ToArray().CopyTo(prodosProbe, block.LogicalBlock * 512);
        if (LooksLikeProDos(prodosProbe)) return new("apple2.prodos", 512, trackCount, 1, 8, prodosBlocks);
        return new(LooksLikeDos33(ToDense(dosBlocks, trackCount * 16, 256)) ? "apple2.dos33" : "apple2.gcr",
            256, trackCount, 1, sectorsPerTrack, dosBlocks);
    }

    private static byte[] ToDense(IEnumerable<SectorBlock> blocks, int count, int blockSize)
    {
        var data = new byte[count * blockSize];
        foreach (var block in blocks) block.Data.ToArray().CopyTo(data, block.LogicalBlock * blockSize);
        return data;
    }

    private static bool LooksLikeDos33(ReadOnlySpan<byte> data)
    {
        if (data.Length != 143_360) return false;
        var vtoc = data.Slice(17 * 16 * 256, 256);
        return vtoc[1] is > 0 and < 35 && vtoc[2] < 16 && vtoc[0x35] is >= 13 and <= 16 && vtoc[0x36] == 0;
    }

    private static bool LooksLikeProDos(ReadOnlySpan<byte> data)
    {
        if (data.Length < 3 * 512) return false;
        var block = data.Slice(2 * 512, 512);
        var storage = block[4] >> 4; var nameLength = block[4] & 0x0f;
        return storage == 0x0f && nameLength is > 0 and <= 15 && block[0x23] == 0x27;
    }

    private static bool LooksLikeMac(ReadOnlySpan<byte> data)
    {
        if (data.Length < 1536) return false;
        var signature = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(1024, 2));
        return signature is 0xd2d7 or 0x4244;
    }

    private static bool LooksLikeSos(ReadOnlySpan<byte> data)
    {
        if (data.Length != 143_360) return false;
        var boot = System.Text.Encoding.ASCII.GetString(data[..Math.Min(128, data.Length)]);
        return boot.Contains("SOS", StringComparison.OrdinalIgnoreCase);
    }

    private static byte[] ConvertDosOrderToProDosBlocks(ReadOnlySpan<byte> dosOrder)
    {
        if (dosOrder.Length % (16 * 256) != 0) throw new InvalidDataException("The Apple 5.25-inch image has an invalid length.");
        var output = new byte[dosOrder.Length];
        var tracks = dosOrder.Length / (16 * 256);
        for (var track = 0; track < tracks; track++)
        {
            for (var logicalSector = 0; logicalSector < 16; logicalSector++)
            {
                var physicalSector = ProDosToPhysical[logicalSector];
                var dosFileSector = PhysicalToDos[physicalSector];
                dosOrder.Slice((track * 16 + dosFileSector) * 256, 256)
                    .CopyTo(output.AsSpan((track * 16 + logicalSector) * 256, 256));
            }
        }
        return output;
    }
}
