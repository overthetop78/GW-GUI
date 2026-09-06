using GWGUI.App.Services.Emulation;
using GWGUI.Emulation.Interfaces;
using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Emulation.EmulationContracts;
internal static class DraftAndSaveScenarios
{
    public static async Task PendingProfiles(bool flush)
    {
        var files = new MemoryVideoProfileFiles(); var scheduled = new List<Action>();
        var store = new GWGUI.VideoPresentation.Services.VideoPresentationProfileStore("virtual-video",
            fileSystem:files,schedule:action => { scheduled.Add(action); return Task.CompletedTask; });
        var id = Guid.NewGuid(); var other = Guid.NewGuid();
        var first = store.Get("module",id);
        store.Set("module",id,first with { Renderer = GWGUI.VideoPresentation.Enums.EmulationVideoRenderer.OpenGL });
        await store.SaveAsync("module",id);
        store.Set("module",id,first with { Renderer = GWGUI.VideoPresentation.Enums.EmulationVideoRenderer.Wpf });
        await store.SaveAsync("module",id);
        store.Set("module",other,first with { Renderer = GWGUI.VideoPresentation.Enums.EmulationVideoRenderer.Vulkan });
        await store.SaveAsync("module",other);
        Assert.Empty(files.Writes); Assert.Equal(3,scheduled.Count);
        if(flush) store.FlushPending(); else foreach(var action in scheduled) action();
        Assert.Equal(2,files.Writes.Count);
        foreach(var action in scheduled) action(); store.FlushPending(); Assert.Equal(2,files.Writes.Count);
        var reloaded = new GWGUI.VideoPresentation.Services.VideoPresentationProfileStore("virtual-video",fileSystem:files);
        Assert.Equal(GWGUI.VideoPresentation.Enums.EmulationVideoRenderer.Wpf,reloaded.Get("module",id).Renderer);
        Assert.Equal(GWGUI.VideoPresentation.Enums.EmulationVideoRenderer.Vulkan,reloaded.Get("module",other).Renderer);
        Assert.Equal(GWGUI.VideoPresentation.Enums.EmulationVideoRenderer.Direct3D11,first.Renderer);
        store.Copy("module",id,Guid.Empty); Assert.Equal(GWGUI.VideoPresentation.Enums.EmulationVideoRenderer.Wpf,store.Get("module",Guid.Empty).Renderer);
        await store.SaveAsync("module",id); store.Delete("module",id); scheduled[^1](); store.FlushPending();
        Assert.Equal(2,files.Files.Count);
        Assert.Equal(GWGUI.VideoPresentation.Enums.EmulationVideoRenderer.Direct3D11,store.Get("module",id).Renderer);
    }
    public static async Task FailedProfileSave()
    {
        var files = new MemoryVideoProfileFiles();
        var store = new GWGUI.VideoPresentation.Services.VideoPresentationProfileStore("virtual-video",fileSystem:files,
            schedule: action => { try { action(); return Task.CompletedTask; } catch(Exception error) { return Task.FromException(error); } });
        var id = Guid.NewGuid(); store.Save("module",id); var previous = Assert.Single(files.Files).Value;
        store.Set("module",id,new(GWGUI.VideoPresentation.Enums.EmulationVideoRenderer.Wpf)); files.FailWrite = true;
        await Assert.ThrowsAsync<IOException>(() => store.SaveAsync("module",id));
        Assert.Equal(previous,Assert.Single(files.Files).Value); Assert.Single(files.Writes);
        files.FailWrite = false; store.FlushPending();
        Assert.Equal(2,files.Writes.Count); Assert.NotEqual(previous,Assert.Single(files.Files).Value);
        Assert.Equal(GWGUI.VideoPresentation.Enums.EmulationVideoRenderer.Wpf,
            new GWGUI.VideoPresentation.Services.VideoPresentationProfileStore("virtual-video",fileSystem:files).Get("module",id).Renderer);
    }
    private static IEmulationConfiguration Configuration(string id) => ControlledDependencies.Simulate<IEmulationConfiguration>((method, _) => method.Name == "get_MachineId" ? id : throw new InvalidOperationException(method.Name));
    public static void Drafts()
    {
        var first = Configuration("one"); var second = Configuration("two"); var replacement = Configuration("one");
        try
        {
            Assert.False(EmulationConfigurationDraftStore.TryGet("test-a", "one", out _));
            EmulationConfigurationDraftStore.Set("test-a", first);
            EmulationConfigurationDraftStore.Set("test-a", second);
            EmulationConfigurationDraftStore.Set("test-b", first);
            EmulationConfigurationDraftStore.Set("test-a", replacement);
            Assert.True(EmulationConfigurationDraftStore.TryGet("test-a", "one", out var selected)); Assert.Same(replacement, selected);
            Assert.True(EmulationConfigurationDraftStore.TryGet("test-b", "one", out selected)); Assert.Same(first, selected);
            EmulationConfigurationDraftStore.Remove("test-a", "one");
            Assert.False(EmulationConfigurationDraftStore.TryGet("test-a", "one", out _));
            Assert.True(EmulationConfigurationDraftStore.TryGet("test-a", "two", out selected)); Assert.Same(second, selected);
        }
        finally { EmulationConfigurationDraftStore.Remove("test-a", "one"); EmulationConfigurationDraftStore.Remove("test-a", "two"); EmulationConfigurationDraftStore.Remove("test-b", "one"); }
    }
}
