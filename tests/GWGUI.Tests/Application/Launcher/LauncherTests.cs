namespace GWGUI.Tests.Application.Launcher;
public sealed class LauncherTests
{
    [Fact] public void AssemblyResolutionUsesVirtualTree() => LauncherDecisionScenarios.Resolve();
    [Fact] public void EntryPointReceivesArgumentsAndReturnsExitCode() => LauncherDecisionScenarios.Run();
    [Fact] public void MissingAndFailedEntryPointPropagate() => LauncherDecisionScenarios.Failure();
}
