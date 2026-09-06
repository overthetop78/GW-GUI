using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Interface.ExplorerViews;
[Collection("WPF")]
public sealed class ExplorerViewsTests(StaExecutionScenarios sta)
{
    [Fact] public Task DocumentReplacement() => sta.Run(ExplorerDocumentScenarios.Replace);
    [Fact] public Task AlternativeVolumesAndUnknownImage() => sta.Run(ExplorerDocumentScenarios.Interpretations);
    [Fact] public Task FolderAndFileSelection() => sta.Run(ExplorerTreeScenarios.Select);
    [Fact] public Task ClearAndLoading() => sta.Run(ExplorerFailureScenarios.Clear);
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task PendingFailureAndRetry(bool staleFailure) => sta.RunAsync(() => ExplorerFailureScenarios.LoadingAndRetry(staleFailure));
}
