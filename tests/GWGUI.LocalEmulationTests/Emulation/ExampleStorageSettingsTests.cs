using System.Net.Http;
using GWGUI.Emulation.Interfaces;
using System.Reflection;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;
using GWGUI.Emulation.Example.Modules;

namespace GWGUI.Tests.Emulation;

public sealed class ExampleStorageSettingsTests
{
    [Fact]
    public void PublicStorageApiKeepsEmptyAndPartialSelectionsForBothProfilesAndAllThreeBackends()
    {
        using var httpClient = new HttpClient();
        var module = new ExampleModuleFactory().Create(new EmulationModuleContext(AppContext.BaseDirectory, AppContext.BaseDirectory, httpClient));
        var manager = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module);
        foreach (var profile in HardwareModelCatalog.All)
        foreach (var core in Enum.GetValues<Emulator>().Where(core => core != Emulator.None))
        {
            var original = new MachineConfiguration(profile.Model, core: core);
            var initial = manager.DescribeStorageSettings(original);
            foreach (var selected in new[] { initial.ConfiguredSlots.Take(1).ToArray(), Array.Empty<EmulationMediaSlot>() })
            {
                var requested = initial with { ConfiguredSlots = selected, MountedMedia = [], DeviceSettings = [] };
                var updated = Assert.IsType<MachineConfiguration>(manager.ApplyStorageSettings(original, requested));
                Assert.Equal(selected, updated.ConfiguredSlots);
                Assert.Equal(selected, manager.DescribeStorageSettings(updated).ConfiguredSlots);
                Assert.Equal(original.Id, updated.Id);
                Assert.Equal(original.Core, updated.Core);
                Assert.Equal(initial.ConfiguredSlots, original.ConfiguredSlots);
                Assert.Empty(updated.Media);
            }
        }
    }

    private static readonly Assembly Module = typeof(ExampleModuleFactory).Assembly;
    private static readonly Type Storage = Module.GetType("GWGUI.Emulation.Example.Common.Interfaces.IEmulatorStorageSettings", throwOnError: true)!;

    [Fact]
    public void ChangingProfileUsesItsDefaultReadersAndKeepsSelectionsWhenTheProfileIsUnchanged()
    {
        using var httpClient = new HttpClient();
        var module = new ExampleModuleFactory().Create(new EmulationModuleContext(AppContext.BaseDirectory, AppContext.BaseDirectory, httpClient));
        var storage = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module);
        foreach (var profile in HardwareModelCatalog.All)
        foreach (var core in Enum.GetValues<Emulator>())
        {
            var original = new MachineConfiguration(profile.Model, core: core);
            foreach (var selected in new[] { original.ConfiguredSlots.Take(1).ToArray(), Array.Empty<EmulationMediaSlot>() })
            {
                var configuration = original with { ConfiguredSlots = selected };
                var same = Assert.IsType<MachineConfiguration>(module.ChangeMachine(configuration, profile.StableModelId));
                Assert.Equal(selected, same.ConfiguredSlots);
                var target = HardwareModelCatalog.All.Single(model => model.Model != profile.Model);
                var changed = Assert.IsType<MachineConfiguration>(module.ChangeMachine(configuration, target.StableModelId));
                Assert.Equal(target.Model, changed.Model);
                Assert.Equal(storage.DescribeStorageSettings(changed).AvailableDevices.Select(device => device.Slot), changed.ConfiguredSlots);
                Assert.Equal(configuration.Id, changed.Id);
                Assert.Equal(configuration.Core, changed.Core);
                Assert.Equal(profile.Model, configuration.Model);
                Assert.Equal(selected, configuration.ConfiguredSlots);
            }
        }
    }

    [Fact]
    public void BothProfilesAndAllThreeBackendsKeepPartialAndEmptySelectionsAfterDocumentRoundTrip()
    {
        var adapters = Adapters();
        Assert.Equal(3, adapters.Length);
        Assert.Equal(2, HardwareModelCatalog.All.Count);
        foreach (var adapter in adapters)
        foreach (var model in HardwareModelCatalog.All)
        {
            var core = (Emulator)adapter.GetType().GetProperty("Emulator")!.GetValue(adapter)!;
            var original = new MachineConfiguration(model.Model, core: core);
            var initial = Describe(adapter, original);
            Assert.Equal(initial.AvailableDevices.Select(device => device.Slot), initial.ConfiguredSlots);
            foreach (var selected in new[] { initial.ConfiguredSlots.Take(1).ToArray(), Array.Empty<EmulationMediaSlot>() })
            {
                var updated = Apply(adapter, original, initial with { ConfiguredSlots = selected });
                Assert.Equal(selected, updated.ConfiguredSlots);
                Assert.Equal(selected, Describe(adapter, updated).ConfiguredSlots);
                Assert.Equal(initial.ConfiguredSlots, original.ConfiguredSlots);

                var document = InvokeStatic("ConfigurationStoreFunctions", "ToDocument", updated, AppContext.BaseDirectory)!;
                var json = JsonSerializer.SerializeToElement(document, document.GetType(), new JsonSerializerOptions(JsonSerializerDefaults.Web));
                Assert.Equal(selected.Length, json.GetProperty("configuredSlots").GetArrayLength());
                var parsed = InvokeStatic("ConfigurationStoreFunctions", "Deserialize", json)!;
                var restored = (MachineConfiguration)InvokeStatic("ConfigurationStoreFunctions", "FromDocument", parsed, AppContext.BaseDirectory)!;
                Assert.Equal(selected, restored.ConfiguredSlots);
                Assert.Equal(selected, Describe(adapter, restored).ConfiguredSlots);
                Assert.Equal(original.Id, restored.Id);
                Assert.Equal(original.Core, restored.Core);
            }
        }
    }

    [Fact]
    public void EmptySelectionAlsoWorksBeforeChoosingABackend()
    {
        var configuration = new MachineConfiguration(MachineModel.ExampleMachineA, core: Emulator.None);
        var initial = (EmulationStorageSettings)InvokeStatic("StorageSettingsFunctions", "Describe", configuration)!;
        var updated = (MachineConfiguration)InvokeStatic("StorageSettingsFunctions", "Apply", configuration, initial with { ConfiguredSlots = [] })!;
        Assert.Empty(updated.ConfiguredSlots);
        Assert.Empty(((EmulationStorageSettings)InvokeStatic("StorageSettingsFunctions", "Describe", updated)!).ConfiguredSlots);
        Assert.NotEmpty(configuration.ConfiguredSlots);
    }

    [Fact]
    public void ConstructorAndAdapterCopyTheirSelectedSlotLists()
    {
        var originalSlots = new List<EmulationMediaSlot> { EmulationMediaSlot.Floppy0 };
        var configuration = new MachineConfiguration(MachineModel.ExampleMachineA, configuredSlots: originalSlots);
        originalSlots.Clear();
        Assert.Equal(EmulationMediaSlot.Floppy0, Assert.Single(configuration.ConfiguredSlots));

        var selected = new List<EmulationMediaSlot> { EmulationMediaSlot.Cassette0 };
        var adapter = Adapters()[0];
        var updated = (MachineConfiguration)Storage.GetMethod("ApplyStorageConfiguration")!.Invoke(adapter, [configuration, selected])!;
        selected.Clear();
        Assert.Equal(EmulationMediaSlot.Cassette0, Assert.Single(updated.ConfiguredSlots));
        Assert.Equal(EmulationMediaSlot.Floppy0, Assert.Single(configuration.ConfiguredSlots));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnknownOrDuplicateSelectedSlotsAreRejected(bool duplicate)
    {
        var configuration = new MachineConfiguration(MachineModel.ExampleMachineA);
        var adapter = Adapters()[0];
        var initial = Describe(adapter, configuration);
        EmulationMediaSlot[] selected = duplicate ? [EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy0] : [EmulationMediaSlot.MemoryCard0];
        var error = Assert.Throws<TargetInvocationException>(() => Apply(adapter, configuration, initial with { ConfiguredSlots = selected }));
        Assert.IsType<ArgumentException>(error.InnerException);
        Assert.Equal(initial.ConfiguredSlots, configuration.ConfiguredSlots);
    }

    [Fact]
    public void MediaCannotRemainMountedInADisabledSlot()
    {
        var configuration = new MachineConfiguration(MachineModel.ExampleMachineA);
        var adapter = Adapters()[0];
        var initial = Describe(adapter, configuration);
        var settings = initial with { ConfiguredSlots = [], MountedMedia = [new EmulationMedia(Path: "sample.adf", Type: EmulationMediaType.Floppy, Slot: EmulationMediaSlot.Floppy0, IsReadOnly: false, IsInserted: true)] };
        var error = Assert.Throws<TargetInvocationException>(() => Apply(adapter, configuration, settings));
        Assert.IsType<ArgumentException>(error.InnerException);

        var invalid = configuration with { ConfiguredSlots = [], Media = [new MediaConfiguration("sample.adf", MediaCategory.Floppy, EmulationMediaSlot.Floppy0)] };
        error = Assert.Throws<TargetInvocationException>(() => InvokeStatic("ConfigurationFunctions", "Validate", invalid));
        Assert.IsType<ArgumentException>(error.InnerException);
    }

    private static object[] Adapters() => Module.GetTypes().Where(type => type.IsClass && !type.IsAbstract && Storage.IsAssignableFrom(type)).Select(type => Activator.CreateInstance(type, nonPublic: true)!).ToArray();
    private static EmulationStorageSettings Describe(object adapter, MachineConfiguration configuration) => (EmulationStorageSettings)Storage.GetMethod("DescribeStorage")!.Invoke(adapter, [configuration])!;
    private static MachineConfiguration Apply(object adapter, MachineConfiguration configuration, EmulationStorageSettings settings) => (MachineConfiguration)Storage.GetMethod("ApplyStorage")!.Invoke(adapter, [configuration, settings])!;
    private static object? InvokeStatic(string typeName, string operation, params object[] arguments) => Module.GetType("GWGUI.Emulation.Example.Common.Machines.Common.Functions." + typeName, throwOnError: true)!
        .GetMethod(operation, BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, arguments);
}
