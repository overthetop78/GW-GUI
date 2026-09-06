using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Interface.ProfileViews;
[Collection("WPF")]
public class ProfileViewsTests(StaExecutionScenarios sta)
{
    [Theory] [InlineData(null)] [InlineData("")] [InlineData("   ")] [InlineData("  named profile  ")]
    public Task NameDialogValidatesBeforeAccepting(string? value) => sta.Run(() => ProfileEditingScenarios.Name(value));
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public Task ChangingAndResettingProfileUsesSelectedIdentity(int operation) => sta.Run(() => ProfileSelectionScenarios.SwitchAndReset(operation));
    [Theory] [InlineData(0,0)] [InlineData(0,1)] [InlineData(0,2)] [InlineData(0,3)] [InlineData(1,0)] [InlineData(1,1)] [InlineData(1,2)] [InlineData(1,3)] [InlineData(2,0)] [InlineData(2,1)] [InlineData(2,2)] [InlineData(2,3)]
    public Task SaveConflictAndSelectionStayWithinOperation(int operation,int decision) => sta.Run(()=>ProfileEditingScenarios.Save(operation,decision));
    [Theory]
    [InlineData(null,0,0)]
    [InlineData("changed",1,0)]
    [InlineData("second",0,1)]
    public Task RenameUsesSimulatedDialogAndRejectsDuplicate(string? name,int saves,int warnings)=>sta.Run(()=>ProfileEditingScenarios.Rename(name,saves,warnings));
    [Theory][InlineData(false)][InlineData(true)]
    public Task DeletionRequiresConfirmation(bool accepted)=>sta.Run(()=>ProfileDeletionScenarios.Delete(accepted));
    [Fact] public Task SelectionAndApplyKeepOtherOperations()=>sta.Run(ProfileSelectionScenarios.Apply);
}
