using GWGUI.App.Functions.Localization;
using GWGUI.App.Localization.Extensions;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.App.Functions.Explorer;

internal static class ExplorerIssueBuilder
{
    public static int CountEntries(IEnumerable<FileSystemEntry> entries) =>
        entries.Sum(entry => 1 + CountEntries(entry.Children));

    public static IReadOnlyList<string> Build(ExploredDiskImage document)
    {
        var issues = new List<string>();
        foreach (var warning in (document.DetectedFileSystems ?? [])
                     .SelectMany(item => item.Volume.Warnings)
                     .Concat(document.Volume.Warnings)
                     .Where(warning => !string.IsNullOrWhiteSpace(warning))
                     .Distinct(StringComparer.CurrentCultureIgnoreCase))
            issues.Add(ExplorerWarningLocalizer.Localize(warning));

        AddSectorIssues(issues, document.Image);
        return issues.Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
    }

    public static IReadOnlyList<string> Build(ExploredMediaImage document, ExploredMediaVolume volume)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volume);
        var issues = document.Diagnostics
            .Concat(volume.Diagnostics)
            .Concat(volume.FileSystem?.Warnings ?? [])
            .Where(issue => !string.IsNullOrWhiteSpace(issue))
            .Select(ExplorerWarningLocalizer.Localize)
            .ToList();
        if (document.Document.Representation is SectorMediaImageRepresentation sectors)
            AddSectorIssues(issues, sectors.Image);
        return issues.Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
    }

    private static void AddSectorIssues(List<string> issues, SectorImage image)
    {
        foreach (var block in image.AvailableBlocks.Where(block => block.IntegrityValid == false)
                     .OrderBy(block => block.Address.Cylinder).ThenBy(block => block.Address.Head).ThenBy(block => block.Address.Number))
            issues.Add(LocExtension.Get("Visual.SectorDetail", block.Address.Cylinder, block.Address.Head,
                block.Address.Number, block.Data.Count, LocExtension.Get("Visual.Integrity.Crc"), LocExtension.Get("Visual.CrcInvalid")));

        foreach (var logical in image.MissingBlocks)
        {
            var sectorsPerCylinder = Math.Max(1, image.Heads * image.SectorsPerTrack);
            var cylinder = logical / sectorsPerCylinder;
            var withinCylinder = logical % sectorsPerCylinder;
            var head = withinCylinder / Math.Max(1, image.SectorsPerTrack);
            var sector = withinCylinder % Math.Max(1, image.SectorsPerTrack);
            issues.Add(LocExtension.Get("Visual.SectorDetail", cylinder, head, sector, 0,
                LocExtension.Get("Visual.Integrity.Crc"), LocExtension.Get("Explorer.Unknown")));
        }
    }
}
