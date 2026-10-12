using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Contracts;
using System.IO;
using System.Buffers.Binary;
using System.Text.Json;

namespace GWGUI.Emulation.Fairchild.Emulators.Common.Functions;

internal static class StateFunctions
{
    internal static bool IsAvailable(ExternalCoreExports exports)
    {
        var size = exports.GetSerializedSize();
        return size > nuint.Zero && size <= SavedStateConstants.MaximumStateSize;
    }
}

