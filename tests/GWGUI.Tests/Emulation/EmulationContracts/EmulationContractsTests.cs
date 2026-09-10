namespace GWGUI.Tests.Emulation.EmulationContracts;
public class EmulationContractsTests
{
    [Fact] public Task CancelledSessionDoesNotAffectOtherSessionAndCanRetry() => SessionLifecycleScenarios.CancellationAndIsolation();
    [Theory]
    [InlineData(false,false,false)] [InlineData(false,false,true)] [InlineData(true,false,false)] [InlineData(true,false,true)]
    [InlineData(true,true,false)] [InlineData(true,true,true)]
    public Task MediaChangesRecreateOnlyWhenRequired(bool powered,bool recreate,bool eject) => SessionLifecycleScenarios.MediaRecreation(powered,recreate,eject);
    [Theory] [InlineData(false,false)] [InlineData(true,false)] [InlineData(true,true)]
    public Task QuickStatesRouteCurrentSessionAndPreserveStateOnFailure(bool supported,bool fail) => MediaAndSavedStateScenarios.States(supported,fail);
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task DeferredProfilesCoalesceAndFlushLastValues(bool flush) => DraftAndSaveScenarios.PendingProfiles(flush);
    [Fact] public Task FailedProfileSavePreservesPreviousAndRetriesPending() => DraftAndSaveScenarios.FailedProfileSave();
    [Fact] public void DraftsAreIsolatedByModuleAndMachine() => DraftAndSaveScenarios.Drafts();
    [Fact] public Task SessionPowerPauseAndRecreation() => SessionLifecycleScenarios.Lifecycle();
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task UpdatedConfigurationIsUsedOnNextStart(bool powered) =>
        SessionLifecycleScenarios.UpdatedConfigurationIsUsedOnNextStart(powered);
    [Fact] public Task FailedStartIsDisposedAndCanRetry() => SessionLifecycleScenarios.Failure();
    [Fact] public Task MediaFailureKeepsMountedSelection() => SessionLifecycleScenarios.MediaFailure();
    [Fact] public void MediaReplacementAndEjectionTargetOnlySelectedSlot()=>MediaAndSavedStateScenarios.Media();
    [Fact] public void InvalidMediaConfigurationsAreRejected()=>MediaAndSavedStateScenarios.Invalid();
    [Fact] public void Atari800OptionsDistinguishHotChangesFromRestart() =>
        RuntimeOptionScenarios.Atari800HotAndRestartOptions();
    [Fact] public void RuntimePropagationUsesOnlyRecognizedHotOptions() =>
        RuntimeOptionScenarios.OnlyRecognizedHotOptionsArePropagated();
    [Fact] public void Atari800CoreMachinesExposeOneSystemRomSlot() =>
        RuntimeOptionScenarios.Atari800CoreMachinesExposeOneSystemRomSlot();
    [Fact] public void AtariExternalFirmwareModelsExposeOnlyUsableRomTabs() =>
        RuntimeOptionScenarios.AtariExternalFirmwareModelsExposeOnlyUsableRomTabs();
}
