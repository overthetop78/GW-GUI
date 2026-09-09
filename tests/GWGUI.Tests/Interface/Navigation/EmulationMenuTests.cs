using GWGUI.Tests.Application.TestInfrastructure;

namespace GWGUI.Tests.Interface.Navigation;

[Collection("WPF")]
public sealed class EmulationMenuTests(StaExecutionScenarios sta)
{
    [Fact]
    public Task NoInstalledModuleCreatesNoDynamicMenuEntry() =>
        sta.Run(EmulationMenuScenarios.Empty);

    [Fact]
    public Task InstalledModulesCreateLocalizedEntriesAndDispatchTheirOwnIdentifiers() =>
        sta.Run(EmulationMenuScenarios.DynamicEntries);

    [Fact]
    public Task ArbitraryModuleIdentifiersOpenTheSameGenericWindowType() =>
        sta.Run(EmulationMenuScenarios.ArbitraryModulesUseTheSameWindowType);
}
