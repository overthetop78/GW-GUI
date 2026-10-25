using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Nec.Common.Dictionaries;
using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nec.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Nec.Common.Machines.Common.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using FirmwareFunctions = GWGUI.Emulation.Nec.Common.Machines.Common.Functions.FirmwareFunctions;

namespace GWGUI.Tests.Emulation.Nec;

public sealed class NecEmulatorCatalogTests
{
    [Theory]
    [InlineData("Pc8001", "quasi88")]
    [InlineData("Pc8801", "quasi88")]
    [InlineData("Pc8801MkII", "quasi88")]
    [InlineData("Pc8801MkIISR", "quasi88")]
    [InlineData("Pc9801", "neko_project_ii,np2kai")]
    [InlineData("Pc9821", "neko_project_ii,np2kai")]
    [InlineData("PcEngine", "beetle_pce,beetle_pce_fast,geargrafx")]
    [InlineData("PcEngineCd", "beetle_pce,beetle_pce_fast,geargrafx")]
    [InlineData("PcEngineSuperCd", "beetle_pce,beetle_pce_fast,geargrafx")]
    [InlineData("PcEngineArcadeCard", "beetle_pce,beetle_pce_fast")]
    [InlineData("SuperGrafx", "beetle_pce,beetle_sgx,geargrafx")]
    [InlineData("PcFx", "beetle_pcfx_fast")]
    public void RequestedMachinesExposeExactlyTheirEmulators(string machine, string ids)
    {
        Assert.NotNull(ModelCatalog.Get(machine));
        Assert.Equal(ids.Split(',').Order(), EmulatorCatalog.GetAll(machine).Select(item => item.Id).Order());
        Assert.Contains(EmulatorCatalog.DefaultEmulator(machine), ids.Split(','));
    }

    [Theory]
    [InlineData("PcEngine", "beetle_pce_fast")]
    [InlineData("SuperGrafx", "beetle_sgx")]
    [InlineData("PcFx", "beetle_pcfx_fast")]
    [InlineData("LaserActive", "geargrafx")]
    public void ExistingDefaultEmulatorsArePreserved(string machine, string emulator) => Assert.Equal(emulator, EmulatorCatalog.DefaultEmulator(machine));

    [Theory]
    [InlineData("beetle_pce", 42)]
    [InlineData("quasi88", 9)]
    [InlineData("neko_project_ii", 17)]
    [InlineData("np2kai", 46)]
    public void NewCoreOptionsMatchCompiledCoreDefinitions(string emulator, int count)
    {
        var options = CoreSettingsFunctions.Definitions(emulator);
        Assert.Equal(count, options.Count);
        Assert.Equal(count, options.Select(option => option.Key).Distinct().Count());
        foreach (var option in options)
            Assert.Contains(option.Choices, choice => choice.Value == option.DefaultValue);
    }

    [Theory]
    [InlineData("Pc8001", "N")]
    [InlineData("Pc8801", "N88 V1S")]
    [InlineData("Pc8801MkII", "N88 V1H")]
    [InlineData("Pc8801MkIISR", "N88 V2")]
    public void QuasiBasicModeFollowsSelectedMachine(string machine, string basicMode)
    {
        var configured = CoreSettingsFunctions.Configure(new MachineConfiguration(machine, "quasi88"));
        Assert.Equal(basicMode, configured.Options!["q88_basic_mode"]);
    }

    [Theory]
    [InlineData("PcEngine", "beetle_pce_fast", false)]
    [InlineData("PcEngine", "beetle_pce_fast", true)]
    [InlineData("PcFx", "beetle_pcfx_fast", false)]
    [InlineData("PcFx", "beetle_pcfx_fast", true)]
    public void RuntimeOptionSnapshotsKeepOnlyExplicitOptionsAndNeverShareTheSourceDictionary(
        string machine, string emulator, bool noOptions)
    {
        var options = noOptions ? null : new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["native_option"] = "first",
            ["NATIVE_OPTION"] = "second"
        };
        var configuration = new MachineConfiguration(machine, emulator, options,
            Id: Guid.Parse("10101010-2020-3030-4040-505050505050"), AudioEnabled: false);
        var snapshot = emulator == "beetle_pce_fast"
            ? GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Functions.BeetlePceFastOptionFunctions.ToNative(configuration)
            : GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Functions.BeetlePcfxOptionFunctions.ToNative(configuration);
        Assert.NotSame(configuration, snapshot);
        Assert.Equal(configuration with { Options = snapshot.Options }, snapshot);
        Assert.NotNull(snapshot.Options);
        Assert.NotSame(options, snapshot.Options);
        Assert.Equal(noOptions ? 0 : 2, snapshot.Options.Count);
        if (!noOptions)
        {
            Assert.Equal("first", snapshot.Options["native_option"]);
            Assert.Equal("second", snapshot.Options["NATIVE_OPTION"]);
        }
        Assert.IsAssignableFrom<IDictionary<string, string>>(snapshot.Options).Add("later", "value");
        Assert.False(options?.ContainsKey("later") ?? false);
    }

    [Theory]
    [InlineData("quasi88", 13)]
    [InlineData("neko_project_ii", 11)]
    [InlineData("np2kai", 11)]
    public void FirmwareSlotsUseRoleLabelsAndKeepUserPaths(string emulator, int count)
    {
        var adapter = Assert.Single(EmulatorCatalog.CreateAdapters(), item => item.EmulatorId == emulator);
        var configuration = new MachineConfiguration(adapter.Definition.MachineIds.First(), emulator);
        var slots = FirmwareFunctions.Slots(configuration);
        Assert.Equal(count, slots.Count);
        Assert.All(slots, slot => Assert.StartsWith("Emulation.Nec.Firmware.", slot.LabelResourceKey));
        configuration = configuration with { FirmwarePaths = slots.ToDictionary(slot => slot.FieldId, slot => slot.FieldId + ".rom") };
        var fields = GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.FirmwareFunctions.Fields(configuration);
        Assert.Equal(slots.Select(slot => slot.FieldId), fields.Select(field => field.Id));
        Assert.Equal(slots.Select(slot => configuration.FirmwarePaths[slot.FieldId]), fields.Select(field => field.Value));
        var paths = slots.Take(2).ToDictionary(slot => slot.FieldId, slot => (string?)$"renamed-{slot.FileName}");
        var configured = FirmwareFunctions.Apply(configuration, paths);
        Assert.All(paths, path => Assert.Equal(path.Value, configured.FirmwarePaths![path.Key]));
    }

    [Theory]
    [InlineData("Pc8801", "quasi88", 2, 0)]
    [InlineData("Pc9801", "neko_project_ii", 2, 2)]
    [InlineData("Pc9821", "np2kai", 2, 2)]
    public void ComputerStorageSeparatesFloppyDrivesAndHardDisks(string machine, string emulator, int floppies, int hardDisks)
    {
        var settings = StorageSettingsFunctions.Describe(new MachineConfiguration(machine, emulator));
        Assert.Equal(floppies, settings.AvailableDevices.Count(device => device.MediaType == EmulationMediaType.Floppy));
        Assert.Equal(hardDisks, settings.AvailableDevices.Count(device => device.MediaType == EmulationMediaType.HardDisk));
        Assert.All(settings.AvailableDevices, device => Assert.DoesNotContain(".m3u", device.AcceptedExtensions));
    }
}
