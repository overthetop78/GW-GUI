using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Traitements communs des options natives et de leurs champs de réglage, sous un contrat public sans état de session.</summary>
/// <remarks>Le service est emprunté et ne conserve ni pointeur natif, ni catalogue, ni callback.
/// Les pointeurs restent valides pendant les copies ; les délégués reçus définissent les types et politiques
/// du module. Les dictionnaires mutables et paramètres ref/out conservent leurs effets existants.
/// Les délégués natifs renvoyés restent empruntés au code du cœur et cessent d’être utilisés avant son déchargement.</remarks>
public interface IEmulationCoreOptionProcessing
{
    /// <summary>Copie la visibilité native, actualise sa table et produit la projection correspondante du catalogue.</summary>
    /// <typeparam name="T">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <param name="data">Adresse native empruntée, valide pendant l’appel selon la structure prévue par cette commande.</param>
    /// <param name="visibility">Table de visibilité ; les seules opérations recevant IDictionary la mettent à jour.</param>
    /// <param name="catalog">Descriptions courantes empruntées, utilisées pour les validations et projections.</param>
    /// <param name="keyFor">Lecteur de la clé stable de chaque description.</param>
    /// <param name="update">Projection produisant une description actualisée sans modifier la description source.</param>
    /// <param name="result">Catalogue projeté en sortie ; conserve le catalogue reçu si la requête native est refusée.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool ApplyNativeVisibility<T>(nint data, IDictionary<string, bool> visibility, IReadOnlyList<T> catalog, Func<T, string> keyFor, Func<T, bool, T> update, out IReadOnlyList<T> result);

    /// <summary>Applique les valeurs reçues au dictionnaire de l’appelant ; une valeur nulle retire sa clé, selon le filtre d’exclusion fourni.</summary>
    /// <param name="options">Options ou table native reçues selon le type déclaré ; la propriété reste à l’appelant.</param>
    /// <param name="values">Valeurs ou choix reçus selon le type déclaré ; seuls les dictionnaires mutables explicitement reçus sont modifiés.</param>
    /// <param name="excluded">Filtre facultatif des clés à conserver sans modification ; null pour appliquer toutes les entrées.</param>
    void ApplyValues(IDictionary<string, string> options, IReadOnlyDictionary<string, string?> values, Func<string, bool>? excluded = null);

    /// <summary>Copie les tables anglaise et locale avec les lecteurs du module, puis fusionne leurs descriptions et, si présentes, leurs catégories.</summary>
    /// <typeparam name="TOption">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <typeparam name="TCategory">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <param name="data">Adresse native empruntée, valide pendant l’appel selon la structure prévue par cette commande.</param>
    /// <param name="englishPointerIndex">Position du pointeur anglais dans la structure native, comptée en pointeurs.</param>
    /// <param name="localPointerIndex">Position du pointeur local dans la structure native, comptée en pointeurs.</param>
    /// <param name="read">Lecteur emprunté copiant la table native dans les types du module.</param>
    /// <param name="mergeOptions">Fusion empruntée des options anglaises et locales.</param>
    /// <param name="mergeCategories">Fusion empruntée des catégories anglaises et locales.</param>
    /// <param name="categories">Catégories copiées en sortie ; elles ne dépendent plus des adresses natives après le retour.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<TOption> CopyInternationalDefinitions<TOption, TCategory>(nint data,
        int englishPointerIndex, int localPointerIndex,
        EmulationNativeOptionTableReader<TOption, TCategory> read,
        Func<IReadOnlyList<TOption>, IReadOnlyList<TOption>, IReadOnlyList<TOption>> mergeOptions,
        Func<IReadOnlyList<TCategory>, IReadOnlyList<TCategory>, IReadOnlyList<TCategory>> mergeCategories,
        out IReadOnlyList<TCategory> categories);

    /// <summary>Copie les tables anglaise et locale avec les lecteurs du module, puis fusionne leurs descriptions et, si présentes, leurs catégories.</summary>
    /// <typeparam name="TOption">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <param name="data">Adresse native empruntée, valide pendant l’appel selon la structure prévue par cette commande.</param>
    /// <param name="englishPointerIndex">Position du pointeur anglais dans la structure native, comptée en pointeurs.</param>
    /// <param name="localPointerIndex">Position du pointeur local dans la structure native, comptée en pointeurs.</param>
    /// <param name="read">Lecteur emprunté copiant la table native dans les types du module.</param>
    /// <param name="merge">Fusion empruntée combinant les descriptions de base et leurs textes localisés.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<TOption> CopyInternationalDefinitions<TOption>(nint data,
        int englishPointerIndex, int localPointerIndex, Func<nint, IReadOnlyList<TOption>> read,
        Func<IReadOnlyList<TOption>, IReadOnlyList<TOption>, IReadOnlyList<TOption>> merge);

    /// <summary>Copie les descriptions natives de version un dans les types d’options du module.</summary>
    /// <typeparam name="TOption">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <typeparam name="TValue">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <param name="definitions">Adresse empruntée de la table de descriptions natives terminée par une entrée nulle.</param>
    /// <param name="createOption">Constructeur emprunté d’une description typée à partir des champs natifs copiés.</param>
    /// <param name="createValue">Constructeur emprunté d’un choix à partir de sa valeur et de son libellé copiés.</param>
    /// <param name="valueText">Lecteur de l’identité native d’un choix, utilisé pour valider ou déterminer le défaut.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<TOption> CopyLegacyDefinitions<TOption, TValue>(nint definitions, EmulationNativeOptionFactory<TOption, TValue> createOption, Func<string, string, TValue> createValue, Func<TValue, string> valueText);

    /// <summary>Copie la table native de variables de version zéro et ses choix dans les types d’options du module.</summary>
    /// <typeparam name="TOption">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <typeparam name="TValue">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <param name="variables">Adresse empruntée de la table de variables natives terminée par une clé nulle.</param>
    /// <param name="createOption">Constructeur emprunté d’une description typée à partir des champs natifs copiés.</param>
    /// <param name="createValue">Constructeur emprunté d’un choix à partir de sa valeur et de son libellé copiés.</param>
    /// <param name="valueText">Lecteur de l’identité native d’un choix, utilisé pour valider ou déterminer le défaut.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<TOption> CopyLegacyVariables<TOption, TValue>(nint variables, EmulationNativeOptionFactory<TOption, TValue> createOption, Func<string, string, TValue> createValue, Func<TValue, string> valueText);

    /// <summary>Copie les descriptions et catégories natives de version deux, avec les constructeurs typés du module.</summary>
    /// <typeparam name="TOption">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <typeparam name="TValue">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <typeparam name="TCategory">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <param name="options">Options ou table native reçues selon le type déclaré ; la propriété reste à l’appelant.</param>
    /// <param name="categories">Catégories copiées en sortie ; elles ne dépendent plus des adresses natives après le retour.</param>
    /// <param name="createOption">Constructeur emprunté d’une description typée à partir des champs natifs copiés.</param>
    /// <param name="createValue">Constructeur emprunté d’un choix à partir de sa valeur et de son libellé copiés.</param>
    /// <param name="valueText">Lecteur de l’identité native d’un choix, utilisé pour valider ou déterminer le défaut.</param>
    /// <param name="createCategory">Constructeur emprunté d’une catégorie depuis sa clé, son nom et son explication copiés.</param>
    /// <param name="categoryKey">Lecteur de la clé d’une catégorie pour vérifier les références des options.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<TOption> CopyVersionTwoDefinitions<TOption, TValue, TCategory>(nint options,
        out IReadOnlyList<TCategory> categories, EmulationNativeOptionFactory<TOption, TValue> createOption,
        Func<string, string, TValue> createValue, Func<TValue, string> valueText,
        Func<string, string, string?, TCategory> createCategory, Func<TCategory, string> categoryKey);

    /// <summary>Construit un champ déjà décrit en évaluant les fournisseurs de valeur, d’explication et d’état dans leur ordre existant.</summary>
    /// <param name="key">Identifiant stable, ou lecteur de cet identifiant lorsque le paramètre est un délégué typé.</param>
    /// <param name="tab">Onglet auquel le champ appartient.</param>
    /// <param name="block">Identifiant stable du bloc auquel le champ appartient.</param>
    /// <param name="resourceKey">Clé de ressource déjà sélectionnée pour présenter le champ.</param>
    /// <param name="editor">Type d’éditeur choisi pour ce champ.</param>
    /// <param name="choices">Choix déjà projetés dans leurs contrats de présentation.</param>
    /// <param name="currentValue">Fournisseur de la valeur courante du champ.</param>
    /// <param name="explanation">Fournisseur de la clé de ressource d’explication, éventuellement nulle.</param>
    /// <param name="enabledValue">Fournisseur de la valeur native correspondant à l’état actif.</param>
    /// <param name="disabledValue">Fournisseur de la valeur native correspondant à l’état inactif.</param>
    /// <param name="isEnabled">Fournisseur facultatif de l’état actif du champ ; null le laisse actif.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    EmulationSettingsField CreatePreparedSettingsField(string key, EmulationMachineTab tab,
        string block, string resourceKey, EmulationSettingsEditor editor, IReadOnlyList<EmulationSettingsChoice> choices,
        Func<string> currentValue, Func<string?> explanation, Func<string> enabledValue,
        Func<string> disabledValue, Func<bool>? isEnabled = null);

    /// <summary>Groupe les options visibles ayant une catégorie dans les blocs de réglage et les onglets existants.</summary>
    /// <typeparam name="TOption">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <param name="options">Options ou table native reçues selon le type déclaré ; la propriété reste à l’appelant.</param>
    /// <param name="visible">Prédicat définissant les options présentées dans les blocs.</param>
    /// <param name="category">Lecteur de la catégorie native ; null exclut une option du regroupement.</param>
    /// <param name="resourceFor">Fournisseur de la clé de ressource de chaque onglet.</param>
    /// <param name="field">Projection d’une description vers son champ, avec l’onglet et la ressource du bloc.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<EmulationSettingsBlock> CreateSettingsBlocks<TOption>(IReadOnlyList<TOption> options,
        Func<TOption, bool> visible, Func<TOption, string?> category,
        Func<EmulationMachineTab, string> resourceFor,
        Func<TOption, EmulationMachineTab, string, EmulationSettingsField> field);

    /// <summary>Construit un champ de choix et sélectionne le mode interrupteur lorsque les deux choix actif et inactif sont identifiés.</summary>
    /// <typeparam name="TValue">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <param name="key">Identifiant stable, ou lecteur de cet identifiant lorsque le paramètre est un délégué typé.</param>
    /// <param name="tab">Onglet auquel le champ appartient.</param>
    /// <param name="block">Identifiant stable du bloc auquel le champ appartient.</param>
    /// <param name="resource">Fournisseur de la clé de ressource du champ, évalué pendant sa construction.</param>
    /// <param name="values">Valeurs ou choix reçus selon le type déclaré ; seuls les dictionnaires mutables explicitement reçus sont modifiés.</param>
    /// <param name="choice">Projection d’un choix du module vers le contrat commun de présentation.</param>
    /// <param name="enabledChoice">Prédicat identifiant le choix correspondant à l’état actif.</param>
    /// <param name="disabledChoice">Prédicat identifiant le choix correspondant à l’état inactif.</param>
    /// <param name="nativeValue">Fournisseur de la valeur native, ou de son adresse pour une variable ; une adresse retournée reste possédée par le fournisseur.</param>
    /// <param name="currentValue">Fournisseur de la valeur courante du champ.</param>
    /// <param name="explanation">Fournisseur de la clé de ressource d’explication, éventuellement nulle.</param>
    /// <param name="isEnabled">Fournisseur facultatif de l’état actif du champ ; null le laisse actif.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    EmulationSettingsField CreateSettingsField<TValue>(string key, EmulationMachineTab tab,
        string block, Func<string> resource, IReadOnlyList<TValue> values,
        Func<TValue, EmulationSettingsChoice> choice, Func<TValue, bool> enabledChoice,
        Func<TValue, bool> disabledChoice, Func<TValue, string> nativeValue,
        Func<string> currentValue, Func<string?> explanation, Func<bool>? isEnabled = null)
        where TValue : class;

    /// <summary>Construit un champ dont le mode interrupteur et les valeurs actives et inactives sont décidés par le module.</summary>
    /// <typeparam name="TValue">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <param name="key">Identifiant stable, ou lecteur de cet identifiant lorsque le paramètre est un délégué typé.</param>
    /// <param name="tab">Onglet auquel le champ appartient.</param>
    /// <param name="block">Identifiant stable du bloc auquel le champ appartient.</param>
    /// <param name="resource">Fournisseur de la clé de ressource du champ, évalué pendant sa construction.</param>
    /// <param name="values">Valeurs ou choix reçus selon le type déclaré ; seuls les dictionnaires mutables explicitement reçus sont modifiés.</param>
    /// <param name="choice">Projection d’un choix du module vers le contrat commun de présentation.</param>
    /// <param name="isToggle">Prédicat décidant du mode interrupteur.</param>
    /// <param name="currentValue">Fournisseur de la valeur courante du champ.</param>
    /// <param name="explanation">Fournisseur de la clé de ressource d’explication, éventuellement nulle.</param>
    /// <param name="enabledValue">Fournisseur de la valeur native correspondant à l’état actif.</param>
    /// <param name="disabledValue">Fournisseur de la valeur native correspondant à l’état inactif.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    EmulationSettingsField CreateSettingsFieldWithToggle<TValue>(string key, EmulationMachineTab tab,
        string block, Func<string> resource, IReadOnlyList<TValue> values, Func<TValue, EmulationSettingsChoice> choice,
        Func<bool> isToggle, Func<string> currentValue, Func<string?> explanation,
        Func<bool, string>? enabledValue = null, Func<bool, string>? disabledValue = null)
        where TValue : class;

    /// <summary>Recense les identifiants des champs visibles, actifs et éditables des blocs reçus.</summary>
    /// <param name="blocks">Blocs empruntés, dont les champs sont parcourus sans modification.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    HashSet<string> EditableFieldIds(IEnumerable<EmulationSettingsBlock> blocks);

    /// <summary>Copie les seules options autorisées pour l’émulateur, en conservant les trois clés audio fournies par le module.</summary>
    /// <param name="allowedKeys">Identifiants des options propres à l’émulateur, comparés ordinalement.</param>
    /// <param name="options">Options ou table native reçues selon le type déclaré ; la propriété reste à l’appelant.</param>
    /// <param name="audioOutputKey">Identifiant invariant du réglage de sortie audio à conserver.</param>
    /// <param name="audioLatencyKey">Identifiant invariant du réglage de latence audio à conserver.</param>
    /// <param name="audioVolumeKey">Identifiant invariant du réglage de volume audio à conserver.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    Dictionary<string, string> FilterForEmulator(IEnumerable<string> allowedKeys, IReadOnlyDictionary<string, string> options, string audioOutputKey, string audioLatencyKey, string audioVolumeKey);

    /// <summary>Consomme atomiquement l’indicateur de modification et l’écrit dans le booléen natif emprunté.</summary>
    /// <param name="data">Adresse native empruntée, valide pendant l’appel selon la structure prévue par cette commande.</param>
    /// <param name="updated">Indicateur possédé par l’appelant, consommé par échange atomique après validation de l’adresse native.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool GetAndClearUpdated(nint data, ref int updated);

    /// <summary>Fusionne les textes par clé en conservant l’ordre du catalogue de base et les descriptions absentes de la traduction.</summary>
    /// <typeparam name="T">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <param name="english">Descriptions du catalogue de base, conservées dans leur ordre initial.</param>
    /// <param name="localized">Descriptions localisées, rapprochées du catalogue de base par leur clé ordinale.</param>
    /// <param name="key">Identifiant stable, ou lecteur de cet identifiant lorsque le paramètre est un délégué typé.</param>
    /// <param name="merge">Fusion empruntée combinant les descriptions de base et leurs textes localisés.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<T> MergeLocalized<T>(IReadOnlyList<T> english, IReadOnlyList<T> localized, Func<T, string> key, Func<T, T, T> merge);

    /// <summary>Copie les options déclarées par la table native de variables de version zéro et initialise les valeurs absentes.</summary>
    /// <param name="data">Adresse native empruntée, valide pendant l’appel selon la structure prévue par cette commande.</param>
    /// <param name="currentValues">Valeurs courantes ; les lecteurs natifs ajoutent seulement les défauts des clés absentes.</param>
    /// <param name="visibility">Table de visibilité ; les seules opérations recevant IDictionary la mettent à jour.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<EmulationOption> ReadLegacy(nint data, IDictionary<string, string> currentValues, IReadOnlyDictionary<string, bool> visibility);

    /// <summary>Copie les descriptions natives de version un et initialise les valeurs absentes ; une adresse nulle produit null.</summary>
    /// <param name="definitions">Adresse empruntée de la table de descriptions natives terminée par une entrée nulle.</param>
    /// <param name="currentValues">Valeurs courantes ; les lecteurs natifs ajoutent seulement les défauts des clés absentes.</param>
    /// <param name="visibility">Table de visibilité ; les seules opérations recevant IDictionary la mettent à jour.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<EmulationOption>? ReadVersionOne(nint definitions, IDictionary<string, string> currentValues, IReadOnlyDictionary<string, bool> visibility);

    /// <summary>Copie les descriptions natives de version deux et initialise les valeurs absentes ; une adresse nulle produit null.</summary>
    /// <param name="options">Options ou table native reçues selon le type déclaré ; la propriété reste à l’appelant.</param>
    /// <param name="currentValues">Valeurs courantes ; les lecteurs natifs ajoutent seulement les défauts des clés absentes.</param>
    /// <param name="visibility">Table de visibilité ; les seules opérations recevant IDictionary la mettent à jour.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<EmulationOption>? ReadVersionTwo(nint options, IDictionary<string, string> currentValues, IReadOnlyDictionary<string, bool> visibility);

    /// <summary>Écrit la valeur d’une variable native demandée ; la valeur de retour pour une clé indisponible reste celle choisie par le module.</summary>
    /// <param name="data">Adresse native empruntée, valide pendant l’appel selon la structure prévue par cette commande.</param>
    /// <param name="values">Valeurs ou choix reçus selon le type déclaré ; seuls les dictionnaires mutables explicitement reçus sont modifiés.</param>
    /// <param name="nativeValue">Fournisseur de la valeur native, ou de son adresse pour une variable ; une adresse retournée reste possédée par le fournisseur.</param>
    /// <param name="unavailableResult">Résultat renvoyé lorsque la variable ou sa valeur ne peut pas être fournie.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool ReturnNativeValue(nint data, IReadOnlyDictionary<string, string> values, Func<string, string, nint> nativeValue, bool unavailableResult = true);

    /// <summary>Retourne la table locale disponible ou la table anglaise ; l’adresse reste empruntée au cœur.</summary>
    /// <param name="data">Adresse native empruntée, valide pendant l’appel selon la structure prévue par cette commande.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    nint SelectInternationalDefinitions(nint data);

    /// <summary>Copie la clé et la valeur de la variable native puis appelle le setter fourni si elles sont valides.</summary>
    /// <param name="data">Adresse native empruntée, valide pendant l’appel selon la structure prévue par cette commande.</param>
    /// <param name="setValue">Setter emprunté qui valide et applique la valeur copiée.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool SetNativeValue(nint data, Func<string, string, bool> setValue);

    /// <summary>Lit le callback natif de rafraîchissement de l’affichage ; le code du cœur doit rester chargé pendant son utilisation.</summary>
    /// <param name="data">Adresse native empruntée, valide pendant l’appel selon la structure prévue par cette commande.</param>
    /// <param name="callback">Délégué natif emprunté en sortie, éventuellement nul ; le destinataire en borne l’usage à la vie du cœur.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool TryGetDisplayCallback(nint data, out ExternalCoreApi.UpdateCoreOptionsDisplay? callback);

    /// <summary>Valide les valeurs configurées contre les choix déclarés et appelle le diagnostic fourni pour les rejets.</summary>
    /// <typeparam name="TOption">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <typeparam name="TValue">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <param name="configuredKeys">Clés provenant de la configuration, qui doivent être validées.</param>
    /// <param name="values">Valeurs ou choix reçus selon le type déclaré ; seuls les dictionnaires mutables explicitement reçus sont modifiés.</param>
    /// <param name="catalog">Descriptions courantes empruntées, utilisées pour les validations et projections.</param>
    /// <param name="keyFor">Lecteur de la clé stable de chaque description.</param>
    /// <param name="choicesFor">Lecteur des choix autorisés par une description ; une liste vide conserve la validation existante des valeurs libres.</param>
    /// <param name="valueFor">Lecteur de l’identité native d’un choix.</param>
    /// <param name="invalid">Diagnostic emprunté appelé avec la clé puis la valeur refusée ; ses exceptions sont propagées.</param>
    void ValidateConfiguredValues<TOption, TValue>(IEnumerable<string> configuredKeys,
        IReadOnlyDictionary<string, string> values, IReadOnlyList<TOption> catalog, Func<TOption, string> keyFor,
        Func<TOption, IReadOnlyList<TValue>> choicesFor, Func<TValue, string> valueFor, Action<string, string> invalid)
        where TOption : class;

    /// <summary>Projette les descriptions avec leurs défauts et valeurs courantes, en conservant la politique de mise à jour du module.</summary>
    /// <typeparam name="TOption">Type du module conservé par les lecteurs, constructeurs et projections reçus.</typeparam>
    /// <param name="options">Options ou table native reçues selon le type déclaré ; la propriété reste à l’appelant.</param>
    /// <param name="defaults">Valeurs par défaut définies par le module.</param>
    /// <param name="currentValues">Valeurs courantes ; les lecteurs natifs ajoutent seulement les défauts des clés absentes.</param>
    /// <param name="key">Identifiant stable, ou lecteur de cet identifiant lorsque le paramètre est un délégué typé.</param>
    /// <param name="originalDefault">Lecteur de la valeur initiale utilisée si aucun défaut spécifique n’est fourni.</param>
    /// <param name="update">Projection produisant une description actualisée sans modifier la description source.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<TOption> WithDefaults<TOption>(IReadOnlyList<TOption> options,
        IReadOnlyDictionary<string, string> defaults, IReadOnlyDictionary<string, string> currentValues,
        Func<TOption, string> key, Func<TOption, string> originalDefault,
        Func<TOption, string, string, TOption> update);
}
