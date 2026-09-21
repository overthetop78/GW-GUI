using GWGUI.MediaEngine.Enums;
using FileSystemIds = GWGUI.MediaFileSystems.Definitions.FileSystemIds;

namespace GWGUI.MediaAudit;

internal static class MediaAuditValidatorSelfTests
{
    internal static void Run()
    {
        ExpectPassed(
            MediaAuditValidator.ValidateVolumes(MediaKind.Optical, 4096, []),
            "A medium without a volume must remain auditable.");

        ExpectFailed(
            MediaAuditValidator.ValidateVolumes(MediaKind.Floppy, 1024, [Volume(capacity: 1024)]),
            "no file was extracted");
        ExpectPassed(
            MediaAuditValidator.ValidateVolumes(MediaKind.Floppy, 1024,
                [Volume(capacity: 1024, fileSystemId: FileSystemIds.AtariKFile)]),
            "A recognized K-file using KBoot has no file catalog to enumerate.");
        ExpectPassed(
            MediaAuditValidator.ValidateVolumes(MediaKind.Floppy, 1024,
                [Volume(capacity: 1024, fileSystemId: FileSystemIds.AtariBootDisk)]),
            "A recognized Atari boot disk has no file catalog to enumerate.");
        ExpectFailed(
            MediaAuditValidator.ValidateVolumes(MediaKind.Floppy, 1024,
                [Volume(capacity: 1024, freeBytes: 1025, entries: [File(size: 1)])]),
            "free space");
        ExpectFailed(
            MediaAuditValidator.ValidateVolumes(MediaKind.Floppy, 1024,
                [Volume(capacity: 1024, entries: [File(size: 1025)])]),
            "exceeding capacity");
        ExpectFailed(
            MediaAuditValidator.ValidateVolumes(MediaKind.Floppy, 1024,
                [Volume(capacity: 1024, entries: [File(size: 4, contentLength: 3)])]),
            "differs from declared size");
        ExpectFailed(
            MediaAuditValidator.ValidateVolumes(MediaKind.Floppy, long.MaxValue,
                [Volume(capacity: 1, start: long.MaxValue, length: 1, entries: [File(size: 1)])]),
            "range overflows");
        ExpectPassed(
            MediaAuditValidator.ValidateVolumes(MediaKind.Floppy, 1024,
                [Volume(capacity: 1024, freeBytes: 512, entries: [File(size: 4)])]),
            "A coherent floppy volume must pass.");
    }

    private static VolumeAudit Volume(
        long capacity,
        long freeBytes = 0,
        long start = 0,
        long? length = null,
        IReadOnlyList<FileEntryAudit>? entries = null,
        string fileSystemId = "self-test-fs")
    {
        entries ??= [];
        var flattened = Flatten(entries).ToArray();
        var files = flattened.Where(entry => entry.Kind == FileSystemEntryKind.File.ToString()).ToArray();
        return new VolumeAudit(
            start, length ?? capacity, "self-test", null, null, null, null, null, null, "TEST",
            "self-test-reader", fileSystemId, capacity, freeBytes, true, false, null, null,
            [], [], [],
            flattened.Count(entry => entry.Kind == FileSystemEntryKind.Directory.ToString()),
            files.Length,
            files.Sum(entry => entry.Size),
            files.All(entry => entry.OccupiedSize.HasValue) ? files.Sum(entry => entry.OccupiedSize!.Value) : null,
            entries);
    }

    private static FileEntryAudit File(long size, long? contentLength = null) => new(
        "FILE.BIN", "FILE.BIN", FileSystemEntryKind.File.ToString(), "File", "Data",
        "StructuredData", "NotApplicable", "None", "Hexadecimal", "data", "Explorer.Type.Data",
        size, size, null, null, null, string.Empty, 0, 1, true, true, false, null, null,
        [], [], new Dictionary<string, string>(), "HASH", null, null, null,
        contentLength ?? size, true, []);

    private static IEnumerable<FileEntryAudit> Flatten(IEnumerable<FileEntryAudit> entries)
    {
        foreach (var entry in entries)
        {
            yield return entry;
            foreach (var child in Flatten(entry.Children)) yield return child;
        }
    }

    private static void ExpectPassed(ValidationAudit result, string message)
    {
        if (!result.Passed)
            throw new InvalidOperationException($"{message} Errors: {string.Join(" | ", result.Errors)}");
    }

    private static void ExpectFailed(ValidationAudit result, string expectedFragment)
    {
        if (result.Passed || !result.Errors.Any(error =>
                error.Contains(expectedFragment, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(
                $"Expected validation error containing '{expectedFragment}', got: {string.Join(" | ", result.Errors)}");
    }
}
