using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Interface.VisualizerViews;
[Collection("WPF")]
public sealed class VisualizerViewsTests(StaExecutionScenarios sta)
{
    [Fact] public Task ReplacedCancellationTokenRemainsReadable() => sta.Run(VisualizerDocumentScenarios.ReplacedCancellationTokenRemainsReadable);
    [Fact] public Task MultipleAndUnknownInterpretationsRefreshVisualizerClassification() => sta.RunAsync(VisualizerDocumentScenarios.Classification);
    [Fact] public Task UndetectedManualFormatRunsAnExplicitExploration() => sta.RunAsync(VisualizerDocumentScenarios.UndetectedManualFormatRunsAnExplicitExploration);
    [Fact] public Task NonScpFormatSelectionAttemptsTheChosenFormatAndReloadsTheInitialFormat() => sta.RunAsync(VisualizerDocumentScenarios.NonScpFormatSelectionAttemptsTheChosenFormatAndReloadsTheInitialFormat);
    [Fact] public Task ScpMultiFormatSelectorKeepsAmigaIbmAndAtari() => sta.Run(VisualizerDocumentScenarios.ScpMultiFormatSelector);
    [Fact] public Task ImageAndRendererBoundary() => sta.Run(VisualizerDocumentScenarios.Image);
    [Fact] public Task ProgressiveTrackPresentationUsesMinimumInterval() => sta.RunAsync(VisualizerDocumentScenarios.ProgressiveTrackPresentationUsesMinimumInterval);
    [Fact] public Task SectorTrackPresentationDeductsAnalysisTime() => sta.Run(VisualizerDocumentScenarios.SectorTrackPresentationDeductsAnalysisTime);
    [Fact] public Task SelectingScpSectorFormatRevealsEveryTrack() => sta.RunAsync(VisualizerDocumentScenarios.SelectingScpSectorFormatRevealsEveryTrack);
    [Fact] public Task ExplorerAndVisualizerSynchronizeFormatsForTheSameImage() => sta.RunAsync(VisualizerDocumentScenarios.ExplorerAndVisualizerSynchronizeFormatsForTheSameImage);
    [Fact] public Task BothVisualizerOpenButtonsUseTheSameAction() => sta.Run(VisualizerDocumentScenarios.BothVisualizerOpenButtonsUseTheSameAction);
    [Fact] public Task SharedScpLoadClearsThenBuildsFluxDuringRecognition() => sta.RunAsync(VisualizerDocumentScenarios.SharedScpLoadClearsThenBuildsFluxDuringRecognition);
    [Fact] public Task SharedOpeningReadsMediaOnce() => sta.RunAsync(VisualizerDocumentScenarios.SharedOpeningReadsMediaOnce);
    [Fact] public Task SharedNonScpProgressStaysInLoadingPanelsUntilTrackProgress() => sta.Run(VisualizerDocumentScenarios.SharedNonScpProgressStaysInLoadingPanelsUntilTrackProgress);
    [Theory] [InlineData(40, 1)] [InlineData(80, 2)]
    public Task SectorStatusProgressUsesMediaGeometry(int cylinders, int heads) => sta.RunAsync(() => VisualizerDocumentScenarios.SectorStatusProgressUsesMediaGeometry(cylinders, heads));
    [Fact] public Task ReplacedSectorPresentationDoesNotEscapeCancellation() => sta.RunAsync(VisualizerDocumentScenarios.ReplacedSectorPresentationDoesNotEscapeCancellation);
    [Fact] public Task VisualizerAndExplorerKeepIndependentImageFolders() => sta.Run(VisualizerDocumentScenarios.ImageFoldersRemainIndependent);
    [Fact] public Task GeometryAndTrackHitTesting() => sta.Run(ViewportGeometryScenarios.Geometry);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public Task FaceLayoutFollowsCapture(int mask) => sta.RunAsync(() => VisualizerDocumentScenarios.Heads(mask));
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task OldLoadCannotReplaceCurrentDocument(bool failure) => sta.RunAsync(() => VisualizerDocumentScenarios.LateCompletion(failure));
    [Fact] public Task FailedAndCancelledLoadCanRetry() => sta.RunAsync(VisualizerDocumentScenarios.CancelAndRetry);
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task ReplacedAnalysisCannotRestartVisualization(bool combined) => sta.RunAsync(() => VisualizerDocumentScenarios.ReplacedAnalysis(combined));
    [Fact] public Task SharedLoadsAreSerializedAndIntermediateRequestsAreDiscarded() => sta.RunAsync(VisualizerDocumentScenarios.SharedLoadsAreSerializedAndCoalesced);
    [Fact] public Task CassettePresentationUsesRecognizedMediaKindRatherThanExtension() => sta.RunAsync(VisualizerDocumentScenarios.CassettePresentationUsesRecognizedMediaKindRatherThanExtension);
    [Fact] public Task InspectorDocumentClears() => sta.Run(InspectorSelectionScenarios.Clear);
    [Theory] [InlineData(0)] [InlineData(1)]
    public Task InspectorTrackRevolutionsAndSectors(int head) => sta.RunAsync(() => InspectorSelectionScenarios.Selection(head));
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task LinkedZoomAndReset(bool linked) => sta.Run(() => ViewportGeometryScenarios.Zoom(linked));
}
