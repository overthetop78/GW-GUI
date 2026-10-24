using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Contrat de cœur utilisé par la machine Common pour exécuter et commander un backend.</summary>
/// <remarks>Les contrats hérités portent l’initialisation, les entrées, le calcul des frames, les sorties,
/// les options, les états et les commandes de médias. Le runtime public porte aussi le statut du processus,
/// les compteurs et retours audio, ainsi que la demande de fermeture du cœur. Common sérialise les accès au cœur pendant l’exécution.
/// La machine qui possède le cœur arrête sa boucle puis le détruit dans finally ; les fichiers de médias reçus
/// restent possédés par leurs appelants. L’hébergement peut être local ou confié à un processus séparé.</remarks>
internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>
{
    /// <summary>Identité du backend utilisé par ce cœur, distincte du modèle matériel émulé.</summary>
    Emulator Emulator { get; }
}

