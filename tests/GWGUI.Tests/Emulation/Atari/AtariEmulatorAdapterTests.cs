using System.Reflection;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Atari.Emulators.Libretro.Dictionaries;
using GWGUI.Emulation.Atari.Modules;
using GWGUI.Emulation.Atari.Common.Services;

namespace GWGUI.Tests.Emulation.Atari;

public sealed class AtariEmulatorAdapterTests
{
    [Fact]
    public void ConcreteAdaptersUseTheirPhysicalNamespaces()
    {
        var names = typeof(AtariEmulationModule).Assembly.GetTypes().Select(type => type.FullName).ToHashSet();
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.Hatari.Factories.HatariMachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.Atari800.Factories.Atari800MachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.Stella.Factories.StellaMachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.ProSystem.Factories.ProSystemMachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.BeetleLynx.Factories.BeetleLynxMachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Factories.VirtualJaguarMachineFactory", names);
    }

    [Fact]
    public void EngineRegistrationsMatchTheCatalog()
    {
        var field = typeof(Engine).GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic);
        var adapters = Assert.IsAssignableFrom<System.Collections.IEnumerable>(field!.GetValue(new Engine()));
        var ids = adapters.Cast<object>().Select(item => item.GetType().GetProperty("Key")!.GetValue(item) as string)
            .Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(CoreCatalog.All.Select(item => item.Id).Order(StringComparer.Ordinal), ids);
        foreach (var entry in CoreCatalog.All)
        {
            var adapter = adapters.Cast<object>().Single(item =>
                string.Equals(item.GetType().GetProperty("Key")!.GetValue(item) as string,
                    entry.Id, StringComparison.Ordinal));
            var value = adapter.GetType().GetProperty("Value")!.GetValue(adapter)!;
            var definition = Assert.IsType<EmulationEmulatorDefinition>(
                value.GetType().GetProperty("Definition")!.GetValue(value));
            Assert.Equal(CoreCatalog.GetDefinition(entry).MachineIds.Order(StringComparer.Ordinal),
                definition.MachineIds.Order(StringComparer.Ordinal));
        }
    }
}
