using GWGUI.Emulation.Constants;

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
        var keys = new HashSet<EmulationKey>(processing.MapKeyboard(frozen, configuration.KeyboardBindings ?? configuration.KeyboardMappings?.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal)));
        var controllers = frozen.Controllers.ToArray();
        var pointer = frozen.Pointer;
        foreach (var binding in configuration.Controllers ?? [])
        {
            if (binding.Port < 0 || binding.Port >= ControllerPortConstants.MaximumControllerPortCount) continue;
            if (controllers.Length <= binding.Port) Array.Resize(ref controllers, binding.Port + 1);
            var source = processing.ResolveController(binding.DeviceId, frozen.Controllers, binding.Port);
            if (binding.Peripheral == PeripheralCategory.None) { controllers[binding.Port] = EmulationControllerState.Empty; continue; }
            if (binding.Peripheral == PeripheralCategory.Keyboard)
            {
                // Un clavier de port produit des touches, comme le clavier principal. Chaque expression
                // peut choisir sa source explicitement ; sinon le périphérique sélectionné fournit la source.
                // Les touches s'ajoutent aux entrées principales sans transmettre les boutons bruts du port.
                if (binding.Mappings is { } keyboardMappings)
                {
                    foreach (var mapping in keyboardMappings)
                    {
                        if (Enum.TryParse<EmulationKey>(mapping.Key, out var key) && key != EmulationKey.Unknown && Enum.IsDefined(key)
                            && processing.InputSourcePressure(mapping.Value, source, frozen) > .5f) keys.Add(key);
                    }
                }
                controllers[binding.Port] = EmulationControllerState.Empty;
                continue;
            }
            if (binding.Peripheral == PeripheralCategory.Mouse)
            {
                // Le protocole lit les commandes de souris dans Pointer, et non dans le masque de manette.
                // Les associations du port utilisent le même traitement que celles de la souris principale.
                if (binding.Mappings is { } mouseMappings) pointer = ApplyMouseMappings(pointer, mouseMappings, source, frozen);
                controllers[binding.Port] = EmulationControllerState.Empty;
                continue;
            }
            if (binding.Mappings is not { Count: > 0 }) { controllers[binding.Port] = processing.ApplyDeadZones(source, binding.DeadZonePercent); continue; }
            // Le dictionnaire de commandes transporte aussi les boutons et axes qui dépassent le masque natif.
            // Un port configuré explicitement ne reçoit pas en plus les boutons bruts de la source physique.
            var values = binding.Mappings.ToDictionary(item => item.Key, item => processing.MappedInputMagnitude(item.Value,
                processing.ResolveSourceController(item.Value, frozen.Controllers, source), frozen, binding.DeadZonePercent), StringComparer.Ordinal);
            // Les commandes restent disponibles par leur identité générique, et les sticks et gâchettes
            // alimentent aussi les champs lus par le protocole natif. Les amplitudes ont déjà reçu leur
            // zone morte : la conversion commune les représente sur seize bits sans la réappliquer.
            controllers[binding.Port] = EmulationControllerState.Empty with
            {
                DeviceId = source.DeviceId,
                Controls = new EmulationControllerControls(values),
                LeftX = processing.NormalizedAxis(values.GetValueOrDefault(EmulationControllerCommandIds.LeftStickRight) - values.GetValueOrDefault(EmulationControllerCommandIds.LeftStickLeft)),
                LeftY = processing.NormalizedAxis(values.GetValueOrDefault(EmulationControllerCommandIds.LeftStickDown) - values.GetValueOrDefault(EmulationControllerCommandIds.LeftStickUp)),
                RightX = processing.NormalizedAxis(values.GetValueOrDefault(EmulationControllerCommandIds.RightStickRight) - values.GetValueOrDefault(EmulationControllerCommandIds.RightStickLeft)),
                RightY = processing.NormalizedAxis(values.GetValueOrDefault(EmulationControllerCommandIds.RightStickDown) - values.GetValueOrDefault(EmulationControllerCommandIds.RightStickUp)),
                LeftTrigger = processing.NormalizedAxis(values.GetValueOrDefault(EmulationControllerCommandIds.L2)),
                RightTrigger = processing.NormalizedAxis(values.GetValueOrDefault(EmulationControllerCommandIds.R2))
            };
        }
        for (var index = 0; index < controllers.Length; index++) controllers[index] ??= EmulationControllerState.Empty;
        if (configuration.MouseMappings is { } mouse)
        {
            var source = frozen.Controllers.FirstOrDefault() ?? EmulationControllerState.Empty;
            pointer = ApplyMouseMappings(pointer, mouse, source, frozen);
        }
        return frozen with { Keys = keys, Controllers = controllers, Pointer = pointer };
    }

    /// <summary>Applique les associations de boutons et molettes sans remplacer les déplacements physiques.</summary>
    /// <param name="pointer">État courant du pointeur ; les commandes non associées conservent leurs valeurs.</param>
    /// <param name="mappings">Expressions des cinq boutons et des directions des deux molettes.</param>
    /// <param name="source">Contrôleur de repli choisi pour ce port ou pour la souris principale.</param>
    /// <param name="snapshot">Sources physiques figées, utilisées pour évaluer les expressions communes.</param>
    /// <returns>Nouvel état de pointeur ; les sources et associations reçues restent inchangées.</returns>
    /// <remarks>Une molette associée produit une impulsion par frame. Les molettes sans association
    /// conservent leur amplitude physique. Les associations principales sont appliquées après celles des ports.</remarks>
    private static EmulationPointerState ApplyMouseMappings(EmulationPointerState pointer, IReadOnlyDictionary<string, string> mappings, EmulationControllerState source, EmulationInputSnapshot snapshot)
    {
        var processing = EmulationSdk.InputProcessing;
        bool Pressed(string name, bool fallback) => mappings.TryGetValue(name, out var expression) ? processing.InputSourcePressure(expression, source, snapshot) > .5f : fallback;
        var result = pointer with { Left = Pressed("Left", pointer.Left), Right = Pressed("Right", pointer.Right), Middle = Pressed("Middle", pointer.Middle), ExtendedButton1 = Pressed("XButton1", pointer.ExtendedButton1), ExtendedButton2 = Pressed("XButton2", pointer.ExtendedButton2) };
        if (mappings.ContainsKey("WheelUp") || mappings.ContainsKey("WheelDown")) result = result with { Wheel = (Pressed("WheelUp", pointer.Wheel > 0) ? 1 : 0) - (Pressed("WheelDown", pointer.Wheel < 0) ? 1 : 0) };
        if (mappings.ContainsKey("WheelRight") || mappings.ContainsKey("WheelLeft")) result = result with { HorizontalWheel = (Pressed("WheelRight", pointer.HorizontalWheel > 0) ? 1 : 0) - (Pressed("WheelLeft", pointer.HorizontalWheel < 0) ? 1 : 0) };
        return result;
    }
}
