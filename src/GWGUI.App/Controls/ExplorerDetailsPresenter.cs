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
            return document.UsesCustomSectorLoader ? new($"({LocExtension.Get("Explorer.Unnamed")})", true) : new(LocExtension.Get("Explorer.Unknown"), false);
        if (!string.IsNullOrWhiteSpace(document.Volume.Name))
            return new(document.Volume.Name, false);
        return new($"({LocExtension.Get("Explorer.Unnamed")})", true);
    }

    public static string FileSystemText(ExploredDiskImage document)
    {
        if (document.FileSystemRecognized) return document.Volume.FileSystemId;
        var resourceKey = document.UsesCustomSectorLoader ? "Explorer.CustomSectorLoaderNoCatalog" : "Explorer.PhysicalSectorsNoFileSystem";
        return LocExtension.Get(resourceKey);
    }

    public static ExplorerDetailsPresentation ForDisk(ExploredDiskImage document, string? currentSystem = null)
    {
        var volume = document.Volume;
        var volumeName = VolumeName(document);
        var rows = new List<ExplorerDetailRow>
        {
            new("Explorer.Volume", volumeName.Text, volumeName.IsSynthetic),
            new("Explorer.System", currentSystem ?? ExplorerMetadataPresenter.Systems(document.Metadata)),
            new("Explorer.Protection", ExplorerMetadataPresenter.Protection(document.Metadata)),
            new("Explorer.FileSystem", FileSystemText(document))
        };
        if (document.UsesCustomSectorLoader)
        {
            rows.Add(new("Explorer.Organization", LocExtension.Get("Explorer.CustomSectorLoader")));
            if (document.Metadata.Content.ModificationId is { } modificationId) rows.Add(new("Explorer.Modification", LocExtension.Get($"Explorer.Content.{modificationId}")));
            foreach (var compressionId in document.Metadata.Content.CompressionIds) rows.Add(new("Explorer.Compression", LocExtension.Get($"Explorer.Content.{compressionId}")));
            if (document.Metadata.Content.OrganizationMemberCount > 0) rows.Add(new("Explorer.DataBlocks", document.Metadata.Content.OrganizationMemberCount.ToString()));
        }
        rows.Add(new("Explorer.Capacity", ExplorerFormatting.FormatBytes(volume.Capacity)));
        if (!document.UsesCustomSectorLoader)
        {
            rows.Add(new("Explorer.Free", document.FileSystemRecognized && volume.FreeSpaceKnown ? ExplorerFormatting.FormatBytes(volume.FreeBytes) : "\u2014"));
            rows.Add(new("Explorer.Entries", ExplorerSection.CountEntries(volume.Entries).ToString()));
            rows.Add(new("Explorer.Warnings", ExplorerIssueBuilder.Build(document).Count.ToString()));
        }
        return new(volumeName.Text, ExplorerIconKind.DiskImage, rows, volumeName.IsSynthetic);
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
