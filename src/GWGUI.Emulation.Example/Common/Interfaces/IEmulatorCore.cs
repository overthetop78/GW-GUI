using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Contrat de cœur utilisé par la machine Common pour exécuter et commander un backend.</summary>
/// <remarks>Les contrats hérités portent l’initialisation, les entrées, le calcul des frames, les sorties,
/// les options, les états et les commandes de médias. Le runtime public porte aussi le statut du processus,
/// les compteurs et retours audio, la sélection des images, la capacité de capture et la demande de fermeture du cœur. Common sérialise les accès au cœur pendant l’exécution.
/// La machine qui possède le cœur arrête sa boucle puis le détruit dans finally ; les fichiers de médias reçus
/// restent possédés par leurs appelants. Le contrat d’identité conserve le type de backend du module, distinct du modèle matériel. L’hébergement peut être local ou confié à un processus séparé.</remarks>
internal interface IEmulatorCore : IEmulationCoreRuntime<MachineConfiguration, CoreOption>, IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>, IEmulationCoreIdentity<Emulator>
{
}

