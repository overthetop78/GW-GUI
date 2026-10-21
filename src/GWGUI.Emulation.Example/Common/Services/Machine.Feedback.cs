namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Partie de la machine qui adapte les effets de ports aux moteurs physiques choisis par l’utilisateur.</summary>
internal sealed partial class Machine
{
    /// <summary>Configuration utilisée lors du dernier calcul des associations de sorties.</summary>
    private MachineConfiguration? _feedbackConfiguration;
    /// <summary>Entrées courantes utilisées pour ce calcul, suivies séparément des options de configuration.</summary>
    private InputConfiguration? _feedbackInput;
    /// <summary>Associations autorisées, indexées par port et commande du cœur.</summary>
    private IReadOnlyDictionary<(int Port, string CommandId), EmulationControllerOutputBinding> _feedbackBindings = new Dictionary<(int, string), EmulationControllerOutputBinding>();

    /// <summary>Instantané des sorties physiques correspondant aux effets valides du cœur courant.</summary>
    /// <remarks>Une machine arrêtée, en pause ou détruite retourne une liste vide, ce qui arrête les effets précédents.
    /// Les associations sont recalculées après un changement de configuration ou d’entrées. Les intensités
    /// non finies ou hors de zéro à un sont ignorées avant toute publication vers l’hôte.</remarks>
    public IReadOnlyList<EmulationControllerOutputState> ControllerOutputs
    {
        get
        {
            if (_disposed || State != EmulationMachineState.Running || _mediaSuspended) return [];
            IReadOnlyDictionary<(int Port, string CommandId), EmulationControllerOutputBinding> bindings;
            lock (_gate)
            {
                if (!ReferenceEquals(Configuration, _feedbackConfiguration) || !ReferenceEquals(_inputConfiguration, _feedbackInput))
                {
                    var configuration = CurrentConfiguration();
                    var adapter = new Engine().SelectedAdapter(configuration);
                    var selected = new Dictionary<(int, string), EmulationControllerOutputBinding>();
                    foreach (var port in _inputConfiguration.Controllers ?? [])
                    {
                        if (port.Port < 0 || port.Port >= ConfigurationFunctions.ControllerPortCount(configuration.Model)) continue;
                        var commands = (adapter?.GetControllerOutputs(configuration, port.Peripheral) ?? []).Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
                        foreach (var binding in port.OutputBindings ?? new Dictionary<string, EmulationControllerOutputBinding>())
                            if (commands.Contains(binding.Key) && !string.IsNullOrWhiteSpace(binding.Value.DeviceId) && Enum.IsDefined(binding.Value.Motor)) selected[(port.Port, binding.Key)] = binding.Value;
                    }
                    _feedbackBindings = selected;
                    _feedbackConfiguration = Configuration;
                    _feedbackInput = _inputConfiguration;
                }
                bindings = _feedbackBindings;
            }
            if (bindings.Count == 0) return [];
            var result = new List<EmulationControllerOutputState>();
            foreach (var effect in _core.ControllerFeedback)
                if (float.IsFinite(effect.Strength) && effect.Strength is >= 0 and <= 1 && bindings.TryGetValue((effect.Port, effect.CommandId), out var target)) result.Add(new(target.DeviceId, target.Motor, effect.Strength));
            return State == EmulationMachineState.Running && !_disposed && !_mediaSuspended ? result : [];
        }
    }
}
