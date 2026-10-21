namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Projection des sources physiques vers les commandes génériques consommées par les cœurs fictifs.</summary>
/// <remarks>Les expressions de sources sont évaluées par le SDK ; le module conserve l’association des ports.
/// Les commandes étendues restent dans Controls afin de représenter les catégories sans multiplier les profils de manettes.</remarks>
internal static class InputSnapshotFunctions
{
    /// <summary>Projette les associations de la configuration et transmet l’impulsion de bascule du pointeur.</summary>
    /// <param name="snapshot">Entrées physiques stables de la frame.</param>
    /// <param name="configuration">Configuration du modèle et du périphérique choisi sur chaque port.</param>
    /// <param name="mapper">Adaptateur qui traduit les commandes communes vers celles de son cœur.</param>
    /// <param name="controllerPointerSwitchPressed">Impulsion de la commande native de bascule du pointeur.</param>
    /// <returns>Entrées du cœur ; l’impulsion utilise Contrôle droit comme les adaptations existantes.</returns>
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, MachineConfiguration configuration, IEmulatorInputMapper? mapper, bool controllerPointerSwitchPressed)
    {
        var mapped = mapper?.MapInput(snapshot, configuration) ?? Apply(snapshot, configuration.Input);
        return controllerPointerSwitchPressed ? mapped with { Keys = new HashSet<EmulationKey>(mapped.Keys) { EmulationKey.RightControl } } : mapped;
    }

    /// <summary>Applique les associations génériques sans dépendre du modèle d’un contrôleur physique.</summary>
    /// <param name="snapshot">Sources physiques empruntées ; leurs collections sont copiées avant publication.</param>
    /// <param name="configuration">Associations des ports, du clavier et de la souris.</param>
    /// <returns>Instantané stable dont les commandes supplémentaires restent transportables dans Controls.</returns>
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration configuration)
    {
        var processing = EmulationSdk.InputProcessing;
        var frozen = processing.Freeze(snapshot);
        var keys = processing.MapKeyboard(frozen, configuration.KeyboardBindings ?? configuration.KeyboardMappings?.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal));
        var controllers = frozen.Controllers.ToArray();
        foreach (var binding in configuration.Controllers ?? [])
        {
            if (binding.Port < 0 || binding.Port >= ControllerPortConstants.MaximumControllerPortCount) continue;
            if (controllers.Length <= binding.Port) Array.Resize(ref controllers, binding.Port + 1);
            var source = processing.ResolveController(binding.DeviceId, frozen.Controllers, binding.Port);
            if (binding.Peripheral == PeripheralCategory.None) { controllers[binding.Port] = EmulationControllerState.Empty; continue; }
            if (binding.Mappings is not { Count: > 0 }) { controllers[binding.Port] = processing.ApplyDeadZones(source, binding.DeadZonePercent); continue; }
            // Le dictionnaire de commandes transporte aussi les boutons et axes qui dépassent le masque natif.
            // Un port configuré explicitement ne reçoit pas en plus les boutons bruts de la source physique.
            var values = binding.Mappings.ToDictionary(item => item.Key, item => processing.MappedInputMagnitude(item.Value,
                processing.ResolveSourceController(item.Value, frozen.Controllers, source), frozen, binding.DeadZonePercent), StringComparer.Ordinal);
            controllers[binding.Port] = EmulationControllerState.Empty with { DeviceId = source.DeviceId, Controls = new EmulationControllerControls(values) };
        }
        for (var index = 0; index < controllers.Length; index++) controllers[index] ??= EmulationControllerState.Empty;
        var pointer = frozen.Pointer;
        if (configuration.MouseMappings is { } mouse)
        {
            var source = frozen.Controllers.FirstOrDefault() ?? EmulationControllerState.Empty;
            bool Pressed(string name, bool fallback) => mouse.TryGetValue(name, out var expression) ? processing.InputSourcePressure(expression, source, frozen) > .5f : fallback;
            pointer = pointer with { Left = Pressed("Left", pointer.Left), Right = Pressed("Right", pointer.Right), Middle = Pressed("Middle", pointer.Middle), ExtendedButton1 = Pressed("XButton1", pointer.ExtendedButton1), ExtendedButton2 = Pressed("XButton2", pointer.ExtendedButton2) };
            // Les molettes non associées conservent leur amplitude physique ; les associations produisent une impulsion par frame.
            if (mouse.ContainsKey("WheelUp") || mouse.ContainsKey("WheelDown")) pointer = pointer with { Wheel = (Pressed("WheelUp", frozen.Pointer.Wheel > 0) ? 1 : 0) - (Pressed("WheelDown", frozen.Pointer.Wheel < 0) ? 1 : 0) };
            if (mouse.ContainsKey("WheelRight") || mouse.ContainsKey("WheelLeft")) pointer = pointer with { HorizontalWheel = (Pressed("WheelRight", frozen.Pointer.HorizontalWheel > 0) ? 1 : 0) - (Pressed("WheelLeft", frozen.Pointer.HorizontalWheel < 0) ? 1 : 0) };
        }
        return frozen with { Keys = keys, Controllers = controllers, Pointer = pointer };
    }
}
