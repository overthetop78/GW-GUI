namespace GWGUI.Tests.Interface.ConversionViews;
[Collection("WPF")]
public class ConversionViewsTests(GWGUI.Tests.Application.TestInfrastructure.StaExecutionScenarios sta)
{
    [Fact] public void SelectionAndProfileUseIsolatedCollections()=>ConversionSelectionScenarios.Selection();
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task DestinationTagsAndOptionsReachConversion(bool tags) => sta.RunAsync(() => ConversionSelectionScenarios.Parameters(tags));
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task PartialBatchReportsEachDestination(bool firstFails) => sta.RunAsync(() => ConversionOperationScenarios.PartialBatch(firstFails));
    [Fact] public Task SourceChangesRemoveIncompatibleChoices() => sta.RunAsync(ConversionSelectionScenarios.SourceChanges);
    [Theory] [InlineData(0)] [InlineData(5)] [InlineData(-1)] public Task OutcomeRestoresControls(int exit) => sta.RunAsync(() => ConversionOperationScenarios.Outcome(exit));
    [Fact] public Task RepeatedExecutionCancelsCurrentBatch() => sta.RunAsync(ConversionOperationScenarios.Cancel);
    [Theory] [InlineData(-1)] [InlineData(0)] [InlineData(1)] [InlineData(2)] public Task ConflictDecisionControlsOutputs(int choice) => sta.RunAsync(() => ConversionConflictScenarios.Decision(choice));
}
