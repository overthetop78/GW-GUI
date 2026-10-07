using ModelConstants = GWGUI.Emulation.Commodore.Common.Machines.C64.Constants.ModelConstants;
using GWGUI.Emulation.Commodore.Common.Machines.C64.Constants;

namespace GWGUI.Emulation.Commodore.Common.Machines.C64.Dictionaries;

internal static class ModelCatalog
{
    internal static Model C64 { get; } = new(
        ModelConstants.C64, ModelConstants.C64DisplayName,
        [CpuModel.Mos6510], [ChipsetModel.VICII, ChipsetModel.SID], (int)RamCapacity._64KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None]);

    internal static Model C64Dtv { get; } = new(
        ModelConstants.C64Dtv, ModelConstants.C64DtvDisplayName,
        [CpuModel.Mos6510], [ChipsetModel.DTV], (int)RamCapacity._2MB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None]);

    internal static Model C64SuperCpu { get; } = new(
        ModelConstants.C64SuperCpu, ModelConstants.C64SuperCpuDisplayName,
        [CpuModel.Wdc65816], [ChipsetModel.VICII, ChipsetModel.SID], (int)RamCapacity._64KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None]);

    internal static IReadOnlyList<Model> All { get; } =
    [
        C64,
        C64Dtv,
        C64SuperCpu
    ];
}
