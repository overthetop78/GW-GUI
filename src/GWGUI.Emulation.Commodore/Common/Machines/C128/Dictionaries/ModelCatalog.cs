using ModelConstants = GWGUI.Emulation.Commodore.Common.Machines.C128.Constants.ModelConstants;
using GWGUI.Emulation.Commodore.Common.Machines.C128.Constants;

namespace GWGUI.Emulation.Commodore.Common.Machines.C128.Dictionaries;

internal static class ModelCatalog
{
    internal static Model C128 { get; } = new(
        ModelConstants.C128, ModelConstants.C128DisplayName,
        [CpuModel.Mos8502, CpuModel.ZilogZ80], [ChipsetModel.VICII, ChipsetModel.VDC, ChipsetModel.SID], (int)RamCapacity._128KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None]);

    internal static IReadOnlyList<Model> All { get; } =
    [
        C128
    ];
}
