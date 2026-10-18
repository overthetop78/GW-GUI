using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class CoreInstallationFlowTests
{
    [Fact]
    public async Task PublishesManifestAndActivationBeforeReportingCompletion()
    {
        var events = new List<string>();
        var manifest = new object();
        await EmulationCoreReleaseFunctions.InstallLibraryAsync(
            () => { events.Add("download"); return Task.FromResult(123L); },
            () => events.Add("extract"),
            size => { Assert.Equal(123L, size); events.Add("validate"); return manifest; },
            () => events.Add("replace"),
            value => { Assert.Same(manifest, value); events.Add("manifest"); return Task.CompletedTask; },
            () => { events.Add("activate"); return Task.CompletedTask; },
            size => { Assert.Equal(123L, size); events.Add("complete"); });
        Assert.Equal(new[] { "download", "extract", "validate", "replace", "manifest", "activate", "complete" }, events);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public async Task FailureStopsPublicationAndPropagatesTheOriginalError(int failureStage)
    {
        var error = new InvalidOperationException("installation failed");
        var visited = new List<int>();
        void Visit(int stage)
        {
            visited.Add(stage);
            if (stage == failureStage) throw error;
        }
        var observed = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            EmulationCoreReleaseFunctions.InstallLibraryAsync(
                () => { Visit(0); return Task.FromResult(123L); },
                () => Visit(1),
                size => { Visit(2); return new object(); },
                () => Visit(3),
                value => { Visit(4); return Task.CompletedTask; },
                () => { Visit(5); return Task.CompletedTask; },
                size => Visit(6)));
        Assert.Same(error, observed);
        Assert.Equal(Enumerable.Range(0, failureStage + 1), visited);
    }
}
