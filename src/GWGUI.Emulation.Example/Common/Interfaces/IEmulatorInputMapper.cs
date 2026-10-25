using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Example.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Projection des entrées physiques vers le protocole du cœur par un contrat Common unique.</summary>
/// <remarks>Chaque adaptateur consulte la configuration complète de session et conserve ses règles de projection.
/// Les instantanés reçus restent empruntés ; leurs collections ne sont pas modifiées par cet appel.</remarks>
internal interface IEmulatorInputMapper
{
    /// <summary>Projette les sources physiques vers les entrées attendues par le cœur pour ce modèle.</summary>
    /// <param name="snapshot">Instantané physique stable pendant l’opération, conservé par son appelant.</param>
    /// <param name="configuration">Configuration complète donnant le modèle, les options du cœur, les associations et l’organisation des ports.</param>
    /// <returns>Instantané adapté au protocole du cœur, avec des collections stables pour leur consommation.</returns>
    EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration);
}
