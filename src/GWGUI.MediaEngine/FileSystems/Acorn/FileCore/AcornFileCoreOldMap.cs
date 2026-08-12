using GWGUI.MediaEngine.FileSystems.Acorn.Adfs;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Acorn.FileCore;

/// <summary>Résout les adresses d'une ancienne carte FileCore.</summary>
public sealed class AcornFileCoreOldMap : IFileCoreAddressResolver
{
    private const int RootDirectoryAddress = 4;
    private const int FirstNameHalfOffset = 247;
    private const int SecondNameHalfOffset = 502;
    private const int NameHalfLength = 5;
    private const int FreeEntriesOffset = 256;
    private const int FreeEntryCount = 82;
    private readonly long _capacity;

    /// <summary>Crée le résolveur depuis le premier bloc et la capacité de l'image.</summary>
    public AcornFileCoreOldMap(ReadOnlySpan<byte> map, long capacity)
    {
        _capacity = capacity;
        RootAddress = RootDirectoryAddress;
        VolumeName = ReadName(map);
        FreeBytes = ReadFreeBytes(map, capacity);
    }

    /// <inheritdoc />
    public int RootAddress { get; }
    /// <inheritdoc />
    public string VolumeName { get; }
    /// <inheritdoc />
    public long FreeBytes { get; }

    /// <inheritdoc />
    public bool TryResolveByteOffset(int indirectAddress, long objectByteOffset, out long physicalByteOffset)
    {
        physicalByteOffset = (long)indirectAddress * AcornAdfsLayout.FileCoreUnitSize + objectByteOffset;
        return indirectAddress > 0 && objectByteOffset >= 0 && physicalByteOffset >= 0 && physicalByteOffset < _capacity;
    }

    private static string ReadName(ReadOnlySpan<byte> map)
    {
        if (map.Length < SecondNameHalfOffset + NameHalfLength) return string.Empty;
        Span<byte> name = stackalloc byte[NameHalfLength * 2];
        for (var index = 0; index < NameHalfLength; index++) { name[index * 2] = map[FirstNameHalfOffset + index]; name[index * 2 + 1] = map[SecondNameHalfOffset + index]; }
        return AcornAdfsNameCodec.Decode(name);
    }

    private static long ReadFreeBytes(ReadOnlySpan<byte> map, long capacity)
    {
        if (map.Length < FreeEntriesOffset + FreeEntryCount * LittleEndianUInt24.Size) return 0;
        long sectors = 0;
        for (var index = 0; index < FreeEntryCount; index++) sectors += LittleEndianUInt24.Read(map, FreeEntriesOffset + index * LittleEndianUInt24.Size);
        return Math.Min(capacity, sectors * (long)AcornAdfsLayout.FileCoreUnitSize);
    }
}
