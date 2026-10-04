using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;

internal static class PcEngineDuoMachineConstants
{
    internal const string Id = "PcEngineDuo";
    internal const string DisplayName = "PC Engine Duo / TurboDuo";
    internal const string BackendModel = "pcecd";
    internal const int RamKib = 8;
    internal const bool HasKeyboard = false;
    internal const int BuiltInFloppyDriveCount = 0;
    internal const int MaximumFloppyDriveCount = 0;
    internal const bool HasBuiltInCassetteDrive = false;
    internal const bool SupportsCassetteDrive = false;
    internal const bool HasBuiltInCartridgeSlot = true;
    internal const bool SupportsCartridgeSlot = true;
    internal const int ControllerPortCount = 5;
    internal const int MouseButtonCount = 2;
    internal const string CpuName = "HuC6280";
    internal const string CpuClock = "7.16 MHz / 1.79 MHz";
    internal const bool HasBuiltInCdDrive = true;
    internal const bool SupportsCdDrive = true;
    internal const int CdMemoryBlockPosition = 3;
    internal const string CdMemoryBlockId = "cd-memory";
    internal const string CdRamSuffix = ".cdRam";
    internal const string CdRamResourceKey = "Emulation.Nec.Memory.CdRam";
    internal const string CdRamDisplay = "256 KiB";

    internal static readonly Model Definition = new(Id, DisplayName, BackendModel, RamKib,
        HasKeyboard, BuiltInFloppyDriveCount, MaximumFloppyDriveCount,
        HasBuiltInCassetteDrive, SupportsCassetteDrive, HasBuiltInCartridgeSlot,
        SupportsCartridgeSlot, ControllerPortCount, MouseButtonCount, CpuName, CpuClock,
        HasBuiltInCdDrive, SupportsCdDrive);
}
