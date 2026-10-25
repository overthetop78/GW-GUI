
namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static partial class FirmwareFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        new Engine().Adapter(configuration).PrepareFirmware(configuration, systemDirectory);
    }
}
