using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nec.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Nec.Common.Machines.Pc8801.Constants;

internal static class MachineConstants
{
    internal const string Id = "Pc8801";
    internal const string DisplayName = "PC-8801";
    internal const string MkIIId = "Pc8801MkII";
    internal const string MkIISrId = "Pc8801MkIISR";
    internal const string MkIIName = "PC-8801mkII";
    internal const string MkIISrName = "PC-8801mkIISR";
    internal const string CpuName = "Z80";
    internal const string CpuClock = "4 MHz";
    internal static Model Definition { get; } = new(Id, DisplayName, Id,
        (int)RamCapacity._64KB, HasKeyboard: true,
        BuiltInFloppyDriveCount: (int)DriveCount.None,
        MaximumFloppyDriveCount: (int)DriveCount.Two,
        HasBuiltInCassetteDrive: false, SupportsCassetteDrive: false,
        HasBuiltInCartridgeSlot: false, SupportsCartridgeSlot: false,
        ControllerPortCount: (int)ControllerPortCount.Two,
        MouseButtonCount: (int)MouseButtonCount.None,
        CpuName: CpuName, CpuClock: CpuClock,
        HasBuiltInCdDrive: false, SupportsCdDrive: false);
}
