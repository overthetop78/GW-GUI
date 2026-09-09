using System.Text.Json;
using GWGUI.App.Contracts.Updates;
using GWGUI.App.Services.Updates;
using GWGUI.Updates.Contracts;

namespace GWGUI.Tests.Updates;

internal static class PendingModuleInstallationScenarios
{
    internal static void StoreKeepsDistinctModulesAndRejectsDuplicate()
    {
        var root = TemporaryDirectory();
        try
        {
            var store = new PendingModuleInstallationStore();
            var changes = 0;
            store.Changed += (_, _) => changes++;
            Assert.True(store.TryAdd(Installation(root, "amiga")));
            Assert.True(store.TryAdd(Installation(root, "atari")));
            Assert.False(store.TryAdd(Installation(root, "amiga")));
            Assert.Equal(new[] { "amiga", "atari" }, store.Items.Select(item => item.ModuleId).Order().ToArray());
            Assert.Equal(2, changes);
            store.Clear();
            Assert.Empty(store.Items);
            Assert.Equal(3, changes);
        }
        finally { Delete(root); }
    }

    internal static async Task OneLaunchPlanContainsEveryPendingModule()
    {
        var root = TemporaryDirectory();
        var application = Path.Combine(root, "application");
        var working = Path.Combine(root, "working");
        Directory.CreateDirectory(Path.Combine(application, "Updater"));
        File.WriteAllText(Path.Combine(application, "gwgui.exe"), string.Empty);
        File.WriteAllText(Path.Combine(application, "Updater", "gwgui.updater.exe"), string.Empty);
        var first = Installation(root, "amiga");
        var second = Installation(root, "atari");
        try
        {
            var service = new UpdatePackagePreparationService(
                installedState: () => new InstalledUpdateState("0.3.0", "1.0", []),
                applicationDirectory: application,
                workingRootDirectory: working);
            var launch = await service.CreateModuleLaunchAsync([first, second]);
            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(launch.PlanPath));
            var components = document.RootElement.GetProperty("components").EnumerateArray().ToArray();
            Assert.Equal(2, components.Length);
            Assert.Equal(new[] { "amiga", "atari" }, components
                .Select(component => component.GetProperty("componentId").GetString()).Order().ToArray());
            Assert.True(File.Exists(launch.UpdaterExecutable));
        }
        finally { Delete(root); }
    }

    private static PendingModuleInstallation Installation(string root, string id)
    {
        var work = Path.Combine(root, "source-" + id);
        var prepared = Path.Combine(work, "prepared");
        Directory.CreateDirectory(prepared);
        File.WriteAllText(Path.Combine(prepared, "module.json"), id);
        return new(id, id, "1.0.0", $"https://example.invalid/{id}.json",
            new PreparedUpdateComponent(id, UpdateComponentKind.Module,
                UpdateComponentOperation.InstallModule, "1.0.0", prepared), work);
    }

    private static string TemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "GWGUI.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void Delete(string path)
    {
        try { if (Directory.Exists(path)) Directory.Delete(path, recursive: true); }
        catch { }
    }
}
