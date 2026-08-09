using GWGUI.App.Localization;
using GWGUI.Scp.FileSystems;
using GWGUI.Scp.Images;

namespace GWGUI.App.Controls;

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

        foreach (var block in document.Image.AvailableBlocks.Where(block => block.IntegrityValid == false)
                     .OrderBy(block => block.Address.Cylinder).ThenBy(block => block.Address.Head).ThenBy(block => block.Address.Number))
            issues.Add(LocExtension.Get("Visual.SectorDetail", block.Address.Cylinder, block.Address.Head,
                block.Address.Number, block.Data.Count, LocExtension.Get("Visual.Integrity.Crc"), LocExtension.Get("Visual.CrcInvalid")));

        foreach (var logical in document.Image.MissingBlocks)
        {
            var sectorsPerCylinder = Math.Max(1, document.Image.Heads * document.Image.SectorsPerTrack);
            var cylinder = logical / sectorsPerCylinder;
            var withinCylinder = logical % sectorsPerCylinder;
            var head = withinCylinder / Math.Max(1, document.Image.SectorsPerTrack);
            var sector = withinCylinder % Math.Max(1, document.Image.SectorsPerTrack);
            issues.Add(LocExtension.Get("Visual.SectorDetail", cylinder, head, sector, 0,
                LocExtension.Get("Visual.Integrity.Crc"), LocExtension.Get("Explorer.Unknown")));
        }
        return issues.Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
    }
}
