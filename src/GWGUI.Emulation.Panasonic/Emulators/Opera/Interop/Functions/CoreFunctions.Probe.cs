using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Constants;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Contracts;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Functions;

internal static class CoreProbeFunctions
{
    private const uint ExpectedApiVersion = 1;
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate uint GetApiVersion();

    internal static ExternalCoreInfo Inspect(string absolutePath)
    {
        using var library = new ExternalCoreLibrary(Path.GetFullPath(absolutePath));
        var version = library.Resolve<GetApiVersion>(ExternalCoreExportNames.ApiVersion)();
        if (version != ExpectedApiVersion) throw Rejected();
        var exports = CoreFunctions.ResolveExports(library);
        exports.GetSystemInfo(out var info);
        var name = Marshal.PtrToStringUTF8(info.LibraryName) ?? string.Empty;
        if (!string.Equals(name, EmulatorConstants.LibraryName, StringComparison.OrdinalIgnoreCase)) throw Rejected();
        var extensions = ExternalCoreLibrary.ParseExtensions(info.ValidExtensions);
        return new ExternalCoreInfo(Emulator.Opera, name, Marshal.PtrToStringUTF8(info.LibraryVersion) ?? string.Empty, extensions, info.NeedFullPath, info.BlockExtract);
    }

    private static EmulationMessageException Rejected() => new(new EmulationMessage(EmulationMessageCategory.Emulator, EmulationMessageCode.EmulatorRejected, EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog));
}
