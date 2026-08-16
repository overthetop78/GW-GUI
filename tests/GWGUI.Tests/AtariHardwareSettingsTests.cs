using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GWGUI.App.Controls;
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
    public void EditorContainsGeneralCpuRamRomVideoAndAudioTabs()
    {
        RunOnSta(() =>
        {
            var general = new Border();
            var section = new AtariHardwareSettingsSection(general);
            var tabs = Assert.IsType<TabControl>(section.Content);

            Assert.Equal(AtariHardwareSettingsTestConstants.EditorTabCount, tabs.Items.Count);
            Assert.Same(general, Assert.IsType<ScrollViewer>(Assert.IsType<TabItem>(tabs.Items[0]).Content).Content
                is Border wrapper ? wrapper.Child : null);
            Assert.Contains(tabs.Items.OfType<TabItem>(), item => Equals(item.Header, AtariHardwareSettingsConstants.CpuTab));
            Assert.Contains(tabs.Items.OfType<TabItem>(), item => Equals(item.Header, AtariHardwareSettingsConstants.RamTab));
            Assert.Contains(tabs.Items.OfType<TabItem>(), item => Equals(item.Header, AtariHardwareSettingsConstants.RomTab));
        });
    }

    private static void RunOnSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception error) { failure = error; }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(AtariHardwareSettingsTestConstants.StaTimeoutMilliseconds));
        if (failure is not null) throw failure;
    }
}

internal static class AtariHardwareSettingsTestConstants
{
    internal const int CpuFieldCount = 4;
    internal const int MemoryFieldCount = 2;
    internal const int EditorTabCount = 6;
    internal const int StaTimeoutMilliseconds = 10000;
    internal const string UnknownKey = "future_hardware_option";
    internal const string UnknownValue = "preserved";
    internal const string ChangedValue = "changed";
    internal static readonly IReadOnlyDictionary<string, string> UnknownOptions =
        new Dictionary<string, string> { [UnknownKey] = UnknownValue };
    internal static readonly IReadOnlyList<KeyValuePair<string, string>> DisplayedOptions =
        [KeyValuePair.Create(AtariHardwareSettingsConstants.CpuOptionKey, ChangedValue)];
}
