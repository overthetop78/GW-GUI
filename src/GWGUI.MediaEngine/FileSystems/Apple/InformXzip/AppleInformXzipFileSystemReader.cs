using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.InformXzip;

/// <summary>Lit la disposition Apple II XZIP sans catalogue produite par interlz5.</summary>
public sealed class AppleInformXzipFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => Definitions.FileSystemIds.AppleInformXzip;
    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { DiskImageFormatIds.AppleIIDos33, DiskImageFormatIds.AppleIIAppleDos140 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool CanRead(SectorImage image) => HasExpectedGeometry(image) && TryReadStory(image, out var story, out var header) && header!.ChecksumMatches(story);

    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        if (!HasExpectedGeometry(image)) throw AppleInformXzipExceptions.UnsupportedLayout(-1, 0);
        var story = ReadStory(image, out var header);
        if (!header.ChecksumMatches(story)) throw AppleInformXzipExceptions.InvalidChecksum(header.Length);
        var interpreter = ReadLinear(image, 0, AppleInformXzipLayout.InterpreterSectorCount);
        var storyFile = story.AsSpan(0, header.Length).ToArray();
        FileSystemEntry[] entries =
        [
            new("INTERPRETER.BIN", FileSystemEntryKind.File, interpreter.Length, null, string.Empty, 0, 0, true, [], interpreter),
            new($"STORY.Z{header.Version}", FileSystemEntryKind.File, storyFile.Length, null, string.Empty, 0, AppleInformXzipLayout.InterpreterSectorCount, true, [], storyFile)
        ];
        var used = interpreter.LongLength + storyFile.LongLength;
        return new(string.Empty, Definitions.FileSystemIds.AppleInformXzip, image.Capacity, Math.Max(0, image.Capacity - used), null, null, entries, []);
    }

    private static bool HasExpectedGeometry(SectorImage image) => image.BlockSize == AppleInformXzipLayout.SectorSize && image.SectorsPerTrack == AppleInformXzipLayout.SectorsPerTrack && image.Heads == DiskGeometryConstants.SingleSidedHeadCount && image.Cylinders >= AppleInformXzipLayout.TrackCount;

    /// <summary>Lit d'abord l'en-tête, puis uniquement les secteurs nécessaires à la longueur déclarée.</summary>
    private static bool TryReadStory(SectorImage image, out byte[] story, out ZMachineV5Header? header)
    {
        story = [];
        header = null;
        if (!TryReadStorySector(image, 0, out var first) || !ZMachineV5Header.TryParseHeader(first, out header) || header is null) return false;
        var sectorCount = AppleInformXzipLayout.RequiredStorySectors(header.Length);
        story = new byte[sectorCount * AppleInformXzipLayout.SectorSize];
        first.CopyTo(story, 0);
        for (var storySector = 1; storySector < sectorCount; storySector++)
        {
            if (!TryReadStorySector(image, storySector, out var data)) return false;
            data.CopyTo(story, storySector * AppleInformXzipLayout.SectorSize);
        }
        return ZMachineV5Header.TryParse(story, out header);
    }

    /// <summary>Lit l'histoire nécessaire ou signale précisément le premier secteur manquant.</summary>
    private static byte[] ReadStory(SectorImage image, out ZMachineV5Header header)
    {
        if (!TryReadStorySector(image, 0, out var first)) throw AppleInformXzipExceptions.MissingSector(AppleInformXzipLayout.InterpreterSectorCount);
        if (!ZMachineV5Header.TryParseHeader(first, out var parsed) || parsed is null) throw AppleInformXzipExceptions.UnsupportedLayout(first[AppleInformXzipLayout.VersionOffset], first.Length);
        var sectorCount = AppleInformXzipLayout.RequiredStorySectors(parsed.Length);
        var story = new byte[sectorCount * AppleInformXzipLayout.SectorSize];
        first.CopyTo(story, 0);
        for (var storySector = 1; storySector < sectorCount; storySector++)
        {
            if (!TryReadStorySector(image, storySector, out var data)) throw AppleInformXzipExceptions.MissingSector(StoredStorySector(storySector));
            data.CopyTo(story, storySector * AppleInformXzipLayout.SectorSize);
        }
        if (!ZMachineV5Header.TryParse(story, out parsed) || parsed is null) throw AppleInformXzipExceptions.UnsupportedLayout(first[AppleInformXzipLayout.VersionOffset], story.Length);
        header = parsed;
        return story;
    }

    private static bool TryReadStorySector(SectorImage image, int storySector, out byte[] data)
    {
        var storedSector = StoredStorySector(storySector);
        data = [];
        if (!image.TryGetBlock(storedSector, out var block) || block.Data.Count != AppleInformXzipLayout.SectorSize) return false;
        data = block.Data.ToArray();
        return true;
    }

    private static int StoredStorySector(int storySector) => AppleInformXzipLayout.InterpreterSectorCount + (storySector & AppleInformXzipLayout.StoryTrackMask) + AppleInformXzipLayout.StoredSectorIndex(storySector & AppleInformXzipLayout.SectorInTrackMask);

    private static byte[] ReadLinear(SectorImage image, int firstSector, int sectorCount)
    {
        var output = new byte[sectorCount * AppleInformXzipLayout.SectorSize];
        for (var logical = firstSector; logical < firstSector + sectorCount; logical++)
        {
            if (!image.TryGetBlock(logical, out var block) || block.Data.Count != AppleInformXzipLayout.SectorSize) throw AppleInformXzipExceptions.MissingSector(logical);
            var offset = (logical - firstSector) * AppleInformXzipLayout.SectorSize;
            for (var index = 0; index < block.Data.Count; index++) output[offset + index] = block.Data[index];
        }
        return output;
    }
}
