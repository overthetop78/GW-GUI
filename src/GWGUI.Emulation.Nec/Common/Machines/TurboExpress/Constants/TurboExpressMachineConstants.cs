using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Nec.Common.Machines.TurboExpress.Constants;

internal static class TurboExpressMachineConstants
{
    internal const string Id = "TurboExpress";
    internal const string DisplayName = "TurboExpress / PC Engine GT";
    internal const string BackendModel = "pce";
    internal const int RamKib = 8;
    internal const bool HasKeyboard = false;
    internal const int BuiltInFloppyDriveCount = 0;
    internal const int MaximumFloppyDriveCount = 0;
    internal const bool HasBuiltInCassetteDrive = false;
    internal const bool SupportsCassetteDrive = false;
    internal const bool HasBuiltInCartridgeSlot = true;
    internal const bool SupportsCartridgeSlot = true;
    internal const int ControllerPortCount = 1;
    internal const int MouseButtonCount = 0;
    internal const string CpuName = "HuC6280";
    internal const string CpuClock = "7.16 MHz / 1.79 MHz";
    internal const bool HasBuiltInCdDrive = false;
    internal const bool SupportsCdDrive = false;

    internal static readonly Model Definition = new(Id, DisplayName, BackendModel, RamKib,
        HasKeyboard, BuiltInFloppyDriveCount, MaximumFloppyDriveCount,
        HasBuiltInCassetteDrive, SupportsCassetteDrive, HasBuiltInCartridgeSlot,
        SupportsCartridgeSlot, ControllerPortCount, MouseButtonCount, CpuName, CpuClock,
        HasBuiltInCdDrive, SupportsCdDrive);
}
