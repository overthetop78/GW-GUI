using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation;
using GWGUI.App.Contracts.Emulation.Firmware;
using GWGUI.App.Factories.Views.Common;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.Emulation;


namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static partial class EmulationSettingsLayout
{
    internal static Grid FirmwareSettingsPage(EmulationFirmwareSettingsContent settings)
    {
        settings.DetectedFirmware.MinWidth = 360;
        settings.DetectedFirmware.BorderThickness = new Thickness(0);
        settings.DetectedFirmware.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        ScrollViewer.SetHorizontalScrollBarVisibility(settings.DetectedFirmware, ScrollBarVisibility.Disabled);
        ScrollViewer.SetVerticalScrollBarVisibility(settings.DetectedFirmware, ScrollBarVisibility.Auto);

        var refresh = ControlUiFactory.IconTextButton(EmulationFirmwareSettingsConstants.RefreshIcon,
            LocExtension.Get("Common.Refresh"));
        refresh.Click += async (_, _) => await settings.Refresh(refresh);
        var use = settings.UseSelected;
        use.Content = LocExtension.Get("Emulation.Firmware.Use");
        use.MinWidth = 100;
        use.Margin = new Thickness(8, 0, 0, 0);

        var actions = new StackPanel { Orientation = Orientation.Horizontal };
        actions.Children.Add(refresh);
        actions.Children.Add(use);
        var configuredCard = new Border { Child = settings.ConfiguredFirmware };
        var detectedCard = ActionCard(settings.DetectedFirmware, LocExtension.Get("Emulation.Firmware.Rom.Detected"), actions);
        var page = TwoColumnPage(configuredCard, detectedCard);
        page.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);

        var openFolder = ControlUiFactory.IconTextButton(EmulationFirmwareSettingsConstants.OpenFolderIcon,
            LocExtension.Get("Emulation.Firmware.Rom.OpenFolder"));
        openFolder.HorizontalAlignment = HorizontalAlignment.Left;
        openFolder.Margin = new Thickness(0, 12, 0, 0);
        openFolder.Click += async (_, _) => await settings.OpenFolder(openFolder);
        Grid.SetRow(openFolder, 1);
        Grid.SetColumnSpan(openFolder, 2);
        page.Children.Add(openFolder);
        return page;
    }

    internal static Grid FirmwareRow(string name, string? version,
        EmulationFirmwareCompatibility compatibility, string? sourcePath = null)
    {
        var grid = new Grid { MinHeight = EmulationFirmwareSettingsConstants.FirmwareRowMinimumHeight,
            Margin = new Thickness(8, 2, 8, 2) };
        grid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = new GridLength(EmulationFirmwareSettingsConstants.FirmwareIconColumnWidth) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = new GridLength(EmulationFirmwareSettingsConstants.FirmwareCompatibilityColumnWidth) });

        grid.Children.Add(new TextBlock { Text = EmulationFirmwareSettingsConstants.FirmwareIcon,
            FontFamily = ControlVisualConstants.IconFont, FontSize = 22,
            VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center });
        var identityText = string.IsNullOrWhiteSpace(version) ? name : $"{name} — {version}";
        var identity = new TextBlock
        {
            Text = identityText,
            ToolTip = sourcePath ?? identityText,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(8, 0, 12, 0)
        };
        Grid.SetColumn(identity, 1); grid.Children.Add(identity);

        var colors = FirmwareBadgeColors(compatibility);
        var badge = new Border { Child = new TextBlock { Text = colors.Text, Foreground = colors.Foreground,
                VerticalAlignment = VerticalAlignment.Center }, Background = colors.Background, BorderBrush = colors.Border,
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Padding = new Thickness(10, 5, 10, 5),
            HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 8, 0) };
        Grid.SetColumn(badge, 2); grid.Children.Add(badge);

        return grid;
    }

    internal static int FirmwareCompatibilityOrder(EmulationFirmwareCompatibility compatibility) => compatibility switch
    {
        EmulationFirmwareCompatibility.Official => 0,
        EmulationFirmwareCompatibility.Compatible => 1,
        EmulationFirmwareCompatibility.PartiallyCompatible => 2,
        EmulationFirmwareCompatibility.Incompatible => 3,
        _ => 4
    };

    internal static void UpdateFirmwareUseButton(Button button,
        EmulationFirmwareCompatibility? compatibility) =>
        button.IsEnabled = compatibility is not null and not EmulationFirmwareCompatibility.Incompatible;

    private static (string Text, Brush Foreground, Brush Background, Brush Border) FirmwareBadgeColors(
        EmulationFirmwareCompatibility compatibility) => compatibility switch
        {
            EmulationFirmwareCompatibility.Official =>
                (LocExtension.Get("Emulation.Firmware.Official"), new SolidColorBrush(Color.FromRgb(31, 87, 142)),
                    new SolidColorBrush(Color.FromRgb(230, 242, 255)), new SolidColorBrush(Color.FromRgb(130, 181, 230))),
            EmulationFirmwareCompatibility.Compatible =>
                (LocExtension.Get("Emulation.Cpu.Compatibility.Compatible"), new SolidColorBrush(Color.FromRgb(31, 111, 58)),
                    new SolidColorBrush(Color.FromRgb(231, 247, 235)), new SolidColorBrush(Color.FromRgb(146, 211, 159))),
            EmulationFirmwareCompatibility.PartiallyCompatible =>
                (LocExtension.Get("Emulation.Firmware.PartiallyCompatible"), new SolidColorBrush(Color.FromRgb(133, 85, 8)),
                    new SolidColorBrush(Color.FromRgb(255, 246, 218)), new SolidColorBrush(Color.FromRgb(234, 187, 91))),
            EmulationFirmwareCompatibility.Incompatible =>
                (LocExtension.Get("Emulation.Firmware.Incompatible"), new SolidColorBrush(Color.FromRgb(145, 33, 33)),
                    new SolidColorBrush(Color.FromRgb(255, 235, 235)), new SolidColorBrush(Color.FromRgb(225, 145, 145))),
            _ => (LocExtension.Get("Common.Unknown"), new SolidColorBrush(Color.FromRgb(78, 85, 96)),
                new SolidColorBrush(Color.FromRgb(239, 241, 244)), new SolidColorBrush(Color.FromRgb(190, 195, 204)))
        };
}
