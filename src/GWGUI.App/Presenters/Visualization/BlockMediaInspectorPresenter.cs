using PartitionTableIds = global::GWGUI.MediaEngine.Constants.PartitionTableIds;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Rendering.Blocks;
using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Enums.Rendering.Blocks;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using System.IO;

namespace GWGUI.App.Presenters.Visualization;

public sealed class BlockMediaInspectorPresenter(Func<string, object[], string> localize)
{
    public BlockMediaRenderModel BuildRenderModel(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Representation is not BlockMediaImageRepresentation blocks)
            throw new ArgumentException("A block media representation is required.", nameof(document));
        var logicalLength = blocks.LogicalLength
            ?? throw new InvalidDataException("A block media representation must declare its logical length.");

        var boundaries = new SortedSet<long> { 0, logicalLength };
        foreach (var sourceRange in blocks.Ranges)
        {
            boundaries.Add(sourceRange.Address);
            boundaries.Add(checked(sourceRange.Address + sourceRange.Length));
        }
        foreach (var volume in document.Volumes)
        {
            boundaries.Add(volume.Start);
            boundaries.Add(checked(volume.Start + volume.Length));
        }

        var points = boundaries.Where(point => point >= 0 && point <= logicalLength).ToArray();
        var ranges = new List<BlockMediaRange>();
        for (var index = 0; index < points.Length - 1; index++)
        {
            var start = points[index];
            var end = points[index + 1];
            if (end <= start) continue;
            var sourceRange = blocks.Ranges.FirstOrDefault(item =>
                item.Address <= start && item.Address + item.Length >= end);
            var volume = document.Volumes.FirstOrDefault(item =>
                item.Start <= start && item.Start + item.Length >= end);
            var state = sourceRange is null || sourceRange.Kind == MediaDataRangeKind.Unavailable
                ? BlockMediaRangeState.Unknown
                : volume?.PartitionTable is PartitionTableIds.Mbr or PartitionTableIds.Gpt
                    ? BlockMediaRangeState.Allocated
                    : BlockMediaRangeState.Available;
            AddRange(ranges, new BlockMediaRange(
                start / blocks.LogicalBlockSize,
                (end - start) / blocks.LogicalBlockSize,
                state,
                volume?.PartitionTable,
                volume?.PartitionNumber,
                volume?.FileSystemId));
        }

        var geometry = blocks.Geometry is null
            ? null
            : new BlockMediaGeometry(
                blocks.Geometry.Cylinders,
                blocks.Geometry.Heads,
                blocks.Geometry.SectorsPerTrack);
        return new(blocks.LogicalBlockCount, ranges, geometry);
    }

    public MediaInspectorModel BuildInspectorModel(BlockMediaRenderModel model, BlockMediaRange? range, int? surface = null)
    {
        ArgumentNullException.ThrowIfNull(model);
        var sections = new List<MediaInspectorSection>
        {
            new(Localize("Visual.SummaryTab"), ControlVisualConstants.InformationGlyph,
            [
                new(Localize("Visual.CapacityLabel"), model.LogicalLength.ToString("N0"), Localize("Visual.BlocksUnit")),
                new(Localize("Visual.AddressingLabel"), "LBA")
            ])
        };

        if (range is not null)
        {
            var entries = new List<MediaInspectorEntry>
            {
                new(Localize("Visual.StartLabel"), range.Start.ToString("N0"), "LBA"),
                new(Localize("Visual.LengthLabel"), range.Length.ToString("N0"), Localize("Visual.BlocksUnit")),
                new(Localize("Visual.StateLabel"), Localize("Visual.BlockState." + range.State))
            };
            entries.Add(new(
                Localize("Visual.PartitionTableLabel"),
                string.IsNullOrWhiteSpace(range.PartitionTable)
                    ? Localize("Visual.PartitionTable.None")
                    : range.PartitionTable.ToUpperInvariant()));
            if (range.PartitionNumber is { } partitionNumber)
                entries.Add(new(Localize("Visual.PartitionNumberLabel"), partitionNumber.ToString()));
            if (!string.IsNullOrWhiteSpace(range.FileSystemId))
                entries.Add(new(Localize("Visual.FileSystemLabel"), range.FileSystemId));
            sections.Add(new(Localize("Visual.RangeTitle"), ControlVisualConstants.InformationGlyph, entries));
        }

        if (model.Geometry is { } geometry)
        {
            var entries = new List<MediaInspectorEntry>
            {
                new("CHS", $"{geometry.Cylinders:N0} × {geometry.Heads:N0} × {geometry.SectorsPerTrack:N0}")
            };
            if (surface is { } selectedSurface)
                entries.Add(new(Localize("Visual.SurfaceLabel"), selectedSurface.ToString()));
            if (geometry.PlatterCount is { } platterCount)
                entries.Add(new(Localize("Visual.PlatterCountLabel"), platterCount.ToString()));
            sections.Add(new(Localize("Visual.ChsGeometryTitle"), ControlVisualConstants.InformationGlyph, entries));
        }

        return new(
            Localize("Visual.BlockInspectorTitle"),
            range is null ? null : $"LBA {range.Start:N0} · {range.Length:N0}",
            sections);
    }

    private string Localize(string key, params object[] arguments) => localize(key, arguments);

    private static void AddRange(List<BlockMediaRange> ranges, BlockMediaRange range)
    {
        if (range.Length <= 0) return;
        if (ranges.Count > 0)
        {
            var previous = ranges[^1];
            if (previous.Start + previous.Length == range.Start
                && previous.State == range.State
                && previous.PartitionTable == range.PartitionTable
                && previous.PartitionNumber == range.PartitionNumber
                && previous.FileSystemId == range.FileSystemId)
            {
                ranges[^1] = previous with { Length = previous.Length + range.Length };
                return;
            }
        }
        ranges.Add(range);
    }
}
