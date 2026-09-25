using GWGUI.Emulation.Atari.Common.Contracts;
using GWGUI.Emulation.Atari.Common.Enums;
using GWGUI.Emulation.Atari.Common.Functions;
using GWGUI.Emulation.Contracts;

namespace GWGUI.Tests.Emulation.MachineAdapters;

internal static class AtariCartridgeScenarios
{
    internal static void FiltersFollowTheSelectedModel()
    {
        AssertExtensions(MachineModel.Atari800, ".bin", ".car", ".rom");
        AssertExtensions(MachineModel.Atari5200, ".a52", ".bin", ".rom");
        AssertExtensions(MachineModel.Atari2600, ".a26", ".bin");
        AssertExtensions(MachineModel.Atari7800, ".a78", ".bin", ".cdf");
        AssertExtensions(MachineModel.Lynx, ".bll", ".lnx", ".lyx", ".o");
        AssertExtensions(MachineModel.Jaguar, ".abs", ".bin", ".cof", ".j64", ".jag", ".prg", ".rom");
        AssertExtensions(MachineModel.JaguarCd, ".abs", ".bin", ".cof", ".j64", ".jag", ".prg", ".rom");
    }

    internal static void CartridgeIsTheInitialAtari800Content()
    {
        var floppy = new MediaConfiguration("third.atr", MediaCategory.Floppy,
            EmulationMediaSlot.Floppy0, MountOrder: 0);
        var cassette = new MediaConfiguration("first.cas", MediaCategory.Cassette,
            EmulationMediaSlot.Cassette0, MountOrder: 2);
        var cartridge = new MediaConfiguration("second.car", MediaCategory.Cartridge,
            EmulationMediaSlot.Cartridge0, MountOrder: 3);

        Assert.Equal(cartridge, Atari800MediaFunctions.Primary([floppy, cassette, cartridge]));
        Assert.Equal(cassette, Atari800MediaFunctions.Primary([floppy, cassette]));
        Assert.Equal(floppy, Atari800MediaFunctions.Primary([floppy]));
    }

    private static void AssertExtensions(MachineModel model, params string[] expected)
    {
        var storage = StorageSettingsFunctions.Describe(new MachineConfiguration(model));
        var cartridge = Assert.Single(storage.AvailableDevices,
            device => device.Slot == EmulationMediaSlot.Cartridge0);
        Assert.Equal(expected, cartridge.AcceptedExtensions);
    }
}
