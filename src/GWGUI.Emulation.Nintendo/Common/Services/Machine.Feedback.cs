using CommonControllerFunctions = GWGUI.Emulation.Nintendo.Common.Functions.ControllerFunctions;

namespace GWGUI.Emulation.Nintendo.Common.Services;

/// <summary>Projection des effets du cœur vers les moteurs physiques associés, par le même service Common que les autres machines.</summary>
internal sealed partial class Machine
{
    /// <summary>Identité de configuration utilisée lors du dernier calcul des associations.</summary>
    private MachineConfiguration? _feedbackConfiguration;
    /// <summary>Entrées de session utilisées pour ce calcul, suivies indépendamment des options de la machine.</summary>
    private InputConfiguration? _feedbackInput;
    /// <summary>Associations validées par le service commun, indexées par port technique et commande.</summary>
    private IReadOnlyDictionary<(int Port, string CommandId), EmulationControllerOutputBinding> _feedbackBindings = new Dictionary<(int, string), EmulationControllerOutputBinding>();

    /// <summary>Instantané des sorties physiques correspondant aux effets autorisés du cœur.</summary>
    /// <remarks>La pause, la suspension et la destruction produisent une liste vide pour arrêter les sorties.
    /// Les associations sont recalculées lorsque la configuration ou les entrées de session changent.
    /// Les périphériques physiques restent possédés par leur hôte ; ce getter ne les ouvre ni ne les ferme.
    /// La seconde vérification de cycle de vie empêche de publier après un arrêt survenu pendant la projection.</remarks>
    public IReadOnlyList<EmulationControllerOutputState> ControllerOutputs
    {
        get
        {
            if (!CanPublishControllerOutputs) return [];
            IReadOnlyDictionary<(int Port, string CommandId), EmulationControllerOutputBinding> bindings;
            lock (_gate)
            {
                var configurationIdentity = Configuration;
                var input = CurrentInputConfiguration;
                if (!ReferenceEquals(configurationIdentity, _feedbackConfiguration) || !ReferenceEquals(input, _feedbackInput))
                {
                    var configuration = CurrentConfiguration();
                    var adapter = new Engine().SelectedAdapter(configuration);
                    _feedbackBindings = CommonControllerFunctions.Feedback.CreateBindings(InputSettingsFunctions.Describe(configuration).ControllerPorts,
                        port => adapter is not null && Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, true, out var peripheral) ? adapter.GetControllerOutputs(configuration, peripheral) : []);
                    _feedbackConfiguration = configurationIdentity;
                    _feedbackInput = input;
                }
                bindings = _feedbackBindings;
            }
            if (bindings.Count == 0) return [];
            var result = CommonControllerFunctions.Feedback.ResolveStates(_core.ControllerFeedback, bindings);
            return CanPublishControllerOutputs ? result : [];
        }
    }
}
