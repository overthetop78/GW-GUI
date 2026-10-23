using GWGUI.Emulation.Example.Emulators.Internal.Constants;
using GWGUI.Emulation.Example.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Example.Emulators.Internal.Factories;

/// <summary>Adaptateur du cœur ExampleInternal sous les mêmes interfaces que les deux autres émulateurs fictifs.</summary>
/// <remarks>Le catalogue découvre cette fabrique par son interface. Les deux profils sont déclarés par
/// MachineFactory ; leur validation, leur stockage et leur boucle de session restent dans Common.
/// Le cœur est appelé directement dans le processus de la machine, par le même contrat IEmulatorCore.</remarks>
internal sealed class InternalMachineFactory() : MachineFactory(Emulator.Internal, EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorConstants.DescriptionResourceKey), IEmulatorMachineDefaults
{
    /// <summary>Recommande le cœur interne pour les deux profils matériels que sa définition prend en charge.</summary>
    /// <param name="machineId">Identifiant stable du profil demandé par le catalogue ou par Engine.</param>
    /// <returns>True pour un profil pris en charge ; false laisse Common conserver son choix initial ou examiner les autres adaptateurs.</returns>
    /// <remarks>La fabrique dérivée déclare explicitement IEmulatorMachineDefaults dans sa liste de contrats
    /// pour relier cette méthode à la recommandation héritée. Common consulte cette préférence uniquement
    /// pendant la création initiale et adapte ensuite la configuration via son contrat de sélection.
    /// Déclarer une recommandation ne télécharge rien et ne remplace pas le choix explicite enregistré
    /// par un utilisateur. Les deux autres simulations conservent le comportement Common sans recommandation.</remarks>
    public bool IsDefaultFor(string machineId) => Definition.MachineIds.Contains(machineId);
}
