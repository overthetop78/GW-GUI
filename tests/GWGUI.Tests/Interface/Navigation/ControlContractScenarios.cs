using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GWGUI.App.Localization.Sources;
using GWGUI.App.Views.Controls.Common;
using GWGUI.App.Views.Controls.Conversion;
using GWGUI.App.Views.Controls.Read;
using GWGUI.App.Views.Controls.Shell;
using GWGUI.App.Views.Controls.Write;
using GWGUI.App.Views.Controls.Options;
using GWGUI.App.Views.Windows.Updates;
using GWGUI.App.Services.Updates;
using GWGUI.App.Contracts.Updates;
using GWGUI.Updates.Contracts;

namespace GWGUI.Tests.Interface.Navigation;

internal static class ControlContractScenarios
{
    public static void MenuLabels(string cultureName)
    {
        var localization = LocalizationSource.Instance;
        var previous = (localization.Culture, localization.UiCulture);
        var culture = CultureInfo.GetCultureInfo(cultureName);
        try
        {
            localization.SetCultures(culture, culture);
            var menu = new MainMenu();
            var entries = NavigationScenarios.Descendants((Menu)menu.Content).ToArray();
            Assert.NotEmpty(entries);
            foreach (var entry in entries)
            {
                var binding = entry.GetBindingExpression(HeaderedItemsControl.HeaderProperty);
                Assert.NotNull(binding);
                binding.UpdateTarget();
                Assert.Equal(BindingStatus.Active, binding.Status);
                var label = Assert.IsType<string>(entry.Header);
                Assert.False(string.IsNullOrWhiteSpace(label));
                Assert.False(label.StartsWith('[') && label.EndsWith(']'), $"Unresolved menu label: {label}");
                Assert.True(entry.Focusable);
            }

            var updates = new UpdatesWindow(new PendingModuleInstallationStore(), _ => { });
            try
            {
                var navigation = updates.Section.NavigationControl;
                var updateEntries = Assert.IsAssignableFrom<IEnumerable<UpdateNavigationItem>>(
                    navigation.ItemsSource).ToArray();
                Assert.True(updateEntries.Length >= 3);
                Assert.All(updateEntries, entry =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(entry.Title));
                    Assert.False(entry.Title.StartsWith('[') && entry.Title.EndsWith(']'));
                    Assert.False(string.IsNullOrWhiteSpace(entry.Subtitle));
                    Assert.False(string.IsNullOrWhiteSpace(entry.Icon));
                    Assert.NotNull(entry.Foreground);
                    Assert.NotNull(entry.Background);
                });
                Assert.True(navigation.Focusable);
                Assert.True(KeyboardNavigation.GetIsTabStop(navigation));
            }
            finally { updates.Close(); }
            Assert.False(menu.IsLoaded);
        }
        finally { localization.SetCultures(previous.Culture, previous.UiCulture); }
    }

    public static void PrimaryCommand(int index, string automationId)
    {
        var calls = 0;
        UserControl section;
        Button execute;
        switch (index)
        {
            case 0:
                var read = new ReadTabSection();
                read.ExecuteRequested += (_, _) => calls++;
                section = read;
                execute = read.ExecuteActionButton;
                break;
            case 1:
                var write = new WriteTabSection();
                write.ExecuteRequested += (_, _) => calls++;
                section = write;
                execute = write.ExecuteActionButton;
                break;
            case 2:
                var conversion = new ConversionTabSection();
                conversion.ExecuteRequested += (_, _) => calls++;
                section = conversion;
                execute = conversion.ExecuteActionButton;
                break;
            default: throw new ArgumentOutOfRangeException(nameof(index));
        }
        Assert.Equal(automationId, AutomationProperties.GetAutomationId(execute));
        Assert.True(execute.Focusable);
        Assert.True(KeyboardNavigation.GetIsTabStop(execute));
        Assert.Equal(Visibility.Visible, execute.Visibility);
        var labelBinding = execute.GetBindingExpression(ContentControl.ContentProperty);
        Assert.NotNull(labelBinding);
        labelBinding.UpdateTarget();
        Assert.False(string.IsNullOrWhiteSpace(Assert.IsType<string>(execute.Content)));
        execute.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, execute));
        Assert.Equal(1, calls);
        Assert.False(section.IsLoaded);
    }

    public static void UpdatesFooterReflectsPendingModuleInstallation()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI.Tests", Guid.NewGuid().ToString("N"));
        var prepared = Path.Combine(root, "prepared");
        Directory.CreateDirectory(prepared);
        var store = new PendingModuleInstallationStore();
        var confirmations = 0;
        var window = new UpdatesWindow(store, _ => { }, () =>
        {
            confirmations++;
            return MessageBoxResult.No;
        });
        try
        {
            Assert.Equal(GWGUI.App.Localization.Extensions.LocExtension.Get("Common.Close"),
                window.FooterButton.Content);
            Assert.Equal(Visibility.Collapsed, window.FooterHint.Visibility);
            Assert.False(window.RequiresFinishingBeforeClose);

            Assert.True(store.TryAdd(new PendingModuleInstallation(
                "synthetic", "Synthetic", "1.0.0", "https://example.invalid/catalog.json",
                new PreparedUpdateComponent("synthetic", UpdateComponentKind.Module,
                    UpdateComponentOperation.InstallModule, "1.0.0", prepared), root)));

            Assert.Equal(GWGUI.App.Localization.Extensions.LocExtension.Get("Updates.FinishModuleInstallations"),
                window.FooterButton.Content);
            Assert.Equal(Visibility.Visible, window.FooterHint.Visibility);
            Assert.True(window.RequiresFinishingBeforeClose);

            window.Close();
            Assert.Equal(1, confirmations);
            Assert.Empty(store.Items);
            Assert.False(window.RequiresFinishingBeforeClose);
        }
        finally
        {
            store.Clear();
            window.Close();
            try { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); } catch { }
        }
    }

    public static void PathControls()
    {
        var section = new PathSection { Label = "synthetic location", Text = "synthetic path" };
        section.Input.GetBindingExpression(TextBox.TextProperty)!.UpdateTarget();
        section.Input.GetBindingExpression(AutomationProperties.NameProperty)!.UpdateTarget();
        Assert.Equal(section.Text, section.Input.Text);
        Assert.Equal(section.Label, AutomationProperties.GetName(section.Input));
        Assert.True(section.Input.IsReadOnly);
        Assert.True(section.Input.Focusable);
        Assert.True(KeyboardNavigation.GetIsTabStop(section.Input));
        Assert.True(section.BrowseButton.Focusable);
        Assert.True(KeyboardNavigation.GetIsTabStop(section.BrowseButton));
        Assert.True(Grid.GetColumn(section.Input) < Grid.GetColumn(section.BrowseButton));
        Assert.Equal(Visibility.Visible, section.BrowseButton.Visibility);
        Assert.Equal(Visibility.Collapsed, section.ActionButton.Visibility);
        Assert.False(section.IsLoaded);
    }
}
