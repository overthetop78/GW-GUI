using GWGUI.App.Localization;
using GWGUI.MediaEngine.Exploration.Results;

namespace GWGUI.App.Controls;

public sealed record ExplorerDetailRow(string Key, string Value, bool IsSyntheticValue = false);
public sealed record ExplorerDetailsPresentation(string Title, ExplorerIconKind IconKind, IReadOnlyList<ExplorerDetailRow> Rows, bool IsSyntheticTitle = false);

public sealed record ExplorerVolumeNamePresentation(string Text, bool IsSynthetic);

public static class ExplorerDetailsPresenter
{
    public static ExplorerVolumeNamePresentation VolumeName(ExploredDiskImage document)
    {
        if (!document.FileSystemRecognized)
            return new(LocExtension.Get("Explorer.Unknown"), false);
        if (!string.IsNullOrWhiteSpace(document.Volume.Name))
            return new(document.Volume.Name, false);
        return new($"({LocExtension.Get("Explorer.Unnamed")})", true);
    }

    public static string FileSystemText(ExploredDiskImage document) => document.FileSystemRecognized
        ? string.Join(" + ", (document.DetectedFileSystems ?? []).Select(item => item.Volume.FileSystemId)
            .Distinct(StringComparer.CurrentCultureIgnoreCase).DefaultIfEmpty(document.Volume.FileSystemId))
        : LocExtension.Get("Explorer.PhysicalSectorsNoFileSystem");

    public static ExplorerDetailsPresentation ForDisk(ExploredDiskImage document)
    {
        var volume = document.Volume;
        var volumeName = VolumeName(document);
        return new(volumeName.Text, ExplorerIconKind.DiskImage,
        [
            new("Explorer.Volume", volumeName.Text, volumeName.IsSynthetic),
            new("Explorer.System", ExplorerMetadataPresenter.Systems(document.Metadata)),
            new("Explorer.Protection", ExplorerMetadataPresenter.Protection(document.Metadata)),
            new("Explorer.FileSystem", FileSystemText(document)),
            new("Explorer.Capacity", ExplorerFormatting.FormatBytes(volume.Capacity)),
            new("Explorer.Free", document.FileSystemRecognized ? ExplorerFormatting.FormatBytes(volume.FreeBytes) : "\u2014"),
            new("Explorer.Entries", ExplorerSection.CountEntries(volume.Entries).ToString()),
            new("Explorer.Warnings", ExplorerIssueBuilder.Build(document).Count.ToString())
        ], volumeName.IsSynthetic);
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
