using GWGUI.Emulation.Atari.Contracts;
using GWGUI.Emulation.Atari.Enums;
using GWGUI.Emulation.Atari.Functions;
using GWGUI.Emulation.Contracts;

namespace GWGUI.Tests.Emulation.MachineAdapters;

internal static class AtariCartridgeScenarios
{
    internal static void FiltersFollowTheSelectedModel()
    {
        AssertExtensions(AtariMachineModel.Atari800, ".bin", ".car", ".rom");
        AssertExtensions(AtariMachineModel.Atari5200, ".a52", ".bin", ".rom");
        AssertExtensions(AtariMachineModel.Atari2600, ".a26", ".bin");
        AssertExtensions(AtariMachineModel.Atari7800, ".a78", ".bin", ".cdf");
        AssertExtensions(AtariMachineModel.Lynx, ".bll", ".lnx", ".lyx", ".o");
        AssertExtensions(AtariMachineModel.Jaguar, ".abs", ".bin", ".cof", ".j64", ".jag", ".prg", ".rom");
        AssertExtensions(AtariMachineModel.JaguarCd, ".abs", ".bin", ".cof", ".j64", ".jag", ".prg", ".rom");
    }

    internal static void CartridgeIsTheInitialAtari800Content()
    {
        var floppy = new AtariMediaConfiguration("third.atr", AtariMediaCategory.Floppy,
            EmulationMediaSlot.Floppy0, MountOrder: 0);
        var cassette = new AtariMediaConfiguration("first.cas", AtariMediaCategory.Cassette,
            EmulationMediaSlot.Cassette0, MountOrder: 2);
        var cartridge = new AtariMediaConfiguration("second.car", AtariMediaCategory.Cartridge,
            EmulationMediaSlot.Cartridge0, MountOrder: 3);

        Assert.Equal(cartridge, Atari800MediaFunctions.Primary([floppy, cassette, cartridge]));
        Assert.Equal(cassette, Atari800MediaFunctions.Primary([floppy, cassette]));
        Assert.Equal(floppy, Atari800MediaFunctions.Primary([floppy]));
    }

    private static void AssertExtensions(AtariMachineModel model, params string[] expected)
    {
        var storage = AtariStorageSettingsFunctions.Describe(new AtariMachineConfiguration(model));
        var cartridge = Assert.Single(storage.AvailableDevices,
            device => device.Slot == EmulationMediaSlot.Cartridge0);
        Assert.Equal(expected, cartridge.AcceptedExtensions);
    }
}
