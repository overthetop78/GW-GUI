namespace GWGUI.Emulation.Example.Common.Contracts;

/// <summary>Champ de sélection d’un fichier de firmware demandé par un adaptateur.</summary>
/// <param name="FieldId">Identifiant stable du champ dans la configuration.</param>
/// <param name="FileName">Nom de fichier attendu lors de la préparation du système du cœur.</param>
/// <param name="LabelResourceKey">Clé du libellé présenté à l’utilisateur.</param>
/// <param name="IsRequired">Indique si ce fichier précis est obligatoire.</param>
/// <param name="MachineIds">Identifiants des modèles concernés ; null signifie que le champ ne restreint pas les modèles.</param>
/// <param name="DisplayName">Nom technique facultatif du fichier attendu.</param>
/// <param name="ExplanationResourceKey">Clé d’explication courte du champ.</param>
/// <param name="DetailedExplanationResourceKey">Clé d’explication détaillée du champ.</param>
/// <param name="CandidateFileNames">Noms alternatifs admis lors de la recherche du fichier.</param>
/// <remarks>Une obligation globale de firmware reste distincte de l’obligation de chaque emplacement.
/// Les listes et ensembles reçus restent empruntés ; aucune lecture de fichier n’est effectuée par ce record.</remarks>
internal sealed record FirmwareSlot(string FieldId, string FileName, string LabelResourceKey, bool IsRequired, IReadOnlySet<string>? MachineIds = null,
    string? DisplayName = null, string? ExplanationResourceKey = null, string? DetailedExplanationResourceKey = null, IReadOnlyList<string>? CandidateFileNames = null);
