namespace GWGUI.Emulation.Atari;

public static class AtariStModelCatalog
{
    private static readonly IReadOnlyList<AtariStCpu> Cpu68000 =
        AtariStModelFunctions.Values(AtariStCpu.Motorola68000);
    private static readonly IReadOnlyList<AtariStCpu> Cpu68030 =
        AtariStModelFunctions.Values(AtariStCpu.Motorola68030);
    private static readonly IReadOnlyList<AtariStFpu> NoFpu =
        AtariStModelFunctions.Values(AtariStFpu.None);
    private static readonly IReadOnlyList<AtariStFpu> CoprocessorFpus =
        AtariStModelFunctions.Values(AtariStFpu.None, AtariStFpu.Motorola68881, AtariStFpu.Motorola68882);
    private static readonly IReadOnlyList<AtariStCpuPrecision> Precisions =
        AtariStModelFunctions.Values(AtariStCpuPrecision.Compatible, AtariStCpuPrecision.CycleExact);
    private static readonly IReadOnlyList<int> StandardFrequency =
        AtariStModelFunctions.Values(AtariStModelConstants.BaseCpuFrequencyMhz);
    private static readonly IReadOnlyList<int> MegaSteFrequencies = AtariStModelFunctions.Values(
        AtariStModelConstants.EnhancedCpuFrequencyMhz, AtariStModelConstants.BaseCpuFrequencyMhz);
    private static readonly IReadOnlyList<int> TtFrequency =
        AtariStModelFunctions.Values(AtariStModelConstants.TtCpuFrequencyMhz);
    private static readonly IReadOnlyList<int> FalconFrequency =
        AtariStModelFunctions.Values(AtariStModelConstants.EnhancedCpuFrequencyMhz);
    private static readonly IReadOnlyList<int> StandardMemory = AtariStModelFunctions.Values(
        AtariStModelConstants.HalfMibibyteKib, AtariStModelConstants.OneMibibyteKib,
        AtariStModelConstants.TwoMibibytesKib, AtariStModelConstants.FourMibibytesKib);
    private static readonly IReadOnlyList<int> MegaStMemory = AtariStModelFunctions.Values(
        AtariStModelConstants.OneMibibyteKib, AtariStModelConstants.TwoMibibytesKib,
        AtariStModelConstants.FourMibibytesKib);
    private static readonly IReadOnlyList<int> MegaSteMemory = AtariStModelFunctions.Values(
        AtariStModelConstants.OneMibibyteKib, AtariStModelConstants.TwoMibibytesKib,
        AtariStModelConstants.FourMibibytesKib, AtariStModelConstants.EightMibibytesKib);
    private static readonly IReadOnlyList<int> TtMemory = AtariStModelFunctions.Values(
        AtariStModelConstants.TwoMibibytesKib, AtariStModelConstants.FourMibibytesKib,
        AtariStModelConstants.EightMibibytesKib);
    private static readonly IReadOnlyList<int> FalconMemory = AtariStModelFunctions.Values(
        AtariStModelConstants.OneMibibyteKib, AtariStModelConstants.TwoMibibytesKib,
        AtariStModelConstants.FourMibibytesKib, AtariStModelConstants.EightMibibytesKib,
        AtariStModelConstants.FourteenMibibytesKib);
    private static readonly IReadOnlyList<int> NoAlternateMemory =
        AtariStModelFunctions.Values(AtariStModelConstants.NoAlternateMemoryMib);
    private static readonly IReadOnlyList<int> AlternateMemory = AtariStModelFunctions.InclusiveRange(
        AtariStModelConstants.NoAlternateMemoryMib, AtariStModelConstants.OneThousandTwentyFourMibibytes,
        AtariStModelConstants.AlternateMemoryStepMib);
    private static readonly IReadOnlyList<AtariStRegion> AllRegions =
        AtariStModelFunctions.EnumValues<AtariStRegion>();
    private static readonly IReadOnlyList<AtariStAudioCapability> StandardAudio =
        AtariStModelFunctions.Values(AtariStAudioCapability.ProgrammableSoundGenerator);
    private static readonly IReadOnlyList<AtariStAudioCapability> EnhancedAudio = AtariStModelFunctions.Values(
        AtariStAudioCapability.ProgrammableSoundGenerator, AtariStAudioCapability.StereoDma,
        AtariStAudioCapability.Microwire);
    private static readonly IReadOnlyList<AtariStPortCapability> StandardPorts = AtariStModelFunctions.Values(
        AtariStPortCapability.Keyboard, AtariStPortCapability.Mouse, AtariStPortCapability.Joystick,
        AtariStPortCapability.Midi, AtariStPortCapability.Parallel, AtariStPortCapability.Serial,
        AtariStPortCapability.Cartridge);
    private static readonly IReadOnlyList<AtariStPortCapability> EnhancedPorts = AtariStModelFunctions.Values(
        AtariStPortCapability.Keyboard, AtariStPortCapability.Mouse, AtariStPortCapability.Joystick,
        AtariStPortCapability.EnhancedJoystick, AtariStPortCapability.Midi, AtariStPortCapability.Parallel,
        AtariStPortCapability.Serial, AtariStPortCapability.Cartridge);
    private static readonly IReadOnlyList<AtariStStorageCapability> StandardStorage =
        AtariStModelFunctions.Values(AtariStStorageCapability.FloppyDoubleDensity,
            AtariStStorageCapability.Acsi, AtariStStorageCapability.Ide,
            AtariStStorageCapability.GemdosDirectory);

    private static readonly IReadOnlyList<AtariStModelDefinition> Definitions =
        AtariStModelFunctions.Values(
        CreateStandard(AtariMachineModel.St, AtariStModelConstants.StDisplayNameResource),
        CreateStandard(AtariMachineModel.Stf, AtariStModelConstants.StfDisplayNameResource),
        CreateStandard(AtariMachineModel.Stfm, AtariStModelConstants.StfmDisplayNameResource),
        new(AtariMachineModel.MegaSt, AtariStModelConstants.StMachineId,
            AtariStModelConstants.MegaStDisplayNameResource, AtariStCpu.Motorola68000, Cpu68000, NoFpu,
            StandardFrequency, Precisions, MegaStMemory, NoAlternateMemory,
            AtariStModelFunctions.Values(AtariStModelConstants.Tos102, AtariStModelConstants.Tos104,
                AtariStModelConstants.Tos206), AllRegions,
            AtariStModelFunctions.Values(AtariStVideoCapability.Pal, AtariStVideoCapability.Ntsc,
                AtariStVideoCapability.Monochrome, AtariStVideoCapability.Blitter),
            StandardAudio, StandardStorage, StandardPorts),
        new(AtariMachineModel.Ste, AtariStModelConstants.SteMachineId,
            AtariStModelConstants.SteDisplayNameResource, AtariStCpu.Motorola68000, Cpu68000, NoFpu,
            StandardFrequency, Precisions, StandardMemory, NoAlternateMemory,
            AtariStModelFunctions.Values(AtariStModelConstants.Tos106, AtariStModelConstants.Tos162,
                AtariStModelConstants.Tos205, AtariStModelConstants.Tos206), AllRegions,
            AtariStModelFunctions.Values(AtariStVideoCapability.Pal, AtariStVideoCapability.Ntsc,
                AtariStVideoCapability.Monochrome, AtariStVideoCapability.Blitter,
                AtariStVideoCapability.EnhancedPalette, AtariStVideoCapability.HardwareScrolling),
            EnhancedAudio, StandardStorage, EnhancedPorts),
        new(AtariMachineModel.MegaSte, AtariStModelConstants.SteMachineId,
            AtariStModelConstants.MegaSteDisplayNameResource, AtariStCpu.Motorola68000, Cpu68000, NoFpu,
            MegaSteFrequencies, Precisions, MegaSteMemory, NoAlternateMemory,
            AtariStModelFunctions.Values(AtariStModelConstants.Tos205, AtariStModelConstants.Tos206), AllRegions,
            AtariStModelFunctions.Values(AtariStVideoCapability.Pal, AtariStVideoCapability.Ntsc,
                AtariStVideoCapability.Monochrome, AtariStVideoCapability.Blitter,
                AtariStVideoCapability.EnhancedPalette, AtariStVideoCapability.HardwareScrolling),
            EnhancedAudio,
            AtariStModelFunctions.Values(AtariStStorageCapability.FloppyDoubleDensity,
                AtariStStorageCapability.FloppyHighDensity, AtariStStorageCapability.Acsi,
                AtariStStorageCapability.Ide, AtariStStorageCapability.GemdosDirectory),
            AtariStModelFunctions.Values(AtariStPortCapability.Keyboard, AtariStPortCapability.Mouse,
                AtariStPortCapability.Joystick, AtariStPortCapability.EnhancedJoystick,
                AtariStPortCapability.Midi, AtariStPortCapability.Parallel, AtariStPortCapability.Serial,
                AtariStPortCapability.Cartridge, AtariStPortCapability.LocalAreaNetwork, AtariStPortCapability.Vme)),
        new(AtariMachineModel.Tt, AtariStModelConstants.TtMachineId,
            AtariStModelConstants.TtDisplayNameResource, AtariStCpu.Motorola68030, Cpu68030, CoprocessorFpus,
            TtFrequency, Precisions, TtMemory, AlternateMemory,
            AtariStModelFunctions.Values(AtariStModelConstants.Tos301, AtariStModelConstants.Tos305,
                AtariStModelConstants.Tos306), AllRegions,
            AtariStModelFunctions.Values(AtariStVideoCapability.Pal, AtariStVideoCapability.Ntsc,
                AtariStVideoCapability.Monochrome, AtariStVideoCapability.EnhancedPalette,
                AtariStVideoCapability.TtShifter), EnhancedAudio,
            AtariStModelFunctions.Values(AtariStStorageCapability.FloppyDoubleDensity,
                AtariStStorageCapability.FloppyHighDensity, AtariStStorageCapability.Acsi,
                AtariStStorageCapability.Ide, AtariStStorageCapability.Scsi,
                AtariStStorageCapability.GemdosDirectory),
            AtariStModelFunctions.Values(AtariStPortCapability.Keyboard, AtariStPortCapability.Mouse,
                AtariStPortCapability.Joystick, AtariStPortCapability.Midi, AtariStPortCapability.Parallel,
                AtariStPortCapability.Serial, AtariStPortCapability.Cartridge,
                AtariStPortCapability.LocalAreaNetwork, AtariStPortCapability.Vme)),
        new(AtariMachineModel.Falcon, AtariStModelConstants.FalconMachineId,
            AtariStModelConstants.FalconDisplayNameResource, AtariStCpu.Motorola68030, Cpu68030, CoprocessorFpus,
            FalconFrequency, Precisions, FalconMemory, AlternateMemory,
            AtariStModelFunctions.Values(AtariStModelConstants.Tos400, AtariStModelConstants.Tos401,
                AtariStModelConstants.Tos402, AtariStModelConstants.Tos404), AllRegions,
            AtariStModelFunctions.Values(AtariStVideoCapability.Pal, AtariStVideoCapability.Ntsc,
                AtariStVideoCapability.Monochrome, AtariStVideoCapability.Blitter,
                AtariStVideoCapability.EnhancedPalette, AtariStVideoCapability.HardwareScrolling,
                AtariStVideoCapability.Videl),
            AtariStModelFunctions.Values(AtariStAudioCapability.ProgrammableSoundGenerator,
                AtariStAudioCapability.StereoDma, AtariStAudioCapability.DigitalSignalProcessor,
                AtariStAudioCapability.Microphone),
            AtariStModelFunctions.Values(AtariStStorageCapability.FloppyDoubleDensity,
                AtariStStorageCapability.FloppyHighDensity, AtariStStorageCapability.Ide,
                AtariStStorageCapability.Scsi, AtariStStorageCapability.GemdosDirectory),
            AtariStModelFunctions.Values(AtariStPortCapability.Keyboard, AtariStPortCapability.Mouse,
                AtariStPortCapability.Joystick, AtariStPortCapability.EnhancedJoystick,
                AtariStPortCapability.Midi, AtariStPortCapability.Parallel, AtariStPortCapability.Serial,
                AtariStPortCapability.Cartridge, AtariStPortCapability.LocalAreaNetwork))
    );

    private static readonly IReadOnlyDictionary<AtariMachineModel, AtariStModelDefinition> ByModel =
        AtariStModelFunctions.Index(Definitions);

    public static IReadOnlyList<AtariStModelDefinition> All => Definitions;

    public static AtariStModelDefinition Get(AtariMachineModel model) =>
        ByModel.TryGetValue(model, out var definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(model), model, AtariErrorMessages.UnknownStModel);

    private static AtariStModelDefinition CreateStandard(AtariMachineModel model, string displayNameResource) =>
        new(model, AtariStModelConstants.StMachineId, displayNameResource, AtariStCpu.Motorola68000,
            Cpu68000, NoFpu, StandardFrequency, Precisions, StandardMemory, NoAlternateMemory,
            AtariStModelFunctions.Values(AtariStModelConstants.Tos100, AtariStModelConstants.Tos102,
                AtariStModelConstants.Tos104, AtariStModelConstants.Tos206), AllRegions,
            AtariStModelFunctions.Values(AtariStVideoCapability.Pal, AtariStVideoCapability.Ntsc,
                AtariStVideoCapability.Monochrome), StandardAudio, StandardStorage, StandardPorts);
}
