using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Enums;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Dictionaries;

internal static partial class OptionCatalog
{
    private static IReadOnlyList<CoreOption> CpuOptions { get; } =
    [
        Define(OptionConstants.VuAccurateAddsubKey, OptionConstants.VuAccurateAddsubLabel, OptionConstants.VuAccurateAddsubHelp,
            "disabled", EmulationMachineTab.Cpu,
            ToggleChoices),
        Define(OptionConstants.EeAccurateFpuKey, OptionConstants.EeAccurateFpuLabel, OptionConstants.EeAccurateFpuHelp,
            "enabled", EmulationMachineTab.Cpu,
            ToggleChoices),
        Define(OptionConstants.VuExactMulKey, OptionConstants.VuExactMulLabel, OptionConstants.VuExactMulHelp,
            "disabled", EmulationMachineTab.Cpu,
            ToggleChoices),
        Define(OptionConstants.VuExactDivKey, OptionConstants.VuExactDivLabel, OptionConstants.VuExactDivHelp,
            "disabled", EmulationMachineTab.Cpu,
            ToggleChoices),
        Define(OptionConstants.FpuSoftfloatKey, OptionConstants.FpuSoftfloatLabel, OptionConstants.FpuSoftfloatHelp,
            "disabled", EmulationMachineTab.Cpu,
            ToggleChoices),
        Define(OptionConstants.Vu0SoftfloatKey, OptionConstants.Vu0SoftfloatLabel, OptionConstants.Vu0SoftfloatHelp,
            "disabled", EmulationMachineTab.Cpu,
            ToggleChoices),
        Define(OptionConstants.Vu1SoftfloatKey, OptionConstants.Vu1SoftfloatLabel, OptionConstants.Vu1SoftfloatHelp,
            "disabled", EmulationMachineTab.Cpu,
            ToggleChoices),
        Define(OptionConstants.EeCycleRateKey, OptionConstants.EeCycleRateLabel, null,
            "100% (Normal Speed)", EmulationMachineTab.Cpu,
            [
                new("50% (Underclock)", "50 %"),
                new("60% (Underclock)", "60 %"),
                new("75% (Underclock)", "75 %"),
                new("100% (Normal Speed)", "100 %"),
                new("130% (Overclock)", "130 %"),
                new("180% (Overclock)", "180 %"),
                new("300% (Overclock)", "300 %"),
            ]),
        Define(OptionConstants.EeCpuKey, OptionConstants.EeCpuLabel, OptionConstants.EeCpuHelp,
            "Recompiler", EmulationMachineTab.Cpu,
            [
                new(OptionValueFunctions.ToNative(CpuExecutionProvider.Recompiler), "Emulation.Option.Sony.Value.Recompiler"),
                new(OptionValueFunctions.ToNative(CpuExecutionProvider.Interpreter), "Emulation.Option.Sony.Value.Interpreter"),
            ], restart: true),
        Define(OptionConstants.FastmemKey, OptionConstants.FastmemLabel, OptionConstants.FastmemHelp,
            "enabled", EmulationMachineTab.Cpu,
            ToggleChoices, restart: true),
        Define(OptionConstants.MtvuKey, OptionConstants.MtvuLabel, OptionConstants.MtvuHelp,
            "disabled", EmulationMachineTab.Cpu,
            ToggleChoices, restart: true),
        Define(OptionConstants.InstantVu1Key, OptionConstants.InstantVu1Label, OptionConstants.InstantVu1Help,
            "enabled", EmulationMachineTab.Cpu,
            ToggleChoices, restart: true),
        Define(OptionConstants.EeCycleSkipKey, OptionConstants.EeCycleSkipLabel, null,
            "disabled", EmulationMachineTab.Cpu,
            [
                new(OptionValueFunctions.ToNative(CycleSkipping.Disabled), OptionValueResourceConstants.DisabledLabelResource),
                new(OptionValueFunctions.ToNative(CycleSkipping.Mild), OptionValueResourceConstants.LowLabelResource),
                new(OptionValueFunctions.ToNative(CycleSkipping.Moderate), OptionValueResourceConstants.MediumLabelResource),
                new(OptionValueFunctions.ToNative(CycleSkipping.Maximum), "Emulation.Option.Sony.Value.Maximum"),
            ]),
        Define(OptionConstants.GameEnhancementsHintKey, OptionConstants.GameEnhancementsHintLabel, OptionConstants.GameEnhancementsHintHelp,
            "disabled", EmulationMachineTab.Cheats,
            ToggleChoices, restart: true),
        Define(OptionConstants.UncappedFramerateHintKey, OptionConstants.UncappedFramerateHintLabel, OptionConstants.UncappedFramerateHintHelp,
            "disabled", EmulationMachineTab.Video,
            [
                new(OptionValueFunctions.ToNative(FrameratePatch.Disabled), OptionValueResourceConstants.DisabledLabelResource),
                new(OptionValueFunctions.ToNative(FrameratePatch.Enabled), OptionValueResourceConstants.EnabledLabelResource),
                new(OptionValueFunctions.ToNative(FrameratePatch.PalToNtsc), "PAL → NTSC (60 FPS)"),
            ], restart: true),
    ];
}
