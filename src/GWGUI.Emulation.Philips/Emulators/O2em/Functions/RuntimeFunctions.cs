using GWGUI.Emulation.Philips.Emulators.O2em.Constants;
using GWGUI.Emulation.Philips.Emulators.O2em.Dictionaries;

namespace GWGUI.Emulation.Philips.Emulators.O2em.Functions;

internal static class O2emRuntimeFunctions
{
    internal static IReadOnlyDictionary<string, string> Options(MachineConfiguration configuration)
    {
        var options = O2emOptionCatalog.All.Where(option => configuration.Options.ContainsKey(option.Key))
            .ToDictionary(option => option.Key, option => configuration.Options[option.Key], StringComparer.Ordinal);
        options[OptionConstants.Bios] = configuration.Model switch
        {
            MachineModel.Odyssey2 => OptionConstants.O2romBin,
            MachineModel.VideopacG7000 => OptionConstants.C52Bin,
            _ => throw new ArgumentOutOfRangeException(nameof(configuration))
        };
        return options;
    }
}
