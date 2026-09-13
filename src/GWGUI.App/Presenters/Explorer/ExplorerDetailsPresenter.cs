using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Explorer;
using GWGUI.App.Enums.Explorer;
using GWGUI.App.Functions.Explorer;
using GWGUI.App.Functions.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.ViewModels.Explorer;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Representations.Optical;


namespace GWGUI.App.Presenters.Explorer;

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
        rows.Add(new("Explorer.Capacity", StorageSizeFormatter.FormatBytes(volume.Capacity)));
        if (!document.UsesCustomSectorLoader)
        {
            rows.Add(new("Explorer.Free", document.FileSystemRecognized && volume.FreeSpaceKnown ? StorageSizeFormatter.FormatBytes(volume.FreeBytes) : ControlVisualConstants.EmptyValue));
            rows.Add(new("Explorer.Entries", ExplorerSection.CountEntries(volume.Entries).ToString()));
            rows.Add(new("Explorer.Warnings", ExplorerIssueBuilder.Build(document).Count.ToString()));
        }
        return new(volumeName.Text, ExplorerIconCategory.DiskImage, rows, volumeName.IsSynthetic);
    }

    public static ExplorerDetailsPresentation ForMedia(
        ExploredMediaImage document,
        ExploredMediaVolume exploredVolume,
        string? currentSystem = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(exploredVolume);
        var volume = exploredVolume.FileSystem;
        var syntheticName = string.IsNullOrWhiteSpace(volume?.Name);
        var volumeName = syntheticName
            ? $"({LocExtension.Get("Explorer.Unnamed")})"
            : volume!.Name;
        var capacity = volume?.Capacity ?? exploredVolume.Descriptor.Length;
        var entries = volume?.Entries ?? [];
        var rows = new List<ExplorerDetailRow>
        {
            new("Explorer.Volume", volumeName, syntheticName),
            new("Explorer.System", currentSystem ?? document.Document.MediaKind.ToString()),
            new("Explorer.Protection", LocExtension.Get("Explorer.Metadata.None")),
            new("Explorer.FileSystem", volume?.FileSystemId ?? ControlVisualConstants.EmptyValue)
        };
        var descriptor = exploredVolume.Descriptor;
        if (descriptor.SessionNumber is { } sessionNumber)
            rows.Add(new("Explorer.Session", sessionNumber.ToString()));
        if (descriptor.TrackNumber is { } trackNumber)
            rows.Add(new("Explorer.Track", trackNumber.ToString()));
        if (document.Document.Representation is OpticalMediaImageRepresentation optical)
        {
            if (optical.LayerCount is { } layerCount)
                rows.Add(new("Explorer.Layers", layerCount.ToString()));
            if (optical.FaceCount is { } faceCount)
                rows.Add(new("Explorer.Faces", faceCount.ToString()));
        }
        if (!string.IsNullOrWhiteSpace(descriptor.PartitionScheme)
            && descriptor.Origin != MediaVolumeOrigins.OpticalTrack)
            rows.Add(new("Visual.PartitionSchemeLabel", descriptor.PartitionScheme));
        if (descriptor.PartitionNumber is { } partitionNumber)
            rows.Add(new("Visual.PartitionNumberLabel", partitionNumber.ToString()));
        if (!string.IsNullOrWhiteSpace(descriptor.PartitionType))
            rows.Add(new("Explorer.PartitionType", descriptor.PartitionType));
        if (!string.IsNullOrWhiteSpace(descriptor.PartitionId))
            rows.Add(new("Explorer.PartitionId", descriptor.PartitionId));
        rows.Add(new("Explorer.Capacity", StorageSizeFormatter.FormatBytes(capacity)));
        rows.Add(new("Explorer.Free", volume?.FreeSpaceKnown == true ? StorageSizeFormatter.FormatBytes(volume.FreeBytes) : ControlVisualConstants.EmptyValue));
        rows.Add(new("Explorer.Entries", ExplorerSection.CountEntries(entries).ToString()));
        rows.Add(new("Explorer.Warnings", ExplorerIssueBuilder.Build(document, exploredVolume).Count.ToString()));
        return new(volumeName, ExplorerIconCategory.DiskImage, rows, syntheticName);
    }

    public static ExplorerDetailsPresentation ForOpticalTrack(OpticalTrackDescriptor track)
    {
        ArgumentNullException.ThrowIfNull(track);
        var rows = new List<ExplorerDetailRow>
        {
            new("Explorer.Session", track.SessionNumber.ToString()),
            new("Explorer.Track", track.TrackNumber.ToString()),
            new("Explorer.TrackMode", track.Mode.ToString()),
            new("Explorer.FirstSector", track.FirstSector.ToString()),
            new("Explorer.SectorCount", track.SectorCount.ToString()),
            new("Explorer.StoredSectorSize", StorageSizeFormatter.FormatBytes(track.StoredSectorSize)),
            new("Explorer.UserDataSize", StorageSizeFormatter.FormatBytes(track.UserDataLength)),
            new("Explorer.Subchannels", track.HasSubchannels ? LocExtension.Get("Common.Yes") : LocExtension.Get("Common.No"))
        };
        return new(
            $"{LocExtension.Get("Explorer.Track")} {track.TrackNumber}",
            track.IsAudio ? ExplorerIconCategory.Audio : ExplorerIconCategory.DiskImage,
            rows);
    }

    public static ExplorerDetailsPresentation ForItem(ExplorerContentItem item)
    {
        var rows = new List<ExplorerDetailRow>
        {
            new("Explorer.Type", item.TypeText),
            new("Explorer.Size", item.SizeText),
            new("Explorer.Modified", item.ModifiedText),
            new("Explorer.Comment", string.IsNullOrWhiteSpace(item.Entry.Comment) ? "\u2014" : item.Entry.Comment)
        };
        if (item.Definition.ExecutionKind != ExplorerExecutionKind.None)
            rows.Add(new("Explorer.Execution", LocExtension.Get($"Explorer.Execution.{item.Definition.ExecutionKind}")));
        if (item.Entry.Kind == GWGUI.MediaEngine.FileSystems.FileSystemEntryKind.Directory)
            rows.Add(new("Explorer.Entries", ExplorerSection.CountEntries(item.Entry.Children).ToString()));
        return new(item.Name, item.IconCategory, rows, false, item.Tone);
    }
}
