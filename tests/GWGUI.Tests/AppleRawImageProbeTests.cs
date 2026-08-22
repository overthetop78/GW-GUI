using GWGUI.MediaEngine.FileSystems.Apple.Dos;
using GWGUI.MediaEngine.FileSystems.Apple.Lisa;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.FileSystems.Apple.ProDos;
using GWGUI.MediaEngine.FileSystems.Macintosh;
using GWGUI.MediaEngine.FileSystems.Sos;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.Recognition.Apple;
using System.Buffers.Binary;

namespace GWGUI.Tests;

/// <summary>Vérifie les sondes structurelles des représentations Apple brutes.</summary>
public sealed class AppleRawImageProbeTests
{
    /// <summary>Vérifie un VTOC DOS 3.3 valide et un voisin dont le secteur de catalogue est hors plage.</summary>
    [Fact]
    public void ProbesDos33VtocWithoutFalsePositive()
    {
        var data = new byte[AppleIIGeometry.Capacity];
        var offset = AppleDosFileSystemLayout.VtocTrack * AppleIIGeometry.TrackSize;
        data[offset + AppleDosFileSystemLayout.VtocCatalogTrackOffset] = 1;
        data[offset + AppleDosFileSystemLayout.VtocCatalogSectorOffset] = 0;
        data[offset + AppleDosFileSystemLayout.VtocSectorsPerTrackOffset] = AppleIIGeometry.SectorsPerTrack;
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset + AppleDosFileSystemLayout.VtocSectorSizeOffset), AppleIIGeometry.SectorSize);
        Assert.True(AppleRawImageProbe.LooksLikeDos33(data));
        data[offset + AppleDosFileSystemLayout.VtocCatalogSectorOffset] = AppleIIGeometry.SectorsPerTrack;
        Assert.False(AppleRawImageProbe.LooksLikeDos33(data));
    }

    /// <summary>Vérifie un en-tête ProDOS valide et un voisin sans nom de volume.</summary>
    [Fact]
    public void ProbesProDosVolumeHeaderWithoutFalsePositive()
    {
        var data = new byte[(ProDosFileSystemLayout.RootBlock + 1) * ProDosFileSystemLayout.BlockSize];
        var offset = ProDosFileSystemLayout.RootBlock * ProDosFileSystemLayout.BlockSize;
        data[offset + ProDosFileSystemLayout.HeaderOffset] = (byte)(((int)ProDosStorageType.VolumeHeader << ProDosFileSystemLayout.StorageTypeShift) | 1);
        data[offset + ProDosFileSystemLayout.HeaderEntryLengthOffset] = ProDosFileSystemLayout.EntrySize;
        Assert.True(AppleRawImageProbe.LooksLikeProDos(data));
        data[offset + ProDosFileSystemLayout.HeaderOffset] = (byte)((int)ProDosStorageType.VolumeHeader << ProDosFileSystemLayout.StorageTypeShift);
        Assert.False(AppleRawImageProbe.LooksLikeProDos(data));
    }

    /// <summary>Vérifie séparément les signatures MFS, HFS et une valeur voisine inconnue.</summary>
    [Fact]
    public void ProbesMacintoshMasterDirectoryBlockSignatures()
    {
        var data = new byte[MacintoshVolumeSignatures.MinimumImageLength];
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(MacintoshVolumeSignatures.ByteOffset), MacintoshVolumeSignatures.Mfs);
        Assert.True(AppleRawImageProbe.LooksLikeMac(data));
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(MacintoshVolumeSignatures.ByteOffset), MacintoshVolumeSignatures.Hfs);
        Assert.True(AppleRawImageProbe.LooksLikeMac(data));
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(MacintoshVolumeSignatures.ByteOffset), MacintoshVolumeSignatures.Hfs + 1);
        Assert.False(AppleRawImageProbe.LooksLikeMac(data));
    }

    /// <summary>Vérifie une page Lisa valide ainsi que les versions, noms et caractères invalides proches.</summary>
    [Fact]
    public void ProbesLisaVersionAndPrintableName()
    {
        var data = CreateLisaImage((ushort)LisaCatalogVersion.Table, "Lisa"u8);
        Assert.True(AppleRawImageProbe.LooksLikeLisaOffice(data));
        Assert.False(AppleRawImageProbe.LooksLikeLisaOffice(CreateLisaImage(0x0010, "Lisa"u8)));
        Assert.False(AppleRawImageProbe.LooksLikeLisaOffice(CreateLisaImage((ushort)LisaCatalogVersion.Hash, [])));
        Assert.False(AppleRawImageProbe.LooksLikeLisaOffice(CreateLisaImage((ushort)LisaCatalogVersion.BTree, new byte[LisaVolumeHeader.MaximumNameLength + 1])));
        Assert.False(AppleRawImageProbe.LooksLikeLisaOffice(CreateLisaImage((ushort)LisaCatalogVersion.Table, [0x1F])));
    }

    /// <summary>Vérifie la recherche ASCII bornée et sans casse du marqueur SOS.</summary>
    [Fact]
    public void ProbesSosMarkerWithoutDecodingAString()
    {
        var data = new byte[SosBootFormat.ImageCapacity];
        "sos"u8.CopyTo(data.AsSpan(SosBootFormat.SearchLength - SosBootFormat.Marker.Length));
        Assert.True(AppleRawImageProbe.LooksLikeSos(data));
        data[SosBootFormat.SearchLength - 1] = (byte)'X';
        Assert.False(AppleRawImageProbe.LooksLikeSos(data));
    }

    /// <summary>Vérifie qu'aucune sonde ne lit au-delà d'un tampon vide ou juste trop court.</summary>
    [Fact]
    public void RejectsEmptyAndShortBuffers()
    {
        Assert.False(AppleRawImageProbe.LooksLikeDos33([]));
        Assert.False(AppleRawImageProbe.LooksLikeDos33(new byte[AppleIIGeometry.Capacity - 1]));
        Assert.False(AppleRawImageProbe.LooksLikeProDos([]));
        Assert.False(AppleRawImageProbe.LooksLikeProDos(new byte[((ProDosFileSystemLayout.RootBlock + 1) * ProDosFileSystemLayout.BlockSize) - 1]));
        Assert.False(AppleRawImageProbe.LooksLikeMac([]));
        Assert.False(AppleRawImageProbe.LooksLikeMac(new byte[MacintoshVolumeSignatures.MinimumImageLength - 1]));
        Assert.False(AppleRawImageProbe.LooksLikeLisaOffice([]));
        Assert.False(AppleRawImageProbe.LooksLikeLisaOffice(new byte[LisaVolumeHeader.Capacity - 1]));
        Assert.False(AppleRawImageProbe.LooksLikeSos([]));
        Assert.False(AppleRawImageProbe.LooksLikeSos(new byte[SosBootFormat.ImageCapacity - 1]));
    }

    /// <summary>Crée une image Lisa dont la première page contient les champs soumis au sondage.</summary>
    private static byte[] CreateLisaImage(ushort version, ReadOnlySpan<byte> name)
    {
        var data = new byte[LisaVolumeHeader.Capacity];
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(LisaVolumeHeader.VersionOffset), version);
        data[LisaVolumeHeader.NameLengthOffset] = checked((byte)name.Length);
        name.CopyTo(data.AsSpan(LisaVolumeHeader.NameOffset));
        return data;
    }
}
