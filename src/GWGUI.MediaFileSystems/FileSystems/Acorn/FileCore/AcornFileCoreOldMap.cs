using GWGUI.MediaFileSystems.FileSystems.Acorn.Adfs;
using GWGUI.MediaFileSystems.Primitives;

namespace GWGUI.MediaFileSystems.FileSystems.Acorn.FileCore;

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
    private readonly int _cylinders, _heads, _sectorsPerTrack;

    /// <summary>Crée le résolveur depuis le premier bloc et la capacité de l'image.</summary>
    public AcornFileCoreOldMap(ReadOnlySpan<byte> map, long capacity, int rootAddress = RootDirectoryAddress, int cylinders = 0, int heads = 1, int sectorsPerTrack = 0)
    {
        _capacity = capacity;
        _cylinders = cylinders; _heads = heads; _sectorsPerTrack = sectorsPerTrack;
        RootAddress = rootAddress;
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
        if (indirectAddress <= 0 || objectByteOffset < 0 || physicalByteOffset < 0 || physicalByteOffset >= _capacity) return false;
        // ADFS L numérote toute la face 0 avant la face 1 ; le conteneur ADL entrelace les pistes.
        if (_cylinders > 0 && _heads > 1 && _sectorsPerTrack > 0)
        {
            var sector = physicalByteOffset / AcornAdfsLayout.FileCoreUnitSize;
            var sectorsPerSide = (long)_cylinders * _sectorsPerTrack;
            var head = sector / sectorsPerSide;
            var cylinder = sector % sectorsPerSide / _sectorsPerTrack;
            var physicalSector = (cylinder * _heads + head) * _sectorsPerTrack + sector % _sectorsPerTrack;
            physicalByteOffset = physicalSector * AcornAdfsLayout.FileCoreUnitSize + physicalByteOffset % AcornAdfsLayout.FileCoreUnitSize;
        }
        return physicalByteOffset < _capacity;
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
