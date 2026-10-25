using GWGUI.Emulation.Snk.Emulators.Geolith.Constants;
using GWGUI.Emulation.Snk.Emulators.Geolith.Dictionaries;

namespace GWGUI.Emulation.Snk.Emulators.Geolith.Functions;
internal static class RuntimeOptionFunctions
{
    private static readonly IReadOnlySet<string> CartridgeOptions = new HashSet<string>(StringComparer.Ordinal)
    {
        RuntimeOptionConstants.SystemType,
        RuntimeOptionConstants.UnibiosHw,
        RuntimeOptionConstants.Memcard,
        RuntimeOptionConstants.MemcardWp,
        RuntimeOptionConstants.Settingmode,
        RuntimeOptionConstants.FourPlayer,
        RuntimeOptionConstants.Freeplay
    };
    private static readonly IReadOnlySet<string> CdOptions = new HashSet<string>(StringComparer.Ordinal)
    {
        RuntimeOptionConstants.CdSystemType,
        RuntimeOptionConstants.CdSpeedHack,
        RuntimeOptionConstants.CdDmaLenLimit,
        RuntimeOptionConstants.CdSkipLoading
    };
    internal static IReadOnlyDictionary<string, string> Defaults(MachineModel model)
    {
        if (!EmulatorConstants.Entry.Models.Contains(model))
            throw new ArgumentOutOfRangeException(nameof(model));
        var result = OptionCatalog.Options.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
        result[RuntimeOptionConstants.SystemType] = model == MachineModel.NeoGeoMvs ? RuntimeOptionConstants.Mvs : RuntimeOptionConstants.Aes;
        result[RuntimeOptionConstants.UnibiosHw] = result[RuntimeOptionConstants.SystemType];
        result[RuntimeOptionConstants.CdSystemType] = model == MachineModel.NeoGeoCd ? RuntimeOptionConstants.CdTop : RuntimeOptionConstants.Cdz;
        return result;
    }

    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        var result = new Dictionary<string, string>(Defaults(configuration.Model), StringComparer.Ordinal);
        foreach (var option in OptionCatalog.Options)
            if (configuration.Options.TryGetValue(option.Key, out var value))
                result[option.Key] = value;
        result[RuntimeOptionConstants.UnibiosHw] = configuration.Model == MachineModel.NeoGeoMvs ? RuntimeOptionConstants.Mvs : RuntimeOptionConstants.Aes;
        var memoryCard = configuration.Media.SingleOrDefault(media => media.Slot == EmulationMediaSlot.MemoryCard0);
        if (memoryCard is not null)
        {
            result[RuntimeOptionConstants.Memcard] = memoryCard.IsInserted ? RuntimeOptionConstants.On : RuntimeOptionConstants.Off;
            result[RuntimeOptionConstants.MemcardWp] = memoryCard.IsReadOnly ? RuntimeOptionConstants.On : RuntimeOptionConstants.Off;
        }

        if (!AllowsFourPlayers(configuration.Model, result))
            result[RuntimeOptionConstants.FourPlayer] = RuntimeOptionConstants.Off;
        return result;
    }

    internal static bool AllowsFourPlayers(MachineModel model,
        IReadOnlyDictionary<string, string> options) => model == MachineModel.NeoGeoMvs && options[RuntimeOptionConstants.SystemType] == RuntimeOptionConstants.Mvs && options[RuntimeOptionConstants.Region] is RuntimeOptionConstants.Japan or RuntimeOptionConstants.Asia;
    internal static int ControllerPorts(MachineConfiguration configuration)
    {
        var options = Prepare(configuration);
        return AllowsFourPlayers(configuration.Model, options) && options[RuntimeOptionConstants.FourPlayer] == RuntimeOptionConstants.On ? RuntimeOptionConstants.ExtendedControllerPorts : RuntimeOptionConstants.StandardControllerPorts;
    }

    internal static IReadOnlyList<CoreOption> Describe(MachineConfiguration configuration)
    {
        var defaults = Defaults(configuration.Model);
        var options = Prepare(configuration);
        var cd = ConfigurationFunctions.Family(configuration.Model) == MachineFamily.NeoGeoCd;
        return OptionCatalog.Options.Select(option =>
        {
            var values = option.Values;
            if (option.Key == RuntimeOptionConstants.SystemType)
                values = option.Values.Where(value => value.Value == defaults[option.Key] || value.Value == RuntimeOptionConstants.UniverseBios).ToArray();
            if (option.Key == RuntimeOptionConstants.CdSystemType)
                values = option.Values.Where(value => configuration.Model == MachineModel.NeoGeoCd ? value.Value is RuntimeOptionConstants.CdFront or RuntimeOptionConstants.CdTop : value.Value is RuntimeOptionConstants.Cdz or RuntimeOptionConstants.CdzUniverseBios).ToArray();
            var visible = option.IsVisible && (cd ? !CartridgeOptions.Contains(option.Key) : !CdOptions.Contains(option.Key));
            if (option.Key is RuntimeOptionConstants.Memcard or RuntimeOptionConstants.MemcardWp && configuration.Media.Any(media => media.Slot == EmulationMediaSlot.MemoryCard0))
                visible = false;
            var enabled = option.Key != RuntimeOptionConstants.FourPlayer || AllowsFourPlayers(configuration.Model, options);
            if (option.Key is RuntimeOptionConstants.Settingmode or RuntimeOptionConstants.Freeplay)
                enabled = !cd && configuration.Model == MachineModel.NeoGeoMvs;
            if (option.Key == RuntimeOptionConstants.CdSpeedHack)
                enabled = cd && options[RuntimeOptionConstants.CdSystemType] != RuntimeOptionConstants.CdzUniverseBios;
            return option with
            {
                DefaultValue = defaults[option.Key],
                CurrentValue = options[option.Key],
                Values = values,
                IsVisible = visible,
                IsEnabled = enabled
            };
        }).ToArray();
    }
}