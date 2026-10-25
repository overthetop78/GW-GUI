using GWGUI.Emulation.Coleco.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Coleco.Common.Interfaces;

/// <summary>Adaptation de la configuration lors du choix explicite d’un backend.</summary>
/// <remarks>Ce rôle concerne les choix rendus nécessaires par l’émulateur sélectionné, avant la création
/// de sa machine. Il reste distinct de la validation, de l’application des éditeurs et de la préparation
/// des fichiers de session : sélectionner une configuration ne démarre pas le cœur et ne réécrit pas son magasin.</remarks>
internal interface IEmulatorConfigurationSelection
{
    /// <summary>Adapte les valeurs de la configuration au backend que l’utilisateur vient de choisir.</summary>
    /// <param name="configuration">Configuration initiale empruntée, dont l’identité de machine doit être conservée.</param>
    /// <returns>Configuration adaptée, ou la même instance lorsque ce choix ne demande aucune transformation.</returns>
    /// <remarks>Common utilise ce contrat pour appliquer les contraintes de sélection propres au backend.
    /// L’implémentation conserve les données qu’elle n’a pas à modifier. Le résultat reste soumis à la
    /// validation de configuration ; ce contrat ne remplace ni les contrôles de sauvegarde ni ceux du runtime.</remarks>
    MachineConfiguration SelectConfiguration(MachineConfiguration configuration);
}
