namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Description immuable d’un contenu monté, conservée même après son éjection.</summary>
/// <param name="Path">Chemin du fichier ou dossier utilisateur ; le record ne l’ouvre pas et ne le possède pas.</param>
/// <param name="Category">Nature du contenu, distincte de son lecteur et de son format de cartouche.</param>
/// <param name="Slot">Identité unique du lecteur ; cet emplacement reste inchangé lors d’un remplacement de contenu.</param>
/// <param name="Label">Libellé facultatif de présentation, indépendant du chemin.</param>
/// <param name="IsReadOnly">Protection du contenu demandée au backend ; les copies de session restent sous sa responsabilité.</param>
/// <param name="IsInserted">False conserve les réglages d’un contenu éjecté sans demander son ouverture.</param>
/// <param name="CassetteAutoBoot">Demande l’impulsion de démarrage déjà prévue par le transport de cassette.</param>
/// <param name="StorageBus">Interface de stockage sélectionnée, ou null pour la décision du profil ou du backend.</param>
/// <param name="MountPoint">Point de montage logique facultatif, par exemple une lettre de volume ; ce texte n’est pas un chemin hôte.</param>
/// <param name="MountOrder">Ordre d’affectation des contenus, distinct de l’identité Slot ; zéro est la valeur initiale.</param>
/// <param name="CartridgePlatform">Variante de format de cartouche ; null laisse sa résolution au profil ou à la reconnaissance du contenu.</param>
/// <param name="CartridgeType">Code du format de cartouche reconnu par le backend ; null représente une détection automatique.</param>
/// <param name="CartridgeRegion">Standard vidéo associé à la cartouche ; null représente le choix automatique.</param>
/// <remarks>Les paramètres optionnels réunissent les descriptions existantes. Ils traversent le document de configuration
/// et les remplacements effectués avec with sans créer de dictionnaire de montages parallèle. Le backend décide
/// comment employer les paramètres applicables au contenu qu’il reçoit. La région réutilise EmulationVideoStandard,
/// dont les valeurs NTSC, PAL et SECAM existent déjà ; aucune seconde énumération de région n’est nécessaire.
/// Les sept premiers paramètres conservent leur ordre, notamment CassetteAutoBoot, pour les appels de l’exemple.</remarks>
/// <example><code>
/// var disk = new MediaConfiguration("disque.img", MediaCategory.HardDisk, EmulationMediaSlot.HardDisk0,
///     StorageBus: StorageBus.Ide, MountPoint: "C", MountOrder: 0);
/// var cartridge = new MediaConfiguration("cartouche.rom", MediaCategory.Cartridge, EmulationMediaSlot.Cartridge0,
///     CartridgePlatform: CartridgePlatform.Standard, CartridgeType: 1, CartridgeRegion: EmulationVideoStandard.Pal);
/// </code></example>
public sealed record MediaConfiguration(string Path, MediaCategory Category, EmulationMediaSlot Slot, string? Label = null, bool IsReadOnly = false, bool IsInserted = true, bool CassetteAutoBoot = false,
    StorageBus? StorageBus = null, string? MountPoint = null, int MountOrder = 0, CartridgePlatform? CartridgePlatform = null, int? CartridgeType = null, EmulationVideoStandard? CartridgeRegion = null);

/// <summary>Compatibilité d’un profil avec un backend, projetée depuis ses descriptions matérielles et ses réglages.</summary>
/// <param name="Model">Identité du profil matériel, distincte du backend.</param>
/// <param name="Core">Backend auquel cette description s’applique.</param>
/// <param name="VisibleTabs">Onglets décrits par le contrat SDK existant, sans énumération locale concurrente.</param>
/// <param name="Firmware">Rôles de firmware acceptés par le profil.</param>
/// <param name="Media">Règles associant chaque catégorie de contenu à ses lecteurs.</param>
/// <param name="ControllerPortCount">Nombre de ports de jeu du profil, distinct du dernier indice utilisable.</param>
/// <remarks>Cette projection ne possède ni cœur, ni fichier. Son calcul utilise les catalogues existants
/// afin de conserver une seule déclaration des machines et de leurs lecteurs.</remarks>
public sealed record CompatibilityDefinition(MachineModel Model, Emulator Core, IReadOnlyList<EmulationMachineTab> VisibleTabs, IReadOnlyList<FirmwareCategory> Firmware, IReadOnlyList<MediaCompatibilityRule> Media, int ControllerPortCount);

/// <summary>Association d’une catégorie de contenu aux emplacements autorisés par un profil.</summary>
/// <param name="Category">Catégorie contrôlée avant le montage.</param>
/// <param name="Slots">Emplacements logiques acceptant ce contenu, indépendants de son ordre de montage.</param>
/// <param name="Availability">Disponibilité effective de cette règle pour le profil et le backend.</param>
/// <param name="ExplanationResourceKey">Clé de traduction facultative expliquant une indisponibilité ; aucune traduction n’est stockée dans la règle.</param>
/// <remarks>La règle réutilise les identités de lecteurs du SDK ; elle n’ouvre pas les contenus qu’elle décrit.</remarks>
public sealed record MediaCompatibilityRule(MediaCategory Category, IReadOnlyList<EmulationMediaSlot> Slots, MediaAvailability Availability = MediaAvailability.Available, string? ExplanationResourceKey = null);