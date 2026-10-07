using ModelConstants = GWGUI.Emulation.Commodore.Common.Machines.Plus4.Constants.ModelConstants;
using GWGUI.Emulation.Commodore.Common.Machines.Plus4.Constants;

namespace GWGUI.Emulation.Commodore.Common.Machines.Plus4.Dictionaries;

internal static class ModelCatalog
{
    internal static Model C16 { get; } = new(
        ModelConstants.C16, ModelConstants.C16DisplayName,
        [CpuModel.Mos7501], [ChipsetModel.TED], (int)RamCapacity._16KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None]);

    internal static Model Plus4 { get; } = new(
        ModelConstants.Plus4, ModelConstants.Plus4DisplayName,
        [CpuModel.Mos7501], [ChipsetModel.TED], (int)RamCapacity._64KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None]);

    internal static Model V364 { get; } = new(
        ModelConstants.V364, ModelConstants.V364DisplayName,
        [CpuModel.Mos7501], [ChipsetModel.TED], (int)RamCapacity._64KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None]);

    internal static Model C232 { get; } = new(
        ModelConstants.C232, ModelConstants.C232DisplayName,
        [CpuModel.Mos7501], [ChipsetModel.TED], (int)RamCapacity._32KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None]);

    internal static IReadOnlyList<Model> All { get; } =
    [
        C16,
        Plus4,
        V364,
        C232
    ];
}
