using MediaVolumeOrigins = global::GWGUI.MediaEngine.Constants.MediaVolumeOrigins;
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
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Reading.Recognition;


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
        if (document.Document.MediaKind == MediaKind.Tape)
        {
            var metadata = document.Document.Metadata;
            var tapeRows = new List<ExplorerDetailRow>
            {
                new("Explorer.Type", LocExtension.Get("Explorer.Cassette")),
                new("Explorer.Format", document.Document.FormatId.Equals(TapeImageFormatIds.AtariCas, StringComparison.OrdinalIgnoreCase) ? "Atari CAS" : document.Document.FormatId),
                new("Explorer.System", currentSystem ?? SystemName(ReadMetadata(metadata, "systemId")) ?? document.Document.MediaKind.ToString()),
                new("Explorer.Size", StorageSizeFormatter.FormatBytes(capacity))
            };
            if (document.Document.Representation is SequentialMediaImageRepresentation sequential && sequential.Duration is { } duration)
                tapeRows.Add(new("Visual.DurationLabel", duration.ToString("g")));
            AddMetadataRow(tapeRows, metadata, "internalName", "Explorer.InternalName");
            AddMetadataRow(tapeRows, metadata, "baudRates", "Explorer.BaudRates", "baud");
            AddChunkSummaryRow(tapeRows, metadata);
            AddMetadataRow(tapeRows, metadata, "dataChunkCount", "Explorer.DataBlocks");
            AddMetadataRow(tapeRows, metadata, "fskChunkCount", "Explorer.FskBlocks");
            tapeRows.Add(new("Explorer.Entries", ExplorerSection.CountEntries(entries).ToString()));
            tapeRows.Add(new("Explorer.Warnings", ExplorerIssueBuilder.Build(document, exploredVolume).Count.ToString()));
            return new(volumeName, ExplorerIconCategory.DiskImage, tapeRows, syntheticName);
        }
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
            new("Explorer.Subchannels", track.HasSubchannels ? LocExtension.Get("Controllers.Yes") : LocExtension.Get("Controllers.No"))
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
            new("Explorer.Type", item.TypeText)
        };
        if (item.Entry.Kind == GWGUI.MediaEngine.Enums.FileSystemEntryKind.File)
        {
            rows.Add(new("Explorer.Category", LocExtension.Get($"Explorer.Category.{item.Definition.Category}")));
            rows.Add(new("Explorer.ContentFormat", LocExtension.Get($"Explorer.ContentFormat.{item.Definition.ContentFormat}")));
            if (item.Definition.TextEncoding != ExplorerTextEncoding.NotApplicable)
                rows.Add(new("Explorer.TextEncoding", TextEncoding(item.Definition.TextEncoding)));
            rows.Add(new("Explorer.Preview", LocExtension.Get($"Explorer.Preview.{item.Definition.PreviewKind}")));
        }
        rows.AddRange(
        [
            new("Explorer.Size", item.SizeText),
            new("Explorer.Modified", item.ModifiedText),
            new("Explorer.Comment", string.IsNullOrWhiteSpace(item.Entry.Comment) ? "\u2014" : item.Entry.Comment)
        ]);
        if (item.Definition.ExecutionKind != ExplorerExecutionKind.None)
            rows.Add(new("Explorer.Execution", LocExtension.Get($"Explorer.Execution.{item.Definition.ExecutionKind}")));
        if (item.Entry.Kind == GWGUI.MediaEngine.Enums.FileSystemEntryKind.Directory)
            rows.Add(new("Explorer.Entries", ExplorerSection.CountEntries(item.Entry.Children).ToString()));
        AddMetadataRow(rows, item.Entry.Metadata, "recordCount", "Explorer.Records");
        AddMetadataRow(rows, item.Entry.Metadata, "fullRecordCount", "Explorer.FullRecords");
        AddMetadataRow(rows, item.Entry.Metadata, "partialRecordCount", "Explorer.PartialRecords");
        AddMetadataRow(rows, item.Entry.Metadata, "baudRates", "Explorer.BaudRates", "baud");
        if (item.Entry.Metadata.TryGetValue("endRecordPresent", out var endRecordPresent))
            rows.Add(new("Explorer.EndRecord", bool.TryParse(endRecordPresent, out var present) && present
                ? LocExtension.Get("Controllers.Yes")
                : LocExtension.Get("Controllers.No")));
        return new(item.Name, item.IconCategory, rows, false, item.Tone);
    }

    private static string? ReadMetadata(IReadOnlyDictionary<string, string> metadata, string key) =>
        metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    private static string? SystemName(string? systemId) => systemId switch
    {
        DiskSystemIds.Atari8Bit => "Atari 8-bit",
        _ => systemId
    };

    private static void AddMetadataRow(
        ICollection<ExplorerDetailRow> rows,
        IReadOnlyDictionary<string, string> metadata,
        string metadataKey,
        string labelKey,
        string? unit = null)
    {
        if (ReadMetadata(metadata, metadataKey) is { } value)
            rows.Add(new(labelKey, unit is null ? value : $"{value} {unit}"));
    }

    private static void AddChunkSummaryRow(
        ICollection<ExplorerDetailRow> rows,
        IReadOnlyDictionary<string, string> metadata)
    {
        var count = ReadMetadata(metadata, "chunkCount");
        var types = ReadMetadata(metadata, "chunkTypes");
        if (count is not null || types is not null)
            rows.Add(new("Explorer.Chunks", types is null ? count! : count is null ? types : $"{types} ({count})"));
    }

    private static string TextEncoding(ExplorerTextEncoding encoding) => encoding switch
    {
        ExplorerTextEncoding.Unknown => LocExtension.Get("Explorer.Unknown"),
        ExplorerTextEncoding.DosOem => "DOS OEM",
        ExplorerTextEncoding.Latin1 => "Latin-1",
        ExplorerTextEncoding.MacRoman => "Mac Roman",
        ExplorerTextEncoding.AppleAscii => "Apple ASCII",
        _ => encoding.ToString().ToUpperInvariant()
    };
}
