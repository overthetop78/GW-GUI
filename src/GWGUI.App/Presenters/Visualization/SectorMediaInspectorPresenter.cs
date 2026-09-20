using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Rendering.Sectors;
using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Enums.Rendering.Sectors;
using GWGUI.App.Enums.ViewModels.Visualization;
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.App.Presenters.Visualization;

public sealed class SectorMediaInspectorPresenter(Func<string, object[], string> localize)
{
    public SectorMediaRenderModel BuildRenderModel(
        SectorImage image,
        IReadOnlyDictionary<int, string>? fileSystemPaths = null)
    {
        ArgumentNullException.ThrowIfNull(image);
        var surfaces = Enumerable.Range(0, image.Heads).Select(surface => new SectorMediaSurface(surface,
            Enumerable.Range(0, image.Cylinders)
                .Select(cylinder => AnalyzeTrack(image, surface, cylinder, fileSystemPaths))
                .ToArray())).ToArray();
        return new(image.FormatId, image.BlockSize, surfaces);
    }

    public SectorMediaRenderModel BuildGeometryModel(SectorImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        var surfaces = Enumerable.Range(0, image.Heads).Select(surface => new SectorMediaSurface(surface,
            Enumerable.Range(0, image.Cylinders).Select(cylinder => new SectorMediaTrack(cylinder,
                Enumerable.Range(0, image.SectorsPerTrack).Select(slot =>
                {
                    var logicalBlock = LogicalBlock(image, surface, cylinder, slot);
                    return new SectorMediaElement(logicalBlock, logicalBlock, cylinder, slot, image.BlockSize,
                        SectorMediaElementState.WithoutData);
                }).ToArray())).ToArray())).ToArray();
        return new(image.FormatId, image.BlockSize, surfaces);
    }

    public SectorMediaTrack AnalyzeTrack(
        SectorImage image,
        int surface,
        int cylinder,
        IReadOnlyDictionary<int, string>? fileSystemPaths = null)
    {
        ArgumentNullException.ThrowIfNull(image);
        var blocks = Enumerable.Range(0, image.SectorsPerTrack)
            .Select(slot =>
            {
                var logicalBlock = LogicalBlock(image, surface, cylinder, slot);
                return image.TryGetBlock(logicalBlock, out var block) ? block : null;
            })
            .ToArray();
        var duplicateAddresses = blocks
            .Where(block => block is not null)
            .GroupBy(block => block!.Address.Number)
            .Where(group => group.Skip(1).Any())
            .Select(group => group.Key)
            .ToHashSet();

        return new(cylinder, Enumerable.Range(0, image.SectorsPerTrack).Select(slot =>
        {
            var logicalBlock = LogicalBlock(image, surface, cylinder, slot);
            var block = blocks[slot];
            var available = block is not null;
            var addressInconsistent = available &&
                (block!.Address.Cylinder != cylinder || block.Address.Head != surface);
            var addressDuplicated = available && duplicateAddresses.Contains(block!.Address.Number);
            return new SectorMediaElement(
                logicalBlock,
                logicalBlock,
                cylinder,
                available ? block!.Address.Number : slot,
                available ? block!.Data.Count : image.BlockSize,
                StateFor(block, image.BlockSize, image.AllowsVariableBlockSize, addressInconsistent, addressDuplicated),
                fileSystemPaths?.GetValueOrDefault(logicalBlock));
        }).ToArray());
    }

    public MediaInspectorModel BuildInspectorModel(int surface, SectorMediaElement sector)
    {
        ArgumentNullException.ThrowIfNull(sector);
        var level = sector.State switch
        {
            SectorMediaElementState.Dead => MediaInspectorEntryLevel.Error,
            SectorMediaElementState.Degraded => MediaInspectorEntryLevel.Warning,
            _ => MediaInspectorEntryLevel.Information
        };
        var entries = new List<MediaInspectorEntry>
        {
            new(Localize("Visual.SideLabel"), surface.ToString()),
            new(Localize("Visual.TrackLabel"), sector.Cylinder.ToString()),
            new(Localize("Visual.SectorLabel"), sector.Number.ToString()),
            new(Localize("Visual.LogicalBlockLabel"), sector.LogicalBlock.ToString()),
            new(Localize("Visual.SizeLabel"), sector.Size.ToString(), Localize("Visual.BytesUnit")),
            new(Localize("Visual.StateLabel"), Localize("Visual.Sector" + sector.State), null, level)
        };
        if (!string.IsNullOrWhiteSpace(sector.FileSystemPath))
            entries.Add(new(Localize("Visual.FileSystemPathLabel"), sector.FileSystemPath));

        return new(
            Localize("Visual.SectorInspectorTitle"),
            $"{Localize("Visual.TrackLabel")} {sector.Cylinder} · {Localize("Visual.SectorLabel")} {sector.Number}",
            [new(Localize("Visual.SummaryTab"), ControlVisualConstants.InformationGlyph, entries)]);
    }

    private static int LogicalBlock(SectorImage image, int surface, int cylinder, int slot)
        => checked((cylinder * image.Heads + surface) * image.SectorsPerTrack + slot);

    private static SectorMediaElementState StateFor(
        SectorBlock? block,
        int expectedSize,
        bool allowsVariableSize,
        bool addressInconsistent,
        bool addressDuplicated) => block switch
    {
        null => SectorMediaElementState.WithoutData,
        { IntegrityValid: false } => SectorMediaElementState.Dead,
        { Data.Count: 0 } => SectorMediaElementState.WithoutData,
        _ when addressInconsistent || addressDuplicated => SectorMediaElementState.Degraded,
        { IntegrityValid: null } => SectorMediaElementState.Degraded,
        { Data.Count: var size } when !allowsVariableSize && size != expectedSize => SectorMediaElementState.Degraded,
        _ => SectorMediaElementState.WithData
    };

    private string Localize(string key, params object[] arguments) => localize(key, arguments);
}
