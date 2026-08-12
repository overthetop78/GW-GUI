using GWGUI.MediaEngine.Definitions;
using System.IO;
using GWGUI.MediaEngine.Containers.Coherent;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Coherent;
using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie la lecture et la reconnaissance par contenu des images Coherent locales.</summary>
public sealed class CoherentDiskImageTests
{
    [Fact]
    public async Task Commodore900CoherentVolumeExposesRealDirectoryAndFiles()
    {
        var path = CoherentImagePath();
        var bytes = await File.ReadAllBytesAsync(path);
        var image = await new CoherentRawImageReader().ReadAsync(path);
        var volume = new CoherentFileSystemReader().Read(image);

        Assert.Equal(DiskImageFormatIds.Commodore900Coherent, image.FormatId);
        Assert.Equal(GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.Coherent, volume.FileSystemId);
        Assert.NotEmpty(volume.Entries);
        Assert.Contains(volume.Entries, entry => entry.Name == "coherent");
        Assert.All(volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
        Assert.True(CoherentSuperblockProbe.ReadValidatedFileSystemBlockCount(bytes) < image.BlockCount);
    }

    [Theory]
    [InlineData(0, 16)]
    [InlineData(38, 16)]
    [InlineData(39, 15)]
    [InlineData(52, 15)]
    [InlineData(53, 14)]
    [InlineData(63, 14)]
    [InlineData(64, 13)]
    [InlineData(79, 13)]
    public void Commodore900GeometryDefinesEveryZoneBoundary(int cylinder, int expectedSectors) => Assert.Equal(expectedSectors, Commodore900Geometry.SectorsPerTrack(cylinder));

    [Fact]
    public void CoherentProbeAcceptsTheDocumentedNamesAndCanonicalOrder()
    {
        var conventional = CreateCoherentDump(3, 3, CoherentSuperblockLayout.DefaultVolumeName, CoherentSuperblockLayout.DefaultPackName);
        var placeholders = CreateCoherentDump(3, 3, CoherentSuperblockLayout.PlaceholderName + CoherentSuperblockLayout.VolumePadding, CoherentSuperblockLayout.PlaceholderName + CoherentSuperblockLayout.PackPadding);
        Assert.True(CoherentSuperblockProbe.LooksLikeCoherent(conventional));
        Assert.True(CoherentSuperblockProbe.LooksLikeCoherent(placeholders));
        Assert.Equal(3, CoherentSuperblockProbe.ReadDeclaredFileSystemBlockCount(conventional));
        Assert.Equal(3u, CoherentCanonicalBinary.ReadUInt32(conventional.AsSpan(CoherentSuperblockLayout.FileSystemBlockCountOffset, CoherentCanonicalBinary.UInt32Length)));
    }

    [Fact]
    public async Task CoherentReaderRejectsInvalidSuperblocksAndGeometry()
    {
        await Assert.ThrowsAsync<InvalidDataException>(() => new CoherentRawImageReader().ReadAsync(new byte[CoherentSuperblockLayout.MinimumImageSize - 1]));
        await Assert.ThrowsAsync<InvalidDataException>(() => new CoherentRawImageReader().ReadAsync(CreateCoherentDump(3, 2)));
        await Assert.ThrowsAsync<InvalidDataException>(() => new CoherentRawImageReader().ReadAsync(CreateCoherentDump(3, 4)));
        await Assert.ThrowsAsync<InvalidDataException>(() => new CoherentRawImageReader().ReadAsync(CreateCoherentDump(Commodore900Geometry.BlockCount + 1, 3)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Commodore900Geometry.SectorsPerTrack(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Commodore900Geometry.SectorsPerTrack(Commodore900Geometry.CylinderCount));
    }

    [Fact]
    public async Task RecognizesCoherentSuperblockWithAnUnusualExtensionAndPreservesGeometryAndContent()
    {
        var sourcePath = CoherentImagePath();
        var unusualPath = Path.Combine(Path.GetTempPath(), $"gwgui-coherent-{Guid.NewGuid():N}.unexpected");
        try
        {
            File.Copy(sourcePath, unusualPath);
            var expected = await new CoherentRawImageReader().ReadAsync(sourcePath);

            var explored = await DiskImageExplorer.CreateDefault().ExploreAsync(unusualPath);

            Assert.Equal(DiskImageFormatIds.Commodore900Coherent, explored.Image.FormatId);
            Assert.Equal(expected.BlockSize, explored.Image.BlockSize);
            Assert.Equal(expected.Cylinders, explored.Image.Cylinders);
            Assert.Equal(expected.Heads, explored.Image.Heads);
            Assert.Equal(expected.SectorsPerTrack, explored.Image.SectorsPerTrack);
            Assert.Equal(expected.BlockCount, explored.Image.BlockCount);
            Assert.Equal(expected.Capacity, explored.Image.Capacity);
            foreach (var logicalBlock in new[] { 0, 15, 16, 31, 32, expected.BlockCount - 1 })
            {
                Assert.True(expected.TryGetBlock(logicalBlock, out var expectedBlock));
                Assert.True(explored.Image.TryGetBlock(logicalBlock, out var actualBlock));
                Assert.Equal(expectedBlock.Address, actualBlock.Address);
                Assert.Equal(expectedBlock.Data, actualBlock.Data);
            }
        }
        finally
        {
            if (File.Exists(unusualPath)) File.Delete(unusualPath);
        }
    }

    [Fact]
    public async Task CoherentReaderRejectsSameSizedContentWithoutSuperblockAndRegistryContinues()
    {
        var sourceLength = new FileInfo(CoherentImagePath()).Length;
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-not-coherent-{Guid.NewGuid():N}.bin");
        try
        {
            await using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                stream.SetLength(sourceLength);
            var coherentCandidate = new CoherentReaderCandidatePolicy();
            var fallback = new AcceptedPolicy();
            var registry = new DiskImageRecognitionRegistry([coherentCandidate, fallback]);

            var image = await registry.ReadAsync(path, null, CancellationToken.None);

            Assert.Equal("fallback", image.FormatId);
            Assert.Equal(1, coherentCandidate.ReadCalls);
            Assert.Equal(1, fallback.ReadCalls);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static string CoherentImagePath()
    {
        var path = Directory.EnumerateFiles(
                FindImageTestRoot(),
                "COHERENT - Volume 1 - High Resolution.bin",
                SearchOption.AllDirectories)
            .FirstOrDefault();
        return path ?? throw new FileNotFoundException("L'image Coherent locale requise est absente.");
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

    /// <summary>Crée un dump COHERENT minimal avec les champs canoniques demandés.</summary>
    private static byte[] CreateCoherentDump(int availableBlocks, int declaredBlocks, string? volumeName = null, string? packName = null)
    {
        var bytes = new byte[availableBlocks * CoherentSuperblockLayout.BlockSize];
        WriteCanonicalUInt32(bytes.AsSpan(CoherentSuperblockLayout.FileSystemBlockCountOffset, CoherentCanonicalBinary.UInt32Length), checked((uint)declaredBlocks));
        System.Text.Encoding.ASCII.GetBytes(volumeName ?? CoherentSuperblockLayout.DefaultVolumeName).CopyTo(bytes, CoherentSuperblockLayout.VolumeNameOffset);
        System.Text.Encoding.ASCII.GetBytes(packName ?? CoherentSuperblockLayout.DefaultPackName).CopyTo(bytes, CoherentSuperblockLayout.PackNameOffset);
        return bytes;
    }

    /// <summary>Écrit un entier 32 bits dans l'ordre canonique COHERENT 2, 3, 0, 1.</summary>
    private static void WriteCanonicalUInt32(Span<byte> destination, uint value)
    {
        destination[2] = (byte)value;
        destination[3] = (byte)(value >> 8);
        destination[0] = (byte)(value >> 16);
        destination[1] = (byte)(value >> 24);
    }

    /// <summary>Présélectionne le fichier puis délègue sa validation au lecteur Coherent public.</summary>
    private sealed class CoherentReaderCandidatePolicy : IDiskImageRecognitionPolicy
    {
        /// <summary>Nombre de lectures Coherent tentées.</summary>
        public int ReadCalls { get; private set; }

        /// <summary>Présélectionne le candidat de même taille.</summary>
        public ValueTask<bool> CanReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken) => ValueTask.FromResult(true);

        /// <summary>Tente la lecture Coherent, qui doit rejeter le faux superbloc.</summary>
        public Task<SectorImage> ReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken)
        {
            ReadCalls++;
            return new CoherentRawImageReader().ReadAsync(context.Path, cancellationToken);
        }
    }

    /// <summary>Politique suivante prouvant la poursuite du registre après le rejet Coherent.</summary>
    private sealed class AcceptedPolicy : IDiskImageRecognitionPolicy
    {
        /// <summary>Nombre de lectures de secours effectuées.</summary>
        public int ReadCalls { get; private set; }

        /// <summary>Accepte le candidat transmis par le registre.</summary>
        public ValueTask<bool> CanReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken) => ValueTask.FromResult(true);

        /// <summary>Produit l'image minimale attendue.</summary>
        public Task<SectorImage> ReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken)
        {
            ReadCalls++;
            return Task.FromResult(new SectorImage(
                "fallback",
                1,
                1,
                1,
                1,
                [new SectorBlock(0, new SectorAddress(0, 0, 1), new byte[] { 0x01 })]));
        }
    }
}
