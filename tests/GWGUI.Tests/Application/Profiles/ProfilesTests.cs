using GWGUI.Domain.Profiles;
namespace GWGUI.Tests.Application.Profiles;
public class ProfilesTests
{
    [Fact] public void ProfileValuesExcludeCurrentSourcesNamesAndCounters() => ProfileStateScenarios.SessionFields();
    [Fact] public void CapturedProfilesExcludeDefaultsAndOwnTheirValues()=>ProfileStateScenarios.Capture();
    [Theory]
    [InlineData(OperationKind.Read)]
    [InlineData(OperationKind.Write)]
    [InlineData(OperationKind.Convert)]
    public void ProfilesStayWithinTheirOperation(OperationKind operation) => ProfileStateScenarios.Lifecycle(operation);
    [Fact] public void SystemProfileCannotBeChanged() => ProfileStateScenarios.SystemProtection();
    [Fact] public void ReplacementKeepsIdentityAndRejectsConflicts() => ProfileStateScenarios.Conflicts();
}
