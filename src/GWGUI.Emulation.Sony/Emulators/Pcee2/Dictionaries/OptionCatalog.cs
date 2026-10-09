using AnisotropicFiltering = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.AnisotropicFiltering;
using AspectRatio = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.AspectRatio;
using AudioBufferMs = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.AudioBufferMs;
using AxisDeadzone = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.AxisDeadzone;
using AxisScale = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.AxisScale;
using BlendingAccuracy = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.BlendingAccuracy;
using CasMode = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.CasMode;
using FrameLimiter = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.FrameLimiter;
using HwDownloadMode = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.HwDownloadMode;
using Lightgun = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.Lightgun;
using Multitap = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.Multitap;
using Renderer = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.Renderer;
using TextureFiltering = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.TextureFiltering;
using TrilinearFiltering = GWGUI.Emulation.Sony.Emulators.Pcee2.Enums.TrilinearFiltering;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Dictionaries;

internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new(OptionConstants.FastBootKey, OptionConstants.FastBootLabelResource, OptionConstants.FastBootHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
        ], Tab: EmulationMachineTab.General, RequiresRestart: true),
        new(OptionConstants.RendererKey, OptionConstants.RendererLabelResource, OptionConstants.RendererHelpResource, null,
            OptionConstants.RendererDefaultValue, OptionConstants.RendererDefaultValue,
        [
            new(OptionValueFunctions.ToNative(Renderer.Opengl), OptionValueResourceConstants.OpenGLHardwareLabelResource),
            new(OptionValueFunctions.ToNative(Renderer.Software), OptionValueResourceConstants.SoftwareLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: true),
        new(OptionConstants.UpscaleMultiplierKey, OptionConstants.UpscaleMultiplierLabelResource, OptionConstants.UpscaleMultiplierHelpResource, null,
            OptionConstants.UpscaleMultiplierDefaultValue, OptionConstants.UpscaleMultiplierDefaultValue, NumericOptionValueFunctions.Choices(OptionConstants.UpscaleMultiplierRanges, OptionNumericConstants.IntegerFormat),
            Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.HwDownloadModeKey, OptionConstants.HwDownloadModeLabelResource, OptionConstants.HwDownloadModeHelpResource, null,
            OptionConstants.HwDownloadModeDefaultValue, OptionConstants.HwDownloadModeDefaultValue,
        [
            new(OptionValueFunctions.ToNative(HwDownloadMode.Accurate), OptionValueResourceConstants.AccurateDefaultLabelResource),
            new(OptionValueFunctions.ToNative(HwDownloadMode.Unsynchronized), OptionValueResourceConstants.UnsynchronizedFastLabelResource),
            new(OptionValueFunctions.ToNative(HwDownloadMode.Disabled), OptionValueResourceConstants.DisabledFastestLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.BlendingAccuracyKey, OptionConstants.BlendingAccuracyLabelResource, OptionConstants.BlendingAccuracyHelpResource, null,
            OptionConstants.BlendingAccuracyDefaultValue, OptionConstants.BlendingAccuracyDefaultValue,
        [
            new(OptionValueFunctions.ToNative(BlendingAccuracy.Minimum), OptionValueResourceConstants.MinimumLabelResource),
            new(OptionValueFunctions.ToNative(BlendingAccuracy.Basic), OptionValueResourceConstants.BasicRecommendedLabelResource),
            new(OptionValueFunctions.ToNative(BlendingAccuracy.Medium), OptionValueResourceConstants.MediumLabelResource),
            new(OptionValueFunctions.ToNative(BlendingAccuracy.High), OptionValueResourceConstants.HighLabelResource),
            new(OptionValueFunctions.ToNative(BlendingAccuracy.Full), OptionValueResourceConstants.FullSlowLabelResource),
            new(OptionValueFunctions.ToNative(BlendingAccuracy.Maximum), OptionValueResourceConstants.MaximumVerySlowLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.TextureFilteringKey, OptionConstants.TextureFilteringLabelResource, OptionConstants.TextureFilteringHelpResource, null,
            OptionConstants.TextureFilteringDefaultValue, OptionConstants.TextureFilteringDefaultValue,
        [
            new(OptionValueFunctions.ToNative(TextureFiltering.Nearest), OptionValueResourceConstants.NearestLabelResource),
            new(OptionValueFunctions.ToNative(TextureFiltering.BilinearPs2), OptionValueResourceConstants.BilinearPS2LabelResource),
            new(OptionValueFunctions.ToNative(TextureFiltering.BilinearForced), OptionValueResourceConstants.BilinearForcedLabelResource),
            new(OptionValueFunctions.ToNative(TextureFiltering.BilinearForcedSprite), OptionValueResourceConstants.BilinearForcedExcludingSpritesLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.TrilinearFilteringKey, OptionConstants.TrilinearFilteringLabelResource, null, null,
            OptionConstants.TrilinearFilteringDefaultValue, OptionConstants.TrilinearFilteringDefaultValue,
        [
            new(OptionValueFunctions.ToNative(TrilinearFiltering.Auto), OptionValueResourceConstants.AutomaticDefaultLabelResource),
            new(OptionValueFunctions.ToNative(TrilinearFiltering.Off), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(TrilinearFiltering.Ps2), OptionValueResourceConstants.TrilinearPS2LabelResource),
            new(OptionValueFunctions.ToNative(TrilinearFiltering.Forced), OptionValueResourceConstants.TrilinearForcedLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.AnisotropicFilteringKey, OptionConstants.AnisotropicFilteringLabelResource, OptionConstants.AnisotropicFilteringHelpResource, null,
            OptionConstants.AnisotropicFilteringDefaultValue, OptionConstants.AnisotropicFilteringDefaultValue,
        [
            new(OptionValueFunctions.ToNative(AnisotropicFiltering._0), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(AnisotropicFiltering._2), OptionConstants.AnisotropicFiltering2DisplayName),
            new(OptionValueFunctions.ToNative(AnisotropicFiltering._4), OptionConstants.AnisotropicFiltering4DisplayName),
            new(OptionValueFunctions.ToNative(AnisotropicFiltering._8), OptionConstants.AnisotropicFiltering8DisplayName),
            new(OptionValueFunctions.ToNative(AnisotropicFiltering._16), OptionConstants.AnisotropicFiltering16DisplayName),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.DitheringKey, OptionConstants.DitheringLabelResource, OptionConstants.DitheringHelpResource, null,
            OptionConstants.DitheringDefaultValue, OptionConstants.DitheringDefaultValue, NumericOptionValueFunctions.Choices(OptionConstants.DitheringRanges, OptionNumericConstants.IntegerFormat),
            Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.MipmappingKey, OptionConstants.MipmappingLabelResource, null, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
        ], Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.DeinterlaceModeKey, OptionConstants.DeinterlaceModeLabelResource, OptionConstants.DeinterlaceModeHelpResource, null,
            OptionConstants.DeinterlaceModeDefaultValue, OptionConstants.DeinterlaceModeDefaultValue, [], Tab: EmulationMachineTab.Video, RequiresRestart: false,
            NumericRange: OptionConstants.DeinterlaceModeRange, NativeNumericFormat: OptionNumericConstants.IntegerFormat, NativeZeroValue: null),
        new(OptionConstants.FxaaKey, OptionConstants.FxaaLabelResource, OptionConstants.FxaaHelpResource, null,
            ToggleValueConstants.Disabled, ToggleValueConstants.Disabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.CasModeKey, OptionConstants.CasModeLabelResource, null, null,
            OptionConstants.CasModeDefaultValue, OptionConstants.CasModeDefaultValue,
        [
            new(OptionValueFunctions.ToNative(CasMode.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(CasMode.Sharpen), OptionValueResourceConstants.SharpenOnlyLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.AudioBufferMsKey, OptionConstants.AudioBufferMsLabelResource, OptionConstants.AudioBufferMsHelpResource, null,
            OptionConstants.AudioBufferMsDefaultValue, OptionConstants.AudioBufferMsDefaultValue,
        [
            new(OptionValueFunctions.ToNative(AudioBufferMs._50), OptionConstants.AudioBufferMs50DisplayName),
            new(OptionValueFunctions.ToNative(AudioBufferMs._75), OptionConstants.AudioBufferMs75DisplayName),
            new(OptionValueFunctions.ToNative(AudioBufferMs._100), OptionConstants.AudioBufferMs100DisplayName),
            new(OptionValueFunctions.ToNative(AudioBufferMs._150), OptionConstants.AudioBufferMs150DisplayName),
            new(OptionValueFunctions.ToNative(AudioBufferMs._200), OptionConstants.AudioBufferMs200DisplayName),
        ], IsVisible: true, Tab: EmulationMachineTab.Audio, RequiresRestart: false),
        new(OptionConstants.FrameLimiterKey, OptionConstants.FrameLimiterLabelResource, OptionConstants.FrameLimiterHelpResource, null,
            OptionConstants.FrameLimiterDefaultValue, OptionConstants.FrameLimiterDefaultValue,
        [
            new(OptionValueFunctions.ToNative(FrameLimiter.Frontend), OptionValueResourceConstants.FrontendRetroArchLabelResource),
            new(OptionValueFunctions.ToNative(FrameLimiter.Internal), OptionValueResourceConstants.InternalPCSX2LabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.SkipDuplicateFramesKey, OptionConstants.SkipDuplicateFramesLabelResource, OptionConstants.SkipDuplicateFramesHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
        ], Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.CasSharpnessKey, OptionConstants.CasSharpnessLabelResource, null, null,
            OptionConstants.CasSharpnessDefaultValue, OptionConstants.CasSharpnessDefaultValue, [], Tab: EmulationMachineTab.Video, RequiresRestart: false,
            NumericRange: OptionConstants.CasSharpnessRange, NativeNumericFormat: OptionNumericConstants.IntegerFormat, NativeZeroValue: null),
        new(OptionConstants.AspectRatioKey, OptionConstants.AspectRatioLabelResource, OptionConstants.AspectRatioHelpResource, null,
            OptionConstants.AspectRatioDefaultValue, OptionConstants.AspectRatioDefaultValue,
        [
            new(OptionValueFunctions.ToNative(AspectRatio.Auto), OptionValueResourceConstants.AutomaticLabelResource),
            new(OptionValueFunctions.ToNative(AspectRatio._43), OptionValueFunctions.ToNative(AspectRatio._43)),
            new(OptionValueFunctions.ToNative(AspectRatio._169), OptionValueFunctions.ToNative(AspectRatio._169)),
        ], IsVisible: true, Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.MultitapKey, OptionConstants.MultitapLabelResource, OptionConstants.MultitapHelpResource, null,
            OptionConstants.MultitapDefaultValue, OptionConstants.MultitapDefaultValue,
        [
            new(OptionValueFunctions.ToNative(Multitap.Disabled), OptionValueResourceConstants.Disabled2PlayersLabelResource),
            new(OptionValueFunctions.ToNative(Multitap.Port1), OptionValueResourceConstants.Port15PlayersLabelResource),
            new(OptionValueFunctions.ToNative(Multitap.Port2), OptionValueResourceConstants.Port25PlayersLabelResource),
            new(OptionValueFunctions.ToNative(Multitap.Both), OptionValueResourceConstants.BothPorts8PlayersLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Controllers, RequiresRestart: true),
        new(OptionConstants.LightgunKey, OptionConstants.LightgunLabelResource, OptionConstants.LightgunHelpResource, null,
            OptionConstants.LightgunDefaultValue, OptionConstants.LightgunDefaultValue,
        [
            new(OptionValueFunctions.ToNative(Lightgun.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(OptionValueFunctions.ToNative(Lightgun.Usb1), OptionValueResourceConstants.USBPort1LabelResource),
            new(OptionValueFunctions.ToNative(Lightgun.Usb2), OptionValueResourceConstants.USBPort2LabelResource),
            new(OptionValueFunctions.ToNative(Lightgun.Both), OptionValueResourceConstants.BothPortsLabelResource),
        ], IsVisible: true, Tab: EmulationMachineTab.Controllers, RequiresRestart: true),
        new(OptionConstants.RumbleKey, OptionConstants.RumbleLabelResource, OptionConstants.RumbleHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
        ], Tab: EmulationMachineTab.Controllers, RequiresRestart: false),
        new(OptionConstants.AxisScaleKey, OptionConstants.AxisScaleLabelResource, OptionConstants.AxisScaleHelpResource, null,
            OptionConstants.AxisScaleDefaultValue, OptionConstants.AxisScaleDefaultValue,
        [
            new(OptionValueFunctions.ToNative(AxisScale._100), OptionConstants.AxisScale100DisplayName),
            new(OptionValueFunctions.ToNative(AxisScale._115), OptionConstants.AxisScale115DisplayName),
            new(OptionValueFunctions.ToNative(AxisScale._133), OptionValueResourceConstants._133PercentDefaultLabelResource),
            new(OptionValueFunctions.ToNative(AxisScale._150), OptionConstants.AxisScale150DisplayName),
        ], IsVisible: true, Tab: EmulationMachineTab.Controllers, RequiresRestart: false),
        new(OptionConstants.AxisDeadzoneKey, OptionConstants.AxisDeadzoneLabelResource, OptionConstants.AxisDeadzoneHelpResource, null,
            OptionConstants.AxisDeadzoneDefaultValue, OptionConstants.AxisDeadzoneDefaultValue,
        [
            new(OptionValueFunctions.ToNative(AxisDeadzone._0), OptionValueResourceConstants._0PercentDefaultLabelResource),
            new(OptionValueFunctions.ToNative(AxisDeadzone._5), OptionConstants.AxisDeadzone5DisplayName),
            new(OptionValueFunctions.ToNative(AxisDeadzone._10), OptionConstants.AxisDeadzone10DisplayName),
            new(OptionValueFunctions.ToNative(AxisDeadzone._15), OptionConstants.AxisDeadzone15DisplayName),
            new(OptionValueFunctions.ToNative(AxisDeadzone._20), OptionConstants.AxisDeadzone20DisplayName),
            new(OptionValueFunctions.ToNative(AxisDeadzone._30), OptionConstants.AxisDeadzone30DisplayName),
        ], IsVisible: true, Tab: EmulationMachineTab.Controllers, RequiresRestart: false),
        new(OptionConstants.WidescreenPatchesKey, OptionConstants.WidescreenPatchesLabelResource, OptionConstants.WidescreenPatchesHelpResource, null,
            ToggleValueConstants.Disabled, ToggleValueConstants.Disabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.NoInterlacingPatchesKey, OptionConstants.NoInterlacingPatchesLabelResource, OptionConstants.NoInterlacingPatchesHelpResource, null,
            ToggleValueConstants.Disabled, ToggleValueConstants.Disabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
        ], Tab: EmulationMachineTab.Video, RequiresRestart: false),
        new(OptionConstants.MtvuKey, OptionConstants.MtvuLabelResource, OptionConstants.MtvuHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
        ], Tab: EmulationMachineTab.Cpu, RequiresRestart: false),
        new(OptionConstants.InstantVu1Key, OptionConstants.InstantVu1LabelResource, OptionConstants.InstantVu1HelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
        ], Tab: EmulationMachineTab.Cpu, RequiresRestart: false),
        new(OptionConstants.EeCycleRateKey, OptionConstants.EeCycleRateLabelResource, OptionConstants.EeCycleRateHelpResource, null,
            OptionConstants.EeCycleRateDefaultValue, OptionConstants.EeCycleRateDefaultValue, NumericOptionValueFunctions.Choices(OptionConstants.EeCycleRateRanges, OptionNumericConstants.IntegerFormat),
            Tab: EmulationMachineTab.Cpu, RequiresRestart: false),
        new(OptionConstants.EeCycleSkipKey, OptionConstants.EeCycleSkipLabelResource, OptionConstants.EeCycleSkipHelpResource, null,
            OptionConstants.EeCycleSkipDefaultValue, OptionConstants.EeCycleSkipDefaultValue, NumericOptionValueFunctions.Choices(OptionConstants.EeCycleSkipRanges, OptionNumericConstants.IntegerFormat),
            Tab: EmulationMachineTab.Cpu, RequiresRestart: false),
        new(OptionConstants.CpuRecompilerKey, OptionConstants.CpuRecompilerLabelResource, OptionConstants.CpuRecompilerHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledJITDefaultLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledInterpreterLabelResource),
        ], Tab: EmulationMachineTab.Cpu, RequiresRestart: true),
        new(OptionConstants.RecEeKey, OptionConstants.RecEeLabelResource, OptionConstants.RecEeHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledDefaultLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledInterpreterLabelResource),
        ], Tab: EmulationMachineTab.Cpu, RequiresRestart: true),
        new(OptionConstants.RecIopKey, OptionConstants.RecIopLabelResource, OptionConstants.RecIopHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledDefaultLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledInterpreterLabelResource),
        ], Tab: EmulationMachineTab.Cpu, RequiresRestart: true),
        new(OptionConstants.RecVu0Key, OptionConstants.RecVu0LabelResource, OptionConstants.RecVu0HelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledDefaultLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledInterpreterLabelResource),
        ], Tab: EmulationMachineTab.Cpu, RequiresRestart: true),
        new(OptionConstants.RecVu1Key, OptionConstants.RecVu1LabelResource, OptionConstants.RecVu1HelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledDefaultLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledInterpreterLabelResource),
        ], Tab: EmulationMachineTab.Cpu, RequiresRestart: true),
        new(OptionConstants.MemcardSlot1EnableKey, OptionConstants.MemcardSlot1EnableLabelResource, OptionConstants.MemcardSlot1EnableHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
        ], Tab: EmulationMachineTab.Storage, RequiresRestart: false),
        new(OptionConstants.MemcardSlot2EnableKey, OptionConstants.MemcardSlot2EnableLabelResource, OptionConstants.MemcardSlot2EnableHelpResource, null,
            ToggleValueConstants.Enabled, ToggleValueConstants.Enabled,
        [
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Enabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.EnabledLabelResource),
            new(ToggleValueFunctions.ToNative(EmulationToggleState.Disabled, ToggleValueConstants.Enabled, ToggleValueConstants.Disabled), OptionValueResourceConstants.DisabledLabelResource),
        ], Tab: EmulationMachineTab.Storage, RequiresRestart: false),
    ];
}
