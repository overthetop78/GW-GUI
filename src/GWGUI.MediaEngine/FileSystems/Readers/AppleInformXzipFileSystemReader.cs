using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.InformXzip;
using GWGUI.MediaEngine.SectorImages;


using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Lit la disposition Apple II XZIP sans catalogue produite par interlz5.</summary>
public sealed class AppleInformXzipFileSystemReader : IFileSystemReader
{
    /// <summary>Identifiant technique du lecteur.</summary>
    public string Id => Definitions.FileSystemIds.AppleInformXzip;
    /// <summary>Formats sectoriels dans lesquels la disposition peut apparaître.</summary>
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.AppleIIDos33, DiskImageFormatIds.AppleIIAppleDos140 };

    /// <summary>Indique si l'image contient une histoire version 5 valide.</summary>
    public bool CanRead(SectorImage image)
    {
        if (image.BlockSize != AppleInformXzipLayout.SectorSize || image.SectorsPerTrack != AppleInformXzipLayout.SectorsPerTrack || image.Heads != DiskGeometryConstants.SingleSidedHeadCount || image.BlockCount < AppleInformXzipLayout.TrackCount * AppleInformXzipLayout.SectorsPerTrack)
            return false;

        var story = ReadStory(image, throwOnMissing: false);
        return ZMachineStoryHeader.TryParse(story, out var header) && header.ChecksumMatches(story);
    }

    /// <summary>Extrait l'interpréteur et l'histoire Z-machine.</summary>
    public FileSystemVolume Read(SectorImage image)
    {
        var story = ReadStory(image, throwOnMissing: true);
        if (!ZMachineStoryHeader.TryParse(story, out var header)) throw AppleInformXzipExceptions.UnsupportedLayout(story.Length == 0 ? -1 : story[AppleInformXzipLayout.VersionOffset], story.Length);
        if (!header.ChecksumMatches(story)) throw AppleInformXzipExceptions.InvalidChecksum(header.Length);

        var interpreter = ReadLinear(image, 0, AppleInformXzipLayout.InterpreterSectorCount);
        var storyFile = story.Take(header.Length).ToArray();

        FileSystemEntry[] entries =
        [
            new("INTERPRETER.BIN", FileSystemEntryKind.File, interpreter.Length, null, "", 0, 0, true, [], interpreter),
            new($"STORY.Z{header.Version}", FileSystemEntryKind.File, storyFile.Length, null, "", 0, AppleInformXzipLayout.InterpreterSectorCount, true, [], storyFile)
        ];

        var used = interpreter.LongLength + storyFile.LongLength;
        return new("", Definitions.FileSystemDisplayNames.AppleInformXzip, image.Capacity, Math.Max(0, image.Capacity - used), null, null,
            entries, []);
    }

    /// <summary>Lit les secteurs entrelacés de l'histoire.</summary>
    private static byte[] ReadStory(SectorImage image, bool throwOnMissing)
    {
        using var output = new MemoryStream(AppleInformXzipLayout.MaximumStorySectorCount * AppleInformXzipLayout.SectorSize);
        for (var storySector = 0; storySector < AppleInformXzipLayout.MaximumStorySectorCount; storySector++)
        {
            var storedSector = AppleInformXzipLayout.InterpreterSectorCount + (storySector & AppleInformXzipLayout.StoryTrackMask) + AppleInformXzipLayout.StoredSectorIndex(storySector & AppleInformXzipLayout.SectorInTrackMask);
            if (!image.TryGetBlock(storedSector, out var block) || block.Data.Count != AppleInformXzipLayout.SectorSize)
            {
                if (throwOnMissing) throw AppleInformXzipExceptions.MissingSector(storedSector);
                return [];
            }
            output.Write(block.Data.ToArray());
        }
        return output.ToArray();
    }

    /// <summary>Lit une plage linéaire de secteurs.</summary>
    private static byte[] ReadLinear(SectorImage image, int firstSector, int sectorCount)
    {
        using var output = new MemoryStream(sectorCount * AppleInformXzipLayout.SectorSize);
        for (var logical = firstSector; logical < firstSector + sectorCount; logical++)
        {
            if (!image.TryGetBlock(logical, out var block) || block.Data.Count != AppleInformXzipLayout.SectorSize) throw AppleInformXzipExceptions.MissingSector(logical);
            output.Write(block.Data.ToArray());
        }
        return output.ToArray();
    }

}
