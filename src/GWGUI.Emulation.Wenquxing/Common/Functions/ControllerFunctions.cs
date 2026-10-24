using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Wenquxing.Common.Functions;

/// <summary>Relais des entrées du module vers les contrats publics du SDK.</summary>
/// <remarks>Les adaptations de cœur passent par ce propriétaire Common ; les algorithmes restent dans le SDK.</remarks>
internal static class ControllerFunctions
{
    /// <summary>Service public de consultation des descriptions et de traitement des entrées, emprunté au SDK.</summary>
    /// <remarks>Le service ne possède aucune machine ni aucun périphérique et ne conserve pas les descriptions reçues.</remarks>
    internal static IEmulationInputProcessing InputProcessing => EmulationSdk.InputProcessing;

    /// <summary>Lit le nombre de ports matériels avant les extensions propres au cœur sélectionné.</summary>
    /// <param name="configuration">Configuration empruntée dont le modèle détermine les ports de base.</param>
    /// <returns>Nombre matériel utilisé sans adaptateur ou comme comportement par défaut de son contrat.</returns>
    internal static int ModelPortCount(MachineConfiguration configuration) => 0;

    /// <summary>Résout les ports de session par le contrat Common du cœur sélectionné.</summary>
    /// <param name="configuration">Configuration empruntée, y compris les options de multitap ou d’extension.</param>
    /// <returns>Nombre annoncé par l’adaptateur ; nombre matériel lorsque la machine ne sélectionne aucun cœur.</returns>
    /// <remarks>Le contrat reçoit la configuration entière. Cette consultation ne crée ni cœur ni ressource native.</remarks>
    internal static int PortCount(MachineConfiguration configuration) => new Engine().SelectedAdapter(configuration)?.GetControllerPortCount(configuration) ?? ModelPortCount(configuration);

    /// <summary>Service public de description, validation et projection des retours, emprunté au SDK sans ressource physique possédée.</summary>
    internal static IEmulationControllerFeedbackProcessing Feedback => EmulationSdk.ControllerFeedback;

    /// <summary>Valide la syntaxe commune des associations clavier, souris et contrôleur.</summary>
    /// <param name="binding">Association empruntée ; une valeur nulle, vide ou blanche est acceptée.</param>
    /// <returns>True si chaque composante respecte la syntaxe commune, sans modifier les libellés ni les correspondances du module.</returns>
    internal static bool IsValidBinding(string? binding) => EmulationSdk.InputProcessing.IsValidBinding(binding);

    /// <summary>Crée un accumulateur indépendant pour conserver les entrées reçues entre deux lectures du cœur.</summary>
    /// <remarks>Ce relais Common demande l’instance au contrat SDK. Les collections restent empruntées et doivent être stables ; Update additionne les mouvements avec saturation, Consume remet seulement les déplacements et molettes à zéro, et Reset efface toutes les entrées. Le service ne possède aucune ressource native.</remarks>
    /// <returns>Contrat appartenant au consommateur ; chaque appel fournit un état indépendant.</returns>
    internal static IEmulationInputAccumulator CreateInputAccumulator() => EmulationSdk.InputProcessing.CreateInputAccumulator();

    /// <summary>Publie les codes ordonnés ; le propriétaire conserve la responsabilité de mémoriser l’état suivant.</summary>
    internal static void PublishCodeTransitions<TSet>(TSet current, ref TSet previous, ExternalCoreApi.KeyboardEvent keyboardEvent, ushort modifiers,
        Func<IEnumerable<uint>, IEnumerable<uint>> orderReleased, Func<IEnumerable<uint>, IEnumerable<uint>> orderPressed, Func<uint, uint> characterFor) where TSet : IReadOnlySet<uint>
        => EmulationSdk.InputProcessing.PublishCodeTransitions(current, ref previous, keyboardEvent, modifiers, orderReleased, orderPressed, characterFor);



    /// <summary>Valide la sélection publique du clavier intégré, seul périphérique fixe de ce modèle.</summary>
    /// <param name="peripheral">Le clavier est la seule catégorie acceptée par le cœur existant.</param>
    /// <param name="configuration">Configuration empruntée de la session ; la validation ne la modifie pas.</param>
    /// <param name="port">Port zéro attendu par le protocole du clavier fixe.</param>
    /// <returns>Catégorie transmise inchangée à la commande du cœur.</returns>
    /// <remarks>Cette sélection ne crée pas de prise pour manette et ne remplace pas les associations clavier.</remarks>
    internal static EmulationPeripheralCategory FromCommon(EmulationPeripheralCategory peripheral, MachineConfiguration configuration, int port)
    {
        if (port != 0 || peripheral != EmulationPeripheralCategory.Keyboard) throw new ArgumentException(nameof(peripheral));
        return peripheral;
    }
}
