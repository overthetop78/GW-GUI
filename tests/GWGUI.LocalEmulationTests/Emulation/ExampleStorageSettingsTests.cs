using System.IO;
using System.Net.Http;
using GWGUI.Emulation.Interfaces;
using System.Reflection;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;
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
            foreach (var device in initial.AvailableDevices)
            {
                if (device.MediaType == EmulationMediaType.MemoryCard)
                {
                    Assert.Equal(EmulationStorageConfigurationKind.MemoryCard, device.ConfigurationKind);
                    Assert.True(device.FixedImageSizeBytes > 0);
                    var cardFormats = Assert.IsAssignableFrom<IReadOnlyList<HardDiskImageFormat>>(device.HardDiskFormats);
                    Assert.Equal(device.AcceptedExtensions, cardFormats.Select(format => format.Extension));
                    Assert.All(cardFormats, format =>
                    {
                        Assert.Equal((long)device.FixedImageSizeBytes!.Value, format.DefaultBytes);
                        Assert.Equal(format.DefaultBytes, format.MinimumBytes);
                        Assert.Equal(format.DefaultBytes, format.MaximumBytes);
                        Assert.Equal(DiskContainerKind.Raw, format.Container);
                    });
                    continue;
                }
                if (device.MediaType != EmulationMediaType.HardDisk)
                {
                    Assert.Null(device.HardDiskFormats);
                    if (device.MediaType == EmulationMediaType.Floppy)
                    {
                        Assert.Equal(EmulationStorageConfigurationKind.FloppyDrive, device.ConfigurationKind);
                        Assert.NotNull(device.FloppyOptions);
                        Assert.NotEmpty(device.FloppyOptions.Models);
                    }
                    if (device.MediaType == EmulationMediaType.Cartridge) Assert.Equal(EmulationStorageConfigurationKind.CartridgeSlot, device.ConfigurationKind);
                    continue;
                }
                Assert.Equal(EmulationStorageConfigurationKind.HardDiskDrive, device.ConfigurationKind);
                var formats = Assert.IsAssignableFrom<IReadOnlyList<HardDiskImageFormat>>(device.HardDiskFormats);
                Assert.NotEmpty(formats);
                Assert.Equal(device.AcceptedExtensions, formats.Select(format => format.Extension));
                Assert.Equal(formats.Count, formats.Select(format => format.Id).Distinct().Count());
                foreach (var format in formats)
                {
                    Assert.InRange(format.DefaultBytes, format.MinimumBytes, format.MaximumBytes);
                    Assert.Contains(512, format.LogicalSectorSizes);
                    Assert.Equal(0, format.DefaultBytes % 512);
                    Assert.True(format.Operations.HasFlag(DiskFormatOperations.Create));
                    Assert.Equal(format.Extension switch { ".vhd" => DiskContainerKind.Vhd, ".vhdx" => DiskContainerKind.Vhdx, _ => DiskContainerKind.Raw }, format.Container);
                }
            }
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

    [Fact]
    public void MountMetadataSurvivesStorageApplicationAndDocumentRoundTripForEveryBackend()
    {
        foreach (var adapter in Adapters())
        foreach (var model in HardwareModelCatalog.All)
        {
            var core = (Emulator)adapter.GetType().GetProperty("Emulator")!.GetValue(adapter)!;
            var media = model.Model == MachineModel.ExampleMachineA
                ? new[]
                {
                    new MediaConfiguration(Path.Combine(AppContext.BaseDirectory, "disk.img"), MediaCategory.HardDisk, EmulationMediaSlot.HardDisk0,
                        Label: "Volume", StorageBus: StorageBus.Ide, MountPoint: "C", MountOrder: 2),
                    new MediaConfiguration(Path.Combine(AppContext.BaseDirectory, "tape.cas"), MediaCategory.Cassette, EmulationMediaSlot.Cassette0,
                        CassetteAutoBoot: true, MountOrder: 1)
                }
                : new[]
                {
                    new MediaConfiguration(Path.Combine(AppContext.BaseDirectory, "cartridge.rom"), MediaCategory.Cartridge, EmulationMediaSlot.Cartridge0,
                        Label: "Cartouche", CartridgePlatform: CartridgePlatform.Alternate, CartridgeType: 57, CartridgeRegion: EmulationVideoStandard.Secam)
                };
            var original = new MachineConfiguration(model.Model, core: core) with { Media = media };
            var compatibility = (CompatibilityDefinition)typeof(ModelCatalog).GetMethod("GetCompatibility", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [original])!;
            Assert.Equal(model.Model, compatibility.Model);
            Assert.Equal(core, compatibility.Core);
            Assert.All(media, item => Assert.Contains(compatibility.Media, rule => rule.Category == item.Category && rule.Slots.Contains(item.Slot)));
            var initial = Describe(adapter, original);
            var requested = initial with
            {
                MountedMedia = initial.MountedMedia.Select(item => item with { Path = Path.Combine(AppContext.BaseDirectory, "changed-" + Path.GetFileName(item.Path)) }).ToArray()
            };
            var updated = Apply(adapter, original, requested);
            Assert.Equal(media.Select(item => item with { Path = Path.Combine(AppContext.BaseDirectory, "changed-" + Path.GetFileName(item.Path)) }), updated.Media);
            Assert.Equal(media, original.Media);
            var document = InvokeStatic("ConfigurationStoreFunctions", "ToDocument", updated, AppContext.BaseDirectory)!;
            var json = JsonSerializer.SerializeToElement(document, document.GetType(), new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var parsed = InvokeStatic("ConfigurationStoreFunctions", "Deserialize", json)!;
            var restored = (MachineConfiguration)InvokeStatic("ConfigurationStoreFunctions", "FromDocument", parsed, AppContext.BaseDirectory)!;
            Assert.Equal(updated.Media, restored.Media);
            Assert.Equal(updated.ConfiguredSlots, restored.ConfiguredSlots);
            Assert.Equal(updated.Id, restored.Id);
            Assert.Equal(updated.Core, restored.Core);
        }

        foreach (var category in new[] { MediaCategory.Directory, MediaCategory.Snapshot, MediaCategory.CompactDisc })
        {
            var slot = category == MediaCategory.Directory ? EmulationMediaSlot.HardDisk0 : category == MediaCategory.Snapshot ? EmulationMediaSlot.Floppy0 : EmulationMediaSlot.Cd0;
            var original = new MediaConfiguration("content", category, slot, StorageBus: category == MediaCategory.Directory ? StorageBus.Gemdos : null, MountOrder: 3);
            var common = (EmulationMedia)InvokeStatic("EmulationMediaConversionFunctions", "ToCommon", original)!;
            var updated = (MediaConfiguration)InvokeStatic("EmulationMediaConversionFunctions", "ToModule", common with { Path = "changed" }, new[] { original })!;
            Assert.Equal(original with { Path = "changed" }, updated);
        }
    }
    [Fact]
    public void DeviceParametersSurviveApplicationAndPortableDocumentRoundTripForBothProfilesAndEveryBackend()
    {
        using var httpClient = new HttpClient();
        var module = new ExampleModuleFactory().Create(new EmulationModuleContext(AppContext.BaseDirectory, AppContext.BaseDirectory, httpClient));
        var manager = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module);
        foreach (var profile in HardwareModelCatalog.All)
        foreach (var core in Enum.GetValues<Emulator>())
        {
            var original = new MachineConfiguration(profile.Model, core: core, options: new Dictionary<string, string> { ["unrelated"] = "unchanged" });
            var initial = manager.DescribeStorageSettings(original);
            var extra = new Dictionary<string, string> { ["mode"] = "mock" };
            EmulationStorageDeviceSettings[] requested = profile.Model == MachineModel.ExampleMachineA
                ? [new(EmulationMediaSlot.Floppy0, Floppy: new("Floppy", "400", true, true, extra)),
                    new(EmulationMediaSlot.HardDisk0, InterfaceId: "IDE", Options: new Dictionary<string, string> { ["unit"] = "0" }),
                    new(EmulationMediaSlot.Cassette0, Options: new Dictionary<string, string> { ["transport"] = "mock" })]
                : [new(EmulationMediaSlot.Cartridge0, Cartridge: new(true, extra)),
                    new(EmulationMediaSlot.MemoryCard0, MemoryCardPath: Path.Combine(AppContext.BaseDirectory, "cards", "card.mem"), Options: new Dictionary<string, string> { ["bank"] = "0" })];
            var updated = Assert.IsType<MachineConfiguration>(manager.ApplyStorageSettings(original, initial with { DeviceSettings = requested }));
            var described = manager.DescribeStorageSettings(updated);
            foreach (var expected in requested)
                Assert.Equal(JsonSerializer.Serialize(expected), JsonSerializer.Serialize(Assert.Single(described.DeviceSettings!, item => item.Slot == expected.Slot)));
            Assert.Equal("unchanged", updated.Options["unrelated"]);
            Assert.Empty(manager.DescribeStorageSettings(original).DeviceSettings!);
            if (core != Emulator.None)
            {
                var adapter = ExampleFirmwareSamplesTests.Adapter(core);
                var resolve = adapter.GetType().GetMethod("ResolveConfiguredMedia")!;
                var resolved = Assert.IsAssignableFrom<IReadOnlyList<EmulationMedia>>(resolve.Invoke(adapter, [updated]));
                if (profile.Model == MachineModel.ExampleMachineB)
                {
                    var card = Assert.Single(resolved, item => item.Slot == EmulationMediaSlot.MemoryCard0);
                    Assert.Equal(requested.Single(item => item.Slot == card.Slot).MemoryCardPath, card.Path);
                    Assert.Equal(EmulationMediaType.MemoryCard, card.Type);
                    Assert.False(card.IsReadOnly);
                    Assert.True(card.IsInserted);
                    Assert.Empty(updated.Media);
                    var explicitCard = new MediaConfiguration("explicit.mem", MediaCategory.MemoryCard, card.Slot, IsInserted: false);
                    var explicitConfiguration = updated with { Media = [explicitCard] };
                    var explicitResolved = Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<EmulationMedia>>(resolve.Invoke(adapter, [explicitConfiguration])));
                    Assert.Equal(explicitCard.Path, explicitResolved.Path);
                    Assert.False(explicitResolved.IsInserted);
                    var disabled = updated with { ConfiguredSlots = [] };
                    Assert.Empty(Assert.IsAssignableFrom<IReadOnlyList<EmulationMedia>>(resolve.Invoke(adapter, [disabled])));
                }
                else
                    Assert.DoesNotContain(resolved, item => item.Type == EmulationMediaType.MemoryCard);
            }
            extra["mode"] = "changed-after-application";
            var current = Assert.Single(manager.DescribeStorageSettings(updated).DeviceSettings!, item => item.Slot == requested[0].Slot);
            Assert.Equal("mock", (current.Floppy?.Options ?? current.Cartridge!.Options)!["mode"]);

            var document = InvokeStatic("ConfigurationStoreFunctions", "ToDocument", updated, AppContext.BaseDirectory)!;
            var json = JsonSerializer.SerializeToElement(document, document.GetType(), new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var parsed = InvokeStatic("ConfigurationStoreFunctions", "Deserialize", json)!;
            var restored = (MachineConfiguration)InvokeStatic("ConfigurationStoreFunctions", "FromDocument", parsed, AppContext.BaseDirectory)!;
            Assert.Equal(updated.Options.OrderBy(item => item.Key), restored.Options.OrderBy(item => item.Key));
            Assert.Equal(JsonSerializer.Serialize(described.DeviceSettings), JsonSerializer.Serialize(manager.DescribeStorageSettings(restored).DeviceSettings));
            if (profile.Model == MachineModel.ExampleMachineB)
            {
                var relocatedBase = Path.Combine(AppContext.BaseDirectory, "relocated");
                var relocated = (MachineConfiguration)InvokeStatic("ConfigurationStoreFunctions", "FromDocument", parsed, relocatedBase)!;
                Assert.Equal(Path.Combine(relocatedBase, "cards", "card.mem"), Assert.Single(manager.DescribeStorageSettings(relocated).DeviceSettings!, item => item.Slot == EmulationMediaSlot.MemoryCard0).MemoryCardPath);
            }
            var cleared = Assert.IsType<MachineConfiguration>(manager.ApplyStorageSettings(updated, described with { DeviceSettings = [] }));
            Assert.Empty(manager.DescribeStorageSettings(cleared).DeviceSettings!);
            Assert.Equal(original.Options, cleared.Options);
        }
    }

    [Fact]
    public void DeviceParametersRejectWrongReadersModelsSpeedsInterfacesAndConflictingOptions()
    {
        using var httpClient = new HttpClient();
        var module = new ExampleModuleFactory().Create(new EmulationModuleContext(AppContext.BaseDirectory, AppContext.BaseDirectory, httpClient));
        var manager = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module);
        foreach (var core in Enum.GetValues<Emulator>())
        {
            var configuration = new MachineConfiguration(MachineModel.ExampleMachineA, core: core);
            var original = manager.DescribeStorageSettings(configuration);
            foreach (var invalid in new EmulationStorageDeviceSettings[]
            {
                new(EmulationMediaSlot.HardDisk0, Floppy: new("Floppy", "100", false, false)),
                new(EmulationMediaSlot.Floppy0, Floppy: new("unknown", "100", false, false)),
                new(EmulationMediaSlot.Floppy0, Floppy: new("Floppy", "invalid", false, false)),
                new(EmulationMediaSlot.Floppy0, MemoryCardPath: "card.mem"),
                new(EmulationMediaSlot.HardDisk0, InterfaceId: "unknown"),
                new(EmulationMediaSlot.Floppy0, Cartridge: new(true)),
                new(EmulationMediaSlot.Floppy0, CompactDisc: new()),
                new(EmulationMediaSlot.Floppy0, Floppy: new("Floppy", "100", false, false, new Dictionary<string, string> { ["mode"] = "first" }), Options: new Dictionary<string, string> { ["mode"] = "second" })
            })
                Assert.Throws<ArgumentException>(() => manager.ApplyStorageSettings(configuration, original with { DeviceSettings = [invalid] }));
            Assert.Empty(configuration.Options);
            Assert.Empty(manager.DescribeStorageSettings(configuration).DeviceSettings!);
        }
    }

    private static object[] Adapters() => Module.GetTypes().Where(type => type.IsClass && !type.IsAbstract && Storage.IsAssignableFrom(type)).Select(type => Activator.CreateInstance(type, nonPublic: true)!).ToArray();
    private static EmulationStorageSettings Describe(object adapter, MachineConfiguration configuration) => (EmulationStorageSettings)Storage.GetMethod("DescribeStorage")!.Invoke(adapter, [configuration])!;
    private static MachineConfiguration Apply(object adapter, MachineConfiguration configuration, EmulationStorageSettings settings) => (MachineConfiguration)Storage.GetMethod("ApplyStorage")!.Invoke(adapter, [configuration, settings])!;
    private static object? InvokeStatic(string typeName, string operation, params object[] arguments) => Module.GetType("GWGUI.Emulation.Example.Common.Machines.Common.Functions." + typeName, throwOnError: true)!
        .GetMethod(operation, BindingFlags.Static | BindingFlags.NonPublic, binder: null, types: arguments.Select(argument => argument.GetType()).ToArray(), modifiers: null)!.Invoke(null, arguments);
}
