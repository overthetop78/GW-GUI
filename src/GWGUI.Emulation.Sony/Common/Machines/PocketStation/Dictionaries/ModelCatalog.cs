using Hardware = GWGUI.Emulation.Sony.Common.Machines.PocketStation.Constants.ModelConstants;

namespace GWGUI.Emulation.Sony.Common.Machines.PocketStation.Dictionaries;

internal static class ModelCatalog
{
    internal static Model PocketStation { get; } = new(
        Hardware.Id, Hardware.DisplayName, Hardware.BackendModel, (int)RamCapacity._2KB,
        HasKeyboard: false,
        BuiltInFloppyDriveCount: Hardware.NoFloppyDrive,
        MaximumFloppyDriveCount: Hardware.NoFloppyDrive,
        HasBuiltInCassetteDrive: false, SupportsCassetteDrive: false,
        HasBuiltInCartridgeSlot: false, SupportsCartridgeSlot: false,
        ControllerPortCount: Hardware.IntegratedControllerCount,
        CpuModels: [Hardware.Cpu], VideoChip: Hardware.Display, AudioChip: Hardware.Audio,
        RomKib: Hardware.BiosCapacityKib, CpuFrequency: Hardware.MaximumCpuFrequency,
        HasBuiltInMemoryCard: true);
}
