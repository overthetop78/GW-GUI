using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.Tests;

public sealed class ScpReaderDeterministicTests
{
    private static readonly Lazy<TestImages> Images = new(CreateTestImages);

    [Fact]
    public void PreservesScpBinaryLayoutValuesDerivedFromFieldSizes()
    {
        Assert.Equal(3, ScpFormatConstants.VersionOffset);
        Assert.Equal(16, ScpFormatConstants.HeaderLength);
        Assert.Equal(16, ScpFormatConstants.TrackTableOffset);
        Assert.Equal(3, ScpFormatConstants.TrackNumberOffset);
        Assert.Equal(0, ScpFormatConstants.RevolutionIndexTimeOffset);
        Assert.Equal(4, ScpFormatConstants.RevolutionFluxCountOffset);
        Assert.Equal(8, ScpFormatConstants.RevolutionDataOffset);
        Assert.Equal(12, ScpFormatConstants.RevolutionDescriptorSize);
        Assert.Equal(4, ScpFormatConstants.TrackTableEntrySize);
        Assert.Equal(2, ScpFormatConstants.FluxIntervalSize);
        Assert.Equal(65_536u, ScpFormatConstants.ZeroFluxIntervalOverflow);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, 1)]
    [InlineData(166, 83, 0)]
    [InlineData(167, 83, 1)]
    public void ConvertsScpTrackNumbersToCylinderAndHead(int trackNumber, int expectedCylinder, int expectedHead)
    {
        var address = ScpFormatAlgorithms.ToTrackAddress(trackNumber);

        Assert.Equal(expectedCylinder, address.Cylinder);
        Assert.Equal(expectedHead, address.Head);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(ScpFormatConstants.FloppyTrackSlots)]
    public void RejectsScpTrackNumbersOutsideTheTrackTable(int trackNumber) => Assert.Throws<ArgumentOutOfRangeException>(() => ScpFormatAlgorithms.ToTrackAddress(trackNumber));

    [Fact]
    public void ComputesUpdatesAndValidatesScpChecksums()
    {
        byte[] first = [byte.MaxValue, 1];
        byte[] second = [2, 3];
        var checksum = ScpFormatAlgorithms.UpdateChecksum(ScpFormatAlgorithms.ComputeChecksum(first), second);

        Assert.Equal(261u, checksum);
        Assert.True(ScpFormatAlgorithms.IsChecksumValid(checksum, ScpFlags.None, checksum));
        Assert.False(ScpFormatAlgorithms.IsChecksumValid(checksum + 1, ScpFlags.None, checksum));
        Assert.True(ScpFormatAlgorithms.IsChecksumValid(ScpFormatConstants.MissingChecksum, ScpFlags.Writable, checksum));
        Assert.False(ScpFormatAlgorithms.IsChecksumValid(ScpFormatConstants.MissingChecksum, ScpFlags.IndexAligned, checksum));
        Assert.Equal(0u, ScpFormatAlgorithms.UpdateChecksum(uint.MaxValue, [1]));
    }

    [Theory]
    [InlineData(ScpFlags.None)]
    [InlineData(ScpFlags.IndexAligned)]
    [InlineData(ScpFlags.Tpi96)]
    [InlineData(ScpFlags.Rpm360)]
    [InlineData(ScpFlags.Normalized)]
    [InlineData(ScpFlags.Writable)]
    [InlineData(ScpFlags.Footer)]
    [InlineData(ScpFlags.Extended)]
    [InlineData(ScpFlags.ThirdPartyCreator)]
    [InlineData(ScpFlags.IndexAligned | ScpFlags.Writable)]
    [InlineData(ScpFlags.Tpi96 | ScpFlags.Rpm360 | ScpFlags.Normalized)]
    [InlineData(ScpFlags.IndexAligned | ScpFlags.Tpi96 | ScpFlags.Rpm360 | ScpFlags.Normalized |
                ScpFlags.Writable | ScpFlags.Footer | ScpFlags.Extended | ScpFlags.ThirdPartyCreator)]
    public void ReadsIndividualAndCombinedScpFlags(ScpFlags expectedFlags)
    {
        var data = new byte[ScpFormatConstants.HeaderLength];
        ScpFormatConstants.FileSignature.CopyTo(data);
        data[ScpFormatConstants.RevolutionCountOffset] = ScpFormatConstants.MinimumRevolutionCount;
        data[ScpFormatConstants.FlagsOffset] = (byte)expectedFlags;

        var header = ScpReader.ReadHeader(data);

        Assert.Equal(expectedFlags, header.Flags);
    }

    [Theory]
    [InlineData(ScpBitCellEncoding.Default16Bit)]
    [InlineData(ScpBitCellEncoding.Explicit16Bit)]
    public void ReadsEachSupportedScpBitCellEncoding(ScpBitCellEncoding expectedEncoding)
    {
        var data = CreateValidHeader();
        data[ScpFormatConstants.BitCellWidthOffset] = (byte)expectedEncoding;

        var header = ScpReader.ReadHeader(data);

        Assert.Equal(expectedEncoding, header.BitCellEncoding);
    }

    [Theory]
    [InlineData(ScpHeadSelection.Both)]
    [InlineData(ScpHeadSelection.Side0)]
    [InlineData(ScpHeadSelection.Side1)]
    public void ReadsEachSupportedScpHeadSelection(ScpHeadSelection expectedHeads)
    {
        var data = CreateValidHeader();
        data[ScpFormatConstants.HeadsOffset] = (byte)expectedHeads;

        var header = ScpReader.ReadHeader(data);

        Assert.Equal(expectedHeads, header.Heads);
    }

    [Fact]
    public void RejectsEveryUnsupportedScpBitCellEncoding()
    {
        var supported = Enum.GetValues<ScpBitCellEncoding>().Select(value => (byte)value).ToHashSet();
        foreach (var value in Enumerable.Range(byte.MinValue, byte.MaxValue + 1).Select(value => (byte)value).Where(value => !supported.Contains(value)))
        {
            var data = CreateValidHeader();
            data[ScpFormatConstants.BitCellWidthOffset] = value;

            Assert.Throws<NotSupportedException>(() => ScpReader.ReadHeader(data));
        }
    }

    [Fact]
    public void RejectsEveryUnsupportedScpHeadSelection()
    {
        var supported = Enum.GetValues<ScpHeadSelection>().Select(value => (byte)value).ToHashSet();
        foreach (var value in Enumerable.Range(byte.MinValue, byte.MaxValue + 1).Select(value => (byte)value).Where(value => !supported.Contains(value)))
        {
            var data = CreateValidHeader();
            data[ScpFormatConstants.HeadsOffset] = value;

            Assert.Throws<InvalidDataException>(() => ScpReader.ReadHeader(data));
        }
    }

    [Fact]
    public void ConvertsScpVersionResolutionDurationAndRotationSpeed()
    {
        var header = new ScpHeader(0x25, 0, 1, 0, 0, ScpFlags.None, ScpBitCellEncoding.Default16Bit, ScpHeadSelection.Both, 1, 0);
        var revolution = new ScpRevolution(4_000_000, 0, []);

        Assert.Equal("2.5", header.VersionText);
        Assert.Equal(50, header.ResolutionNanoseconds);
        Assert.Equal(200d, revolution.DurationMilliseconds(header.ResolutionNanoseconds));
        Assert.Equal(300d, revolution.Rpm(header.ResolutionNanoseconds));
    }

    [Fact]
    public void ReadsEveryScpHeaderFieldWithoutChangingItsValue()
    {
        var data = CreateValidHeader();
        data[ScpFormatConstants.VersionOffset] = 0x31;
        data[ScpFormatConstants.DiskTypeOffset] = 7;
        data[ScpFormatConstants.RevolutionCountOffset] = 3;
        data[ScpFormatConstants.StartTrackOffset] = 4;
        data[ScpFormatConstants.EndTrackOffset] = 9;
        data[ScpFormatConstants.FlagsOffset] = (byte)(ScpFlags.IndexAligned | ScpFlags.Writable);
        data[ScpFormatConstants.BitCellWidthOffset] = (byte)ScpBitCellEncoding.Explicit16Bit;
        data[ScpFormatConstants.HeadsOffset] = (byte)ScpHeadSelection.Side1;
        data[ScpFormatConstants.ResolutionOffset] = 2;
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(ScpFormatConstants.ChecksumOffset, ScpFormatConstants.ChecksumLength), 123_456u);

        var header = ScpReader.ReadHeader(data);

        Assert.Equal(new ScpHeader(0x31, 7, 3, 4, 9, ScpFlags.IndexAligned | ScpFlags.Writable, ScpBitCellEncoding.Explicit16Bit, ScpHeadSelection.Side1, 2, 123_456u), header);
    }

    [Fact]
    public void ProtectsScpRevolutionFluxIntervalsFromSourceChanges()
    {
        var source = new List<uint> { 100, 200 };
        var revolution = new ScpRevolution(4_000_000, 2, source);

        source[0] = 999;
        source.Add(300);

        Assert.Equal(new uint[] { 100, 200 }, revolution.FluxIntervals);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositiveResolutionForScpRevolutionCalculations(int resolutionNanoseconds)
    {
        var revolution = new ScpRevolution(4_000_000, 0, []);

        var durationException = Assert.Throws<ArgumentOutOfRangeException>(() => revolution.DurationMilliseconds(resolutionNanoseconds));
        var rpmException = Assert.Throws<ArgumentOutOfRangeException>(() => revolution.Rpm(resolutionNanoseconds));
        Assert.Equal(resolutionNanoseconds, durationException.ActualValue);
        Assert.Equal(resolutionNanoseconds, rpmException.ActualValue);
    }

    [Fact]
    public void ProtectsScpTrackRevolutionsAndPreservesItsAddress()
    {
        var first = new ScpRevolution(4_000_000, 0, []);
        var source = new List<ScpRevolution> { first };
        var track = new ScpTrack(11, 5, 1, source);

        source[0] = new ScpRevolution(8_000_000, 0, []);
        source.Clear();

        Assert.Equal((byte)11, track.TrackNumber);
        Assert.Equal(5, track.Cylinder);
        Assert.Equal(1, track.Head);
        Assert.Same(first, Assert.Single(track.Revolutions));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ProtectsScpImageTracksAndPreservesChecksumState(bool checksumValid)
    {
        var header = ScpReader.ReadHeader(CreateValidHeader());
        var first = new ScpTrack(0, 0, 0, []);
        var source = new List<ScpTrack> { first };
        var image = new ScpImage(header, source, checksumValid, 1024);

        source[0] = new ScpTrack(1, 0, 1, []);
        source.Clear();

        Assert.Same(header, image.Header);
        Assert.Same(first, Assert.Single(image.Tracks));
        Assert.Equal(checksumValid, image.ChecksumValid);
        Assert.Equal(1024, image.FileSize);
    }

    [Fact]
    public void RejectsNegativeScpImageFileSize()
    {
        var header = ScpReader.ReadHeader(CreateValidHeader());

        Assert.Throws<ArgumentOutOfRangeException>(() => new ScpImage(header, [], true, -1));
    }

    [Fact]
    public void RejectsNullScpImageHeader() => Assert.Throws<ArgumentNullException>(() => new ScpImage(null!, [], true, 0));

    [Fact]
    public async Task ReadsTrackDescriptorsAndTheirRevolutions()
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
        Assert.Equal(new uint[] { 120, 140 }, first.Revolutions[1].FluxIntervals);

        var second = image.Tracks[1];
        Assert.Equal(1, second.Cylinder);
        Assert.Equal(1, second.Head);
        Assert.Equal(new uint[] { 300, 400 }, second.Revolutions[0].FluxIntervals);
        Assert.Equal(new uint[] { 500, 600, 700 }, second.Revolutions[1].FluxIntervals);
        Assert.Equal(new FileInfo(Images.Value.Valid).Length, image.FileSize);
    }

    [Fact]
    public void IgnoresOnlyTheTrackWhoseTableOffsetIsMissing()
    {
        var data = File.ReadAllBytes(Images.Value.Valid);
        data.AsSpan(ScpFormatConstants.TrackTableOffset, ScpFormatConstants.TrackTableEntrySize).Clear();
        WriteChecksum(data);

        var image = new ScpReader().Read(data);

        var track = Assert.Single(image.Tracks);
        Assert.Equal((byte)3, track.TrackNumber);
        Assert.Equal(1, track.Cylinder);
        Assert.Equal(1, track.Head);
        Assert.True(image.ChecksumValid);
        Assert.Equal(data.Length, image.FileSize);
    }

    [Fact]
    public async Task DecodesScpFluxOverflowMarkers()
    {
        var image = await new ScpReader().ReadAsync(Images.Value.Valid);

        Assert.Equal(new uint[] { 100, 65_556, 200 }, image.Tracks[0].Revolutions[0].FluxIntervals);
    }

    [Fact]
    public async Task ReusesUnchangedFileFromCache()
    {
        var images = Images.Value;
        var reader = new ScpReader();
        File.WriteAllBytes(images.Cache, File.ReadAllBytes(images.Valid));

        var first = await reader.ReadAsync(images.Cache);
        var unchanged = await reader.ReadAsync(images.Cache);

        Assert.Same(first, unchanged);
    }

    [Fact]
    public async Task SharesOneCachedImageBetweenConcurrentReads()
    {
        var path = Path.Combine(Path.GetDirectoryName(Images.Value.Cache)!, "concurrent-cache.scp");
        File.WriteAllBytes(path, File.ReadAllBytes(Images.Value.Valid));
        var reader = new ScpReader();

        var images = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => reader.ReadAsync(path)));

        Assert.All(images, image => Assert.Same(images[0], image));
    }

    [Fact]
    public async Task UsesOneCacheEntryForDifferentPathCasing()
    {
        var path = Path.Combine(Path.GetDirectoryName(Images.Value.Cache)!, "case-cache.scp");
        File.WriteAllBytes(path, File.ReadAllBytes(Images.Value.Valid));
        var reader = new ScpReader();

        var first = await reader.ReadAsync(path.ToLowerInvariant());
        var second = await reader.ReadAsync(path.ToUpperInvariant());

        Assert.Same(first, second);
    }

    [Fact]
    public async Task CallerCancellationDoesNotRemoveTheSharedLoadingEntry()
    {
        var path = Path.Combine(Path.GetDirectoryName(Images.Value.Cache)!, "cancelled-wait-cache.scp");
        var data = File.ReadAllBytes(Images.Value.Valid);
        Array.Resize(ref data, data.Length + 64 * 1024 * 1024);
        data[ScpFormatConstants.FlagsOffset] |= (byte)ScpFlags.Writable;
        data.AsSpan(ScpFormatConstants.ChecksumOffset, ScpFormatConstants.ChecksumLength).Clear();
        File.WriteAllBytes(path, data);
        var reader = new ScpReader();

        var sharedLoad = reader.ReadAsync(path);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reader.ReadAsync(path, cancellation.Token));
        var loaded = await sharedLoad;
        var cached = await reader.ReadAsync(path);

        Assert.Same(loaded, cached);
    }

    [Fact]
    public async Task RemovesFailedLoadBeforeRetryingTheSameFileIdentity()
    {
        var path = Path.Combine(Path.GetDirectoryName(Images.Value.Cache)!, "failed-cache.scp");
        var valid = File.ReadAllBytes(Images.Value.Valid);
        var invalid = (byte[])valid.Clone();
        invalid[ScpFormatConstants.FileStartOffset] = (byte)'X';
        File.WriteAllBytes(path, invalid);
        var writeTime = File.GetLastWriteTimeUtc(path);
        var reader = new ScpReader();

        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(path));
        File.WriteAllBytes(path, valid);
        File.SetLastWriteTimeUtc(path, writeTime);
        var loaded = await reader.ReadAsync(path);

        Assert.Equal(new byte[] { 0, 3 }, loaded.Tracks.Select(track => track.TrackNumber));
    }

    [Fact]
    public async Task InvalidatesCacheWhenFileChanges()
    {
        var images = Images.Value;
        var reader = new ScpReader();
        File.WriteAllBytes(images.Cache, File.ReadAllBytes(images.Valid));
        var initialWriteTime = File.GetLastWriteTimeUtc(images.Cache);

        var first = await reader.ReadAsync(images.Cache);
        var modifiedBytes = File.ReadAllBytes(images.Cache);
        var firstTrackOffset = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(modifiedBytes.AsSpan(ScpFormatConstants.TrackTableOffset, ScpFormatConstants.TrackTableEntrySize)));
        var firstFluxOffset = firstTrackOffset + ScpFormatConstants.TrackDescriptorHeaderSize + 2 * ScpFormatConstants.RevolutionDescriptorSize;
        BinaryPrimitives.WriteUInt16BigEndian(modifiedBytes.AsSpan(firstFluxOffset, ScpFormatConstants.FluxIntervalSize), 101);
        WriteChecksum(modifiedBytes);
        File.WriteAllBytes(images.Cache, modifiedBytes);
        File.SetLastWriteTimeUtc(images.Cache, initialWriteTime.AddSeconds(2));

        var modified = await reader.ReadAsync(images.Cache);

        Assert.NotSame(first, modified);
        Assert.Equal(101u, modified.Tracks[0].Revolutions[0].FluxIntervals[0]);
    }

    [Fact]
    public async Task InvalidatesCacheWhenFileSizeChanges()
    {
        var path = Path.Combine(Path.GetDirectoryName(Images.Value.Cache)!, "size-cache.scp");
        var data = File.ReadAllBytes(Images.Value.Valid);
        File.WriteAllBytes(path, data);
        var reader = new ScpReader();
        var first = await reader.ReadAsync(path);

        Array.Resize(ref data, data.Length + 1);
        data[ScpFormatConstants.FlagsOffset] |= (byte)ScpFlags.Writable;
        data.AsSpan(ScpFormatConstants.ChecksumOffset, ScpFormatConstants.ChecksumLength).Clear();
        File.WriteAllBytes(path, data);
        var resized = await reader.ReadAsync(path);

        Assert.NotSame(first, resized);
        Assert.Equal(data.Length, resized.FileSize);
    }

    [Theory]
    [InlineData("invalid-signature")]
    [InlineData("invalid-header")]
    [InlineData("invalid-range")]
    [InlineData("invalid-offset")]
    [InlineData("invalid-track-signature")]
    public async Task RejectsInvalidContainerStructures(string variant)
    {
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new ScpReader().ReadAsync(Images.Value.Invalid[variant]));
    }

    [Fact]
    public async Task RejectsTruncatedScpRevolutionFlux()
    {
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => new ScpReader().ReadAsync(Images.Value.Invalid["truncated-flux"]));

        Assert.Contains("track 3, revolution 2 flux", exception.Message, StringComparison.Ordinal);
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

    private static byte[] CreateValidHeader()
    {
        var data = new byte[ScpFormatConstants.HeaderLength];
        ScpFormatConstants.FileSignature.CopyTo(data);
        data[ScpFormatConstants.RevolutionCountOffset] = ScpFormatConstants.MinimumRevolutionCount;
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
        var checksum = ScpFormatAlgorithms.ComputeChecksum(data.AsSpan(ScpFormatConstants.TrackTableOffset));
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(ScpFormatConstants.ChecksumOffset, ScpFormatConstants.ChecksumLength), checksum);
    }

    private static int FirstTrackOffset => ScpFormatConstants.TrackTableOffset
        + ScpFormatConstants.FloppyTrackSlots * ScpFormatConstants.TrackTableEntrySize;

    private sealed record TestImages(
        string Valid,
        string Cache,
        string CorruptedChecksum,
        IReadOnlyDictionary<string, string> Invalid);
}
