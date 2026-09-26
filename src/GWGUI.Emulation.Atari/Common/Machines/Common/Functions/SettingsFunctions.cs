using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static readonly IReadOnlyDictionary<string, string> FieldHelpResources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SettingsConstants.Fpu] = "Emulation.Help.Cpu.FpuModel",
            [SettingsConstants.CpuPrecision] = "Emulation.Help.Cpu.Precision",
            [SettingsConstants.CpuFrequency] = "Emulation.Help.Cpu.Speed",
            [SettingsConstants.AlternateMemory] = "Emulation.Help.Memory.Extensions",
            [EightBitSettingsConstants.MosaicMemoryOptionKey] = "Emulation.Help.Memory.Mosaic",
            [EightBitSettingsConstants.AxlonMemoryOptionKey] = "Emulation.Help.Memory.Axlon",
            [EightBitSettingsConstants.AxlonShadowOptionKey] = "Emulation.Help.Memory.AxlonShadow",
            [EightBitSettingsConstants.MapRamOptionKey] = "Emulation.Help.Memory.MapRam",
            [SettingsDescriptionFunctionsConstants.FastBoot] = "Emulation.Help.Firmware.FastBoot",
            [VideoAudioSettingsConstants.StandardOption] = "Emulation.Help.Video.Standard",
            [VideoAudioSettingsConstants.StandardOption] = "Emulation.Help.Video.Standard",
            [VideoAudioSettingsConstants.AspectRatioOption] = "Emulation.Help.Video.AspectRatio",
            [VideoAudioSettingsConstants.FrameSkipOption] = "Emulation.Help.Video.FrameSkip",
            [SettingsConstants.Region] = "Emulation.Help.Video.Region",
            [EightBitSettingsConstants.ArtifactingModeOptionKey] = "Emulation.Help.Video.Artifacting",
            [EightBitSettingsConstants.ColorGammaOptionKey] = "Emulation.Help.Video.Gamma",
            [EightBitSettingsConstants.ColorDelayOptionKey] = "Emulation.Help.Video.ColorDelay",
            [EightBitSettingsConstants.ExternalPaletteOptionKey] = "Emulation.Help.Video.ExternalPalette",
            [VideoAudioSettingsConstants.AudioLatencyOption] = "Emulation.Help.Audio.Latency",
            [VideoAudioSettingsConstants.PolarizedFilterOption] = "Emulation.Help.Audio.PolarizedFilter",
            [EightBitSettingsConstants.PokeyStereoOptionKey] = "Emulation.Help.Audio.PokeyStereo",
            [EightBitSettingsConstants.ControllerCompatibilityOptionKey] = "Emulation.Help.Controller.Compatibility",
            [EightBitSettingsConstants.DigitalSensitivityOptionKey] = "Emulation.Help.Controller.DigitalSensitivity",
            [EightBitSettingsConstants.AnalogSensitivityOptionKey] = "Emulation.Help.Controller.AnalogSensitivity",
            [EightBitSettingsConstants.AutofireOptionKey] = "Emulation.Help.Controller.Autofire",
            [EightBitSettingsConstants.PaddleMovementSpeedOptionKey] = "Emulation.Help.Controller.PaddleSpeed",
            [EightBitSettingsConstants.CassetteBootOptionKey] = "Emulation.Help.Storage.CassetteBoot",
            [EightBitSettingsConstants.RealTimeClockOptionKey] = "Emulation.Help.Storage.RealTimeClock",
            [EightBitSettingsConstants.PrinterDeviceOptionKey] = "Emulation.Help.Storage.PrinterDevice",
            [EightBitSettingsConstants.SerialDeviceOptionKey] = "Emulation.Help.Storage.SerialDevice"
        };

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var compatibility = CompatibilityCatalog.Get(configuration.Model);
        var blocks = (compatibility.Core == Emulator.Hatari
            ? CreateSt(configuration)
            : CreateHardware(configuration)).ToList();
        AddGeneralFolders(configuration, compatibility, blocks);
        AddMouseSettings(configuration, compatibility, blocks);
        return blocks;
    }

}
