using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;
using GWGUI.Emulation.Acorn.Emulators.B2.Constants;
using GWGUI.Emulation.Acorn.Emulators.B2.Functions;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Dictionaries;

namespace GWGUI.Tests.Emulation.Acorn;

public sealed class AcornModuleTests
{
    [Theory]
    [InlineData(EmulationKey.AcornF0)]
    [InlineData(EmulationKey.AcornCopy)]
    [InlineData(EmulationKey.AcornShiftLock)]
    [InlineData(EmulationKey.AcornKeypadHash)]
    [InlineData(EmulationKey.AcornKeypadDelete)]
    [InlineData(EmulationKey.AcornKeypadComma)]
    public void ErasedAssociationSurvivesPersistenceWithoutRestoringTheDefault(EmulationKey target)
    {
        var configuration = new MachineConfiguration(MachineModel.BbcMaster128);
        var settings = InputSettingsFunctions.Describe(configuration);
        var values = settings.Keyboard!.Definitions.ToDictionary(item => item.Id, item => item.DefaultBinding, StringComparer.Ordinal);
        values[target.ToString()] = nameof(EmulationKey.Z);
        var assigned = InputSettingsFunctions.Apply(configuration, settings with { Keyboard = settings.Keyboard with { Values = values } });
        var assignedSettings = InputSettingsFunctions.Describe(assigned);
        values = new Dictionary<string, string>(assignedSettings.Keyboard!.Values) { [target.ToString()] = string.Empty };
        var erased = InputSettingsFunctions.Apply(assigned, assignedSettings with { Keyboard = assignedSettings.Keyboard with { Values = values } });
        var document = ConfigurationStoreFunctions.ToDocument(erased, Path.GetTempPath());
        var json = JsonSerializer.SerializeToElement(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var restored = ConfigurationStoreFunctions.FromDocument(ConfigurationStoreFunctions.Deserialize(json), Path.GetTempPath());
        Assert.Equal(string.Empty, InputSettingsFunctions.Describe(restored).Keyboard!.Values[target.ToString()]);
        var previous = settings.Keyboard.Definitions.Single(item => item.Id == target.ToString()).DefaultBinding;
        var keys = new HashSet<EmulationKey> { EmulationKey.Z, EmulationKey.A, EmulationKey.Numpad1 };
        if (Enum.TryParse<EmulationKey>(previous, out var old)) keys.Add(old);
        var mapped = InputSnapshotFunctions.Apply(EmulationInputSnapshot.Empty with { Keys = keys }, restored.Input);
        Assert.DoesNotContain(target, mapped.Keys);
        Assert.Contains(EmulationKey.A, mapped.Keys);
        Assert.Contains(EmulationKey.Numpad1, mapped.Keys);
        var native = KeyboardFunctions.CreateKeyMap();
        Assert.DoesNotContain(mapped.Keys.Where(native.ContainsKey).Select(key => native[key]), code => code == native[target]);
    }
    [Fact]
    public void MachineArtworkUsesExistingSharedResources()
    {
        var modelA = MachineCatalog.All.Single(machine => machine.Id == nameof(MachineModel.BbcMicroModelA));
        var modelB = MachineCatalog.All.Single(machine => machine.Id == nameof(MachineModel.BbcMicroModelB));
        var master = MachineCatalog.All.Single(machine => machine.Id == nameof(MachineModel.BbcMaster128));
        Assert.Equal(modelA.ImageResourceName, modelB.ImageResourceName);
        Assert.NotEqual(modelB.ImageResourceName, master.ImageResourceName);
        var resources = typeof(MachineCatalog).Assembly.GetManifestResourceNames();
        foreach (var machine in MachineCatalog.All)
            Assert.Contains(machine.ImageResourceName!, resources);
        Assert.DoesNotContain(resources, name => name.EndsWith(".BbcMicroModelA.png", StringComparison.Ordinal));
        Assert.DoesNotContain(resources, name => name.EndsWith(".BbcMicroModelB.png", StringComparison.Ordinal));
    }

    [Fact]
    public void ModelARemainsDistinctWithoutPretendingB2SupportsIt()
    {
        Assert.Equal(3, ModelCatalog.All.Count);
        var modelA = new MachineConfiguration(MachineModel.BbcMicroModelA);
        Assert.False(ConfigurationFunctions.SupportsRuntime(modelA.Model));
        Assert.Empty(CoreCatalog.GetAll(modelA.Model));
        Assert.Empty(B2ModelFunctions.GetModels(modelA.Model));
        Assert.DoesNotContain(SettingsDescriptionFunctions.Create(modelA).SelectMany(block => block.Fields), field => field.Tab == EmulationMachineTab.General);
        foreach (var model in Enum.GetValues<MachineModel>())
            Assert.DoesNotContain(SettingsDescriptionFunctions.Create(new MachineConfiguration(model)).SelectMany(block => block.Fields), field => field.Editor == EmulationSettingsEditor.DirectoryPath);
        Assert.True(ConfigurationFunctions.SupportsRuntime(MachineModel.BbcMicroModelB));
        Assert.True(ConfigurationFunctions.SupportsRuntime(MachineModel.BbcMaster128));
    }

    [Theory]
    [InlineData(MachineModel.BbcMicroModelA, 3)]
    [InlineData(MachineModel.BbcMicroModelB, 3)]
    [InlineData(MachineModel.BbcMaster128, 6)]
    public void KeyboardEditorContainsOnlyMachineSpecificKeys(MachineModel model, int count)
    {
        var keyboard = InputSettingsFunctions.Describe(new MachineConfiguration(model)).Keyboard!;
        Assert.Equal(count, keyboard.Definitions.Count);
        Assert.DoesNotContain(keyboard.Definitions, key => key.Id == nameof(EmulationKey.A));
        Assert.DoesNotContain(keyboard.Definitions, key => key.Id == nameof(EmulationKey.LeftAmiga));
        Assert.Equal(nameof(EmulationKey.F10), keyboard.Definitions.Single(key => key.Id == nameof(EmulationKey.AcornF0)).DefaultBinding);
    }

    [Theory]
    [InlineData(EmulationKey.AcornF0, EmulationKey.F10)]
    [InlineData(EmulationKey.AcornCopy, EmulationKey.Insert)]
    [InlineData(EmulationKey.AcornShiftLock, EmulationKey.PageDown)]
    public void CustomAssignmentReplacesDefaultThroughEditorAndPersistence(EmulationKey target, EmulationKey previous)
    {
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelB);
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
        var map = KeyboardFunctions.CreateKeyMap();
        var custom = InputSnapshotFunctions.Apply(Pressed(EmulationKey.Z), restored.Input);
        Assert.Contains(target, custom.Keys);
        Assert.True(map.ContainsKey(target));
        var old = InputSnapshotFunctions.Apply(Pressed(previous), restored.Input);
        Assert.DoesNotContain(target, old.Keys);
        Assert.DoesNotContain(old.Keys, map.ContainsKey);
    }

    [Fact]
    public void DefaultsWorkWithoutOverridingCommonKeys()
    {
        var mapped = InputSnapshotFunctions.Apply(Pressed(EmulationKey.F10, EmulationKey.A), new InputConfiguration());
        Assert.Contains(EmulationKey.AcornF0, mapped.Keys);
        Assert.Contains(EmulationKey.A, mapped.Keys);
        Assert.DoesNotContain(EmulationKey.F10, mapped.Keys);
    }

    [Fact]
    public void NativeModelCannotCrossBetweenModelBAndMaster()
    {
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.BbcMicroModelB, options: new Dictionary<string, string> { [OptionConstants.Model] = OptionConstants.Master128 }));
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.BbcMaster128, options: new Dictionary<string, string> { [OptionConstants.Model] = OptionConstants.ModelBAcorn1770 }));
    }

    [Fact]
    public void NativeDiskLimitRejectsUnsupportedSlotsAndCassette()
    {
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.BbcMicroModelB, media: [new("disk.ssd", MediaCategory.Floppy, EmulationMediaSlot.Floppy1)]));
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.BbcMicroModelB, media: [new("tape.uef", MediaCategory.Cassette, EmulationMediaSlot.Floppy0)]));
        var accepted = new MachineConfiguration(MachineModel.BbcMicroModelB, media: [new("disk.ssd", MediaCategory.Floppy, EmulationMediaSlot.Floppy0)]);
        Assert.Single(accepted.Media);
    }

    private static EmulationInputSnapshot Pressed(params EmulationKey[] keys) => EmulationInputSnapshot.Empty with { Keys = keys.ToHashSet() };
}
