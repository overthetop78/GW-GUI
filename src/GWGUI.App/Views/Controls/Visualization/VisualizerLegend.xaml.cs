using GWGUI.App.Localization.Extensions;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Visualization;
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
                ("Visual.QualityDead", 0xBE373E),
                ("Visual.QualityPoor", 0xD37130),
                ("Visual.QualityPartial", 0x847639),
                ("Visual.QualityGood", 0x3F7448),
                ("Visual.QualityExcellent", 0x2FA65B)),
            MediaRepresentationKind.Sectors => Items(
                ("Visual.SectorWithData", 0x2DB064),
                ("Visual.SectorWithoutData", 0x4A535E),
                ("Visual.SectorDegraded", 0xE0972F),
                ("Visual.SectorDead", 0xCF4343)),
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
                ("Visual.SequentialSignal", 0x39444E),
                ("Visual.SequentialDecoded", 0x2FA65B),
                ("Visual.SequentialSilence", 0xC2CAD0),
                ("Visual.SequentialError", 0xCF373E)),
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
