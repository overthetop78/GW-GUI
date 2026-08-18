using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

internal sealed record EmulationCpuSettingsContent(
    FrameworkElement CpuModel,
    TextBlock CpuSummary,
    FrameworkElement Precision,
    FrameworkElement Fpu,
    TextBlock OriginalSpeed,
    FrameworkElement CpuSpeed);

internal static partial class EmulationSettingsLayout
{
    private const string ProcessorIcon = "\uE950";
    private const string CompatibilityIcon = "\uEA18";
    private const string AccelerationIcon = "\uE945";

    internal static Grid CpuSettingsPage(EmulationCpuSettingsContent settings)
    {
        var processor = new StackPanel();
        processor.Children.Add(SettingsFieldGrid((LocExtension.Get("Emulation.Cpu.Model"), settings.CpuModel)));
        settings.CpuSummary.Margin = new Thickness(12, 0, 12, 12);
        settings.CpuSummary.VerticalAlignment = VerticalAlignment.Center;
        settings.CpuSummary.TextWrapping = TextWrapping.Wrap;
        settings.CpuSummary.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
        processor.Children.Add(settings.CpuSummary);

        var root = TwoColumnPage(
            IconCard(processor, LocExtension.Get("Emulation.Cpu.Processor"), ProcessorIcon),
            IconCard(SettingsFieldGrid(
                    (LocExtension.Get("Emulation.Cpu.Precision"), settings.Precision),
                    (LocExtension.Get("Emulation.Fpu.Model"), settings.Fpu)),
                LocExtension.Get("Emulation.Cpu.Compatibility"), CompatibilityIcon));

        var acceleration = IconCard(SettingsFieldGrid(2,
                (LocExtension.Get("Emulation.Cpu.SpeedOriginal"), settings.OriginalSpeed),
                (LocExtension.Get("Emulation.Cpu.Speed"), settings.CpuSpeed)),
            LocExtension.Get("Emulation.Cpu.Acceleration"), AccelerationIcon);
        acceleration.Margin = new Thickness(0, 10, 0, 0);
        Grid.SetRow(acceleration, 1);
        Grid.SetColumnSpan(acceleration, 2);
        root.Children.Add(acceleration);
        return root;
    }
}
