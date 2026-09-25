namespace GWGUI.Emulation.Atari.Common.Machines.AtariClassic.Dictionaries;

public static class ClassicModelCatalog
{
    private static readonly IReadOnlyList<ClassicCpu> EightBitCpu =
        ClassicModelFunctions.Values(ClassicCpu.Mos6502C);
    private static readonly IReadOnlyList<ClassicCpu> OriginalEightBitCpu =
        ClassicModelFunctions.Values(ClassicCpu.Mos6502B);
    private static readonly IReadOnlyList<ClassicCpu> Atari2600Cpu =
        ClassicModelFunctions.Values(ClassicCpu.Mos6507);
    private static readonly IReadOnlyList<ClassicCpu> Atari7800Cpu =
        ClassicModelFunctions.Values(ClassicCpu.Sally6502C);
    private static readonly IReadOnlyList<ClassicCpu> LynxCpu =
        ClassicModelFunctions.Values(ClassicCpu.Wdc65Sc02);
    private static readonly IReadOnlyList<ClassicCpu> JaguarCpus = ClassicModelFunctions.Values(
        ClassicCpu.Motorola68000, ClassicCpu.TomGraphicsProcessor,
        ClassicCpu.JerrySignalProcessor);
    private static readonly IReadOnlyList<ClassicRegion> TelevisionRegions =
        ClassicModelFunctions.Values(ClassicRegion.Ntsc, ClassicRegion.Pal);
    private static readonly IReadOnlyList<ClassicRegion> RegionFree =
        ClassicModelFunctions.Values(ClassicRegion.RegionFree);
    private static readonly IReadOnlyList<ClassicVideoCapability> EightBitVideo =
        ClassicModelFunctions.Values(ClassicVideoCapability.Antic, ClassicVideoCapability.Ctia,
            ClassicVideoCapability.Gtia);
    private static readonly IReadOnlyList<ClassicAudioCapability> PokeyAudio =
        ClassicModelFunctions.Values(ClassicAudioCapability.Pokey);
    private static readonly IReadOnlyList<ClassicStorageCapability> EightBitStorage =
        ClassicModelFunctions.Values(ClassicStorageCapability.Floppy,
            ClassicStorageCapability.Cassette, ClassicStorageCapability.Cartridge);
    private static readonly IReadOnlyList<ClassicPortDefinition> FourPortComputerPorts =
        ClassicModelFunctions.Values(
            new ClassicPortDefinition(ClassicPortCapability.Keyboard, ClassicModelConstants.OnePort),
            new ClassicPortDefinition(ClassicPortCapability.Joystick, ClassicModelConstants.FourPorts),
            new ClassicPortDefinition(ClassicPortCapability.Paddle, ClassicModelConstants.FourPorts));
    private static readonly IReadOnlyList<ClassicPortDefinition> TwoPortComputerPorts =
        ClassicModelFunctions.Values(
            new ClassicPortDefinition(ClassicPortCapability.Keyboard, ClassicModelConstants.OnePort),
            new ClassicPortDefinition(ClassicPortCapability.Joystick, ClassicModelConstants.TwoPorts),
            new ClassicPortDefinition(ClassicPortCapability.Paddle, ClassicModelConstants.TwoPorts));
    private static readonly IReadOnlyList<FirmwareCategory> Atari400And800Firmware =
        ClassicModelFunctions.Values(FirmwareCategory.AtariOsA, FirmwareCategory.AtariOsB,
            FirmwareCategory.AtariBasic);
    private static readonly IReadOnlyList<FirmwareCategory> Atari400Firmware =
        ClassicModelFunctions.Values(FirmwareCategory.AtariSystemOs);
    private static readonly IReadOnlyList<FirmwareCategory> XlXeFirmware =
        ClassicModelFunctions.Values(FirmwareCategory.AtariXlOs, FirmwareCategory.AtariBasic);
    private static readonly IReadOnlyList<MediaCategory> EightBitMedia =
        ClassicModelFunctions.Values(MediaCategory.Floppy, MediaCategory.Cassette,
            MediaCategory.Cartridge);
    private static readonly IReadOnlyList<ClassicStorageCapability> CartridgeStorage =
        ClassicModelFunctions.Values(ClassicStorageCapability.Cartridge);
    private static readonly IReadOnlyList<MediaCategory> CartridgeMedia =
        ClassicModelFunctions.Values(MediaCategory.Cartridge);
    private static readonly IReadOnlyList<FirmwareCategory> NoFirmware =
        ClassicModelFunctions.Values<FirmwareCategory>();

    private static readonly IReadOnlyList<ClassicModelDefinition> Definitions =
        ClassicModelFunctions.Values(
            ClassicModelFunctions.CreateEightBit(MachineModel.Atari400, ClassicModelConstants.Atari400And800ModelId,
                ClassicModelConstants.Atari400DisplayNameResource, ClassicModelConstants.FortyEightKibibytes,
                OriginalEightBitCpu, TelevisionRegions, EightBitVideo, PokeyAudio, EightBitStorage, FourPortComputerPorts,
                Atari400Firmware, EightBitMedia),
            ClassicModelFunctions.CreateEightBit(MachineModel.Atari800, ClassicModelConstants.Atari400And800ModelId,
                ClassicModelConstants.Atari800DisplayNameResource, ClassicModelConstants.FortyEightKibibytes,
                OriginalEightBitCpu, TelevisionRegions, EightBitVideo, PokeyAudio, EightBitStorage, FourPortComputerPorts,
                Atari400And800Firmware, EightBitMedia),
            ClassicModelFunctions.CreateEightBit(MachineModel.Atari800Xl, ClassicModelConstants.Atari800XlModelId,
                ClassicModelConstants.Atari800XlDisplayNameResource, ClassicModelConstants.SixtyFourKibibytes,
                EightBitCpu, TelevisionRegions, EightBitVideo, PokeyAudio, EightBitStorage, TwoPortComputerPorts,
                XlXeFirmware, EightBitMedia),
            ClassicModelFunctions.CreateEightBit(MachineModel.Atari130Xe, ClassicModelConstants.Atari130XeModelId,
                ClassicModelConstants.Atari130XeDisplayNameResource,
                ClassicModelConstants.OneHundredTwentyEightKibibytes, EightBitCpu, TelevisionRegions,
                EightBitVideo, PokeyAudio, EightBitStorage, TwoPortComputerPorts, XlXeFirmware, EightBitMedia),
            ClassicModelFunctions.CreateEightBit(MachineModel.XlXe, ClassicModelConstants.XlXeModelId,
                ClassicModelConstants.XlXeDisplayNameResource, ClassicModelConstants.ThreeHundredTwentyKibibytes,
                EightBitCpu, TelevisionRegions, EightBitVideo, PokeyAudio, EightBitStorage, TwoPortComputerPorts,
                XlXeFirmware, EightBitMedia),
            ClassicModelFunctions.CreateEightBit(MachineModel.Xegs, ClassicModelConstants.XegsModelId,
                ClassicModelConstants.XegsDisplayNameResource, ClassicModelConstants.SixtyFourKibibytes,
                EightBitCpu, TelevisionRegions, EightBitVideo, PokeyAudio, EightBitStorage, TwoPortComputerPorts,
                ClassicModelFunctions.Values(FirmwareCategory.AtariXlOs, FirmwareCategory.AtariBasic,
                    FirmwareCategory.AtariXegsBios), EightBitMedia),
            ClassicModelFunctions.Create(MachineModel.Atari5200, ClassicModelConstants.Atari5200ModelId,
                ClassicModelConstants.Atari5200DisplayNameResource, Emulator.Atari800,
                ClassicModelConstants.Atari8BitCpuFrequencyHz, ClassicModelConstants.SixteenKibibytes,
                EightBitCpu, TelevisionRegions, EightBitVideo, PokeyAudio, CartridgeStorage,
                ClassicModelFunctions.Values(
                    new ClassicPortDefinition(ClassicPortCapability.AnalogJoystick,
                        ClassicModelConstants.FourPorts),
                    new ClassicPortDefinition(ClassicPortCapability.NumericKeypad,
                        ClassicModelConstants.FourPorts)),
                ClassicModelFunctions.Values(FirmwareCategory.Atari5200Bios), CartridgeMedia),
            ClassicModelFunctions.Create(MachineModel.Atari2600, ClassicModelConstants.Atari2600ModelId,
                ClassicModelConstants.Atari2600DisplayNameResource, Emulator.Stella,
                ClassicModelConstants.Atari2600CpuFrequencyHz, ClassicModelConstants.OneHundredTwentyEightBytes,
                Atari2600Cpu, TelevisionRegions,
                ClassicModelFunctions.Values(ClassicVideoCapability.Tia),
                ClassicModelFunctions.Values(ClassicAudioCapability.Tia), CartridgeStorage,
                ClassicModelFunctions.Values(
                    new ClassicPortDefinition(ClassicPortCapability.Joystick,
                        ClassicModelConstants.TwoPorts),
                    new ClassicPortDefinition(ClassicPortCapability.Paddle,
                        ClassicModelConstants.TwoPorts),
                    new ClassicPortDefinition(ClassicPortCapability.DrivingController,
                        ClassicModelConstants.TwoPorts)),
                NoFirmware, CartridgeMedia),
            ClassicModelFunctions.Create(MachineModel.Atari7800, ClassicModelConstants.Atari7800ModelId,
                ClassicModelConstants.Atari7800DisplayNameResource, Emulator.ProSystem,
                ClassicModelConstants.Atari7800CpuFrequencyHz, ClassicModelConstants.FourKibibytes,
                Atari7800Cpu, TelevisionRegions,
                ClassicModelFunctions.Values(ClassicVideoCapability.Maria),
                ClassicModelFunctions.Values(ClassicAudioCapability.Tia,
                    ClassicAudioCapability.CartridgePokey), CartridgeStorage,
                ClassicModelFunctions.Values(
                    new ClassicPortDefinition(ClassicPortCapability.ProLineController,
                        ClassicModelConstants.TwoPorts),
                    new ClassicPortDefinition(ClassicPortCapability.LightGun,
                        ClassicModelConstants.TwoPorts)),
                ClassicModelFunctions.Values(FirmwareCategory.Atari7800Bios), CartridgeMedia),
            ClassicModelFunctions.Create(MachineModel.Lynx, ClassicModelConstants.LynxModelId,
                ClassicModelConstants.LynxDisplayNameResource, Emulator.BeetleLynx,
                ClassicModelConstants.LynxCpuFrequencyHz, ClassicModelConstants.SixtyFourKibibytes,
                LynxCpu, RegionFree,
                ClassicModelFunctions.Values(ClassicVideoCapability.Suzy, ClassicVideoCapability.Mikey),
                ClassicModelFunctions.Values(ClassicAudioCapability.Mikey), CartridgeStorage,
                ClassicModelFunctions.Values(
                    new ClassicPortDefinition(ClassicPortCapability.EnhancedController,
                        ClassicModelConstants.OnePort)),
                ClassicModelFunctions.Values(FirmwareCategory.LynxBootRom), CartridgeMedia),
            ClassicModelFunctions.CreateJaguar(MachineModel.Jaguar, ClassicModelConstants.JaguarModelId,
                ClassicModelConstants.JaguarDisplayNameResource, JaguarCpus, TelevisionRegions,
                ClassicModelFunctions.Values(ClassicVideoCapability.Tom),
                ClassicModelFunctions.Values(ClassicAudioCapability.Jerry),
                ClassicModelFunctions.Values(new ClassicPortDefinition(
                    ClassicPortCapability.EnhancedController, ClassicModelConstants.TwoPorts)),
                NoFirmware, CartridgeStorage, CartridgeMedia),
            ClassicModelFunctions.CreateJaguar(MachineModel.JaguarCd, ClassicModelConstants.JaguarCdModelId,
                ClassicModelConstants.JaguarCdDisplayNameResource,
                JaguarCpus, TelevisionRegions,
                ClassicModelFunctions.Values(ClassicVideoCapability.Tom),
                ClassicModelFunctions.Values(ClassicAudioCapability.Jerry),
                ClassicModelFunctions.Values(new ClassicPortDefinition(
                    ClassicPortCapability.EnhancedController, ClassicModelConstants.TwoPorts)),
                ClassicModelFunctions.Values(FirmwareCategory.JaguarCdBios),
                ClassicModelFunctions.Values(ClassicStorageCapability.Cartridge,
                    ClassicStorageCapability.CompactDisc),
                ClassicModelFunctions.Values(MediaCategory.Cartridge, MediaCategory.CompactDisc))
        );

    private static readonly IReadOnlyDictionary<MachineModel, ClassicModelDefinition> ByModel =
        ClassicModelFunctions.Index(Definitions);

    public static IReadOnlyList<ClassicModelDefinition> All => Definitions;

    public static ClassicModelDefinition Get(MachineModel model) =>
        ByModel.TryGetValue(model, out var definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(model), model, ErrorMessages.UnknownClassicModel);

}
