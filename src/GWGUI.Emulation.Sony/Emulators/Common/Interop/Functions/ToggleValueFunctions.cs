namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class ToggleValueFunctions
{
    internal static string ToNative(EmulationToggleState state, string enabled, string disabled) => state switch
    {
        EmulationToggleState.Enabled => enabled,
        EmulationToggleState.Disabled => disabled,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
    };
}
