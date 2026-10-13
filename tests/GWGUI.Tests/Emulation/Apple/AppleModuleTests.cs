using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Apple.Common.Machines.Common.Constants;
using GWGUI.Emulation.Apple.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;
using GWGUI.Emulation.Apple.Common.Machines.Common.Functions;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Functions;

namespace GWGUI.Tests.Emulation.Apple;

public sealed class AppleModuleTests
{
    [Theory]
    [InlineData(MachineModel.AppleIIe, EmulationKey.AppleOpenApple, EmulationKey.LeftAlt)]
    [InlineData(MachineModel.AppleIIe, EmulationKey.AppleSolidApple, EmulationKey.RightAlt)]
    [InlineData(MachineModel.MacintoshII, EmulationKey.MacintoshCommand, EmulationKey.LeftAlt)]
    [InlineData(MachineModel.MacintoshII, EmulationKey.MacintoshKeypadClear, EmulationKey.Delete)]
    [InlineData(MachineModel.MacintoshII, EmulationKey.MacintoshKeypadEquals, EmulationKey.Insert)]
    public void ErasedSpecialKeyRemainsErasedAfterPersistenceAndDoesNotRestoreItsDefault(MachineModel model, EmulationKey target, EmulationKey previous)
    {
        var configuration = new MachineConfiguration(model);
        var settings = InputSettingsFunctions.Describe(configuration);
        var applied = InputSettingsFunctions.Apply(configuration, settings with { Keyboard = settings.Keyboard! with { Values = settings.Keyboard.Definitions.ToDictionary(item => item.Id, item => item.Id == target.ToString() ? string.Empty : item.DefaultBinding, StringComparer.Ordinal) } });
        var document = ConfigurationStoreFunctions.ToDocument(applied, Path.GetTempPath());
        var json = JsonSerializer.SerializeToElement(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var restored = ConfigurationStoreFunctions.FromDocument(ConfigurationStoreFunctions.Deserialize(json), Path.GetTempPath());
        Assert.Equal(string.Empty, InputSettingsFunctions.Describe(restored).Keyboard!.Values[target.ToString()]);
        var snapshot = InputSnapshotFunctions.Apply(Pressed(previous, EmulationKey.A, EmulationKey.Numpad1), restored.Input, model);
        Assert.DoesNotContain(target, snapshot.Keys);
        Assert.Contains(EmulationKey.A, snapshot.Keys);
        Assert.Contains(EmulationKey.Numpad1, snapshot.Keys);
        var native = KeyboardFunctions.CreateKeyMap();
        Assert.DoesNotContain(snapshot.Keys.Where(native.ContainsKey).Select(key => native[key]), code => code == native[target]);
    }
    [Theory]
    [InlineData(MachineModel.AppleII, true, 0, 1)]
    [InlineData(MachineModel.AppleIIPlus, true, 0, 1)]
    [InlineData(MachineModel.AppleIIJPlus, true, 0, 1)]
    [InlineData(MachineModel.AppleIIe, true, 2, 1)]
    [InlineData(MachineModel.AppleIIeEnhanced, true, 2, 1)]
    [InlineData(MachineModel.Macintosh128K, false, 3, 0)]
    [InlineData(MachineModel.Macintosh512K, false, 3, 0)]
    [InlineData(MachineModel.MacintoshPlus, false, 3, 0)]
    [InlineData(MachineModel.MacintoshClassic, false, 3, 0)]
    [InlineData(MachineModel.MacintoshII, true, 3, 0)]
    public void SelectionDescribesPhysicalModelWithoutClaimingUnsupportedRuntime(MachineModel model, bool executable, int keys, int ports)
    {
        var configuration = new MachineConfiguration(model);
        var blocks = SettingsDescriptionFunctions.Create(configuration);
        Assert.DoesNotContain(blocks.SelectMany(block => block.Fields), field => field.Editor == EmulationSettingsEditor.DirectoryPath || field.Id == SettingsConstants.ResourceUnavailableModel);
        var settings = InputSettingsFunctions.Describe(configuration);
        Assert.Equal(executable, ConfigurationFunctions.SupportsRuntime(model));
        Assert.Equal(executable, CoreCatalog.GetAll(model).Count > 0);
        Assert.Equal(keys, settings.Keyboard!.Definitions.Count);
        Assert.Equal(ports, settings.ControllerPorts.Count);
        Assert.DoesNotContain(settings.Keyboard.Definitions, definition => definition.Id is nameof(EmulationKey.A) or nameof(EmulationKey.LeftAmiga));
    }

    [Theory]
    [InlineData(MachineModel.AppleIIe, EmulationKey.AppleOpenApple, EmulationKey.LeftAlt)]
    [InlineData(MachineModel.AppleIIe, EmulationKey.AppleSolidApple, EmulationKey.RightAlt)]
    [InlineData(MachineModel.MacintoshII, EmulationKey.MacintoshCommand, EmulationKey.LeftAlt)]
    [InlineData(MachineModel.MacintoshII, EmulationKey.MacintoshKeypadClear, EmulationKey.Delete)]
    [InlineData(MachineModel.MacintoshII, EmulationKey.MacintoshKeypadEquals, EmulationKey.Insert)]
    public void ReassignmentSurvivesPersistenceAndRemovesNativeDefaultAlias(MachineModel model, EmulationKey target, EmulationKey previous)
    {
        var configuration = new MachineConfiguration(model);
        var settings = InputSettingsFunctions.Describe(configuration);
        var changed = settings with
        {
            Keyboard = settings.Keyboard! with
            {
                Values = new Dictionary<string, string> { [target.ToString()] = nameof(EmulationKey.Z) }
            }
        };
        var applied = InputSettingsFunctions.Apply(configuration, changed);
        var pathBase = Path.GetTempPath();
        var document = ConfigurationStoreFunctions.ToDocument(applied, pathBase);
        var json = JsonSerializer.SerializeToElement(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var restored = ConfigurationStoreFunctions.FromDocument(ConfigurationStoreFunctions.Deserialize(json), pathBase);
        Assert.Equal(nameof(EmulationKey.Z), InputSettingsFunctions.Describe(restored).Keyboard!.Values[target.ToString()]);
        var mapped = InputSnapshotFunctions.Apply(Pressed(EmulationKey.Z, EmulationKey.A), restored.Input, model);
        Assert.Contains(target, mapped.Keys);
        Assert.Contains(EmulationKey.A, mapped.Keys);
        var old = InputSnapshotFunctions.Apply(Pressed(previous), restored.Input, model);
        var codes = KeyboardFunctions.CreateKeyMap();
        Assert.DoesNotContain(old.Keys.Where(codes.ContainsKey).Select(key => codes[key]), code => code == codes[target]);
    }

    [Fact]
    public void MacintoshMouseButtonCanBeAssignedToKeyboardAndPersisted()
    {
        var configuration = new MachineConfiguration(MachineModel.MacintoshII);
        var settings = InputSettingsFunctions.Describe(configuration);
        var applied = InputSettingsFunctions.Apply(configuration, settings with
        {
            Mouse = settings.Mouse! with
            {
                Values = new Dictionary<string, string> { ["Left"] = "Keyboard:Space" }
            }
        });
        Assert.True(InputSnapshotFunctions.Apply(Pressed(EmulationKey.Space), applied.Input, applied.Model).Pointer.Left);
        var physical = EmulationInputSnapshot.Empty with
        {
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Left = true }
        };
        Assert.False(InputSnapshotFunctions.Apply(physical, applied.Input, applied.Model).Pointer.Left);
        Assert.Equal("Keyboard:Space", InputSettingsFunctions.Describe(applied).Mouse!.Values["Left"]);
    }

    [Fact]
    public void BothNativeJoystickButtonsAreAssignable()
    {
        var configuration = new MachineConfiguration(MachineModel.AppleIIe);
        var settings = InputSettingsFunctions.Describe(configuration);
        var port = Assert.Single(settings.ControllerPorts);
        Assert.Contains(port.Bindings.Definitions, definition => definition.Id == EmulationControllerCommandIds.Fire2);
        var applied = InputSettingsFunctions.Apply(configuration, settings with
        {
            ControllerPorts = [port with
            {
                Bindings = port.Bindings with
                {
                    Values = new Dictionary<string, string>
                    {
                        [EmulationControllerCommandIds.Fire2] = "Keyboard:Space"
                    }
                }
            }]
        });
        var input = InputSnapshotFunctions.Apply(Pressed(EmulationKey.Space), applied.Input, applied.Model);
        Assert.Equal(1u << EmulationInputMappingConstants.BIndex, input.Controllers[0].Buttons);
    }

    [Fact]
    public void SingleDiskFrontendRejectsMultipleMountedImages()
    {
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.MacintoshII, media: [new("floppy.dsk", MediaCategory.Floppy, EmulationMediaSlot.Floppy0), new("disk.img", MediaCategory.HardDisk, EmulationMediaSlot.HardDisk0)]));
    }

    [Fact]
    public void EmptyHardDiskDeviceSurvivesPersistenceAndCanBeRemoved()
    {
        var configuration = new MachineConfiguration(MachineModel.AppleIIe);
        var settings = StorageSettingsFunctions.Describe(configuration);
        var applied = StorageSettingsFunctions.Apply(configuration, settings with
        {
            ConfiguredSlots = settings.ConfiguredSlots.Append(EmulationMediaSlot.HardDisk0).ToArray()
        });
        var pathBase = Path.GetTempPath();
        var document = ConfigurationStoreFunctions.ToDocument(applied, pathBase);
        var json = JsonSerializer.SerializeToElement(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var restored = ConfigurationStoreFunctions.FromDocument(ConfigurationStoreFunctions.Deserialize(json), pathBase);
        var reloaded = StorageSettingsFunctions.Describe(restored);
        Assert.Contains(EmulationMediaSlot.HardDisk0, reloaded.ConfiguredSlots);
        Assert.Empty(reloaded.MountedMedia);
        var removed = StorageSettingsFunctions.Apply(restored, reloaded with
        {
            ConfiguredSlots = reloaded.ConfiguredSlots.Where(slot => slot != EmulationMediaSlot.HardDisk0).ToArray()
        });
        Assert.DoesNotContain(EmulationMediaSlot.HardDisk0, StorageSettingsFunctions.Describe(removed).ConfiguredSlots);
        Assert.Contains(EmulationMediaSlot.Floppy0, StorageSettingsFunctions.Describe(removed).ConfiguredSlots);
        Assert.Contains(EmulationMediaSlot.Floppy1, StorageSettingsFunctions.Describe(removed).ConfiguredSlots);
    }

    [Fact]
    public void EveryPhysicalMachineHasEmbeddedArtwork()
    {
        var resources = typeof(MachineCatalog).Assembly.GetManifestResourceNames();
        foreach (var machine in MachineCatalog.All)
            Assert.Contains(machine.ImageResourceName!, resources);
    }

    private static EmulationInputSnapshot Pressed(params EmulationKey[] keys) => EmulationInputSnapshot.Empty with { Keys = keys.ToHashSet() };
}
