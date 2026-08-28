using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed class EmulationConfigurationTable : UserControl
{
    private readonly Grid _headers = new();
    private readonly ItemsControl _items = new();
    private IReadOnlyList<EmulationConfigurationTableRow> _rows = [];

    internal event Action<EmulationConfigurationTableRow>? EditRequested;
    internal event Action<EmulationConfigurationTableRow>? DeleteRequested;

    internal EmulationConfigurationTable()
    {
        RefreshLocalizedContent();
        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        root.Children.Add(_headers);
        var scroll = new ScrollViewer
        {
            Content = _items,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        Grid.SetRow(scroll, root.RowDefinitions.Count - 1);
        root.Children.Add(scroll);
        Content = root;
    }

    internal void RefreshLocalizedContent()
        => BuildHeaders();

    internal void SetRows(IReadOnlyList<EmulationConfigurationTableRow> rows)
    {
        _rows = rows;
        _items.ItemsSource = _rows.Select(CreateRow).ToArray();
    }
    private void BuildHeaders()
    {
        _headers.Children.Clear();
        _headers.ColumnDefinitions.Clear();
        _headers.SetResourceReference(Panel.BackgroundProperty, ControlVisualConstants.CardBrushResource);
        foreach (var resourceKey in EmulationConfigurationTableConstants.HeaderResourceKeys)
        {
            _headers.ColumnDefinitions.Add(new ColumnDefinition());
            var label = new TextBlock { Text = LocExtension.Get(resourceKey) };
            label.SetResourceReference(FrameworkElement.StyleProperty,
                EmulationConfigurationTableConstants.TableHeaderTextStyleResource);
            var cell = new Border
            {
                BorderThickness = EmulationConfigurationTableConstants.HeaderSeparatorThickness,
                Child = label
            };
            cell.SetResourceReference(Border.BorderBrushProperty, ControlVisualConstants.BorderBrushResource);
            Grid.SetColumn(cell, _headers.Children.Count);
            _headers.Children.Add(cell);
        }
    }

    private Border CreateRow(EmulationConfigurationTableRow row)
    {
        var grid = new Grid();
        foreach (var _ in EmulationConfigurationTableConstants.HeaderResourceKeys)
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        AddCell(grid, TextCell(row.MachineName));
        AddCell(grid, TextCell(row.Cpu));
        AddCell(grid, TextCell(row.TotalRam));
        AddCell(grid, GlyphCell(row.ReaderGlyphs));
        AddCell(grid, GlyphCell(row.PeripheralGlyphs));
        AddCell(grid, ActionsCell(row));
        var container = new Border
        {
            BorderThickness = EmulationConfigurationTableConstants.RowSeparatorThickness,
            Child = grid
        };
        container.SetResourceReference(Border.BackgroundProperty, ControlVisualConstants.CardBrushResource);
        container.SetResourceReference(Border.BorderBrushProperty, ControlVisualConstants.BorderBrushResource);
        container.MouseLeftButtonDown += (_, e) =>
        {
            if (e.ClickCount != 2)
                return;

            RequestEdit(row);
        };
        return container;
    }

    private static TextBlock TextCell(string text) => new()
    {
        Text = text,
        Margin = EmulationConfigurationTableConstants.CellMargin,
        VerticalAlignment = VerticalAlignment.Center
    };

    private static WrapPanel GlyphCell(IEnumerable<string> glyphs)
    {
        var panel = new WrapPanel
        {
            Margin = EmulationConfigurationTableConstants.CellMargin,
            VerticalAlignment = VerticalAlignment.Center
        };
        foreach (var glyph in glyphs)
            panel.Children.Add(new TextBlock
            {
                Text = glyph,
                FontFamily = ControlVisualConstants.IconFont,
                VerticalAlignment = VerticalAlignment.Center
            });
        return panel;
    }

    private StackPanel ActionsCell(EmulationConfigurationTableRow row)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        var edit = new Button
        {
            Content = ControlVisualConstants.EditGlyph,
            FontFamily = ControlVisualConstants.IconFont,
            Padding = new Thickness(8)
        };
        edit.Click += (_, _) => RequestEdit(row);
        panel.Children.Add(edit);
        var delete = new Button
        {
            Content = ControlVisualConstants.DeleteGlyph,
            FontFamily = ControlVisualConstants.IconFont,
            Padding = new Thickness(8)
        };
        delete.Click += (_, e) =>
        {
            e.Handled = true;
            DeleteRequested?.Invoke(row);
        };
        panel.Children.Add(delete);
        return panel;
    }

    private void RequestEdit(EmulationConfigurationTableRow row)
        => EditRequested?.Invoke(row);

    private static void AddCell(Grid row, UIElement content)
    {
        var cell = new Border
        {
            BorderThickness = EmulationConfigurationTableConstants.HeaderSeparatorThickness,
            Child = content
        };
        cell.SetResourceReference(Border.BorderBrushProperty, ControlVisualConstants.BorderBrushResource);
        Grid.SetColumn(cell, row.Children.Count);
        row.Children.Add(cell);
    }

}
