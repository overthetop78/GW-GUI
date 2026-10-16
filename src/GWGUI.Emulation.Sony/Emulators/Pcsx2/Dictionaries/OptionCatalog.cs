using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Enums;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Dictionaries;

internal static partial class OptionCatalog
{
    private static readonly Lazy<IReadOnlyList<CoreOption>> Catalogue = new(CreateCatalogue);
    internal static IReadOnlyList<CoreOption> All => Catalogue.Value;

    private static IReadOnlyList<CoreOption> CreateCatalogue() =>
        [.. SystemOptions, .. VideoOptions, .. CpuOptions, .. HardwareFixesOptions, .. ControllersOptions];

    private static IReadOnlyList<CoreOptionValue> ToggleChoices =>
        [new(ToggleValueConstants.Enabled, OptionValueResourceConstants.EnabledLabelResource),
            new(ToggleValueConstants.Disabled, OptionValueResourceConstants.DisabledLabelResource)];

    private static CoreOption Define(string key, string label, string? help, string defaultValue,
        EmulationMachineTab tab, IReadOnlyList<CoreOptionValue> values, bool restart = false, bool visible = true,
        EmulationSettingsNumericRange? range = null, string? nativeZero = null,
        IReadOnlyDictionary<EmulationGraphicsApi, string>? graphicsApis = null, IReadOnlyList<string>? rendererModes = null) =>
        new(key, label, help, null, defaultValue, defaultValue, values, IsVisible: visible, Tab: tab,
            RequiresRestart: restart, NumericRange: range, NativeNumericFormat: range is null ? null : "0", NativeZeroValue: nativeZero,
            GraphicsApiValues: graphicsApis, RetainedRendererValues: rendererModes);

}
