using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GWGUI.App.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

[Collection(AtariNativeCoreTestConstants.CollectionName)]
public sealed class AtariHardwareSettingsTests
{
    public static TheoryData<AtariMachineModel> EveryModel => new(Enum.GetValues<AtariMachineModel>());

    [Theory]
    [MemberData(nameof(EveryModel))]
    public void EveryModelBuildsCpuMemoryFirmwareAndRegionViewsFromCatalogs(AtariMachineModel model)
    {
        var view = AtariHardwareSettingsFunctions.Create(model,
            AtariHardwareSettingsTestConstants.UnknownOptions);

        Assert.Equal(AtariHardwareSettingsTestConstants.CpuFieldCount, view.Cpu.Count);
        Assert.Equal(AtariHardwareSettingsTestConstants.MemoryFieldCount, view.Memory.Count);
        Assert.Equal(AtariFirmwareCatalog.ForModel(model), view.Firmware);
        Assert.NotEmpty(view.Regions);
        Assert.All(view.Cpu.Concat(view.Memory), field =>
        {
            Assert.NotEmpty(field.Choices);
            Assert.Contains(field.Choices, choice => choice.Value == field.SelectedValue);
            var rule = AtariCompatibilityCatalog.Get(model).Options.Single(rule => rule.Option == field.Option);
            Assert.Equal(rule.Availability, field.Availability);
        });
    }

    [Theory]
    [InlineData(AtariMachineModel.St)]
    [InlineData(AtariMachineModel.Atari800)]
    [InlineData(AtariMachineModel.Atari2600)]
    [InlineData(AtariMachineModel.JaguarCd)]
    public void UnknownOptionsSurviveHardwareChanges(AtariMachineModel model)
    {
        var source = new AtariMachineConfiguration(model,
            options: AtariHardwareSettingsTestConstants.UnknownOptions);
        var result = AtariHardwareSettingsFunctions.ReplaceOptions(source,
            AtariHardwareSettingsTestConstants.DisplayedOptions);

        Assert.Equal(AtariHardwareSettingsTestConstants.UnknownValue,
            result.Options[AtariHardwareSettingsTestConstants.UnknownKey]);
        Assert.Equal(AtariHardwareSettingsTestConstants.ChangedValue,
            result.Options[AtariHardwareSettingsConstants.CpuOptionKey]);
    }

    [Fact]
    public void EditorContainsAllImplementedAtariSettingsTabs()
    {
        RunOnSta(() =>
        {
            var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
            app.InitializeComponent();
            var general = new Border();
            var section = new AtariHardwareSettingsSection(general);
            var tabs = Assert.IsType<TabControl>(section.Content);

            Assert.Equal(AtariHardwareSettingsTestConstants.EditorTabCount, tabs.Items.Count);
            Assert.Same(general, Assert.IsType<ScrollViewer>(Assert.IsType<TabItem>(tabs.Items[0]).Content).Content
                is Border wrapper ? wrapper.Child : null);
            Assert.Contains(tabs.Items.OfType<TabItem>(), item => Header(item) == AtariHardwareSettingsConstants.CpuTab);
            Assert.Contains(tabs.Items.OfType<TabItem>(), item => Header(item) == AtariHardwareSettingsConstants.RamTab);
            Assert.Contains(tabs.Items.OfType<TabItem>(), item => Header(item) == AtariHardwareSettingsConstants.RomTab);
            Assert.Contains(tabs.Items.OfType<TabItem>(), item => Header(item) ==
                LocExtension.Get(AtariStorageSettingsConstants.StorageTabResource));
        });
    }

    [Fact]
    public void EditorCanLoadTheSameConfigurationRepeatedly()
    {
        RunOnSta(() =>
        {
            var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
            app.InitializeComponent();
            var section = new AtariHardwareSettingsSection(new Border());
            var configuration = new AtariMachineConfiguration(AtariMachineModel.St);
            var dispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));

            WaitWithDispatcher(section.LoadAsync(configuration), dispatcher);
            WaitWithDispatcher(section.LoadAsync(configuration), dispatcher);
        });
    }

    [Fact]
    public void EditorLoadsEveryAtariModelWithoutLeavingASettingsPageEmpty()
    {
        RunOnSta(() =>
        {
            var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
            app.InitializeComponent();
            var section = new AtariHardwareSettingsSection(new Border
            {
                Child = new TextBlock { Text = AtariHardwareSettingsTestConstants.GeneralContent }
            });
            var dispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));

            foreach (var model in AtariHardwareSettingsTestConstants.RepresentativeModels)
            {
                WaitWithDispatcher(section.LoadAsync(new AtariMachineConfiguration(model)), dispatcher);
                var tabs = Assert.IsType<TabControl>(section.Content);
                Assert.All(tabs.Items.OfType<TabItem>(), item => Assert.Contains(
                    Descendants(item.Content), IsMeaningfulVisibleContent));
            }
        });
    }

    private static bool IsMeaningfulVisibleContent(object item) => item switch
    {
        TextBlock text => text.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(text.Text),
        ComboBox combo => combo.Visibility == Visibility.Visible && combo.Items.Count > 0,
        ListBox list => list.Visibility == Visibility.Visible,
        InputBindingEditor editor => editor.Visibility == Visibility.Visible,
        _ => false
    };

    private static IEnumerable<object> Descendants(object? root)
    {
        if (root is null) yield break;
        yield return root;
        if (root is ContentControl content && content.Content is not null)
            foreach (var child in Descendants(content.Content)) yield return child;
        if (root is Panel panel)
            foreach (var element in panel.Children.Cast<object>())
                foreach (var child in Descendants(element)) yield return child;
        if (root is Decorator decorator && decorator.Child is not null)
            foreach (var child in Descendants(decorator.Child)) yield return child;
        if (root is ItemsControl items)
            foreach (var item in items.Items.Cast<object>())
                foreach (var child in Descendants(item)) yield return child;
    }

    private static void WaitWithDispatcher(Task task, Dispatcher dispatcher)
    {
        while (!task.IsCompleted)
            dispatcher.Invoke(() => { }, DispatcherPriority.Background);
        task.GetAwaiter().GetResult();
    }

    private static string? Header(TabItem item) => (item.Header as MainTabHeader)?.Text;

    private static void RunOnSta(Action action)
        => WpfTestHost.Run(action);
}

internal static class AtariHardwareSettingsTestConstants
{
    internal const int CpuFieldCount = 4;
    internal const int MemoryFieldCount = 2;
    internal const int EditorTabCount = 10;
    internal const int StaTimeoutMilliseconds = 30000;
    internal const string UnknownKey = "future_hardware_option";
    internal const string UnknownValue = "preserved";
    internal const string ChangedValue = "changed";
    internal const string GeneralContent = "Atari general settings";
    internal static readonly IReadOnlyList<AtariMachineModel> RepresentativeModels =
        [AtariMachineModel.St, AtariMachineModel.Atari800, AtariMachineModel.Atari2600,
            AtariMachineModel.Atari7800, AtariMachineModel.Lynx, AtariMachineModel.JaguarCd];
    internal static readonly IReadOnlyDictionary<string, string> UnknownOptions =
        new Dictionary<string, string> { [UnknownKey] = UnknownValue };
    internal static readonly IReadOnlyList<KeyValuePair<string, string>> DisplayedOptions =
        [KeyValuePair.Create(AtariHardwareSettingsConstants.CpuOptionKey, ChangedValue)];
}
