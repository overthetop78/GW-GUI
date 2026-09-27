namespace GWGUI.Emulation.Atari.Common.Machines.AtariST.Dictionaries;

public static class StModelCatalog
{
    private static readonly IReadOnlyList<StCpu> Cpu68000 =
        StModelFunctions.Values(StCpu.Motorola68000);
    private static readonly IReadOnlyList<StCpu> Cpu68030 =
        StModelFunctions.Values(StCpu.Motorola68030);
    private static readonly IReadOnlyList<StFpu> NoFpu =
        StModelFunctions.Values(StFpu.None);
    private static readonly IReadOnlyList<StFpu> CoprocessorFpus =
        StModelFunctions.Values(StFpu.None, StFpu.Motorola68881, StFpu.Motorola68882);
    private static readonly IReadOnlyList<StCpuPrecision> Precisions =
        StModelFunctions.Values(StCpuPrecision.Compatible, StCpuPrecision.CycleExact);
    private static readonly IReadOnlyList<int> StandardFrequency =
        StModelFunctions.Values(StModelConstants.BaseCpuFrequencyMhz);
    private static readonly IReadOnlyList<int> MegaSteFrequencies = StModelFunctions.Values(
        StModelConstants.EnhancedCpuFrequencyMhz, StModelConstants.BaseCpuFrequencyMhz);
    private static readonly IReadOnlyList<int> TtFrequency =
        StModelFunctions.Values(StModelConstants.TtCpuFrequencyMhz);
    private static readonly IReadOnlyList<int> FalconFrequency =
        StModelFunctions.Values(StModelConstants.EnhancedCpuFrequencyMhz);
    private static readonly IReadOnlyList<int> StandardMemory = StModelFunctions.Values(
        StModelConstants.HalfMibibyteKib, StModelConstants.OneMibibyteKib,
        StModelConstants.TwoMibibytesKib, StModelConstants.FourMibibytesKib);
    private static readonly IReadOnlyList<int> MegaStMemory = StModelFunctions.Values(
        StModelConstants.OneMibibyteKib, StModelConstants.TwoMibibytesKib,
        StModelConstants.FourMibibytesKib);
    private static readonly IReadOnlyList<int> MegaSteMemory = StModelFunctions.Values(
        StModelConstants.OneMibibyteKib, StModelConstants.TwoMibibytesKib,
        StModelConstants.FourMibibytesKib, StModelConstants.EightMibibytesKib);
    private static readonly IReadOnlyList<int> TtMemory = StModelFunctions.Values(
        StModelConstants.TwoMibibytesKib, StModelConstants.FourMibibytesKib,
        StModelConstants.EightMibibytesKib);
    private static readonly IReadOnlyList<int> FalconMemory = StModelFunctions.Values(
        StModelConstants.OneMibibyteKib, StModelConstants.TwoMibibytesKib,
        StModelConstants.FourMibibytesKib, StModelConstants.EightMibibytesKib,
        StModelConstants.FourteenMibibytesKib);
    private static readonly IReadOnlyList<int> NoAlternateMemory =
        StModelFunctions.Values(StModelConstants.NoAlternateMemoryMib);
    private static readonly IReadOnlyList<int> AlternateMemory = StModelFunctions.InclusiveRange(
        StModelConstants.NoAlternateMemoryMib, StModelConstants.OneThousandTwentyFourMibibytes,
        StModelConstants.AlternateMemoryStepMib);
    private static readonly IReadOnlyList<StRegion> AllRegions =
        StModelFunctions.EnumValues<StRegion>();
    private static readonly IReadOnlyList<StAudioCapability> StandardAudio =
        StModelFunctions.Values(StAudioCapability.ProgrammableSoundGenerator);
    private static readonly IReadOnlyList<StAudioCapability> EnhancedAudio = StModelFunctions.Values(
        StAudioCapability.ProgrammableSoundGenerator, StAudioCapability.StereoDma,
        StAudioCapability.Microwire);
    private static readonly IReadOnlyList<StPortCapability> StandardPorts = StModelFunctions.Values(
        StPortCapability.Keyboard, StPortCapability.Mouse, StPortCapability.Joystick,
        StPortCapability.Midi, StPortCapability.Parallel, StPortCapability.Serial,
        StPortCapability.Cartridge);
    private static readonly IReadOnlyList<StPortCapability> EnhancedPorts = StModelFunctions.Values(
        StPortCapability.Keyboard, StPortCapability.Mouse, StPortCapability.Joystick,
        StPortCapability.EnhancedJoystick, StPortCapability.Midi, StPortCapability.Parallel,
        StPortCapability.Serial, StPortCapability.Cartridge);
    private static readonly IReadOnlyList<StStorageCapability> StandardStorage =
        StModelFunctions.Values(StStorageCapability.FloppyDoubleDensity,
            StStorageCapability.Acsi, StStorageCapability.Ide,
            StStorageCapability.GemdosDirectory);

    private static readonly IReadOnlyList<StModelDefinition> Definitions =
        StModelFunctions.Values(
        CreateStandard(MachineModel.St, StModelConstants.StDisplayNameResource),
        CreateStandard(MachineModel.Stf, StModelConstants.StfDisplayNameResource),
        CreateStandard(MachineModel.Stfm, StModelConstants.StfmDisplayNameResource),
        new(MachineModel.MegaSt, StModelConstants.StMachineId,
            StModelConstants.MegaStDisplayNameResource, StCpu.Motorola68000, Cpu68000, NoFpu,
            StandardFrequency, Precisions, MegaStMemory, NoAlternateMemory,
            StModelFunctions.Values(StModelConstants.Tos102, StModelConstants.Tos104,
                StModelConstants.Tos206), AllRegions,
            StModelFunctions.Values(StVideoCapability.Pal, StVideoCapability.Ntsc,
                StVideoCapability.Monochrome, StVideoCapability.Blitter),
            StandardAudio, StandardStorage, StandardPorts),
        new(MachineModel.Ste, StModelConstants.SteMachineId,
            StModelConstants.SteDisplayNameResource, StCpu.Motorola68000, Cpu68000, NoFpu,
            StandardFrequency, Precisions, StandardMemory, NoAlternateMemory,
            StModelFunctions.Values(StModelConstants.Tos106, StModelConstants.Tos162,
                StModelConstants.Tos205, StModelConstants.Tos206), AllRegions,
            StModelFunctions.Values(StVideoCapability.Pal, StVideoCapability.Ntsc,
                StVideoCapability.Monochrome, StVideoCapability.Blitter,
                StVideoCapability.EnhancedPalette, StVideoCapability.HardwareScrolling),
            EnhancedAudio, StandardStorage, EnhancedPorts),
        new(MachineModel.MegaSte, StModelConstants.SteMachineId,
            StModelConstants.MegaSteDisplayNameResource, StCpu.Motorola68000, Cpu68000, NoFpu,
            MegaSteFrequencies, Precisions, MegaSteMemory, NoAlternateMemory,
            StModelFunctions.Values(StModelConstants.Tos205, StModelConstants.Tos206), AllRegions,
            StModelFunctions.Values(StVideoCapability.Pal, StVideoCapability.Ntsc,
                StVideoCapability.Monochrome, StVideoCapability.Blitter,
                StVideoCapability.EnhancedPalette, StVideoCapability.HardwareScrolling),
            EnhancedAudio,
            StModelFunctions.Values(StStorageCapability.FloppyDoubleDensity,
                StStorageCapability.FloppyHighDensity, StStorageCapability.Acsi,
                StStorageCapability.Ide, StStorageCapability.GemdosDirectory),
            StModelFunctions.Values(StPortCapability.Keyboard, StPortCapability.Mouse,
                StPortCapability.Joystick, StPortCapability.EnhancedJoystick,
                StPortCapability.Midi, StPortCapability.Parallel, StPortCapability.Serial,
                StPortCapability.Cartridge, StPortCapability.LocalAreaNetwork, StPortCapability.Vme)),
        new(MachineModel.Tt, StModelConstants.TtMachineId,
            StModelConstants.TtDisplayNameResource, StCpu.Motorola68030, Cpu68030, CoprocessorFpus,
            TtFrequency, Precisions, TtMemory, AlternateMemory,
            StModelFunctions.Values(StModelConstants.Tos301, StModelConstants.Tos305,
                StModelConstants.Tos306), AllRegions,
            StModelFunctions.Values(StVideoCapability.Pal, StVideoCapability.Ntsc,
                StVideoCapability.Monochrome, StVideoCapability.EnhancedPalette,
                StVideoCapability.TtShifter), EnhancedAudio,
            StModelFunctions.Values(StStorageCapability.FloppyDoubleDensity,
                StStorageCapability.FloppyHighDensity, StStorageCapability.Acsi,
                StStorageCapability.Ide, StStorageCapability.Scsi,
                StStorageCapability.GemdosDirectory),
            StModelFunctions.Values(StPortCapability.Keyboard, StPortCapability.Mouse,
                StPortCapability.Joystick, StPortCapability.Midi, StPortCapability.Parallel,
                StPortCapability.Serial, StPortCapability.Cartridge,
                StPortCapability.LocalAreaNetwork, StPortCapability.Vme)),
        new(MachineModel.Falcon, StModelConstants.FalconMachineId,
            StModelConstants.FalconDisplayNameResource, StCpu.Motorola68030, Cpu68030, CoprocessorFpus,
            FalconFrequency, Precisions, FalconMemory, AlternateMemory,
            StModelFunctions.Values(StModelConstants.Tos400, StModelConstants.Tos401,
                StModelConstants.Tos402, StModelConstants.Tos404), AllRegions,
            StModelFunctions.Values(StVideoCapability.Pal, StVideoCapability.Ntsc,
                StVideoCapability.Monochrome, StVideoCapability.Blitter,
                StVideoCapability.EnhancedPalette, StVideoCapability.HardwareScrolling,
                StVideoCapability.Videl),
            StModelFunctions.Values(StAudioCapability.ProgrammableSoundGenerator,
                StAudioCapability.StereoDma, StAudioCapability.DigitalSignalProcessor,
                StAudioCapability.Microphone),
            StModelFunctions.Values(StStorageCapability.FloppyDoubleDensity,
                StStorageCapability.FloppyHighDensity, StStorageCapability.Ide,
                StStorageCapability.Scsi, StStorageCapability.GemdosDirectory),
            StModelFunctions.Values(StPortCapability.Keyboard, StPortCapability.Mouse,
                StPortCapability.Joystick, StPortCapability.EnhancedJoystick,
                StPortCapability.Midi, StPortCapability.Parallel, StPortCapability.Serial,
                StPortCapability.Cartridge, StPortCapability.LocalAreaNetwork))
    );

    private static readonly IReadOnlyDictionary<MachineModel, StModelDefinition> ByModel =
        StModelFunctions.Index(Definitions);

    public static IReadOnlyList<StModelDefinition> All => Definitions;

    public static StModelDefinition Get(MachineModel model) =>
        ByModel.TryGetValue(model, out var definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(model), model, ErrorMessages.UnknownStModel);

    private static StModelDefinition CreateStandard(MachineModel model, string displayNameResource) =>
        new(model, StModelConstants.StMachineId, displayNameResource, StCpu.Motorola68000,
            Cpu68000, NoFpu, StandardFrequency, Precisions, StandardMemory, NoAlternateMemory,
            StModelFunctions.Values(StModelConstants.Tos100, StModelConstants.Tos102,
                StModelConstants.Tos104, StModelConstants.Tos206), AllRegions,
            StModelFunctions.Values(StVideoCapability.Pal, StVideoCapability.Ntsc,
                StVideoCapability.Monochrome), StandardAudio, StandardStorage, StandardPorts);
}
