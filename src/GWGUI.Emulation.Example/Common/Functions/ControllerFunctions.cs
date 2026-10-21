using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Functions;

/// <summary>Stabilisation des entrées déjà traduites vers les commandes natives.</summary>
/// <remarks>Les zones mortes s’appliquent aux sources physiques avant leur encodage. Les appliquer
/// à nouveau ici altérerait les boutons transportés dans des axes natifs. Cette étape copie les
/// collections et neutralise uniquement les ports explicitement désactivés.</remarks>
internal static class ControllerFunctions
{
    /// <summary>Fournit la table clavier du contrat natif par l’interface commune du SDK.</summary>
    /// <returns>Correspondances dont les identités ne dépendent d’aucune marque de machine.</returns>
    internal static IReadOnlyDictionary<EmulationKey, uint> CreateNativeKeyMap() => EmulationSdk.InputProcessing.CreateKeyMap();

    /// <summary>Lit l’instantané stable de la frame pour un callback d’entrée natif.</summary>
    /// <param name="snapshot">Entrées déjà projetées par l’adaptateur du module.</param>
    /// <param name="port">Indice du port émulé.</param>
    /// <param name="device">Classe native de périphérique demandée par le cœur.</param>
    /// <param name="index">Indice de composante de cette classe.</param>
    /// <param name="id">Identifiant de la commande native à interroger.</param>
    /// <returns>Valeur calculée par les sélecteurs existants du SDK.</returns>
    internal static short ReadNativeInput(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id) => EmulationSdk.InputProcessing.ReadNativeInput(snapshot, port, device, index, id);

    /// <summary>Applique le changement de périphérique d’un port sans perdre ses associations physiques.</summary>
    /// <param name="configuration">Configuration d’origine à conserver immuable.</param>
    /// <param name="port">Indice du port émulé, à partir de zéro.</param>
    /// <param name="peripheral">Catégorie générique à connecter sur ce port.</param>
    /// <returns>Configuration validée dont seul le périphérique du port demandé est remplacé.</returns>
    internal static MachineConfiguration WithPeripheral(MachineConfiguration configuration, int port, PeripheralCategory peripheral)
    {
        var controllers = (configuration.Input.Controllers ?? []).Where(binding => binding.Port != port).ToList();
        var previous = configuration.Input.Controllers?.FirstOrDefault(binding => binding.Port == port);
        controllers.Add(previous is null ? new ControllerBinding(port, peripheral) : previous with { Peripheral = peripheral });
        var updated = configuration with { Input = configuration.Input with { Controllers = controllers.OrderBy(binding => binding.Port).ToArray() } };
        ConfigurationFunctions.Validate(updated);
        return updated;
    }

    /// <summary>Fige l’instantané et applique les désactivations de ports sans redimensionner les axes.</summary>
    /// <param name="input">Entrées mappées empruntées à l’appelant.</param>
    /// <param name="bindings">Associations des ports ; null conserve tous les contrôleurs.</param>
    /// <returns>Instantané stable avec les ports désactivés remplacés par leur état vide.</returns>
    internal static EmulationInputSnapshot FreezeMappedInput(EmulationInputSnapshot input, IReadOnlyList<ControllerBinding>? bindings = null)
    {
        var snapshot = EmulationSdk.InputProcessing.Freeze(input);
        if (bindings is null) return snapshot;
        var controllers = snapshot.Controllers.ToArray();
        foreach (var binding in bindings)
        {
            if (binding.Port < 0 || binding.Port >= controllers.Length) continue;
            if (binding.Peripheral == PeripheralCategory.None) controllers[binding.Port] = EmulationControllerState.Empty;
        }
        return snapshot with { Controllers = controllers };
    }
}
