namespace GWGUI.Tests.Hardware.HardwareSelection;
[Collection("WPF")]
public class HardwareSelectionTests(GWGUI.Tests.Application.TestInfrastructure.StaExecutionScenarios sta)
{
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] public Task RemovedOrDisconnectedSelectionCannotReuseOldAvailability(int change)=>sta.Run(()=>DriveRoutingScenarios.Selection(change));
    [Theory] [InlineData(false)] [InlineData(true)] public Task DuplicateDiscoveryAndNetworkWarningPreserveUniqueControllers(bool warning) => DeviceDiscoveryScenarios.DuplicatesAndFailedInformation(warning);
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task StartupScanWaitsAndCancellationPreservesSettings(bool cancel) => HardwareRefreshScenarios.Refresh(cancel);
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task MissingToolMarksOnlyConfiguredHardwareUnavailable(bool configured) => HardwareRefreshScenarios.Unavailable(configured);
    [Fact] public void CandidateMatchingUsesUsbIdentityAndKnownAliases()=>DeviceDiscoveryScenarios.Identity();
    [Theory] [InlineData(false)] [InlineData(true)] public Task ScanPreservesConfiguredIdentity(bool present) => DeviceDiscoveryScenarios.Scan(present);
    [Fact] public void DriveArgumentsDisambiguateControllersAndUnits()=>DriveRoutingScenarios.Route();
    [Fact] public void AutomaticSelectionAffectsOnlyRequestedController()=>DriveRoutingScenarios.Assign();
}
