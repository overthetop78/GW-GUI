using GWGUI.Emulation.Sinclair.Emulators.Common.Dictionaries;
namespace GWGUI.Emulation.Sinclair.Emulators.Common.Functions;

internal static class NativeRuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration) => CoreCatalog.CreateAdapter(configuration.Core).GetRuntimeOptions(configuration);
}
