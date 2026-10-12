namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;

internal static class InputSettingsFunctions
{
    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) => KeyboardDictionary.Bindings(input, model);
    internal static EmulationInputSettings Describe(MachineConfiguration configuration) => new(new(KeyboardDictionary.Definitions(configuration.Model), KeyboardBindings(configuration.Input, configuration.Model), EmulationInputSource.Keyboard), null, []);
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        if (settings.Keyboard is null || settings.Mouse is not null || settings.ControllerPorts.Count != 0) throw new ArgumentException(nameof(settings));
        var input = new InputConfiguration(new Dictionary<string, string>(settings.Keyboard.Values, StringComparer.Ordinal));
        var result = configuration with { Input = input with { Keyboard = KeyboardDictionary.Bindings(input, configuration.Model) } };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}