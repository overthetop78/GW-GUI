using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

public static class AtariStorageConfigurationFunctions
{
    public static AtariMachineFamily Family(AtariMachineModel model) => model switch
    {
        AtariMachineModel.St or AtariMachineModel.Stf or AtariMachineModel.Stfm or AtariMachineModel.MegaSt
            or AtariMachineModel.Ste or AtariMachineModel.MegaSte or AtariMachineModel.Tt
            or AtariMachineModel.Falcon => AtariMachineFamily.St,
        AtariMachineModel.Atari400 or AtariMachineModel.Atari800 or AtariMachineModel.Atari800Xl
            or AtariMachineModel.Atari130Xe or AtariMachineModel.Xegs or AtariMachineModel.XlXe
            => AtariMachineFamily.EightBit,
        AtariMachineModel.Atari5200 => AtariMachineFamily.Atari5200,
        AtariMachineModel.Atari2600 => AtariMachineFamily.Atari2600,
        AtariMachineModel.Atari7800 => AtariMachineFamily.Atari7800,
        AtariMachineModel.Lynx => AtariMachineFamily.Lynx,
        AtariMachineModel.Jaguar or AtariMachineModel.JaguarCd => AtariMachineFamily.Jaguar,
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
    };

    public static (AtariMediaCategory Category, EmulationMediaSlot Slot)? PrimaryDevice(AtariMachineModel model) =>
        model switch
        {
            AtariMachineModel.Atari400 => null,
            AtariMachineModel.JaguarCd => (AtariMediaCategory.CompactDisc, EmulationMediaSlot.Cd0),
            AtariMachineModel.Atari2600 or AtariMachineModel.Atari5200 or AtariMachineModel.Atari7800
                or AtariMachineModel.Lynx or AtariMachineModel.Jaguar or AtariMachineModel.Xegs
                => (AtariMediaCategory.Cartridge, EmulationMediaSlot.Cartridge0),
            _ => (AtariMediaCategory.Floppy, EmulationMediaSlot.Floppy0)
        };

    public static bool IsRemovable(AtariMediaCategory category) => category is AtariMediaCategory.Floppy
        or AtariMediaCategory.Cassette or AtariMediaCategory.Cartridge or AtariMediaCategory.CompactDisc;

    public static bool IsPrimaryDevice(AtariMachineModel model, EmulationMediaSlot slot) =>
        PrimaryDevice(model)?.Slot == slot;
}
