using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Ajoute les impulsions de démarrage d’une cassette aux entrées physiques déjà mappées.</summary>
/// <remarks>Les accès sont sérialisés par un verrou. Une commande Play produit une impulsion Retour
/// sur une frame ; la frame suivante retrouve les touches physiques. Aucun média n’est écrit ici.</remarks>
internal sealed class CassetteInputController
{
    /// <summary>Délai de base avant l’impulsion automatique, exprimé en frames calculées.</summary>
    internal const int AutomaticReturnDelayFrames = 75;
    /// <summary>Commandes de transport prises en charge par ce contrôleur de démarrage.</summary>
    internal static IReadOnlySet<EmulationCassetteCommand> AvailableCommands { get; } = new HashSet<EmulationCassetteCommand> { EmulationCassetteCommand.Play };
    /// <summary>Verrou protégeant la file, l'instantané physique et le compteur de retour automatique.</summary>
    /// <remarks>Les producteurs d'entrées et la boucle de frames peuvent appeler le contrôleur depuis des threads distincts.</remarks>
    private readonly object _gate = new();
    /// <summary>Impulsions à consommer dans leur ordre d'arrivée, à raison d'une touche ajoutée par frame.</summary>
    /// <remarks>La file possède ses éléments ; elle n'acquiert aucune ressource de cassette ni aucun flux de média.</remarks>
    private readonly Queue<EmulationKey> _pulses = new();
    /// <summary>Instantané physique emprunté qui reste la référence après chaque impulsion synthétique.</summary>
    /// <remarks>NextFrame copie les touches uniquement pour ajouter une impulsion ; il ne modifie pas cet instantané.</remarks>
    private EmulationInputSnapshot _physicalInput = EmulationInputSnapshot.Empty;
    /// <summary>Nombre de frames physiques à laisser passer avant l'impulsion Retour automatique.</summary>
    /// <remarks>Zéro déclenche l'impulsion au prochain appel ; moins un désactive ce déclenchement après émission.</remarks>
    private int _automaticReturnFrames;

    /// <summary>Prépare le délai de retour selon les règles de démarrage de la configuration.</summary>
    /// <param name="configuration">Configuration empruntée ; les médias restent sous la responsabilité de leur propriétaire.</param>
    internal CassetteInputController(MachineConfiguration configuration)
    {
        _automaticReturnFrames = CassetteBootFunctions.RequiresDelayedReturn(configuration) ? AutomaticReturnDelayFrames : -1;
    }

    /// <summary>Remplace les entrées physiques qui seront complétées lors de la prochaine frame.</summary>
    /// <param name="snapshot">Instantané stable fourni par le propriétaire des entrées.</param>
    internal void SetPhysicalInput(EmulationInputSnapshot snapshot)
    {
        lock (_gate) _physicalInput = snapshot;
    }

    /// <summary>Planifie une impulsion Retour pour déclencher la lecture du contenu chargé.</summary>
    internal void Play()
    {
        lock (_gate) _pulses.Enqueue(EmulationKey.Return);
    }

    /// <summary>Consomme une impulsion éventuelle et renvoie les entrées de la frame suivante.</summary>
    /// <returns>Instantané physique initial, ou copie de ses touches complétée par l’impulsion.</returns>
    /// <remarks>La valeur moins un désactive le retour automatique après son déclenchement.
    /// L’ensemble des touches n’est copié que lorsqu’une impulsion doit être ajoutée.</remarks>
    internal EmulationInputSnapshot NextFrame()
    {
        lock (_gate)
        {
            if (_automaticReturnFrames > 0) _automaticReturnFrames--;
            else if (_automaticReturnFrames == 0)
            {
                _pulses.Enqueue(EmulationKey.Return);
                _automaticReturnFrames = -1;
            }
            if (_pulses.Count == 0) return _physicalInput;
            var keys = _physicalInput.Keys.ToHashSet();
            keys.Add(_pulses.Dequeue());
            return _physicalInput with { Keys = keys };
        }
    }
}
