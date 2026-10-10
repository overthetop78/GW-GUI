using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Interface.SettingsViews;
[Collection("WPF")]
public sealed class SettingsViewsTests(StaExecutionScenarios sta)
{
    [Fact]
    public Task ColecoSelectorRetainsEachSelectedEmulator() => sta.RunAsync(async () =>
    {
        using var http = new System.Net.Http.HttpClient();
        var directory = System.IO.Path.GetTempPath();
        var module = new GWGUI.Emulation.Coleco.Modules.ColecoEmulationModule(directory, directory, http, directory);
        GWGUI.Emulation.Interfaces.IEmulationConfiguration configuration = module.CreateConfiguration("ColecoVision");
        var controller = new GWGUI.App.Controllers.Emulation.Options.EmulationEmulatorManagementController(
            module, () => configuration, value => configuration = value, () => false, module);
        try
        {
            var panel = Assert.IsType<GWGUI.App.Views.Controls.Emulation.Options.EmulationCoreManagementPanel>(
                controller.CreateView(new System.Windows.Controls.Button()));
            await controller.RefreshAsync();
            var choices = panel.Emulators.Items.Cast<GWGUI.Emulation.Contracts.EmulationEmulatorInstallation>().ToArray();
            Assert.Equal(3, choices.Length);
            foreach (var choice in choices)
            {
                panel.Emulators.SelectedValue = choice.EmulatorId;
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ContextIdle);
                Assert.Equal(choice.EmulatorId, panel.Emulators.SelectedValue);
                Assert.Equal(choice.EmulatorId, (await module.GetEmulatorInstallationAsync(configuration)).EmulatorId);
            }
        }
        finally { await controller.DisposeAsync(); }
    });
    [Fact] public Task FailedSaveReleasesGateAndPreservesEditableValues() => sta.RunAsync(SettingsFailureScenarios.Retry);
    [Fact] public Task AutomaticSavePresentsFailureAndAllowsRetry() => sta.RunAsync(SettingsFailureScenarios.AutomaticSave);
    [Fact] public Task EmulationAutomaticSavePresentsFailureAndAllowsRetry() => sta.RunAsync(SettingsFailureScenarios.EmulationAutomaticSave);
    [Fact] public Task ModuleWindowPresentsSaveFailureWithoutLosingTheEditor() =>
        sta.RunAsync(SettingsFailureScenarios.ModuleWindowSaveFailure);
    [Fact] public Task ModuleMachinesUseVerticalNavigationAndPreserveExistingTabs() =>
        sta.RunAsync(EmulationModuleSettingsNavigationScenarios.VerticalMachinesPreserveConfigurationStateAndTabs);
    [Fact] public Task UnsavedModuleMachineChoosesExactlyOneEmulator() =>
        sta.RunAsync(EmulationModuleSettingsNavigationScenarios.UnsavedMachineChoosesExactlyOneEmulator);
    [Fact] public Task EmptyEmulatorCatalogKeepsOptionsOpenAndBlocksCreation() =>
        sta.RunAsync(EmulationModuleSettingsNavigationScenarios.EmptyEmulatorCatalogKeepsOptionsOpenAndBlocksCreation);
    [Fact] public Task EmulatorControllerDisposalCancelsAndDetaches() =>
        sta.RunAsync(EmulationModuleSettingsNavigationScenarios.EmulatorControllerDisposalCancelsAndDetaches);
    [Fact] public Task EmulatorDescriptionUsesModuleLocalization() =>
        sta.RunAsync(EmulationModuleSettingsNavigationScenarios.EmulatorDescriptionUsesModuleLocalization);
    [Fact] public Task EmulatorVersionsAreSelectedAndInstalledExplicitly() =>
        sta.RunAsync(EmulationModuleSettingsNavigationScenarios.EmulatorVersionsAreSelectedAndInstalledExplicitly);
    [Fact] public Task VideoFieldsKeepTheirHelpContent() =>
        sta.Run(EmulationModuleSettingsNavigationScenarios.VideoFieldsKeepTheirHelpContent);
    [Fact] public Task NativeVideoOptionsPreserveTheProcessingPosition() =>
        sta.Run(EmulationModuleSettingsNavigationScenarios.NativeVideoOptionsPreserveTheProcessingPosition);
    [Fact] public Task ModuleWindowReopensWithANewVisualTree() =>
        sta.RunAsync(EmulationModuleSettingsNavigationScenarios.ModuleWindowReopensWithANewVisualTree);
    [Fact] public Task AmigaModuleWindowBuildsItsVisualTree() =>
        sta.RunAsync(EmulationModuleSettingsNavigationScenarios.AmigaModuleWindowBuildsItsVisualTree);
    [Fact] public Task NecModuleWindowOpensAndClosesWithSavedMachine() =>
        sta.RunAsync(EmulationModuleSettingsNavigationScenarios.NecModuleWindowOpensAndClosesWithSavedMachine);
    [Fact] public Task EachModuleUsesTheGenericSettingsWindow() =>
        sta.Run(EmulationModuleSettingsNavigationScenarios.ModuleWindowUsesTheGenericSectionAndDynamicTitle);
    [Fact] public Task EngineChangesPersistAndApply() => sta.Run(SettingsEditingScenarios.Engines);
    [Fact] public Task GeneralChangesUseSelectedValues() => sta.Run(SettingsEditingScenarios.General);
    [Fact] public Task InitializationAndUnselectedValues() => sta.Run(SettingsValidationScenarios.Initialization);
    [Theory] [InlineData(null)] [InlineData("  virtual-new  ")]
    public Task FolderDialogAppliesOnlyAcceptedSelection(string? response) => sta.Run(() => SettingsEditingScenarios.Browse(response));
    [Fact] public Task TagsRetainPatternWhenDisabledAndPresetsApply() => sta.Run(SettingsValidationScenarios.Tags);
    [Theory] [InlineData("0", 0)] [InlineData("1", 1)] [InlineData("2147483647", 2147483647)]
    [InlineData("-1", 12)] [InlineData("2147483648", 12)] [InlineData("invalid", 12)]
    public Task LogSizeValidationPrecedesPersistence(string text, int expected) => sta.RunAsync(() => SettingsValidationScenarios.LogSize(text, expected));
}
