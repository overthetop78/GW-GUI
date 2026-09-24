using System.IO;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Models.Flux;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using FileSystemIds = GWGUI.MediaFileSystems.Definitions.FileSystemIds;

namespace GWGUI.MediaAudit;

internal static class MediaAuditValidator
{
    internal static ValidationAudit Validate(
        MediaImageDocument document,
        MediaRecognitionResult recognition,
        int visualizationElementCount,
        IReadOnlyList<VolumeAudit> volumes)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(recognition);
        ArgumentNullException.ThrowIfNull(volumes);
        var errors = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(document.FormatId)
            || document.FormatId.Equals(DiskImageFormatIds.Unknown, StringComparison.OrdinalIgnoreCase))
            errors.Add($"The media format identifier is missing or unknown ('{document.FormatId}').");
        if (document.MediaKind == MediaKind.Unknown)
            errors.Add("The media family is unknown.");
        if (!recognition.Reader.SupportsFormatId(document.FormatId))
            errors.Add($"Reader '{recognition.Reader.GetType().FullName}' does not declare recognized format '{document.FormatId}'.");
        var extension = Path.GetExtension(document.Source.PrimaryPath);
        if (!recognition.Reader.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            warnings.Add($"Reader '{recognition.Reader.GetType().FullName}' does not declare source extension '{extension}'.");
        if (visualizationElementCount == 0)
            errors.Add($"The visualizer produced no element for representation '{document.Representation.RepresentationKind}'.");

        ValidateRepresentation(document, errors, warnings);
        ValidateVolumes(document.MediaKind, document.Representation.LogicalLength, volumes, errors, warnings);
        return Result(errors, warnings);
    }

    internal static ValidationAudit ValidateVolumes(
        MediaKind mediaKind,
        long? mediaLogicalLength,
        IReadOnlyList<VolumeAudit> volumes)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        ValidateVolumes(mediaKind, mediaLogicalLength, volumes, errors, warnings);
        return Result(errors, warnings);
    }

    private static void ValidateRepresentation(
        MediaImageDocument document,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        if (document.Representation.LogicalLength is <= 0)
            errors.Add($"Representation '{document.Representation.RepresentationKind}' has invalid logical length {document.Representation.LogicalLength}.");

        switch (document.Representation)
        {
            case SectorMediaImageRepresentation sectors:
                var image = sectors.Image;
                if (image.Capacity <= 0 || image.BlockCount <= 0 || image.BlockSize <= 0)
                    errors.Add($"Sector image geometry is invalid: capacity={image.Capacity}, blocks={image.BlockCount}, blockSize={image.BlockSize}.");
                if (image.MissingBlocks.Count > 0)
                    errors.Add($"Sector image is missing {image.MissingBlocks.Count} of {image.BlockCount} logical blocks.");
                var invalidBlocks = image.AvailableBlocks.Count(block => block.IntegrityValid == false);
                if (invalidBlocks > 0)
                    errors.Add($"Sector image contains {invalidBlocks} block(s) with invalid integrity.");
                var unexpectedSizes = image.AvailableBlocks.Count(block =>
                    block.Data.Count <= 0 || (!image.AllowsVariableBlockSize && block.Data.Count != image.BlockSize));
                if (unexpectedSizes > 0)
                    errors.Add($"Sector image contains {unexpectedSizes} block(s) with an invalid data length.");
                break;
            case FluxMediaImageRepresentation flux:
                if (flux.Tracks.Count == 0 || flux.Surfaces.Count == 0)
                    errors.Add("Flux image contains no track or surface.");
                if (flux.RevolutionCount == 0 || flux.TransitionCount == 0)
                    errors.Add($"Flux image contains no usable flux data: revolutions={flux.RevolutionCount}, transitions={flux.TransitionCount}.");
                var emptyRevolutions = flux.Tracks.SelectMany(track => track.Revolutions)
                    .Count(revolution => revolution.ResolutionNanoseconds <= 0 || revolution.Flux.FluxIntervals.Count == 0);
                if (emptyRevolutions > 0)
                    errors.Add($"Flux image contains {emptyRevolutions} empty or invalid revolution(s).");
                break;
            case BlockMediaImageRepresentation blocks:
                if (blocks.Capacity <= 0 || blocks.LogicalBlockCount <= 0)
                    errors.Add($"Block image geometry is invalid: capacity={blocks.Capacity}, blocks={blocks.LogicalBlockCount}, blockSize={blocks.LogicalBlockSize}.");
                if (blocks.Ranges.Count == 0)
                    warnings.Add("Block image describes no readable or unavailable data range.");
                break;
            case SequentialMediaImageRepresentation sequential:
                if (sequential.LogicalLength is null && sequential.Duration is null && sequential.Segments is null)
                    errors.Add("Sequential image contains no length, duration, or segment information.");
                break;
            case OpticalMediaImageRepresentation optical:
                if ((optical.Tracks?.Count ?? 0) == 0)
                    errors.Add("Optical image contains no track.");
                break;
        }
    }

    private static void ValidateVolumes(
        MediaKind mediaKind,
        long? mediaLogicalLength,
        IReadOnlyList<VolumeAudit> volumes,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        if (volumes.Count == 0)
        {
            warnings.Add("No volume was detected; this can be valid for a medium without a volume structure.");
            return;
        }

        var recognizedVolumes = volumes.Where(volume => !string.IsNullOrWhiteSpace(volume.FileSystemId)).ToArray();
        if (recognizedVolumes.Length == 0)
            warnings.Add($"{volumes.Count} volume range(s) were found, but no file system was recognized.");
        if (!TrySum(volumes.Where(volume => volume.Capacity.HasValue).Select(volume => volume.Capacity!.Value), out _))
            errors.Add("The sum of the reported volume capacities exceeds the supported 64-bit size.");

        foreach (var volume in volumes)
        {
            var identity = VolumeIdentity(volume);
            if (volume.Start < 0 || volume.Length <= 0)
            {
                errors.Add($"{identity} has an invalid range: start={volume.Start}, length={volume.Length}.");
                continue;
            }
            long end;
            try
            {
                end = checked(volume.Start + volume.Length);
            }
            catch (OverflowException)
            {
                errors.Add($"{identity} range overflows: start={volume.Start}, length={volume.Length}.");
                continue;
            }
            if (mediaLogicalLength is { } mediaLength && end > mediaLength)
                errors.Add($"{identity} ends at {end} bytes, beyond media length {mediaLength} bytes.");
            if (volume.Capacity is { } capacity)
            {
                if (capacity <= 0)
                    errors.Add($"{identity} reports invalid capacity {capacity} bytes.");
                if (capacity > volume.Length)
                    errors.Add($"{identity} capacity {capacity} bytes exceeds its range length {volume.Length} bytes.");
                if (volume.FreeSpaceKnown == true && volume.FreeBytes is { } freeBytes && (freeBytes < 0 || freeBytes > capacity))
                    errors.Add($"{identity} free space {freeBytes} bytes is outside capacity {capacity} bytes.");
                if (volume.LogicalFileBytes > capacity)
                    errors.Add($"{identity} contains {volume.LogicalFileBytes} logical file bytes, exceeding capacity {capacity} bytes.");
                if (volume.OccupiedFileBytes is { } occupied && occupied > capacity)
                    errors.Add($"{identity} files occupy {occupied} bytes, exceeding capacity {capacity} bytes.");
            }

            var flattened = Flatten(volume.Entries).ToArray();
            var directories = flattened.Count(entry => entry.Kind == FileSystemEntryKind.Directory.ToString());
            var files = flattened.Where(entry => entry.Kind == FileSystemEntryKind.File.ToString()).ToArray();
            if (directories != volume.DirectoryCount)
                errors.Add($"{identity} directory total {volume.DirectoryCount} does not match its reported entries.");
            if (files.Length != volume.FileCount)
                errors.Add($"{identity} file total {volume.FileCount} does not match its reported entries.");
            if (!TrySum(files.Select(entry => entry.Size), out var logicalBytes))
                errors.Add($"{identity} logical file-size total exceeds the supported 64-bit size.");
            else if (logicalBytes != volume.LogicalFileBytes)
                errors.Add($"{identity} logical file-size total {volume.LogicalFileBytes} does not match its entries ({logicalBytes}).");
            var occupiedSizes = files.Select(entry => entry.OccupiedSize).ToArray();
            if (occupiedSizes.All(size => size.HasValue))
            {
                if (!TrySum(occupiedSizes.Select(size => size!.Value), out var occupiedBytes))
                    errors.Add($"{identity} occupied file-size total exceeds the supported 64-bit size.");
                else if (volume.OccupiedFileBytes != occupiedBytes)
                    errors.Add($"{identity} occupied file-size total {volume.OccupiedFileBytes} does not match its entries ({occupiedBytes}).");
            }
            else if (volume.OccupiedFileBytes is not null)
                errors.Add($"{identity} reports an occupied file-size total although at least one file has no occupied size.");
            foreach (var entry in flattened) ValidateEntry(identity, entry, errors, warnings);
        }

        var fileCount = volumes.Sum(volume => volume.FileCount);
        if (recognizedVolumes.Length > 0 && fileCount == 0)
        {
            if (recognizedVolumes.All(volume =>
                    volume.FileSystemId!.Equals(FileSystemIds.AtariKFile, StringComparison.OrdinalIgnoreCase)
                    || volume.FileSystemId.Equals(FileSystemIds.AtariBootDisk, StringComparison.OrdinalIgnoreCase)))
                warnings.Add("The recognized Atari boot media has no file catalog to enumerate.");
            else
                errors.Add($"A file system was recognized on {recognizedVolumes.Length} volume(s), but no file was extracted.");
        }
        else if (mediaKind == MediaKind.Floppy && fileCount == 0)
            errors.Add("The floppy contains volume data, but no logical file was extracted.");
    }

    private static void ValidateEntry(
        string volume,
        FileEntryAudit entry,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        var identity = $"{volume}, entry '{entry.DisplayName}'";
        if (string.IsNullOrWhiteSpace(entry.Name)) errors.Add($"{identity} has an empty native name.");
        if (entry.Size < 0) errors.Add($"{identity} has invalid size {entry.Size} bytes.");
        if (entry.OccupiedSize is < 0) errors.Add($"{identity} has invalid occupied size {entry.OccupiedSize} bytes.");
        if (!entry.MetadataValid) warnings.Add($"{identity} reports invalid metadata.");
        if (entry.DataValid == false) warnings.Add($"{identity} reports invalid data.");
        if (entry.Kind == FileSystemEntryKind.File.ToString())
        {
            if (!entry.ContentExtracted)
                errors.Add($"{identity} has no extracted content for declared size {entry.Size} bytes.");
            else if (entry.ContentLength != entry.Size)
                errors.Add($"{identity} content length {entry.ContentLength} bytes differs from declared size {entry.Size} bytes.");
            if (entry.Children.Count != 0) errors.Add($"{identity} is a file but contains {entry.Children.Count} child entries.");
        }
        if (entry.IconId is null || entry.TypeResourceKey is null)
            errors.Add($"{identity} has no MediaAnalysis classification.");
        if (entry.Kind == FileSystemEntryKind.File.ToString()
            && entry.Category == GWGUI.App.Enums.Explorer.ExplorerFileCategory.File.ToString()
            && entry.ContentFormat == GWGUI.App.Enums.Explorer.ExplorerContentFormat.Unknown.ToString())
            errors.Add($"{identity} content type remains unknown.");
    }

    private static IEnumerable<FileEntryAudit> Flatten(IEnumerable<FileEntryAudit> entries)
    {
        foreach (var entry in entries)
        {
            yield return entry;
            foreach (var child in Flatten(entry.Children)) yield return child;
        }
    }

    private static ValidationAudit Result(IEnumerable<string> errors, IEnumerable<string> warnings)
    {
        var distinctErrors = errors.Distinct(StringComparer.Ordinal).ToArray();
        return new(distinctErrors.Length == 0, distinctErrors,
            warnings.Distinct(StringComparer.Ordinal).ToArray());
    }

    private static bool TrySum(IEnumerable<long> values, out long sum)
    {
        sum = 0;
        try
        {
            foreach (var value in values) sum = checked(sum + value);
            return true;
        }
        catch (OverflowException)
        {
            sum = long.MaxValue;
            return false;
        }
    }

    private static string VolumeIdentity(VolumeAudit volume) =>
        $"Volume '{volume.Name ?? "(unnamed)"}' (start={volume.Start}, length={volume.Length})";
}
