using AltFlip = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.AltFlip;
using AnalogAxisModifier = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.AnalogAxisModifier;
using AnalogCombo = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.AnalogCombo;
using Bios = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.Bios;
using DisplayFpsV2 = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.DisplayFpsV2;
using Dithering = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.Dithering;
using FrameskipType = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.FrameskipType;
using Memcard1 = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.Memcard1;
using Memcard2 = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.Memcard2;
using Multitap = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.Multitap;
using Region = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.Region;
using ScreenCentering = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.ScreenCentering;
using ShowBiosBootlogo = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.ShowBiosBootlogo;
using ShowOverscan = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.ShowOverscan;
using SpuInterpolation = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Enums.SpuInterpolation;
using GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;

namespace GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants;

internal static class OptionConstants
{
    internal const string RegionKey = "pcsx_rearmed_region";
    internal const string RegionLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_region.Label";
    internal const string RegionHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_region.Help";
    internal const string RegionAuto = "auto";
    internal const string RegionNTSC = "NTSC";
    internal const string RegionPAL = "PAL";
    internal static readonly string RegionDefaultValue = OptionValueFunctions.ToNative(Region.Auto);
    internal const string BiosKey = "pcsx_rearmed_bios";
    internal const string BiosLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_bios.Label";
    internal const string BiosHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_bios.Help";
    internal const string BiosAuto = "auto";
    internal const string BiosHLE = "HLE";
    internal static readonly string BiosDefaultValue = OptionValueFunctions.ToNative(Bios.Auto);
    internal const string ShowBiosBootlogoKey = "pcsx_rearmed_show_bios_bootlogo";
    internal const string ShowBiosBootlogoLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_show_bios_bootlogo.Label";
    internal const string ShowBiosBootlogoHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_show_bios_bootlogo.Help";
    internal const string ShowBiosBootlogoDisabled = "disabled";
    internal const string ShowBiosBootlogoEnabled = "enabled";
    internal const string ShowBiosBootlogoEnabledNoPcsx = "enabled_no_pcsx";
    internal static readonly string ShowBiosBootlogoDefaultValue = OptionValueFunctions.ToNative(ShowBiosBootlogo.Disabled);
    internal const string Memcard1Key = "pcsx_rearmed_memcard1";
    internal const string Memcard1LabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_memcard1.Label";
    internal const string Memcard1HelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_memcard1.Help";
    internal const string Memcard1Libretro = "libretro";
    internal const string Memcard1Serial = "serial";
    internal const string Memcard1Shared = "shared";
    internal const string Memcard1None = "none";
    internal static readonly string Memcard1DefaultValue = OptionValueFunctions.ToNative(Memcard1.Libretro);
    internal const string Memcard2Key = "pcsx_rearmed_memcard2";
    internal const string Memcard2LabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_memcard2.Label";
    internal const string Memcard2HelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_memcard2.Help";
    internal const string Memcard2Serial = "serial";
    internal const string Memcard2Shared = "shared";
    internal const string Memcard2None = "none";
    internal static readonly string Memcard2DefaultValue = OptionValueFunctions.ToNative(Memcard2.Shared);
    internal const string CdReadaheadKey = "pcsx_rearmed_cd_readahead";
    internal const string CdReadaheadLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_cd_readahead.Label";
    internal const string CdReadaheadHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_cd_readahead.Help";
    internal static IReadOnlyList<EmulationSettingsNumericRange> CdReadaheadRanges { get; } =
    [
        new(0, 16, 1),
        new(32, 64, 32),
        new(128, 256, 128),
        new(512, 1024, 512),
        new(333000, 333000, 1),
    ];
    internal const string CdReadaheadDefaultValue = "12";
    internal const string DrcKey = "pcsx_rearmed_drc";
    internal const string DrcLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_drc.Label";
    internal const string DrcHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_drc.Help";
    internal const string PsxclockKey = "pcsx_rearmed_psxclock";
    internal const string PsxclockLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_psxclock.Label";
    internal const string PsxclockHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_psxclock.Help";
    internal static IReadOnlyList<EmulationSettingsNumericRange> PsxclockRanges { get; } =
    [
        new(30, 100, 1),
    ];
    internal const string PsxclockDefaultValue = "auto";
    internal const string PsxclockAuto = "auto";
    internal const string DitheringKey = "pcsx_rearmed_dithering";
    internal const string DitheringLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_dithering.Label";
    internal const string DitheringHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_dithering.Help";
    internal const string DitheringDisabled = "disabled";
    internal const string DitheringEnabled = "enabled";
    internal const string DitheringForce = "force";
    internal static readonly string DitheringDefaultValue = OptionValueFunctions.ToNative(Dithering.Enabled);
    internal const string GpuThreadRenderingKey = "pcsx_rearmed_gpu_thread_rendering";
    internal const string GpuThreadRenderingLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_gpu_thread_rendering.Label";
    internal const string GpuThreadRenderingHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_gpu_thread_rendering.Help";
    internal const string GpuThreadRenderingAuto = "auto";
    internal const string GpuThreadRenderingDisabled = "disabled";
    internal const string GpuThreadRenderingEnabled = "enabled";
    internal static readonly string GpuThreadRenderingDefaultValue = OptionValueFunctions.ToNative(AutomaticToggleState.Auto);
    internal const string FrameskipTypeKey = "pcsx_rearmed_frameskip_type";
    internal const string FrameskipTypeLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_frameskip_type.Label";
    internal const string FrameskipTypeHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_frameskip_type.Help";
    internal const string FrameskipTypeDisabled = "disabled";
    internal const string FrameskipTypeAuto = "auto";
    internal const string FrameskipTypeAutoThreshold = "auto_threshold";
    internal const string FrameskipTypeFixedInterval = "fixed_interval";
    internal static readonly string FrameskipTypeDefaultValue = OptionValueFunctions.ToNative(FrameskipType.Disabled);
    internal const string FrameskipThresholdKey = "pcsx_rearmed_frameskip_threshold";
    internal const string FrameskipThresholdLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_frameskip_threshold.Label";
    internal const string FrameskipThresholdHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_frameskip_threshold.Help";
    internal static IReadOnlyList<EmulationSettingsNumericRange> FrameskipThresholdRanges { get; } =
    [
        new(15, 60, 3),
        new(65, 80, 5),
    ];
    internal const string FrameskipThresholdDefaultValue = "33";
    internal const string FrameskipIntervalKey = "pcsx_rearmed_frameskip_interval";
    internal const string FrameskipIntervalLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_frameskip_interval.Label";
    internal const string FrameskipIntervalHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_frameskip_interval.Help";
    internal static EmulationSettingsNumericRange FrameskipIntervalRange { get; } = new(1, 10, 1);
    internal const string FrameskipIntervalDefaultValue = "3";
    internal const string DisplayFpsV2Key = "pcsx_rearmed_display_fps_v2";
    internal const string DisplayFpsV2LabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_display_fps_v2.Label";
    internal const string DisplayFpsV2HelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_display_fps_v2.Help";
    internal const string DisplayFpsV2Disabled = "disabled";
    internal const string DisplayFpsV2Enabled = "enabled";
    internal const string DisplayFpsV2Extra = "extra";
    internal static readonly string DisplayFpsV2DefaultValue = OptionValueFunctions.ToNative(DisplayFpsV2.Disabled);
    internal const string DisplayInfoKey = "pcsx_rearmed_display_info";
    internal const string DisplayInfoLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_display_info.Label";
    internal const string DisplayInfoHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_display_info.Help";
    internal const string FractionalFramerateKey = "pcsx_rearmed_fractional_framerate";
    internal const string FractionalFramerateLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_fractional_framerate.Label";
    internal const string FractionalFramerateHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_fractional_framerate.Help";
    internal static readonly string FractionalFramerateDefaultValue = OptionValueFunctions.ToNative(AutomaticToggleState.Auto);
    internal const string AltFlipKey = "pcsx_rearmed_alt_flip";
    internal const string AltFlipLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_alt_flip.Label";
    internal const string AltFlipHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_alt_flip.Help";
    internal const string AltFlipAuto = "auto";
    internal const string AltFlipEarly = "early";
    internal const string AltFlipLate = "late";
    internal static readonly string AltFlipDefaultValue = OptionValueFunctions.ToNative(AltFlip.Auto);
    internal const string Rgb32OutputKey = "pcsx_rearmed_rgb32_output";
    internal const string Rgb32OutputLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_rgb32_output.Label";
    internal const string Rgb32OutputHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_rgb32_output.Help";
    internal const string ScaleHiresKey = "pcsx_rearmed_scale_hires";
    internal const string ScaleHiresLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_scale_hires.Label";
    internal const string ScaleHiresHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_scale_hires.Help";
    internal const string GpuSlowLlistsKey = "pcsx_rearmed_gpu_slow_llists";
    internal const string GpuSlowLlistsLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_gpu_slow_llists.Label";
    internal const string GpuSlowLlistsHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_gpu_slow_llists.Help";
    internal static readonly string GpuSlowLlistsDefaultValue = OptionValueFunctions.ToNative(AutomaticToggleState.Auto);
    internal const string ShowOverscanKey = "pcsx_rearmed_show_overscan";
    internal const string ShowOverscanLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_show_overscan.Label";
    internal const string ShowOverscanHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_show_overscan.Help";
    internal const string ShowOverscanDisabled = "disabled";
    internal const string ShowOverscanAuto = "auto";
    internal const string ShowOverscanHack = "hack";
    internal static readonly string ShowOverscanDefaultValue = OptionValueFunctions.ToNative(ShowOverscan.Disabled);
    internal const string ScreenCenteringKey = "pcsx_rearmed_screen_centering";
    internal const string ScreenCenteringLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_screen_centering.Label";
    internal const string ScreenCenteringHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_screen_centering.Help";
    internal const string ScreenCenteringAuto = "auto";
    internal const string ScreenCenteringGame = "game";
    internal const string ScreenCenteringBorderless = "borderless";
    internal const string ScreenCenteringManual = "manual";
    internal static readonly string ScreenCenteringDefaultValue = OptionValueFunctions.ToNative(ScreenCentering.Auto);
    internal const string ScreenCenteringXKey = "pcsx_rearmed_screen_centering_x";
    internal const string ScreenCenteringXLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_screen_centering_x.Label";
    internal const string ScreenCenteringXHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_screen_centering_x.Help";
    internal static EmulationSettingsNumericRange ScreenCenteringXRange { get; } = new(-16, 16, 2);
    internal const string ScreenCenteringXDefaultValue = "0";
    internal const string ScreenCenteringYKey = "pcsx_rearmed_screen_centering_y";
    internal const string ScreenCenteringYLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_screen_centering_y.Label";
    internal const string ScreenCenteringYHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_screen_centering_y.Help";
    internal static EmulationSettingsNumericRange ScreenCenteringYRange { get; } = new(-16, 16, 1);
    internal const string ScreenCenteringYDefaultValue = "0";
    internal const string ScreenCenteringHAdjKey = "pcsx_rearmed_screen_centering_h_adj";
    internal const string ScreenCenteringHAdjLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_screen_centering_h_adj.Label";
    internal const string ScreenCenteringHAdjHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_screen_centering_h_adj.Help";
    internal static IReadOnlyList<EmulationSettingsNumericRange> ScreenCenteringHAdjRanges { get; } =
    [
        new(-64, -48, 16),
        new(-40, -8, 8),
        new(-7, 0, 1),
    ];
    internal const string ScreenCenteringHAdjDefaultValue = "0";
    internal const string NeonInterlaceEnableV2Key = "pcsx_rearmed_neon_interlace_enable_v2";
    internal const string NeonInterlaceEnableV2LabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_neon_interlace_enable_v2.Label";
    internal const string NeonInterlaceEnableV2HelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_neon_interlace_enable_v2.Help";
    internal static readonly string NeonInterlaceEnableV2DefaultValue = OptionValueFunctions.ToNative(AutomaticToggleState.Auto);
    internal const string NeonEnhancementEnableKey = "pcsx_rearmed_neon_enhancement_enable";
    internal const string NeonEnhancementEnableLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_neon_enhancement_enable.Label";
    internal const string NeonEnhancementEnableHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_neon_enhancement_enable.Help";
    internal const string NeonEnhancementNoMainKey = "pcsx_rearmed_neon_enhancement_no_main";
    internal const string NeonEnhancementNoMainLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_neon_enhancement_no_main.Label";
    internal const string NeonEnhancementNoMainHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_neon_enhancement_no_main.Help";
    internal const string NeonEnhancementTexAdjV2Key = "pcsx_rearmed_neon_enhancement_tex_adj_v2";
    internal const string NeonEnhancementTexAdjV2LabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_neon_enhancement_tex_adj_v2.Label";
    internal const string NeonEnhancementTexAdjV2HelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_neon_enhancement_tex_adj_v2.Help";
    internal const string SpuReverbKey = "pcsx_rearmed_spu_reverb";
    internal const string SpuReverbLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_spu_reverb.Label";
    internal const string SpuReverbHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_spu_reverb.Help";
    internal const string SpuInterpolationKey = "pcsx_rearmed_spu_interpolation";
    internal const string SpuInterpolationLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_spu_interpolation.Label";
    internal const string SpuInterpolationHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_spu_interpolation.Help";
    internal const string SpuInterpolationSimple = "simple";
    internal const string SpuInterpolationGaussian = "gaussian";
    internal const string SpuInterpolationCubic = "cubic";
    internal const string SpuInterpolationOff = "off";
    internal static readonly string SpuInterpolationDefaultValue = OptionValueFunctions.ToNative(SpuInterpolation.Simple);
    internal const string NocdaudioKey = "pcsx_rearmed_nocdaudio";
    internal const string NocdaudioLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_nocdaudio.Label";
    internal const string NocdaudioHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_nocdaudio.Help";
    internal const string NoxadecodingKey = "pcsx_rearmed_noxadecoding";
    internal const string NoxadecodingLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_noxadecoding.Label";
    internal const string NoxadecodingHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_noxadecoding.Help";
    internal const string ShowInputSettingsKey = "pcsx_rearmed_show_input_settings";
    internal const string ShowInputSettingsLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_show_input_settings.Label";
    internal const string ShowInputSettingsHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_show_input_settings.Help";
    internal const string AnalogAxisModifierKey = "pcsx_rearmed_analog_axis_modifier";
    internal const string AnalogAxisModifierLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_analog_axis_modifier.Label";
    internal const string AnalogAxisModifierHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_analog_axis_modifier.Help";
    internal const string AnalogAxisModifierCircle = "circle";
    internal const string AnalogAxisModifierSquare = "square";
    internal static readonly string AnalogAxisModifierDefaultValue = OptionValueFunctions.ToNative(AnalogAxisModifier.Square);
    internal const string VibrationKey = "pcsx_rearmed_vibration";
    internal const string VibrationLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_vibration.Label";
    internal const string VibrationHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_vibration.Help";
    internal const string AnalogComboKey = "pcsx_rearmed_analog_combo";
    internal const string AnalogComboLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_analog_combo.Label";
    internal const string AnalogComboHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_analog_combo.Help";
    internal const string AnalogComboDisabled = "disabled";
    internal const string AnalogComboL1PlusR1PlusSelect = "l1+r1+select";
    internal const string AnalogComboL1PlusR1PlusStart = "l1+r1+start";
    internal const string AnalogComboL1PlusR1PlusL3 = "l1+r1+l3";
    internal const string AnalogComboL1PlusR1PlusR3 = "l1+r1+r3";
    internal const string AnalogComboL3PlusR3 = "l3+r3";
    internal static readonly string AnalogComboDefaultValue = OptionValueFunctions.ToNative(AnalogCombo.L1PlusR1PlusSelect);
    internal const string MultitapKey = "pcsx_rearmed_multitap";
    internal const string MultitapLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_multitap.Label";
    internal const string MultitapHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_multitap.Help";
    internal const string MultitapDisabled = "disabled";
    internal const string MultitapPort1 = "port 1";
    internal const string MultitapPort2 = "port 2";
    internal const string MultitapPorts1And2 = "ports 1 and 2";
    internal static readonly string MultitapDefaultValue = OptionValueFunctions.ToNative(Multitap.Disabled);
    internal const string NegconDeadzoneKey = "pcsx_rearmed_negcon_deadzone";
    internal const string NegconDeadzoneLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_negcon_deadzone.Label";
    internal const string NegconDeadzoneHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_negcon_deadzone.Help";
    internal static IReadOnlyList<EmulationSettingsNumericRange> NegconDeadzoneRanges { get; } =
    [
        new(0, 3, 3),
        new(5, 7, 2),
        new(10, 13, 3),
        new(15, 17, 2),
        new(20, 23, 3),
        new(25, 27, 2),
        new(30, 30, 1),
    ];
    internal const string NegconDeadzoneDefaultValue = "0";
    internal const string NegconResponseKey = "pcsx_rearmed_negcon_response";
    internal const string NegconResponseLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_negcon_response.Label";
    internal const string NegconResponseHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_negcon_response.Help";
    internal const string NegconResponseLinear = "linear";
    internal const string NegconResponseQuadratic = "quadratic";
    internal const string NegconResponseCubic = "cubic";
    internal static readonly string NegconResponseDefaultValue = OptionValueFunctions.ToNative(AnalogResponseCurve.Linear);
    internal const string InputSensitivityKey = "pcsx_rearmed_input_sensitivity";
    internal const string InputSensitivityLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_input_sensitivity.Label";
    internal const string InputSensitivityHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_input_sensitivity.Help";
    internal static EmulationSettingsNumericRange InputSensitivityRange { get; } = new(0.05, 2.00, 0.05);
    internal const string InputSensitivityDefaultValue = "1.00";
    internal const string Crosshair1Key = "pcsx_rearmed_crosshair1";
    internal const string Crosshair1LabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_crosshair1.Label";
    internal const string Crosshair1HelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_crosshair1.Help";
    internal const string Crosshair1Disabled = "disabled";
    internal const string Crosshair1Blue = "blue";
    internal const string Crosshair1Green = "green";
    internal const string Crosshair1Red = "red";
    internal const string Crosshair1White = "white";
    internal static readonly string Crosshair1DefaultValue = OptionValueFunctions.ToNative(CrosshairVisibility.Disabled);
    internal const string Crosshair2Key = "pcsx_rearmed_crosshair2";
    internal const string Crosshair2LabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_crosshair2.Label";
    internal const string Crosshair2HelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_crosshair2.Help";
    internal static readonly string Crosshair2DefaultValue = OptionValueFunctions.ToNative(CrosshairVisibility.Disabled);
    internal const string KonamigunadjustxKey = "pcsx_rearmed_konamigunadjustx";
    internal const string KonamigunadjustxLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_konamigunadjustx.Label";
    internal const string KonamigunadjustxHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_konamigunadjustx.Help";
    internal static EmulationSettingsNumericRange KonamigunadjustxRange { get; } = new(-40, 40, 1);
    internal const string KonamigunadjustxDefaultValue = "0";
    internal const string KonamigunadjustyKey = "pcsx_rearmed_konamigunadjusty";
    internal const string KonamigunadjustyLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_konamigunadjusty.Label";
    internal const string KonamigunadjustyHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_konamigunadjusty.Help";
    internal static EmulationSettingsNumericRange KonamigunadjustyRange { get; } = new(-40, 40, 1);
    internal const string KonamigunadjustyDefaultValue = "0";
    internal const string GunconadjustxKey = "pcsx_rearmed_gunconadjustx";
    internal const string GunconadjustxLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_gunconadjustx.Label";
    internal const string GunconadjustxHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_gunconadjustx.Help";
    internal static EmulationSettingsNumericRange GunconadjustxRange { get; } = new(-40, 40, 1);
    internal const string GunconadjustxDefaultValue = "0";
    internal const string GunconadjustyKey = "pcsx_rearmed_gunconadjusty";
    internal const string GunconadjustyLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_gunconadjusty.Label";
    internal const string GunconadjustyHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_gunconadjusty.Help";
    internal static EmulationSettingsNumericRange GunconadjustyRange { get; } = new(-40, 40, 1);
    internal const string GunconadjustyDefaultValue = "0";
    internal const string GunconadjustratioxKey = "pcsx_rearmed_gunconadjustratiox";
    internal const string GunconadjustratioxLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_gunconadjustratiox.Label";
    internal const string GunconadjustratioxHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_gunconadjustratiox.Help";
    internal static EmulationSettingsNumericRange GunconadjustratioxRange { get; } = new(0.75, 1.25, 0.01);
    internal const string GunconadjustratioxDefaultValue = "1.00";
    internal const string GunconadjustratioyKey = "pcsx_rearmed_gunconadjustratioy";
    internal const string GunconadjustratioyLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_gunconadjustratioy.Label";
    internal const string GunconadjustratioyHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_gunconadjustratioy.Help";
    internal static EmulationSettingsNumericRange GunconadjustratioyRange { get; } = new(0.75, 1.25, 0.01);
    internal const string GunconadjustratioyDefaultValue = "1.00";
    internal const string IcacheEmulationKey = "pcsx_rearmed_icache_emulation";
    internal const string IcacheEmulationLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_icache_emulation.Label";
    internal const string IcacheEmulationHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_icache_emulation.Help";
    internal const string ExceptionEmulationKey = "pcsx_rearmed_exception_emulation";
    internal const string ExceptionEmulationLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_exception_emulation.Label";
    internal const string ExceptionEmulationHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_exception_emulation.Help";
    internal const string CdTurboKey = "pcsx_rearmed_cd_turbo";
    internal const string CdTurboLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_cd_turbo.Label";
    internal const string CdTurboHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_cd_turbo.Help";
    internal const string NostallsKey = "pcsx_rearmed_nostalls";
    internal const string NostallsLabelResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_nostalls.Label";
    internal const string NostallsHelpResource = "Emulation.Option.pcsx_rearmed.pcsx_rearmed_nostalls.Help";
}
