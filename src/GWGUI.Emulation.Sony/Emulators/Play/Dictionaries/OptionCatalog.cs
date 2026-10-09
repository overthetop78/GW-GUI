using GWGUI.Emulation.Sony.Emulators.Play.Constants;
using GWGUI.Emulation.Sony.Emulators.Play.Enums;
using GWGUI.Emulation.Sony.Emulators.Play.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Play.Dictionaries;

internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new(OptionConstants.ResolutionKey, OptionConstants.ResolutionLabelResource,
            OptionConstants.ResolutionHelpResource, null,
            OptionValueFunctions.ToNative(ResolutionScale.Native),
            OptionValueFunctions.ToNative(ResolutionScale.Native),
            Enum.GetValues<ResolutionScale>().Select(scale =>
                new CoreOptionValue(OptionValueFunctions.ToNative(scale), OptionValueFunctions.ToNative(scale))).ToArray(),
            Tab: EmulationMachineTab.Video),
        new(OptionConstants.PresentationKey, OptionConstants.PresentationLabelResource,
            OptionConstants.PresentationHelpResource, null,
            OptionValueFunctions.ToNative(PresentationMode.FitScreen),
            OptionValueFunctions.ToNative(PresentationMode.FitScreen),
            [
                new(OptionValueFunctions.ToNative(PresentationMode.FitScreen), OptionConstants.FitScreenLabelResource),
                new(OptionValueFunctions.ToNative(PresentationMode.FillScreen), OptionConstants.FillScreenLabelResource),
                new(OptionValueFunctions.ToNative(PresentationMode.OriginalSize), OptionConstants.OriginalSizeLabelResource)
            ], Tab: EmulationMachineTab.Video),
        new(OptionConstants.BilinearKey, OptionConstants.BilinearLabelResource,
            OptionConstants.BilinearHelpResource, null,
            OptionValueFunctions.ToNative(EmulationToggleState.Disabled),
            OptionValueFunctions.ToNative(EmulationToggleState.Disabled),
            [
                new(OptionValueFunctions.ToNative(EmulationToggleState.Disabled), OptionValueResourceConstants.DisabledLabelResource),
                new(OptionValueFunctions.ToNative(EmulationToggleState.Enabled), OptionValueResourceConstants.EnabledLabelResource)
            ], Tab: EmulationMachineTab.Video)
    ];
}
