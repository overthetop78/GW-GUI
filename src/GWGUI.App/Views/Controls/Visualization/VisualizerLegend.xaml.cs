using GWGUI.App.Localization.Extensions;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Visualization;
using System.Windows.Controls;
using System.Windows.Media;

namespace GWGUI.App.Views.Controls.Visualization;

public partial class VisualizerLegend : UserControl
{
    public VisualizerLegend() => InitializeComponent();

    public void Configure(MediaVisualizationDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        LegendItems.ItemsSource = descriptor.RepresentationKind switch
        {
            MediaRepresentationKind.Flux => Items(
                ("Visual.NormalFlux", 0x24B35D),
                ("Visual.ShortTransition", 0x8F68FF),
                ("Visual.LongTransition", 0x53ADFF),
                ("Visual.StructureHeader", 0xFFCD40),
                ("Visual.StructureData", 0x43DCFF),
                ("Visual.StructureAnomaly", 0xFF4B60)),
            MediaRepresentationKind.Sectors => Items(
                ("Visual.SectorPresent", 0x24B35D),
                ("Visual.SectorAllocated", 0x1677D2),
                ("Visual.SectorFree", 0x73C991),
                ("Visual.SectorReserved", 0xFFCD40),
                ("Visual.SectorMissing", 0x9AA0A6),
                ("Visual.SectorUnreadable", 0xFF4B60)),
            MediaRepresentationKind.Blocks => Items(
                ("Visual.BlockAllocated", 0x1677D2),
                ("Visual.BlockFree", 0x73C991),
                ("Visual.BlockReserved", 0xFFCD40),
                ("Visual.BlockUnknown", 0x9AA0A6)),
            MediaRepresentationKind.OpticalTracks => Items(
                ("Visual.OpticalDataTrack", 0x1677D2),
                ("Visual.OpticalAudioTrack", 0x8F68FF),
                ("Visual.OpticalPregap", 0x9AA0A6),
                ("Visual.OpticalSession", 0x24B35D)),
            MediaRepresentationKind.Sequential => Items(
                ("Visual.SequentialSignal", 0x1677D2),
                ("Visual.SequentialDecoded", 0x24B35D),
                ("Visual.SequentialSilence", 0x9AA0A6),
                ("Visual.SequentialError", 0xFF4B60)),
            _ => []
        };
    }

    private static IReadOnlyList<KeyValuePair<string, Brush>> Items(params (string Key, uint Color)[] values)
        => values.Select(value => new KeyValuePair<string, Brush>(LocExtension.Get(value.Key), CreateBrush(value.Color))).ToArray();

    private static Brush CreateBrush(uint rgb)
    {
        var brush = new SolidColorBrush(Color.FromRgb(
            (byte)((rgb >> 16) & 0xff),
            (byte)((rgb >> 8) & 0xff),
            (byte)(rgb & 0xff)));
        brush.Freeze();
        return brush;
    }
}
