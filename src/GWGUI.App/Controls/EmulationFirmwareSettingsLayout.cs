using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

internal sealed record EmulationFirmwareSettingsContent(
    UIElement ConfiguredFirmware,
    ListBox DetectedFirmware,
    Func<Button, Task> Refresh,
    Button UseSelected,
    Func<Button, Task> OpenFolder);

internal enum EmulationFirmwareCompatibility
{
    Compatible,
    PartiallyCompatible,
    Unknown
}

internal static partial class EmulationSettingsLayout
{
    private const string RefreshIcon = "\uE72C";
    private const string OpenFolderIcon = "\uE838";
    private const string FirmwareIcon = "\uE950";
    private const double FirmwareRowMinimumHeight = 66;
    private const double FirmwareIconColumnWidth = 44;
    private const double FirmwareVersionColumnWidth = 145;
    private const double FirmwareCompatibilityColumnWidth = 185;

    internal static ScrollViewer FirmwareSettingsPage(EmulationFirmwareSettingsContent settings)
    {
        settings.DetectedFirmware.MinWidth = 360;
        settings.DetectedFirmware.BorderThickness = new Thickness(0);
        settings.DetectedFirmware.HorizontalContentAlignment = HorizontalAlignment.Stretch;

        var refresh = ControlUiFactory.IconTextButton(RefreshIcon, LocExtension.Get("Common.Refresh"));
        refresh.Click += async (_, _) => await settings.Refresh(refresh);
        var use = settings.UseSelected;
        use.Content = LocExtension.Get("Emulation.Firmware.Use");
        use.MinWidth = 100;
        use.Margin = new Thickness(8, 0, 0, 0);

        var actions = new StackPanel { Orientation = Orientation.Horizontal };
        actions.Children.Add(refresh);
        actions.Children.Add(use);
        var page = TwoColumnPage(
            ActionCard(settings.ConfiguredFirmware, LocExtension.Get("Emulation.Firmware.Rom.System")),
            ActionCard(settings.DetectedFirmware, LocExtension.Get("Emulation.Firmware.Rom.Detected"), actions));

        var openFolder = ControlUiFactory.IconTextButton(OpenFolderIcon, LocExtension.Get("Emulation.Firmware.Rom.OpenFolder"));
        openFolder.HorizontalAlignment = HorizontalAlignment.Left;
        openFolder.Margin = new Thickness(0, 12, 0, 0);
        openFolder.Click += async (_, _) => await settings.OpenFolder(openFolder);
        Grid.SetRow(openFolder, 1);
        Grid.SetColumnSpan(openFolder, 2);
        page.Children.Add(openFolder);
        return ScrollPage(page);
    }

    internal static Grid FirmwareRow(string name, string? version,
        EmulationFirmwareCompatibility compatibility, Action useFirmware)
    {
        var grid = new Grid { MinHeight = FirmwareRowMinimumHeight, Margin = new Thickness(8, 2, 8, 2) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(FirmwareIconColumnWidth) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(FirmwareVersionColumnWidth) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(FirmwareCompatibilityColumnWidth) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        grid.Children.Add(new TextBlock { Text = FirmwareIcon, FontFamily = ControlVisualConstants.IconFont, FontSize = 22,
            VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center });
        var nameText = new TextBlock { Text = name, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness(8, 0, 8, 0) };
        Grid.SetColumn(nameText, 1); grid.Children.Add(nameText);
        var versionText = new TextBlock { Text = version ?? LocExtension.Get("Common.Unknown"),
            VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(8, 0, 8, 0) };
        Grid.SetColumn(versionText, 2); grid.Children.Add(versionText);

        var colors = FirmwareBadgeColors(compatibility);
        var badge = new Border { Child = new TextBlock { Text = colors.Text, Foreground = colors.Foreground,
                VerticalAlignment = VerticalAlignment.Center }, Background = colors.Background, BorderBrush = colors.Border,
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Padding = new Thickness(10, 5, 10, 5),
            HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 8, 0) };
        Grid.SetColumn(badge, 3); grid.Children.Add(badge);

        var use = new Button { Content = LocExtension.Get("Emulation.Firmware.Use"), MinWidth = 90,
            Margin = new Thickness(8, 8, 0, 8) };
        use.Click += (_, _) => useFirmware();
        Grid.SetColumn(use, 4); grid.Children.Add(use);
        return grid;
    }

    private static (string Text, Brush Foreground, Brush Background, Brush Border) FirmwareBadgeColors(
        EmulationFirmwareCompatibility compatibility) => compatibility switch
        {
            EmulationFirmwareCompatibility.Compatible =>
                (LocExtension.Get("Emulation.Cpu.Compatibility.Compatible"), new SolidColorBrush(Color.FromRgb(31, 111, 58)),
                    new SolidColorBrush(Color.FromRgb(231, 247, 235)), new SolidColorBrush(Color.FromRgb(146, 211, 159))),
            EmulationFirmwareCompatibility.PartiallyCompatible =>
                (LocExtension.Get("Emulation.Firmware.PartiallyCompatible"), new SolidColorBrush(Color.FromRgb(133, 85, 8)),
                    new SolidColorBrush(Color.FromRgb(255, 246, 218)), new SolidColorBrush(Color.FromRgb(234, 187, 91))),
            _ => (LocExtension.Get("Common.Unknown"), new SolidColorBrush(Color.FromRgb(78, 85, 96)),
                new SolidColorBrush(Color.FromRgb(239, 241, 244)), new SolidColorBrush(Color.FromRgb(190, 195, 204)))
        };
}
