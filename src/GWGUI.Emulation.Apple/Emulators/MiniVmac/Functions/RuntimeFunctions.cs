namespace GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions;

internal static class RuntimeFunctions
{
    internal static bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => true;
    internal static bool RequiresOptionRestart(string key) => false;
}
