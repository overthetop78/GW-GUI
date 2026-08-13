using GWGUI.Domain.Parity;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.App.Services.Parity;

public static class MediaParityComparisonService
{
    public static MediaParityRow Compare(
        string formatId,
        string sourceContainer,
        string targetContainer,
        ExploredDiskImage mediaEngine,
        ExploredDiskImage greaseweazle,
        ParityValidationStatus fluxIdentical = ParityValidationStatus.NotApplicable,
        ParityValidationStatus physicalWrite = ParityValidationStatus.Pending,
        bool gwFallbackAvailable = true)
    {
        var sameGeometry = SameGeometry(mediaEngine.Image, greaseweazle.Image);
        var sameBlocks = sameGeometry && SameBlocks(mediaEngine.Image, greaseweazle.Image);
        var compareFiles = mediaEngine.FileSystemRecognized && greaseweazle.FileSystemRecognized;
        var sameFiles = compareFiles && SameFiles(mediaEngine.Volume.Entries, greaseweazle.Volume.Entries);
        var sameMetadata = compareFiles && SameMetadata(mediaEngine, greaseweazle);
        var conversionPassed = sameGeometry && sameBlocks && (!compareFiles || sameFiles && sameMetadata);

        return new MediaParityRow(
            formatId,
            Normalize(sourceContainer),
            Normalize(targetContainer),
            Geometry(mediaEngine.Image),
            ParityValidationStatus.Passed,
            Status(conversionPassed),
            Status(conversionPassed),
            Status(sameBlocks),
            compareFiles ? Status(sameFiles) : ParityValidationStatus.NotApplicable,
            compareFiles ? Status(sameMetadata) : ParityValidationStatus.NotApplicable,
            fluxIdentical,
            physicalWrite,
            gwFallbackAvailable,
            "media-engine-gw-document-comparison");
    }

    private static bool SameGeometry(SectorImage first, SectorImage second) =>
        first.BlockSize == second.BlockSize &&
        first.Cylinders == second.Cylinders &&
        first.Heads == second.Heads &&
        first.SectorsPerTrack == second.SectorsPerTrack &&
        first.BlockCount == second.BlockCount &&
        first.Capacity == second.Capacity;

    private static bool SameBlocks(SectorImage first, SectorImage second)
    {
        if (!first.MissingBlocks.SequenceEqual(second.MissingBlocks)) return false;
        for (var logicalBlock = 0; logicalBlock < first.BlockCount; logicalBlock++)
        {
            if (!first.TryGetBlock(logicalBlock, out var firstBlock)) continue;
            if (!second.TryGetBlock(logicalBlock, out var secondBlock)) return false;
            if (!firstBlock.Data.SequenceEqual(secondBlock.Data)) return false;
            if (!SequenceEqual(firstBlock.Tag, secondBlock.Tag)) return false;
        }

        return true;
    }

    private static bool SameFiles(
        IReadOnlyList<FileSystemEntry> first,
        IReadOnlyList<FileSystemEntry> second)
    {
        if (first.Count != second.Count) return false;
        for (var index = 0; index < first.Count; index++)
        {
            var left = first[index];
            var right = second[index];
            if (left.Name != right.Name || left.Kind != right.Kind || left.Size != right.Size)
                return false;
            if (!SequenceEqual(left.Content, right.Content)) return false;
            if (!SameFiles(left.Children, right.Children)) return false;
        }

        return true;
    }

    private static bool SameMetadata(ExploredDiskImage first, ExploredDiskImage second)
    {
        if (first.Volume.Name != second.Volume.Name ||
            first.Volume.FileSystemId != second.Volume.FileSystemId ||
            first.Volume.Capacity != second.Volume.Capacity ||
            first.Volume.FreeBytes != second.Volume.FreeBytes ||
            first.Volume.FreeSpaceKnown != second.Volume.FreeSpaceKnown ||
            first.Volume.Created != second.Volume.Created ||
            first.Volume.Modified != second.Volume.Modified ||
            !first.Metadata.SystemIds.SequenceEqual(second.Metadata.SystemIds) ||
            first.Metadata.ProtectionId != second.Metadata.ProtectionId ||
            first.Metadata.Content.HasValidAmigaBootLoader != second.Metadata.Content.HasValidAmigaBootLoader ||
            first.Metadata.Content.ModificationId != second.Metadata.Content.ModificationId ||
            !first.Metadata.Content.CompressionIds.SequenceEqual(second.Metadata.Content.CompressionIds))
            return false;

        return SameEntryMetadata(first.Volume.Entries, second.Volume.Entries);
    }

    private static bool SameEntryMetadata(
        IReadOnlyList<FileSystemEntry> first,
        IReadOnlyList<FileSystemEntry> second)
    {
        if (first.Count != second.Count) return false;
        for (var index = 0; index < first.Count; index++)
        {
            var left = first[index];
            var right = second[index];
            if (left.Modified != right.Modified ||
                left.Comment != right.Comment ||
                left.RawAttributes != right.RawAttributes ||
                left.StorageReference != right.StorageReference ||
                left.MetadataValid != right.MetadataValid ||
                !SameEntryMetadata(left.Children, right.Children))
                return false;
        }

        return true;
    }

    private static bool SequenceEqual(IReadOnlyList<byte>? first, IReadOnlyList<byte>? second) =>
        first is null ? second is null : second is not null && first.SequenceEqual(second);

    private static string Geometry(SectorImage image) =>
        $"{image.Cylinders}x{image.Heads}x{image.SectorsPerTrack}x{image.BlockSize}";

    private static ParityValidationStatus Status(bool passed) => passed
        ? ParityValidationStatus.Passed
        : ParityValidationStatus.Failed;

    private static string Normalize(string extension) => extension.StartsWith('.')
        ? extension.ToLowerInvariant()
        : "." + extension.ToLowerInvariant();
}
