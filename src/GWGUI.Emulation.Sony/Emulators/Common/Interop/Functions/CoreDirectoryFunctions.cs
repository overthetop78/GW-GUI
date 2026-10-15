using System.IO;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class CoreDirectoryFunctions
{
    internal static (string SystemDirectory, string ContentDirectory) NativeDataDirectories(CoreDefinition definition, string sessionDirectory, string saveDirectory)
    {
        var root = definition.PersistentDataDirectories ? Path.Combine(saveDirectory, definition.Id) : sessionDirectory;
        return (Path.Combine(root, CoreDirectoryConstants.SystemDirectoryName), Path.Combine(root, CoreDirectoryConstants.ContentDirectoryName));
    }
}
