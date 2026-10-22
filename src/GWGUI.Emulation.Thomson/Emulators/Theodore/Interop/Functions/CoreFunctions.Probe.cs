using CommonCoreHostFunctions = GWGUI.Emulation.Thomson.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Constants;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Contracts;

namespace GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Functions;

internal static class CoreProbeFunctions
{
    private const uint ExpectedApiVersion = 1;
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate uint GetApiVersion();

    internal static ExternalCoreInfo Inspect(string absolutePath)
    {
        using var library = CommonCoreHostFunctions.NativeResources.CreateLibrary(Path.GetFullPath(absolutePath));
        var version = library.Resolve<GetApiVersion>(ExternalCoreExportNames.ApiVersion)();
        if (version != ExpectedApiVersion) throw Rejected();
        var exports = CoreFunctions.ResolveExports(library);
        exports.GetSystemInfo(out var info);
        var name = Marshal.PtrToStringUTF8(info.LibraryName) ?? string.Empty;
        if (!string.Equals(name, EmulatorConstants.LibraryName, StringComparison.OrdinalIgnoreCase)) throw Rejected();
        var extensions = CommonCoreHostFunctions.NativeResources.ParseExtensions(info.ValidExtensions);
        return new ExternalCoreInfo(Emulator.Theodore, name, Marshal.PtrToStringUTF8(info.LibraryVersion) ?? string.Empty, extensions, info.NeedFullPath, info.BlockExtract);
    }

    private static EmulationMessageException Rejected() => CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.Emulator, EmulationMessageCode.EmulatorRejected, null);
}
