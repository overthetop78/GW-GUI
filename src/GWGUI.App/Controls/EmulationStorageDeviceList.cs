using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

public enum EmulationStorageDeviceType
{
    Floppy,
    HardDisk,
    CompactDisc,
    Zip,
    Tape,
    Cartridge,
    Directory
}

public sealed record EmulationStorageDeviceItem(
    string Identifier,
    EmulationStorageDeviceType Type,
    string Model,
    string? SupportPath,
    bool CanRemove = true);

public sealed class EmulationStorageDeviceEventArgs(EmulationStorageDeviceItem device) : EventArgs
{
    public EmulationStorageDeviceItem Device { get; } = device;
}

/// <summary>
/// Common storage-device list. Machine-family editors provide the supported devices and
/// open their own configuration dialogs; this control owns only the shared presentation.
/// </summary>
public sealed class EmulationStorageDeviceList : UserControl
{
    private readonly StackPanel _rows = new();
    private readonly Button _add;
    private IReadOnlyList<EmulationStorageDeviceItem> _devices = [];

    public event EventHandler<EmulationStorageDeviceEventArgs>? ConfigureRequested;
    public event EventHandler<EmulationStorageDeviceEventArgs>? RemoveRequested;
    public event EventHandler? AddRequested;

    public EmulationStorageDeviceList()
    {
        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.Children.Add(BuildHeader());
        Grid.SetRow(_rows, 1);
        root.Children.Add(_rows);

        var footer = new Grid { Margin = new Thickness(0, 10, 0, 0) };
        footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        footer.ColumnDefinitions.Add(new ColumnDefinition());
        _add = new Button
        {
            Content = $"{ControlVisualConstants.AddGlyph}  {LocExtension.Get("Emulation.AddStorageDevice")}",
            MinWidth = 180,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _add.Click += (_, _) => AddRequested?.Invoke(this, EventArgs.Empty);
        footer.Children.Add(_add);
        var hint = new TextBlock
        {
            Text = LocExtension.Get("Emulation.StorageDeviceCapabilitiesHint"),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(18, 0, 4, 0)
        };
        hint.SetResourceReference(ForegroundProperty, "MutedTextBrush");
        Grid.SetColumn(hint, 1);
        footer.Children.Add(hint);
        Grid.SetRow(footer, 2);
        root.Children.Add(footer);
        Content = root;
    }

    public void SetDevices(IEnumerable<EmulationStorageDeviceItem> devices)
    {
        _devices = devices.ToArray();
        RebuildRows();
    }

    public void SetCanAdd(bool canAdd) => _add.Visibility = canAdd ? Visibility.Visible : Visibility.Collapsed;

    private static Grid BuildHeader()
    {
        var header = CreateColumns();
        header.Margin = new Thickness(0, 0, 0, 2);
        AddHeader(header, LocExtension.Get("Emulation.DeviceId"), 0);
        AddHeader(header, LocExtension.Get("Emulation.Type"), 1);
        AddHeader(header, LocExtension.Get("Emulation.Model"), 2);
        AddHeader(header, LocExtension.Get("Emulation.AssociatedMedia"), 3);
        AddHeader(header, LocExtension.Get("Emulation.InputActions"), 4);
        return header;
    }

    private void RebuildRows()
    {
        _rows.Children.Clear();
        foreach (var device in _devices)
        {
            var row = CreateColumns();
            row.MinHeight = 64;
            row.Children.Add(Cell(device.Identifier, FontWeights.SemiBold));
            AddCell(row, TypeLabel(device.Type), 1);
            AddCell(row, device.Model, 2);
            var support = string.IsNullOrWhiteSpace(device.SupportPath)
                ? LocExtension.Get("Emulation.NotUsed")
                : SupportSummary(device.SupportPath);
            var supportText = Cell(support);
            if (string.IsNullOrWhiteSpace(device.SupportPath))
                supportText.SetResourceReference(ForegroundProperty, "MutedTextBrush");
            Grid.SetColumn(supportText, 3);
            row.Children.Add(supportText);

            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            var configure = new Button
            {
                Content = LocExtension.Get("Emulation.ConfigureDevice"),
                MinWidth = 112,
                Tag = device
            };
            configure.Click += (_, _) => ConfigureRequested?.Invoke(this, new EmulationStorageDeviceEventArgs(device));
            actions.Children.Add(configure);
            var remove = new Button
            {
                Content = ControlVisualConstants.DeleteGlyph,
                FontFamily = ControlVisualConstants.IconFont,
                MinWidth = 40,
                Padding = new Thickness(8),
                IsEnabled = device.CanRemove,
                ToolTip = LocExtension.Get("Common.Delete"),
                Tag = device
            };
            remove.Click += (_, _) => RemoveRequested?.Invoke(this, new EmulationStorageDeviceEventArgs(device));
            actions.Children.Add(remove);
            Grid.SetColumn(actions, 4);
            row.Children.Add(actions);

            var separator = new Border
            {
                Child = row,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 3, 0, 3)
            };
            separator.SetResourceReference(BorderBrushProperty, "BorderBrush");
            _rows.Children.Add(separator);
        }
    }

    private static Grid CreateColumns()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(105) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(185) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.35, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.15, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(210) });
        return grid;
    }

    private static void AddHeader(Grid grid, string text, int column)
    {
        var header = Cell(text, FontWeights.SemiBold);
        Grid.SetColumn(header, column);
        grid.Children.Add(header);
    }

    private static void AddCell(Grid grid, string text, int column)
    {
        var cell = Cell(text);
        Grid.SetColumn(cell, column);
        grid.Children.Add(cell);
    }

    private static TextBlock Cell(string text, FontWeight? weight = null) => new()
    {
        Text = text,
        FontWeight = weight ?? FontWeights.Normal,
        VerticalAlignment = VerticalAlignment.Center,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(8, 8, 8, 8)
    };

    private static string TypeLabel(EmulationStorageDeviceType type) => type switch
    {
        EmulationStorageDeviceType.Floppy => LocExtension.Get("Emulation.FloppyDevice"),
        EmulationStorageDeviceType.HardDisk => LocExtension.Get("Emulation.HardDiskDevice"),
        EmulationStorageDeviceType.CompactDisc => LocExtension.Get("Emulation.CompactDiscDevice"),
        EmulationStorageDeviceType.Zip => "ZIP",
        EmulationStorageDeviceType.Tape => LocExtension.Get("Emulation.TapeDevice"),
        EmulationStorageDeviceType.Cartridge => LocExtension.Get(AtariStorageSettingsConstants.CartridgeResource),
        EmulationStorageDeviceType.Directory => LocExtension.Get(AtariStorageSettingsConstants.DirectoryResource),
        _ => type.ToString()
    };

    private static string SupportSummary(string path)
    {
        if (!File.Exists(path)) return Path.GetFileName(path);
        var size = new FileInfo(path).Length;
        var value = StorageSizeFormatter.FormatCapacity(size);
        return $"{Path.GetFileName(path)}{ControlVisualConstants.DetailSeparator}{value}";
    }
}
