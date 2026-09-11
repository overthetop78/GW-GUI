using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Rendering.Sectors;
using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Enums.Rendering.Sectors;
using GWGUI.App.Enums.ViewModels.Visualization;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.App.Presenters.Visualization;

public sealed class SectorMediaInspectorPresenter(Func<string, object[], string> localize)
{
    public SectorMediaRenderModel BuildRenderModel(
        SectorImage image,
        IReadOnlyDictionary<int, string>? fileSystemPaths = null)
    {
        ArgumentNullException.ThrowIfNull(image);
        var surfaces = Enumerable.Range(0, image.Heads).Select(surface =>
            new SectorMediaSurface(surface, Enumerable.Range(0, image.Cylinders).Select(cylinder =>
                new SectorMediaTrack(cylinder, Enumerable.Range(0, image.SectorsPerTrack).Select(slot =>
                {
                    var logicalBlock = checked((cylinder * image.Heads + surface) * image.SectorsPerTrack + slot);
                    var available = image.TryGetBlock(logicalBlock, out var block);
                    return new SectorMediaElement(
                        logicalBlock,
                        logicalBlock,
                        cylinder,
                        available ? block.Address.Number : slot,
                        available ? block.Data.Count : image.BlockSize,
                        StateFor(available ? block : null),
                        fileSystemPaths?.GetValueOrDefault(logicalBlock));
                }).ToArray())).ToArray())).ToArray();
        return new(image.FormatId, image.BlockSize, surfaces);
    }

    public MediaInspectorModel BuildInspectorModel(int surface, SectorMediaElement sector)
    {
        ArgumentNullException.ThrowIfNull(sector);
        var level = sector.State switch
        {
            SectorMediaElementState.IntegrityInvalid => MediaInspectorEntryLevel.Error,
            SectorMediaElementState.IntegrityUnknown or SectorMediaElementState.Missing => MediaInspectorEntryLevel.Warning,
            _ => MediaInspectorEntryLevel.Information
        };
        var entries = new List<MediaInspectorEntry>
        {
            new(Localize("Visual.SideLabel"), surface.ToString()),
            new(Localize("Visual.TrackLabel"), sector.Cylinder.ToString()),
            new(Localize("Visual.SectorLabel"), sector.Number.ToString()),
            new(Localize("Visual.LogicalBlockLabel"), sector.LogicalBlock.ToString()),
            new(Localize("Visual.SizeLabel"), sector.Size.ToString(), Localize("Visual.BytesUnit")),
            new(Localize("Visual.StateLabel"), Localize("Visual.SectorState." + sector.State), null, level)
        };
        if (!string.IsNullOrWhiteSpace(sector.FileSystemPath))
            entries.Add(new(Localize("Visual.FileSystemPathLabel"), sector.FileSystemPath));

        return new(
            Localize("Visual.SectorInspectorTitle"),
            $"{Localize("Visual.TrackLabel")} {sector.Cylinder} · {Localize("Visual.SectorLabel")} {sector.Number}",
            [new(Localize("Visual.SummaryTab"), ControlVisualConstants.InformationGlyph, entries)]);
    }

    private static SectorMediaElementState StateFor(SectorBlock? block) => block switch
    {
        null => SectorMediaElementState.Missing,
        { IntegrityValid: false } => SectorMediaElementState.IntegrityInvalid,
        { IntegrityValid: null } => SectorMediaElementState.IntegrityUnknown,
        _ => SectorMediaElementState.Available
    };

    private string Localize(string key, params object[] arguments) => localize(key, arguments);
}
