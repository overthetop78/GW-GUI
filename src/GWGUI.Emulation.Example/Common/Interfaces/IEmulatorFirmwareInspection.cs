using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Identification d’un firmware depuis les métadonnées déjà observées par Common.</summary>
/// <remarks>Ce contrat reprend IdentifyFirmware en utilisant le résultat d’analyse unique ScannedFirmware.
/// L’adaptateur consulte ses règles et données de reconnaissance sans recevoir de flux ni posséder le fichier.
/// Il peut fournir l’identité, la version, les modèles compatibles et les autres informations du catalogue.</remarks>
internal interface IEmulatorFirmwareInspection
{
    /// <summary>Recherche une identité connue pour les données observées du candidat.</summary>
    /// <param name="firmware">Chemin, taille et empreintes observés ; Detection peut encore indiquer Unknown et la compatibilité reste à établir.</param>
    /// <returns>Résultat complété avec son identité reconnue, ou null lorsqu’aucune règle ne reconnaît le candidat.</returns>
    /// <remarks>Le résultat conserve les données observées. Les informations reconnues appartiennent à
    /// Definition ; aucun second objet Firmware parallèle ne transporte une autre copie de ces mêmes données.</remarks>
    ScannedFirmware? IdentifyFirmware(ScannedFirmware firmware);
}
