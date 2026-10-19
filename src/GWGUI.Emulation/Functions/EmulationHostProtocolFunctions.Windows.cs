using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace GWGUI.Emulation.Functions;

internal static partial class EmulationHostProtocolFunctions
{
    private const string KernelLibrary = "kernel32.dll";
    private const uint SuppressCriticalErrorDialogs = 0x0001;
    private const uint SuppressFaultDialogs = 0x0002;

    [SupportedOSPlatform("windows")]
    internal static void WithSuppressedErrorDialogs(Action run)
    {
        var previousErrorMode = GetErrorMode();
        SetErrorMode(previousErrorMode | SuppressCriticalErrorDialogs | SuppressFaultDialogs);
        try { run(); }
        finally { SetErrorMode(previousErrorMode); }
    }

    [DllImport(KernelLibrary)]
    private static extern uint GetErrorMode();

    [DllImport(KernelLibrary)]
    private static extern uint SetErrorMode(uint mode);
}
