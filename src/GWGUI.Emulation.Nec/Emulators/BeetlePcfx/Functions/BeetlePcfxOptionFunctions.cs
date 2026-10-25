namespace GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Functions;

internal static class BeetlePcfxOptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    => GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions.CoreSettingsFunctions.CopyConfiguredOptions(configuration);
}


