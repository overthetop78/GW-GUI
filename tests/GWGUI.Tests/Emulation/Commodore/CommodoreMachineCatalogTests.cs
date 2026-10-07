using System.Net.Http;
using GWGUI.Emulation.Commodore.Common.Dictionaries;
using GWGUI.Emulation.Commodore.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Commodore.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Commodore.Common.Machines.Common.Enums;
using GWGUI.Emulation.Commodore.Common.Machines.Common.Functions;
using GWGUI.Emulation.Commodore.Modules;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Tests.Emulation.Commodore;

public sealed class CommodoreMachineCatalogTests
{
    public static IEnumerable<object[]> RequestedMachineProfiles()
    {
        yield return ["C16", new[] {"vice_xplus4"}];
        yield return ["C64", new[] {"frodo", "vice_x64", "vice_x64sc"}];
        yield return ["C64Dtv", new[] {"vice_x64dtv"}];
        yield return ["C64SuperCpu", new[] {"vice_xscpu64"}];
        yield return ["C128", new[] {"vice_x128"}];
        yield return ["CbmII510", new[] {"vice_xcbm5x0"}];
        foreach (var model in new[] {"CbmII610", "CbmII620", "CbmII620Plus", "CbmII710", "CbmII720", "CbmII720Plus"})
            yield return [model, new[] {"vice_xcbm2"}];
        foreach (var model in new[] {"Pet2001", "Pet3008", "Pet3016", "Pet3032", "Pet3032B", "Pet4016", "Pet4032", "Pet4032B", "Pet8032", "Pet8096", "Pet8296", "SuperPet"})
            yield return [model, new[] {"vice_xpet"}];
        foreach (var model in new[] {"Plus4", "V364", "C232"}) yield return [model, new[] {"vice_xplus4"}];
        foreach (var model in new[] {"Vic20", "Vic21"}) yield return [model, new[] {"vice_xvic"}];
    }

    [Theory]
    [MemberData(nameof(RequestedMachineProfiles))]
    public void RequestedMachinesHaveExactlyTheirProfilesAndGenericHardware(string machine, string[] profiles)
    {
        Assert.IsType<Model>(ModelCatalog.Get(machine));
        Assert.Equal(profiles.Order(), EmulatorCatalog.GetAll(machine).Select(profile => profile.Id).Order());
        Assert.Single(MachineCatalog.All, model => model.Id == machine);
        Assert.Null(typeof(Model).GetProperty("IsAmiga"));
        Assert.Null(typeof(Model).GetProperty("DefaultEmulator"));
    }

    [Fact]
    public void CatalogHasUniqueMachinesAndProfiles()
    {
        Assert.Equal(39, ModelCatalog.All.Count);
        Assert.Equal(39, ModelCatalog.All.Select(model => model.Id).Distinct().Count());
        Assert.Equal(14, EmulatorCatalog.All.Count);
        Assert.Equal(14, EmulatorCatalog.All.Select(profile => profile.Id).Distinct().Count());
        Assert.Equal(ModelCatalog.All.Select(model => model.Id).Order(), MachineCatalog.All.Select(model => model.Id).Order());
    }

    [Fact]
    public void SparseMediaSlotsAndReadOnlyFlagsSurviveConfigurationConversion()
    {
        EmulationMedia[] media =
        [new("disk", new(EmulationMediaCategory.FloppyDrive, 3), EmulationMediaType.Floppy, true, true),
         new("tape", new(EmulationMediaCategory.CassetteDrive, 0), EmulationMediaType.Cassette, false, true),
         new("game", new(EmulationMediaCategory.CartridgeSlot, 0), EmulationMediaType.Cartridge, true, true)];
        var restored = EmulationMediaConversionFunctions.ToCommon(EmulationMediaConversionFunctions.FromCommon(media));
        Assert.Equal(media.Select(item => item.Slot), restored.Select(item => item.Slot));
        Assert.Equal(media.Select(item => item.IsReadOnly), restored.Select(item => item.IsReadOnly));
    }

    [Fact]
    public async Task NonAmigaMediaPreparationDoesNotApplyAmigaScpConversion()
    {
        var configuration = new MachineConfiguration("C64", Emulator.ViceX64);
        var media = new EmulationMedia("unavailable.scp", EmulationMediaSlot.Floppy0,
            EmulationMediaType.Floppy, true, true);
        Assert.Same(media, await RuntimeMediaFunctions.PrepareMediaAsync(configuration, media, "unused"));
        Assert.Same(configuration, await RuntimeMediaFunctions.PrepareConfigurationAsync(configuration, "unused"));
    }

    [Fact]
    public async Task AmigaPreparationPreservesSlotsAndLeavesNonFloppyMediaUnchanged()
    {
        var configuration = new MachineConfiguration("A500", Emulator.PUAE,
            Media: [new MediaConfiguration("disk.adf", MediaCategory.Floppy, SlotIndex: 3),
                    new MediaConfiguration("hardfile.hdf", MediaCategory.HardDrive, SlotIndex: 1)]);
        var prepared = await RuntimeMediaFunctions.PrepareConfigurationAsync(configuration, "unused");
        Assert.Equal(configuration.Media, prepared.Media);
        var media = new EmulationMedia("hardfile.scp", EmulationMediaSlot.HardDisk0,
            EmulationMediaType.HardDisk, true, true);
        Assert.Same(media, await RuntimeMediaFunctions.PrepareMediaAsync(configuration, media, "unused"));
    }

    [Theory]
    [InlineData("C64", Emulator.Frodo, 4)]
    [InlineData("C128", Emulator.ViceX128, 6)]
    [InlineData("SuperPet", Emulator.ViceXPet, 10)]
    public void ExternalSystemRomsAreSeparateFromGameMedia(string model, Emulator core, int slotCount)
    {
        using var http = new HttpClient();
        var module = new CommodoreEmulationModule("virtual-config", "virtual-base", http, "virtual-core");
        var configuration = new MachineConfiguration(model, core);
        var fields = module.Describe(model, configuration).Blocks
            .Where(block => block.Tab == EmulationMachineTab.Rom).SelectMany(block => block.Fields).ToArray();
        Assert.Equal(slotCount, fields.Length);
        Assert.All(fields, field => Assert.Equal(EmulationSettingsEditor.Path, field.Editor));
        Assert.Equal(slotCount, fields.Select(field => field.Id).Distinct().Count());
        Assert.All(fields, field => Assert.Equal(EmulationDefaultFolderCategory.Firmware, field.DefaultFolderCategory));
        Assert.Empty(configuration.Media ?? []);
    }
}
