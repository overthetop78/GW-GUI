using GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;
using GWGUI.Emulation.Apple.Common.Machines.AppleII.Constants;

namespace GWGUI.Emulation.Apple.Common.Machines.AppleII.Dictionaries;

public static class AppleIIModelCatalog
{
    public static IReadOnlyList<HardwareModelDefinition> All { get; } = Array.AsReadOnly(new[]
    {
        Create(MachineModel.AppleII, HardwareCpu.Mos6502, AppleIIModelConstants.FortyEightKibibytes),
        Create(MachineModel.AppleIIPlus, HardwareCpu.Mos6502, AppleIIModelConstants.FortyEightKibibytes),
        Create(MachineModel.AppleIIJPlus, HardwareCpu.Mos6502, AppleIIModelConstants.FortyEightKibibytes),
        Create(MachineModel.AppleIIe, HardwareCpu.Mos6502, AppleIIModelConstants.SixtyFourKibibytes),
        Create(MachineModel.AppleIIeEnhanced, HardwareCpu.Cmos65C02, AppleIIModelConstants.SixtyFourKibibytes)
    });

    private static HardwareModelDefinition Create(MachineModel model, HardwareCpu cpu, long memory) =>
        new(model, model.ToString(), AppleIIModelConstants.ResourcePrefix + model, Emulator.AppleWin,
            Array.AsReadOnly(new[] { cpu }), AppleIIModelConstants.CpuFrequencyHz, memory,
            Array.AsReadOnly(new[] { HardwareRegion.Ntsc, HardwareRegion.Pal }),
            Array.AsReadOnly(new[] { model is MachineModel.AppleIIe or MachineModel.AppleIIeEnhanced
                ? HardwareVideoCapability.AppleIIe : HardwareVideoCapability.AppleII }),
            Array.AsReadOnly(new[] { HardwareAudioCapability.Speaker }),
            Array.AsReadOnly(new[] { HardwareStorageCapability.Floppy, HardwareStorageCapability.HardDisk,
                HardwareStorageCapability.Cassette }),
            Array.AsReadOnly(new[]
            {
                new HardwarePortDefinition(HardwarePortCapability.Keyboard, AppleIIModelConstants.OnePort),
                new HardwarePortDefinition(HardwarePortCapability.AnalogJoystick, AppleIIModelConstants.TwoPorts)
            }),
            Array.AsReadOnly(new[] { FirmwareCategory.SystemRom }),
            Array.AsReadOnly(new[] { MediaCategory.Floppy, MediaCategory.HardDisk, MediaCategory.Cassette }));
}
