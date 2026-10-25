namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Contracts;

/// <summary>Emplacement de firmware décrit par un adaptateur pour la configuration courante.</summary>
/// <param name="FieldId">Identifiant stable du champ utilisé pour sélectionner le fichier.</param>
/// <param name="FileName">Nom attendu par le cœur ; chaîne vide lorsque le catalogue décrit seulement un champ de sélection.</param>
/// <param name="LabelResourceKey">Clé du libellé présenté dans les réglages.</param>
/// <param name="IsRequired">Obligation de fournir précisément cet emplacement, distincte du besoin global de firmware du cœur.</param>
/// <param name="MachineIds">Modèles concernés, ou null lorsque le catalogue ne les restreint pas.</param>
/// <param name="DisplayName">Nom technique facultatif présenté à côté du champ.</param>
/// <param name="ExplanationResourceKey">Clé de l'aide courte déjà associée au champ.</param>
/// <param name="DetailedExplanationResourceKey">Clé de l'aide détaillée déjà associée au champ.</param>
/// <param name="CandidateFileNames">Noms alternatifs reconnus par le catalogue, ou null sans liste déclarée.</param>
/// <param name="SystemSubdirectory">Sous-dossier relatif au répertoire système de la session ; chaîne vide pour sa racine.</param>
/// <param name="KnownMd5">Empreinte MD5 connue par le catalogue, ou null sans empreinte déclarée.</param>
/// <remarks>Ce contrat conserve les métadonnées déclarées par le backend. Les collections restent empruntées et
/// les fichiers ne sont ni ouverts ni installés. Une valeur absente ne crée aucune contrainte supplémentaire.
/// Le cœur conserve ses validations natives et la préparation des copies dans le dossier possédé par sa session.</remarks>
internal sealed record FirmwareSlot(string FieldId, string FileName, string LabelResourceKey, bool IsRequired = false, IReadOnlySet<string>? MachineIds = null, string? DisplayName = null, string? ExplanationResourceKey = null, string? DetailedExplanationResourceKey = null, IReadOnlyList<string>? CandidateFileNames = null, string SystemSubdirectory = "", string? KnownMd5 = null);
