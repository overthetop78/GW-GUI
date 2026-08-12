using System.Buffers.Binary;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Acorn.FileCore;

/// <summary>Contient les paramètres validés d'un DiscRecord FileCore.</summary>
public sealed record AcornFileCoreDiscRecord(int Log2SectorSize, int IdLength, int Log2BytesPerMapBit, int ZoneCount, int ZoneSpareBits, int RootAddress, long DiscSize, string DiscName, int Log2ShareSize)
{
    /// <summary>Taille utile d'une zone en bits.</summary>
    public int ZoneSizeBits => (BitPrimitives.BitsPerByte << Log2SectorSize) - ZoneSpareBits;

    /// <summary>Tente de lire et valider un DiscRecord.</summary>
    public static bool TryParse(ReadOnlySpan<byte> data, long imageCapacity, out AcornFileCoreDiscRecord? record)
    {
        record = null;
        if (data.Length < AcornFileCoreLayout.DiscRecordLength) return false;
        var log2Sector = data[AcornFileCoreDiscRecordLayout.Log2SectorSizeOffset];
        var idLength = data[AcornFileCoreDiscRecordLayout.IdLengthOffset];
        var log2MapBit = data[AcornFileCoreDiscRecordLayout.Log2BytesPerMapBitOffset];
        var zoneCount = data[AcornFileCoreDiscRecordLayout.ZoneCountLowOffset] | data[AcornFileCoreDiscRecordLayout.ZoneCountHighOffset] << BitPrimitives.BitsPerByte;
        var zoneSpare = BinaryPrimitives.ReadUInt16LittleEndian(data[AcornFileCoreDiscRecordLayout.ZoneSpareBitsOffset..]);
        var root = BinaryPrimitives.ReadInt32LittleEndian(data[AcornFileCoreDiscRecordLayout.RootAddressOffset..]);
        var discSize = ComposeDiscSize(BinaryPrimitives.ReadUInt32LittleEndian(data[AcornFileCoreDiscRecordLayout.DiscSizeLowOffset..]), BinaryPrimitives.ReadUInt32LittleEndian(data[AcornFileCoreDiscRecordLayout.DiscSizeHighOffset..]));
        var sectorBitCount = BitPrimitives.BitsPerByte << log2Sector;
        if (log2Sector is < AcornFileCoreDiscRecordLayout.MinimumLog2SectorSize or > AcornFileCoreDiscRecordLayout.MaximumLog2SectorSize || idLength < log2Sector + AcornFileCoreDiscRecordLayout.MinimumIdExtraBits || idLength > AcornFileCoreDiscRecordLayout.MaximumIdLength || log2MapBit > log2Sector || zoneCount < AcornFileCoreDiscRecordLayout.MinimumZoneCount || root < AcornFileCoreDiscRecordLayout.MinimumRootAddress || discSize < AcornFileCoreDiscRecordLayout.MinimumDiscSize || discSize > imageCapacity || zoneSpare >= sectorBitCount) return false;
        var name = AcornFileCoreNameCodec.Decode(data.Slice(AcornFileCoreDiscRecordLayout.DiscNameOffset, AcornFileCoreDiscRecordLayout.DiscNameLength));
        record = new(log2Sector, idLength, log2MapBit, zoneCount, zoneSpare, root, discSize, name, data[AcornFileCoreDiscRecordLayout.Log2ShareSizeOffset] & AcornFileCoreDiscRecordLayout.Log2ShareSizeMask);
        return true;
    }

    /// <summary>Compose une taille sur 64 bits depuis ses mots little-endian.</summary>
    public static long ComposeDiscSize(uint low, uint high) => (long)low | (long)high << 32;
}
