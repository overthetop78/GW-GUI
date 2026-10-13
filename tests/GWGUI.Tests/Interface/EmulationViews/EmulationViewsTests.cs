using System.IO;
using System.Net.Http;

namespace GWGUI.Tests.Interface.EmulationViews;
[Collection("WPF")]
public class EmulationViewsTests(GWGUI.Tests.Application.TestInfrastructure.StaExecutionScenarios sta)
{
    [Fact] public Task DraftReloadDiscardAndSavedConfigurationAreIsolated() => sta.RunAsync(MachineConfigurationScenarios.DraftLifecycle);
    [Fact] public Task MachineSwitchKeepsSavedAndDraftChanges() => sta.RunAsync(MachineConfigurationScenarios.MachineSwitchKeepsSavedAndDraftChanges);
    [Fact] public Task VideoProfileChangesArePublishedAndPersisted() => sta.RunAsync(MachineConfigurationScenarios.VideoProfileChangesArePublishedAndPersisted);
    [Fact] public Task TabsSelectAndCloseOnlyTargetSession() => sta.RunAsync(MachineTabsScenarios.Tabs);
    [Fact] public Task CollapsedSelectorShowsLocalizedLabel() => sta.Run(MachineTabsScenarios.CollapsedSelectorShowsLocalizedLabel);
    [Theory] [InlineData(false)] [InlineData(true)] public Task ConfigurationDraftSaveAndRetry(bool failure) => sta.RunAsync(() => MachineConfigurationScenarios.ConfigurationEditing(failure));
    [Theory] [InlineData(false)] [InlineData(true)] public Task CommandsRespectPowerStateAndReportErrors(bool failure) => sta.Run(() => EmulationInteractionScenarios.Commands(failure));
    [Fact] public Task EmulationErrorsAppearOnceInConsoleAndDetachCleanly() => sta.Run(EmulationInteractionScenarios.ErrorLogAppearsOnceInConsoleAndDetaches);
    [Fact] public Task ConsoleLayoutIsIndependentForEachMainTab() => sta.Run(EmulationInteractionScenarios.ConsoleLayoutIsIndependentForEachMainTab);
    [Fact] public void CassetteCommandsReflectCapabilitiesAndTransportState() => CassetteTransportPresentationScenarios.CommandsReflectCapabilitiesAndTransportState();
    [Fact] public Task CassettePanelShowsEveryCommandBelowTheDevice() => sta.Run(CassetteTransportPresentationScenarios.PanelContainsASeparateRowWithEveryCommand);
    [Fact] public Task EmptyDeviceKeepsItsSizeAndCentersItsLed() => sta.Run(CassetteTransportPresentationScenarios.DeviceHeaderKeepsEjectSpaceAndCentersLedWhenEmpty);

    [Fact]
    public Task AcornJoystickVisualAndDisconnectedPortRemainEditable() => sta.Run(() =>
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Acorn.Modules.AcornEmulationModule(Path.GetTempPath(), Path.GetTempPath(), http, Path.GetTempPath());
        var configuration = module.CreateConfiguration("BbcMicroModelB");
        var controller = new GWGUI.App.Controllers.Emulation.Input.EmulationInputSettingsController(module);
        var root = controller.CreateContent(GWGUI.Emulation.Enums.EmulationMachineTab.Controllers, configuration, []);
        var window = new System.Windows.Window { Content = root, Width = 1100, Height = 800, ShowInTaskbar = false };
        try
        {
            window.Show();
            var selectors = Descendants(root).OfType<System.Windows.Controls.ComboBox>()
                .Where(combo => combo.Items.OfType<GWGUI.App.Contracts.Emulation.Controllers.EmulationControllerChoiceView>().Any()).ToArray();
            Assert.Equal(2, selectors.Length);
            foreach (var selector in selectors)
            {
                var disconnected = selector.Items.OfType<GWGUI.App.Contracts.Emulation.Controllers.EmulationControllerChoiceView>().Single(choice => choice.Choice.Id == "None");
                var joystick = selector.Items.OfType<GWGUI.App.Contracts.Emulation.Controllers.EmulationControllerChoiceView>().Single(choice => choice.Choice.Id == "AnalogJoystick");
                selector.SelectedItem = disconnected;
                var without = module.DescribeInputSettings(controller.Apply(configuration));
                Assert.Contains(without.ControllerPorts, port => port.SelectedControllerId == "None");
                selector.SelectedItem = joystick;
            }
            var restored = module.DescribeInputSettings(controller.Apply(configuration));
            Assert.All(restored.ControllerPorts, port =>
            {
                Assert.Equal(GWGUI.Emulation.Constants.EmulationControllerVisualIds.CompetitionPro5000, port.VisualId);
                Assert.Equal(5, port.Bindings.Definitions.Count);
                Assert.All(port.Bindings.Values.Values, Assert.Empty);
            });
        }
        finally
        {
            GWGUI.Tests.Application.TestInfrastructure.TestResourceCleanup.ReleaseWindows(System.Windows.Application.Current);
        }
    });

    private static IEnumerable<System.Windows.DependencyObject> Descendants(System.Windows.DependencyObject root)
    {
        yield return root;
        foreach (var child in System.Windows.LogicalTreeHelper.GetChildren(root).OfType<System.Windows.DependencyObject>())
            foreach (var descendant in Descendants(child)) yield return descendant;
    }
}
