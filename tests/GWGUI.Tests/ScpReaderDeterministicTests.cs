using System.Buffers.Binary;
using System.IO;
using GWGUI.Scp;
using GWGUI.Scp.Containers.Scp;

namespace GWGUI.Tests;

public sealed class ScpReaderDeterministicTests
{
    private static readonly Lazy<TestImages> Images = new(CreateTestImages);

    [Fact]
    public async Task ReadsTwoTracksTwoRevolutionsAndFluxOverflow()
    {
        var image = await new ScpReader().ReadAsync(Images.Value.Valid);

        Assert.Equal(0x25, image.Header.Version);
        Assert.Equal(ScpFlags.IndexAligned, image.Header.Flags);
        Assert.Equal(0, image.Header.StartTrack);
        Assert.Equal(3, image.Header.EndTrack);
        Assert.Equal(2, image.Header.Revolutions);
        Assert.Equal(50, image.Header.ResolutionNanoseconds);
        Assert.True(image.ChecksumValid);
        Assert.Equal(new byte[] { 0, 3 }, image.Tracks.Select(track => track.TrackNumber));

        var first = image.Tracks[0];
        Assert.Equal(0, first.Cylinder);
        Assert.Equal(0, first.Head);
        Assert.Equal(2, first.Revolutions.Count);
        Assert.Equal(4u, first.Revolutions[0].DeclaredFluxCount);
        Assert.Equal(new uint[] { 100, 65_556, 200 }, first.Revolutions[0].FluxIntervals);
        Assert.Equal(new uint[] { 120, 140 }, first.Revolutions[1].FluxIntervals);

        var second = image.Tracks[1];
        Assert.Equal(1, second.Cylinder);
        Assert.Equal(1, second.Head);
        Assert.Equal(new uint[] { 300, 400 }, second.Revolutions[0].FluxIntervals);
        Assert.Equal(new uint[] { 500, 600, 700 }, second.Revolutions[1].FluxIntervals);
        Assert.Equal(new FileInfo(Images.Value.Valid).Length, image.FileSize);
    }

    [Fact]
    public async Task ReusesUnchangedFileAndInvalidatesModifiedFile()
    {
        var images = Images.Value;
        var reader = new ScpReader();
        File.WriteAllBytes(images.Cache, File.ReadAllBytes(images.Valid));
        var initialWriteTime = File.GetLastWriteTimeUtc(images.Cache);

        var first = await reader.ReadAsync(images.Cache);
        var unchanged = await reader.ReadAsync(images.Cache);

        Assert.Same(first, unchanged);

        var modifiedBytes = File.ReadAllBytes(images.Cache);
        var firstTrackOffset = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(modifiedBytes.AsSpan(16, 4)));
        BinaryPrimitives.WriteUInt16BigEndian(modifiedBytes.AsSpan(firstTrackOffset + 28, 2), 101);
        WriteChecksum(modifiedBytes);
        File.WriteAllBytes(images.Cache, modifiedBytes);
        File.SetLastWriteTimeUtc(images.Cache, initialWriteTime.AddSeconds(2));

        var modified = await reader.ReadAsync(images.Cache);

        Assert.NotSame(first, modified);
        Assert.Equal(101u, modified.Tracks[0].Revolutions[0].FluxIntervals[0]);
    }

    [Theory]
    [InlineData("invalid-signature")]
    [InlineData("invalid-header")]
    [InlineData("invalid-range")]
    [InlineData("invalid-offset")]
    [InlineData("invalid-track-signature")]
    [InlineData("truncated-flux")]
    public async Task RejectsInvalidContainerStructures(string variant)
    {
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new ScpReader().ReadAsync(Images.Value.Invalid[variant]));
    }

    [Fact]
    public async Task ReportsInvalidChecksumWithoutDiscardingTracks()
    {
        var image = await new ScpReader().ReadAsync(Images.Value.CorruptedChecksum);

        Assert.False(image.ChecksumValid);
        Assert.Equal(2, image.Tracks.Count);
    }

    private static TestImages CreateTestImages()
    {
        var outputDirectory = Path.Combine(FindImageTestRoot(), "_generated", "scp-reader");
        Directory.CreateDirectory(outputDirectory);

        var validBytes = BuildValidCapture();
        var valid = Write(outputDirectory, "two-tracks-two-revolutions.scp", validBytes);
        var cache = Write(outputDirectory, "cache.scp", validBytes);

        var corruptedChecksumBytes = (byte[])validBytes.Clone();
        corruptedChecksumBytes[^1] ^= 0xff;
        var corruptedChecksum = Write(outputDirectory, "invalid-checksum.scp", corruptedChecksumBytes);

        var invalid = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["invalid-signature"] = WriteVariant(outputDirectory, "invalid-signature.scp", validBytes, bytes => bytes[0] = (byte)'X'),
            ["invalid-header"] = WriteVariant(outputDirectory, "invalid-header.scp", validBytes, bytes => bytes[5] = 0),
            ["invalid-range"] = WriteVariant(outputDirectory, "invalid-range.scp", validBytes, bytes => { bytes[6] = 4; bytes[7] = 3; }),
            ["invalid-offset"] = WriteVariant(outputDirectory, "invalid-offset.scp", validBytes,
                bytes => BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(16, 4), checked((uint)bytes.Length + 128))),
            ["invalid-track-signature"] = WriteVariant(outputDirectory, "invalid-track-signature.scp", validBytes, bytes => bytes[688] = (byte)'X'),
            ["truncated-flux"] = Write(outputDirectory, "truncated-flux.scp", validBytes[..^2])
        };

        return new(valid, cache, corruptedChecksum, invalid);
    }

    private static byte[] BuildValidCapture()
    {
        var firstTrack = BuildTrack(0, [
            [100, 0, 20, 200],
            [120, 140]
        ]);
        var secondTrack = BuildTrack(3, [
            [300, 400],
            [500, 600, 700]
        ]);
        var firstTrackOffset = ScpFormatConstants.TrackTableOffset + ScpFormatConstants.FloppyTrackSlots * 4;
        var secondTrackOffset = firstTrackOffset + firstTrack.Length;
        var data = new byte[secondTrackOffset + secondTrack.Length];

        "SCP"u8.CopyTo(data);
        data[3] = 0x25;
        data[5] = 2;
        data[6] = 0;
        data[7] = 3;
        data[8] = (byte)ScpFlags.IndexAligned;
        data[10] = 0;
        data[11] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(16, 4), checked((uint)firstTrackOffset));
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(16 + 3 * 4, 4), checked((uint)secondTrackOffset));
        firstTrack.CopyTo(data, firstTrackOffset);
        secondTrack.CopyTo(data, secondTrackOffset);
        WriteChecksum(data);
        return data;
    }

    private static byte[] BuildTrack(byte trackNumber, IReadOnlyList<ushort[]> revolutions)
    {
        var descriptorLength = 4 + revolutions.Count * 12;
        var track = new byte[descriptorLength + revolutions.Sum(words => words.Length * 2)];
        track[0] = (byte)'T';
        track[1] = (byte)'R';
        track[2] = (byte)'K';
        track[3] = trackNumber;
        var dataOffset = descriptorLength;
        for (var revolution = 0; revolution < revolutions.Count; revolution++)
        {
            var words = revolutions[revolution];
            var descriptor = 4 + revolution * 12;
            BinaryPrimitives.WriteUInt32LittleEndian(track.AsSpan(descriptor, 4), checked((uint)(8_000_000 + revolution)));
            BinaryPrimitives.WriteUInt32LittleEndian(track.AsSpan(descriptor + 4, 4), checked((uint)words.Length));
            BinaryPrimitives.WriteUInt32LittleEndian(track.AsSpan(descriptor + 8, 4), checked((uint)dataOffset));
            foreach (var word in words)
            {
                BinaryPrimitives.WriteUInt16BigEndian(track.AsSpan(dataOffset, 2), word);
                dataOffset += 2;
            }
        }
        return track;
    }

    private static string WriteVariant(string directory, string fileName, byte[] source, Action<byte[]> change)
    {
        var bytes = (byte[])source.Clone();
        change(bytes);
        WriteChecksum(bytes);
        return Write(directory, fileName, bytes);
    }

    private static string Write(string directory, string fileName, byte[] bytes)
    {
        var path = Path.Combine(directory, fileName);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private static string FindImageTestRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "image_test");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Le dossier local image_test est introuvable.");
    }

    private static void WriteChecksum(byte[] data)
    {
        uint checksum = 0;
        foreach (var value in data.AsSpan(ScpFormatConstants.TrackTableOffset)) checksum = unchecked(checksum + value);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(12, 4), checksum);
    }

    private sealed record TestImages(
        string Valid,
        string Cache,
        string CorruptedChecksum,
        IReadOnlyDictionary<string, string> Invalid);
}
