namespace GWGUI.Tests.Application.ExternalProcesses;
public sealed class ExternalProcessesTests
{
    [Fact] public Task LogFailurePreservesExecutionResult() => ProcessBoundaryScenarios.LoggingFailure();
    [Fact] public Task FactoryFailureAllowsRetry() => ProcessBoundaryScenarios.FactoryFailure();
    [Theory] [InlineData(0)] [InlineData(7)] public Task OutputAndExitCode(int code) => ProcessBoundaryScenarios.Completion(code);
    [Fact] public Task StuckProcessIsStoppedAndDisposed() => ProcessBoundaryScenarios.Cancellation();
    [Fact] public Task StartFailureReleasesRunner() => ProcessBoundaryScenarios.StartFailure();
}
