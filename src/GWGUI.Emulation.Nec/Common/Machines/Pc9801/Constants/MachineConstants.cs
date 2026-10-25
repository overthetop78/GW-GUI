using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nec.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Nec.Common.Machines.Pc9801.Constants;

internal static class MachineConstants
{
    internal const string Id = "Pc9801";
    internal const string DisplayName = "PC-9801";
    internal const string CpuName = "V30 / 80286";
    internal const string CpuClock = "5 MHz / 8 MHz";
    internal static Model Definition { get; } = new(Id, DisplayName, Id,
        (int)RamCapacity._640KB, HasKeyboard: true,
        BuiltInFloppyDriveCount: (int)DriveCount.Two,
        MaximumFloppyDriveCount: (int)DriveCount.Two,
        HasBuiltInCassetteDrive: false, SupportsCassetteDrive: false,
        HasBuiltInCartridgeSlot: false, SupportsCartridgeSlot: false,
        ControllerPortCount: (int)ControllerPortCount.Two,
        MouseButtonCount: (int)MouseButtonCount.Two,
        CpuName: CpuName, CpuClock: CpuClock,
        HasBuiltInCdDrive: false, SupportsCdDrive: false, SupportsHardDrives: true);
}
