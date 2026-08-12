using GWGUI.App.Localization;
using GWGUI.MediaEngine.Exploration.Results;

namespace GWGUI.App.Controls;

public sealed record ExplorerDetailRow(string Key, string Value);
public sealed record ExplorerDetailsPresentation(string Title, ExplorerIconKind IconKind, IReadOnlyList<ExplorerDetailRow> Rows);

public static class ExplorerDetailsPresenter
{
    public static string FileSystemText(ExploredDiskImage document) => document.FileSystemRecognized
        ? string.Join(" + ", (document.DetectedFileSystems ?? []).Select(item => item.Volume.FileSystemId)
            .Distinct(StringComparer.CurrentCultureIgnoreCase).DefaultIfEmpty(document.Volume.FileSystemId))
        : LocExtension.Get("Explorer.PhysicalSectorsNoFileSystem");

    public static ExplorerDetailsPresentation ForDisk(ExploredDiskImage document)
    {
        var volume = document.Volume;
        var title = !document.FileSystemRecognized
            ? LocExtension.Get("Explorer.Unknown")
            : string.IsNullOrWhiteSpace(volume.Name) ? LocExtension.Get("Explorer.Unnamed") : volume.Name;
        return new(title, ExplorerIconKind.DiskImage,
        [
            new("Explorer.Volume", title),
            new("Explorer.System", ExplorerMetadataPresenter.Systems(document.Metadata)),
            new("Explorer.Protection", ExplorerMetadataPresenter.Protection(document.Metadata)),
            new("Explorer.FileSystem", FileSystemText(document)),
            new("Explorer.Capacity", ExplorerFormatting.FormatBytes(volume.Capacity)),
            new("Explorer.Free", document.FileSystemRecognized ? ExplorerFormatting.FormatBytes(volume.FreeBytes) : "\u2014"),
            new("Explorer.Entries", ExplorerSection.CountEntries(volume.Entries).ToString()),
            new("Explorer.Warnings", ExplorerIssueBuilder.Build(document).Count.ToString())
        ]);
    }

    public static ExplorerDetailsPresentation ForItem(ExplorerContentItem item)
    {
        var rows = new List<ExplorerDetailRow>
        {
            new("Explorer.Type", LocExtension.Get(ExplorerFileIconClassifier.TypeResourceKeyFor(item.IconKind))),
            new("Explorer.Size", item.SizeText),
            new("Explorer.Modified", item.ModifiedText),
            new("Explorer.Comment", string.IsNullOrWhiteSpace(item.Entry.Comment) ? "\u2014" : item.Entry.Comment)
        };
        if (item.Entry.Kind == GWGUI.MediaEngine.FileSystems.FileSystemEntryKind.Directory)
            rows.Add(new("Explorer.Entries", ExplorerSection.CountEntries(item.Entry.Children).ToString()));
        return new(item.Name, item.IconKind, rows);
    }
}
