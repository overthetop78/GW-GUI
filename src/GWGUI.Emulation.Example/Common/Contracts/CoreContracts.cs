using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Example.Common.Contracts;

/// <summary>Description complète d’une option native, commune à tous les adaptateurs du module.</summary>
/// <param name="Key">Clé technique envoyée au cœur, indépendante de la langue d’affichage.</param>
/// <param name="Name">Nom technique fourni par le cœur.</param>
/// <param name="Description">Description native facultative.</param>
/// <param name="Category">Catégorie native facultative, utilisée pour regrouper les options.</param>
/// <param name="DefaultValue">Code de la valeur initiale du cœur.</param>
/// <param name="CurrentValue">Code de la valeur actuellement choisie.</param>
/// <param name="Values">Choix autorisés, conservés dans leur ordre de présentation.</param>
/// <param name="IsVisible">Indique si l’éditeur doit être présenté.</param>
/// <param name="CategorizedName">Nom natif adapté à une présentation par catégories.</param>
/// <param name="CategorizedDescription">Description native adaptée à cette présentation.</param>
/// <param name="RequiresRestart">Indique si une modification nécessite de recréer le cœur.</param>
/// <param name="IsEnabled">Indique si l’utilisateur peut modifier le champ.</param>
/// <param name="ResourceKey">Clé du libellé localisé.</param>
/// <param name="ExplanationResourceKey">Clé de l’aide localisée.</param>
/// <param name="Tab">Onglet de présentation de l’option.</param>
/// <param name="NumericRange">Bornes et pas d’un éditeur numérique, ou null pour une sélection.</param>
/// <param name="NativeNumericFormat">Format du nombre attendu par le protocole natif.</param>
/// <param name="NativeZeroValue">Code particulier représentant zéro dans ce protocole.</param>
/// <param name="GraphicsApiValues">Codes natifs associés aux moteurs graphiques de l’hôte.</param>
/// <param name="RetainedRendererValues">Valeurs de moteur à conserver lors de la projection des options.</param>
/// <param name="IsGlobalStorageOption">Indique une option commune à tous les lecteurs du cœur.</param>
/// <remarks>Le record décrit des données ; il ne charge pas de bibliothèque et n’applique aucune option.
/// Les collections reçues restent empruntées et doivent être stables pendant leur consultation.</remarks>
public sealed record CoreOption(string Key, string Name, string? Description, string? Category, string DefaultValue, string CurrentValue, IReadOnlyList<CoreOptionValue> Values,
    bool IsVisible = true, string? CategorizedName = null, string? CategorizedDescription = null, bool RequiresRestart = false, bool IsEnabled = true,
    string? ResourceKey = null, string? ExplanationResourceKey = null, EmulationMachineTab Tab = EmulationMachineTab.General, EmulationSettingsNumericRange? NumericRange = null,
    string? NativeNumericFormat = null, string? NativeZeroValue = null, IReadOnlyDictionary<EmulationGraphicsApi, string>? GraphicsApiValues = null,
    IReadOnlyList<string>? RetainedRendererValues = null, bool IsGlobalStorageOption = false);

/// <summary>Choix d’une option native, avec sa présentation et sa cadence facultative.</summary>
/// <param name="Value">Code technique transmis au cœur.</param>
/// <param name="Label">Libellé technique de repli fourni par le cœur.</param>
/// <param name="ResourceKey">Clé de traduction du choix, ou null pour une donnée technique invariante.</param>
/// <param name="FrameSkipRatio">Rapport de saut d’images associé au choix, lorsqu’il en décrit un.</param>
public sealed record CoreOptionValue(string Value, string Label, string? ResourceKey = null, FrameSkipRatio? FrameSkipRatio = null);
