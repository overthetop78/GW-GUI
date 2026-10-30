using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation;
using GWGUI.App.Localization.Extensions;
using System.IO;
using System.Windows.Automation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.Emulation;


namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static partial class EmulationSettingsLayout
{
    internal static Grid FirmwareRow(string name, string? version, EmulationFirmwareCompatibility compatibility, string? sourcePath = null, string destination = "")
    {
        var grid = new Grid { MinHeight = EmulationFirmwareSettingsConstants.FirmwareRowMinimumHeight,
            Margin = new Thickness(6, 0, 6, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(EmulationFirmwareSettingsConstants.FirmwareIconColumnWidth) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, SharedSizeGroup = "FirmwareCompatibility" });
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = GridLength.Auto,
            SharedSizeGroup = EmulationFirmwareSettingsConstants.FirmwareBadgeSharedSizeGroup
        });

        grid.Children.Add(new TextBlock { Text = EmulationFirmwareSettingsConstants.FirmwareIcon,
            FontFamily = ControlVisualConstants.IconFont, FontSize = 15,
            VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center });
        var identityText = string.IsNullOrWhiteSpace(version) || name.Contains(version, StringComparison.OrdinalIgnoreCase) ? name : $"{name} — {version}";
        var identity = new TextBlock
        {
            Text = identityText,
            ToolTip = sourcePath ?? identityText,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(6, 0, 10, 0)
        };
        Grid.SetColumn(identity, 1); grid.Children.Add(identity);
        var file = new TextBlock { Text = sourcePath is null ? string.Empty : Path.GetFileName(sourcePath), ToolTip = sourcePath,
            TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0), Opacity = 0.7 };
        Grid.SetColumn(file, 2); grid.Children.Add(file);

        var colors = FirmwareBadgeColors(compatibility);
        var badge = FirmwareBadge(string.Empty, colors);
        var status = new StackPanel { Orientation = Orientation.Horizontal };
        var glyph = new TextBlock
        {
            Text = compatibility switch
            {
                EmulationFirmwareCompatibility.Official or EmulationFirmwareCompatibility.Compatible => IconGlyphs.Current,
                EmulationFirmwareCompatibility.PartiallyCompatible => IconGlyphs.Warning,
                EmulationFirmwareCompatibility.Incompatible => IconGlyphs.Error,
                _ => IconGlyphs.Information
            },
            FontFamily = ControlVisualConstants.IconFont,
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        glyph.SetResourceReference(TextBlock.ForegroundProperty, colors.Foreground);
        status.Children.Add(glyph);
        var label = new TextBlock { Text = colors.Text, Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        label.SetResourceReference(TextBlock.ForegroundProperty, colors.Foreground);
        status.Children.Add(label);
        badge.Child = status;
        badge.Padding = new Thickness(6, 2, 6, 2);
        badge.ToolTip = colors.Text;
        AutomationProperties.SetName(badge, colors.Text);
        Grid.SetColumn(badge, 3); grid.Children.Add(badge);

        if (!string.IsNullOrWhiteSpace(destination))
        {
            var destinationBadge = FirmwareBadge(destination, colors);
            Grid.SetColumn(destinationBadge, 4);
            grid.Children.Add(destinationBadge);
        }

        return grid;
    }

    private static Border FirmwareBadge(string text, (string Text, string Foreground, string Background, string Border) colors)
    {
        var label = new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        label.SetResourceReference(TextBlock.ForegroundProperty, colors.Foreground);
        var badge = new Border
        {
            Child = label,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(6, 2, 6, 2),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(EmulationFirmwareSettingsConstants.FirmwareBadgeSpacing / 2, 0, EmulationFirmwareSettingsConstants.FirmwareBadgeSpacing / 2, 0)
        };
        badge.SetResourceReference(Border.BackgroundProperty, colors.Background);
        badge.SetResourceReference(Border.BorderBrushProperty, colors.Border);
        return badge;
    }

    internal static int FirmwareCompatibilityOrder(EmulationFirmwareCompatibility compatibility) => compatibility switch
    {
        EmulationFirmwareCompatibility.Official => 0,
        EmulationFirmwareCompatibility.Compatible => 1,
        EmulationFirmwareCompatibility.PartiallyCompatible => 2,
        EmulationFirmwareCompatibility.Unknown => 3,
        EmulationFirmwareCompatibility.Incompatible => 4,
        _ => 5
    };

    private static (string Text, string Foreground, string Background, string Border) FirmwareBadgeColors(EmulationFirmwareCompatibility compatibility) => compatibility switch
    {
        EmulationFirmwareCompatibility.Official =>
            (LocExtension.Get("Emulation.Firmware.Official"), "AccentBrush", "SelectedBrush", "AccentBrush"),
        EmulationFirmwareCompatibility.Compatible =>
            (LocExtension.Get("Emulation.Cpu.Compatibility.Compatible"), "Theme.SuccessBrush", "Theme.SuccessSurface", "Theme.SuccessBrush"),
        EmulationFirmwareCompatibility.PartiallyCompatible =>
            (LocExtension.Get("Emulation.Firmware.PartiallyCompatible"), "Theme.WarningBrush", "CardBrush", "Theme.WarningBrush"),
        EmulationFirmwareCompatibility.Incompatible =>
            (LocExtension.Get("Emulation.Firmware.Incompatible"), "Theme.ErrorBrush", "Theme.DangerSurface", "Theme.ErrorBrush"),
        _ => (LocExtension.Get("Common.Unknown"), "MutedTextBrush", "CardBrush", "BorderBrush")
    };
}
