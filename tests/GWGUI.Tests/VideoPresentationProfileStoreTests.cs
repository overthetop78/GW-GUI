using System.IO;
using System.Text.Json;
using GWGUI.VideoPresentation.Services;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Amiga.Contracts;
using GWGUI.Emulation.Atari.Contracts;
using GWGUI.Emulation.Atari.Enums;
using GWGUI.Emulation.Atari.Functions;

namespace GWGUI.Tests;

public sealed class VideoPresentationProfileStoreTests
{
    [Fact]
    public void EmulationAssembliesHaveNoPresentationContractOrDependency()
    {
        foreach (var assembly in new[] { typeof(IEmulationConfiguration).Assembly,
            typeof(AmigaMachineConfiguration).Assembly, typeof(AtariMachineConfiguration).Assembly })
        {
            Assert.DoesNotContain(assembly.GetReferencedAssemblies(),
                name => name.Name == typeof(EmulationVideoPresentationProfile).Assembly.GetName().Name);
            Assert.DoesNotContain(assembly.GetTypes(), type =>
                type.Name is "EmulationVideoProcessingConfiguration" or "EmulationVideoRenderer");
        }
        Assert.DoesNotContain(typeof(IEmulationConfiguration).GetProperties(),
            property => property.Name is "VideoRenderer" or "VideoProcessing");
        Assert.DoesNotContain(typeof(IEmulationModule).GetMethods(),
            method => method.Name == "ApplyVideoProcessing");
    }

    [Fact]
    public void DraftDoesNotWriteAndPersistedProfilesAreIsolatedByModuleAndConfiguration()
    {
        using var directory = new TestDirectory();
        var store = new VideoPresentationProfileStore(directory.Profiles);
        var id = Guid.NewGuid();
        var other = Guid.NewGuid();
        var expected = Profile();
        store.Set("amiga", id, expected);
        Assert.False(Directory.Exists(directory.Profiles));
        Assert.NotEqual(expected, store.Get("atari", id));
        Assert.NotEqual(expected, store.Get("amiga", other));
        store.Save("amiga", id);
        var restarted = new VideoPresentationProfileStore(directory.Profiles);
        Assert.Equal(expected, restarted.Get("amiga", id));
        Assert.NotEqual(expected, restarted.Get("atari", id));
    }

    [Fact]
    public async Task CopyDeleteAndPendingSavesDoNotResurrectDeletedProfiles()
    {
        using var directory = new TestDirectory();
        var store = new VideoPresentationProfileStore(directory.Profiles);
        var source = Guid.NewGuid();
        var destination = Guid.NewGuid();
        store.Set("test", source, Profile());
        store.Copy("test", source, destination);
        Assert.Equal(Profile(), new VideoPresentationProfileStore(directory.Profiles).Get("test", destination));
        var saving = store.SaveAsync("test", destination);
        store.Delete("test", destination);
        await saving;
        Assert.NotEqual(Profile(), new VideoPresentationProfileStore(directory.Profiles).Get("test", destination));
        Assert.Equal(Profile(), store.Get("test", source));
    }

    [Theory]
    [InlineData("amiga")]
    [InlineData("atari")]
    public void MigrationIsIdempotentAndLeavesTheLegacyFileUntouched(string module)
    {
        using var directory = new TestDirectory();
        var legacy = Path.Combine(directory.Root, "legacy.json");
        const string json = """{"videoRenderer":"Vulkan","videoProcessing":{"projection":{"ambientLight":42},"displayTechnology":"Projection"}}""";
        File.WriteAllText(legacy, json);
        var id = Guid.NewGuid();
        var store = new VideoPresentationProfileStore(directory.Profiles, (_, _) => [legacy]);
        var migrated = store.Get(module, id);
        Assert.Equal(EmulationVideoRenderer.Vulkan, migrated.Renderer);
        Assert.Equal(42, migrated.Processing!.Projection.AmbientLight);
        Assert.Equal(json, File.ReadAllText(legacy));
        File.WriteAllText(legacy, "{}");
        Assert.Equal(migrated, new VideoPresentationProfileStore(directory.Profiles, (_, _) => [legacy]).Get(module, id));
    }

    [Fact]
    public void FailedMigrationCanBeRetriedWithoutReplacingSourceOrCachingDefaults()
    {
        using var directory = new TestDirectory();
        var legacy = Path.Combine(directory.Root, "legacy.json");
        const string json = """{"videoRenderer":1,"videoProcessing":{"adjustments":{"brightness":7}}}""";
        File.WriteAllText(legacy, json);
        File.WriteAllText(directory.Profiles, "blocks the destination directory");
        var id = Guid.NewGuid();
        var store = new VideoPresentationProfileStore(directory.Profiles, (_, _) => [legacy]);
        Assert.ThrowsAny<IOException>(() => store.Get("amiga", id));
        Assert.Equal(json, File.ReadAllText(legacy));
        File.Delete(directory.Profiles);
        Assert.Equal(7, store.Get("amiga", id).Processing!.Adjustments.Brightness);
    }

    [Fact]
    public void CorruptHostProfileIsReportedAndNeverSilentlyOverwritten()
    {
        using var directory = new TestDirectory();
        var id = Guid.NewGuid();
        var store = new VideoPresentationProfileStore(directory.Profiles);
        store.Set("test", id, Profile());
        store.Save("test", id);
        var path = Assert.Single(Directory.GetFiles(directory.Profiles, "*.json", SearchOption.AllDirectories));
        File.WriteAllText(path, "{broken");
        var restarted = new VideoPresentationProfileStore(directory.Profiles);
        Assert.Throws<JsonException>(() => restarted.Get("test", id));
        Assert.Equal("{broken", File.ReadAllText(path));
    }

    [Fact]
    public async Task FlushKeepsTheLatestRequestedProfile()
    {
        using var directory = new TestDirectory();
        var id = Guid.NewGuid();
        var store = new VideoPresentationProfileStore(directory.Profiles);
        var requests = new List<Task>();
        for (var value = 0; value <= 10; value++)
        {
            store.Set("test", id, Profile() with
            {
                Processing = new() { Adjustments = new(Brightness: value) }
            });
            requests.Add(store.SaveAsync("test", id));
        }
        store.FlushPending();
        await Task.WhenAll(requests);
        Assert.Equal(10, new VideoPresentationProfileStore(directory.Profiles)
            .Get("test", id).Processing!.Adjustments.Brightness);
    }

    [Fact]
    public void AtariStateCompatibilityIgnoresPresentationButStillRejectsNativeChanges()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.St);
        var nativeHash = AtariSavedStateFunctions.ConfigurationHash(configuration);
        Assert.True(AtariSavedStateFunctions.IsCompatibleConfigurationHash(configuration, nativeHash));
        for (var value = 0; value < 4; value++)
        {
            var legacyHash = AtariSavedStateFunctions.LegacyConfigurationHash(configuration, value);
            // Reproduce the historical contract independently of the migration implementation.
            var historical = new
            {
                configuration.SchemaVersion, configuration.Model, configuration.Core,
                configuration.AudioEnabled, VideoRenderer = value,
                Content = Array.Empty<AtariStateContentEntry>(),
                Options = configuration.Options.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray(),
                Input = new AtariStateInputFingerprint([], [], configuration.Input.MouseDeviceId,
                    configuration.Input.CaptureMouse, configuration.Input.ReleaseMouseKey.ToString()),
                Media = Array.Empty<AtariMediaConfiguration>(),
                Firmwares = Array.Empty<AtariFirmwareConfiguration>()
            };
            Assert.Equal(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                JsonSerializer.SerializeToUtf8Bytes(historical, new JsonSerializerOptions(JsonSerializerDefaults.Web)))),
                legacyHash);
            Assert.NotEqual(nativeHash, legacyHash);
            Assert.True(AtariSavedStateFunctions.IsCompatibleConfigurationHash(configuration, legacyHash));
            Assert.False(AtariSavedStateFunctions.IsCompatibleConfigurationHash(
                configuration with { AudioEnabled = !configuration.AudioEnabled }, legacyHash));
        }
    }

    private static EmulationVideoPresentationProfile Profile() =>
        new EmulationVideoPresentationProfile(EmulationVideoRenderer.OpenGL,
            new() { DisplayTechnology = EmulationVideoDisplayTechnology.Projection,
                Projection = new(AmbientLight: 37, Vignette: 61) }).Normalize();

    private sealed class TestDirectory : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "GWGUI.ProfileTests", Guid.NewGuid().ToString("N"));
        public string Profiles => Path.Combine(Root, "profiles");
        public TestDirectory() => Directory.CreateDirectory(Root);
        public void Dispose() => Directory.Delete(Root, recursive: true);
    }
}
