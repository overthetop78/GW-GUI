using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Constants;

internal static class CoreHostConstants
{
    internal const string Windows = "windows";
    internal const string LocalPipeServer = ".";
    internal const string KernelLibrary = "kernel32.dll";
    internal const int ConnectionTimeoutMilliseconds = 15_000;
    internal const uint SuppressCriticalErrorDialogs = 0x0001;
    internal const uint SuppressFaultDialogs = 0x0002;
}



