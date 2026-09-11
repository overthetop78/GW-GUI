using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Rendering.Blocks;
using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Enums.Rendering.Blocks;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Representations.Blocks;
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

        var ranges = new List<BlockMediaRange>();
        long cursor = 0;
        foreach (var sourceRange in blocks.Ranges.OrderBy(range => range.Address))
        {
            if (sourceRange.Address > cursor)
                ranges.Add(new(cursor, sourceRange.Address - cursor, BlockMediaRangeState.Unknown));
            var volume = document.Volumes.FirstOrDefault(item =>
                item.Start <= sourceRange.Address && item.Start + item.Length >= sourceRange.Address + sourceRange.Length);
            ranges.Add(new(
                sourceRange.Address,
                sourceRange.Length,
                BlockMediaRangeState.Available,
                volume?.PartitionScheme,
                volume?.PartitionNumber,
                volume?.FileSystemId));
            cursor = sourceRange.Address + sourceRange.Length;
        }
        if (cursor < logicalLength)
            ranges.Add(new(cursor, logicalLength - cursor, BlockMediaRangeState.Unknown));

        return new(logicalLength, ranges);
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
            if (!string.IsNullOrWhiteSpace(range.PartitionScheme))
                entries.Add(new(Localize("Visual.PartitionSchemeLabel"), range.PartitionScheme));
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
}
