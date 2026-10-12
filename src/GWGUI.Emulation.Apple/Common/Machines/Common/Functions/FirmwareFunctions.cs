using System.IO;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static partial class FirmwareFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        if (new Engine().Adapter(configuration) is IEmulatorFirmwareAdapter adapter) adapter.PrepareFirmware(configuration, systemDirectory);
        else Directory.CreateDirectory(systemDirectory);
    }
}
