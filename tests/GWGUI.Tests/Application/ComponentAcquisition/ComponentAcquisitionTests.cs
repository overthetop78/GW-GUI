namespace GWGUI.Tests.Application.ComponentAcquisition;
public sealed class ComponentAcquisitionTests
{
    [Fact] public Task CancellationDuringDownloadRemovesStagingWithoutPromotion() => ComponentAcquisitionScenarios.CancelDuringDownload();
    [Fact] public Task ReleaseMetadataSelectsWindowsAsset() => ComponentAcquisitionScenarios.Metadata();
    [Fact] public Task ArchiveIsInstalledInVirtualTree() => ComponentAcquisitionScenarios.Install();
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public Task InvalidDownloadCleansVirtualStaging(int variant) => ComponentAcquisitionScenarios.Invalid(variant);
    [Fact] public void SelectionAndRollbackUseExistingVirtualFiles() => ComponentAcquisitionScenarios.Selection();
    [Fact] public Task HttpFailureAndCancellationCleanStaging() => ComponentAcquisitionScenarios.TransportFailure();
}
