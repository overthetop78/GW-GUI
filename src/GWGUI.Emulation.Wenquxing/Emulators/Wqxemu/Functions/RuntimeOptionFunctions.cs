using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Constants;
using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Dictionaries;

namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Functions;

internal static class RuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        ConfigurationFunctions.Validate(configuration);
        return new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal) { [RuntimeOptionConstants.Model] = ModelDictionary.NativeValues[configuration.Model].ToUpperInvariant() };
    }
}