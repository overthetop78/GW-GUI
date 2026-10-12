using GWGUI.Emulation.Snk.Emulators.FbAlpha2012NeoGeo.Constants;

namespace GWGUI.Emulation.Snk.Emulators.FbAlpha2012NeoGeo.Dictionaries;

internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> Options { get; } = Array.AsReadOnly(new CoreOption[] { new(
        RuntimeOptionConstants.Aspect,
        "Core-Provided Aspect Ratio",
        null,
        EmulationMachineTab.Video.ToString(),
        "DAR",
        "DAR",
        [new("DAR", "DAR", ResourceKey: SettingsChoiceConstants.ValueDAR), new("PAR", "PAR", ResourceKey: SettingsChoiceConstants.ValuePAR)],
        ResourceKey: SettingsTextConstants.SettingCoreProvidedAspectRatio
    ), new(
        RuntimeOptionConstants.CpuSpeedAdjust,
        "CPU Speed (%)",
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
        ResourceKey: SettingsTextConstants.SettingCPUClock
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
        ResourceKey: SettingsTextConstants.SettingDiagnosticInput
    ), new(
        RuntimeOptionConstants.LowpassFilter,
        "Audio Filter",
        null,
        EmulationMachineTab.Audio.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("enabled", "enabled")],
        ResourceKey: SettingsTextConstants.SettingLowPassFilter
    ), new(
        RuntimeOptionConstants.LowpassRange,
        "Audio Filter Level (%)",
        null,
        EmulationMachineTab.Audio.ToString(),
        "60",
        "60",
        [
        new("5", "5", ResourceKey: SettingsChoiceConstants.Value5),
        new("10", "10", ResourceKey: SettingsChoiceConstants.Value10),
        new("15", "15", ResourceKey: SettingsChoiceConstants.Value15),
        new("20", "20", ResourceKey: SettingsChoiceConstants.Value20),
        new("25", "25", ResourceKey: SettingsChoiceConstants.Value25),
        new("30", "30", ResourceKey: SettingsChoiceConstants.Value30),
        new("35", "35", ResourceKey: SettingsChoiceConstants.Value35),
        new("40", "40", ResourceKey: SettingsChoiceConstants.Value40),
        new("45", "45", ResourceKey: SettingsChoiceConstants.Value45),
        new("50", "50", ResourceKey: SettingsChoiceConstants.Value50),
        new("55", "55", ResourceKey: SettingsChoiceConstants.Value55),
        new("60", "60", ResourceKey: SettingsChoiceConstants.Value60),
        new("65", "65", ResourceKey: SettingsChoiceConstants.Value65),
        new("70", "70", ResourceKey: SettingsChoiceConstants.Value70),
        new("75", "75", ResourceKey: SettingsChoiceConstants.Value75),
        new("80", "80", ResourceKey: SettingsChoiceConstants.Value80),
        new("85", "85", ResourceKey: SettingsChoiceConstants.Value85),
        new("90", "90", ResourceKey: SettingsChoiceConstants.Value90),
        new("95", "95", ResourceKey: SettingsChoiceConstants.Value95)
    ],
        ResourceKey: SettingsTextConstants.SettingLowPassFilterLevel
    ), new(
        RuntimeOptionConstants.Frameskip,
        "Frameskip",
        null,
        EmulationMachineTab.Video.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("Auto", "Auto"), new("Manual", "Manual", ResourceKey: SettingsChoiceConstants.ValueManual)],
        ResourceKey: SettingsTextConstants.SettingFrameskip
    ), new(
        RuntimeOptionConstants.FrameskipThreshold,
        "Frameskip Threshold (%)",
        null,
        EmulationMachineTab.Video.ToString(),
        "33",
        "33",
        [
        new("15", "15", ResourceKey: SettingsChoiceConstants.Value15),
        new("18", "18", ResourceKey: SettingsChoiceConstants.Value18),
        new("21", "21", ResourceKey: SettingsChoiceConstants.Value21),
        new("24", "24", ResourceKey: SettingsChoiceConstants.Value24),
        new("27", "27", ResourceKey: SettingsChoiceConstants.Value27),
        new("30", "30", ResourceKey: SettingsChoiceConstants.Value30),
        new("33", "33", ResourceKey: SettingsChoiceConstants.Value33),
        new("36", "36", ResourceKey: SettingsChoiceConstants.Value36),
        new("39", "39", ResourceKey: SettingsChoiceConstants.Value39),
        new("42", "42", ResourceKey: SettingsChoiceConstants.Value42),
        new("45", "45", ResourceKey: SettingsChoiceConstants.Value45),
        new("48", "48", ResourceKey: SettingsChoiceConstants.Value48),
        new("51", "51", ResourceKey: SettingsChoiceConstants.Value51),
        new("54", "54", ResourceKey: SettingsChoiceConstants.Value54),
        new("57", "57", ResourceKey: SettingsChoiceConstants.Value57),
        new("60", "60", ResourceKey: SettingsChoiceConstants.Value60)
    ],
        ResourceKey: SettingsTextConstants.SettingFrameskipThreshold
    ), new(
        RuntimeOptionConstants.NeoGeoMode,
        "Neo Geo Mode",
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
        ResourceKey: SettingsTextConstants.SettingNeoGeoMode
    ), });
}
