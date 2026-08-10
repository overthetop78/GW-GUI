using System.Buffers.Binary;
using System.IO;
using GWGUI.Scp.Exploration;

namespace GWGUI.Tests;

public sealed class ScpCaptureInfoTests
{
    private static readonly Lazy<TestImages> Images = new(CreateTestImages);

    [Fact]
    public async Task ReadsMetadataFromRealLocalCapture()
    {
        var image = Images.Value;

        var info = await ScpCaptureInfoReader.ReadAsync(image.Source);

        Assert.Equal(84, info.CapturedTracks);
        Assert.Equal(0, info.MissingTracks);
        Assert.Equal(42, info.Cylinders);
        Assert.Equal(2, info.Sides);
        Assert.True(info.ChecksumValid);
        Assert.Equal(4_898_364, info.FileSize);
        Assert.Equal(0, info.Header.StartTrack);
        Assert.Equal(83, info.Header.EndTrack);
        Assert.Equal(1, info.Header.Revolutions);
    }

    [Fact]
    public async Task ReadsGeneratedCaptureWithOneMissingTrack()
    {
        var image = Images.Value;

        var info = await ScpCaptureInfoReader.ReadAsync(image.MissingTrack);

        Assert.Equal(83, info.CapturedTracks);
        Assert.Equal(1, info.MissingTracks);
        Assert.Equal(42, info.Cylinders);
        Assert.Equal(2, info.Sides);
        Assert.True(info.ChecksumValid);
        Assert.Equal(4_898_364, info.FileSize);
    }

    [Fact]
    public async Task ReportsCorruptedChecksum()
    {
        var info = await ScpCaptureInfoReader.ReadAsync(Images.Value.Corrupted);

        Assert.False(info.ChecksumValid);
    }

    [Fact]
    public async Task RejectsTruncatedTrackTable()
    {
        await Assert.ThrowsAsync<EndOfStreamException>(() =>
            ScpCaptureInfoReader.ReadAsync(Images.Value.Truncated));
    }

    [Fact]
    public async Task PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ScpCaptureInfoReader.ReadAsync(Images.Value.Source, cancellation.Token));
    }

    private static TestImages CreateTestImages()
    {
        var imageTestRoot = FindImageTestRoot();
        var source = Path.Combine(imageTestRoot, "IBM PC", "PFS File B01 (1985) (5.25-160k) disk02.scp");
        if (!File.Exists(source)) throw new FileNotFoundException("La capture SCP locale requise est absente.", source);

        var outputDirectory = Path.Combine(imageTestRoot, "_generated", "scp-capture-info");
        Directory.CreateDirectory(outputDirectory);
        var missingTrack = Path.Combine(outputDirectory, "pfs-file-disk02-missing-track.scp");
        var corrupted = Path.Combine(outputDirectory, "pfs-file-disk02-corrupted-checksum.scp");
        var truncated = Path.Combine(outputDirectory, "pfs-file-disk02-truncated-table.scp");

        var sourceBytes = File.ReadAllBytes(source);

        var missingBytes = (byte[])sourceBytes.Clone();
        BinaryPrimitives.WriteUInt32LittleEndian(missingBytes.AsSpan(16 + 83 * 4, 4), 0);
        WriteChecksum(missingBytes);
        File.WriteAllBytes(missingTrack, missingBytes);

        var corruptedBytes = (byte[])sourceBytes.Clone();
        corruptedBytes[^1] ^= 0xff;
        File.WriteAllBytes(corrupted, corruptedBytes);

        File.WriteAllBytes(truncated, sourceBytes.AsSpan(0, 100).ToArray());

        return new(source, missingTrack, corrupted, truncated);
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
        foreach (var value in data.AsSpan(16)) checksum = unchecked(checksum + value);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(12, 4), checksum);
    }

    private sealed record TestImages(string Source, string MissingTrack, string Corrupted, string Truncated);
}
