namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Projection des entrées physiques vers les touches et contrôleurs configurés de la machine.</summary>
/// <remarks>L’instantané reçu reste emprunté. Les collections de touches et de contrôleurs produites décrivent
/// les entrées émulées ; le pointeur est conservé dans la copie de l’instantané.</remarks>
internal static class InputSnapshotFunctions
{
    /// <summary>Applique les associations de touches, boutons et directions à un instantané physique.</summary>
    /// <param name="snapshot">Valeurs physiques stables pendant la projection.</param>
    /// <param name="configuration">Associations facultatives ; les touches initiales du modèle restent la base de la projection.</param>
    /// <returns>Instantané projeté ; les contrôleurs sont absents lorsqu’aucune association de port n’est configurée.</returns>
    /// <remarks>Les associations explicites de touches remplacent les correspondances initiales.
    /// Les ports désactivés, hors limites ou sans commandes restent vides. Pour chaque port associé, le masque
    /// de boutons et les axes gauches sont reconstruits ; les autres champs du contrôleur source sont conservés.
    /// La zone morte n’est pas appliquée ici : elle intervient après la projection des ports dans le parcours natif.</remarks>
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration? configuration)
    {
        var keyboardMappings = ExampleMachineAKeyboardDictionary.DefaultHostKeys.ToDictionary(item => item.Key.ToString(), item => item.Value.ToString(), StringComparer.Ordinal);
        var bindings = configuration?.KeyboardBindings ?? configuration?.KeyboardMappings?.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal);
        if (bindings is not null)
            foreach (var mapping in bindings)
                keyboardMappings[mapping.Key] = mapping.Value;
        var mapped = snapshot with
        {
            Keys = EmulationInputMappingFunctions.MapKeyboard(snapshot, keyboardMappings)
        };
        if (configuration?.Controllers is not { Count: > 0 }) return mapped with { Controllers = [] };
        var controllers = Enumerable.Repeat(EmulationControllerState.Empty, ControllerPortConstants.MaximumControllerPortCount).ToArray();
        foreach (var binding in configuration.Controllers)
        {
            if (binding.Port < ControllerPortConstants.MinimumControllerPort
                || binding.Port >= controllers.Length || binding.Peripheral == PeripheralCategory.None) continue;
            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId, snapshot.Controllers, binding.Port);
            if (binding.Mappings is not { Count: > 0 })
            {
                controllers[binding.Port] = EmulationControllerState.Empty;
                continue;
            }
            uint buttons = 0;
            var horizontalMapped = false;
            var verticalMapped = false;
            var horizontal = 0f;
            var vertical = 0f;
            foreach (var mapping in binding.Mappings)
            {
                var controller = EmulationInputMappingFunctions.ResolveSourceController(mapping.Value, snapshot.Controllers, source);
                var target = InputSnapshotDictionary.CommonButtons.GetValueOrDefault(mapping.Key, -1);
                if (target < 0) continue;
                if (IsPressed(mapping.Value, controller, snapshot)) buttons |= 1u << target;
                var magnitude = Magnitude(mapping.Value, controller, snapshot);
                if (string.Equals(mapping.Key, InputSnapshotFunctionsConstants.Left, StringComparison.OrdinalIgnoreCase))
                {
                    horizontalMapped = true;
                    horizontal -= magnitude;
                }
                else if (string.Equals(mapping.Key, InputSnapshotFunctionsConstants.Right, StringComparison.OrdinalIgnoreCase))
                {
                    horizontalMapped = true;
                    horizontal += magnitude;
                }
                else if (string.Equals(mapping.Key, InputSnapshotFunctionsConstants.Up, StringComparison.OrdinalIgnoreCase))
                {
                    verticalMapped = true;
                    vertical -= magnitude;
                }
                else if (string.Equals(mapping.Key, InputSnapshotFunctionsConstants.Down, StringComparison.OrdinalIgnoreCase))
                {
                    verticalMapped = true;
                    vertical += magnitude;
                }
            }
            controllers[binding.Port] = source with
            {
                Buttons = buttons,
                LeftX = horizontalMapped ? Axis(horizontal) : (short)0,
                LeftY = verticalMapped ? Axis(vertical) : (short)0
            };
        }
        // Les zones mortes sont appliquées une seule fois par l’hôte natif, après projection des ports et périphériques.
        return mapped with { Controllers = controllers };
    }

    /// <summary>Convertit une amplitude normalisée vers un axe signé sur seize bits.</summary>
    /// <param name="value">Amplitude obtenue en combinant les sens opposés d’un axe.</param>
    /// <returns>Valeur bornée entre −32 767 et 32 767 après arrondi.</returns>
    private static short Axis(float value) => (short)Math.Round(Math.Clamp(value, -1f, 1f) * ControllerConstants.MaximumAxisMagnitude);

    /// <summary>Obtient l’amplitude d’une source de commande physique.</summary>
    /// <param name="source">Expression de source : combinaison, touche, souris ou commande de contrôleur.</param>
    /// <param name="controller">Contrôleur source résolu pour cette association.</param>
    /// <param name="snapshot">Instantané physique emprunté pour les touches, le pointeur et les combinaisons.</param>
    /// <returns>Pression fournie par le traitement commun ; zéro ou un pour une source clavier ou souris simple.</returns>
    /// <remarks>Un signe plus final peut désigner le sens d’un axe ; il n’est pas traité comme une combinaison de sources.</remarks>
    private static float Magnitude(string source, EmulationControllerState controller, EmulationInputSnapshot snapshot)
    {
        if (source.Contains('+' ) && !source.EndsWith('+')) return EmulationInputMappingFunctions.InputSourcePressure(source, controller, snapshot);
        if (source.StartsWith(InputSnapshotFunctionsConstants.Keyboard, StringComparison.OrdinalIgnoreCase)
            || source.StartsWith(InputSnapshotFunctionsConstants.Mouse, StringComparison.OrdinalIgnoreCase))
            return IsPressed(source, controller, snapshot) ? 1f : 0f;
        return EmulationInputMappingFunctions.ControllerSourcePressure(source, controller);
    }

    /// <summary>Détermine l’état appuyé d’une expression de commande physique.</summary>
    /// <param name="source">Expression à évaluer, avec ses éventuels préfixes clavier ou souris.</param>
    /// <param name="controller">Contrôleur physique utilisé pour les expressions de manette.</param>
    /// <param name="snapshot">Touches et pointeur physiques, empruntés pendant l’évaluation.</param>
    /// <returns>État du bouton ou de la touche ; les combinaisons exigent une pression strictement supérieure à 0,5.</returns>
    /// <remarks>Les mouvements de molette sont reconnus par le signe de leurs déplacements.
    /// Une commande de souris inconnue retourne false.</remarks>
    private static bool IsPressed(string source, EmulationControllerState controller, EmulationInputSnapshot snapshot)
    {
        if (source.Contains('+' ) && !source.EndsWith('+')) return EmulationInputMappingFunctions.InputSourcePressure(source, controller, snapshot) > .5f;
        if (TryRemovePrefix(source, InputSnapshotFunctionsConstants.Keyboard, out var keyboard)
            && Enum.TryParse<EmulationKey>(keyboard, true, out var key)) return snapshot.Keys.Contains(key);
        if (TryRemovePrefix(source, InputSnapshotFunctionsConstants.Mouse, out var mouse))
            return mouse switch
            {
                InputSnapshotFunctionsConstants.Left => snapshot.Pointer.Left,
                InputSnapshotFunctionsConstants.Right => snapshot.Pointer.Right,
                InputSnapshotFunctionsConstants.Middle => snapshot.Pointer.Middle,
                InputSnapshotFunctionsConstants.XButton1 => snapshot.Pointer.ExtendedButton1,
                InputSnapshotFunctionsConstants.XButton2 => snapshot.Pointer.ExtendedButton2,
                InputSnapshotFunctionsConstants.WheelUp => snapshot.Pointer.Wheel > 0,
                InputSnapshotFunctionsConstants.WheelDown => snapshot.Pointer.Wheel < 0,
                InputSnapshotFunctionsConstants.WheelLeft => snapshot.Pointer.HorizontalWheel < 0,
                InputSnapshotFunctionsConstants.WheelRight => snapshot.Pointer.HorizontalWheel > 0,
                _ => false
            };
        return EmulationInputMappingFunctions.IsControllerSourcePressed(source, controller);
    }

    /// <summary>Confie au traitement commun la reconnaissance d’un préfixe d’expression d’entrée.</summary>
    /// <param name="value">Expression complète reçue de la configuration.</param>
    /// <param name="prefix">Préfixe technique attendu, comme Keyboard: ou Mouse:.</param>
    /// <param name="source">Partie de l’expression extraite lorsque le préfixe est reconnu.</param>
    /// <returns>True lorsque le préfixe a été reconnu et retiré.</returns>
    private static bool TryRemovePrefix(string value, string prefix, out string source)
    => GWGUI.Emulation.Functions.EmulationInputMappingFunctions.TryRemovePrefix(value, prefix, out source);
}
