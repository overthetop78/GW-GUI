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
using GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants;

namespace GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Dictionaries;

internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new(OptionConstants.RegionKey, OptionConstants.RegionLabelResource, OptionConstants.RegionHelpResource, null,
            OptionConstants.RegionDefaultValue, OptionConstants.RegionDefaultValue,
        [
            new(OptionValueFunctions.ToNative(Region.Auto), OptionValueResourceConstants.AutomaticLabelResource),
            new(OptionValueFunctions.ToNative(Region.NTSC), OptionValueFunctions.ToNative(Region.NTSC)),
            new(OptionValueFunctions.ToNative(Region.PAL), OptionValueFunctions.ToNative(Region.PAL)),
        ], IsVisible: true, Tab: EmulationMachineTab.General, RequiresRestart: false),
        new(OptionConstants.BiosKey, OptionConstants.BiosLabelResource, OptionConstants.BiosHelpResource, null,
            OptionConstants.BiosDefaultValue, OptionConstants.BiosDefaultValue,
        [
            new(OptionValueFunctions.ToNative(Bios.Auto), OptionValueResourceConstants.AutomaticLabelResource),
            new(OptionValueFunctions.ToNative(Bios.HLE), OptionValueFunctions.ToNative(Bios.HLE)),
        ], IsVisible: true, Tab: EmulationMachineTab.Rom, RequiresRestart: false),
        new(OptionConstants.ShowBiosBootlogoKey, OptionConstants.ShowBiosBootlogoLabelResource, OptionConstants.ShowBiosBootlogoHelpResource, null,
            OptionConstants.ShowBiosBootlogoDefaultValue, OptionConstants.ShowBiosBootlogoDefaultValue,
        [
            new(OptionValueFunctions.ToNative(ShowBiosBootlogo.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(ShowBiosBootlogo.Enabled), OptionValueResourceConstants.EnabledLabelResource),
            new(OptionValueFunctions.ToNative(ShowBiosBootlogo.EnabledNoPcsx), OptionValueResourceConstants.ONWOPCSXtmLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Rom, RequiresRestart: false),
        new(OptionConstants.Memcard1Key, OptionConstants.Memcard1LabelResource, OptionConstants.Memcard1HelpResource, null,
            OptionConstants.Memcard1DefaultValue, OptionConstants.Memcard1DefaultValue,
        [
            new(OptionValueFunctions.ToNative(Memcard1.Libretro), OptionValueResourceConstants.LibretroDefaultLabelResource),
            new(OptionValueFunctions.ToNative(Memcard1.Serial), OptionValueResourceConstants.GameCodeSerialLabelResource),
            new(OptionValueFunctions.ToNative(Memcard1.Shared), OptionValueResourceConstants.SharedBetweenAllGamesLabelResource),
            new(OptionValueFunctions.ToNative(Memcard1.None), OptionValueResourceConstants.NoMemoryCardLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Storage, RequiresRestart: true),
        new(OptionConstants.Memcard2Key, OptionConstants.Memcard2LabelResource, OptionConstants.Memcard2HelpResource, null,
            OptionConstants.Memcard2DefaultValue, OptionConstants.Memcard2DefaultValue,
        [
            new(OptionValueFunctions.ToNative(Memcard2.Serial), OptionValueResourceConstants.GameCodeSerialLabelResource),
            new(OptionValueFunctions.ToNative(Memcard2.Shared), OptionValueResourceConstants.SharedBetweenAllGamesLabelResource),
            new(OptionValueFunctions.ToNative(Memcard2.None), OptionValueResourceConstants.NoMemoryCardLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Storage, RequiresRestart: false),
        new(OptionConstants.CdReadaheadKey, OptionConstants.CdReadaheadLabelResource, OptionConstants.CdReadaheadHelpResource, null,
            OptionConstants.CdReadaheadDefaultValue, OptionConstants.CdReadaheadDefaultValue, NumericOptionValueFunctions.Choices(OptionConstants.CdReadaheadRanges, OptionNumericConstants.IntegerFormat),
            Tab: EmulationMachineTab.Storage, RequiresRestart: false),
        new(OptionConstants.DrcKey, OptionConstants.DrcLabelResource, OptionConstants.DrcHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Cpu, RequiresRestart: false),
        new(OptionConstants.PsxclockKey, OptionConstants.PsxclockLabelResource, OptionConstants.PsxclockHelpResource, null,
            OptionConstants.PsxclockDefaultValue, OptionConstants.PsxclockDefaultValue, [
            new(OptionConstants.PsxclockAuto, OptionValueResourceConstants.AutomaticLabelResource),
            ..NumericOptionValueFunctions.Choices(OptionConstants.PsxclockRanges, OptionNumericConstants.IntegerFormat),
        ],
            Tab: EmulationMachineTab.Cpu, RequiresRestart: false),
        new(OptionConstants.DitheringKey, OptionConstants.DitheringLabelResource, OptionConstants.DitheringHelpResource, null,
            OptionConstants.DitheringDefaultValue, OptionConstants.DitheringDefaultValue,
        [
            new(OptionValueFunctions.ToNative(Dithering.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(Dithering.Enabled), OptionValueResourceConstants.EnabledLabelResource),
            new(OptionValueFunctions.ToNative(Dithering.Force), OptionValueResourceConstants.ForceLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.GpuThreadRenderingKey, OptionConstants.GpuThreadRenderingLabelResource, OptionConstants.GpuThreadRenderingHelpResource, null,
            OptionConstants.GpuThreadRenderingDefaultValue, OptionConstants.GpuThreadRenderingDefaultValue,
        [
            new(OptionValueFunctions.ToNative(AutomaticToggleState.Auto), OptionValueResourceConstants.AutomaticLabelResource),
            new(OptionValueFunctions.ToNative(AutomaticToggleState.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(AutomaticToggleState.Enabled), OptionValueResourceConstants.EnabledLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.FrameskipTypeKey, OptionConstants.FrameskipTypeLabelResource, OptionConstants.FrameskipTypeHelpResource, null,
            OptionConstants.FrameskipTypeDefaultValue, OptionConstants.FrameskipTypeDefaultValue,
        [
            new(OptionValueFunctions.ToNative(FrameskipType.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(FrameskipType.Auto), OptionValueResourceConstants.AutomaticLabelResource),
            new(OptionValueFunctions.ToNative(FrameskipType.AutoThreshold), OptionValueResourceConstants.AutoThresholdLabelResource),
            new(OptionValueFunctions.ToNative(FrameskipType.FixedInterval), OptionValueResourceConstants.FixedIntervalLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.FrameskipThresholdKey, OptionConstants.FrameskipThresholdLabelResource, OptionConstants.FrameskipThresholdHelpResource, null,
            OptionConstants.FrameskipThresholdDefaultValue, OptionConstants.FrameskipThresholdDefaultValue, NumericOptionValueFunctions.Choices(OptionConstants.FrameskipThresholdRanges, OptionNumericConstants.IntegerFormat),
            Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.FrameskipIntervalKey, OptionConstants.FrameskipIntervalLabelResource, OptionConstants.FrameskipIntervalHelpResource, null,
            OptionConstants.FrameskipIntervalDefaultValue, OptionConstants.FrameskipIntervalDefaultValue, [], Tab: EmulationMachineTab.Video, RequiresRestart: false,
            NumericRange: OptionConstants.FrameskipIntervalRange, NativeNumericFormat: OptionNumericConstants.IntegerFormat, NativeZeroValue: null),
        new(OptionConstants.DisplayFpsV2Key, OptionConstants.DisplayFpsV2LabelResource, OptionConstants.DisplayFpsV2HelpResource, null,
            OptionConstants.DisplayFpsV2DefaultValue, OptionConstants.DisplayFpsV2DefaultValue,
        [
            new(OptionValueFunctions.ToNative(DisplayFpsV2.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(DisplayFpsV2.Enabled), OptionValueResourceConstants.EnabledLabelResource),
            new(OptionValueFunctions.ToNative(DisplayFpsV2.Extra), OptionValueResourceConstants.ExtraLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.DisplayInfoKey, OptionConstants.DisplayInfoLabelResource, OptionConstants.DisplayInfoHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.FractionalFramerateKey, OptionConstants.FractionalFramerateLabelResource, OptionConstants.FractionalFramerateHelpResource, null,
            OptionConstants.FractionalFramerateDefaultValue, OptionConstants.FractionalFramerateDefaultValue,
        [
            new(OptionValueFunctions.ToNative(AutomaticToggleState.Auto), OptionValueResourceConstants.AutomaticLabelResource),
            new(OptionValueFunctions.ToNative(AutomaticToggleState.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(AutomaticToggleState.Enabled), OptionValueResourceConstants.EnabledLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.AltFlipKey, OptionConstants.AltFlipLabelResource, OptionConstants.AltFlipHelpResource, null,
            OptionConstants.AltFlipDefaultValue, OptionConstants.AltFlipDefaultValue,
        [
            new(OptionValueFunctions.ToNative(AltFlip.Auto), OptionValueResourceConstants.AutomaticLabelResource),
            new(OptionValueFunctions.ToNative(AltFlip.Early), OptionValueResourceConstants.EarlyLabelResource),
            new(OptionValueFunctions.ToNative(AltFlip.Late), OptionValueResourceConstants.LateLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.Rgb32OutputKey, OptionConstants.Rgb32OutputLabelResource, OptionConstants.Rgb32OutputHelpResource, null,
            ToggleValueConstants.Disabled, ToggleValueConstants.Disabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.ScaleHiresKey, OptionConstants.ScaleHiresLabelResource, OptionConstants.ScaleHiresHelpResource, null,
            ToggleValueConstants.Disabled, ToggleValueConstants.Disabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.GpuSlowLlistsKey, OptionConstants.GpuSlowLlistsLabelResource, OptionConstants.GpuSlowLlistsHelpResource, null,
            OptionConstants.GpuSlowLlistsDefaultValue, OptionConstants.GpuSlowLlistsDefaultValue,
        [
            new(OptionValueFunctions.ToNative(AutomaticToggleState.Auto), OptionValueResourceConstants.AutomaticLabelResource),
            new(OptionValueFunctions.ToNative(AutomaticToggleState.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(AutomaticToggleState.Enabled), OptionValueResourceConstants.EnabledLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.ShowOverscanKey, OptionConstants.ShowOverscanLabelResource, OptionConstants.ShowOverscanHelpResource, null,
            OptionConstants.ShowOverscanDefaultValue, OptionConstants.ShowOverscanDefaultValue,
        [
            new(OptionValueFunctions.ToNative(ShowOverscan.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(ShowOverscan.Auto), OptionValueResourceConstants.AutomaticLabelResource),
            new(OptionValueFunctions.ToNative(ShowOverscan.Hack), OptionValueResourceConstants.HackLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.ScreenCenteringKey, OptionConstants.ScreenCenteringLabelResource, OptionConstants.ScreenCenteringHelpResource, null,
            OptionConstants.ScreenCenteringDefaultValue, OptionConstants.ScreenCenteringDefaultValue,
        [
            new(OptionValueFunctions.ToNative(ScreenCentering.Auto), OptionValueResourceConstants.AutomaticLabelResource),
            new(OptionValueFunctions.ToNative(ScreenCentering.Game), OptionValueResourceConstants.GameControlledLabelResource),
            new(OptionValueFunctions.ToNative(ScreenCentering.Borderless), OptionValueResourceConstants.BorderlessLabelResource),
            new(OptionValueFunctions.ToNative(ScreenCentering.Manual), OptionValueResourceConstants.ManualLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.ScreenCenteringXKey, OptionConstants.ScreenCenteringXLabelResource, OptionConstants.ScreenCenteringXHelpResource, null,
            OptionConstants.ScreenCenteringXDefaultValue, OptionConstants.ScreenCenteringXDefaultValue, [], Tab: EmulationMachineTab.Video, RequiresRestart: false,
            NumericRange: OptionConstants.ScreenCenteringXRange, NativeNumericFormat: OptionNumericConstants.IntegerFormat, NativeZeroValue: null),
        new(OptionConstants.ScreenCenteringYKey, OptionConstants.ScreenCenteringYLabelResource, OptionConstants.ScreenCenteringYHelpResource, null,
            OptionConstants.ScreenCenteringYDefaultValue, OptionConstants.ScreenCenteringYDefaultValue, [], Tab: EmulationMachineTab.Video, RequiresRestart: false,
            NumericRange: OptionConstants.ScreenCenteringYRange, NativeNumericFormat: OptionNumericConstants.IntegerFormat, NativeZeroValue: null),
        new(OptionConstants.ScreenCenteringHAdjKey, OptionConstants.ScreenCenteringHAdjLabelResource, OptionConstants.ScreenCenteringHAdjHelpResource, null,
            OptionConstants.ScreenCenteringHAdjDefaultValue, OptionConstants.ScreenCenteringHAdjDefaultValue, NumericOptionValueFunctions.Choices(OptionConstants.ScreenCenteringHAdjRanges, OptionNumericConstants.IntegerFormat),
            Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.NeonInterlaceEnableV2Key, OptionConstants.NeonInterlaceEnableV2LabelResource, OptionConstants.NeonInterlaceEnableV2HelpResource, null,
            OptionConstants.NeonInterlaceEnableV2DefaultValue, OptionConstants.NeonInterlaceEnableV2DefaultValue,
        [
            new(OptionValueFunctions.ToNative(AutomaticToggleState.Auto), OptionValueResourceConstants.AutomaticLabelResource),
            new(OptionValueFunctions.ToNative(AutomaticToggleState.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(AutomaticToggleState.Enabled), OptionValueResourceConstants.EnabledLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.NeonEnhancementEnableKey, OptionConstants.NeonEnhancementEnableLabelResource, OptionConstants.NeonEnhancementEnableHelpResource, null,
            ToggleValueConstants.Disabled, ToggleValueConstants.Disabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.NeonEnhancementNoMainKey, OptionConstants.NeonEnhancementNoMainLabelResource, OptionConstants.NeonEnhancementNoMainHelpResource, null,
            ToggleValueConstants.Disabled, ToggleValueConstants.Disabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.NeonEnhancementTexAdjV2Key, OptionConstants.NeonEnhancementTexAdjV2LabelResource, OptionConstants.NeonEnhancementTexAdjV2HelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.SpuReverbKey, OptionConstants.SpuReverbLabelResource, OptionConstants.SpuReverbHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Audio, RequiresRestart: false),
        new(OptionConstants.SpuInterpolationKey, OptionConstants.SpuInterpolationLabelResource, OptionConstants.SpuInterpolationHelpResource, null,
            OptionConstants.SpuInterpolationDefaultValue, OptionConstants.SpuInterpolationDefaultValue,
        [
            new(OptionValueFunctions.ToNative(SpuInterpolation.Simple), OptionValueResourceConstants.SimpleLabelResource),
            new(OptionValueFunctions.ToNative(SpuInterpolation.Gaussian), OptionValueResourceConstants.GaussianLabelResource),
            new(OptionValueFunctions.ToNative(SpuInterpolation.Cubic), OptionValueResourceConstants.CubicLabelResource),
            new(OptionValueFunctions.ToNative(SpuInterpolation.Off), OptionValueResourceConstants.DisabledLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Audio, RequiresRestart: false),
        new(OptionConstants.NocdaudioKey, OptionConstants.NocdaudioLabelResource, OptionConstants.NocdaudioHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Audio, RequiresRestart: false),
        new(OptionConstants.NoxadecodingKey, OptionConstants.NoxadecodingLabelResource, OptionConstants.NoxadecodingHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Audio, RequiresRestart: false),
        new(OptionConstants.ShowInputSettingsKey, OptionConstants.ShowInputSettingsLabelResource, OptionConstants.ShowInputSettingsHelpResource, null,
            ToggleValueConstants.Disabled, ToggleValueConstants.Disabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Controllers, RequiresRestart: false),
        new(OptionConstants.AnalogAxisModifierKey, OptionConstants.AnalogAxisModifierLabelResource, OptionConstants.AnalogAxisModifierHelpResource, null,
            OptionConstants.AnalogAxisModifierDefaultValue, OptionConstants.AnalogAxisModifierDefaultValue,
        [
            new(OptionValueFunctions.ToNative(AnalogAxisModifier.Circle), OptionValueResourceConstants.CircleLabelResource),
            new(OptionValueFunctions.ToNative(AnalogAxisModifier.Square), OptionValueResourceConstants.SquareLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Controllers, RequiresRestart: false),
        new(OptionConstants.VibrationKey, OptionConstants.VibrationLabelResource, OptionConstants.VibrationHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Controllers, RequiresRestart: false),
        new(OptionConstants.AnalogComboKey, OptionConstants.AnalogComboLabelResource, OptionConstants.AnalogComboHelpResource, null,
            OptionConstants.AnalogComboDefaultValue, OptionConstants.AnalogComboDefaultValue,
        [
            new(OptionValueFunctions.ToNative(AnalogCombo.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(AnalogCombo.L1PlusR1PlusSelect), OptionValueResourceConstants.L1PlusR1PlusSelectLabelResource),
            new(OptionValueFunctions.ToNative(AnalogCombo.L1PlusR1PlusStart), OptionValueResourceConstants.L1PlusR1PlusStartLabelResource),
            new(OptionValueFunctions.ToNative(AnalogCombo.L1PlusR1PlusL3), OptionValueResourceConstants.L1PlusR1PlusL3LabelResource),
            new(OptionValueFunctions.ToNative(AnalogCombo.L1PlusR1PlusR3), OptionValueResourceConstants.L1PlusR1PlusR3LabelResource),
            new(OptionValueFunctions.ToNative(AnalogCombo.L3PlusR3), OptionValueResourceConstants.L3PlusR3LabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Controllers, RequiresRestart: false),
        new(OptionConstants.MultitapKey, OptionConstants.MultitapLabelResource, OptionConstants.MultitapHelpResource, null,
            OptionConstants.MultitapDefaultValue, OptionConstants.MultitapDefaultValue,
        [
            new(OptionValueFunctions.ToNative(Multitap.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(Multitap.Port1), OptionValueResourceConstants.Port1LabelResource),
            new(OptionValueFunctions.ToNative(Multitap.Port2), OptionValueResourceConstants.Port2LabelResource),
            new(OptionValueFunctions.ToNative(Multitap.Ports1And2), OptionValueResourceConstants.Ports1And2LabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Controllers, RequiresRestart: false),
        new(OptionConstants.NegconDeadzoneKey, OptionConstants.NegconDeadzoneLabelResource, OptionConstants.NegconDeadzoneHelpResource, null,
            OptionConstants.NegconDeadzoneDefaultValue, OptionConstants.NegconDeadzoneDefaultValue, NumericOptionValueFunctions.Choices(OptionConstants.NegconDeadzoneRanges, OptionNumericConstants.IntegerFormat),
            Tab: EmulationMachineTab.Controllers, RequiresRestart: false),
        new(OptionConstants.NegconResponseKey, OptionConstants.NegconResponseLabelResource, OptionConstants.NegconResponseHelpResource, null,
            OptionConstants.NegconResponseDefaultValue, OptionConstants.NegconResponseDefaultValue,
        [
            new(OptionValueFunctions.ToNative(AnalogResponseCurve.Linear), OptionValueResourceConstants.LinearLabelResource),
            new(OptionValueFunctions.ToNative(AnalogResponseCurve.Quadratic), OptionValueResourceConstants.QuadraticLabelResource),
            new(OptionValueFunctions.ToNative(AnalogResponseCurve.Cubic), OptionValueResourceConstants.CubicLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Controllers, RequiresRestart: false),
        new(OptionConstants.InputSensitivityKey, OptionConstants.InputSensitivityLabelResource, OptionConstants.InputSensitivityHelpResource, null,
            OptionConstants.InputSensitivityDefaultValue, OptionConstants.InputSensitivityDefaultValue, [], Tab: EmulationMachineTab.Controllers, RequiresRestart: false,
            NumericRange: OptionConstants.InputSensitivityRange, NativeNumericFormat: OptionNumericConstants.TwoDecimalPlacesFormat, NativeZeroValue: null),
        new(OptionConstants.Crosshair1Key, OptionConstants.Crosshair1LabelResource, OptionConstants.Crosshair1HelpResource, null,
            OptionConstants.Crosshair1DefaultValue, OptionConstants.Crosshair1DefaultValue,
        [
            new(OptionValueFunctions.ToNative(CrosshairVisibility.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(CrosshairVisibility.Blue), OptionValueResourceConstants.BlueLabelResource),
            new(OptionValueFunctions.ToNative(CrosshairVisibility.Green), OptionValueResourceConstants.GreenLabelResource),
            new(OptionValueFunctions.ToNative(CrosshairVisibility.Red), OptionValueResourceConstants.RedLabelResource),
            new(OptionValueFunctions.ToNative(CrosshairVisibility.White), OptionValueResourceConstants.WhiteLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Controllers, RequiresRestart: false),
        new(OptionConstants.Crosshair2Key, OptionConstants.Crosshair2LabelResource, OptionConstants.Crosshair2HelpResource, null,
            OptionConstants.Crosshair2DefaultValue, OptionConstants.Crosshair2DefaultValue,
        [
            new(OptionValueFunctions.ToNative(CrosshairVisibility.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(CrosshairVisibility.Blue), OptionValueResourceConstants.BlueLabelResource),
            new(OptionValueFunctions.ToNative(CrosshairVisibility.Green), OptionValueResourceConstants.GreenLabelResource),
            new(OptionValueFunctions.ToNative(CrosshairVisibility.Red), OptionValueResourceConstants.RedLabelResource),
            new(OptionValueFunctions.ToNative(CrosshairVisibility.White), OptionValueResourceConstants.WhiteLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Controllers, RequiresRestart: false),
        new(OptionConstants.KonamigunadjustxKey, OptionConstants.KonamigunadjustxLabelResource, OptionConstants.KonamigunadjustxHelpResource, null,
            OptionConstants.KonamigunadjustxDefaultValue, OptionConstants.KonamigunadjustxDefaultValue, [], Tab: EmulationMachineTab.Controllers, RequiresRestart: false,
            NumericRange: OptionConstants.KonamigunadjustxRange, NativeNumericFormat: OptionNumericConstants.IntegerFormat, NativeZeroValue: null),
        new(OptionConstants.KonamigunadjustyKey, OptionConstants.KonamigunadjustyLabelResource, OptionConstants.KonamigunadjustyHelpResource, null,
            OptionConstants.KonamigunadjustyDefaultValue, OptionConstants.KonamigunadjustyDefaultValue, [], Tab: EmulationMachineTab.Controllers, RequiresRestart: false,
            NumericRange: OptionConstants.KonamigunadjustyRange, NativeNumericFormat: OptionNumericConstants.IntegerFormat, NativeZeroValue: null),
        new(OptionConstants.GunconadjustxKey, OptionConstants.GunconadjustxLabelResource, OptionConstants.GunconadjustxHelpResource, null,
            OptionConstants.GunconadjustxDefaultValue, OptionConstants.GunconadjustxDefaultValue, [], Tab: EmulationMachineTab.Controllers, RequiresRestart: false,
            NumericRange: OptionConstants.GunconadjustxRange, NativeNumericFormat: OptionNumericConstants.IntegerFormat, NativeZeroValue: null),
        new(OptionConstants.GunconadjustyKey, OptionConstants.GunconadjustyLabelResource, OptionConstants.GunconadjustyHelpResource, null,
            OptionConstants.GunconadjustyDefaultValue, OptionConstants.GunconadjustyDefaultValue, [], Tab: EmulationMachineTab.Controllers, RequiresRestart: false,
            NumericRange: OptionConstants.GunconadjustyRange, NativeNumericFormat: OptionNumericConstants.IntegerFormat, NativeZeroValue: null),
        new(OptionConstants.GunconadjustratioxKey, OptionConstants.GunconadjustratioxLabelResource, OptionConstants.GunconadjustratioxHelpResource, null,
            OptionConstants.GunconadjustratioxDefaultValue, OptionConstants.GunconadjustratioxDefaultValue, [], Tab: EmulationMachineTab.Controllers, RequiresRestart: false,
            NumericRange: OptionConstants.GunconadjustratioxRange, NativeNumericFormat: OptionNumericConstants.TwoDecimalPlacesFormat, NativeZeroValue: null),
        new(OptionConstants.GunconadjustratioyKey, OptionConstants.GunconadjustratioyLabelResource, OptionConstants.GunconadjustratioyHelpResource, null,
            OptionConstants.GunconadjustratioyDefaultValue, OptionConstants.GunconadjustratioyDefaultValue, [], Tab: EmulationMachineTab.Controllers, RequiresRestart: false,
            NumericRange: OptionConstants.GunconadjustratioyRange, NativeNumericFormat: OptionNumericConstants.TwoDecimalPlacesFormat, NativeZeroValue: null),
        new(OptionConstants.IcacheEmulationKey, OptionConstants.IcacheEmulationLabelResource, OptionConstants.IcacheEmulationHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
        ], Tab: EmulationMachineTab.Cpu, RequiresRestart: false),
        new(OptionConstants.ExceptionEmulationKey, OptionConstants.ExceptionEmulationLabelResource, OptionConstants.ExceptionEmulationHelpResource, null,
            ToggleValueConstants.Disabled, ToggleValueConstants.Disabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Cpu, RequiresRestart: false),
        new(OptionConstants.CdTurboKey, OptionConstants.CdTurboLabelResource, OptionConstants.CdTurboHelpResource, null,
            ToggleValueConstants.Disabled, ToggleValueConstants.Disabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Storage, RequiresRestart: false),
        new(OptionConstants.NostallsKey, OptionConstants.NostallsLabelResource, OptionConstants.NostallsHelpResource, null,
            ToggleValueConstants.Disabled, ToggleValueConstants.Disabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Cpu, RequiresRestart: false),
    ];
}
