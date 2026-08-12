using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;


using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>
/// Reads the catalog-less Apple II XZIP disk layout produced by interlz5.
/// The first 16 KiB contain the interpreter and the remaining used sectors
/// contain an interleaved Z-machine v5 story file.
/// </summary>
public sealed class AppleInformXzipFileSystemReader : IFileSystemReader
{
    private const int SectorSize = 256;
    private const int InterpreterSectors = 64;
    private const int MaximumStorySectors = 394;

    private static readonly int[] Interleave = [0, 7, 14, 6, 13, 5, 12, 4, 11, 3, 10, 2, 9, 1, 8, 15];

    public string Id => Definitions.FileSystemIds.AppleInformXzip;
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.AppleIIDos33, DiskImageFormatIds.AppleIIAppleDos140 };

    public bool CanRead(SectorImage image)
    {
        if (image.BlockSize != SectorSize || image.SectorsPerTrack != 16 || image.Heads != DiskGeometryConstants.SingleSidedHeadCount || image.BlockCount < 35 * 16)
            return false;

        var story = ReadStory(image, headerOnly: false);
        return TryReadStoryLength(story, out var length) && ChecksumMatches(story, length);
    }

    public FileSystemVolume Read(SectorImage image)
    {
        var story = ReadStory(image, headerOnly: false);
        if (!TryReadStoryLength(story, out var storyLength) || !ChecksumMatches(story, storyLength))
            throw new InvalidDataException("The image does not contain an Apple II Inform/XZIP disk layout.");

        var interpreter = ReadLinear(image, 0, InterpreterSectors);
        var storyFile = story.Take(storyLength).ToArray();
        var version = storyFile[0];

        FileSystemEntry[] entries =
        [
            new("INTERPRETER.BIN", FileSystemEntryKind.File, interpreter.Length, null, "", 0, 0, true, [], interpreter),
            new($"STORY.Z{version}", FileSystemEntryKind.File, storyFile.Length, null, "", 0, InterpreterSectors, true, [], storyFile)
        ];

        var used = interpreter.LongLength + storyFile.LongLength;
        return new("", "Apple II Inform/XZIP", image.Capacity, Math.Max(0, image.Capacity - used), null, null,
            entries, []);
    }

    private static byte[] ReadStory(SectorImage image, bool headerOnly)
    {
        var sectorCount = headerOnly ? 1 : MaximumStorySectors;
        using var output = new MemoryStream(sectorCount * SectorSize);
        for (var storySector = 0; storySector < sectorCount; storySector++)
        {
            var storedSector = InterpreterSectors + (storySector & 0xff0) +
                Array.IndexOf(Interleave, storySector & 0x0f);
            if (!image.TryGetBlock(storedSector, out var block) || block.Data.Count != SectorSize)
                return [];
            output.Write(block.Data.ToArray());
        }
        return output.ToArray();
    }

    private static byte[] ReadLinear(SectorImage image, int firstSector, int sectorCount)
    {
        using var output = new MemoryStream(sectorCount * SectorSize);
        for (var logical = firstSector; logical < firstSector + sectorCount; logical++)
        {
            if (!image.TryGetBlock(logical, out var block) || block.Data.Count != SectorSize)
                throw new InvalidDataException($"Apple II sector {logical} is missing.");
            output.Write(block.Data.ToArray());
        }
        return output.ToArray();
    }

    private static bool TryReadStoryLength(ReadOnlySpan<byte> story, out int length)
    {
        length = 0;
        if (story.Length < 64 || story[0] != 5) return false;
        var units = BinaryPrimitives.ReadUInt16BigEndian(story.Slice(0x1a, 2));
        length = units * 4;
        if (length is < 64 or > MaximumStorySectors * SectorSize || length > story.Length) return false;

        var highMemory = BinaryPrimitives.ReadUInt16BigEndian(story.Slice(0x04, 2));
        var initialPc = BinaryPrimitives.ReadUInt16BigEndian(story.Slice(0x06, 2));
        var dictionary = BinaryPrimitives.ReadUInt16BigEndian(story.Slice(0x08, 2));
        var objects = BinaryPrimitives.ReadUInt16BigEndian(story.Slice(0x0a, 2));
        var globals = BinaryPrimitives.ReadUInt16BigEndian(story.Slice(0x0c, 2));
        var staticMemory = BinaryPrimitives.ReadUInt16BigEndian(story.Slice(0x0e, 2));
        return highMemory >= 64 && initialPc >= highMemory && initialPc < length &&
            dictionary >= 64 && dictionary < length && objects >= 64 && objects < length &&
            globals >= 64 && globals < length && staticMemory >= 64 && staticMemory < length;
    }

    private static bool ChecksumMatches(ReadOnlySpan<byte> story, int length)
    {
        var expected = BinaryPrimitives.ReadUInt16BigEndian(story.Slice(0x1c, 2));
        var checksum = 0;
        for (var index = 0x40; index < length; index++) checksum = (checksum + story[index]) & 0xffff;
        return checksum == expected;
    }
}
