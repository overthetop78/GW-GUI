using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

internal static partial class AtariSettingsDescriptionFunctions
{
    private static readonly IReadOnlyDictionary<string, string> FieldHelpResources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [AtariSettingsConstants.Fpu] = "Emulation.Help.Cpu.FpuModel",
            [AtariSettingsConstants.CpuPrecision] = "Emulation.Help.Cpu.Precision",
            [AtariSettingsConstants.CpuFrequency] = "Emulation.Help.Cpu.Speed",
            [AtariSettingsConstants.AlternateMemory] = "Emulation.Help.Memory.Extensions",
            [AtariEightBitSettingsConstants.MosaicMemoryOptionKey] = "Emulation.Help.Memory.Mosaic",
            [AtariEightBitSettingsConstants.AxlonMemoryOptionKey] = "Emulation.Help.Memory.Axlon",
            [AtariEightBitSettingsConstants.AxlonShadowOptionKey] = "Emulation.Help.Memory.AxlonShadow",
            [AtariEightBitSettingsConstants.MapRamOptionKey] = "Emulation.Help.Memory.MapRam",
            [AtariSettingsDescriptionFunctionsConstants.HatariFastboot] = "Emulation.Help.Firmware.FastBoot",
            [AtariVideoAudioSettingsConstants.StandardOption] = "Emulation.Help.Video.Standard",
            [AtariConfigurationOptionConstants.VideoStandard] = "Emulation.Help.Video.Standard",
            [AtariVideoAudioSettingsConstants.AspectRatioOption] = "Emulation.Help.Video.AspectRatio",
            [AtariVideoAudioSettingsConstants.FrameSkipOption] = "Emulation.Help.Video.FrameSkip",
            [AtariSettingsConstants.Region] = "Emulation.Help.Video.Region",
            [AtariEightBitSettingsConstants.ArtifactingModeOptionKey] = "Emulation.Help.Video.Artifacting",
            [AtariEightBitSettingsConstants.ColorGammaOptionKey] = "Emulation.Help.Video.Gamma",
            [AtariEightBitSettingsConstants.ColorDelayOptionKey] = "Emulation.Help.Video.ColorDelay",
            [AtariEightBitSettingsConstants.ExternalPaletteOptionKey] = "Emulation.Help.Video.ExternalPalette",
            [AtariVideoAudioSettingsConstants.AudioLatencyOption] = "Emulation.Help.Audio.Latency",
            [AtariVideoAudioSettingsConstants.PolarizedFilterOption] = "Emulation.Help.Audio.PolarizedFilter",
            [AtariEightBitSettingsConstants.PokeyStereoOptionKey] = "Emulation.Help.Audio.PokeyStereo",
            [AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey] = "Emulation.Help.Controller.Compatibility",
            [AtariEightBitSettingsConstants.DigitalSensitivityOptionKey] = "Emulation.Help.Controller.DigitalSensitivity",
            [AtariEightBitSettingsConstants.AnalogSensitivityOptionKey] = "Emulation.Help.Controller.AnalogSensitivity",
            [AtariEightBitSettingsConstants.AutofireOptionKey] = "Emulation.Help.Controller.Autofire",
            [AtariEightBitSettingsConstants.PaddleMovementSpeedOptionKey] = "Emulation.Help.Controller.PaddleSpeed",
            [AtariEightBitSettingsConstants.CassetteBootOptionKey] = "Emulation.Help.Storage.CassetteBoot",
            [AtariEightBitSettingsConstants.RealTimeClockOptionKey] = "Emulation.Help.Storage.RealTimeClock",
            [AtariEightBitSettingsConstants.PrinterDeviceOptionKey] = "Emulation.Help.Storage.PrinterDevice",
            [AtariEightBitSettingsConstants.SerialDeviceOptionKey] = "Emulation.Help.Storage.SerialDevice"
        };

    internal static IReadOnlyList<EmulationSettingsBlock> Create(AtariMachineConfiguration configuration)
    {
        var compatibility = AtariCompatibilityCatalog.Get(configuration.Model);
        var blocks = (compatibility.Core == AtariEmulator.Hatari
            ? CreateSt(configuration)
            : CreateClassic(configuration)).ToList();
        AddGeneralFolders(configuration, compatibility, blocks);
        AddMouseSettings(configuration, compatibility, blocks);
        return blocks;
    }

}
