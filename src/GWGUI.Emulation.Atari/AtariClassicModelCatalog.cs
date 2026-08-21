namespace GWGUI.Emulation.Atari;

public static class AtariClassicModelCatalog
{
    private static readonly IReadOnlyList<AtariClassicCpu> EightBitCpu =
        AtariClassicModelFunctions.Values(AtariClassicCpu.Mos6502C);
    private static readonly IReadOnlyList<AtariClassicCpu> OriginalEightBitCpu =
        AtariClassicModelFunctions.Values(AtariClassicCpu.Mos6502B);
    private static readonly IReadOnlyList<AtariClassicCpu> Atari2600Cpu =
        AtariClassicModelFunctions.Values(AtariClassicCpu.Mos6507);
    private static readonly IReadOnlyList<AtariClassicCpu> Atari7800Cpu =
        AtariClassicModelFunctions.Values(AtariClassicCpu.Sally6502C);
    private static readonly IReadOnlyList<AtariClassicCpu> LynxCpu =
        AtariClassicModelFunctions.Values(AtariClassicCpu.Wdc65Sc02);
    private static readonly IReadOnlyList<AtariClassicCpu> JaguarCpus = AtariClassicModelFunctions.Values(
        AtariClassicCpu.Motorola68000, AtariClassicCpu.TomGraphicsProcessor,
        AtariClassicCpu.JerrySignalProcessor);
    private static readonly IReadOnlyList<AtariClassicRegion> TelevisionRegions =
        AtariClassicModelFunctions.Values(AtariClassicRegion.Ntsc, AtariClassicRegion.Pal);
    private static readonly IReadOnlyList<AtariClassicRegion> RegionFree =
        AtariClassicModelFunctions.Values(AtariClassicRegion.RegionFree);
    private static readonly IReadOnlyList<AtariClassicVideoCapability> EightBitVideo =
        AtariClassicModelFunctions.Values(AtariClassicVideoCapability.Antic, AtariClassicVideoCapability.Ctia,
            AtariClassicVideoCapability.Gtia);
    private static readonly IReadOnlyList<AtariClassicAudioCapability> PokeyAudio =
        AtariClassicModelFunctions.Values(AtariClassicAudioCapability.Pokey);
    private static readonly IReadOnlyList<AtariClassicStorageCapability> EightBitStorage =
        AtariClassicModelFunctions.Values(AtariClassicStorageCapability.Floppy,
            AtariClassicStorageCapability.Cassette, AtariClassicStorageCapability.Cartridge);
    private static readonly IReadOnlyList<AtariClassicPortDefinition> FourPortComputerPorts =
        AtariClassicModelFunctions.Values(
            new AtariClassicPortDefinition(AtariClassicPortCapability.Keyboard, AtariClassicModelConstants.OnePort),
            new AtariClassicPortDefinition(AtariClassicPortCapability.Joystick, AtariClassicModelConstants.FourPorts),
            new AtariClassicPortDefinition(AtariClassicPortCapability.Paddle, AtariClassicModelConstants.FourPorts));
    private static readonly IReadOnlyList<AtariClassicPortDefinition> TwoPortComputerPorts =
        AtariClassicModelFunctions.Values(
            new AtariClassicPortDefinition(AtariClassicPortCapability.Keyboard, AtariClassicModelConstants.OnePort),
            new AtariClassicPortDefinition(AtariClassicPortCapability.Joystick, AtariClassicModelConstants.TwoPorts),
            new AtariClassicPortDefinition(AtariClassicPortCapability.Paddle, AtariClassicModelConstants.TwoPorts));
    private static readonly IReadOnlyList<AtariFirmwareCategory> Atari400And800Firmware =
        AtariClassicModelFunctions.Values(AtariFirmwareCategory.AtariOsA, AtariFirmwareCategory.AtariOsB,
            AtariFirmwareCategory.AtariBasic);
    private static readonly IReadOnlyList<AtariFirmwareCategory> Atari400Firmware =
        AtariClassicModelFunctions.Values(AtariFirmwareCategory.AtariSystemOs);
    private static readonly IReadOnlyList<AtariFirmwareCategory> XlXeFirmware =
        AtariClassicModelFunctions.Values(AtariFirmwareCategory.AtariXlOs, AtariFirmwareCategory.AtariBasic);
    private static readonly IReadOnlyList<AtariMediaCategory> EightBitMedia =
        AtariClassicModelFunctions.Values(AtariMediaCategory.Floppy, AtariMediaCategory.Cassette,
            AtariMediaCategory.Cartridge);
    private static readonly IReadOnlyList<AtariClassicStorageCapability> CartridgeStorage =
        AtariClassicModelFunctions.Values(AtariClassicStorageCapability.Cartridge);
    private static readonly IReadOnlyList<AtariMediaCategory> CartridgeMedia =
        AtariClassicModelFunctions.Values(AtariMediaCategory.Cartridge);
    private static readonly IReadOnlyList<AtariFirmwareCategory> NoFirmware =
        AtariClassicModelFunctions.Values<AtariFirmwareCategory>();

    private static readonly IReadOnlyList<AtariClassicModelDefinition> Definitions =
        AtariClassicModelFunctions.Values(
            AtariClassicModelFunctions.CreateEightBit(AtariMachineModel.Atari400, AtariClassicModelConstants.Atari400And800ModelId,
                AtariClassicModelConstants.Atari400DisplayNameResource, AtariClassicModelConstants.FortyEightKibibytes,
                OriginalEightBitCpu, TelevisionRegions, EightBitVideo, PokeyAudio, EightBitStorage, FourPortComputerPorts,
                Atari400Firmware, EightBitMedia),
            AtariClassicModelFunctions.CreateEightBit(AtariMachineModel.Atari800, AtariClassicModelConstants.Atari400And800ModelId,
                AtariClassicModelConstants.Atari800DisplayNameResource, AtariClassicModelConstants.FortyEightKibibytes,
                OriginalEightBitCpu, TelevisionRegions, EightBitVideo, PokeyAudio, EightBitStorage, FourPortComputerPorts,
                Atari400And800Firmware, EightBitMedia),
            AtariClassicModelFunctions.CreateEightBit(AtariMachineModel.Atari800Xl, AtariClassicModelConstants.Atari800XlModelId,
                AtariClassicModelConstants.Atari800XlDisplayNameResource, AtariClassicModelConstants.SixtyFourKibibytes,
                EightBitCpu, TelevisionRegions, EightBitVideo, PokeyAudio, EightBitStorage, TwoPortComputerPorts,
                XlXeFirmware, EightBitMedia),
            AtariClassicModelFunctions.CreateEightBit(AtariMachineModel.Atari130Xe, AtariClassicModelConstants.Atari130XeModelId,
                AtariClassicModelConstants.Atari130XeDisplayNameResource,
                AtariClassicModelConstants.OneHundredTwentyEightKibibytes, EightBitCpu, TelevisionRegions,
                EightBitVideo, PokeyAudio, EightBitStorage, TwoPortComputerPorts, XlXeFirmware, EightBitMedia),
            AtariClassicModelFunctions.CreateEightBit(AtariMachineModel.XlXe, AtariClassicModelConstants.XlXeModelId,
                AtariClassicModelConstants.XlXeDisplayNameResource, AtariClassicModelConstants.ThreeHundredTwentyKibibytes,
                EightBitCpu, TelevisionRegions, EightBitVideo, PokeyAudio, EightBitStorage, TwoPortComputerPorts,
                XlXeFirmware, EightBitMedia),
            AtariClassicModelFunctions.CreateEightBit(AtariMachineModel.Xegs, AtariClassicModelConstants.XegsModelId,
                AtariClassicModelConstants.XegsDisplayNameResource, AtariClassicModelConstants.SixtyFourKibibytes,
                EightBitCpu, TelevisionRegions, EightBitVideo, PokeyAudio, EightBitStorage, TwoPortComputerPorts,
                AtariClassicModelFunctions.Values(AtariFirmwareCategory.AtariXlOs, AtariFirmwareCategory.AtariBasic,
                    AtariFirmwareCategory.AtariXegsBios), EightBitMedia),
            AtariClassicModelFunctions.Create(AtariMachineModel.Atari5200, AtariClassicModelConstants.Atari5200ModelId,
                AtariClassicModelConstants.Atari5200DisplayNameResource, AtariEmulator.Atari800,
                AtariClassicModelConstants.Atari8BitCpuFrequencyHz, AtariClassicModelConstants.SixteenKibibytes,
                EightBitCpu, TelevisionRegions, EightBitVideo, PokeyAudio, CartridgeStorage,
                AtariClassicModelFunctions.Values(
                    new AtariClassicPortDefinition(AtariClassicPortCapability.AnalogJoystick,
                        AtariClassicModelConstants.FourPorts),
                    new AtariClassicPortDefinition(AtariClassicPortCapability.NumericKeypad,
                        AtariClassicModelConstants.FourPorts)),
                AtariClassicModelFunctions.Values(AtariFirmwareCategory.Atari5200Bios), CartridgeMedia),
            AtariClassicModelFunctions.Create(AtariMachineModel.Atari2600, AtariClassicModelConstants.Atari2600ModelId,
                AtariClassicModelConstants.Atari2600DisplayNameResource, AtariEmulator.Stella,
                AtariClassicModelConstants.Atari2600CpuFrequencyHz, AtariClassicModelConstants.OneHundredTwentyEightBytes,
                Atari2600Cpu, TelevisionRegions,
                AtariClassicModelFunctions.Values(AtariClassicVideoCapability.Tia),
                AtariClassicModelFunctions.Values(AtariClassicAudioCapability.Tia), CartridgeStorage,
                AtariClassicModelFunctions.Values(
                    new AtariClassicPortDefinition(AtariClassicPortCapability.Joystick,
                        AtariClassicModelConstants.TwoPorts),
                    new AtariClassicPortDefinition(AtariClassicPortCapability.Paddle,
                        AtariClassicModelConstants.TwoPorts),
                    new AtariClassicPortDefinition(AtariClassicPortCapability.DrivingController,
                        AtariClassicModelConstants.TwoPorts)),
                NoFirmware, CartridgeMedia),
            AtariClassicModelFunctions.Create(AtariMachineModel.Atari7800, AtariClassicModelConstants.Atari7800ModelId,
                AtariClassicModelConstants.Atari7800DisplayNameResource, AtariEmulator.ProSystem,
                AtariClassicModelConstants.Atari7800CpuFrequencyHz, AtariClassicModelConstants.FourKibibytes,
                Atari7800Cpu, TelevisionRegions,
                AtariClassicModelFunctions.Values(AtariClassicVideoCapability.Maria),
                AtariClassicModelFunctions.Values(AtariClassicAudioCapability.Tia,
                    AtariClassicAudioCapability.CartridgePokey), CartridgeStorage,
                AtariClassicModelFunctions.Values(
                    new AtariClassicPortDefinition(AtariClassicPortCapability.ProLineController,
                        AtariClassicModelConstants.TwoPorts),
                    new AtariClassicPortDefinition(AtariClassicPortCapability.LightGun,
                        AtariClassicModelConstants.TwoPorts)),
                AtariClassicModelFunctions.Values(AtariFirmwareCategory.Atari7800Bios), CartridgeMedia),
            AtariClassicModelFunctions.Create(AtariMachineModel.Lynx, AtariClassicModelConstants.LynxModelId,
                AtariClassicModelConstants.LynxDisplayNameResource, AtariEmulator.BeetleLynx,
                AtariClassicModelConstants.LynxCpuFrequencyHz, AtariClassicModelConstants.SixtyFourKibibytes,
                LynxCpu, RegionFree,
                AtariClassicModelFunctions.Values(AtariClassicVideoCapability.Suzy, AtariClassicVideoCapability.Mikey),
                AtariClassicModelFunctions.Values(AtariClassicAudioCapability.Mikey), CartridgeStorage,
                AtariClassicModelFunctions.Values(
                    new AtariClassicPortDefinition(AtariClassicPortCapability.EnhancedController,
                        AtariClassicModelConstants.OnePort)),
                AtariClassicModelFunctions.Values(AtariFirmwareCategory.LynxBootRom), CartridgeMedia),
            AtariClassicModelFunctions.CreateJaguar(AtariMachineModel.Jaguar, AtariClassicModelConstants.JaguarModelId,
                AtariClassicModelConstants.JaguarDisplayNameResource, JaguarCpus, TelevisionRegions,
                AtariClassicModelFunctions.Values(AtariClassicVideoCapability.Tom),
                AtariClassicModelFunctions.Values(AtariClassicAudioCapability.Jerry),
                AtariClassicModelFunctions.Values(new AtariClassicPortDefinition(
                    AtariClassicPortCapability.EnhancedController, AtariClassicModelConstants.TwoPorts)),
                NoFirmware, CartridgeStorage, CartridgeMedia),
            AtariClassicModelFunctions.CreateJaguar(AtariMachineModel.JaguarCd, AtariClassicModelConstants.JaguarCdModelId,
                AtariClassicModelConstants.JaguarCdDisplayNameResource,
                JaguarCpus, TelevisionRegions,
                AtariClassicModelFunctions.Values(AtariClassicVideoCapability.Tom),
                AtariClassicModelFunctions.Values(AtariClassicAudioCapability.Jerry),
                AtariClassicModelFunctions.Values(new AtariClassicPortDefinition(
                    AtariClassicPortCapability.EnhancedController, AtariClassicModelConstants.TwoPorts)),
                AtariClassicModelFunctions.Values(AtariFirmwareCategory.JaguarCdBios),
                AtariClassicModelFunctions.Values(AtariClassicStorageCapability.Cartridge,
                    AtariClassicStorageCapability.CompactDisc),
                AtariClassicModelFunctions.Values(AtariMediaCategory.Cartridge, AtariMediaCategory.CompactDisc))
        );

    private static readonly IReadOnlyDictionary<AtariMachineModel, AtariClassicModelDefinition> ByModel =
        AtariClassicModelFunctions.Index(Definitions);

    public static IReadOnlyList<AtariClassicModelDefinition> All => Definitions;

    public static AtariClassicModelDefinition Get(AtariMachineModel model) =>
        ByModel.TryGetValue(model, out var definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(model), model, AtariErrorMessages.UnknownClassicModel);

}
