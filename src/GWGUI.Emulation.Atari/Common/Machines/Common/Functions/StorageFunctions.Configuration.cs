using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

public static class StorageConfigurationFunctions
{
    public static MachineFamily Family(MachineModel model) => model switch
    {
        MachineModel.St or MachineModel.Stf or MachineModel.Stfm or MachineModel.MegaSt
            or MachineModel.Ste or MachineModel.MegaSte or MachineModel.Tt
            or MachineModel.Falcon => MachineFamily.St,
        MachineModel.Atari400 or MachineModel.Atari800 or MachineModel.Atari800Xl
            or MachineModel.Atari130Xe or MachineModel.Xegs or MachineModel.XlXe
            => MachineFamily.EightBit,
        MachineModel.Atari5200 => MachineFamily.Atari5200,
        MachineModel.Atari2600 => MachineFamily.Atari2600,
        MachineModel.Atari7800 => MachineFamily.Atari7800,
        MachineModel.Lynx => MachineFamily.Lynx,
        MachineModel.Jaguar or MachineModel.JaguarCd => MachineFamily.Jaguar,
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
    };

    public static (MediaCategory Category, EmulationMediaSlot Slot)? PrimaryDevice(MachineModel model) =>
        model switch
        {
            MachineModel.Atari400 => null,
            MachineModel.JaguarCd => (MediaCategory.CompactDisc, EmulationMediaSlot.Cd0),
            MachineModel.Atari2600 or MachineModel.Atari5200 or MachineModel.Atari7800
                or MachineModel.Lynx or MachineModel.Jaguar or MachineModel.Xegs
                => (MediaCategory.Cartridge, EmulationMediaSlot.Cartridge0),
            _ => (MediaCategory.Floppy, EmulationMediaSlot.Floppy0)
        };

    public static bool IsRemovable(MediaCategory category) => category is MediaCategory.Floppy
        or MediaCategory.Cassette or MediaCategory.Cartridge or MediaCategory.CompactDisc;

    public static bool IsPrimaryDevice(MachineModel model, EmulationMediaSlot slot) =>
        PrimaryDevice(model)?.Slot == slot;
}
