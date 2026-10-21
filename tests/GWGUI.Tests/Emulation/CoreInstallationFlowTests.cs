using GWGUI.Emulation;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class CoreInstallationFlowTests
{
    [Fact]
    public async Task PublishesManifestAndActivationBeforeReportingCompletion()
    {
        var events = new List<string>();
        var manifest = new object();
        await EmulationSdk.CoreReleases.InstallLibraryAsync(
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
            EmulationSdk.CoreReleases.InstallLibraryAsync(
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

    [Fact]
    public async Task WaitsForPreparationAfterValidationAndBeforeReplacement()
    {
        var events = new List<string>();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var installation = EmulationSdk.CoreReleases.InstallLibraryAsync(
            () => { events.Add("download"); return Task.FromResult(123L); },
            () => events.Add("extract"),
            size => { events.Add("validate"); return "sha256"; },
            () => events.Add("replace"),
            hash => { Assert.Equal("sha256", hash); events.Add("manifest"); return Task.CompletedTask; },
            () => { events.Add("activate"); return Task.CompletedTask; },
            size => events.Add("complete"),
            prepareReplacement: async () =>
            {
                events.Add("prepare");
                entered.SetResult();
                await release.Task;
                events.Add("prepared");
            });
        try
        {
            await entered.Task;
            Assert.False(installation.IsCompleted);
            Assert.Equal(new[] { "download", "extract", "validate", "prepare" }, events);
        }
        finally
        {
            release.TrySetResult();
            await installation;
        }
        Assert.Equal(new[] { "download", "extract", "validate", "prepare", "prepared", "replace", "manifest", "activate", "complete" }, events);
    }

    [Fact]
    public async Task PreparationFailureStopsReplacementAndPropagatesTheOriginalError()
    {
        var error = new InvalidOperationException("preparation failed");
        var events = new List<string>();
        var observed = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            EmulationSdk.CoreReleases.InstallLibraryAsync(
                () => { events.Add("download"); return Task.FromResult(123L); },
                () => events.Add("extract"),
                size => { events.Add("validate"); return "sha256"; },
                () => events.Add("replace"),
                hash => { events.Add("manifest"); return Task.CompletedTask; },
                () => { events.Add("activate"); return Task.CompletedTask; },
                size => events.Add("complete"),
                prepareReplacement: () => { events.Add("prepare"); return Task.FromException(error); }));
        Assert.Same(error, observed);
        Assert.Equal(new[] { "download", "extract", "validate", "prepare" }, events);
    }
}
