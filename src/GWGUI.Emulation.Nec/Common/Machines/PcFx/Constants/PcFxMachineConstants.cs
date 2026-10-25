using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Nec.Common.Machines.PcFx.Constants;

internal static class PcFxMachineConstants
{
    internal const string Id = "PcFx";
    internal const string DisplayName = "PC-FX";
    internal const string BackendModel = "pcfx";
    internal const int RamKib = 2048;
    internal const bool HasKeyboard = false;
    internal const int BuiltInFloppyDriveCount = 0;
    internal const int MaximumFloppyDriveCount = 0;
    internal const bool HasBuiltInCassetteDrive = false;
    internal const bool SupportsCassetteDrive = false;
    internal const bool HasBuiltInCartridgeSlot = false;
    internal const bool SupportsCartridgeSlot = false;
    internal const int ControllerPortCount = 2;
    internal const int MouseButtonCount = 2;
    internal const string CpuName = "V810";
    internal const string CpuClock = "21.48 MHz";
    internal const bool HasBuiltInCdDrive = true;
    internal const bool SupportsCdDrive = true;

    internal static readonly Model Definition = new(Id, DisplayName, BackendModel, RamKib,
        HasKeyboard, BuiltInFloppyDriveCount, MaximumFloppyDriveCount,
        HasBuiltInCassetteDrive, SupportsCassetteDrive, HasBuiltInCartridgeSlot,
        SupportsCartridgeSlot, ControllerPortCount, MouseButtonCount, CpuName, CpuClock,
        HasBuiltInCdDrive, SupportsCdDrive);
}
