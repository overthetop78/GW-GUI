using GWGUI.Emulation.Sony.Emulators.Common.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Functions;

internal static class GraphicsSettingsFunctions
{
    internal static MachineConfiguration Configure(MachineConfiguration configuration, IReadOnlyList<CoreOption> options)
    {
        if (configuration.GraphicsApi is not { } api) return configuration;
        var values = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var option in options.Where(option => option.GraphicsApiValues is not null))
        {
            var current = values.GetValueOrDefault(option.Key) ?? option.DefaultValue;
            if (option.RetainedRendererValues?.Contains(current) == true) continue;
            if (option.GraphicsApiValues!.TryGetValue(api, out var selected)
                || option.GraphicsApiValues.TryGetValue(EmulationGraphicsApi.Automatic, out selected))
                values[option.Key] = selected;
        }
        return configuration with { Options = values };
    }

    internal static CoreOption Present(CoreOption option, string selected, EmulationGraphicsApi? api)
    {
        if (option.GraphicsApiValues is not { } mapping) return option;
        var retained = option.RetainedRendererValues ?? [];
        var hardware = selected;
        if (retained.Contains(selected) || !option.Values.Any(value => value.Value == selected))
        {
            if (!mapping.TryGetValue(api ?? EmulationGraphicsApi.Automatic, out hardware))
                hardware = mapping[EmulationGraphicsApi.Automatic];
        }
        return option with
        {
            Name = GraphicsSettingsConstants.ModeLabel,
            Description = GraphicsSettingsConstants.ModeHelp,
            CategorizedDescription = GraphicsSettingsConstants.ModeHelp,
            CurrentValue = selected,
            Values = [new(hardware, GraphicsSettingsConstants.HardwareRenderingLabel), .. option.Values.Where(value => retained.Contains(value.Value))]
        };
    }
}
