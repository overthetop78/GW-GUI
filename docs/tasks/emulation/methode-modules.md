# Méthode de réalisation des modules

Règle de présentation pour TOUS les fichiers, sans exception de format : ne jamais couper artificiellement une même instruction ou entrée sur plusieurs lignes physiques. Garder notamment les attributs XML sur la ligne de leur balise, les entrées RESX complètes sur une ligne et les lignes de tableau entières. Appliquer cette règle dès la création, la modification et dans les générateurs ; conserver les sauts structurels séparant des instructions ou des blocs distincts.

Ce document est la procédure réutilisable pour fabriquer chaque module correctement et plus vite. Il contient les conventions, les fichiers à produire, leurs responsabilités, les dépendances et les contrôles qui évitent les reprises. Les capacités, noms de modèles, options, incidents et résultats d'un module appartiennent à son plan et à ses preuves, pas ici.

Corriger directement la directive concernée après une erreur ou une amélioration : remplacer la mauvaise démarche et supprimer ce qui ne sert plus. L'historique indique seulement quelle partie de la méthode a été améliorée et pour quel gain. Il ne raconte pas les travaux effectués sur un module. Appliquer la directive corrigée dès l'action suivante et à tous les modules suivants. Chaque correction demandée, erreur évitable, lecture inutile ou démarche trop lente doit modifier ici la procédure qui en est responsable : indiquer directement la bonne action, les fichiers concernés, leurs dépendances et le contrôle qui évite de recommencer. Ne pas se contenter d'une ligne historique.

## 1. Reprendre le travail sans recommencer

1. Vérifier l'état courant du dépôt et du processus éventuellement en cours ; reprendre le premier point non terminé du plan du module. Une observation sans résultat ne prouve pas l'arrêt d'un processus. Conserver son handle et attendre son résultat, sans lancer une deuxième exécution.
2. Relever l'heure locale du PC avec `Get-Date -Format 'yyyy-MM-dd HH:mm:ss'` et ajouter une ligne au chronomètre à chaque reprise de l'objectif. Conserver le départ et les interruptions autorisées ; écrire le dépassement comme une durée négative `-hh:mm:ss`. Les timestamps restent les heures réelles du PC.
3. Utiliser les chemins, contrats, sources et preuves déjà relevés. Ne pas relire tout le projet, redécouvrir un fichier connu, refaire un contrôle acquis sans changement concerné, ni poser une question dont les logs donnent la réponse.
4. Si une erreur se répète, corriger d'abord sa cause dans la procédure ou le générateur, puis les fichiers déjà produits par ce mécanisme. Ne pas poursuivre une série de copies incorrectes.

## 2. Préparer un plan concret depuis les références

Le plan est propre à chaque module. Il suit cet ordre : périmètre et contrats → projet et données → services et adaptateurs → description et application des réglages → ressources et visuels → intégration → vérification → nettoyage → commit. Chaque feuille indique une création, modification, suppression, copie, déplacement ou renommage de fichier, son chemin exact et son contenu attendu. Ajouter tout travail manquant avant de l'exécuter ; cocher après réalisation seulement. Une lecture ou la mise à jour du plan ne constitue pas une tâche autonome de fabrication.

1. Parcourir toutes les sections de la liste fournie afin de couvrir toutes les machines et tous les moteurs demandés, même classés sous une autre marque. Ne pas ajouter de matériel, accessoires ou fonctions par supposition.
2. Utiliser le catalogue de fabrication et les conventions consignés ci-dessous, déjà relevés dans les références. Ne pas relire les anciens modules à chaque nouvelle marque. Pour un besoin réellement absent de cette procédure, consulter une fois le composant manquant, relever son nom de fichier, son namespace, sa signature et ses dépendances, puis compléter ici la recette réutilisable avant de poursuivre. Un nom de classe ne donne pas forcément le nom du fichier.
3. Préparer les chemins du nouveau module depuis ce catalogue : fichiers, rôles, dépendances, identités à remplacer et adaptations natives nécessaires. Réutiliser le relevé ou générateur corrigé déjà disponible ; consulter uniquement les sources du nouveau moteur et les contrats qui ont effectivement changé. Ne pas refaire l'inventaire des mêmes services pour chaque module.
4. Relever les contrats du paquet réellement distribué et de ses sources officielles : identité, architecture, machines compatibles, formats, firmware, ports, commandes, options, valeurs, défauts, sorties et états. Utiliser les exports SDK existants si une vérification du binaire est nécessaire ; ne pas créer une machine pour une simple lecture de métadonnées.
5. Définir avant génération les champs utiles et les preuves nécessaires. Une option du moteur n'a pas automatiquement sa place dans l'interface. Exclure les dossiers par défaut, doublons des fonctions communes, catalogues de jeux et métadonnées sans utilité demandée.
6. Prévoir les contrôles temporaires et leur suppression dans ce même plan. Réaliser d'abord un parcours complet représentatif, puis ajouter les différences attestées des autres variantes.

## 3. Fichiers, noms et responsabilités

`<Marque>`, `<id>`, `<Cœur>` et `<Famille>` sont des paramètres à remplacer. Les noms exacts déjà différents entre références restent ceux de la référence choisie ; ne pas les uniformiser arbitrairement. Le plan du module fixe les chemins réels et ne recrée pas un fichier déjà réalisé dans une sous-tâche.

| Fichier ou emplacement | Travail à effectuer et responsabilité |
|---|---|
| `src/GWGUI.Emulation.<Marque>/GWGUI.Emulation.<Marque>.csproj` | Reprendre cible, architecture, nom d'assembly `gwgui.emulation.<id>`, références nécessaires, incorporation des ressources, copie du manifeste et exclusion des assemblies fournies par l'hôte. Aucun lien de ressources vers un autre module. |
| `module.json` | Définir l'identité stable en minuscules, `entryAssembly`, version, compatibilité d'API et catalogue de mise à jour selon le schéma existant. `entryAssembly` doit correspondre exactement à la DLL produite. |
| `EmulationGlobalUsings.cs` | Importer les namespaces SDK et locaux réellement consommés. Retirer les imports étrangers. Chaque fichier C# utilisant une API IO comporte explicitement `using System.IO;`. |
| `Modules/<Marque>EmulationModuleFactory.cs` | Fournir l'unique factory publique `IEmulationModuleFactory`, constructible sans argument. Son identité correspond au manifeste ; elle reçoit chemins et services depuis `EmulationModuleContext`. |
| `Modules/<Marque>EmulationModule.cs` | Implémenter `IEmulationModule` et les gestionnaires SDK nécessaires. Déléguer description, configuration, persistance, choix du moteur, entrées, stockage, firmware et runtime aux composants concernés. |
| `Common/Constants/EmulationModuleConstants.cs` | Centraliser identité, préfixes de ressources et noms contractuels du module selon la référence. Ne pas y déplacer les constantes du protocole d'un moteur. |
| `Common/Contracts/EmulatorContracts.cs` et contrats de rôle existants | Reprendre les contextes et données génériques de création et gestion. Recevoir les services de l'hôte ; ne pas reconstruire leurs équivalents dans le module. |
| `Common/Services/Engine.cs` | Choisir l'adaptateur depuis la configuration courante et déléguer. Aucun chargement ou branche de protocole propre à un moteur. |
| `Common/Services/ConfigurationStore.cs` | Reprendre la persistance atomique, les chemins portables, la validation du schéma et le nettoyage du fichier temporaire. |
| `Common/Machines/Common/Enums/*Enums.cs` | Définir les ensembles fermés réellement utilisés : modèles, familles, moteurs, périphériques, firmware et médias. Garder les fichiers de rôle existants et les identités persistées stables. |
| `Common/Machines/Common/Contracts/*Contracts.cs` | Définir configuration, entrées, supports, matériel et firmware ; retirer les propriétés étrangères au nouveau périmètre. Séparer données persistées et ressources natives vivantes. |
| `Common/Machines/Common/Dictionaries/ModelCatalog.cs`, `MachineCatalog.cs` et catalogues de rôle | Associer les modèles à leurs identités, ressources, matériel, capacités et compatibilités. Utiliser les catalogues typés existants plutôt que des listes ou registres parallèles. |
| `Common/Machines/Common/Constants`, `Dictionaries`, `Functions` | Ranger les constantes, données et opérations auprès de leur rôle : validation, description, conversions, firmware, stockage, entrées et runtime. Conserver les fichiers partiels de la référence, notamment les fonctions de configuration et d'entrée. |
| `Common/Machines/<Famille>/...` | Ajouter seulement un comportement réellement particulier à une famille. Une simple différence de données ne justifie pas un nouvel arbre de classes par modèle. |
| `Emulators/Common/Interfaces/IEmulatorCore.cs` et contrat générique existant | Porter les commandes communes vers la machine et le SDK : démarrage, entrées, supports, états, audio/vidéo et arrêt. Aucun export, callback, masque ou type propre à une ABI dans ces signatures. |
| `Emulators/Common/Factories/...` | Construire la machine à partir d'un cœur abstrait. Les capacités particulières et décisions de remplacement des supports viennent de l'adaptateur, jamais d'un test du nom de moteur dans la machine commune. |
| `Emulators/<Cœur>/Constants`, `Dictionaries`, `Factories` | Définir identité native, versions, options exactes et factory concrète du moteur. Ne proposer ce moteur qu'aux modèles pris en charge. Vérifier son raccordement au contrat générique avant de mutualiser. |
| `Emulators/Common/Interop/...` et adaptations du moteur | Réutiliser le transport natif réellement partagé : exports, callbacks, buffers, chargement et conversion des entrées. Le partage d'une ABI ne définit pas le contrat commun des moteurs ; une autre DLL doit pouvoir implémenter le même contrat générique. |
| Services de buffers, transport, processus, audio et vidéo existants | Reprendre seulement les fichiers utilisés et leurs dépendances complètes. Préserver leur gestion de propriété et de fermeture ; ne pas copier de helpers sans appelant. |
| `Resources/00-Base/*.resx`, `Resources/<culture>/*.resx` | Garder les catalogues et noms logiques de la référence. Base pour les invariants et le secours anglais ; cultures pour les traductions. Réutiliser les clés communes avant toute création locale. |
| `Assets/<id>.png`, `Assets/Machines/*.png` | Incorporer le vrai logo de marque et les photographies de machines consommées par les définitions. Conserver sources, licences et documents de travail hors Assets. |

### Recette du projet et du manifeste

Le projet de fabrication actuellement relevé cible `net10.0`, active `ImplicitUsings` et `Nullable`, fixe `Platforms` et `PlatformTarget` à `x64`, produit `gwgui.emulation.<id>` et référence `../GWGUI.Emulation/GWGUI.Emulation.csproj`. Ajouter une autre bibliothèque uniquement lorsqu'un consommateur en a besoin. Les accès internes des contrôles utilisent `InternalsVisibleTo`, pas une copie des types SDK.

Copier `module.json` à la sortie et à la publication avec `PreserveNewest`. Incorporer `Assets/*.png` sous `GWGUI.Emulation.<Marque>.Assets.%(Filename)%(Extension)` et `Assets/Machines/*.png` sous `GWGUI.Emulation.<Marque>.Assets.Machines.%(Filename)%(Extension)`. Les RESX utilisent `WithCulture=false` et le nom logique `GWGUI.Emulation.<Marque>.Resources.<catalogue>.<culture>.resources`. Reprendre le target `RemoveHostProvidedAssembliesFromCopyLocal` après `ResolveAssemblyReferences` pour retirer les références copiées dont le filename commence par `gwgui.`.

Le manifeste emploie `schemaVersion: 2`, `id: <id>`, `entryAssembly: gwgui.emulation.<id>.dll`, `moduleVersion: 1.0.0` à la création, `hostApiMinimum: 1.0`, `hostApiMaximum: 1.0` et `updateCatalogUrl: https://github.com/overthetop78/GW-GUI/releases/download/module-<id>-catalog/update-catalog.json`. Ces versions sont la base actuelle, à modifier seulement lorsqu'un changement réel de schéma ou d'API l'exige.

La factory de module a une propriété `Id`, reçoit `EmulationModuleContext` dans `Create`, et transmet `ModuleDirectory`, `DataDirectory` et le `HttpClient` emprunté aux services existants. Les chemins de configurations, firmware et moteurs utilisent leurs constantes de rôle. Ne pas créer un client HTTP supplémentaire et ne pas détruire celui de l'hôte.

### Utiliser et améliorer cette recette

À chaque nouvelle marque, remplacer les paramètres de noms, ajouter uniquement ses données et protocoles attestés, puis suivre l'ordre des dépendances. Ne pas recopier des branches étrangères avant de les corriger. Quand une correction révèle un nom, contrat ou dépendance manquant, compléter la ligne de catalogue correspondante avec l'information exacte : le module suivant doit pouvoir l'utiliser sans retrouver cette information dans les anciens modules. Remplacer toute procédure plus lente par la démarche éprouvée plus rapide ; supprimer les lectures et contrôles devenus redondants. L'historique n'est pas un journal d'exécution.

### Catalogue concret à utiliser pour préparer le module suivant

Les chemins ci-dessous sont relatifs à `src/GWGUI.Emulation.<Marque>/`. Les listes entre accolades désignent plusieurs fichiers, pas un nouveau fichier portant ce nom. Ce catalogue donne les noms déjà relevés et l'ordre de raccordement ; il ne prescrit pas de créer tous les fichiers. Retenir seulement les composants consommés par les capacités demandées et leurs dépendances. Une fonctionnalité absente ne reçoit ni fichier, ni champ, ni ressource pour compléter artificiellement l'arbre.

| Ordre et emplacement | Fichiers connus | Contenu à adapter et raccordement à conserver |
|---|---|---|
| 1. `Common/Constants/` | `EmulationModuleConstants.cs`, `ConfigurationStoreConstants.cs`, `CoreDirectoryConstants.cs`, `ErrorContextConstants.cs` | Identité, schéma, noms de répertoires internes et contexte d'erreurs ; consommateurs : factory de module, persistance et installation. Les répertoires internes ne deviennent pas des choix de dossiers dans l'interface. |
| 2. `Common/Machines/Common/Enums/` | `MachineEnums.cs`, `CoreEnums.cs`, `ControllerEnums.cs`, `FirmwareEnums.cs`, `MediaEnums.cs`, `RuntimeEnums.cs` | Ensembles attestés, identités persistées ; retirer les membres étrangers avant de produire les catalogues. Ne pas remplacer les enums SDK déjà adaptées au concept. |
| 3. `Common/Machines/Common/Contracts/` | `ConfigurationContracts.cs`, `MachineContracts.cs`, `HardwareContracts.cs`, `ControllerContracts.cs`, `InputContracts.cs`, `FirmwareContracts.cs`, `MediaContracts.cs`, `StateContracts.cs` | Données de configuration et de description, entrées sémantiques, supports et états. Ne conserver que les propriétés réellement utilisées ; aucun type ou handle d'ABI dans ces données. |
| 4. `Common/Contracts/` | `EmulatorContracts.cs`, `CoreContracts.cs` | Contextes et contrats utilisés par les services du module et le SDK ; conserver les objets empruntés et la configuration opaque. Vérifier les signatures présentes plutôt que recréer un contrat parallèle. |
| 5. `Common/Machines/Common/Constants/` | `MachineConstants.cs`, `HardwareConstants.cs`, `InputMappingConstants.cs`, `SettingsConstants.cs`, `VideoAudioSettingsConstants.cs`, `StorageSettingsConstants.cs`, `RuntimeConstants.cs`, `StateConstants.cs` ; `FirmwareConstants.cs` si nécessaire | Bornes et identités propres aux données de machine. Les codes du protocole natif restent dans l'adaptateur. Ne pas ajouter de touches, accessoires, formats ou valeurs par analogie. |
| 6. `Common/Machines/Common/Dictionaries/` | `ModelCatalog.cs`, `MachineCatalog.cs`, `HardwareModelCatalog.cs`, `ControllerProtocolDictionary.cs`, `ControllerVisualDictionary.cs` | Modèle → famille et matériel ; modèle → moteurs compatibles ; commande sémantique → identité ; périphérique → visuel App. Conserver les modèles demandés sans leur attribuer un moteur incompatible. |
| 7. `Common/Machines/Common/Functions/` | `ConfigurationFunctions.cs`, `ConfigurationFunctions.Persistence.cs`, `ConfigurationFunctions.Summary.cs`, `MachineFunctions.cs`, `CoreOptionValidationFunctions.cs` | Validation complète → persistance portable → résumé et descriptions ; utiliser les catalogues précédents. Préserver les champs absents lors d'une modification partielle. |
| 8. Même répertoire | `SettingsFunctions.cs`, `FirmwareFunctions.cs`, `StorageFunctions.Settings.cs`, `MediaFunctions.Conversion.cs`, `MediaFunctions.Runtime.cs`, `RuntimeFunctions.cs`, `StateFunctions.Saved.cs` | Champs SDK utiles, firmware et supports structurés ; conversion et exécution via le contrat de moteur. Ranger les options du support dans sa configuration. Les capacités viennent de l'adaptateur, sans test du nom du moteur dans la machine commune. |
| 9. Même répertoire | `InputFunctions.Settings.cs`, `InputFunctions.Snapshot.cs`, `EmulationPeripheralConversionFunctions.cs` | Édition → persistance → snapshot sémantique ; associations de manettes initialement vides. Les commandes clavier communes gardent leur identité SDK ; ajouter seulement les touches spéciales nécessaires et modifiables. |
| 10. `Emulators/Common/Interfaces/` et `Contracts/` | `IEmulatorCore.cs`, `ICoreReleaseService.cs` ; `CoreContracts.cs`, `RuntimeContracts.cs`, `MediaContracts.cs`, `ControllerContracts.cs` | Commandes communes indépendantes du backend. Ajouter une autre ABI en implémentant ce contrat, sans faire remonter ses callbacks ou constantes dans la machine ou l'App. |
| 11. `Emulators/Common/` | `Factories/MachineFactory.cs`, `Dictionaries/CoreCatalog.cs`, `Enums/CoreEnums.cs`, `Enums/RuntimeEnums.cs` ; `Functions/CoreFunctions.Catalog.cs`, `CoreFunctions.Host.cs`, `CoreFunctions.Release.cs`, `RuntimeFunctions.Options.cs`, `InputFunctions.cs`, `ControllerFunctions.cs`, `AudioFunctions.cs`, `StateFunctions.cs` | Sélection d'adaptateur, création, commandes et données communes. Mutualiser seulement après avoir relevé ce que les adaptateurs partagent réellement ; ne pas assimiler le contrat commun à une ABI. |
| 12. `Emulators/<Cœur>/` | `Constants/EmulatorConstants.cs`, `Dictionaries/OptionCatalog.cs`, `Factories/<Cœur>MachineFactory.cs` | Identité du paquet, DLL, source, description, modèles compatibles, options exactes et factory concrète. La factory fournit les capacités et l'implémentation du contrat commun ; adapter les différences avant raccordement. |
| 13. `Emulators/Common/Interop/`, seulement pour le transport partagé concerné | `Factories/LibretroMachineFactory.cs`, `Contracts/ExternalCoreContracts.cs` ; `Services/CoreProvider.cs`, `CoreReleaseService.cs`, `ExternalCore.cs`, `ExternalCore.Content.cs`, `ExternalCore.Media.cs`, `ExternalCore.State.cs`, `LoadedContent.cs`, `ContentPath.cs` | Installation et bibliothèque native → préparation du contenu → chargement, états et libération. Ces fichiers appartiennent au transport concerné ; un moteur utilisant une autre ABI n'en dépend pas. Adapter le chargement et les limites du nouveau lecteur, sans conserver les branches du moteur précédent. |
| 14. Même répertoire | `Services/ExternalHostCallbacks.cs`, `ExternalHostCallbacks.Environment.cs`, `ExternalHostCallbacks.EnvironmentDetails.cs`, `ExternalHostCallbacks.AudioVideoInput.cs`, `CoreOptionHost.cs`, `KeyboardState.cs`, `FrameTimer.cs` | Callbacks natifs, options et sorties ; durée de vie sous le propriétaire du cœur. Raccorder les options relevées et les formats de sortie réellement produits ; détacher les callbacks avant libération. |
| 15. Même répertoire | `Functions/CoreFunctions.cs`, `CoreFunctions.Lifecycle.cs`, `CoreFunctions.Options.cs`, `CoreFunctions.OptionProbe.cs`, `CoreFunctions.ExternalProbe.cs`, `CoreFunctions.Diagnostics.cs`, `RuntimeFunctions.Environment.cs`, `NativeInputFunctions.cs`, `NativeAudioFunctions.cs`, `VideoFunctions.cs`, `KeyboardFunctions.cs`, `ControllerPortFunctions.cs`, `FirmwareRuntimeFunctions.cs`, `MediaFunctions.Content.cs` | Conversions et appels d'ABI exclusivement ici. Adapter commandes et lectures de tous les ports depuis les sources natives. Vérifier tailles, unités et bornes avant appel ; ne pas transmettre implicitement les entrées hôtes sans association. |
| 16. Composants facultatifs | `Interop/Services/DiskControl.cs`, `Interop/Functions/MediaFunctions.Disk.cs`, constantes et erreurs associées ; services particuliers du matériel uniquement si nécessaires | Créer seulement pour une capacité réellement utilisée. Ne pas recopier des services de voix, disques ou accessoires sans appelant ; conserver les interfaces communes requises en signalant correctement l'indisponibilité de l'opération. |
| 17. `Common/Services/` | `ConfigurationStore.cs`, `Engine.cs`, `Machine.cs`, `Machine.Commands.cs`, `Machine.Lifecycle.cs`, `InputFrameStore.cs` | Persistance → sélection → machine → file de commandes → cycle de vie. Inspecter aussi les conditions et helpers copiés : des namespaces corrigés ne suppriment pas un comportement étranger. |
| 18. Même répertoire, selon l'hébergement retenu | `CoreHost.cs`, `ProcessCore.cs`, `ProcessCore.Host.cs`, `ProcessCore.Protocol.cs`, `SharedVideoWriter.cs`, `VideoBufferSet.cs`, `AudioBuffer.cs`, `AudioOutputController.cs` | Réutiliser le transport, les buffers et sorties éprouvés avec leurs contrats ; conserver arrêt, finally et attente de destruction. Ne pas fabriquer un second mécanisme d'hébergement. |
| 19. `Modules/` | `<Marque>EmulationModule.cs`, `<Marque>EmulationModuleFactory.cs` | Déléguer aux composants réalisés ; vérifier identité du manifeste et parcours SDK de chaque capacité. Aucun réglage, registre ou cast spécifique à la marque ajouté dans l'App. |
| 20. Ressources, visuels et intégration | Chemins des sections 7 et 8 | Extraire les clés réellement consommées après le tri, réutiliser les visuels App, puis raccorder solution et build. Terminer par le parcours livré, le nettoyage et le commit de la section 9. |

Pour les répertoires de constantes et d'erreurs du transport, reprendre les fichiers utilisés par les composants retenus, pas tout le répertoire. Le relevé existant contient notamment `CoreHostConstants.cs`, `CoreHostFunctionsConstants.cs`, `CoreHostValues.cs`, `ProcessCoreConstants.cs`, `CoreReleaseConstants.cs`, `EmulatorCatalogConstants.cs`, ainsi que les constantes d'Interop `ExternalCoreInteropConstants.cs`, `CoreLifecycleConstants.cs`, `CoreOptionConstants.cs`, `CoreOptionProbeConstants.cs`, `CoreOptionProbeValues.cs`, `EnvironmentConstants.cs`, `EnvironmentFunctionsConstants.cs`, `InputConstants.cs`, `KeyboardConstants.cs`, `MediaRuntimeConstants.cs` et `DiskControlConstants.cs`. Les valeurs d'un nouveau protocole se relèvent dans ce protocole, jamais dans une autre machine.

Préparer une seule carte de fabrication réutilisable : chemin source connu → chemin destination paramétré → rôle → dépendances → identités à remplacer → adaptation nécessaire ou exclusion. Ne pas se contenter d'une consigne identique « remplacer la marque » pour tous les fichiers. Partir de cette carte corrigée pour le module suivant ; lire les fichiers source retenus pour leur reprise, sans refaire l'inventaire ni rechercher à nouveau comment les références sont organisées. Si la source a changé depuis le relevé, actualiser uniquement l'entrée concernée avant de l'utiliser.

Après chaque correction, identifier l'étape qui l'aurait évitée et modifier cette étape immédiatement. Par exemple : une copie inutile corrige la sélection des consommateurs ; une clé brute corrige le relevé et la résolution des ressources ; un mauvais visuel corrige le propriétaire et le type de l'asset ; un ancien comportement corrige la liste des adaptations avant copie. Écrire la marche à suivre et le contrôle concret, puis ajouter une courte ligne datée indiquant l'amélioration et le travail évité. Ne pas conserver l'incident particulier comme directive destinée aux autres marques.

### Frontières SDK, module et App

- **SDK** : interfaces, descriptions, slots, entrées, configurations opaques et services partagés existants. Réutiliser leurs types et signatures ; ne pas recopier leurs enums ou records dans le module.
- **Module** : configuration typée, modèles, catalogues, validations et adaptations natives de sa marque. Le module référence le SDK ; le SDK ne référence pas le projet du module.
- **App** : manipule les interfaces et descriptions SDK, sans conversion vers une configuration de marque ni branche propre à un module. La découverte utilise le manifeste et la factory ; ne pas ajouter de registre de marques à côté.
- **Contrôleurs App** : photos dans `src/GWGUI.App/Assets/Controllers/`, profils dans le catalogue de visualisation existant. Réutiliser les photos et profils déjà présents. Aucune deuxième copie dans le module et aucun `Resource Link` vers ses assets.

### Enums, constantes et catalogues

Utiliser une enum pour une catégorie fermée interprétée par le code, une constante pour un identifiant contractuel, une borne, une taille ou une valeur de protocole, et un catalogue typé pour les associations entre ces données. Réutiliser les types SDK représentant déjà le concept. Garder chaque donnée chez son propriétaire ; les valeurs natives appartiennent à leur adaptation. Un nom technique n'est pas un libellé traduit. Préserver les valeurs persistées lorsqu'une liste évolue.

## 4. Réaliser les dépendances avant leurs consommateurs

1. Créer projet, manifeste, identité et imports ; puis enums, contrats, constantes et catalogues. Retirer immédiatement les modèles, propriétés et dépendances étrangers repris avec la référence.
2. Reprendre la configuration et sa persistance : identité non vide, schéma, validation, copie des collections, chemins portables et écriture atomique. Conserver séparément périphériques configurés et médias montés ; un périphérique vide doit survivre à la sauvegarde.
3. Reprendre les services génériques sélectionnés et toutes leurs dépendances utiles. Contrôler les prédicats, conversions et branches, pas seulement les namespaces. Une substitution de marque ne remplace pas l'adaptation d'un protocole.
4. Créer les factories concrètes et leurs adaptations ; relier ensuite la factory commune et la machine. L'installation et l'hôte natif restent dans l'adaptateur du backend. Le choix courant du moteur doit traverser description, installation, recherche, lancement et rechargement sans revenir au défaut.
5. Relier les méthodes du module aux mêmes contrats. Ne pas annoncer une capacité que l'application ne peut pas effectivement sélectionner, enregistrer et appliquer.
6. Vérifier les fichiers produits avant la génération des variantes et des langues : symboles étrangers, dépendances manquantes, imports, signatures SDK et remplacements réellement effectués. Corriger la source de génération une fois au lieu de réparer chaque copie.

## 5. Entrées, paramètres, firmware et supports

### Entrées

- Réutiliser les identités communes du clavier physique et virtuel. L'éditeur n'ajoute que les touches de machine absentes du clavier hôte ; leurs affectations par défaut restent modifiables, persistées et appliquées.
- Les associations des manettes commencent toutes vides, y compris boutons, touches, souris et axes. Sans affectation utilisateur, aucune transmission implicite des commandes physiques. Conserver les associations déjà enregistrées.
- Séparer commande émulée, source physique et transport natif. Les snapshots communs restent indépendants du backend ; la conversion en indices, codes et masques natifs appartient à Interop.
- Relever les commandes et lectures de tous les ports, les axes, leurs unités et les périphériques réellement présents. Appliquer normalisation et zone morte une fois. Une commande intégrée obligatoire ne propose pas un périphérique absent.
- Après réaffectation, l'ancien défaut ne doit plus produire la commande. Vérifier le parcours éditeur → configuration → sauvegarde → snapshot → entrée native, avec transitions de pression et relâchement.

### Paramètres

- Raccorder chaque champ à sa donnée et à son effet ; préserver les valeurs absentes lors d'une application partielle. Valider la combinaison complète avant ses effets et empêcher un éditeur ancien d'écraser une autre configuration.
- Classer selon la fonction réelle : processeur, mémoire, vidéo, audio, contrôleurs ou support. Les options d'un périphérique vont dans sa configuration, pas dans un cadre global supplémentaire.
- Préserver les blocs communs et leur emplacement. Ajouter les options natives audio à gauche du bloc commun à droite ; ne pas déplacer le traitement vidéo commun ou doubler les shaders. Adapter les colonnes à la place et à la lisibilité, sans copie mécanique de la disposition.
- Ne pas afficher de choix de dossiers par défaut, de suffixes « redémarrage requis » ou de clé de ressource brute. Préserver les métadonnées internes du cycle de vie. Les données fixes sont des informations, les vrais choix des éditeurs ; fréquences et tailles utilisent des unités lisibles.
- Donner des identifiants uniques aux blocs, résoudre leurs libellés et borner les aides. Vérifier le rendu de la vraie fenêtre, pas seulement la description produite.

### Firmware et supports

- Relever les fichiers nécessaires, noms exacts, tailles, signatures, membres d'archive et exigences obligatoires ou facultatives. Exposer leur sélection par les contrats existants, conserver leur persistance et préparer leurs copies sous la session.
- Utiliser le vrai type de stockage accepté : image, dossier ou mémoire interne. Ne pas inventer une carte, un lecteur, une extension ou une image incompatible par ressemblance avec une autre machine.
- Vérifier les limites et unités du lecteur natif ; refuser les données tronquées ou trop grandes avant appel lorsqu'il ne les valide pas. Préparer les fichiers compagnons et les emplacements vides selon son vrai contrat.
- Vérifier insertion, remplacement, éjection et sauvegarde. Si une opération native ne fait pas le travail, utiliser le mécanisme existant de recréation ou déclarer la capacité indisponible ; ne pas simuler une réussite.
- Les copies modifiables et extractions restent sous la session autorisée. Ne pas modifier ni supprimer les médias sources ou données personnelles pour les besoins d'un contrôle.

## 6. Cycle de vie commun à tout le code produit

Identifier le propriétaire de chaque cœur, DLL, processus, thread, fenêtre, surface, transport, buffer et sortie audio. Nettoyer dans `finally`, y compris une acquisition partielle, une erreur, une annulation ou une interruption. Une erreur de libération ne doit pas empêcher les suivantes. Un objet emprunté reste sous la responsabilité de son appelant.

Arrêter les commandes, détacher callbacks et surfaces, fermer transports et sorties, libérer buffers et handles, arrêter les threads et processus puis attendre leur destruction effective. Prévoir la terminaison de secours existante si l'arrêt normal échoue. Supprimer les fichiers temporaires après fermeture de leurs propriétaires. La même exigence s'applique aux scripts et contrôles ; le seul appel de déinitialisation ne prouve pas l'absence de résidus.

## 7. Ressources et visuels sans reprises inutiles

1. Achever le tri des champs et extraire les clés de leurs consommateurs avant traduction. Réutiliser les clés et traductions communes déjà validées ; ne pas générer les ressources d'options inutilisées.
2. Conserver noms matériels, acronymes, formats, unités et valeurs invariantes uniquement en base. Garder les valeurs natives intactes ; les libellés et aides sont séparés.
3. Relire et contextualiser la source anglaise avant traduction, surtout les mots courts ambigus. Utiliser Argos et le script existants dans toutes les cultures présentes, sans revérifier son installation. Traduire, normaliser, réparer puis auditer séparément ; retraduire seulement les textes modifiés.
4. Modifier les entrées XML par clé, sans doublon ; préserver `xml:space`, paramètres et caractères protégés. Écrire immédiatement le format existant : déclaration `<?xml version="1.0" encoding="utf-8"?>`, chaque `resheader` et chaque `data` complets sur une ligne physique, attribut `name` en premier, aucune ligne vide. Appeler `format_resx_data_entries` du script `scripts/tools/translate-resx-argos.py` dans le générateur après chaque écriture, plutôt que reprendre les catalogues après livraison ; encoder les retours à la ligne des valeurs en entités XML sans changer le texte résolu. Contrôler doublons avant couverture, puis résolution et sens des libellés dans les vraies vues. Avant une normalisation globale, comparer clés, attributs et valeurs XML avant/après et refuser toute perte de contenu.
5. Chercher d'abord les assets locaux et profils existants. Distinguer logo de marque, machine et périphérique : une console n'est ni un logo ni une manette. Référencer une image commune attestée sans multiplier les copies.
6. Fournir des photographies réelles, PNG avec alpha et dimension maximale de 512 pixels, sans redessiner le matériel. Reprendre le traitement éprouvé ; inspecter contour, boutons, inscriptions et fonds enfermés. Vérifier le fichier effectivement livré et son consommateur, pas seulement son extension.
7. Relier les zones assignables aux commandes physiques du périphérique dans le profil App existant. Garder provenance et limites provisoires hors Assets ; une limite connue ne bloque pas les autres travaux indépendants, mais reste à résoudre avant completion.

## 8. Fichiers d'intégration à vérifier

| Fichier ou propriétaire | Modification nécessaire |
|---|---|
| `GWGUI.sln` | Ajouter le projet, ses configurations de build et son rattachement selon les projets existants ; préserver le reste de la solution. |
| `src/GWGUI.Emulation/GWGUI.Emulation.csproj` | Ajouter seulement les accès internes nécessaires selon la convention existante, sans `ProjectReference` vers le module. |
| Contrats et constantes SDK existants | Modifier seulement si une véritable identité ou capacité commune manque ; préserver leurs consommateurs et identités persistées. |
| `src/GWGUI.Emulation/Constants/EmulationControllerVisualIds.cs` | Ajouter un identifiant visuel partagé seulement si absent et nécessaire au raccordement du profil App. |
| Catalogue et profils de `ControllerVisualization` dans App | Ajouter ou réutiliser le profil du vrai périphérique ; conserver l'image dans les assets locaux App. |
| `tests/GWGUI.Tests/GWGUI.Tests.csproj` | Ajouter la référence nécessaire aux contrôles utiles suivant la convention existante. Ne pas créer des tests qui répètent la génération. |
| `scripts/local-building.cmd` et workflow de module existant | Vérifier leur découverte automatique des projets et manifestes ; modifier uniquement un raccordement réellement manquant, sans nouvelle liste redondante de marques. |

## 9. Vérifier puis commiter chaque module

1. Avant de créer un contrôle temporaire, relever les identifiants, signatures et chemins exacts dans les contrats déjà utilisés par le module ; réutiliser ces références au lieu de deviner un nom SDK ou un chemin. Avant compilation, auditer imports IO, namespaces SDK, dépendances, identifiants étrangers et contrats copiés. Pour les essais natifs, découvrir d'abord les firmwares dans les supports locaux déjà accessibles avec une recherche de noms ciblée, puis vérifier leurs signatures ; solliciter un chemin seulement après cette recherche. Corriger en priorité les modifications récentes si une fonction auparavant opérationnelle échoue.
2. Compiler les nouveaux projets de contrôle avec restauration initiale ; `--no-restore` exige un fichier d'assets déjà présent. Vérifier immédiatement le code de sortie avant toute exécution ; ne pas lancer un vieux binaire après échec.
3. Pour la livraison Debug des modules, lancer directement `scripts/local-building.cmd --building=debug --modules=A` avec l'autorisation de restauration nécessaire. Attendre le résultat terminal ; ne pas modifier les entrées ni lancer un second build pendant l'exécution. Vérifier ensuite `build/Debug/GW GUI/gwgui.exe` et les DLL des manifestes demandés. `--modules=0` ne construit pas les modules.
4. Utiliser le paquet et le registre réellement découverts par l'App, ainsi que le binaire réellement téléchargé et installé. Une DLL reconstruite à part ou un objet de module instancié directement ne prouve pas ce parcours.
5. Contrôler les configurations sans moteur installé, les changements de machine et de moteur, tous les onglets utiles, les valeurs vides, la persistance, les firmware, médias, réaffectations, images et ressources. Inspecter aussi les traductions et la disposition rendues.
6. Vérifier démarrage, première image, entrées, audio, états, supports et arrêt selon les capacités annoncées. Les preuves couvrent la fonction demandée ; un contrôle de présence de fichier ou de métadonnées ne prouve pas son exécution.
7. Réutiliser les contrôles existants et ajouter seulement la preuve autonome manquante. Supprimer les contrôles temporaires et leurs artefacts après fermeture effective, sans supprimer les tests préexistants ; conserver les résultats utiles dans les preuves du module.
8. Examiner aussi le diff des nouveaux fichiers indexés, normaliser leur fin de fichier et vérifier les changements partagés. Commiter le module seulement après vérification complète, sans push sauf demande ; préparer immédiatement le suivant depuis cette méthode corrigée.

## 10. Exécuter les outils efficacement

- Découvrir avec `rg --files`, puis lire les chemins retournés dans l'appel suivant. Pour convertir un chemin en chemin relatif, vérifier son préfixe complet avant Substring ; ne pas supposer que tous les fichiers contiennent le même sous-répertoire. Filtrer `rg` avec un répertoire et `-g`, sans glob positionnel Windows ni chemin deviné. Limiter la sortie aux éléments utiles ; reprendre seulement la plage manquante si elle est tronquée.
- Regrouper les lectures indépendantes ; garder écritures et opérations dépendantes dans l'ordre du plan. Utiliser un répertoire de travail explicite et les versions réellement installées des outils.
- Arrêter un script à la première erreur avec `$ErrorActionPreference = 'Stop'` ; vérifier aussi immédiatement le code de sortie des commandes natives. Vérifier les occurrences avant un remplacement et son résultat immédiatement ; accepter LF et CRLF. Tester une génération sur une valeur avant de la multiplier.
- Avec PowerShell, typer les valeurs requises, initialiser explicitement les tableaux et distinguer une chaîne d'une liste : un résultat unique peut devenir scalaire. Parenthéser le texte de remplacement concaténé. Modifier les textes français par patch ou here-string : les apostrophes typographiques sont également interprétées comme délimiteurs dans une chaîne entre apostrophes ; les remplacer par une autre apostrophe ne corrige pas l'invocation. Après une erreur de parsing, corriger le mode de citation avant de réexécuter.
- Après une erreur, corriger d'abord l'invocation ; ne pas changer d'outil ou inventer un défaut préalable. Demander un outil réellement manquant au lieu de multiplier les contournements. Employer directement les autorisations nécessaires lorsqu'elles sont connues.
- Ne refaire un build ou un contrôle que pour un changement, un échec ou une incertitude précise. Le dépannage propre à un moteur reste dans ses preuves ; intégrer ici seulement la règle réutilisable qui évite de recommencer l'erreur.

## Historique des mises à jour

Chaque ligne décrit une amélioration de cette procédure, pas une réalisation ou un diagnostic de module. Conserver le numéro et l'heure locale réelle, une ligne physique par entrée, sans ligne vide dans le tableau.

| Mise à jour | Date et heure locale du PC | Amélioration de la méthode |
|---|---|---|
| 1 | 2026-10-09 23:14:47 | Corriger les directives en place et tracer les améliorations de procédure. |
| 2 | 2026-10-09 23:20:59 | Décrire les étapes de fabrication pour préparer un plan complet. |
| 3 | 2026-10-09 23:22:32 | Remplacer les exemples particuliers par une procédure réutilisable. |
| 4 | 2026-10-09 23:25:59 | Fixer l'ordre de traduction et d'audit pour éviter les reprises. |
| 5 | 2026-10-09 23:28:02 | Réutiliser les touches communes et limiter les nouvelles affectations. |
| 6 | 2026-10-09 23:30:03 | Formuler les règles communes comme des règles d'application. |
| 7 | 2026-10-09 23:33:12 | Vérifier le parcours de réaffectation complet avant généralisation. |
| 8 | 2026-10-09 23:40:39 | Retirer les propriétés étrangères avant de reprendre leurs dépendances. |
| 9 | 2026-10-09 23:48:29 | Éviter anciens aliases et copies d'images inutiles. |
| 10 | 2026-10-10 00:02:02 | Vérifier les signatures SDK et prévoir le nettoyage des contrôles. |
| 11 | 2026-10-10 00:05:04 | Réutiliser les données découvertes et préserver un build en cours. |
| 12 | 2026-10-10 00:06:52 | Employer l'heure locale sans modifier les durées écoulées. |
| 13 | 2026-10-10 00:08:22 | Simplifier le format des heures de suivi. |
| 14 | 2026-10-10 00:09:03 | Garder le tableau historique continu et lisible. |
| 15 | 2026-10-10 00:10:45 | Vérifier les artefacts après la fin du build seulement. |
| 16 | 2026-10-10 00:22:15 | Contrôler les contrats réels avant d'attribuer une erreur au natif. |
| 17 | 2026-10-10 00:36:27 | Typer les générations et conserver les lignes de fichiers distinctes. |
| 18 | 2026-10-10 00:48:46 | Utiliser les chemins trouvés et vérifier les substitutions de texte. |
| 19 | 2026-10-10 00:55:18 | Éviter les exécutions natives ajoutées sans nécessité. |
| 20 | 2026-10-10 01:15:34 | Limiter les choix aux appareils et ports réellement présents. |
| 21 | 2026-10-10 01:18:52 | Traduire seulement les ressources des fonctions exposées. |
| 22 | 2026-10-10 01:21:11 | Séparer invariants et textes traduits, avec encodage explicite. |
| 23 | 2026-10-10 01:33:12 | Lire les signatures SDK avant d'écrire leurs consommateurs. |
| 24 | 2026-10-10 01:41:30 | Vérifier images et commandes réelles avant génération. |
| 25 | 2026-10-10 01:51:17 | Lier les contrôles et éditeurs à la configuration réellement utilisée. |
| 26 | 2026-10-10 01:56:07 | Contrôler la transparence et préserver les photos originales. |
| 27 | 2026-10-10 01:59:11 | Persister les périphériques indépendamment des médias montés. |
| 28 | 2026-10-10 09:53:42 | Reprendre le traitement éprouvé et poursuivre les travaux indépendants. |
| 29 | 2026-10-10 09:59:42 | Vérifier les nouveaux fichiers indexés avant commit. |
| 30 | 2026-10-10 10:16:40 | Vérifier les contrats du paquet et les limites avant chargement. |
| 31 | 2026-10-10 10:23:07 | Préserver la structure des fichiers lors des générations. |
| 32 | 2026-10-10 10:36:34 | Éviter de recréer les fichiers déjà réalisés après découpage du plan. |
| 33 | 2026-10-10 10:51:37 | Signaler les finitions restantes sans interrompre les travaux indépendants. |
| 34 | 2026-10-10 11:05:28 | Achever et relire les sources de ressources avant traduction. |
| 35 | 2026-10-10 11:14:04 | Contextualiser les libellés avant la traduction globale. |
| 36 | 2026-10-10 11:23:03 | Conserver les heures réelles lors des corrections du chronomètre. |
| 37 | 2026-10-10 11:28:27 | Séparer normalisation, audit et modification des seules traductions. |
| 38 | 2026-10-10 11:39:14 | Vérifier les signatures et limiter le diff de solution au raccordement. |
| 39 | 2026-10-10 11:53:51 | Vérifier les imports et employer des preuves adaptées à la capacité. |
| 40 | 2026-10-10 12:03:44 | Accepter les affectations vides produites par l'éditeur réel. |
| 41 | 2026-10-10 12:34:36 | Vérifier lisibilité, unités et fonctions intégrées avant livraison. |
| 42 | 2026-10-10 12:53:21 | Distinguer preuve de description et preuve de rendu réel. |
| 43 | 2026-10-10 13:06:27 | Résoudre les libellés dans leur propriétaire commun. |
| 44 | 2026-10-10 13:14:16 | Séparer reprise générique et adaptation particulière dès les copies. |
| 45 | 2026-10-10 13:21:49 | Vérifier callbacks et dépendances avant reprise du runtime. |
| 46 | 2026-10-10 13:31:05 | Définir le contrat complet des supports avant ses consommateurs. |
| 47 | 2026-10-10 13:34:30 | Exclure les catalogues de jeux sans utilité demandée. |
| 48 | 2026-10-10 13:39:47 | Préparer chaque module depuis les corrections de méthode acquises. |
| 49 | 2026-10-10 13:45:57 | Distinguer réglages globaux et réglages par port avant application. |
| 50 | 2026-10-10 14:14:02 | Réutiliser les libellés contextualisés et les chemins réels. |
| 51 | 2026-10-10 14:35:24 | Créer les dépendances avant les consommateurs et garder le moteur choisi. |
| 52 | 2026-10-10 14:52:24 | Préparer le contrôle du vrai sélecteur depuis les contrats existants. |
| 53 | 2026-10-10 15:07:14 | Arrêter les générations au premier échec et vérifier les substitutions. |
| 54 | 2026-10-10 15:19:00 | Ranger les réglages de support dans leur configuration. |
| 55 | 2026-10-10 15:37:06 | Ne reprendre que les services utilisés et les fichiers réellement trouvés. |
| 56 | 2026-10-10 15:42:04 | Valider le contenu et les fichiers compagnons avant transfert natif. |
| 57 | 2026-10-10 16:09:25 | Réutiliser les traductions validées et normaliser avant audit. |
| 58 | 2026-10-10 16:18:11 | Réutiliser le traitement existant et reprendre les téléchargements manquants. |
| 59 | 2026-10-10 16:30:21 | Inspecter les contours réels avant de multiplier les visuels. |
| 60 | 2026-10-10 16:33:45 | Garder les assets chez leur propriétaire sans lien entre projets. |
| 61 | 2026-10-10 16:36:27 | Distinguer machine, commandes intégrées et périphérique de contrôle. |
| 62 | 2026-10-10 16:42:08 | Décrire les conventions de fichiers et les frontières de responsabilité. |
| 63 | 2026-10-10 16:46:42 | Éviter une seconde incorporation d'un visuel déjà possédé par App. |
| 64 | 2026-10-10 16:47:43 | Séparer logo, photos et preuves de travail. |
| 65 | 2026-10-10 16:57:53 | Vérifier les ressources par la découverte réelle du paquet. |
| 66 | 2026-10-10 17:06:50 | Attendre la destruction effective des propriétaires natifs. |
| 67 | 2026-10-10 17:35:41 | Vérifier le format d'image et les remplacements avant validation. |
| 68 | 2026-10-10 17:49:38 | Contrôler dépendances et doublons avant le build et la couverture. |
| 69 | 2026-10-10 18:06:25 | Séparer matériel réel et buffers internes avant de renseigner les infos. |
| 70 | 2026-10-10 18:16:39 | Laisser toutes les associations de manette vides initialement. |
| 71 | 2026-10-10 18:50:21 | Relever exactement les exigences firmware avant de générer leurs champs. |
| 72 | 2026-10-10 19:00:15 | Conserver le contrat commun indépendant du protocole natif. |
| 73 | 2026-10-10 19:10:44 | Auditer les fonctions copiées autant que les dictionnaires. |
| 74 | 2026-10-10 19:25:18 | Faire fournir les capacités particulières par les adaptateurs. |
| 75 | 2026-10-10 19:42:46 | Inspecter tout le fond des photos et typer les collections générées. |
| 76 | 2026-10-10 19:56:25 | Supprimer les helpers sans appelant et les types natifs hors adaptation. |
| 77 | 2026-10-10 20:09:29 | Vérifier limites et terminaison des tables avant leur lecture. |
| 78 | 2026-10-10 20:21:11 | Vérifier unicité des blocs et classement fonctionnel avant traduction. |
| 79 | 2026-10-10 20:33:38 | Garder les snapshots communs indépendants du transport natif. |
| 80 | 2026-10-10 20:53:44 | Interroger les capacités au moment approprié et reprendre le seul échec. |
| 81 | 2026-10-10 21:23:33 | Recueillir les preuves avant d'engager une correction native. |
| 82 | 2026-10-10 21:35:00 | Conserver les chemins trouvés et les attributs XML normalisés. |
| 83 | 2026-10-10 21:49:40 | Raccorder une donnée à toute sa chaîne avant de déclarer sa fonction. |
| 84 | 2026-10-10 22:03:31 | Valider le binaire réellement installé, pas une variante de contrôle. |
| 85 | 2026-10-10 22:50:25 | Retirer complètement une adaptation demandée sans casser la persistance. |
| 86 | 2026-10-10 23:04:12 | Restaurer les nouveaux contrôles une fois et réutiliser les visuels existants. |
| 87 | 2026-10-10 23:12:11 | Réécrire la procédure par fichiers, responsabilités et dépendances ; supprimer répétitions et comptes rendus particuliers. |
| 88 | 2026-10-10 23:13:01 | Utiliser la recette acquise sans réétudier les références ; intégrer les corrections directement aux fichiers, dépendances et étapes de fabrication. |
| 89 | 2026-10-10 23:19:00 | Détailler les fichiers, dépendances et adaptations avant copie ; réutiliser la carte corrigée pour éviter inventaires répétés et composants inutiles. |
| 90 | 2026-10-10 23:26:46 | Corriger la citation des textes avant de relancer un script ; arrêter les lectures et générations dès la première erreur. |
| 91 | 2026-10-10 23:35:36 | Produire les RESX au format compact dès chaque génération ; contrôler les valeurs avant et après normalisation pour éviter une reprise globale. |
| 92 | 2026-10-10 23:38:56 | Généraliser la règle sans coupure artificielle à tous les formats et aux générateurs dès leur écriture. |
| 93 | 2026-10-10 23:56:44 | Relever les identifiants SDK avant les contrôles et découvrir les firmwares locaux avant de demander leur emplacement. |
