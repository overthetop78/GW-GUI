using ModelConstants = GWGUI.Emulation.Commodore.Common.Machines.CbmII.Constants.ModelConstants;
using GWGUI.Emulation.Commodore.Common.Machines.CbmII.Constants;

namespace GWGUI.Emulation.Commodore.Common.Machines.CbmII.Dictionaries;

internal static class ModelCatalog
{
    internal static Model CbmII510 { get; } = new(
        ModelConstants.CbmII510, ModelConstants.CbmII510DisplayName,
        [CpuModel.Mos6509], [ChipsetModel.VICII, ChipsetModel.SID], (int)RamCapacity._64KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.CbmII510JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None]);

    internal static Model CbmII610 { get; } = new(
        ModelConstants.CbmII610, ModelConstants.CbmII610DisplayName,
        [CpuModel.Mos6509], [ChipsetModel.CRTC, ChipsetModel.SID], (int)RamCapacity._128KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.BusinessMachineJoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model CbmII620 { get; } = new(
        ModelConstants.CbmII620, ModelConstants.CbmII620DisplayName,
        [CpuModel.Mos6509], [ChipsetModel.CRTC, ChipsetModel.SID], (int)RamCapacity._256KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.BusinessMachineJoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model CbmII620Plus { get; } = new(
        ModelConstants.CbmII620Plus, ModelConstants.CbmII620PlusDisplayName,
        [CpuModel.Mos6509], [ChipsetModel.CRTC, ChipsetModel.SID], (int)RamCapacity._256KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.BusinessMachineJoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model CbmII710 { get; } = new(
        ModelConstants.CbmII710, ModelConstants.CbmII710DisplayName,
        [CpuModel.Mos6509], [ChipsetModel.CRTC, ChipsetModel.SID], (int)RamCapacity._128KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.BusinessMachineJoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model CbmII720 { get; } = new(
        ModelConstants.CbmII720, ModelConstants.CbmII720DisplayName,
        [CpuModel.Mos6509], [ChipsetModel.CRTC, ChipsetModel.SID], (int)RamCapacity._256KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.BusinessMachineJoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model CbmII720Plus { get; } = new(
        ModelConstants.CbmII720Plus, ModelConstants.CbmII720PlusDisplayName,
        [CpuModel.Mos6509], [ChipsetModel.CRTC, ChipsetModel.SID], (int)RamCapacity._256KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.BusinessMachineJoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static IReadOnlyList<Model> All { get; } =
    [
        CbmII510,
        CbmII610,
        CbmII620,
        CbmII620Plus,
        CbmII710,
        CbmII720,
        CbmII720Plus
    ];
}
