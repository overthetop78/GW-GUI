using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Nec.Common.Dictionaries;
using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nec.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Nec.Common.Machines.Common.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;

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
        Assert.Contains(EmulatorCatalog.DefaultFor(machine), ids.Split(','));
    }

    [Theory]
    [InlineData("PcEngine", "beetle_pce_fast")]
    [InlineData("SuperGrafx", "beetle_sgx")]
    [InlineData("PcFx", "beetle_pcfx_fast")]
    [InlineData("LaserActive", "geargrafx")]
    public void ExistingDefaultEmulatorsArePreserved(string machine, string emulator) =>
        Assert.Equal(emulator, EmulatorCatalog.DefaultFor(machine));

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
    [InlineData("quasi88", 13)]
    [InlineData("neko_project_ii", 11)]
    [InlineData("np2kai", 11)]
    public void FirmwareSlotsUseRoleLabelsAndKeepUserPaths(string emulator, int count)
    {
        var slots = FirmwareFunctions.Slots(emulator);
        Assert.Equal(count, slots.Count);
        Assert.All(slots, slot => Assert.StartsWith("Emulation.Nec.Firmware.", slot.LabelResourceKey));
        var paths = slots.Take(2).ToDictionary(slot => slot.FieldId, slot => (string?)$"renamed-{slot.FileName}");
        var configured = FirmwareFunctions.Apply(new MachineConfiguration("Pc8801", emulator), paths);
        Assert.All(paths, path => Assert.Equal(path.Value, configured.FirmwarePaths![path.Key]));
    }

    [Theory]
    [InlineData("Pc8801", "quasi88", 2, 0)]
    [InlineData("Pc9801", "neko_project_ii", 2, 2)]
    [InlineData("Pc9821", "np2kai", 2, 2)]
    public void ComputerStorageSeparatesFloppyDrivesAndHardDisks(string machine, string emulator,
        int floppies, int hardDisks)
    {
        var settings = StorageSettingsFunctions.Describe(new MachineConfiguration(machine, emulator));
        Assert.Equal(floppies, settings.AvailableDevices.Count(device => device.MediaType == EmulationMediaType.Floppy));
        Assert.Equal(hardDisks, settings.AvailableDevices.Count(device => device.MediaType == EmulationMediaType.HardDisk));
        Assert.All(settings.AvailableDevices, device => Assert.DoesNotContain(".m3u", device.AcceptedExtensions));
    }
}
