using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.App.Constants;
using GWGUI.App.Localization;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

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
            Content = $"{ControlVisualConstants.AddGlyph}  {LocExtension.Get(EmulationResourceKeys.StorageDeviceAdd)}",
            MinWidth = EmulationStorageDeviceListConstants.AddButtonMinimumWidth,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _add.Click += (_, _) => AddRequested?.Invoke(this, EventArgs.Empty);
        footer.Children.Add(_add);
        var hint = new TextBlock
        {
            Text = LocExtension.Get(EmulationResourceKeys.StorageDeviceCapabilitiesHint),
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
        AddHeader(header, LocExtension.Get(EmulationResourceKeys.DeviceIdentifier), 0);
        AddHeader(header, LocExtension.Get(EmulationResourceKeys.DeviceType), 1);
        AddHeader(header, LocExtension.Get(EmulationResourceKeys.Model), 2);
        AddHeader(header, LocExtension.Get(EmulationResourceKeys.StorageAssociatedMedia), 3);
        AddHeader(header, LocExtension.Get(EmulationResourceKeys.Actions), 4);
        return header;
    }

    private void RebuildRows()
    {
        _rows.Children.Clear();
        foreach (var device in _devices)
        {
            var row = CreateColumns();
            row.MinHeight = EmulationStorageDeviceListConstants.RowMinimumHeight;
            row.Children.Add(Cell(device.Identifier, FontWeights.SemiBold));
            AddCell(row, TypeLabel(device.Type), 1);
            AddCell(row, device.Model, 2);
            var support = string.IsNullOrWhiteSpace(device.SupportPath)
                ? LocExtension.Get(EmulationResourceKeys.NotUsed)
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
                Content = LocExtension.Get(EmulationResourceKeys.StorageDeviceConfigure),
                MinWidth = EmulationStorageDeviceListConstants.ConfigureButtonMinimumWidth,
                Tag = device
            };
            configure.Click += (_, _) => ConfigureRequested?.Invoke(this, new EmulationStorageDeviceEventArgs(device));
            actions.Children.Add(configure);
            var remove = new Button
            {
                Content = ControlVisualConstants.DeleteGlyph,
                FontFamily = ControlVisualConstants.IconFont,
                MinWidth = EmulationStorageDeviceListConstants.RemoveButtonMinimumWidth,
                Padding = new Thickness(8),
                IsEnabled = device.CanRemove,
                ToolTip = LocExtension.Get(EmulationResourceKeys.Delete),
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
        grid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = new GridLength(EmulationStorageDeviceListConstants.IdentifierColumnWidth) });
        grid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = new GridLength(EmulationStorageDeviceListConstants.TypeColumnWidth) });
        grid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = new GridLength(EmulationStorageDeviceListConstants.ModelColumnRatio, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = new GridLength(EmulationStorageDeviceListConstants.MediaColumnRatio, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = new GridLength(EmulationStorageDeviceListConstants.ActionsColumnWidth) });
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

    private static string TypeLabel(EmulationMediaType type) => type switch
    {
        EmulationMediaType.Floppy => LocExtension.Get(EmulationResourceKeys.FloppyDevice),
        EmulationMediaType.HardDisk => LocExtension.Get(EmulationResourceKeys.HardDiskDevice),
        EmulationMediaType.CompactDisc => LocExtension.Get(EmulationResourceKeys.CompactDiscDevice),
        EmulationMediaType.Cassette => LocExtension.Get(EmulationResourceKeys.CassetteDevice),
        EmulationMediaType.Cartridge => LocExtension.Get(EmulationResourceKeys.CartridgeDevice),
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
