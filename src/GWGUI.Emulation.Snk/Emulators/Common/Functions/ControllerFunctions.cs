namespace GWGUI.Emulation.Snk.Emulators.Common.Functions;

internal static class ControllerFunctions
{
    // Dead zones are applied to physical sources by the mapper before it encodes native axes and carrier bits.
    internal static EmulationInputSnapshot FreezeMappedInput(EmulationInputSnapshot input) => InputFunctions.Freeze(input);
}
