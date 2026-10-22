using GWGUI.Emulation;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation.EmulationContracts;

public sealed class CallbackResourceDisposalTests
{
    private static readonly string[] DisposalOrder =
        ["options", "video", "files", "assets", "save", "content", "system"];

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void EveryOwnedResourceIsReleasedInOrderAfterAnySingleFailure(int failingStage)
    {
        var calls = new List<string>();
        var failure = new InvalidOperationException("Injected disposal failure");
        var resources = DisposalOrder.Select((name, index) =>
            new Resource(name, calls, index == failingStage ? failure : null)).ToArray();

        var actual = Record.Exception(() => EmulationSdk.MachineExecution.DisposeCallbackResources(
            resources[6], resources[5], resources[4], resources[3], resources[2], resources[1], resources[0]));

        Assert.Equal(DisposalOrder, calls);
        if (failingStage < 0) Assert.Null(actual);
        else Assert.Same(failure, actual);
    }

    [Fact]
    public void OuterUsingFailureReplacesEarlierFailuresAfterAllResourcesAreAttempted()
    {
        var calls = new List<string>();
        var failures = DisposalOrder.Select(name => new InvalidOperationException(name)).ToArray();
        var resources = DisposalOrder.Select((name, index) => new Resource(name, calls, failures[index])).ToArray();

        var actual = Assert.Throws<InvalidOperationException>(() => EmulationSdk.MachineExecution.DisposeCallbackResources(
            resources[6], resources[5], resources[4], resources[3], resources[2], resources[1], resources[0]));

        Assert.Equal(DisposalOrder, calls);
        Assert.Same(failures[6], actual);
    }

    [Fact]
    public void MissingUsingResourcesStillReleaseTheOptionHost()
    {
        var calls = new List<string>();
        var options = new Resource("options", calls, null);

        EmulationSdk.MachineExecution.DisposeCallbackResources(null, null, null, null, null, null, options);

        Assert.Equal(new[] { "options" }, calls);
    }

    private sealed class Resource(string name, List<string> calls, Exception? failure) : IDisposable
    {
        public void Dispose()
        {
            calls.Add(name);
            if (failure is not null) throw failure;
        }
    }
}
