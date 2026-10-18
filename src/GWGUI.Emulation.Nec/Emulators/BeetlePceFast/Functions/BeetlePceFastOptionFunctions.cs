namespace GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Functions;

internal static class BeetlePceFastOptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    => GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreSettingsFunctions.CopyConfiguredOptions(configuration);
}


