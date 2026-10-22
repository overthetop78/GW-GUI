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
    private static readonly Assembly Module = typeof(ExampleModuleFactory).Assembly;
    private static readonly Type Storage = Module.GetType("GWGUI.Emulation.Example.Common.Interfaces.IEmulatorStorageSettings", throwOnError: true)!;

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
