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
    [Fact]
    public void EightBitNativeOptionInventoryMatchesTheActiveEmulatorInterface()
    {
        string[] expected =
        [
            "atari800_ntscpal", "atari800_artifacting_mode", "atari800_resolution",
            "color_hue", "color_saturation", "color_contrast", "color_brightness", "color_gamma",
            "color_delay", "external_palette", "atari800_opt2", "paddle_active",
            "paddle_movement_speed", "pot_digital_sensitivity", "pot_analog_sensitivity",
            "pot_analog_deadzone", "atari800_keyboard", "atarixegs_keyboard_detached",
            "atari800_vkbd_enabled", "atari800_system", "atari800_internalbasic", "atari800_os_800",
            "atari800_os_xl", "atari800_os_5200", "atari800_basic_version", "atari800_mosaic",
            "atari800_axlon", "atari800_axlon_shadow", "atari800_mapram", "atari800_autofire",
            "atari800_show_speed", "atari800_show_diskled", "atari800_show_sector",
            "atari800_show_1200leds", "atari800_xep80", "atari800_rtime", "atari800_pdevice",
            "atari800_rdevice", "atari800_slowxex", "atari800_sioaccel", "atari800_cassboot",
            "atari800_pokey_stereo", "atari800_cfg"
        ];
        var actual = AtariEightBitSettingsCatalog.NativeSettings.Select(setting => setting.Key).ToArray();

        Assert.Equal(expected.Order(), actual.Order());
        Assert.Equal(actual.Length, actual.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Atari400NativeSettingsAreNormalizedAndIrrelevantOptionsAreRemoved()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari400, options:
            new Dictionary<string, string>
            {
                [AtariEightBitSettingsConstants.ColorHueOptionKey] = "9.99",
                [AtariEightBitSettingsConstants.DigitalSensitivityOptionKey] = "101",
                [AtariEightBitSettingsConstants.PaddleActiveOptionKey] = AtariEightBitSettingsConstants.Enabled,
                [AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey] =
                    AtariEightBitSettingsConstants.DualStick,
                [AtariConfigurationOptionConstants.VideoStandard] = "SECAM",
                [AtariConfigurationOptionConstants.VideoResolution] = "999x999",
                [AtariEightBitSettingsConstants.ArtifactingModeOptionKey] = "invalid",
                [AtariEightBitSettingsConstants.ShowActivityOptionKey] = "invalid",
                [AtariEightBitSettingsConstants.SioAccelerationOptionKey] = "invalid",
                [AtariEightBitSettingsConstants.XlOsOptionKey] = "something",
                [AtariEightBitSettingsConstants.Xep80OptionKey] = "port 1"
            });

        var options = AtariEightBitSettingsFunctions.Normalize(configuration);

        Assert.Equal(AtariEightBitSettingsConstants.DefaultColorAdjustment,
            options[AtariEightBitSettingsConstants.ColorHueOptionKey]);
        Assert.Equal(AtariEightBitSettingsConstants.DefaultSensitivity,
            options[AtariEightBitSettingsConstants.DigitalSensitivityOptionKey]);
        Assert.Equal(AtariEightBitSettingsConstants.None,
            options[AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey]);
        Assert.Equal(AtariEightBitSettingsConstants.NeutralAnalogDeadZone,
            options[AtariEightBitSettingsConstants.AnalogDeadZoneOptionKey]);
        Assert.Equal(AtariClassicRegion.Ntsc.ToString(),
            options[AtariConfigurationOptionConstants.VideoStandard]);
        Assert.Equal(AtariEightBitSettingsCatalog.OriginalComputerResolutions[0],
            options[AtariConfigurationOptionConstants.VideoResolution]);
        Assert.Equal(AtariEightBitSettingsConstants.None,
            options[AtariEightBitSettingsConstants.ArtifactingModeOptionKey]);
        Assert.Equal(AtariEightBitSettingsConstants.Enabled,
            options[AtariEightBitSettingsConstants.ShowActivityOptionKey]);
        Assert.Equal(AtariEightBitSettingsConstants.Enabled,
            options[AtariEightBitSettingsConstants.SioAccelerationOptionKey]);
        Assert.DoesNotContain(AtariEightBitSettingsConstants.XlOsOptionKey, options);
        Assert.DoesNotContain(AtariEightBitSettingsConstants.Xep80OptionKey, options);
    }


    [Fact]
    public void OriginalAtariOsChoicesFollowTheSelectedVideoStandard()
    {
        Assert.Equal(["auto", "Rev. A PAL", "Rev. B NTSC", "AltirraOS"],
            AtariEightBitSettingsCatalog.OriginalOsRevisions(AtariClassicRegion.Pal));
        Assert.Equal(["auto", "Rev. A NTSC", "Rev. B NTSC", "AltirraOS"],
            AtariEightBitSettingsCatalog.OriginalOsRevisions(AtariClassicRegion.Ntsc));
    }

    [Fact]
    public void OriginalAtariOsFirmwareCompatibilityFollowsTheSelectedVideoStandard()
    {
        var pal = AtariFirmwareCatalog.Get(AtariFirmwareConstants.AtariOsAId);
        var ntscA = AtariFirmwareCatalog.Get(AtariFirmwareConstants.AtariOsANtscId);
        var ntscB = AtariFirmwareCatalog.Get(AtariFirmwareConstants.AtariOsBId);
        var patchedB = AtariFirmwareCatalog.Get(AtariFirmwareConstants.AtariOsBPatchedId);

        Assert.True(AtariEightBitSettingsCatalog.IsOriginalOsCompatible(pal, AtariClassicRegion.Pal));
        Assert.False(AtariEightBitSettingsCatalog.IsOriginalOsCompatible(pal, AtariClassicRegion.Ntsc));
        Assert.True(AtariEightBitSettingsCatalog.IsOriginalOsCompatible(ntscA, AtariClassicRegion.Ntsc));
        Assert.False(AtariEightBitSettingsCatalog.IsOriginalOsCompatible(ntscA, AtariClassicRegion.Pal));
        Assert.True(AtariEightBitSettingsCatalog.IsOriginalOsCompatible(ntscB, AtariClassicRegion.Ntsc));
        Assert.True(AtariEightBitSettingsCatalog.IsOriginalOsCompatible(ntscB, AtariClassicRegion.Pal));
        Assert.True(AtariEightBitSettingsCatalog.IsOriginalOsCompatible(patchedB, AtariClassicRegion.Pal));
        Assert.True(AtariEightBitSettingsCatalog.IsOriginalOsCompatible(patchedB, AtariClassicRegion.Ntsc));
    }

    public static TheoryData<AtariMachineModel> EveryModel => new(Enum.GetValues<AtariMachineModel>());

    [Theory]
    [MemberData(nameof(EveryModel))]
    public void EveryModelBuildsCpuMemoryFirmwareAndRegionViewsFromCatalogs(AtariMachineModel model)
    {
        var view = AtariHardwareSettingsFunctions.Create(model,
            AtariHardwareSettingsTestConstants.UnknownOptions);

        Assert.Equal(AtariHardwareSettingsTestConstants.CpuFieldCount, view.Cpu.Count);
        var expectedMemoryFields = model is AtariMachineModel.Atari400 or AtariMachineModel.Atari800
            ? 5
            : AtariEightBitSettingsCatalog.SupportsMapRam(model) ? 3
            : AtariHardwareSettingsTestConstants.MemoryFieldCount;
        Assert.Equal(expectedMemoryFields, view.Memory.Count);
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
    public void Atari400UsesPalFrequencyAndExposesItsRealMemoryExpansions()
    {
        var options = new Dictionary<string, string>
        {
            [AtariVideoAudioSettingsConstants.StandardOptionKey] = AtariClassicRegion.Pal.ToString(),
            [AtariEightBitSettingsConstants.MosaicMemoryOptionKey] = "80 KB",
            [AtariEightBitSettingsConstants.AxlonMemoryOptionKey] = "256 KB"
        };
        var view = AtariHardwareSettingsFunctions.Create(AtariMachineModel.Atari400, options);

        var cpu = view.Cpu.Single(field => field.Option == AtariSettingOption.CpuModel);
        var speed = view.Cpu.Single(field => field.Option == AtariSettingOption.CpuSpeed);
        Assert.Equal(AtariClassicCpu.Mos6502B.ToString(), cpu.SelectedValue);
        Assert.Equal(AtariEightBitSettingsConstants.PalCpuFrequencyHz.ToString(), speed.SelectedValue);
        Assert.Contains("MHz", speed.Choices.Single().DisplayName);
        Assert.Contains("PAL", speed.Choices.Single().DisplayName, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(48 * 1024 + 80 * 1024 + 256 * 1024,
            AtariHardwareSettingsFunctions.TotalMemoryBytes(options, view));
        Assert.Equal(AtariOptionAvailability.Hidden,
            view.Cpu.Single(field => field.Option == AtariSettingOption.CpuPrecision).Availability);
        Assert.Equal(AtariOptionAvailability.Hidden,
            view.Cpu.Single(field => field.Option == AtariSettingOption.Fpu).Availability);
        Assert.DoesNotContain(view.Memory, field => field.Option == AtariSettingOption.MapRam);
    }

    [Fact]
    public void MapRamIsOfferedOnlyToXlXeModels()
    {
        var xl = AtariHardwareSettingsFunctions.Create(AtariMachineModel.Atari800Xl,
            AtariHardwareSettingsTestConstants.UnknownOptions);
        Assert.Equal(AtariOptionAvailability.Editable,
            xl.Memory.Single(field => field.Option == AtariSettingOption.MapRam).Availability);

        var original = AtariHardwareSettingsFunctions.Create(AtariMachineModel.Atari400,
            AtariHardwareSettingsTestConstants.UnknownOptions);
        Assert.DoesNotContain(original.Memory, field => field.Option == AtariSettingOption.MapRam);
    }

    [Fact]
    public void Atari400EditorHidesMouseTabAndKeepsCommonTabsForOtherModels()
    {
        RunOnSta(() =>
        {
            var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
            app.InitializeComponent();
            var section = new AtariHardwareSettingsSection(new Border());
            var dispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));
            WaitWithDispatcher(section.LoadAsync(new AtariMachineConfiguration(AtariMachineModel.Atari400)), dispatcher);
            var tabs = Assert.IsType<TabControl>(section.Content).Items.OfType<TabItem>().ToArray();
            Assert.Equal(Visibility.Collapsed, tabs.Single(tab =>
                Equals(tab.Tag, EmulationMachineTabKind.Mouse)).Visibility);

            WaitWithDispatcher(section.LoadAsync(new AtariMachineConfiguration(AtariMachineModel.St)), dispatcher);
            Assert.Equal(Visibility.Visible, tabs.Single(tab =>
                Equals(tab.Tag, EmulationMachineTabKind.Mouse)).Visibility);
        });
    }

    [Fact]
    public void Atari400MemoryExtensionsAreMutuallyExclusiveAndShadowFollowsAxlon()
    {
        RunOnSta(() =>
        {
            var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
            app.InitializeComponent();
            var section = new AtariHardwareSettingsSection(new Border());
            var dispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));
            WaitWithDispatcher(section.LoadAsync(new AtariMachineConfiguration(AtariMachineModel.Atari400)), dispatcher);

            var editors = Descendants(section).OfType<ComboBox>().ToArray();
            var mosaic = editors.Single(combo => combo.Items.OfType<AtariHardwareChoice>()
                .Any(choice => choice.Value == "144 KB"));
            var axlon = editors.Single(combo => combo.Items.OfType<AtariHardwareChoice>()
                .Any(choice => choice.Value == "4 MB"));
            var shadow = editors.Single(combo => combo.Items.OfType<AtariHardwareChoice>().Count() == 2
                && combo.Items.OfType<AtariHardwareChoice>().Any(choice =>
                    choice.Value == AtariEightBitSettingsConstants.Enabled)
                && combo.Visibility == Visibility.Collapsed);

            Assert.Equal(Visibility.Collapsed, shadow.Visibility);
            axlon.SelectedValue = "128 KB";
            Assert.Equal(Visibility.Visible, shadow.Visibility);
            mosaic.SelectedValue = "16 KB";
            Assert.Equal(AtariEightBitSettingsConstants.Disabled, axlon.SelectedValue);
            Assert.Equal(Visibility.Collapsed, shadow.Visibility);
        });
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
            Assert.Equal(new Thickness(EmulationMachineTabs.OuterMargin), tabs.Margin);
            Assert.Same(general, Assert.IsType<TabItem>(tabs.Items[0]).Content);
            Assert.All(tabs.Items.OfType<TabItem>(), item => Assert.Equal(
                new Thickness(EmulationMachineTabs.HorizontalPadding, EmulationMachineTabs.VerticalPadding,
                    EmulationMachineTabs.HorizontalPadding, EmulationMachineTabs.VerticalPadding),
                item.Padding));
            Assert.Contains(tabs.Items.OfType<TabItem>(), item => Header(item) == LocExtension.Get("Emulation.Tab.Cpu"));
            Assert.Contains(tabs.Items.OfType<TabItem>(), item => Header(item) == LocExtension.Get("Emulation.Tab.Ram"));
            Assert.Contains(tabs.Items.OfType<TabItem>(), item => Header(item) == LocExtension.Get("Emulation.Tab.Rom"));
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
    public void EditorApplyKeepsEveryDisplayedHardwareChoiceIncludingChangedMemory()
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
            var memory = Descendants(section).OfType<ComboBox>().Single(combo => combo.Items
                .OfType<AtariHardwareChoice>().Any(choice => choice.Value == "1048576"));
            memory.SelectedItem = memory.Items.OfType<AtariHardwareChoice>()
                .Single(choice => choice.Value == "1048576");

            var saved = section.Apply(configuration);

            Assert.Equal("1048576", saved.Options[AtariHardwareSettingsConstants.MainMemoryOptionKey]);
            foreach (var editor in Descendants(section).OfType<ComboBox>()
                         .Where(combo => combo.Items.OfType<AtariHardwareChoice>().Any()))
            {
                var selected = Assert.IsType<AtariHardwareChoice>(editor.SelectedItem);
                Assert.Contains(selected.Value, saved.Options.Values);
            }
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
