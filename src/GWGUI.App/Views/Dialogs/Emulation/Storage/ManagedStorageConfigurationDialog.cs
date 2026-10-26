using GWGUI.App.Constants.Localization;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Functions.Views.Emulation.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using C = GWGUI.App.Constants.Emulation.StorageConfigurationLayoutConstants;

namespace GWGUI.App.Views.Dialogs.Emulation.Storage;

/// <summary>Présente uniquement les champs et groupes déclarés par le module.</summary>
internal sealed class ManagedStorageConfigurationDialog : Window
{
    private readonly Dictionary<string, FrameworkElement> _controls = new(StringComparer.Ordinal);
    internal IReadOnlyDictionary<string, string> Options => _controls.ToDictionary(item => item.Key, item => StorageConfigurationFieldFunctions.Read(item.Value), StringComparer.Ordinal);

    internal ManagedStorageConfigurationDialog(string identifier, string model, string moduleId, IReadOnlyList<EmulationSettingsField> fields, IReadOnlyDictionary<string, string>? values, string? explanation = null, IReadOnlyList<EmulationSettingsBlock>? blocks = null)
    {
        Title = LocExtension.Get(EmulationResourceKeys.StorageDeviceConfigure) + " " + identifier;
        Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        ShowInTaskbar = false; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var groups = blocks?.Where(block => block.IsVisible).ToArray() ?? [new EmulationSettingsBlock(string.Empty, EmulationMachineTab.Storage, string.Empty, fields)];
        Width = groups.Length > 1 ? C.GroupedWidth : C.ManagedWidth;
        MaxWidth = SystemParameters.WorkArea.Width * C.WorkAreaWidthRatio; MinWidth = C.MinimumWidth;
        MaxHeight = SystemParameters.WorkArea.Height * C.WorkAreaHeightRatio; SizeToContent = SizeToContent.Height;
        var body = new StackPanel();
        if (!string.IsNullOrWhiteSpace(explanation)) body.Children.Add(StorageDialogUi.Info(explanation));
        var grid = new Grid();
        for (var column = 0; column < C.MaximumColumns; column++) grid.ColumnDefinitions.Add(new ColumnDefinition());
        var cards = new List<(EmulationSettingsBlock Block, FrameworkElement Card)>();
        foreach (var block in groups)
        {
            var columns = Math.Clamp(block.Columns, 1, C.MaximumColumns);
            var content = new Grid();
            for (var column = 0; column < columns; column++) content.ColumnDefinitions.Add(new ColumnDefinition());
            var index = 0;
            foreach (var field in block.Fields.Where(field => field.IsVisible))
            {
                var control = StorageConfigurationFieldFunctions.Create(field, moduleId, values);
                _controls[field.Id] = control;
                var row = new StackPanel { Margin = new Thickness(0, 3, 10, block.IsCompact ? 7 : 10) };
                var label = LocExtension.GetForModule(moduleId, field.LabelResourceKey);
                if (control is CheckBox check) check.Content = label;
                else row.Children.Add(new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 4) });
                if (field.ExplanationResourceKey is { } help) control.ToolTip = LocExtension.GetForModule(moduleId, help);
                row.Children.Add(control);
                var position = index / columns;
                if (position >= content.RowDefinitions.Count) content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Grid.SetRow(row, position); Grid.SetColumn(row, index % columns); content.Children.Add(row); index++;
            }
            var title = string.IsNullOrWhiteSpace(block.TitleResourceKey) ? model : LocExtension.GetForModule(moduleId, block.TitleResourceKey);
            cards.Add((block, StorageDialogUi.Card(title, content)));
        }
        FrameworkElement[] visibleCards = [];
        void RefreshGroups()
        {
            var visible = cards.Where(item => item.Block.VisibleWhenFieldId is null || _controls.TryGetValue(item.Block.VisibleWhenFieldId, out var dependency) && StorageConfigurationFieldFunctions.Read(dependency) == item.Block.VisibleWhenValue).ToArray();
            var selectedCards = visible.Select(item => item.Card).ToArray();
            if (visibleCards.SequenceEqual(selectedCards)) return;
            visibleCards = selectedCards;
            grid.Children.Clear(); grid.RowDefinitions.Clear();
            var row = 0; var column = 0;
            for (var index = 0; index < visible.Length; index++)
            {
                var item = visible[index];
                var span = item.Block.Columns > 1 || visible.Length == 1 || index == visible.Length - 1 && column == 0 ? C.MaximumColumns : 1;
                if (span > 1 && column > 0) { row++; column = 0; }
                if (row >= grid.RowDefinitions.Count) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                item.Card.Margin = new Thickness(column > 0 ? 5 : 0, 0, span == 1 && column == 0 ? 5 : 0, 10);
                Grid.SetRow(item.Card, row); Grid.SetColumn(item.Card, column); Grid.SetColumnSpan(item.Card, span); grid.Children.Add(item.Card);
                column += span; if (column >= C.MaximumColumns) { row++; column = 0; }
            }
        }
        body.Children.Add(grid);
        grid.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler((_, _) => RefreshGroups()));
        grid.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler((_, _) => RefreshGroups()));
        grid.AddHandler(Selector.SelectionChangedEvent, new SelectionChangedEventHandler((_, _) => RefreshGroups()));
        RefreshGroups();
        var root = new Grid { Margin = new Thickness(18) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.Children.Add(StorageDialogUi.DialogHeader(IconGlyphs.Settings, Title, model));
        var scroll = new ScrollViewer { Content = body, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        Grid.SetRow(scroll, 1); root.Children.Add(scroll);
        var footer = StorageDialogUi.Footer(this, LocExtension.Get(EmulationResourceKeys.CommonSave));
        Grid.SetRow(footer, 2); root.Children.Add(footer); Content = root;
    }
}
