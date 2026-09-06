namespace GWGUI.Tests.Media.FileMigration;
public class FileMigrationTests
{
    public static IEnumerable<object[]> Targets => GWGUI.MediaEngine.Migration.FileSystemMigrationTargetCatalog.All.Select(target=>new object[]{target.FormatId,target.Extension});
    [Theory] [MemberData(nameof(Targets))] public Task AllInternalMigrationTargetsPreserveFiles(string format,string extension)=>FileMigrationScenarios.CatalogTarget(format,extension);
    [Theory] [InlineData(0)] [InlineData(1)] public Task CapacityAndNameConflictsLeaveDestinationIntact(int failure)=>FileMigrationScenarios.Rejected(failure);
    [Theory] [InlineData(false)] [InlineData(true)] public Task FileTransfersAcrossDifferentFileSystems(bool apple)=>FileMigrationScenarios.Transfer(apple);
    [Theory] [InlineData(0)] [InlineData(1)] public Task CancelledOrFailedStoragePreservesSource(int failure)=>FileMigrationScenarios.Failure(failure);
    [Fact] public void PlanPreservesHierarchyAndOwnsContent()=>FileMigrationScenarios.Plan();
    [Fact] public void BlockingLossesCannotBeAcceptedAsMetadata()=>FileMigrationScenarios.Losses();
}
