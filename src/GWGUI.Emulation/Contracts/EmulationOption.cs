namespace GWGUI.Emulation.Contracts;

/// <summary>Description d'une option native et de ses métadonnées de configuration.</summary>
/// <param name="Key">Clé technique envoyée au cœur.</param>
/// <param name="Name">Nom technique de repli annoncé par le cœur.</param>
/// <param name="Description">Description native facultative.</param>
/// <param name="Category">Catégorie native facultative.</param>
/// <param name="DefaultValue">Code de la valeur initiale du cœur.</param>
/// <param name="CurrentValue">Code de la valeur courante.</param>
/// <param name="Values">Choix autorisés dans leur ordre de présentation.</param>
/// <param name="IsVisible">Indique si le champ est présenté.</param>
/// <param name="RequiresRestart">Indique si un changement nécessite de recréer le cœur.</param>
/// <param name="CategorizedName">Nom natif pour une présentation par catégories.</param>
/// <param name="CategorizedDescription">Description native pour une présentation par catégories.</param>
/// <param name="IsEnabled">Indique si le champ peut être modifié.</param>
/// <param name="ResourceKey">Clé du libellé localisé.</param>
/// <param name="ExplanationResourceKey">Clé de l'aide localisée.</param>
/// <param name="RequiresRuntimeReload">Indique si la modification nécessite de recharger les options du runtime.</param>
/// <param name="Tab">Onglet auquel appartient l'option.</param>
/// <param name="NumericRange">Bornes et pas numériques, ou null pour une sélection.</param>
/// <param name="NativeNumericFormat">Format du nombre attendu par le cœur.</param>
/// <param name="NativeZeroValue">Code particulier représentant zéro dans le protocole natif.</param>
/// <param name="GraphicsApiValues">Codes natifs associés aux moteurs graphiques de l'hôte.</param>
/// <param name="RetainedRendererValues">Codes de moteurs à conserver pendant la projection des réglages.</param>
/// <param name="IsGlobalStorageOption">Indique une option commune aux lecteurs, distincte du réglage d'un seul lecteur.</param>
/// <param name="MachineIds">Profils concernés, ou null sans restriction déclarée.</param>
/// <remarks>La description transporte des données. Les collections reçues restent empruntées et stables
/// pendant leur consultation. Elle ne possède ni cœur ni ressource native et n'applique aucune option.
/// Les champs facultatifs reprennent les valeurs par défaut des descriptions existantes.</remarks>
public sealed record EmulationOption(string Key, string Name, string? Description, string? Category, string DefaultValue, string CurrentValue, IReadOnlyList<EmulationOptionValue> Values, bool IsVisible = true, bool RequiresRestart = false,
    string? CategorizedName = null, string? CategorizedDescription = null, bool IsEnabled = true, string? ResourceKey = null, string? ExplanationResourceKey = null, bool RequiresRuntimeReload = false,
    EmulationMachineTab Tab = EmulationMachineTab.General, EmulationSettingsNumericRange? NumericRange = null, string? NativeNumericFormat = null, string? NativeZeroValue = null,
    IReadOnlyDictionary<EmulationGraphicsApi, string>? GraphicsApiValues = null, IReadOnlyList<string>? RetainedRendererValues = null, bool IsGlobalStorageOption = false, IReadOnlySet<string>? MachineIds = null);
