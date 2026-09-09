using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Interface.SettingsViews;
[Collection("WPF")]
public sealed class SettingsViewsTests(StaExecutionScenarios sta)
{
    [Fact] public Task FailedSaveReleasesGateAndPreservesEditableValues() => sta.RunAsync(SettingsFailureScenarios.Retry);
    [Fact] public Task AutomaticSavePresentsFailureAndAllowsRetry() => sta.RunAsync(SettingsFailureScenarios.AutomaticSave);
    [Fact] public Task EmulationAutomaticSavePresentsFailureAndAllowsRetry() => sta.RunAsync(SettingsFailureScenarios.EmulationAutomaticSave);
    [Fact] public Task ModuleWindowPresentsSaveFailureWithoutLosingTheEditor() =>
        sta.RunAsync(SettingsFailureScenarios.ModuleWindowSaveFailure);
    [Fact] public Task ModuleMachinesUseVerticalNavigationAndPreserveExistingTabs() =>
        sta.RunAsync(EmulationModuleSettingsNavigationScenarios.VerticalMachinesPreserveConfigurationStateAndTabs);
    [Fact] public Task ModuleGeneralTabShowsOnlyTheCurrentEmulator() =>
        sta.RunAsync(EmulationModuleSettingsNavigationScenarios.CurrentEmulatorIsTheOnlyChoice);
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
