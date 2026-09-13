using GWGUI.Tests.Application.TestInfrastructure;
using GWGUI.App.Enums.Explorer;
using GWGUI.App.Views.Controls.Common;
using System.Windows.Controls;
namespace GWGUI.Tests.Interface.ExplorerViews;
[Collection("WPF")]
public sealed class ExplorerViewsTests(StaExecutionScenarios sta)
{
    [Fact] public Task DocumentReplacement() => sta.Run(ExplorerDocumentScenarios.Replace);
    [Fact] public Task AlternativeVolumesAndUnknownImage() => sta.Run(ExplorerDocumentScenarios.Interpretations);
    [Fact] public Task ScpMultiFormatSelectorKeepsAmigaIbmAndAtari() => sta.Run(ExplorerDocumentScenarios.ScpMultiFormatSelector);
    [Fact] public Task AutomaticDetectionRestoresInitialMultiformatChoice() => sta.Run(ExplorerDocumentScenarios.AutomaticDetectionRestoresInitialMultiformatChoice);
    [Fact] public Task AtariEightBitUsesFiveAndQuarterFloppyIcon() => sta.Run(ExplorerDocumentScenarios.AtariEightBitUsesFiveAndQuarterFloppyIcon);
    [Fact] public Task FileTypeIconsLoadAtTheirNativeSize() => sta.Run(() =>
    {
        foreach (var category in Enum.GetValues<ExplorerIconCategory>())
        {
            var icon = new FileEntryIcon { Category = category, Width = 20, Height = 20 };
            var image = Assert.IsType<Image>(icon.FindName("NativeIcon"));
            Assert.NotNull(image.Source);
        }
    });
    [Fact] public Task ManualFormatSelectionRemainsAvailableWhileAnalysisRuns() => sta.Run(ExplorerDocumentScenarios.ManualFormatSelectionRemainsAvailableWhileAnalysisRuns);
    [Fact] public Task FolderAndFileSelection() => sta.Run(ExplorerTreeScenarios.Select);
    [Fact] public Task ClearAndLoading() => sta.Run(ExplorerFailureScenarios.Clear);
    [Fact] public Task ManualFormatFailureUsesInformationalDialog() => sta.Run(ExplorerFailureScenarios.ManualFormatFailureUsesInformationalDialog);
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task PendingFailureAndRetry(bool staleFailure) => sta.RunAsync(() => ExplorerFailureScenarios.LoadingAndRetry(staleFailure));
}
