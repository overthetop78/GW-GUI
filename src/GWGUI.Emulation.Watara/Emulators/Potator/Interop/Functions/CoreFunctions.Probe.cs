using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Watara.Emulators.Potator.Constants;
using GWGUI.Emulation.Watara.Emulators.Potator.Interop.Contracts;

namespace GWGUI.Emulation.Watara.Emulators.Potator.Interop.Functions;

internal static class CoreProbeFunctions
{
    private const uint ExpectedApiVersion = 1;
    private const char ExtensionSeparator = '|';
    private const char ExtensionPrefix = '.';
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
        var extensions = (Marshal.PtrToStringUTF8(info.ValidExtensions) ?? string.Empty).Split(ExtensionSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(extension => extension.TrimStart(ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return new ExternalCoreInfo(Emulator.Potator, name, Marshal.PtrToStringUTF8(info.LibraryVersion) ?? string.Empty, extensions, info.NeedFullPath, info.BlockExtract);
    }

    private static EmulationMessageException Rejected() => new(new EmulationMessage(EmulationMessageCategory.Emulator, EmulationMessageCode.EmulatorRejected, EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog));
}
