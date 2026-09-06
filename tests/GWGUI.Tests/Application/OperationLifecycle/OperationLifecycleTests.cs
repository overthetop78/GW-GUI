namespace GWGUI.Tests.Application.OperationLifecycle;
public class OperationLifecycleTests
{
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task CancellationAtCompletionPreservesActualOperationOutcome(bool observesCancellation) => CancellationScenarios.Completion(observesCancellation);
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task BatchPreservesOrderAndStopsOnCancellation(bool cancel)=>BatchExecutionScenarios.Run(cancel);
    [Fact] public Task ConcurrentRequestIsRefusedAndNextRunSucceeds()=>OperationStateScenarios.Concurrency();
    [Fact] public Task ErrorReleasesCoordinatorAndPreservesException()=>OperationStateScenarios.Error();
    [Fact] public Task CancellationReachesCurrentOperationAndDoesNotPoisonNext()=>CancellationScenarios.Cancel();
}
