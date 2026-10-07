using ModelConstants = GWGUI.Emulation.Commodore.Common.Machines.Vic20.Constants.ModelConstants;
using GWGUI.Emulation.Commodore.Common.Machines.Vic20.Constants;

namespace GWGUI.Emulation.Commodore.Common.Machines.Vic20.Dictionaries;

internal static class ModelCatalog
{
    internal static Model Vic20 { get; } = new(
        ModelConstants.Vic20, ModelConstants.Vic20DisplayName,
        [CpuModel.Mos6502], [ChipsetModel.VIC], (int)RamCapacity._5KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None]);

    internal static Model Vic21 { get; } = new(
        ModelConstants.Vic21, ModelConstants.Vic21DisplayName,
        [CpuModel.Mos6502], [ChipsetModel.VIC], (int)RamCapacity._21KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None]);

    internal static IReadOnlyList<Model> All { get; } =
    [
        Vic20,
        Vic21
    ];
}
