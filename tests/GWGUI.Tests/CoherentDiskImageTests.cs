using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Definitions;
using System.IO;
using GWGUI.MediaEngine.Containers.Coherent;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Coherent;
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
        var coherentEntry = Assert.Single(volume.Entries, entry => entry.Name == "coherent");
        Assert.True(coherentEntry.MetadataValid);
        Assert.NotNull(coherentEntry.Content);
        Assert.Equal(coherentEntry.Size, coherentEntry.Content!.Count);
        Assert.Equal((long)CoherentFormat.ReadDeclaredFileSystemBlockCount(bytes) * CoherentFileSystemLayout.BlockSize, volume.Capacity);
        Assert.InRange(volume.FreeBytes, 0, volume.Capacity);
        Assert.All(volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
        Assert.True(CoherentFormat.ReadValidatedFileSystemBlockCount(bytes) < image.BlockCount);
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
        var conventional = CreateCoherentDump(3, 3, CoherentFileSystemLayout.DefaultVolumeName, CoherentFileSystemLayout.DefaultPackName);
        var placeholders = CreateCoherentDump(3, 3, CoherentFileSystemLayout.PlaceholderName + CoherentFileSystemLayout.VolumePadding, CoherentFileSystemLayout.PlaceholderName + CoherentFileSystemLayout.PackPadding);
        Assert.True(CoherentFormat.LooksLikeCoherent(conventional));
        Assert.True(CoherentFormat.LooksLikeCoherent(placeholders));
        Assert.Equal(3, CoherentFormat.ReadDeclaredFileSystemBlockCount(conventional));
        Assert.Equal(3u, CoherentFormat.ReadCanonicalUInt32(conventional.AsSpan(CoherentFileSystemLayout.FileSystemBlockCountOffset, CoherentFormat.UInt32Length)));
    }

    [Fact]
    public async Task CoherentReaderRejectsInvalidSuperblocksAndGeometry()
    {
        await Assert.ThrowsAsync<InvalidDataException>(() => new CoherentRawImageReader().ReadAsync(new byte[CoherentFileSystemLayout.MinimumImageSize - 1]));
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

    [Fact]
    public void CoherentFileDataPreservesThePositionOfAnUnavailableDirectBlock()
    {
        var first = Enumerable.Repeat((byte)0x11, CoherentFileSystemLayout.BlockSize).ToArray();
        var third = Enumerable.Repeat((byte)0x33, CoherentFileSystemLayout.BlockSize).ToArray();
        var image = CoherentImageData.Create(CreateSectorImage(7, new Dictionary<int, byte[]> { [3] = first, [5] = third }));
        var inode = new CoherentInode(0x8000, 3 * CoherentFileSystemLayout.BlockSize, new[] { 3, 4, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0);
        var warnings = new List<string>();

        var data = CoherentFileDataReader.Read(image, inode, warnings, "direct");

        Assert.False(data.IsValid);
        Assert.All(data.Content.AsSpan(0, CoherentFileSystemLayout.BlockSize).ToArray(), value => Assert.Equal(0x11, value));
        Assert.All(data.Content.AsSpan(CoherentFileSystemLayout.BlockSize, CoherentFileSystemLayout.BlockSize).ToArray(), value => Assert.Equal(0, value));
        Assert.All(data.Content.AsSpan(2 * CoherentFileSystemLayout.BlockSize, CoherentFileSystemLayout.BlockSize).ToArray(), value => Assert.Equal(0x33, value));
        Assert.Contains(warnings, warning => warning.Contains("4", StringComparison.Ordinal));
    }

    [Fact]
    public void CoherentFileDataPreservesAnUnavailableSingleIndirectExtentBeforeDoubleIndirectData()
    {
        var doubleRoot = new byte[CoherentFileSystemLayout.BlockSize];
        var singleLeaf = new byte[CoherentFileSystemLayout.BlockSize];
        var payload = Enumerable.Repeat((byte)0x5a, CoherentFileSystemLayout.BlockSize).ToArray();
        WriteCanonicalUInt32(doubleRoot, 2);
        WriteCanonicalUInt32(singleLeaf, 3);
        var image = CoherentImageData.Create(CreateSectorImage(5, new Dictionary<int, byte[]> { [1] = doubleRoot, [2] = singleLeaf, [3] = payload }));
        var pointers = new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 1, 0 };
        var logicalBlocks = CoherentFileSystemLayout.DirectPointerCount + CoherentFileSystemLayout.IndirectPointersPerBlock + 1;
        var warnings = new List<string>();

        var data = CoherentFileDataReader.Read(image, new CoherentInode(0x8000, checked((uint)(logicalBlocks * CoherentFileSystemLayout.BlockSize)), pointers, 0), warnings, "indirect");

        Assert.False(data.IsValid);
        var payloadOffset = (CoherentFileSystemLayout.DirectPointerCount + CoherentFileSystemLayout.IndirectPointersPerBlock) * CoherentFileSystemLayout.BlockSize;
        Assert.All(data.Content.AsSpan(payloadOffset, CoherentFileSystemLayout.BlockSize).ToArray(), value => Assert.Equal(0x5a, value));
    }

    [Fact]
    public void CoherentFileDataRejectsARepeatedIndirectBlock()
    {
        var repeated = new byte[CoherentFileSystemLayout.BlockSize];
        WriteCanonicalUInt32(repeated, 1);
        var image = CoherentImageData.Create(CreateSectorImage(3, new Dictionary<int, byte[]> { [1] = repeated }));
        var pointers = new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 };
        var logicalBlocks = CoherentFileSystemLayout.DirectPointerCount + CoherentFileSystemLayout.IndirectPointersPerBlock + 1;
        var warnings = new List<string>();

        var data = CoherentFileDataReader.Read(image, new CoherentInode(0x8000, checked((uint)(logicalBlocks * CoherentFileSystemLayout.BlockSize)), pointers, 0), warnings, "cycle");

        Assert.False(data.IsValid);
        Assert.Contains(warnings, warning => warning.Contains("plusieurs fois", StringComparison.Ordinal));
    }

    [Fact]
    public void CoherentFileDataReadsTenDirectBlocksAndAllThreeIndirectLevels()
    {
        var blocks = new Dictionary<int, byte[]>();
        var directPointers = new int[CoherentFileSystemLayout.InodePointerCount];
        for (var index = 0; index < CoherentFileSystemLayout.DirectPointerCount; index++)
        {
            directPointers[index] = index + 1;
            blocks[index + 1] = Enumerable.Repeat((byte)(index + 1), CoherentFileSystemLayout.BlockSize).ToArray();
        }
        var single = new byte[CoherentFileSystemLayout.BlockSize];
        var doubleRoot = new byte[CoherentFileSystemLayout.BlockSize];
        var doubleLeaf = new byte[CoherentFileSystemLayout.BlockSize];
        var tripleRoot = new byte[CoherentFileSystemLayout.BlockSize];
        var tripleMiddle = new byte[CoherentFileSystemLayout.BlockSize];
        var tripleLeaf = new byte[CoherentFileSystemLayout.BlockSize];
        WriteCanonicalUInt32(single, 17);
        WriteCanonicalUInt32(doubleRoot, 13);
        WriteCanonicalUInt32(doubleLeaf, 18);
        WriteCanonicalUInt32(tripleRoot, 15);
        WriteCanonicalUInt32(tripleMiddle, 16);
        WriteCanonicalUInt32(tripleLeaf, 19);
        blocks[11] = single;
        blocks[12] = doubleRoot;
        blocks[13] = doubleLeaf;
        blocks[14] = tripleRoot;
        blocks[15] = tripleMiddle;
        blocks[16] = tripleLeaf;
        blocks[17] = Enumerable.Repeat((byte)0x51, CoherentFileSystemLayout.BlockSize).ToArray();
        blocks[18] = Enumerable.Repeat((byte)0x52, CoherentFileSystemLayout.BlockSize).ToArray();
        blocks[19] = Enumerable.Repeat((byte)0x53, CoherentFileSystemLayout.BlockSize).ToArray();
        directPointers[CoherentFileSystemLayout.SingleIndirectPointerIndex] = 11;
        directPointers[CoherentFileSystemLayout.DoubleIndirectPointerIndex] = 12;
        directPointers[CoherentFileSystemLayout.TripleIndirectPointerIndex] = 14;
        var logicalBlocks = CoherentFileSystemLayout.DirectPointerCount + CoherentFileSystemLayout.IndirectPointersPerBlock + CoherentFileSystemLayout.IndirectPointersPerBlock * CoherentFileSystemLayout.IndirectPointersPerBlock + 1;
        var image = CoherentImageData.Create(CreateSectorImage(20, blocks));
        var warnings = new List<string>();

        var data = CoherentFileDataReader.Read(image, new CoherentInode(0x8000, checked((uint)(logicalBlocks * CoherentFileSystemLayout.BlockSize)), directPointers, 0), warnings, "levels");

        Assert.True(data.IsValid, string.Join(Environment.NewLine, warnings));
        Assert.Equal(0x51, data.Content[CoherentFileSystemLayout.DirectPointerCount * CoherentFileSystemLayout.BlockSize]);
        Assert.Equal(0x52, data.Content[(CoherentFileSystemLayout.DirectPointerCount + CoherentFileSystemLayout.IndirectPointersPerBlock) * CoherentFileSystemLayout.BlockSize]);
        Assert.Equal(0x53, data.Content[^CoherentFileSystemLayout.BlockSize]);
    }

    [Fact]
    public void CoherentDirectoryReaderReportsACycleAndASecondReference()
    {
        var superblock = new byte[2 * CoherentFileSystemLayout.BlockSize];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(superblock.AsSpan(CoherentFileSystemLayout.InodeZoneEndOffset), 3);
        WriteCanonicalUInt32(superblock.AsSpan(CoherentFileSystemLayout.FileSystemBlockCountOffset), 5);
        System.Text.Encoding.ASCII.GetBytes(CoherentFileSystemLayout.DefaultVolumeName).CopyTo(superblock, CoherentFileSystemLayout.VolumeNameOffset);
        System.Text.Encoding.ASCII.GetBytes(CoherentFileSystemLayout.DefaultPackName).CopyTo(superblock, CoherentFileSystemLayout.PackNameOffset);
        var inodeBlock = new byte[CoherentFileSystemLayout.BlockSize];
        WriteInode(inodeBlock.AsSpan(CoherentFileSystemLayout.InodeSize), 0x4000, 2 * CoherentFileSystemLayout.DirectoryEntrySize, 3);
        WriteInode(inodeBlock.AsSpan(2 * CoherentFileSystemLayout.InodeSize), 0x4000, CoherentFileSystemLayout.DirectoryEntrySize, 4);
        var rootDirectory = new byte[CoherentFileSystemLayout.BlockSize];
        WriteDirectoryEntry(rootDirectory, 0, 3, "A");
        WriteDirectoryEntry(rootDirectory, CoherentFileSystemLayout.DirectoryEntrySize, 3, "B");
        var childDirectory = new byte[CoherentFileSystemLayout.BlockSize];
        WriteDirectoryEntry(childDirectory, 0, 2, "back");
        var image = CreateSectorImage(5, new Dictionary<int, byte[]> { [0] = superblock[..CoherentFileSystemLayout.BlockSize], [1] = superblock[CoherentFileSystemLayout.BlockSize..], [2] = inodeBlock, [3] = rootDirectory, [4] = childDirectory });

        var volume = new CoherentFileSystemReader().Read(image);

        Assert.Contains(volume.Warnings, warning => warning.Contains("cycle", StringComparison.Ordinal));
        Assert.Contains(volume.Warnings, warning => warning.Contains("déjà été parcouru", StringComparison.Ordinal));
        Assert.Contains(volume.Entries, entry => entry.Name == "A" && !entry.MetadataValid);
        Assert.Contains(volume.Entries, entry => entry.Name == "B" && !entry.MetadataValid);
    }

    [Fact]
    public void CoherentReaderRejectsAMissingSuperblockBlockAndAnInodeOutsideItsZone()
    {
        var missingSuperblock = CreateSectorImage(3, new Dictionary<int, byte[]> { [0] = new byte[CoherentFileSystemLayout.BlockSize] });
        Assert.Throws<InvalidDataException>(() => new CoherentFileSystemReader().Read(missingSuperblock));
        var complete = CoherentImageData.Create(CreateSectorImage(4, new Dictionary<int, byte[]>()));
        Assert.Throws<InvalidDataException>(() => CoherentInodeReader.Read(complete, 3, 9));
    }

    [Theory]
    [InlineData(0x8000, FileSystemEntryKind.File)]
    [InlineData(0x4000, FileSystemEntryKind.Directory)]
    [InlineData(0x2000, FileSystemEntryKind.Unknown)]
    public void CoherentInodeModesMapToTheirCommonKinds(ushort mode, FileSystemEntryKind expected) => Assert.Equal(expected, mode.Type().ToCommonKind());

    private static string CoherentImagePath()
    {
        var path = Directory.EnumerateFiles(
                FindImageTestRoot(),
                "COHERENT - Volume 1 - High Resolution.bin",
                SearchOption.AllDirectories)
            .FirstOrDefault();
        return path ?? throw new FileNotFoundException("L'image Coherent locale requise est absente.");
    }

    /// <summary>Crée une image COHERENT synthétique en rendant présents les blocs fournis et des blocs nuls complets pour les autres indices.</summary>
    private static SectorImage CreateSectorImage(int blockCount, IReadOnlyDictionary<int, byte[]> overrides)
    {
        var blocks = new List<SectorBlock>();
        for (var block = 0; block < blockCount; block++)
        {
            byte[] data;
            if (overrides.Count == 0) data = new byte[CoherentFileSystemLayout.BlockSize];
            else if (!overrides.TryGetValue(block, out data!)) continue;
            blocks.Add(new SectorBlock(block, new SectorAddress(block, 0, 1), data));
        }
        return new(DiskImageFormatIds.Commodore900Coherent, CoherentFileSystemLayout.BlockSize, 1, 1, blockCount, blocks);
    }

    /// <summary>Écrit les champs d'un inode synthétique utilisés par le lecteur.</summary>
    private static void WriteInode(Span<byte> inode, ushort mode, int size, int firstBlock)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(inode, mode);
        WriteCanonicalUInt32(inode[CoherentFileSystemLayout.InodeSizeOffset..], checked((uint)size));
        inode[CoherentFileSystemLayout.InodePointersOffset] = (byte)(firstBlock >> 16);
        inode[CoherentFileSystemLayout.InodePointersOffset + 1] = (byte)firstBlock;
        inode[CoherentFileSystemLayout.InodePointersOffset + 2] = (byte)(firstBlock >> 8);
    }

    /// <summary>Écrit une entrée de répertoire synthétique.</summary>
    private static void WriteDirectoryEntry(Span<byte> directory, int offset, ushort inode, string name)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(directory[offset..], inode);
        System.Text.Encoding.ASCII.GetBytes(name).CopyTo(directory[(offset + CoherentFileSystemLayout.DirectoryInodeLength)..]);
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
        var bytes = new byte[availableBlocks * CoherentFileSystemLayout.BlockSize];
        WriteCanonicalUInt32(bytes.AsSpan(CoherentFileSystemLayout.FileSystemBlockCountOffset, CoherentFormat.UInt32Length), checked((uint)declaredBlocks));
        System.Text.Encoding.ASCII.GetBytes(volumeName ?? CoherentFileSystemLayout.DefaultVolumeName).CopyTo(bytes, CoherentFileSystemLayout.VolumeNameOffset);
        System.Text.Encoding.ASCII.GetBytes(packName ?? CoherentFileSystemLayout.DefaultPackName).CopyTo(bytes, CoherentFileSystemLayout.PackNameOffset);
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
