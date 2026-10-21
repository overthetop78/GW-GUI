namespace GWGUI.Emulation.Example.Common.Interfaces;
/// <summary>Contrat Common d’un backend : gestion de son installation, projection des médias et création de machines.</summary>
/// <remarks>Le contrat SDK hérité fournit les identifiants, la description, les commandes d’hôte et les opérations
/// de recherche et d’installation. Create reçoit les services empruntés et retourne une machine Common possédant
/// son cœur. La fabrique libère le cœur dans finally si sa construction échoue avant ce transfert de propriété.</remarks>
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>
{
    /// <summary>Projette les réglages validés du module vers les valeurs textuelles attendues pendant l’exécution.</summary>
    /// <param name="configuration">Configuration de machine, conservée immuable par le module.</param>
    /// <returns>Associations de clés et valeurs techniques ; les nombres utilisent un format indépendant de la langue.</returns>
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
