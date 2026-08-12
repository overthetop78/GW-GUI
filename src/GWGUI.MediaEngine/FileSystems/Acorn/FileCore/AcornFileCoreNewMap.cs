using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Acorn.FileCore;

/// <summary>Résout les adresses indirectes d'une new-map FileCore en offsets physiques de l'image.</summary>
internal sealed class AcornFileCoreNewMap : IFileCoreAddressResolver
{
    private readonly SectorImage _image;
    private readonly AcornFileCoreZone[] _zones;
    private readonly int _idLength;
    private readonly int _mapToBlockShift;
    private readonly int _log2ShareSize;
    private readonly int _idsPerZone;

    private AcornFileCoreNewMap(SectorImage image, AcornFileCoreDiscRecord record, AcornFileCoreZone[] zones)
    {
        _image = image;
        Record = record;
        _zones = zones;
        _idLength = record.IdLength;
        _mapToBlockShift = record.Log2BytesPerMapBit - record.Log2SectorSize;
        _log2ShareSize = record.Log2ShareSize;
        _idsPerZone = record.ZoneSizeBits / (_idLength + 1);
    }

    internal AcornFileCoreDiscRecord Record { get; }
    /// <inheritdoc />
    public int RootAddress => Record.RootAddress;
    /// <inheritdoc />
    public string VolumeName => Record.DiscName;
    /// <inheritdoc />
    public long FreeBytes => ReadFreeBytes();

    /// <summary>Tente de construire le résolveur à partir des zones présentes dans l'image.</summary>
    internal static bool TryCreate(SectorImage image, out AcornFileCoreNewMap? map)
    {
        map = null;
        if (!image.TryGetBlock(AcornFileCoreLayout.DiscRecordBlock, out var firstBlock) || firstBlock.Data.Count < AcornFileCoreLayout.DiscRecordOffset + AcornFileCoreLayout.DiscRecordLength)
            return false;

        var bytes = firstBlock.Data.ToArray();
        var recordBytes = bytes.AsSpan(AcornFileCoreLayout.DiscRecordOffset, AcornFileCoreLayout.DiscRecordLength);
        if (!AcornFileCoreDiscRecord.TryParse(recordBytes, image.Capacity, out var record) || record is null) return false;

        var mapToBlockShift = record.Log2BytesPerMapBit - record.Log2SectorSize;
        var mapAddress = CalculateMapAddress(record, mapToBlockShift);
        if (mapAddress < 0 || mapAddress + record.ZoneCount > image.BlockCount) return false;

        var zones = new AcornFileCoreZone[record.ZoneCount];
        var describedBits = CalculateDescribedBits(image.Capacity, record.Log2BytesPerMapBit);
        for (var index = 0; index < zones.Length; index++)
        {
            if (!image.TryGetBlock(mapAddress + index, out var block) || block.Data.Count != image.BlockSize || !TryCreateZone(block.Data, index, zones.Length, record, describedBits, out zones[index])) return false;
        }

        map = new(image, record, zones);
        return true;
    }

    /// <summary>Calcule l'adresse du premier bloc de carte.</summary>
    private static int CalculateMapAddress(AcornFileCoreDiscRecord record, int mapToBlockShift)
    {
        var mapBitsBeforeCentre = (record.ZoneCount >> 1) * record.ZoneSizeBits;
        var firstZoneDiscRecordCorrection = record.ZoneCount > 1 ? AcornFileCoreLayout.DiscRecordBitLength : 0;
        return AcornFileCoreShift.Apply(mapBitsBeforeCentre - firstZoneDiscRecordCorrection, mapToBlockShift);
    }

    /// <summary>Calcule le nombre de bits de carte nécessaires pour décrire la capacité.</summary>
    private static int CalculateDescribedBits(long capacity, int log2BytesPerMapBit) => checked((int)(capacity >> log2BytesPerMapBit));

    /// <summary>Construit une zone après validation de ses limites utiles.</summary>
    private static bool TryCreateZone(IReadOnlyCollection<byte> data, int index, int zoneCount, AcornFileCoreDiscRecord record, int describedBits, out AcornFileCoreZone zone)
    {
        zone = null!;
        var containsDiscRecord = index == 0;
        var startBit = AcornFileCoreLayout.ZoneHeaderBitLength + (containsDiscRecord ? AcornFileCoreLayout.DiscRecordBitLength : 0);
        var startMapBit = containsDiscRecord ? 0 : index * record.ZoneSizeBits - AcornFileCoreLayout.DiscRecordBitLength;
        var precedingMapBits = (zoneCount - 1) * record.ZoneSizeBits - AcornFileCoreLayout.DiscRecordBitLength;
        var describedZoneBits = index == zoneCount - 1 ? describedBits - precedingMapBits : record.ZoneSizeBits;
        var endBit = AcornFileCoreLayout.ZoneHeaderBitLength + describedZoneBits;
        if (startBit >= endBit || endBit > data.Count * BitPrimitives.BitsPerByte) return false;
        zone = new(data, startMapBit, startBit, endBit);
        return true;
    }

    /// <inheritdoc />
    public bool TryResolveByteOffset(int indirectAddress, long objectByteOffset, out long physicalByteOffset)
    {
        physicalByteOffset = 0;
        if (indirectAddress <= 0 || objectByteOffset < 0) return false;
        var logicalBlock = checked((int)(objectByteOffset / _image.BlockSize));
        var offsetInBlock = checked((int)(objectByteOffset % _image.BlockSize));
        if (!TryResolveBlock(indirectAddress, logicalBlock, out var physicalBlock)) return false;
        physicalByteOffset = (long)physicalBlock * _image.BlockSize + offsetInBlock;
        return physicalByteOffset >= 0 && physicalByteOffset < _image.Capacity;
    }

    /// <summary>Calcule le nombre d'octets libres décrit par les listes libres des zones.</summary>
    internal long ReadFreeBytes()
    {
        long freeMapBits = 0;
        var fragmentIdLength = Math.Min(_idLength, AcornFileCoreLayout.MaximumFreeIdBitLength);
        var idMask = (1u << fragmentIdLength) - 1;
        foreach (var zone in _zones) freeMapBits += ReadZoneFreeMapBits(zone, idMask);
        var bytesPerMapBit = 1L << Record.Log2BytesPerMapBit;
        return Math.Min(_image.Capacity, checked(freeMapBits * bytesPerMapBit));
    }

    /// <summary>Parcourt la liste libre d'une zone et retourne son nombre de bits de carte libres.</summary>
    private long ReadZoneFreeMapBits(AcornFileCoreZone zone, uint idMask)
    {
        var link = checked((int)AcornFileCoreBitReader.GetBits(zone.Data, AcornFileCoreLayout.FreeLinkBitOffset, idMask));
        if (link == 0) return 0;
        long freeMapBits = 0;
        var start = AcornFileCoreLayout.FreeLinkBitOffset;
        while (true)
        {
            start += link;
            if (start < zone.StartBit || start >= zone.EndBit) return freeMapBits;
            var fragment = AcornFileCoreBitReader.GetBits(zone.Data, start, idMask);
            var fragmentEnd = AcornFileCoreBitReader.FindNextSetBit(zone.Data, start + _idLength, zone.EndBit);
            if (fragmentEnd >= zone.EndBit) return freeMapBits;
            freeMapBits += fragmentEnd + 1L - start;
            if (fragment < _idLength + 1) return freeMapBits;
            link = checked((int)fragment);
        }
    }

    /// <summary>Tente de résoudre un bloc logique d'une adresse indirecte.</summary>
    private bool TryResolveBlock(int indirectAddress, int objectBlock, out int physicalBlock)
    {
        physicalBlock = 0;
        var block = objectBlock;
        var address = AcornFileCoreAddress.Decode(indirectAddress);
        if (address.ShareOffset != 0) block += (address.ShareOffset - AcornFileCoreLayout.ShareOffsetBias) << _log2ShareSize;
        var fragmentId = address.FragmentId;
        if (_idsPerZone <= 0) return false;
        var zoneIndex = fragmentId == AcornFileCoreLayout.RootFragmentId ? _zones.Length >> 1 : checked((int)(fragmentId / _idsPerZone));
        if (zoneIndex < 0 || zoneIndex >= _zones.Length) return false;

        var mapOffset = AcornFileCoreShift.Apply(block, -_mapToBlockShift);
        if (mapOffset < 0) return false;
        var remainingOffset = mapOffset;
        for (var scanned = 0; scanned < _zones.Length; scanned++)
        {
            var zone = _zones[(zoneIndex + scanned) % _zones.Length];
            if (!TryLookupZone(zone, fragmentId, ref remainingOffset, out var foundBit)) continue;
            var resolved = ResolvePhysicalBlock(block, mapOffset, foundBit - zone.StartBit + zone.StartMapBit);
            if (resolved <= 0 || resolved >= _image.BlockCount) return false;
            physicalBlock = resolved;
            return true;
        }
        return false;
    }

    /// <summary>Convertit une position dans la carte en numéro de bloc physique.</summary>
    private int ResolvePhysicalBlock(int logicalBlock, int mapOffset, int mapPosition)
    {
        var sectorOffset = logicalBlock - AcornFileCoreShift.Apply(mapOffset, _mapToBlockShift);
        return checked(sectorOffset + AcornFileCoreShift.Apply(mapPosition, _mapToBlockShift));
    }

    /// <summary>Recherche un fragment et consomme son offset dans une zone.</summary>
    private bool TryLookupZone(AcornFileCoreZone zone, uint fragmentId, ref int offset, out int foundBit)
    {
        foundBit = 0;
        var idMask = (1u << _idLength) - 1;
        var freeMask = idMask & AcornFileCoreLayout.FreeLinkMask;
        var freeLink = ReadFirstFreeLink(zone, freeMask);
        var start = zone.StartBit;
        while (start < zone.EndBit)
        {
            var fragment = AcornFileCoreBitReader.GetBits(zone.Data, start, idMask);
            var fragmentEnd = AcornFileCoreBitReader.FindNextSetBit(zone.Data, start + _idLength, zone.EndBit);
            if (fragmentEnd >= zone.EndBit) return false;
            if (start == freeLink)
            {
                freeLink = AdvanceFreeLink(freeLink, fragment);
            }
            else if (fragment == fragmentId && TryConsumeFragment(start, fragmentEnd, ref offset, out foundBit)) return true;
            start = fragmentEnd + 1;
        }
        return false;
    }

    /// <summary>Lit la première position de la chaîne libre d'une zone.</summary>
    private static int ReadFirstFreeLink(AcornFileCoreZone zone, uint freeMask)
    {
        var value = AcornFileCoreBitReader.GetBits(zone.Data, AcornFileCoreLayout.FreeLinkBitOffset, freeMask);
        return value == 0 ? 0 : checked(AcornFileCoreLayout.FreeLinkBitOffset + (int)value);
    }

    /// <summary>Avance une position dans la chaîne libre à partir du fragment courant.</summary>
    internal static int AdvanceFreeLink(int currentLink, uint fragment) => checked(currentLink + (int)(fragment & AcornFileCoreLayout.FreeLinkMask));

    /// <summary>Consomme la longueur d'un fragment ou retourne le bit demandé lorsqu'il appartient au fragment.</summary>
    private static bool TryConsumeFragment(int start, int end, ref int offset, out int foundBit)
    {
        foundBit = 0;
        var length = checked(end + 1 - start);
        if (offset >= length)
        {
            offset -= length;
            return false;
        }
        foundBit = checked(start + offset);
        return true;
    }

}
