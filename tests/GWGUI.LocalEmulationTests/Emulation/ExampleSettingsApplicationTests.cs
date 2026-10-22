using System.Net.Http;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;
using GWGUI.Emulation.Example.Modules;

namespace GWGUI.Tests.Emulation;

public sealed class ExampleSettingsApplicationTests
{
    [Fact]
    public void ModuleAppliesEditorValuesForEveryProfileAndCoreWithoutChangingTheInput()
    {
        using var httpClient = new HttpClient();
        var module = new ExampleModuleFactory().Create(new EmulationModuleContext(AppContext.BaseDirectory, AppContext.BaseDirectory, httpClient));
        foreach (var profile in HardwareModelCatalog.All)
        foreach (var core in Enum.GetValues<Emulator>())
        {
            var configuration = new MachineConfiguration(profile.Model, core: core);
            var field = AudioEnabledField(configuration);
            var values = new Dictionary<string, string?> { [field] = bool.FalseString };
            var updated = Assert.IsType<MachineConfiguration>(module.ApplySettings(configuration, values));
            Assert.False(updated.AudioEnabled);
            Assert.True(configuration.AudioEnabled);
            Assert.Equal(configuration.Id, updated.Id);
            Assert.Equal(configuration.Core, updated.Core);
            Assert.Equal(configuration.ConfiguredSlots, updated.ConfiguredSlots);
            Assert.DoesNotContain(field, updated.Options.Keys);
            Assert.Empty(configuration.Options);
            Assert.Equal(bool.FalseString, values[field]);
        }
    }

    [Fact]
    public void AllThreeAdaptersApplyTheSameEditorContract()
    {
        var assembly = typeof(ExampleModuleFactory).Assembly;
        var contract = assembly.GetType("GWGUI.Emulation.Example.Common.Interfaces.IEmulatorSettingsApplication", throwOnError: true)!;
        var method = contract.GetMethod("ApplySettings")!;
        var adapters = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
        Assert.Equal(3, adapters.Length);
        foreach (var type in adapters)
        foreach (var profile in HardwareModelCatalog.All)
        {
            var adapter = Activator.CreateInstance(type, nonPublic: true)!;
            var core = (Emulator)type.GetProperty("Emulator")!.GetValue(adapter)!;
            var configuration = new MachineConfiguration(profile.Model, core: core);
            var values = new Dictionary<string, string?> { [AudioEnabledField(configuration)] = bool.FalseString };
            var updated = Assert.IsType<MachineConfiguration>(method.Invoke(adapter, [configuration, values]));
            Assert.False(updated.AudioEnabled);
            Assert.True(configuration.AudioEnabled);
            Assert.Equal(configuration.Model, updated.Model);
            Assert.Equal(configuration.Id, updated.Id);
        }
    }

    [Fact]
    public void InvalidAudioValuesAreRejectedWithOrWithoutASelectedCore()
    {
        using var httpClient = new HttpClient();
        var module = new ExampleModuleFactory().Create(new EmulationModuleContext(AppContext.BaseDirectory, AppContext.BaseDirectory, httpClient));
        foreach (var core in new[] { Emulator.None, Emulator.Internal })
        {
            var configuration = new MachineConfiguration(MachineModel.ExampleMachineA, core: core);
            var values = new Dictionary<string, string?> { [AudioEnabledField(configuration)] = "invalid" };
            Assert.Throws<ArgumentException>(() => module.ApplySettings(configuration, values));
            Assert.True(configuration.AudioEnabled);
            Assert.Empty(configuration.Options);
        }
    }

    private static string AudioEnabledField(MachineConfiguration configuration)
    {
        var assembly = typeof(ExampleModuleFactory).Assembly;
        var contract = assembly.GetType("GWGUI.Emulation.Example.Common.Interfaces.IEmulatorSettings", throwOnError: true)!;
        var adapters = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).Select(type => Activator.CreateInstance(type, nonPublic: true)!).ToArray();
        var adapter = configuration.Core == Emulator.None ? adapters[0] : adapters.Single(candidate =>
            (Emulator)candidate.GetType().GetProperty("Emulator")!.GetValue(candidate)! == configuration.Core);
        var blocks = Assert.IsAssignableFrom<IReadOnlyList<EmulationSettingsBlock>>(contract.GetMethod("DescribeSettings")!.Invoke(adapter, [configuration, null]));
        return Assert.Single(blocks.SelectMany(block => block.Fields), field => field.LabelResourceKey == "Emulation.Audio.Enabled").Id;
    }
}
