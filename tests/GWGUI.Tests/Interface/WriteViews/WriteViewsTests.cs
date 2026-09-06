namespace GWGUI.Tests.Interface.WriteViews;
[Collection("WPF")]
public class WriteViewsTests(GWGUI.Tests.Application.TestInfrastructure.StaExecutionScenarios sta)
{
    [Fact] public void OptionsPreserveValuesAndSeparateVerification()=>WriteConfirmationScenarios.Options();
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task TrackAndAdvancedOptionsReachWriteCommand(bool verify) => sta.RunAsync(() => WriteConfirmationScenarios.Request(verify));
    [Theory] [InlineData(0)] [InlineData(5)] [InlineData(-1)]
    public Task OutcomeRestoresControls(int exit) => sta.RunAsync(() => WriteOperationScenarios.Outcome(exit));
    [Fact] public Task RepeatedExecutionCancelsCurrentWrite() => sta.RunAsync(WriteOperationScenarios.Cancel);
    [Fact] public Task RefusalPreventsWriting() => sta.RunAsync(WriteConfirmationScenarios.Refusal);
    [Fact] public Task SourceChangesReplaceDetectionAndCancellationPreservesIt() => sta.RunAsync(WriteSourceScenarios.Selection);
}
