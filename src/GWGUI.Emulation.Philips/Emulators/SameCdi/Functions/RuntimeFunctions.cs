using GWGUI.Emulation.Philips.Emulators.SameCdi.Constants;
using GWGUI.Emulation.Philips.Emulators.SameCdi.Dictionaries;

namespace GWGUI.Emulation.Philips.Emulators.SameCdi.Functions;

internal static class SameCdiRuntimeFunctions
{
    internal static IReadOnlyDictionary<string, string> Options(MachineConfiguration configuration)
    {
        var options = SameCdiOptionCatalog.All.Where(option => configuration.Options.ContainsKey(option.Key))
            .ToDictionary(option => option.Key, option => configuration.Options[option.Key], StringComparer.Ordinal);
        var peripheral = configuration.Input.Controllers?.FirstOrDefault(binding => binding.Port == ControllerConstants.FirstPort)
            ?.Peripheral ?? PeripheralCategory.CdiGamepad;
        options[OptionConstants.MouseEnable] = peripheral == PeripheralCategory.Mouse
            ? OptionConstants.Enabled : OptionConstants.Disabled;
        options[OptionConstants.LightgunMode] = peripheral == PeripheralCategory.LightGun
            ? OptionConstants.Lightgun : OptionConstants.None;
        return options;
    }
}
