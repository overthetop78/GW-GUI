using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

public static class StorageConfigurationFunctions
{
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

    public static bool IsRemovable(MediaCategory category) => category is MediaCategory.Floppy or MediaCategory.Cassette or MediaCategory.Cartridge or MediaCategory.CompactDisc;

    public static bool IsPrimaryDevice(MachineModel model, EmulationMediaSlot slot) => PrimaryDevice(model)?.Slot == slot;
}
