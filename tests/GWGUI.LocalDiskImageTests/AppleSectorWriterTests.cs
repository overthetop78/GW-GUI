using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Apple.Raw;
using GWGUI.MediaEngine.Containers.Apple.TwoImg;
using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie les Writers Apple bruts, leurs ordres sectoriels et le conteneur 2IMG.</summary>
public sealed class AppleSectorWriterTests
{
    /// <summary>Vérifie les profils D13, DOS 3.3, ProDOS 140 Kio, ProDOS 800 Kio et SOS après réouverture.</summary>
    [Theory]
    [InlineData(DiskImageFormatIds.AppleIIAppleDos113, DiskImageFileExtensions.D13)]
    [InlineData(DiskImageFormatIds.AppleIIAppleDos140, DiskImageFileExtensions.Do)]
    [InlineData(DiskImageFormatIds.AppleIIProDos140, DiskImageFileExtensions.Po)]
    [InlineData(DiskImageFormatIds.AppleIIProDos800, DiskImageFileExtensions.Po)]
    [InlineData(DiskImageFormatIds.AppleIIISos, DiskImageFileExtensions.Do)]
    public async Task WritesAndReopensEveryRawProfile(string targetFormatId, string extension)
    {
        var source = CreateImage(targetFormatId);
        var path = TemporaryPath(extension);
        try
        {
            await new AppleRawImageWriter().WriteAsync(source, path, targetFormatId);
            var reopened = await new AppleDiskImageReader().ReadAsync(path);
            Assert.Equal(ExpectedReaderFormat(targetFormatId), reopened.FormatId);
            Assert.Equal(source.BlockCount, reopened.BlockCount);
            Assert.Empty(reopened.MissingBlocks);
            Assert.Equal(source.GetBlock(0).ToArray(), reopened.GetBlock(0).ToArray());
            Assert.Equal(source.GetBlock(source.BlockCount - 1).ToArray(), reopened.GetBlock(reopened.BlockCount - 1).ToArray());
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Vérifie que les sorties DO et DSK ProDOS appliquent exactement l'inverse de l'ordre lu.</summary>
    [Theory]
    [InlineData(DiskImageFileExtensions.Do)]
    [InlineData(DiskImageFileExtensions.Dsk)]
    public async Task WritesProDosInDosFileOrder(string extension)
    {
        var image = CreateImage(DiskImageFormatIds.AppleIIProDos140);
        var path = TemporaryPath(extension);
        try
        {
            await new AppleRawImageWriter().WriteAsync(image, path, DiskImageFormatIds.AppleIIProDos140);
            var dosOrder = await File.ReadAllBytesAsync(path);
            var proDosOrder = AppleIISectorOrderConverter.DosToProDos(dosOrder);
            Assert.Equal(Flatten(image), proDosOrder);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Vérifie tous les champs du conteneur 2IMG et l'identité des blocs après réouverture.</summary>
    [Theory]
    [InlineData(DiskImageFormatIds.AppleIIAppleDos113, TwoImgImageFormat.Dos, 0)]
    [InlineData(DiskImageFormatIds.AppleIIAppleDos140, TwoImgImageFormat.Dos, 0)]
    [InlineData(DiskImageFormatIds.AppleIIProDos140, TwoImgImageFormat.ProDos, 280)]
    [InlineData(DiskImageFormatIds.AppleIIProDos800, TwoImgImageFormat.ProDos, 1600)]
    [InlineData(DiskImageFormatIds.AppleIIISos, TwoImgImageFormat.ProDos, 280)]
    public async Task WritesAndReopensTwoImg(string targetFormatId, TwoImgImageFormat expectedType, int expectedBlockCount)
    {
        var source = CreateImage(targetFormatId);
        var path = TemporaryPath(DiskImageFileExtensions.TwoMg);
        try
        {
            await new TwoImgWriter().WriteAsync(source, path, targetFormatId);
            var bytes = await File.ReadAllBytesAsync(path);
            Assert.True(bytes.AsSpan(TwoImgLayout.SignatureOffset, TwoImgLayout.SignatureLength).SequenceEqual(TwoImgFormat.SignatureBytes));
            Assert.True(bytes.AsSpan(TwoImgLayout.CreatorOffset, TwoImgLayout.CreatorLength).SequenceEqual(TwoImgFormat.CreatorBytes));
            Assert.Equal(TwoImgLayout.MinimumHeaderSize, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(TwoImgLayout.HeaderSizeOffset)));
            Assert.Equal(TwoImgFormat.SupportedVersion, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(TwoImgLayout.VersionOffset)));
            Assert.Equal((uint)expectedType, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(TwoImgLayout.ImageFormatOffset)));
            Assert.Equal(0u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(TwoImgLayout.FlagsOffset)));
            Assert.Equal((uint)expectedBlockCount, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(TwoImgLayout.BlockCountOffset)));
            Assert.Equal((uint)TwoImgLayout.MinimumHeaderSize, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(TwoImgLayout.DataOffsetOffset)));
            Assert.Equal((uint)source.Capacity, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(TwoImgLayout.DataLengthOffset)));
            var reopened = await new AppleDiskImageReader().ReadAsync(path);
            Assert.Equal(ExpectedReaderFormat(targetFormatId), reopened.FormatId);
            Assert.Equal(source.BlockCount, reopened.BlockCount);
            Assert.Equal(source.GetBlock(0).ToArray(), reopened.GetBlock(0).ToArray());
            Assert.Equal(source.GetBlock(source.BlockCount - 1).ToArray(), reopened.GetBlock(reopened.BlockCount - 1).ToArray());
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Vérifie le routage interne strict sans confondre les extensions Apple ambiguës.</summary>
    [Fact]
    public void RoutesOnlySupportedAppleTargets()
    {
        Assert.True(AppleSectorConversionService.CanCreate(DiskImageFormatIds.AppleIIAppleDos113, DiskImageFileExtensions.D13));
        Assert.True(AppleSectorConversionService.CanCreate(DiskImageFormatIds.AppleIIAppleDos140, DiskImageFileExtensions.Do));
        Assert.True(AppleSectorConversionService.CanCreate(DiskImageFormatIds.AppleIIProDos140, DiskImageFileExtensions.Dsk));
        Assert.True(AppleSectorConversionService.CanCreate(DiskImageFormatIds.AppleIIProDos800, DiskImageFileExtensions.TwoMg));
        Assert.True(AppleSectorConversionService.CanCreate(DiskImageFormatIds.AppleIIISos, DiskImageFileExtensions.Po));
        Assert.False(AppleSectorConversionService.CanCreate(DiskImageFormatIds.AppleIIAppleDos113, DiskImageFileExtensions.Po));
        Assert.False(AppleSectorConversionService.CanCreate(DiskImageFormatIds.AppleIIProDos800, DiskImageFileExtensions.Do));
        Assert.False(AppleSectorConversionService.CanCreate(DiskImageFormatIds.AppleMacHfs, DiskImageFileExtensions.TwoMg));
    }

    /// <summary>Construit une image complète et déterministe pour le profil demandé.</summary>
    private static SectorImage CreateImage(string targetFormatId)
    {
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase)) return CreateLinear(DiskImageFormatIds.AppleIIDos32, AppleIIGeometry.SectorSize, AppleIIGeometry.TrackCount, 1, AppleIIGeometry.Dos32SectorsPerTrack, AppleIIGeometry.TrackCount * AppleIIGeometry.Dos32SectorsPerTrack, AppleIIGeometry.Dos32Capacity, false, false);
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase)) return CreateLinear(DiskImageFormatIds.AppleIIDos33, AppleIIGeometry.SectorSize, AppleIIGeometry.TrackCount, 1, AppleIIGeometry.SectorsPerTrack, AppleIIGeometry.TrackCount * AppleIIGeometry.SectorsPerTrack, AppleIIGeometry.Capacity, false, false);
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIProDos140, StringComparison.OrdinalIgnoreCase)) return CreateLinear(DiskImageFormatIds.AppleIIProDos, AppleIIGeometry.ProDosBlockSize, AppleIIGeometry.TrackCount, 1, AppleIIGeometry.ProDosBlocksPerTrack, AppleIIGeometry.TrackCount * AppleIIGeometry.ProDosBlocksPerTrack, AppleIIGeometry.Capacity, true, false);
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase)) return CreateLinear(DiskImageFormatIds.AppleIIProDos, MacintoshGcrGeometry.BlockSize, MacintoshGcrGeometry.CylinderCount, MacintoshGcrGeometry.DoubleSidedHeadCount, MacintoshGcrGeometry.MaximumSectorsPerTrack, MacintoshGcrGeometry.SingleSidedBlockCount * MacintoshGcrGeometry.DoubleSidedHeadCount, MacintoshGcrGeometry.Capacity800K, true, false);
        return CreateLinear(DiskImageFormatIds.AppleIIISos, AppleIIGeometry.ProDosBlockSize, AppleIIGeometry.TrackCount, 1, AppleIIGeometry.ProDosBlocksPerTrack, AppleIIGeometry.TrackCount * AppleIIGeometry.ProDosBlocksPerTrack, AppleIIGeometry.Capacity, false, true);
    }

    /// <summary>Construit les blocs et les marqueurs structurels requis par les sondes de réouverture.</summary>
    private static SectorImage CreateLinear(string formatId, int blockSize, int cylinders, int heads, int sectorsPerTrack, int blockCount, int capacity, bool proDosHeader, bool sosMarker)
    {
        var payload = new byte[capacity];
        for (var index = 0; index < payload.Length; index++) payload[index] = (byte)((index * 37 + index / blockSize * 19) & 0xff);
        if (proDosHeader)
        {
            var root = 2 * MacintoshGcrGeometry.BlockSize;
            payload[root + 4] = 0xf1;
            payload[root + 0x23] = 0x27;
        }
        if (sosMarker) "SOS KRNL"u8.CopyTo(payload);
        var blocks = Enumerable.Range(0, blockCount).Select(logicalBlock => new SectorBlock(logicalBlock, Address(logicalBlock, cylinders, heads, sectorsPerTrack), payload.AsSpan(logicalBlock * blockSize, blockSize).ToArray(), true)).ToArray();
        return new(formatId, blockSize, cylinders, heads, sectorsPerTrack, blocks, capacity: capacity, logicalBlockCount: blockCount);
    }

    /// <summary>Retourne une adresse stable sans imposer une géométrie linéaire aux images zonées.</summary>
    private static SectorAddress Address(int logicalBlock, int cylinders, int heads, int sectorsPerTrack)
    {
        if (logicalBlock < cylinders * heads * sectorsPerTrack) return new(logicalBlock / (heads * sectorsPerTrack), logicalBlock / sectorsPerTrack % heads, logicalBlock % sectorsPerTrack);
        return new(cylinders - 1, heads - 1, sectorsPerTrack - 1);
    }

    /// <summary>Concatène les blocs d'une image dans leur ordre logique.</summary>
    private static byte[] Flatten(SectorImage image) => Enumerable.Range(0, image.BlockCount).SelectMany(logicalBlock => image.GetBlock(logicalBlock).ToArray()).ToArray();

    /// <summary>Retourne l'identifiant normalisé produit par le Reader du conteneur.</summary>
    private static string ExpectedReaderFormat(string targetFormatId)
    {
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase)) return DiskImageFormatIds.AppleIIDos32;
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase)) return DiskImageFormatIds.AppleIIDos33;
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase)) return DiskImageFormatIds.AppleIIISos;
        return DiskImageFormatIds.AppleIIProDos;
    }

    /// <summary>Crée un chemin temporaire portant l'extension demandée.</summary>
    private static string TemporaryPath(string extension) => Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{extension}");
}
