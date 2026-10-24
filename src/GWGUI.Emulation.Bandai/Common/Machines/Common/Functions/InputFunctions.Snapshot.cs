using GWGUI.Emulation;
namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;

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
        if (configuration?.Controllers is not { Count: > 0 }) return EmulationSdk.InputProcessing.Freeze(snapshot with { Controllers = [] });
        var controllers = Enumerable.Repeat(EmulationControllerState.Empty, ControllerConstants.PortCount).ToArray();
        var targets = InputSnapshotDictionary.ForModel(model);
        foreach (var binding in configuration.Controllers)
        {
            if (binding.Port != ControllerConstants.FirstPort || binding.Peripheral == PeripheralCategory.None) continue;
            var source = EmulationSdk.InputProcessing.ResolveController(binding.DeviceId, snapshot.Controllers, binding.Port);
            if (binding.Mappings is not { Count: > 0 })
            {
                controllers[binding.Port] = EmulationControllerState.Empty;
                continue;
            }
            uint buttons = 0;
            foreach (var mapping in binding.Mappings)
            {
                if (!targets.TryGetValue(mapping.Key, out var target)) continue;
                var controller = EmulationSdk.InputProcessing.ResolveSourceController(mapping.Value, snapshot.Controllers, source);
                if (IsPressed(mapping.Value, controller, snapshot)) buttons |= 1u << target;
            }
            controllers[binding.Port] = source with { Buttons = buttons };
        }
        return snapshot with { Controllers = controllers };
    }

    private static bool IsPressed(string source, EmulationControllerState controller, EmulationInputSnapshot snapshot)
    {
        if (source.Contains('+' ) && !source.EndsWith('+')) return EmulationSdk.InputProcessing.InputSourcePressure(source, controller, snapshot) > .5f;
        if (source.StartsWith(InputSnapshotFunctionsConstants.Keyboard, StringComparison.OrdinalIgnoreCase))
            return Enum.TryParse<EmulationKey>(source[InputSnapshotFunctionsConstants.Keyboard.Length..], true, out var key) && snapshot.Keys.Contains(key);
        if (source.StartsWith(InputSnapshotFunctionsConstants.Mouse, StringComparison.OrdinalIgnoreCase))
            return source[InputSnapshotFunctionsConstants.Mouse.Length..] switch
            {
                EmulationControllerCommandIds.Left => snapshot.Pointer.Left,
                EmulationControllerCommandIds.Right => snapshot.Pointer.Right,
                InputSnapshotFunctionsConstants.Middle => snapshot.Pointer.Middle,
                InputSnapshotFunctionsConstants.XButton1 => snapshot.Pointer.ExtendedButton1,
                InputSnapshotFunctionsConstants.XButton2 => snapshot.Pointer.ExtendedButton2,
                InputSnapshotFunctionsConstants.WheelUp => snapshot.Pointer.Wheel > 0,
                InputSnapshotFunctionsConstants.WheelDown => snapshot.Pointer.Wheel < 0,
                InputSnapshotFunctionsConstants.WheelLeft => snapshot.Pointer.HorizontalWheel < 0,
                InputSnapshotFunctionsConstants.WheelRight => snapshot.Pointer.HorizontalWheel > 0,
                _ => false
            };
        return EmulationSdk.InputProcessing.IsControllerSourcePressed(source, controller);
    }
}
