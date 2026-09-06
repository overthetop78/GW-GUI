namespace GWGUI.Tests.Application.SettingsStorage;
public class SettingsStorageTests
{
    [Theory] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void LegacyFormatTagsAndEngineStructuresAreMigrated(int version) => MigrationAndRecoveryScenarios.LegacyStructures(version);
    [Fact] public Task SaveProducesExpectedJsonAndKeepsBackup()=>SettingsRoundTripScenarios.Save();
    [Fact] public Task LoadUsesInMemoryJsonAndMissingDefaults()=>SettingsRoundTripScenarios.Load();
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void OldSchemasMigrate(int version)=>MigrationAndRecoveryScenarios.Migrate(version);
    [Fact] public Task InvalidJsonIsPreservedAndBackupRecovered()=>MigrationAndRecoveryScenarios.Recover();
    [Fact] public Task FailedReplacementPreservesCurrentSettings()=>MigrationAndRecoveryScenarios.FailedSave();
    [Fact] public Task FutureSchemaDoesNotOverwriteOriginal()=>MigrationAndRecoveryScenarios.FutureSchema();
    [Fact] public Task PathsStayWithinInjectedStore()=>StorageLocationScenarios.Paths();
}
