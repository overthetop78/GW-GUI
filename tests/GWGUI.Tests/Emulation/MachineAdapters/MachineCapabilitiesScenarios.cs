using GWGUI.Emulation.Atari.Common.Enums;
using GWGUI.Emulation.Atari.Common.Contracts;
using GWGUI.Emulation.Amiga.Common.Dictionaries;
using AtariMachineConfiguration = GWGUI.Emulation.Atari.Common.Contracts.MachineConfiguration;
using AtariModelCatalog = GWGUI.Emulation.Atari.Common.Dictionaries.ModelCatalog;
using AmigaModelCatalog = GWGUI.Emulation.Amiga.Common.Dictionaries.ModelCatalog;
namespace GWGUI.Tests.Emulation.MachineAdapters;
internal static class MachineCapabilitiesScenarios
{
    public static void Atari(string model, string family, string adapter)
    {
        var parsed = AtariModelCatalog.Parse(model);
        var configuration = new AtariMachineConfiguration(parsed);
        Assert.Equal(model, configuration.MachineId); Assert.Equal(family, configuration.Family.ToString()); Assert.Equal(adapter, configuration.Core.ToString());
        Assert.Contains(AtariModelCatalog.All, item => item.Id == model);
        var storage = GWGUI.Emulation.Atari.Common.Functions.StorageSettingsFunctions.Describe(configuration);
        Assert.Equal(storage.AvailableDevices.Count, storage.AvailableDevices.Select(d=>d.Slot).Distinct().Count());
        foreach(var device in storage.AvailableDevices)
        {
            var media = GWGUI.Emulation.Atari.Common.Functions.EmulationMediaConversionFunctions.ToAtari(new("virtual-media",device.Slot,device.MediaType,false,true),[]);
            var accepted = new AtariMachineConfiguration(parsed,media:[media]);
            Assert.Equal(media,Assert.Single(accepted.Media));
            Assert.Throws<ArgumentException>(()=>new AtariMachineConfiguration(parsed,media:[media,media]));
            Assert.Throws<ArgumentException>(()=>new AtariMachineConfiguration(parsed,media:[media with {Path=" "}]));
            Assert.Throws<ArgumentException>(()=>new AtariMachineConfiguration(parsed,media:[media with {Slot=new(device.Slot.Category,99)}]));
        }
    }
    public static void Amiga(string model, string cpu, string chipset, int chipMemory)
    {
        var definition = AmigaModelCatalog.Get(model);
        Assert.Equal(cpu, definition.DefaultCpu); Assert.Equal(chipset, definition.Chipset); Assert.Equal(chipMemory, definition.ChipMemoryKib);
        Assert.Equal(2, definition.ControllerPortCount);
        Assert.Contains(MachineCatalog.All, item => item.Id == model);
        var configuration = new GWGUI.Emulation.Amiga.Common.Contracts.MachineConfiguration(model,"virtual-rom",Options:new Dictionary<string,string>{{"gwgui_floppy_drive_count","99"},{"gwgui_hard_drive_count","99"}});
        var storage = GWGUI.Emulation.Amiga.Common.Functions.StorageSettingsFunctions.Describe(configuration);
        Assert.Equal(definition.MaximumFloppyDrives,storage.ConfiguredSlots.Count(s=>s.Category==GWGUI.Emulation.Enums.EmulationMediaCategory.FloppyDrive));
        Assert.Equal(definition.MaximumHardDrives,storage.ConfiguredSlots.Count(s=>s.Category==GWGUI.Emulation.Enums.EmulationMediaCategory.HardDisk));
        Assert.Equal(definition.HasCdDrive,storage.ConfiguredSlots.Contains(GWGUI.Emulation.Contracts.EmulationMediaSlot.Cd0));
        var empty = GWGUI.Emulation.Amiga.Common.Functions.StorageSettingsFunctions.Describe(configuration with { Options=new Dictionary<string,string>{{"gwgui_floppy_drive_count","-1"},{"gwgui_hard_drive_count","-1"}} });
        Assert.All(empty.ConfiguredSlots,slot=>Assert.Equal(GWGUI.Emulation.Contracts.EmulationMediaSlot.Cd0,slot));
    }
    public static void Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AtariModelCatalog.Parse("absent"));
        Assert.Throws<ArgumentOutOfRangeException>(() => AmigaModelCatalog.Get("absent"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AtariMachineConfiguration(MachineModel.St, schemaVersion: -1));
        var firmware = new FirmwareConfiguration(FirmwareCategory.Tos, "virtual", true);
        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(MachineModel.St, firmwares: [firmware, firmware]));
        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(MachineModel.Atari2600, firmwares: [firmware]));
        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(MachineModel.St, firmwares: [firmware with { Path = " " }]));
    }
}
