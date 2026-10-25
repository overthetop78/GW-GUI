using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class CoreIdentityFunctions
{
    internal static bool Accepts(CoreDefinition definition, string? libraryName) => !string.IsNullOrWhiteSpace(libraryName)
        && (string.Equals(libraryName, definition.LibraryName, StringComparison.OrdinalIgnoreCase)
            || definition.PreviousLibraryNames?.Contains(libraryName, StringComparer.OrdinalIgnoreCase) == true);
}
