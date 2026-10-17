namespace GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Functions;

internal static class CoreDiagnosticFunctions
{
    internal static string CalculateSha256(string path) => EmulationCoreDiagnosticFunctions.CalculateSha256(path);
    internal static string ReadArchitecture(string path) => EmulationCoreDiagnosticFunctions.ReadArchitecture(path);
    internal static string ReadDeclaredVersion(string path) => EmulationCoreDiagnosticFunctions.ReadDeclaredVersion(path);
    internal static IReadOnlyList<string> ReadExports(string path) => EmulationCoreDiagnosticFunctions.ReadExports(path);
}
