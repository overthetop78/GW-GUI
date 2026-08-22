using GWGUI.App.Contracts.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Functions.Views.Emulation.Settings;

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

        var processorCard = IconCard(processor, LocExtension.Get("Emulation.Cpu.Processor"), ProcessorIcon);
        var compatibilityFields = new List<(string Label, FrameworkElement Editor)>();
        if (settings.Precision is not null)
            compatibilityFields.Add((LocExtension.Get("Emulation.Cpu.Precision"), settings.Precision));
        if (settings.Fpu is not null)
            compatibilityFields.Add((LocExtension.Get("Emulation.Fpu.Model"), settings.Fpu));
        var root = compatibilityFields.Count == 0
            ? SingleColumnPage(processorCard)
            : TwoColumnPage(processorCard,
                IconCard(SettingsFieldGrid(compatibilityFields.ToArray()),
                    LocExtension.Get("Emulation.Cpu.Compatibility"), CompatibilityIcon));

        var speedFields = new List<(string Label, FrameworkElement Editor)>
        {
            (LocExtension.Get("Emulation.Cpu.SpeedOriginal"), settings.OriginalSpeed)
        };
        if (settings.CpuSpeed is not null)
            speedFields.Add((LocExtension.Get("Emulation.Cpu.Speed"), settings.CpuSpeed));
        var acceleration = IconCard(SettingsFieldGrid(speedFields.Count, speedFields.ToArray()),
            LocExtension.Get("Emulation.Cpu.Acceleration"), AccelerationIcon);
        acceleration.Margin = new Thickness(0, 10, 0, 0);
        Grid.SetRow(acceleration, 1);
        Grid.SetColumnSpan(acceleration, 2);
        root.Children.Add(acceleration);
        return root;
    }
}
