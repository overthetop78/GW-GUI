using System.Globalization;
using GWGUI.Emulation.Sony.Emulators.Play.Constants;
using GWGUI.Emulation.Sony.Emulators.Play.Enums;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

namespace GWGUI.Emulation.Sony.Emulators.Play.Functions;

internal static class OptionValueFunctions
{
    internal static string ToNative(EmulationToggleState state) =>
        ToggleValueFunctions.ToNative(state, ToggleValueConstants.True, ToggleValueConstants.False);

    internal static string ToNative(ResolutionScale scale)
    {
        if (!Enum.IsDefined(scale)) throw new ArgumentOutOfRangeException(nameof(scale), scale, null);
        return ((int)scale).ToString(OptionNumericConstants.IntegerFormat, CultureInfo.InvariantCulture)
            + OptionNumericConstants.MultiplierSuffix;
    }

    internal static string ToNative(PresentationMode mode) => mode switch
    {
        PresentationMode.FitScreen => OptionConstants.FitScreenValue,
        PresentationMode.FillScreen => OptionConstants.FillScreenValue,
        PresentationMode.OriginalSize => OptionConstants.OriginalSizeValue,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };
}
