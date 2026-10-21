using GWGUI.Emulation.Example.Common.Machines.ExampleMachineA.Constants;
using GWGUI.Emulation.Example.Common.Machines.ExampleMachineA.Dictionaries;

namespace GWGUI.Emulation.Example.Common.Machines.ExampleMachineA.Functions;

/// <summary>Décrit les affectations des touches particulières pour les profils de cette famille d’exemple.</summary>
/// <remarks>Les descriptions alimentent l’éditeur de réglages commun. La lecture effective du clavier et la conversion des instantanés restent assurées par les fonctions d’entrée communes.</remarks>
internal static class ExampleMachineAInputSettingsFunctions
{
    /// <summary>Construit les champs d’affectation clavier correspondant au modèle sélectionné.</summary>
    /// <param name="configuration">Configuration dont le modèle choisit le clavier standard ou étendu.</param>
    /// <returns>Nouvelle liste des touches configurables, avec identifiant émulé, libellé traduit et touche physique par défaut.</returns>
    /// <remarks>Le profil C inclut les trois touches supplémentaires du pavé numérique. Une touche sans association par défaut reçoit une chaîne vide.
    /// Les affectations enregistrées par l’utilisateur sont appliquées par les fonctions communes qui consomment cette description.</remarks>
    internal static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions(MachineConfiguration configuration)
    {
        var keys = configuration.Model == MachineModel.ExampleMachineC ? ExampleMachineAKeyboardConstants.ModelCSpecialKeys : ExampleMachineAKeyboardConstants.SpecialKeys;
        return keys.Select(key => new InputBindingDefinition(key.ToString(), ResourceKey(key), ExampleMachineAKeyboardDictionary.DefaultHostKeys.TryGetValue(key, out var physicalKey) ? physicalKey.ToString() : string.Empty)).ToArray();
    }

    /// <summary>Résout le libellé traduit d’une touche particulière présentée dans l’éditeur.</summary>
    /// <param name="key">Identifiant du contrat de clavier, choisi parmi les touches des constantes de cette famille.</param>
    /// <returns>Clé de ressource du libellé à afficher.</returns>
    /// <exception cref="ArgumentOutOfRangeException">La touche n’appartient pas aux six touches particulières décrites ici.</exception>
    private static string ResourceKey(EmulationKey key) => key switch
    {
        EmulationKey.AcornF0 => ExampleMachineAKeyboardConstants.F0Resource,
        EmulationKey.AcornCopy => ExampleMachineAKeyboardConstants.CopyResource,
        EmulationKey.AcornShiftLock => ExampleMachineAKeyboardConstants.ShiftLockResource,
        EmulationKey.AcornKeypadHash => ExampleMachineAKeyboardConstants.KeypadHashResource,
        EmulationKey.AcornKeypadDelete => ExampleMachineAKeyboardConstants.KeypadDeleteResource,
        EmulationKey.AcornKeypadComma => ExampleMachineAKeyboardConstants.KeypadCommaResource,
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
    };
}
