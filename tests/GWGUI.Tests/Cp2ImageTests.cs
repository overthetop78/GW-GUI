using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Containers.Cp2;
using GWGUI.MediaEngine.Images;

namespace GWGUI.Tests;

/// <summary>Vérifie la lecture publique d'un conteneur SNATCH-IT CP2 réel et de ses variantes invalides.</summary>
public sealed class Cp2ImageTests
{
    private const int ExpectedCylinderCount = 40;
    private const int ExpectedHeadCount = 2;
    private const int ExpectedSectorsPerTrack = 9;
    private const int ExpectedGroupCount = 8;
    private const int FirstGroupDescriptorCount = 11;

    /// <summary>Vérifie la structure du premier groupe et la restitution logique de secteurs stockés en ordre angulaire.</summary>
    [Fact]
    public async Task RealImagePreservesGroupsAngularOrderGeometryAndSectorContents()
    {
        var path = Cp2ImagePath();
        var data = await File.ReadAllBytesAsync(path);
        Assert.True(data.AsSpan(Cp2Layout.SignatureOffset, Cp2Format.SignatureLength).SequenceEqual(Cp2Format.Signature));

        var groups = ReadGroups(data);
        Assert.Equal(ExpectedGroupCount, groups.Count);
        Assert.Equal(FirstGroupDescriptorCount, groups[0].DescriptorCount);
        Assert.Equal(new ushort[] { 22, 32, 24, 34, 26, 36, 28, 38, 30 }, groups[0].FirstTrackPositions);

        var image = await new Cp2Reader().ReadAsync(path);
        Assert.Equal("ibm.360", image.FormatId);
        Assert.Equal((ExpectedCylinderCount, ExpectedHeadCount, ExpectedSectorsPerTrack, Cp2Layout.ReconstructedSectorSize), (image.Cylinders, image.Heads, image.SectorsPerTrack, image.BlockSize));
        Assert.Equal(ExpectedCylinderCount * ExpectedHeadCount * ExpectedSectorsPerTrack, image.BlockCount);
        var expectedSectors = ReadExpectedSectors(data);
        Assert.Equal(image.BlockCount, expectedSectors.Count);
        foreach (var sector in expectedSectors)
        {
            var logicalBlock = ((sector.Cylinder * ExpectedHeadCount + sector.Head) * ExpectedSectorsPerTrack) + sector.Number - 1;
            Assert.True(sector.Data.SequenceEqual(image.GetBlock(logicalBlock).Span), $"Le secteur {sector.Cylinder}:{sector.Head}:{sector.Number} ne correspond pas à sa charge utile CP2.");
        }
    }

    /// <summary>Vérifie l'exploration publique du système de fichiers DOS contenu dans l'image CP2 réelle.</summary>
    [Fact]
    public async Task RealImageExposesItsDosFileSystem()
    {
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(Cp2ImagePath());
        Assert.Equal("ibm.360", document.Image.FormatId);
        Assert.True(document.FileSystemRecognized);
        Assert.NotEmpty(document.Volume.Entries);
        Assert.All(document.Volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
    }

    /// <summary>Vérifie qu'un secteur dont la taille n'est pas de 512 octets n'est pas ajouté à l'image reconstruite.</summary>
    [Fact]
    public async Task Non512ByteSectorIsRejectedFromTheReconstructedImage()
    {
        var data = await File.ReadAllBytesAsync(Cp2ImagePath());
        var groups = ReadGroups(data);
        var lastGroup = groups[^1];
        var descriptorOffset = lastGroup.Offset + Cp2Layout.GroupHeaderSize + 2 * Cp2Layout.TrackDescriptorSize;
        var sectorOffset = descriptorOffset + Cp2Layout.TrackHeaderSize + 7 * Cp2Layout.SectorDescriptorSize;
        data[sectorOffset + Cp2Layout.SectorSizeCodeOffset] = 1;
        Array.Resize(ref data, data.Length - 256);

        var image = await ReadTemporaryAsync(data);
        Assert.All(image.GetBlock(718).ToArray(), value => Assert.Equal(0, value));
        Assert.Contains(image.GetBlock(719).ToArray(), value => value != 0);
    }

    /// <summary>Vérifie les diagnostics produits pour une signature, des métadonnées et un descripteur invalides.</summary>
    [Fact]
    public async Task RejectsMissingSignatureInvalidMetadataAndInvalidSectorCount()
    {
        var source = await File.ReadAllBytesAsync(Cp2ImagePath());

        var missingSignature = source.ToArray();
        missingSignature[Cp2Layout.SignatureOffset] ^= byte.MaxValue;
        await AssertRejectedAsync(missingSignature, "signature");

        var invalidMetadata = source.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(invalidMetadata.AsSpan(Cp2Layout.FirstGroupOffset + Cp2Layout.MetadataLengthOffset), 2);
        await AssertRejectedAsync(invalidMetadata, "description");

        var invalidSectorCount = source.ToArray();
        invalidSectorCount[Cp2Layout.FirstGroupOffset + Cp2Layout.GroupHeaderSize + Cp2Layout.TrackSectorCountOffset] = Cp2Layout.MaximumSectorDescriptorCount + 1;
        await AssertRejectedAsync(invalidSectorCount, "count");
    }

    /// <summary>Vérifie les contrôles de limites des descripteurs et des charges utiles CP2.</summary>
    [Fact]
    public async Task RejectsTruncatedDescriptorAndSectorPayload()
    {
        var source = await File.ReadAllBytesAsync(Cp2ImagePath());
        var truncatedDescriptor = source[..(Cp2Layout.FirstGroupOffset + Cp2Layout.GroupHeaderSize + 100)];
        await AssertRejectedAsync(truncatedDescriptor, "truncated");

        var metadataLength = BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(Cp2Layout.FirstGroupOffset + Cp2Layout.MetadataLengthOffset));
        var payloadOffset = Cp2Layout.FirstGroupOffset + Cp2Layout.GroupHeaderSize + metadataLength + Cp2Layout.FramingSize;
        var truncatedPayload = source[..(payloadOffset + Cp2Layout.ReconstructedSectorSize - 1)];
        await AssertRejectedAsync(truncatedPayload, "requires");
    }

    /// <summary>Vérifie que l'annulation demandée avant la lecture est propagée.</summary>
    [Fact]
    public async Task PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new Cp2Reader().ReadAsync(Cp2ImagePath(), cancellation.Token));
    }

    /// <summary>Lit une variante CP2 temporaire avec le lecteur public.</summary>
    private static async Task<GWGUI.MediaEngine.SectorImages.SectorImage> ReadTemporaryAsync(byte[] data)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.cp2");
        try
        {
            await File.WriteAllBytesAsync(path, data);
            return await new Cp2Reader().ReadAsync(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Vérifie qu'une variante CP2 est rejetée avec le fragment de diagnostic attendu.</summary>
    private static async Task AssertRejectedAsync(byte[] data, string expectedMessage)
    {
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => ReadTemporaryAsync(data));
        Assert.Contains(expectedMessage, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Relève les groupes et les positions angulaires du premier descripteur de chaque groupe.</summary>
    private static IReadOnlyList<Cp2Group> ReadGroups(byte[] data)
    {
        var groups = new List<Cp2Group>();
        var groupOffset = Cp2Layout.FirstGroupOffset;
        while (groupOffset + Cp2Layout.GroupHeaderSize <= data.Length)
        {
            var metadataLength = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(groupOffset + Cp2Layout.MetadataLengthOffset, Cp2Layout.LengthFieldSize));
            if (metadataLength == 0 || (metadataLength - Cp2Layout.MetadataLengthAdjustment) % Cp2Layout.TrackDescriptorSize != 0) break;
            var descriptorCount = (metadataLength - Cp2Layout.MetadataLengthAdjustment) / Cp2Layout.TrackDescriptorSize;
            var firstDescriptor = groupOffset + Cp2Layout.GroupHeaderSize;
            var sectorCount = data[firstDescriptor + Cp2Layout.TrackSectorCountOffset];
            var positions = Enumerable.Range(0, sectorCount).Select(index => BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(firstDescriptor + Cp2Layout.TrackHeaderSize + index * Cp2Layout.SectorDescriptorSize + Cp2Layout.SectorPositionOffset, Cp2Layout.SectorPositionLength))).ToArray();
            var payloadOffset = groupOffset + Cp2Layout.GroupHeaderSize + metadataLength + Cp2Layout.FramingSize;
            groups.Add(new(groupOffset, descriptorCount, payloadOffset, positions));

            var payloadLength = 0;
            for (var descriptorIndex = 0; descriptorIndex < descriptorCount; descriptorIndex++)
            {
                var descriptorOffset = firstDescriptor + descriptorIndex * Cp2Layout.TrackDescriptorSize;
                var count = data[descriptorOffset + Cp2Layout.TrackSectorCountOffset];
                for (var sectorIndex = 0; sectorIndex < count; sectorIndex++)
                {
                    var sectorOffset = descriptorOffset + Cp2Layout.TrackHeaderSize + sectorIndex * Cp2Layout.SectorDescriptorSize;
                    if (data[sectorOffset + Cp2Layout.SectorCylinderOffset] != data[descriptorOffset + Cp2Layout.TrackCylinderOffset] || data[sectorOffset + Cp2Layout.SectorHeadOffset] != data[descriptorOffset + Cp2Layout.TrackHeadOffset]) continue;
                    payloadLength += Cp2Layout.BaseSectorSize << data[sectorOffset + Cp2Layout.SectorSizeCodeOffset];
                }
            }
            groupOffset = payloadOffset + payloadLength - Cp2Layout.FramingSize;
        }
        return groups;
    }

    /// <summary>Reconstitue les secteurs attendus à partir de l'ordre angulaire décrit dans chaque groupe.</summary>
    private static IReadOnlyList<ExpectedSector> ReadExpectedSectors(byte[] data)
    {
        var sectors = new Dictionary<(int Cylinder, int Head, int Number), byte[]>();
        foreach (var group in ReadGroups(data))
        {
            var descriptors = new List<List<ExpectedSectorDescriptor>>();
            for (var descriptorIndex = 0; descriptorIndex < group.DescriptorCount; descriptorIndex++)
            {
                var descriptorOffset = group.Offset + Cp2Layout.GroupHeaderSize + descriptorIndex * Cp2Layout.TrackDescriptorSize;
                var trackCylinder = data[descriptorOffset + Cp2Layout.TrackCylinderOffset];
                var trackHead = data[descriptorOffset + Cp2Layout.TrackHeadOffset];
                var count = data[descriptorOffset + Cp2Layout.TrackSectorCountOffset];
                var descriptorSectors = new List<ExpectedSectorDescriptor>();
                for (var sectorIndex = 0; sectorIndex < count; sectorIndex++)
                {
                    var sectorOffset = descriptorOffset + Cp2Layout.TrackHeaderSize + sectorIndex * Cp2Layout.SectorDescriptorSize;
                    var cylinder = data[sectorOffset + Cp2Layout.SectorCylinderOffset];
                    var head = data[sectorOffset + Cp2Layout.SectorHeadOffset];
                    var sizeCode = data[sectorOffset + Cp2Layout.SectorSizeCodeOffset];
                    if (cylinder != trackCylinder || head != trackHead || sizeCode > Cp2Layout.MaximumSectorSizeCode) continue;
                    var number = data[sectorOffset + Cp2Layout.SectorNumberOffset];
                    var size = Cp2Layout.BaseSectorSize << sizeCode;
                    var position = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(sectorOffset + Cp2Layout.SectorPositionOffset, Cp2Layout.SectorPositionLength));
                    descriptorSectors.Add(new(cylinder, head, number, size, position));
                }
                descriptors.Add(descriptorSectors);
            }

            var payloadOffset = group.PayloadOffset;
            foreach (var descriptor in descriptors)
            {
                foreach (var sector in descriptor.OrderBy(item => item.Position))
                {
                    var payload = data.AsSpan(payloadOffset, sector.Size).ToArray();
                    payloadOffset += sector.Size;
                    if (sector.Size == Cp2Layout.ReconstructedSectorSize) sectors.TryAdd((sector.Cylinder, sector.Head, sector.Number), payload);
                }
            }
        }
        return Enumerable.Range(0, ExpectedCylinderCount * ExpectedHeadCount * ExpectedSectorsPerTrack).Select(logicalBlock =>
        {
            var track = logicalBlock / ExpectedSectorsPerTrack;
            var cylinder = track / ExpectedHeadCount;
            var head = track % ExpectedHeadCount;
            var number = logicalBlock % ExpectedSectorsPerTrack + 1;
            return new ExpectedSector(cylinder, head, number, sectors.GetValueOrDefault((cylinder, head, number), new byte[Cp2Layout.ReconstructedSectorSize]));
        }).ToArray();
    }

    /// <summary>Retourne le chemin de l'image CP2 réelle obligatoire.</summary>
    private static string Cp2ImagePath()
    {
        var path = Path.Combine(RepositoryRoot(), "image_test", "IBM PC", "PFS Write C00 (1985) (5.25-360k) disk01.cp2");
        return File.Exists(path) ? path : throw new FileNotFoundException("L'image CP2 de test est introuvable.", path);
    }

    /// <summary>Localise la racine du dépôt courant.</summary>
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("La racine du dépôt est introuvable.");
    }

    /// <summary>Décrit les données structurelles nécessaires aux vérifications d'un groupe CP2.</summary>
    private sealed record Cp2Group(int Offset, int DescriptorCount, int PayloadOffset, IReadOnlyList<ushort> FirstTrackPositions);

    /// <summary>Décrit un secteur attendu et son contenu.</summary>
    private sealed record ExpectedSector(int Cylinder, int Head, int Number, byte[] Data);

    /// <summary>Décrit l'adresse, la taille et la position d'un secteur attendu avant lecture de sa charge utile.</summary>
    private sealed record ExpectedSectorDescriptor(int Cylinder, int Head, int Number, int Size, int Position);
}
