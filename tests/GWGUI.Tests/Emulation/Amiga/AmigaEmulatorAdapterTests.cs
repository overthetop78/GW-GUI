using System.Reflection;
using System.Text.Json;
using GWGUI.Emulation.Amiga.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Amiga.Common.Machines.Common.Enums;
using GWGUI.Emulation.Amiga.Modules;
using GWGUI.Emulation.Amiga.Common.Services;

namespace GWGUI.Tests.Emulation.Amiga;

public sealed class AmigaEmulatorAdapterTests
{
    [Fact]
    public void MachineConfigurationCrossesTheCoreHostJsonBoundary()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var original = new MachineConfiguration("A500", "kickstart.rom", Core: Emulator.External);

        var restored = JsonSerializer.Deserialize<MachineConfiguration>(
            JsonSerializer.Serialize(original, options), options);

        Assert.NotNull(restored);
        Assert.Equal(original.Id, restored.Id);
        Assert.Equal(original.Model, restored.Model);
        Assert.Equal(original.Core, restored.Core);
        Assert.Equal(original.KickstartPath, restored.KickstartPath);
    }

    [Fact]
    public void PuaeUsesItsPhysicalNamespaceAndTheCommonAdapter()
    {
        var assembly = typeof(AmigaEmulationModule).Assembly;
        var adapter = assembly.GetType("GWGUI.Emulation.Amiga.Emulators.PUAE.Factories.PuaeMachineFactory");
        var common = assembly.GetType("GWGUI.Emulation.Amiga.Common.Interfaces.IEmulatorAdapter");
        Assert.NotNull(adapter);
        Assert.NotNull(common);
        Assert.Contains(common!, adapter!.GetInterfaces());
        var required = new[] { "Create", "FindInstalledCorePathAsync", "FindReleasesAsync",
            "GetInstallationAsync", "InstallAsync", "ResolveConfiguredMedia", "TryHandleHostCommand" };
        Assert.All(required, name => Assert.Contains(common!.GetMethods(), method => method.Name == name));
    }

    [Fact]
    public void EngineRegistersPuaeOnlyThroughTheCommonAdapter()
    {
        var field = typeof(Engine).GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic);
        var adapters = Assert.IsAssignableFrom<System.Collections.IEnumerable>(field!.GetValue(new Engine()));
        var entries = adapters.Cast<object>().ToArray();
        var entry = Assert.Single(entries);
        Assert.Equal("puae", entry.GetType().GetProperty("Key")!.GetValue(entry));
    }
}
