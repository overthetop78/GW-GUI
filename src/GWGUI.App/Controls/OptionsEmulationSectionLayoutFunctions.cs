using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

public sealed partial class OptionsEmulationSection
{
    private UIElement BuildGeneralTab()
    {
        var rows = new StackPanel { Margin = new Thickness(12) };
        rows.Children.Add(FolderRow("Emulation.Folder.StorageBase", _storageFolder));
        rows.Children.Add(FolderRow("Emulation.Folder.Capture", _captureFolder));
        rows.Children.Add(FolderRow("Emulation.Folder.State", _stateFolder));
        return EmulationSettingsLayout.ScrollPage(rows);
    }

    private UIElement BuildShortcutsTab()
    {
        _shortcuts.ConfigurePresentation(LocExtension.Get("Emulation.Input.Actions"),
            LocExtension.Get("Emulation.Input.Binding.Search"));
        _shortcuts.BindingsChanged += async (_, _) => await SaveShortcutsAsync();
        return EmulationSettingsLayout.ScrollPage(new StackPanel
        {
            Margin = new Thickness(12),
            Children = { EmulationSettingsLayout.InputBindings(_shortcuts,
                LocExtension.Get("Emulation.Shortcut.Global")) }
        });
    }

    private UIElement BuildConfigurationsTab()
    {
        _configurationList.ItemsSource = _configurations;
        _configurationList.DisplayMemberPath = nameof(EmulationConfigurationListItem.DisplayName);
        var remove = new Button
        {
            Content = LocExtension.Get("Common.Delete"),
            MinWidth = 110,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
        };
        remove.Click += async (_, _) => await DeleteSelectedConfigurationAsync();
        return EmulationSettingsLayout.ScrollPage(new StackPanel
        {
            Margin = new Thickness(12),
            Children = { _configurationList, remove }
        });
    }

    private FrameworkElement FolderRow(string resourceKey, TextBox target)
    {
        var browse = new Button { Content = LocExtension.Get("Common.Browse"), MinWidth = 100 };
        browse.Click += async (_, _) => await BrowseFolderAsync(target);
        return EmulationSettingsLayout.CompactForm(1,
            (LocExtension.Get(resourceKey), PathControl(target, browse)));
    }

    private static FrameworkElement PathControl(TextBox path, Button browse)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.Children.Add(path);
        Grid.SetColumn(browse, 1);
        grid.Children.Add(browse);
        return grid;
    }

    private static void AddTab(ItemsControl tabs, string icon, string title, UIElement content)
    {
        var tab = new TabItem
        {
            Header = new MainTabHeader { Icon = icon, Text = title },
            Content = content,
            Padding = new Thickness(14, 8, 14, 8)
        };
        tab.SetResourceReference(FrameworkElement.StyleProperty, "MainTabItemStyle");
        tabs.Items.Add(tab);
    }
}
