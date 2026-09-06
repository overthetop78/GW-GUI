using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Interface.VisualizerViews;
[Collection("WPF")]
public sealed class VisualizerViewsTests(StaExecutionScenarios sta)
{
    [Fact] public Task MultipleAndUnknownInterpretationsRefreshVisualizerClassification() => sta.RunAsync(VisualizerDocumentScenarios.Classification);
    [Fact] public Task ImageAndRendererBoundary() => sta.Run(VisualizerDocumentScenarios.Image);
    [Fact] public Task GeometryAndTrackHitTesting() => sta.Run(ViewportGeometryScenarios.Geometry);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public Task FaceLayoutFollowsCapture(int mask) => sta.RunAsync(() => VisualizerDocumentScenarios.Heads(mask));
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task OldLoadCannotReplaceCurrentDocument(bool failure) => sta.RunAsync(() => VisualizerDocumentScenarios.LateCompletion(failure));
    [Fact] public Task FailedAndCancelledLoadCanRetry() => sta.RunAsync(VisualizerDocumentScenarios.CancelAndRetry);
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task ReplacedAnalysisCannotRestartVisualization(bool combined) => sta.RunAsync(() => VisualizerDocumentScenarios.ReplacedAnalysis(combined));
    [Fact] public Task InspectorDocumentClears() => sta.Run(InspectorSelectionScenarios.Clear);
    [Theory] [InlineData(0)] [InlineData(1)]
    public Task InspectorTrackRevolutionsAndSectors(int head) => sta.RunAsync(() => InspectorSelectionScenarios.Selection(head));
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task LinkedZoomAndReset(bool linked) => sta.Run(() => ViewportGeometryScenarios.Zoom(linked));
}
