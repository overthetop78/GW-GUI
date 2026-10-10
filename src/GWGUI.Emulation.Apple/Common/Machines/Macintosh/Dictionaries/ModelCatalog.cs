using GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;
using GWGUI.Emulation.Apple.Common.Machines.Macintosh.Constants;

namespace GWGUI.Emulation.Apple.Common.Machines.Macintosh.Dictionaries;

public static class MacintoshModelCatalog
{
    public static IReadOnlyList<HardwareModelDefinition> All { get; } = Array.AsReadOnly(new[]
    {
        Create(MachineModel.Macintosh128K, MacintoshModelConstants.OneHundredTwentyEightKibibytes),
        Create(MachineModel.Macintosh512K, MacintoshModelConstants.FiveHundredTwelveKibibytes),
        Create(MachineModel.MacintoshPlus, MacintoshModelConstants.OneMebibyte),
        Create(MachineModel.MacintoshClassic, MacintoshModelConstants.OneMebibyte),
        Create(MachineModel.MacintoshII, MacintoshModelConstants.OneMebibyte)
    });

    private static HardwareModelDefinition Create(MachineModel model, long memory)
    {
        var macintoshII = model == MachineModel.MacintoshII;
        var hardDisk = model is MachineModel.MacintoshPlus or MachineModel.MacintoshClassic or MachineModel.MacintoshII;
        return new(model, model.ToString(), MacintoshModelConstants.ResourcePrefix + model, Emulator.MiniVmac,
            Array.AsReadOnly(new[] { macintoshII ? HardwareCpu.Motorola68020 : HardwareCpu.Motorola68000 }),
            macintoshII ? MacintoshModelConstants.MacintoshIICpuFrequencyHz : MacintoshModelConstants.CompactCpuFrequencyHz,
            memory, Array.AsReadOnly(new[] { HardwareRegion.Worldwide }),
            Array.AsReadOnly(new[] { macintoshII ? HardwareVideoCapability.MacintoshIIColor : HardwareVideoCapability.MacintoshMonochrome }),
            Array.AsReadOnly(new[] { macintoshII ? HardwareAudioCapability.AppleSoundChip : HardwareAudioCapability.MacintoshSound }),
            Array.AsReadOnly(hardDisk ? new[] { HardwareStorageCapability.Floppy, HardwareStorageCapability.HardDisk }
                : new[] { HardwareStorageCapability.Floppy }),
            Array.AsReadOnly(new[]
            {
                new HardwarePortDefinition(HardwarePortCapability.Keyboard, MacintoshModelConstants.OnePort),
                new HardwarePortDefinition(HardwarePortCapability.Mouse, MacintoshModelConstants.OnePort)
            }),
            Array.AsReadOnly(new[] { FirmwareCategory.SystemRom }),
            Array.AsReadOnly(hardDisk ? new[] { MediaCategory.Floppy, MediaCategory.HardDisk } : new[] { MediaCategory.Floppy }));
    }
}
