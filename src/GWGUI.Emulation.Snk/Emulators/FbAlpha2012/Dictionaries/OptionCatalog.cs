using GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Constants;

namespace GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Dictionaries;

internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> Options { get; } = Array.AsReadOnly(new CoreOption[] { new(
        RuntimeOptionConstants.AllowDepth32,
        "Use 32-bits color depth when available",
        null,
        EmulationMachineTab.Video.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("enabled", "enabled")],
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingUse32BitsColorDepthWhenAvailable,
        ExplanationResourceKey: SettingsTextConstants.HelpUse32BitsColorDepthWhenAvailable
    ), new(
        RuntimeOptionConstants.VerticalMode,
        "Vertical mode",
        null,
        EmulationMachineTab.Video.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("enabled", "enabled")],
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingVerticalMode,
        ExplanationResourceKey: SettingsTextConstants.HelpVerticalMode
    ), new(
        RuntimeOptionConstants.Frameskip,
        "Frameskip",
        null,
        EmulationMachineTab.Video.ToString(),
        "0",
        "0",
        [
        new("0", "0", ResourceKey: SettingsChoiceConstants.Value0),
        new("1", "1", ResourceKey: SettingsChoiceConstants.Value1),
        new("2", "2", ResourceKey: SettingsChoiceConstants.Value2),
        new("3", "3", ResourceKey: SettingsChoiceConstants.Value3),
        new("4", "4", ResourceKey: SettingsChoiceConstants.Value4),
        new("5", "5", ResourceKey: SettingsChoiceConstants.Value5)
    ],
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingFrameskip
    ), new(
        RuntimeOptionConstants.CpuSpeedAdjust,
        "CPU overclock",
        null,
        EmulationMachineTab.Cpu.ToString(),
        "100",
        "100",
        [
        new("100", "100", ResourceKey: SettingsChoiceConstants.Value100),
        new("110", "110", ResourceKey: SettingsChoiceConstants.Value110),
        new("120", "120", ResourceKey: SettingsChoiceConstants.Value120),
        new("130", "130", ResourceKey: SettingsChoiceConstants.Value130),
        new("140", "140", ResourceKey: SettingsChoiceConstants.Value140),
        new("150", "150", ResourceKey: SettingsChoiceConstants.Value150),
        new("160", "160", ResourceKey: SettingsChoiceConstants.Value160),
        new("170", "170", ResourceKey: SettingsChoiceConstants.Value170),
        new("180", "180", ResourceKey: SettingsChoiceConstants.Value180),
        new("190", "190", ResourceKey: SettingsChoiceConstants.Value190),
        new("200", "200", ResourceKey: SettingsChoiceConstants.Value200)
    ],
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingCPUClock,
        ExplanationResourceKey: SettingsTextConstants.HelpCPUClock
    ), new(
        RuntimeOptionConstants.DiagnosticInput,
        "Diagnostic Input",
        null,
        EmulationMachineTab.Controllers.ToString(),
        "None",
        "None",
        [
        new("None", "None"),
        new("Hold Start", "Hold Start", ResourceKey: SettingsChoiceConstants.ValueStart),
        new("Start + A + B", "Start + A + B", ResourceKey: SettingsChoiceConstants.ValueStartAB),
        new("Hold Start + A + B", "Hold Start + A + B", ResourceKey: SettingsChoiceConstants.ValueStartAB),
        new("Start + L + R", "Start + L + R", ResourceKey: SettingsChoiceConstants.ValueStartLR),
        new("Hold Start + L + R", "Hold Start + L + R", ResourceKey: SettingsChoiceConstants.ValueStartLR),
        new("Hold Select", "Hold Select", ResourceKey: SettingsChoiceConstants.ValueSelect),
        new("Select + A + B", "Select + A + B", ResourceKey: SettingsChoiceConstants.ValueSelectAB),
        new("Hold Select + A + B", "Hold Select + A + B", ResourceKey: SettingsChoiceConstants.ValueSelectAB),
        new("Select + L + R", "Select + L + R", ResourceKey: SettingsChoiceConstants.ValueSelectLR),
        new("Hold Select + L + R", "Hold Select + L + R", ResourceKey: SettingsChoiceConstants.ValueSelectLR)
    ],
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingDiagnosticInput,
        ExplanationResourceKey: SettingsTextConstants.HelpDiagnosticInput
    ), new(
        RuntimeOptionConstants.Hiscores,
        "Hiscores",
        null,
        EmulationMachineTab.Storage.ToString(),
        "enabled",
        "enabled",
        [new("enabled", "enabled"), new("disabled", "disabled")],
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingHiscores,
        ExplanationResourceKey: SettingsTextConstants.HelpHiscores
    ), new(
        RuntimeOptionConstants.Samplerate,
        "Samplerate",
        null,
        EmulationMachineTab.Audio.ToString(),
        "48000",
        "48000",
        [
        new("48000", "48000", ResourceKey: SettingsChoiceConstants.Value48000),
        new("44100", "44100", ResourceKey: SettingsChoiceConstants.Value44100),
        new("22050", "22050", ResourceKey: SettingsChoiceConstants.Value22050),
        new("11025", "11025", ResourceKey: SettingsChoiceConstants.Value11025)
    ],
        RequiresRestart: true,
        ResourceKey: SettingsTextConstants.SettingSampleRate,
        ExplanationResourceKey: SettingsTextConstants.HelpSampleRate
    ), new(
        RuntimeOptionConstants.SampleInterpolation,
        "Sample Interpolation",
        null,
        EmulationMachineTab.Audio.ToString(),
        "4-point 3rd order",
        "4-point 3rd order",
        [
        new("4-point 3rd order", "4-point 3rd order", ResourceKey: SettingsChoiceConstants.ValueThirdOrderInterpolation),
        new("2-point 1st order", "2-point 1st order", ResourceKey: SettingsChoiceConstants.Value2Point1stOrder),
        new("disabled", "disabled")
    ],
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingSampleInterpolation,
        ExplanationResourceKey: SettingsTextConstants.HelpSampleInterpolation
    ), new(
        RuntimeOptionConstants.FmInterpolation,
        "FM Interpolation",
        null,
        EmulationMachineTab.Audio.ToString(),
        "4-point 3rd order",
        "4-point 3rd order",
        [new("4-point 3rd order", "4-point 3rd order", ResourceKey: SettingsChoiceConstants.ValueThirdOrderInterpolation), new("disabled", "disabled")],
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingFMInterpolation,
        ExplanationResourceKey: SettingsTextConstants.HelpFMInterpolation
    ), new(
        RuntimeOptionConstants.AnalogSpeed,
        "Analog Speed",
        null,
        EmulationMachineTab.Controllers.ToString(),
        "10",
        "10",
        [
        new("10", "10", ResourceKey: SettingsChoiceConstants.Value10),
        new("9", "9", ResourceKey: SettingsChoiceConstants.Value9),
        new("8", "8", ResourceKey: SettingsChoiceConstants.Value8),
        new("7", "7", ResourceKey: SettingsChoiceConstants.Value7),
        new("6", "6", ResourceKey: SettingsChoiceConstants.Value6),
        new("5", "5", ResourceKey: SettingsChoiceConstants.Value5),
        new("4", "4", ResourceKey: SettingsChoiceConstants.Value4),
        new("3", "3", ResourceKey: SettingsChoiceConstants.Value3),
        new("2", "2", ResourceKey: SettingsChoiceConstants.Value2),
        new("1", "1", ResourceKey: SettingsChoiceConstants.Value1)
    ],
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingAnalogSpeed,
        ExplanationResourceKey: SettingsTextConstants.HelpAnalogSpeed
    ), new(
        RuntimeOptionConstants.NeoGeoMode,
        "Force Neo Geo mode",
        null,
        EmulationMachineTab.General.ToString(),
        "MVS",
        "MVS",
        [
        new("MVS", "MVS", ResourceKey: SettingsChoiceConstants.ValueMVS),
        new("AES", "AES", ResourceKey: SettingsChoiceConstants.ValueAES),
        new("UNIBIOS", "UNIBIOS", ResourceKey: SettingsChoiceConstants.ValueUNIBIOS),
        new("DIPSWITCH", "DIPSWITCH", ResourceKey: SettingsChoiceConstants.ValueDIPSWITCH)
    ],
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingNeoGeoMode,
        ExplanationResourceKey: SettingsTextConstants.HelpNeoGeoMode
    ), });
}
