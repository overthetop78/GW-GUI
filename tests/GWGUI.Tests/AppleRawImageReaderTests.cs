using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Apple.DiskCopy;
using GWGUI.MediaEngine.Containers.Apple.Raw;
using GWGUI.MediaEngine.Containers.Apple.TwoImg;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.AppleDos;
using GWGUI.MediaEngine.FileSystems.Lisa;
using GWGUI.MediaEngine.FileSystems.Macintosh;
using GWGUI.MediaEngine.FileSystems.ProDos;
using GWGUI.MediaEngine.FileSystems.Sos;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie chaque branche du lecteur des représentations Apple sans en-tête.</summary>
public sealed class AppleRawImageReaderTests
{
    /// <summary>Vérifie la géométrie fixe D13 et son classement comme repli géométrique.</summary>
    [Fact]
    public void ReadsD13Geometry()
    {
        var result = AppleRawImageReader.Read(new byte[AppleIIGeometry.Dos32Capacity], DiskImageFileExtensions.D13);
        Assert.Equal(DiskImageFormatIds.AppleIIDos32, result.Image.FormatId);
        Assert.Equal(AppleRawImageMatchKind.GeometryFallback, result.MatchKind);
        AssertImage(result.Image, AppleIIGeometry.SectorSize, AppleIIGeometry.TrackCount, 1, AppleIIGeometry.Dos32SectorsPerTrack, AppleIIGeometry.Dos32Capacity);
    }

    /// <summary>Vérifie les choix DOS 3.3 validé, PO indicé et ProDOS validé pour la capacité ambiguë de 140 Kio.</summary>
    [Fact]
    public void SelectsExactAppleII140KOrder()
    {
        var dos = CreateDos33();
        var dosResult = AppleRawImageReader.Read(dos, DiskImageFileExtensions.Do);
        Assert.Equal(DiskImageFormatIds.AppleIIDos33, dosResult.Image.FormatId);
        Assert.Equal(AppleRawImageMatchKind.ValidatedStructure, dosResult.MatchKind);

        var poResult = AppleRawImageReader.Read(new byte[AppleIIGeometry.Capacity], DiskImageFileExtensions.Po);
        Assert.Equal(DiskImageFormatIds.AppleIIProDos, poResult.Image.FormatId);
        Assert.Equal(AppleRawImageMatchKind.ExtensionHint, poResult.MatchKind);

        var proDos = CreateProDos(AppleIIGeometry.Capacity);
        var proDosResult = AppleRawImageReader.Read(proDos, DiskImageFileExtensions.Do);
        Assert.Equal(DiskImageFormatIds.AppleIIProDos, proDosResult.Image.FormatId);
        Assert.Equal(AppleRawImageMatchKind.ValidatedStructure, proDosResult.MatchKind);
    }

    /// <summary>Vérifie que le marqueur SOS n'est reconnu qu'après la conversion de l'ordre DOS vers ProDOS.</summary>
    [Fact]
    public void DetectsSosAfterSectorOrderConversion()
    {
        var dosOrder = new byte[AppleIIGeometry.Capacity];
        SosBootFormat.Marker.CopyTo(dosOrder);
        var result = AppleRawImageReader.Read(dosOrder, DiskImageFileExtensions.Do);
        Assert.Equal(DiskImageFormatIds.AppleIIISos, result.Image.FormatId);
        Assert.Equal(AppleRawImageMatchKind.ValidatedStructure, result.MatchKind);
    }

    /// <summary>Vérifie Lisa 400 Kio, Macintosh MFS/HFS, ProDOS 800 Kio et Macintosh MFM 1,44 Mio.</summary>
    [Fact]
    public void ReadsAllApple35Interpretations()
    {
        var lisa = new byte[LisaVolumeHeader.Capacity];
        BinaryPrimitives.WriteUInt16BigEndian(lisa, LisaVolumeHeader.TableCatalogVersion);
        lisa[LisaVolumeHeader.NameLengthOffset] = 4;
        "Lisa"u8.CopyTo(lisa.AsSpan(LisaVolumeHeader.NameOffset));
        var lisaResult = AppleRawImageReader.Read(lisa, DiskImageFileExtensions.Img);
        Assert.Equal(DiskImageFormatIds.AppleLisaRaw, lisaResult.Image.FormatId);
        AssertImage(lisaResult.Image, MacintoshGcrGeometry.BlockSize, MacintoshGcrGeometry.CylinderCount, 1, MacintoshGcrGeometry.MaximumSectorsPerTrack, MacintoshGcrGeometry.Capacity400K);

        var mfs = CreateMacintosh(MacintoshGcrGeometry.Capacity400K, MacintoshVolumeSignatures.Mfs);
        var mfsResult = AppleRawImageReader.Read(mfs, DiskImageFileExtensions.Dsk);
        Assert.Equal(DiskImageFormatIds.AppleMacMfs, mfsResult.Image.FormatId);

        var hfs = CreateMacintosh(MacintoshGcrGeometry.Capacity800K, MacintoshVolumeSignatures.Hfs);
        var hfsResult = AppleRawImageReader.Read(hfs, DiskImageFileExtensions.Dsk);
        Assert.Equal(DiskImageFormatIds.AppleMacHfs, hfsResult.Image.FormatId);
        AssertImage(hfsResult.Image, MacintoshGcrGeometry.BlockSize, MacintoshGcrGeometry.CylinderCount, 2, MacintoshGcrGeometry.MaximumSectorsPerTrack, MacintoshGcrGeometry.Capacity800K);

        var proDos = AppleRawImageReader.Read(CreateProDos(MacintoshGcrGeometry.Capacity800K), DiskImageFileExtensions.Po);
        Assert.Equal(DiskImageFormatIds.AppleIIProDos, proDos.Image.FormatId);

        var mfm = AppleRawImageReader.Read(CreateMacintosh(MacintoshMfm1440Geometry.Capacity, MacintoshVolumeSignatures.Hfs), DiskImageFileExtensions.Dsk);
        Assert.Equal(DiskImageFormatIds.Mac1440, mfm.Image.FormatId);
        AssertImage(mfm.Image, MacintoshMfm1440Geometry.SectorSize, MacintoshMfm1440Geometry.CylinderCount, MacintoshMfm1440Geometry.HeadCount, MacintoshMfm1440Geometry.SectorsPerTrack, MacintoshMfm1440Geometry.Capacity);
    }

    /// <summary>Vérifie les erreurs distinctes de capacité connue, inconnue et tronquée.</summary>
    [Fact]
    public void RejectsUnidentifiedAndUnsupportedLengths()
    {
        var known = Assert.Throws<InvalidDataException>(() => AppleRawImageReader.Read(new byte[MacintoshGcrGeometry.Capacity400K], DiskImageFileExtensions.Dsk));
        Assert.Contains("is known", known.Message, StringComparison.Ordinal);
        var unknown = Assert.Throws<InvalidDataException>(() => AppleRawImageReader.Read(new byte[1], DiskImageFileExtensions.Img));
        Assert.Contains("unsupported", unknown.Message, StringComparison.Ordinal);
        Assert.Throws<InvalidDataException>(() => AppleRawImageReader.Read(new byte[AppleIIGeometry.Capacity - 1], DiskImageFileExtensions.Do));
    }

    /// <summary>Vérifie que le fichier brut, 2IMG et DiskCopy produisent les mêmes blocs pour une même charge utile.</summary>
    [Fact]
    public async Task ContainersPreserveRawPayloadInterpretation()
    {
        var raw = CreateDos33();
        var direct = AppleRawImageReader.Read(raw, DiskImageFileExtensions.Do).Image;
        var twoImg = await new AppleDiskImageReader().ReadAsync(BuildTwoImg(raw), DiskImageFileExtensions.TwoMg, null);
        AssertEquivalent(direct, twoImg);

        var mac = CreateMacintosh(MacintoshMfm1440Geometry.Capacity, MacintoshVolumeSignatures.Hfs);
        var rawMac = AppleRawImageReader.Read(mac, DiskImageFileExtensions.Dsk).Image;
        var diskCopy = await new AppleDiskImageReader().ReadAsync(BuildDiskCopy(mac), DiskImageFileExtensions.Image, null);
        AssertEquivalent(rawMac, diskCopy);
    }

    /// <summary>Crée une charge utile DOS 3.3 dont le VTOC est valide.</summary>
    private static byte[] CreateDos33()
    {
        var data = new byte[AppleIIGeometry.Capacity];
        var offset = AppleDosVtoc.Track * AppleIIGeometry.TrackSize;
        data[offset + AppleDosVtoc.CatalogTrackOffset] = 1;
        data[offset + AppleDosVtoc.CatalogSectorOffset] = 0;
        data[offset + AppleDosVtoc.SectorsPerTrackOffset] = AppleIIGeometry.SectorsPerTrack;
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset + AppleDosVtoc.SectorSizeOffset), AppleIIGeometry.SectorSize);
        return data;
    }

    /// <summary>Crée une charge utile contenant un en-tête de volume ProDOS valide.</summary>
    private static byte[] CreateProDos(int capacity)
    {
        var data = new byte[capacity];
        var offset = ProDosVolumeHeader.BlockNumber * ProDosVolumeHeader.BlockSize;
        data[offset + ProDosVolumeHeader.StorageAndNameLengthOffset] = (ProDosVolumeHeader.VolumeStorageType << 4) | 1;
        data[offset + ProDosVolumeHeader.EntryLengthOffset] = ProDosVolumeHeader.EntryLength;
        return data;
    }

    /// <summary>Crée une charge utile Macintosh avec la signature de bloc maître indiquée.</summary>
    private static byte[] CreateMacintosh(int capacity, ushort signature)
    {
        var data = new byte[capacity];
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(MacintoshVolumeSignatures.ByteOffset), signature);
        return data;
    }

    /// <summary>Enveloppe une charge utile DOS dans un en-tête 2IMG minimal.</summary>
    private static byte[] BuildTwoImg(byte[] payload)
    {
        var data = new byte[TwoImgLayout.MinimumHeaderSize + payload.Length];
        TwoImgFormat.SignatureBytes.CopyTo(data);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(TwoImgLayout.HeaderSizeOffset), TwoImgLayout.MinimumHeaderSize);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(TwoImgLayout.VersionOffset), TwoImgFormat.SupportedVersion);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(TwoImgLayout.ImageFormatOffset), (uint)TwoImgImageFormat.Dos);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(TwoImgLayout.DataOffsetOffset), TwoImgLayout.MinimumHeaderSize);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(TwoImgLayout.DataLengthOffset), checked((uint)payload.Length));
        payload.CopyTo(data, TwoImgLayout.MinimumHeaderSize);
        return data;
    }

    /// <summary>Enveloppe une charge utile Macintosh dans un conteneur DiskCopy sans tags.</summary>
    private static byte[] BuildDiskCopy(byte[] payload)
    {
        var data = new byte[DiskCopyLayout.HeaderSize + payload.Length];
        BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(DiskCopyLayout.DataLengthOffset), checked((uint)payload.Length));
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(DiskCopyLayout.PrivateWordOffset), DiskCopyFormat.PrivateWord);
        payload.CopyTo(data, DiskCopyLayout.HeaderSize);
        return data;
    }

    /// <summary>Vérifie les dimensions, la capacité et les adresses terminales d'une image.</summary>
    private static void AssertImage(SectorImage image, int blockSize, int cylinders, int heads, int sectorsPerTrack, int capacity)
    {
        Assert.Equal(blockSize, image.BlockSize);
        Assert.Equal(cylinders, image.Cylinders);
        Assert.Equal(heads, image.Heads);
        Assert.Equal(sectorsPerTrack, image.SectorsPerTrack);
        Assert.Equal(capacity, image.Capacity);
        Assert.Equal(0, image.AvailableBlocks.Min(block => block.LogicalBlock));
        Assert.Equal(image.BlockCount - 1, image.AvailableBlocks.Max(block => block.LogicalBlock));
    }

    /// <summary>Vérifie l'identité, la géométrie, les adresses et le contenu de deux images.</summary>
    private static void AssertEquivalent(SectorImage expected, SectorImage actual)
    {
        Assert.Equal(expected.FormatId, actual.FormatId);
        Assert.Equal(expected.BlockSize, actual.BlockSize);
        Assert.Equal(expected.Cylinders, actual.Cylinders);
        Assert.Equal(expected.Heads, actual.Heads);
        Assert.Equal(expected.SectorsPerTrack, actual.SectorsPerTrack);
        Assert.Equal(expected.AvailableBlocks.Select(block => (block.LogicalBlock, block.Address, Data: block.Data.ToArray())), actual.AvailableBlocks.Select(block => (block.LogicalBlock, block.Address, Data: block.Data.ToArray())));
    }
}
