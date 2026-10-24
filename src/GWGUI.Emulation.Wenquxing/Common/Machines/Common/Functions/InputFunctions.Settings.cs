using CommonControllerFunctions = GWGUI.Emulation.Wenquxing.Common.Functions.ControllerFunctions;
namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;

internal static class InputSettingsFunctions
{
    internal static IReadOnlyDictionary<string, string> KeyboardBindings(InputConfiguration input, MachineModel model) => KeyboardDictionary.Bindings(input, model);
    internal static EmulationInputSettings Describe(MachineConfiguration configuration) => new(new((new Engine().SelectedAdapter(configuration)?.GetKeyboardDefinitions(configuration) ?? KeyboardDictionary.Definitions(configuration.Model)), KeyboardBindings(configuration.Input, configuration.Model), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller), null, []);
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        if (settings.Keyboard is null || settings.Mouse is not null || settings.ControllerPorts.Count != CommonControllerFunctions.PortCount(configuration)) throw new ArgumentException(nameof(settings));
        var input = new InputConfiguration(new Dictionary<string, string>(settings.Keyboard.Values, StringComparer.Ordinal));
        var result = configuration with { Input = input with { KeyboardBindings = KeyboardDictionary.Bindings(input, configuration.Model) } };
        ConfigurationFunctions.Validate(result);
        return new Engine().SelectedAdapter(result)?.ApplyInputConfiguration(result) ?? result;
    }

    /// <summary>Décrit les touches du clavier principal selon les règles matérielles déjà utilisées.</summary>
    /// <param name="configuration">Profil et associations empruntés à consulter.</param>
    /// <returns>Définitions de clavier du profil ; liste vide sans clavier pris en charge.</returns>
    internal static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions(MachineConfiguration configuration) { return KeyboardDictionary.Definitions(configuration.Model); }
}
