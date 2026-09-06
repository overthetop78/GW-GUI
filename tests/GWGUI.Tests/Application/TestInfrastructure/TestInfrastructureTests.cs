using GWGUI.Domain.Settings;
namespace GWGUI.Tests.Application.TestInfrastructure;

[Collection("WPF")]
public class TestInfrastructureTests(StaExecutionScenarios sta)
{
    [Fact]
    public async Task ScenarioFailureIsPropagatedAndDispatcherRemainsUsable()
    {
        var expected=new InvalidOperationException("synthetic failure");
        Assert.Same(expected,await Assert.ThrowsAsync<InvalidOperationException>(()=>sta.Run(()=>throw expected)));
        await sta.Run(()=>Assert.Equal(ApartmentState.STA,Thread.CurrentThread.GetApartmentState()));
    }

    [Fact]
    public async Task UnexpectedBoundaryFailsWithoutRealAccess()
    {
        var dependency=ControlledDependencies.Reject<ISettingsStore>();
        var error=await Assert.ThrowsAsync<InvalidOperationException>(()=>dependency.LoadAsync());
        Assert.Contains(nameof(ISettingsStore.LoadAsync),error.Message);
    }
}
