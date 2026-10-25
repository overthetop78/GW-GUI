using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Construction et présentation communes des réglages matériels et audio, sans état de session.</summary>
/// <remarks>Le service est emprunté. Les fournisseurs de caractéristiques sont évalués pendant l’appel
/// dans leur ordre existant. Les clés partagées restent dans le SDK ; CPU, mémoire, résolutions, unités
/// et autres valeurs invariantes restent dans leurs données communes. Les résultats conservent les choix
/// et champs reçus selon leurs contrats, sans ajouter de copie ou de traduction par module.</remarks>
public interface IEmulationHardwareSettings
{
    /// <summary>Ajoute les blocs CPU, mémoire et résolution à partir des caractéristiques effectivement fournies par le module.</summary>
    /// <param name="blocks">Liste mutable du module recevant les blocs construits.</param>
    /// <param name="fieldPrefix">Préfixe invariant des identifiants de champs du module.</param>
    /// <param name="cpuResourceKey">Clé de ressource décrivant le modèle de processeur.</param>
    /// <param name="cpuFrequency">Fournisseur emprunté de la fréquence présentée ; null comme résultat omet ce champ.</param>
    /// <param name="mainMemory">Fournisseur emprunté de la mémoire principale présentée ; null comme résultat omet ce champ.</param>
    /// <param name="displayMemory">Fournisseur emprunté de la mémoire vidéo présentée ; null comme résultat omet ce champ.</param>
    /// <param name="displayMemoryResourceKey">Clé de ressource du libellé de mémoire vidéo.</param>
    /// <param name="resolution">Fournisseur emprunté de la résolution présentée ; null comme résultat omet le bloc.</param>
    /// <param name="isCompactCpu">Présentation compacte du bloc processeur choisie par le module.</param>
    void AddBlocks(List<EmulationSettingsBlock> blocks, string fieldPrefix, string cpuResourceKey,
        Func<string?> cpuFrequency, Func<string?> mainMemory, Func<string?> displayMemory,
        string displayMemoryResourceKey, Func<string?> resolution, bool isCompactCpu);

    /// <summary>Construit la sélection des sorties audio de l’hôte avec la source de choix commune.</summary>
    /// <param name="id">Identifiant invariant du choix ou du champ, conservé sans traduction.</param>
    /// <param name="block">Identifiant invariant du bloc du champ.</param>
    /// <param name="label">Clé de ressource du libellé du champ.</param>
    /// <param name="value">Valeur courante, conservée dans sa représentation existante.</param>
    /// <param name="choices">Choix reçus, transmis au contrat du champ selon ses règles existantes.</param>
    /// <param name="shortHelp">Clé de ressource facultative de l’aide courte.</param>
    /// <param name="detailedHelp">Clé de ressource facultative de l’aide détaillée.</param>
    /// <returns>Résultat du constructeur ou format existant, avec les mêmes valeurs et ressources.</returns>
    EmulationSettingsField AudioOutput(string id, string block, string label, string value, IReadOnlyList<EmulationSettingsChoice> choices, string? shortHelp, string? detailedHelp);

    /// <summary>Construit une sélection audio numérique avec les options, valeurs de repli et formats fournis.</summary>
    /// <param name="options">Valeurs courantes empruntées au module, sans modification du dictionnaire.</param>
    /// <param name="id">Identifiant invariant du choix ou du champ, conservé sans traduction.</param>
    /// <param name="block">Identifiant invariant du bloc du champ.</param>
    /// <param name="resource">Clé de ressource du libellé de la sélection audio.</param>
    /// <param name="fallback">Valeur de repli lorsqu’aucune option n’est enregistrée.</param>
    /// <param name="values">Valeurs numériques disponibles, conservées dans leur ordre.</param>
    /// <param name="unit">Unité invariante affichée après chaque valeur numérique.</param>
    /// <param name="labelFormatProvider">Culture de présentation des libellés ; null conserve le formatage courant.</param>
    /// <returns>Résultat du constructeur ou format existant, avec les mêmes valeurs et ressources.</returns>
    EmulationSettingsField AudioSelection(IReadOnlyDictionary<string, string> options, string id, string block, string resource, int fallback, IReadOnlyList<int> values, string unit, IFormatProvider? labelFormatProvider);

    /// <summary>Construit un bloc avec son onglet, ses colonnes, son icône et sa présentation compacte éventuelle.</summary>
    /// <param name="id">Identifiant invariant du choix ou du champ, conservé sans traduction.</param>
    /// <param name="tab">Onglet du champ ou du bloc.</param>
    /// <param name="title">Clé de ressource du titre du bloc.</param>
    /// <param name="icon">Identifiant invariant de l’icône du bloc.</param>
    /// <param name="columns">Nombre de colonnes de présentation.</param>
    /// <param name="fields">Champs déjà construits, transmis au contrat du bloc.</param>
    /// <param name="isCompact">Présentation compacte éventuelle du bloc.</param>
    /// <returns>Résultat du constructeur ou format existant, avec les mêmes valeurs et ressources.</returns>
    EmulationSettingsBlock Block(string id, EmulationMachineTab tab, string title, string icon, int columns, EmulationSettingsField[] fields, bool isCompact = false);

    /// <summary>Formate une fréquence en MHz ou kHz selon la politique reçue et la culture courante.</summary>
    /// <param name="hertz">Fréquence en Hz, avec les règles de formatage existantes.</param>
    /// <param name="useKilohertz">True pour employer les kHz sous le seuil existant ; false pour conserver les MHz.</param>
    /// <returns>Résultat du constructeur ou format existant, avec les mêmes valeurs et ressources.</returns>
    string FormatFrequency(long hertz, bool useKilohertz = false);

    /// <summary>Formate une fréquence en MHz avec le format numérique reçu et la culture courante.</summary>
    /// <param name="hertz">Fréquence en Hz, avec les règles de formatage existantes.</param>
    /// <param name="format">Format numérique transmis à la culture courante.</param>
    /// <returns>Résultat du constructeur ou format existant, avec les mêmes valeurs et ressources.</returns>
    string FormatMegahertz(long hertz, string format);

    /// <summary>Formate une capacité mémoire selon la représentation invariante commune.</summary>
    /// <param name="bytes">Capacité en octets, formatée en B ou KiB selon le seuil existant.</param>
    /// <returns>Résultat du constructeur ou format existant, avec les mêmes valeurs et ressources.</returns>
    string FormatMemory(long bytes);

    /// <summary>Construit un champ informatif avec les ressources d’aide reçues.</summary>
    /// <param name="id">Identifiant invariant du choix ou du champ, conservé sans traduction.</param>
    /// <param name="tab">Onglet du champ ou du bloc.</param>
    /// <param name="block">Identifiant invariant du bloc du champ.</param>
    /// <param name="label">Clé de ressource du libellé du champ.</param>
    /// <param name="value">Valeur courante, conservée dans sa représentation existante.</param>
    /// <param name="shortHelp">Clé de ressource facultative de l’aide courte.</param>
    /// <param name="detailedHelp">Clé de ressource facultative de l’aide détaillée.</param>
    /// <returns>Résultat du constructeur ou format existant, avec les mêmes valeurs et ressources.</returns>
    EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value, string? shortHelp, string? detailedHelp);

    /// <summary>Crée un choix dont le texte est invariant, avec la même identité et sans clé de traduction ajoutée.</summary>
    /// <param name="id">Identifiant invariant du choix ou du champ, conservé sans traduction.</param>
    /// <param name="text">Texte invariant reçu du module.</param>
    /// <param name="numericValue">Valeur numérique facultative associée au choix.</param>
    /// <returns>Résultat du constructeur ou format existant, avec les mêmes valeurs et ressources.</returns>
    EmulationSettingsChoice InvariantChoice(string id, string text, long? numericValue = null);

    /// <summary>Construit le champ numérique décrit par le module, sans ajouter de conversion ou de validation.</summary>
    /// <param name="id">Identifiant invariant du choix ou du champ, conservé sans traduction.</param>
    /// <param name="tab">Onglet du champ ou du bloc.</param>
    /// <param name="block">Identifiant invariant du bloc du champ.</param>
    /// <param name="label">Clé de ressource du libellé du champ.</param>
    /// <param name="value">Valeur courante, conservée dans sa représentation existante.</param>
    /// <param name="shortHelp">Clé de ressource facultative de l’aide courte.</param>
    /// <param name="detailedHelp">Clé de ressource facultative de l’aide détaillée.</param>
    /// <returns>Résultat du constructeur ou format existant, avec les mêmes valeurs et ressources.</returns>
    EmulationSettingsField Number(string id, EmulationMachineTab tab, string block, string label, string value, string? shortHelp, string? detailedHelp);

    /// <summary>Construit une sélection avec les aides et la politique de redémarrage reçues.</summary>
    /// <param name="id">Identifiant invariant du choix ou du champ, conservé sans traduction.</param>
    /// <param name="tab">Onglet du champ ou du bloc.</param>
    /// <param name="block">Identifiant invariant du bloc du champ.</param>
    /// <param name="label">Clé de ressource du libellé du champ.</param>
    /// <param name="value">Valeur courante, conservée dans sa représentation existante.</param>
    /// <param name="choices">Choix reçus, transmis au contrat du champ selon ses règles existantes.</param>
    /// <param name="shortHelp">Clé de ressource facultative de l’aide courte.</param>
    /// <param name="detailedHelp">Clé de ressource facultative de l’aide détaillée.</param>
    /// <param name="isEnabled">État actif du champ.</param>
    /// <param name="requiresRestart">Exigence de redémarrage associée à une modification.</param>
    /// <returns>Résultat du constructeur ou format existant, avec les mêmes valeurs et ressources.</returns>
    EmulationSettingsField Select(string id, EmulationMachineTab tab, string block, string label, string value,
        IReadOnlyList<EmulationSettingsChoice> choices, string? shortHelp, string? detailedHelp,
        bool isEnabled = true, bool requiresRestart = false);

    /// <summary>Construit un réglage booléen avec les valeurs activée et désactivée communes.</summary>
    /// <param name="id">Identifiant invariant du choix ou du champ, conservé sans traduction.</param>
    /// <param name="tab">Onglet du champ ou du bloc.</param>
    /// <param name="block">Identifiant invariant du bloc du champ.</param>
    /// <param name="label">Clé de ressource du libellé du champ.</param>
    /// <param name="value">Valeur courante, conservée dans sa représentation existante.</param>
    /// <param name="shortHelp">Clé de ressource facultative de l’aide courte.</param>
    /// <param name="detailedHelp">Clé de ressource facultative de l’aide détaillée.</param>
    /// <param name="refreshSettingsOnChange">True pour actualiser les réglages après modification de la valeur.</param>
    /// <returns>Résultat du constructeur ou format existant, avec les mêmes valeurs et ressources.</returns>
    EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block, string label, bool value, string? shortHelp, string? detailedHelp, bool refreshSettingsOnChange = false);
}
