using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Interface.ReadViews;
[Collection("WPF")]
public class ReadViewsTests(StaExecutionScenarios sta)
{
    [Theory] [InlineData(0)] [InlineData(5)] [InlineData(-1)] public Task ReadOutcomeRestoresUi(int exit) => sta.RunAsync(() => ReadOperationScenarios.Outcome(exit));
    [Fact] public Task RepeatedReadRequestsCancellation() => sta.RunAsync(ReadOperationScenarios.Cancel);
    [Fact] public Task FormatChangesUpdateSelectorsAndReadRequest()=>sta.Run(ReadFormatScenarios.Selection);
    [Theory]
    [InlineData(null)]
    [InlineData("virtual-new")]
    public Task FolderDialogResponseChangesOnlyAcceptedDestination(string? response)=>sta.Run(()=>ReadDestinationScenarios.Browse(response));
    [Fact] public void DestinationAndSequenceAreBuiltFromInput()=>ReadDestinationScenarios.Target();
    [Fact] public void ReadOptionsExcludeIncompatibleChoices()=>ReadFormatScenarios.Options();
    [Theory] [InlineData(-1)] [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public Task ExistingDestinationDecision(int choice) => sta.RunAsync(() => ReadDestinationScenarios.Conflict(choice));
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task CaptureCompletionSummary(bool failure) => sta.RunAsync(() => ReadOperationScenarios.CaptureSummary(failure));
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task DestinationPreviewMatchesCommand(bool alphabetic) => sta.RunAsync(() => ReadDestinationScenarios.Preview(alphabetic));
    [Fact] public Task MissingDestinationNameBlocksRead() => sta.RunAsync(ReadDestinationScenarios.MissingName);
}
