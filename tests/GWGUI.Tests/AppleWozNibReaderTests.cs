using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Containers.Apple.Woz;
using GWGUI.MediaEngine.Images;

namespace GWGUI.Tests;

/// <summary>Vérifie la lecture publique des conteneurs WOZ1, WOZ2 et NIB locaux.</summary>
public sealed class AppleWozNibReaderTests
{
    [Theory]
    [InlineData("3DChart! (1984)(Spectral Graphics Software)(US)(Disk 1 of 2).woz", "WOZ1")]
    [InlineData("816-Paint v3.1 (1987)(Baudville)(IIE)[128K][5.25''].woz", "WOZ2")]
    public async Task ReadsWozSignatureCrcChunksTrackMapBitCountAndTrackData(string fileName, string signature)
    {
        var path = Path.Combine(AppleImageRoot(), fileName);
        var bytes = await File.ReadAllBytesAsync(path);
        var chunks = ReadChunks(bytes);

        Assert.Equal(signature, System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
        Assert.Equal(BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(8, 4)), ComputeCrc32(bytes.AsSpan(12)));
        Assert.True(chunks.TryGetValue("INFO", out var info));
        Assert.Equal(1, bytes[info.Offset + 1]);
        Assert.True(chunks.TryGetValue("TMAP", out var tmap));
        Assert.True(tmap.Length >= 160);
        Assert.True(chunks.TryGetValue("TRKS", out var trks));

        var descriptor = bytes.AsSpan(tmap.Offset, 160).ToArray().First(value => value != 0xff);
        uint bitCount;
        ReadOnlySpan<byte> trackData;
        if (signature == "WOZ1")
        {
            var entry = trks.Offset + descriptor * 6656;
            bitCount = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(entry + 6648, 2));
            trackData = bytes.AsSpan(entry, checked((int)((bitCount + 7) / 8)));
        }
        else
        {
            var entry = trks.Offset + descriptor * 8;
            var startBlock = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(entry, 2));
            var blockCount = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(entry + 2, 2));
            bitCount = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(entry + 4, 4));
            Assert.True(blockCount > 0);
            trackData = bytes.AsSpan(startBlock * 512, checked((int)((bitCount + 7) / 8)));
        }

        Assert.True(bitCount > 0);
        Assert.Contains(trackData.ToArray(), value => value != 0);
        var image = await new AppleDiskImageReader().ReadAsync(path);
        Assert.NotEmpty(image.AvailableBlocks);
    }

    [Fact]
    public async Task ReadsNibTracksInTheirOriginalOrder()
    {
        var path = Path.Combine(AppleImageRoot(), "Merlin (1983)(Southwestern Data Systems)(US)(Side A).nib");
        var bytes = await File.ReadAllBytesAsync(path);

        Assert.Equal(35, bytes.Length / 6656);
        Assert.Equal(0, bytes.Length % 6656);
        Assert.False(bytes.AsSpan(0, 6656).SequenceEqual(bytes.AsSpan(6656, 6656)));
        var image = await new AppleDiskImageReader().ReadAsync(path);
        Assert.True(image.Cylinders >= 35);
        Assert.NotEmpty(image.AvailableBlocks);
        Assert.Equal(Enumerable.Range(0, 35),
            image.AvailableBlocks.Select(block => block.Address.Cylinder).Distinct().Order());
    }

    [Fact]
    public async Task RejectsInvalidWozCrcTruncatedChunkAndOutOfBoundsTrackReference()
    {
        var source = await File.ReadAllBytesAsync(Path.Combine(
            AppleImageRoot(),
            "3DChart! (1984)(Spectral Graphics Software)(US)(Disk 1 of 2).woz"));
        var invalidCrc = (byte[])source.Clone();
        invalidCrc[^1] ^= 0xff;
        var truncatedChunk = (byte[])source.Clone();
        BinaryPrimitives.WriteUInt32LittleEndian(truncatedChunk.AsSpan(16, 4), uint.MaxValue);
        WriteCrc(truncatedChunk);
        var invalidReference = (byte[])source.Clone();
        var tmap = ReadChunks(invalidReference)["TMAP"];
        invalidReference[tmap.Offset] = 0xfe;
        WriteCrc(invalidReference);

        await AssertInvalidWoz(invalidCrc, "CRC32");
        await AssertInvalidWoz(truncatedChunk, "INFO");
        await AssertInvalidWoz(invalidReference, "descriptor 254");
    }

    /// <summary>Vérifie qu'un chunk inconnu n'empêche pas la lecture du conteneur WOZ.</summary>
    [Fact]
    public async Task IgnoresUnknownWozChunk()
    {
        var source = await File.ReadAllBytesAsync(Path.Combine(AppleImageRoot(), "3DChart! (1984)(Spectral Graphics Software)(US)(Disk 1 of 2).woz"));
        var bytes = new byte[source.Length + WozLayout.ChunkHeaderLength + 3];
        source.CopyTo(bytes, 0);
        "TEST"u8.CopyTo(bytes.AsSpan(source.Length + WozLayout.ChunkIdOffset, WozLayout.ChunkIdLength));
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(source.Length + WozLayout.ChunkLengthOffset, WozLayout.ChunkLengthSize), 3);
        bytes[^3..].AsSpan().Fill(0x5a);
        WriteCrc(bytes);

        Assert.NotEmpty(WozReader.Read(bytes).AvailableBlocks);
    }

    /// <summary>Vérifie le traitement des longueurs de bits invalides dans les pistes WOZ1 et WOZ2.</summary>
    [Fact]
    public async Task HandlesInvalidWozTrackBitLengths()
    {
        var woz1 = await File.ReadAllBytesAsync(Path.Combine(AppleImageRoot(), "3DChart! (1984)(Spectral Graphics Software)(US)(Disk 1 of 2).woz"));
        var woz1Chunks = ReadChunks(woz1);
        foreach (var descriptor in woz1.AsSpan(woz1Chunks[WozFormat.TrackMapChunkId].Offset, WozLayout.TrackMapLength).ToArray().Where(value => value != WozLayout.MissingTrackDescriptor).Distinct())
        {
            var entry = woz1Chunks[WozFormat.TracksChunkId].Offset + descriptor * WozLayout.Woz1TrackEntryLength;
            BinaryPrimitives.WriteUInt16LittleEndian(woz1.AsSpan(entry + WozLayout.Woz1BitCountOffset, WozLayout.Woz1BitCountLength), ushort.MaxValue);
        }
        WriteCrc(woz1);
        Assert.Empty(WozReader.Read(woz1).AvailableBlocks);

        var woz2 = await File.ReadAllBytesAsync(Path.Combine(AppleImageRoot(), "816-Paint v3.1 (1987)(Baudville)(IIE)[128K][5.25''].woz"));
        var woz2Chunks = ReadChunks(woz2);
        var descriptor2 = woz2.AsSpan(woz2Chunks[WozFormat.TrackMapChunkId].Offset, WozLayout.TrackMapLength).ToArray().First(value => value != WozLayout.MissingTrackDescriptor);
        var entry2 = woz2Chunks[WozFormat.TracksChunkId].Offset + descriptor2 * WozLayout.Woz2TrackDescriptorLength;
        BinaryPrimitives.WriteUInt32LittleEndian(woz2.AsSpan(entry2 + WozLayout.Woz2BitCountOffset, WozLayout.Woz2BitCountLength), uint.MaxValue);
        WriteCrc(woz2);
        Assert.Throws<InvalidDataException>(() => WozReader.Read(woz2));
    }

    [Fact]
    public async Task RejectsNibLengthThatIsNotAWholeNumberOfTracks()
    {
        var source = await File.ReadAllBytesAsync(Path.Combine(
            AppleImageRoot(),
            "Merlin (1983)(Southwestern Data Systems)(US)(Side A).nib"));
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-invalid-nib-{Guid.NewGuid():N}.nib");
        await File.WriteAllBytesAsync(path, source.AsMemory(0, source.Length - 1));
        try
        {
            var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
                new AppleDiskImageReader().ReadAsync(path));
            Assert.Contains((source.Length - 1).ToString(), exception.Message, StringComparison.Ordinal);
            Assert.Contains("6656", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static async Task AssertInvalidWoz(byte[] bytes, string expectedMessagePart)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-invalid-woz-{Guid.NewGuid():N}.woz");
        await File.WriteAllBytesAsync(path, bytes);
        try
        {
            var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
                new AppleDiskImageReader().ReadAsync(path));
            Assert.Contains(expectedMessagePart, exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static Dictionary<string, (int Offset, int Length)> ReadChunks(byte[] bytes)
    {
        var result = new Dictionary<string, (int Offset, int Length)>(StringComparer.Ordinal);
        var offset = 12;
        while (offset <= bytes.Length - 8)
        {
            var id = System.Text.Encoding.ASCII.GetString(bytes, offset, 4);
            var length = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset + 4, 4)));
            offset += 8;
            if (length < 0 || offset > bytes.Length - length) break;
            result[id] = (offset, length);
            offset += length;
        }
        return result;
    }

    private static void WriteCrc(byte[] bytes) =>
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(8, 4), ComputeCrc32(bytes.AsSpan(12)));

    private static uint ComputeCrc32(ReadOnlySpan<byte> bytes)
    {
        var crc = uint.MaxValue;
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ (0xedb88320u & (uint)-(int)(crc & 1));
        }
        return ~crc;
    }

    private static string AppleImageRoot() => Path.Combine(FindImageTestRoot(), "Apple II");

    private static string FindImageTestRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "image_test");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Le dossier local image_test est introuvable.");
    }
}
