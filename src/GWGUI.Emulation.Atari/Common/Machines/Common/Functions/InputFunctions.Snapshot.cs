using CommonControllerFunctions = GWGUI.Emulation.Atari.Common.Functions.ControllerFunctions;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    /// <summary>Projette les entrées de session par le contrat Common et applique la bascule du pointeur.</summary>
    /// <param name="snapshot">Instantané physique emprunté, dont les collections restent inchangées.</param>
    /// <param name="configuration">Configuration complète et courante : modèle, cœur, options et associations.</param>
    /// <param name="mapper">Adaptateur emprunté chargé de traduire les commandes ; null utilise la projection de repli du module.</param>
    /// <param name="controllerPointerSwitchPressed">Impulsion de session ajoutant Contrôle droit aux touches projetées.</param>
    /// <returns>Instantané adapté au cœur ; sans impulsion, le résultat du projecteur est conservé tel quel.</returns>
    /// <remarks>La bascule est appliquée après la projection, comme pour les cœurs qui la prennent déjà en charge.
    /// Elle copie l'ensemble des touches avant ajout, conserve les autres entrées et ne modifie ni la source
    /// ni la configuration. Cette opération ne crée aucun cœur et ne possède aucune ressource native.</remarks>
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, MachineConfiguration configuration, IEmulatorInputMapper? mapper = null, bool controllerPointerSwitchPressed = false)
    {
        var mapped = mapper?.MapInput(snapshot, configuration) ?? InputSnapshotFunctions.Apply(snapshot, configuration.Input, configuration.Model);
        return controllerPointerSwitchPressed ? mapped with { Keys = new HashSet<EmulationKey>(mapped.Keys) { EmulationKey.RightControl } } : mapped;
    }


    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration? configuration, MachineModel model)
    {
        var mappedKeyboard = snapshot with
        {
            Keys = EmulationSdk.InputProcessing.MapKeyboard(snapshot, configuration?.KeyboardBindings ?? configuration?.KeyboardMappings?.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal))
        };
        var withDeadZones = CommonControllerFunctions.ApplyDeadZones(mappedKeyboard, configuration?.Controllers);
        if (configuration?.Controllers is not { Count: > 0 }) return withDeadZones with { Controllers = [] };
        var count = Math.Max(withDeadZones.Controllers.Count, configuration.Controllers.Max(binding => binding.Port) + 1);
        var controllers = Enumerable.Repeat(EmulationControllerState.Empty, count).ToArray();
        foreach (var binding in configuration.Controllers)
        {
            if (binding.Port < 0 || binding.Port >= controllers.Length) continue;
            var source = EmulationSdk.InputProcessing.ResolveController(binding.DeviceId, withDeadZones.Controllers, binding.Port);
            if (binding.Mappings is not { Count: > 0 })
            {
                controllers[binding.Port] = EmulationControllerState.Empty;
                continue;
            }
            uint buttons = 0;
            foreach (var mapping in binding.Mappings)
            {
                var target = ResolveButton(model, mapping.Key);
                var mappingController = ControllerForSource(mapping.Value, withDeadZones.Controllers, source);
                if (target >= 0 && IsPressed(mapping.Value, mappingController, withDeadZones.Keys, withDeadZones.Pointer))
                    buttons |= 1u << target;
            }
            var analog = binding.Peripheral is PeripheralCategory.AnalogJoystick or PeripheralCategory.Paddle;
            var mouse = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                [InputSnapshotFunctionsConstants.Left] = withDeadZones.Pointer.Left, [InputSnapshotFunctionsConstants.Right] = withDeadZones.Pointer.Right, [InputSnapshotFunctionsConstants.Middle] = withDeadZones.Pointer.Middle,
                [InputSnapshotFunctionsConstants.XButton1] = withDeadZones.Pointer.ExtendedButton1, [InputSnapshotFunctionsConstants.XButton2] = withDeadZones.Pointer.ExtendedButton2,
                [InputSnapshotFunctionsConstants.WheelUp] = withDeadZones.Pointer.Wheel > 0, [InputSnapshotFunctionsConstants.WheelDown] = withDeadZones.Pointer.Wheel < 0
            };
            controllers[binding.Port] = source with
            {
                Buttons = buttons,
                LeftX = analog ? EmulationSdk.InputProcessing.MapControllerAxis(binding.Mappings, EmulationControllerCommandIds.Right, EmulationControllerCommandIds.Left, source, withDeadZones.Controllers, withDeadZones.Keys, mouse) : source.LeftX,
                LeftY = binding.Peripheral == PeripheralCategory.AnalogJoystick ? EmulationSdk.InputProcessing.MapControllerAxis(binding.Mappings, EmulationControllerCommandIds.Down, EmulationControllerCommandIds.Up, source, withDeadZones.Controllers, withDeadZones.Keys, mouse) : source.LeftY
            };
        }
        return withDeadZones with { Controllers = controllers };
    }

    private static bool IsPressed(string sourceName, EmulationControllerState controller, IReadOnlySet<EmulationKey> keys, EmulationPointerState pointer)
    {
        if (sourceName.Contains('+' ) && !sourceName.EndsWith('+')) return EmulationSdk.InputProcessing.InputSourcePressure(sourceName, controller, new EmulationInputSnapshot(keys, pointer, [controller])) > .5f;
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Keyboard, out var keyboardSource)
            && Enum.TryParse<EmulationKey>(keyboardSource, true, out var key))
            return keys.Contains(key);
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Mouse, out var mouseSource))
            return PhysicalMouse(pointer).GetValueOrDefault(mouseSource);
        return EmulationSdk.InputProcessing.IsControllerSourcePressed(sourceName, controller);
    }

    private static IReadOnlyDictionary<string, bool> PhysicalMouse(EmulationPointerState pointer) => EmulationSdk.InputProcessing.PhysicalMouse(pointer);

    private static bool TryRemovePrefix(string? value, string prefix, out string source)
    => EmulationSdk.InputProcessing.TryRemovePrefix(value, prefix, out source);

    private static EmulationControllerState ControllerForSource(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback)
    => EmulationSdk.InputProcessing.ResolveSourceController(source, controllers, fallback, fallbackWhenMissing: true);

    private static int ResolveButton(MachineModel model, string action)
    {
        if (model == MachineModel.Lynx)
            return action switch
            {
                InputSnapshotFunctionsConstants.A => LynxInputConstants.ButtonACommand,
                InputSnapshotFunctionsConstants.B => LynxInputConstants.ButtonBCommand,
                InputSnapshotFunctionsConstants.Option1 => LynxInputConstants.Option1Command,
                InputSnapshotFunctionsConstants.Option2 => LynxInputConstants.Option2Command,
                InputSnapshotFunctionsConstants.Pause => LynxInputConstants.PauseCommand,
                _ => InputSnapshotDictionary.CommonButtons.GetValueOrDefault(action, -1)
            };
        if (model == MachineModel.Atari5200)
            return action switch
            {
                InputSnapshotFunctionsConstants.Pause => 2, InputSnapshotFunctionsConstants.Start => 3, InputSnapshotFunctionsConstants.Key0 => 10, InputSnapshotFunctionsConstants.Key1 => 11, InputSnapshotFunctionsConstants.Key2 => 12,
                InputSnapshotFunctionsConstants.Key3 => 13, InputSnapshotFunctionsConstants.Key7 => 14, InputSnapshotFunctionsConstants.Star => 9, InputSnapshotFunctionsConstants.Hash => 1,
                _ => InputSnapshotDictionary.CommonButtons.GetValueOrDefault(action, -1)
            };
        return InputSnapshotDictionary.CommonButtons.GetValueOrDefault(action, -1);
    }
}
