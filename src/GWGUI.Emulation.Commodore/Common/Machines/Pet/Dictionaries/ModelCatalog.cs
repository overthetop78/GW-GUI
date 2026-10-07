using ModelConstants = GWGUI.Emulation.Commodore.Common.Machines.Pet.Constants.ModelConstants;
using GWGUI.Emulation.Commodore.Common.Machines.Pet.Constants;

namespace GWGUI.Emulation.Commodore.Common.Machines.Pet.Dictionaries;

internal static class ModelCatalog
{
    internal static Model Pet2001 { get; } = new(
        ModelConstants.Pet2001, ModelConstants.Pet2001DisplayName,
        [CpuModel.Mos6502], [ChipsetModel.TTL], (int)RamCapacity._8KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model Pet3008 { get; } = new(
        ModelConstants.Pet3008, ModelConstants.Pet3008DisplayName,
        [CpuModel.Mos6502], [ChipsetModel.TTL], (int)RamCapacity._8KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model Pet3016 { get; } = new(
        ModelConstants.Pet3016, ModelConstants.Pet3016DisplayName,
        [CpuModel.Mos6502], [ChipsetModel.TTL], (int)RamCapacity._16KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model Pet3032 { get; } = new(
        ModelConstants.Pet3032, ModelConstants.Pet3032DisplayName,
        [CpuModel.Mos6502], [ChipsetModel.TTL], (int)RamCapacity._32KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model Pet3032B { get; } = new(
        ModelConstants.Pet3032B, ModelConstants.Pet3032BDisplayName,
        [CpuModel.Mos6502], [ChipsetModel.TTL], (int)RamCapacity._32KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model Pet4016 { get; } = new(
        ModelConstants.Pet4016, ModelConstants.Pet4016DisplayName,
        [CpuModel.Mos6502], [ChipsetModel.CRTC], (int)RamCapacity._16KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model Pet4032 { get; } = new(
        ModelConstants.Pet4032, ModelConstants.Pet4032DisplayName,
        [CpuModel.Mos6502], [ChipsetModel.CRTC], (int)RamCapacity._32KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model Pet4032B { get; } = new(
        ModelConstants.Pet4032B, ModelConstants.Pet4032BDisplayName,
        [CpuModel.Mos6502], [ChipsetModel.CRTC], (int)RamCapacity._32KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model Pet8032 { get; } = new(
        ModelConstants.Pet8032, ModelConstants.Pet8032DisplayName,
        [CpuModel.Mos6502], [ChipsetModel.CRTC], (int)RamCapacity._32KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model Pet8096 { get; } = new(
        ModelConstants.Pet8096, ModelConstants.Pet8096DisplayName,
        [CpuModel.Mos6502], [ChipsetModel.CRTC], (int)RamCapacity._96KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model Pet8296 { get; } = new(
        ModelConstants.Pet8296, ModelConstants.Pet8296DisplayName,
        [CpuModel.Mos6502], [ChipsetModel.CRTC], (int)RamCapacity._128KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static Model SuperPet { get; } = new(
        ModelConstants.SuperPet, ModelConstants.SuperPetDisplayName,
        [CpuModel.Mos6502, CpuModel.Motorola6809], [ChipsetModel.CRTC], (int)RamCapacity._96KB,
        FloppyDriveCapacity: DriveCapacity.DualDrives, HardDriveCapacity: DriveCapacity.None,
        MouseButtonCount: ModelConstants.MouseButtonCount, ControllerPortCount: ModelConstants.JoystickPortCount,
        HasCdDrive: false, HasBuiltInFloppyDrive: false, HasKeyboard: true,
        ControllerTypes: [ControllerType.None]);

    internal static IReadOnlyList<Model> All { get; } =
    [
        Pet2001,
        Pet3008,
        Pet3016,
        Pet3032,
        Pet3032B,
        Pet4016,
        Pet4032,
        Pet4032B,
        Pet8032,
        Pet8096,
        Pet8296,
        SuperPet
    ];
}
