using GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Constants;

namespace GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Functions;

internal static class PaletteFunctions
{
    internal static string Default(MachineModel model) => model switch
    {
        MachineModel.WonderSwan => "wonderswan",
        MachineModel.WonderSwanColor => "wondeswan_color",
        MachineModel.SwanCrystal => "swancrystal",
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    internal static IReadOnlyDictionary<string, string> RuntimeOptions(MachineConfiguration configuration)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        options.TryAdd(OptionConstants.OptionMonoPalette, Default(configuration.Model));
        return options;
    }
}
