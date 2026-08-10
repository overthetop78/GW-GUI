using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.Tests;

public sealed class ScpReaderDeterministicTests
{
    private static readonly Lazy<TestImages> Images = new(CreateTestImages);

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, 1)]
    [InlineData(166, 83, 0)]
    [InlineData(167, 83, 1)]
    public void ConvertsScpTrackNumbersToCylinderAndHead(int trackNumber, int expectedCylinder, int expectedHead)
    {
        var address = ScpFormatConstants.ToTrackAddress(trackNumber);

        Assert.Equal(expectedCylinder, address.Cylinder);
        Assert.Equal(expectedHead, address.Head);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(ScpFormatConstants.FloppyTrackSlots)]
    public void RejectsScpTrackNumbersOutsideTheTrackTable(int trackNumber) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => ScpFormatConstants.ToTrackAddress(trackNumber));

    [Fact]
    public void ComputesUpdatesAndValidatesScpChecksums()
    {
        byte[] first = [byte.MaxValue, 1];
        byte[] second = [2, 3];
        var checksum = ScpFormatConstants.UpdateChecksum(
            ScpFormatConstants.ComputeChecksum(first),
            second);

        Assert.Equal(261u, checksum);
        Assert.True(ScpFormatConstants.IsChecksumValid(checksum, 0, checksum));
        Assert.False(ScpFormatConstants.IsChecksumValid(checksum + 1, 0, checksum));
        Assert.True(ScpFormatConstants.IsChecksumValid(0, ScpFlags.Writable, checksum));
        Assert.False(ScpFormatConstants.IsChecksumValid(0, ScpFlags.IndexAligned, checksum));
    }

    [Fact]
    public void ConvertsScpVersionResolutionDurationAndRotationSpeed()
    {
        var header = new ScpHeader(0x25, 0, 1, 0, 0, (ScpFlags)0, 0, 0, 1, 0);
        var revolution = new ScpRevolution(4_000_000, 0, []);

        Assert.Equal("2.5", header.VersionText);
        Assert.Equal(50, header.ResolutionNanoseconds);
        Assert.Equal(200d, revolution.DurationMilliseconds(header.ResolutionNanoseconds));
        Assert.Equal(300d, revolution.Rpm(header.ResolutionNanoseconds));
    }

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
        var firstTrackOffset = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(modifiedBytes.AsSpan(
            ScpFormatConstants.TrackTableOffset,
            ScpFormatConstants.TrackTableEntrySize)));
        var firstFluxOffset = firstTrackOffset
            + ScpFormatConstants.TrackDescriptorHeaderSize
            + 2 * ScpFormatConstants.RevolutionDescriptorSize;
        BinaryPrimitives.WriteUInt16BigEndian(
            modifiedBytes.AsSpan(firstFluxOffset, ScpFormatConstants.FluxIntervalSize),
            101);
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

    [Theory]
    [InlineData("invalid-track-signature", "Track 0", "track number 0")]
    [InlineData("invalid-track-number", "entry 0", "track 1")]
    public async Task ReportsExpectedAndObservedTrackNumbers(string variant, string expectedText, string observedText)
    {
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            new ScpReader().ReadAsync(Images.Value.Invalid[variant]));

        Assert.Contains(expectedText, exception.Message, StringComparison.Ordinal);
        Assert.Contains(observedText, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReportsIncompleteSectionOffsetAndRequiredLength()
    {
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            new ScpReader().ReadAsync(Images.Value.Invalid["invalid-offset"]));
        var invalidOffset = checked((int)new FileInfo(Images.Value.Valid).Length + 128);
        var requiredLength = ScpFormatConstants.TrackDescriptorHeaderSize
            + 2 * ScpFormatConstants.RevolutionDescriptorSize;

        Assert.Contains("track 0 header", exception.Message, StringComparison.Ordinal);
        Assert.Contains($"offset {invalidOffset}", exception.Message, StringComparison.Ordinal);
        Assert.Contains($"{requiredLength} bytes", exception.Message, StringComparison.Ordinal);
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
            ["invalid-header"] = WriteVariant(outputDirectory, "invalid-header.scp", validBytes,
                bytes => bytes[ScpFormatConstants.RevolutionCountOffset] = 0),
            ["invalid-range"] = WriteVariant(outputDirectory, "invalid-range.scp", validBytes,
                bytes =>
                {
                    bytes[ScpFormatConstants.StartTrackOffset] = 4;
                    bytes[ScpFormatConstants.EndTrackOffset] = 3;
                }),
            ["invalid-offset"] = WriteVariant(outputDirectory, "invalid-offset.scp", validBytes,
                bytes => BinaryPrimitives.WriteUInt32LittleEndian(
                    bytes.AsSpan(ScpFormatConstants.TrackTableOffset, ScpFormatConstants.TrackTableEntrySize),
                    checked((uint)bytes.Length + 128))),
            ["invalid-track-signature"] = WriteVariant(outputDirectory, "invalid-track-signature.scp", validBytes,
                bytes => bytes[FirstTrackOffset] = (byte)'X'),
            ["invalid-track-number"] = WriteVariant(outputDirectory, "invalid-track-number.scp", validBytes,
                bytes => bytes[FirstTrackOffset + ScpFormatConstants.TrackNumberOffset] = 1),
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
        var firstTrackOffset = FirstTrackOffset;
        var secondTrackOffset = firstTrackOffset + firstTrack.Length;
        var data = new byte[secondTrackOffset + secondTrack.Length];

        ScpFormatConstants.FileSignature.CopyTo(data);
        data[ScpFormatConstants.VersionOffset] = 0x25;
        data[ScpFormatConstants.RevolutionCountOffset] = 2;
        data[ScpFormatConstants.StartTrackOffset] = 0;
        data[ScpFormatConstants.EndTrackOffset] = 3;
        data[ScpFormatConstants.FlagsOffset] = (byte)ScpFlags.IndexAligned;
        data[ScpFormatConstants.HeadsOffset] = 0;
        data[ScpFormatConstants.ResolutionOffset] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(
            data.AsSpan(ScpFormatConstants.TrackTableOffset, ScpFormatConstants.TrackTableEntrySize),
            checked((uint)firstTrackOffset));
        BinaryPrimitives.WriteUInt32LittleEndian(
            data.AsSpan(
                ScpFormatConstants.TrackTableOffset + 3 * ScpFormatConstants.TrackTableEntrySize,
                ScpFormatConstants.TrackTableEntrySize),
            checked((uint)secondTrackOffset));
        firstTrack.CopyTo(data, firstTrackOffset);
        secondTrack.CopyTo(data, secondTrackOffset);
        WriteChecksum(data);
        return data;
    }

    private static byte[] BuildTrack(byte trackNumber, IReadOnlyList<ushort[]> revolutions)
    {
        var descriptorLength = ScpFormatConstants.TrackDescriptorHeaderSize
            + revolutions.Count * ScpFormatConstants.RevolutionDescriptorSize;
        var track = new byte[descriptorLength + revolutions.Sum(words => words.Length * ScpFormatConstants.FluxIntervalSize)];
        ScpFormatConstants.TrackSignature.CopyTo(track);
        track[ScpFormatConstants.TrackNumberOffset] = trackNumber;
        var dataOffset = descriptorLength;
        for (var revolution = 0; revolution < revolutions.Count; revolution++)
        {
            var words = revolutions[revolution];
            var descriptor = ScpFormatConstants.TrackDescriptorHeaderSize
                + revolution * ScpFormatConstants.RevolutionDescriptorSize;
            BinaryPrimitives.WriteUInt32LittleEndian(
                track.AsSpan(descriptor + ScpFormatConstants.RevolutionIndexTimeOffset, sizeof(uint)),
                checked((uint)(8_000_000 + revolution)));
            BinaryPrimitives.WriteUInt32LittleEndian(
                track.AsSpan(descriptor + ScpFormatConstants.RevolutionFluxCountOffset, sizeof(uint)),
                checked((uint)words.Length));
            BinaryPrimitives.WriteUInt32LittleEndian(
                track.AsSpan(descriptor + ScpFormatConstants.RevolutionDataOffset, sizeof(uint)),
                checked((uint)dataOffset));
            foreach (var word in words)
            {
                BinaryPrimitives.WriteUInt16BigEndian(
                    track.AsSpan(dataOffset, ScpFormatConstants.FluxIntervalSize),
                    word);
                dataOffset += ScpFormatConstants.FluxIntervalSize;
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
        var checksum = ScpFormatConstants.ComputeChecksum(data.AsSpan(ScpFormatConstants.TrackTableOffset));
        BinaryPrimitives.WriteUInt32LittleEndian(
            data.AsSpan(ScpFormatConstants.ChecksumOffset, ScpFormatConstants.ChecksumLength),
            checksum);
    }

    private static int FirstTrackOffset => ScpFormatConstants.TrackTableOffset
        + ScpFormatConstants.FloppyTrackSlots * ScpFormatConstants.TrackTableEntrySize;

    private sealed record TestImages(
        string Valid,
        string Cache,
        string CorruptedChecksum,
        IReadOnlyDictionary<string, string> Invalid);
}
