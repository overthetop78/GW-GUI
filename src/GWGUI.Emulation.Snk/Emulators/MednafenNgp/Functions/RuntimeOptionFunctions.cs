using GWGUI.Emulation.Snk.Emulators.MednafenNgp.Constants;
using GWGUI.Emulation.Snk.Emulators.MednafenNgp.Dictionaries;

namespace GWGUI.Emulation.Snk.Emulators.MednafenNgp.Functions;

internal static class RuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration) => OptionCatalog.Options.ToDictionary(option => option.Key, option => configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue), StringComparer.Ordinal);
    internal static IReadOnlyList<CoreOption> Describe(MachineConfiguration configuration) => OptionCatalog.Options.Select(option => option with { CurrentValue = configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue) }).ToArray();
}
