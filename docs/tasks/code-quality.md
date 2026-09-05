# Qualité et refactorisation du code

État réconcilié le 5 septembre 2026 avec le code au commit `809bfb24` et son historique. Une case cochée indique une réalisation vérifiée ; la mention **Sans objet** distingue une exigence devenue obsolète. Une case ouverte peut désigner un contrôle restant sur du code déjà présent, et non une implémentation à recommencer. Les validations manuelles et la clôture globale ne sont pas déduites de la seule présence du code. Les travaux restants suivent l’ordre des phases.

## Phase 02 — Refactorisation et rangement du code

Cette phase conserve le suivi du rangement réalisé et des contrôles encore nécessaires. Les anciens plans préparatoires devenus sans objet sont clos explicitement ; les extractions et déplacements déjà réalisés ne sont pas à recommencer.

Les anciens exemples (`AtariScpSectorImageReader.cs`, racine de `GWGUI.App`, `GWGUI.MediaEngine/Images`) sont historiques. L’examen des responsabilités encore mal placées couvre toujours tous les projets de production, dont `MainWindow.xaml.cs` ; il part de l’arborescence actuelle.

### Règles de travail de cette phase

Ces points sont des règles, pas des tâches à cocher.

- Respecter l’ordre du présent document.
- Ne pas recommencer la cartographie générale validée dans la tâche 01.
- Avant de déplacer du code, écrire la structure cible dans un document Markdown, la contrôler, la corriger si nécessaire, puis la faire valider.
- Ne pas relire un fichier ou un document inchangé qui vient déjà d’être analysé. Revenir uniquement sur la partie modifiée ou sur une information précise manquante.
- Ne pas modifier le comportement fonctionnel pendant un déplacement architectural.
- Ne pas transformer une sélection explicite en détection exclusive : les images et disquettes multiformats doivent rester prises en charge.
- Ne pas appliquer automatiquement un modèle unique à tous les cas. Choisir entre registre, stratégie, polymorphisme, `switch` ou conditions simples selon la responsabilité réelle.
- Ne pas créer du code uniquement pour faire diminuer le nombre de lignes. Chaque extraction doit donner un propriétaire clair à une responsabilité.
- Ne pas créer une seconde implémentation pendant une extraction. Le nouveau chemin remplace l’ancien lorsqu’il est raccordé.
- Cocher chaque case immédiatement après sa réalisation et sa vérification, sans attendre la fin de la phase.
- Regrouper les contrôles techniques par bloc cohérent. Ne pas relancer toute la suite de tests après chaque modification mineure.
- Faire un commit lorsqu’une tâche terminée forme une modification autonome.
- Pousser lorsqu’une ou plusieurs tâches terminées forment un bloc complet et cohérent.
- Les constantes et textes bruts relèvent de la tâche 03, les modèles et contrats de la tâche 04, les fonctions et services de la tâche 05, et le rangement des traductions de la tâche 06. La phase 02 doit signaler leurs emplacements, sans exécuter ces phases à leur place.

### 2.1 Définir et valider la structure cible

#### 2.1.1 Préparer le document de structure

- [x] Établir la structure cible initiale de la phase 02.
  - [x] Y reprendre les projets existants sans refaire leur cartographie fonctionnelle complète.
  - [x] Y représenter l’arborescence actuelle seulement au niveau nécessaire pour comprendre les déplacements.
  - [x] Y représenter l’arborescence cible proposée.
  - [x] Y indiquer le rôle de chaque dossier cible.
  - [x] Y indiquer les dépendances autorisées entre les projets.
  - [x] Y indiquer les dépendances interdites entre les projets.

#### 2.1.2 Préparer le plan de déplacement

**Sans objet pour le plan initial** : le document de proposition a été supprimé lors de la réorganisation documentaire (`e8ec6cfb`) et le rangement a depuis évolué. Il n’est plus nécessaire de reconstruire rétroactivement sa table de déplacement. Les cases ci-dessous sont closes à ce titre, sans prétendre que cette table avait été rédigée. Les déplacements encore nécessaires restent soumis à la règle de préparation et de validation de leur structure cible, au moment du bloc concerné.

- [x] Ajouter au document une table de correspondance entre l’emplacement actuel et l’emplacement cible — **sans objet pour l’ancien plan**.
  - [x] Couvrir `GWGUI.App`.
  - [x] Couvrir `GWGUI.Domain`.
  - [x] Couvrir `GWGUI.Infrastructure`.
  - [x] Couvrir `GWGUI.MediaEngine`.
  - [x] Couvrir les fichiers de production présents à la racine de chaque projet.
  - [x] Distinguer les fichiers qui restent à leur place.
  - [x] Distinguer les fichiers qui doivent seulement être déplacés.
  - [x] Distinguer les fichiers qui doivent être renommés.
  - [x] Distinguer les fichiers qui doivent être séparés en plusieurs responsabilités.
  - [x] Distinguer les fichiers dont plusieurs parties doivent rejoindre des modules existants.

#### 2.1.3 Contrôler la structure proposée

**Sans objet pour l’ancienne proposition supprimée**, comme 2.1.2. Ces cases ferment la revue préparatoire de ce document, sans attester une approbation historique. Les contrôles du code actuel restent en 2.2 à 2.6 et 2.9 ; une nouvelle proposition sera préparée uniquement pour les déplacements restant à effectuer.

- [x] Vérifier la structure cible avant toute modification du code.
  - [x] Vérifier qu’un dossier ne mélange pas interface, domaine, infrastructure et formats de disquette.
  - [x] Vérifier que les dossiers génériques comme `Controls`, `Services` et `Images` ne redeviennent pas des dossiers fourre-tout.
  - [x] Vérifier que chaque famille de fichiers possède un emplacement évident.
  - [x] Vérifier qu’un nouveau format pourra être ajouté sans modifier plusieurs dossiers sans rapport.
  - [x] Vérifier que les composants réellement communs ne sont pas dupliqués dans chaque fonction.
  - [x] Vérifier que les composants spécifiques ne sont pas placés artificiellement dans un dossier commun.
  - [x] Corriger le document tant qu’un fichier important n’a pas de destination claire.
- [x] Faire valider la structure cible avant de commencer les déplacements.

### 2.2 Réorganiser `GWGUI.App`

#### 2.2.1 Nettoyer la racine du projet

- [x] Examiner les fichiers actuellement placés à la racine de `GWGUI.App` et constater leur rangement.
  - [x] Laisser à la racine uniquement les fichiers qui appartiennent réellement au démarrage ou à la définition du projet.
  - [x] Ranger les fenêtres secondaires selon leur fonction.
    - [x] Fenêtre À propos.
    - [x] Fenêtres de conflits de Lecture et Conversion.
    - [x] Fenêtre des problèmes de l’Explorateur.
    - [x] Fenêtres liées aux outils GW.
    - [x] Fenêtre de matériel indisponible.
    - [x] Fenêtre d’historique des journaux.
    - [x] Fenêtre de nommage des profils.
    - [x] Fenêtres et vues SCP.
  - [x] Ranger les contrôles actuellement isolés à la racine dans leur fonction réelle.
  - [x] Ranger `StoragePaths` et `ThemeManager` selon leur responsabilité, dans `Services/Storage` et `Services/Theming`.
  - [x] Mettre à jour les namespaces, références XAML et ressources après chaque groupe de déplacements.

#### 2.2.2 Réorganiser les composants visuels

- [x] Remplacer le dossier `Controls` unique par une structure fonctionnelle (`Views/Controls`).
  - [x] Séparer les composants communs réutilisables.
  - [x] Regrouper les composants de Lecture.
  - [x] Regrouper les composants d’Écriture.
  - [x] Regrouper les composants de Conversion.
  - [x] Regrouper les composants du Visualisateur.
  - [x] Regrouper les composants de l’Explorateur.
  - [x] Regrouper les composants des Outils.
  - [x] Regrouper les composants des Options.
  - [x] Regrouper menu, terminal, barre d’état et progression dans les composants de `Views/Controls/Shell`.
  - [x] Vérifier que chaque composant XAML et son code-behind restent ensemble.
  - [x] Vérifier que les composants réutilisés n’embarquent pas l’état d’un onglet particulier.

Vérification ciblée du code et des usages XAML le 5 septembre 2026 : `CardSection`, `PathSection`, `MainTabHeader`, `FileEntryIcon`, `ProfileSection`, `DiskClassificationSelector`, `TrackProgressStrip` et `ScpInspectorPanel`. Les propriétés et collections de présentation sont propres à chaque instance ; les seules brosses partagées de `TrackProgressStrip` sont gelées. Les chemins et profils sont fournis par leurs consommateurs ; le panneau SCP reçoit son modèle par `DataContext`. Aucun de ces composants ne conserve de référence à un onglet concret ni d’état métier statique partagé entre onglets. La synchronisation Explorateur/Visualisateur est explicitement assurée par `DiskImageWorkspaceController`, hors des composants communs. Aucun refactor nécessaire pour ce point. Les essais interactifs d’indépendance restent suivis en 7.1.

#### 2.2.3 Reprendre complètement `MainWindow`

- [x] Établir dans le présent suivi la liste des responsabilités encore présentes dans `MainWindow.xaml.cs`.
  - [x] Initialisation de la fenêtre.
  - [x] Navigation entre les onglets.
  - [x] Lecture.
  - [x] Écriture.
  - [x] Conversion.
  - [x] Visualisation.
  - [x] Exploration.
  - [x] Outils et maintenance.
  - [x] Profils.
  - [x] Matériel et sélection du lecteur.
  - [x] Exécution et arrêt des commandes.
  - [x] Terminal et journaux.
  - [x] Progression et barre d’état.
  - [x] Placement et dimensions de la fenêtre.
  - [x] Synchronisation d’une image entre Explorateur et Visualisateur.
- [x] Attribuer un propriétaire cible à chacune de ces responsabilités.
- [x] **Clos par décision utilisateur — ne pas toucher** : conserver l’extraction actuelle de `MainWindow` et abandonner les déplacements supplémentaires proposés.
  - [x] Déplacer son état — **sans objet, organisation actuelle conservée**.
  - [x] Déplacer ses traitements — **sans objet, organisation actuelle conservée**.
  - [x] Déplacer ses gestionnaires d’événements — **sans objet, organisation actuelle conservée**.
  - [x] Exposer uniquement les commandes, données et événements nécessaires à la fenêtre principale — **refactor supplémentaire abandonné**.
  - [x] Remplacer les accès directs de `MainWindow` aux contrôles internes par l’interface publique du composant concerné — **refactor supplémentaire abandonné**.
  - [x] Vérifier les doubles abonnements après déplacement — **sans objet, déplacement abandonné ; ne vaut pas validation générale des abonnements actuels**.
  - [x] Vérifier les doubles constructions après déplacement — **sans objet, déplacement abandonné ; ne vaut pas validation générale des constructions actuelles**.
  - [x] Supprimer de `MainWindow` l’ancien code après raccordement du nouveau propriétaire — **sans objet, organisation actuelle conservée**.
- [x] **Clos par décision utilisateur** : conserver le rôle actuel de `MainWindow`, y compris les traitements et raccordements examinés.
  - [x] Conserver la création des grands blocs de la fenêtre.
  - [x] Réduire davantage la navigation conservée dans la fenêtre — **sans objet, organisation actuelle conservée**.
  - [x] Réduire davantage les échanges conservés dans la fenêtre — **sans objet, organisation actuelle conservée**.
  - [x] Documenter les responsabilités qui doivent encore y rester et pourquoi.

Responsabilités et propriétaires constatés dans le constructeur et les délégations de `MainWindow` :

| Responsabilité | Propriétaire actuel ou cible pour les restes |
|---|---|
| Initialisation, chargement et fermeture | Composition dans `MainWindow`, cycle de vie dans `MainWindowLifecycleController` |
| Navigation globale et échanges entre onglets | `MainWindow`, car ils relient plusieurs fonctions |
| Lecture / Écriture / Conversion | `ReadTabController`, `WriteTabController`, `ConversionTabController` |
| Visualisation, exploration et synchronisation d’image | `DiskImageWorkspaceController`, `ScpInspectorController`, `ExplorerReadController` |
| Outils et maintenance | `MaintenanceToolsController`, `HostToolsUpdateController` |
| Profils | `OperationProfileController`, `OperationProfileCollection` |
| Matériel et sélection du lecteur | `HardwareSelectionController`, `StartupHardwareMonitor` |
| Exécution et arrêt | `OperationRuntimeController` ; confirmation globale raccordée par la fenêtre |
| Terminal et journaux | `TerminalPanelController`, `ConsoleLogSession` |
| Progression et barre d’état | `OperationProgressController`, `ApplicationStatusBar` |
| Placement et dimensions | `WindowPlacementController` |

Décision utilisateur du 5 septembre 2026 : **ne pas toucher à l’organisation actuelle de `MainWindow` pour ce refactor**. La composition, la navigation, les accès aux contrôles et les traitements restants examinés sont conservés. Cette décision clôt les tâches de déplacement ci-dessus ; elle ne signifie pas que les extractions abandonnées ont été réalisées. Une nouvelle demande explicite sera nécessaire pour reprendre ce chantier.

Motif de la décision : les opérations, profils et états propres à Lecture/Écriture/Conversion sont déjà largement délégués aux trois contrôleurs. Les déplacements supplémentaires concernaient `RefreshFormatSelectors`, les validations de `ToolCommand_Click` et les abonnements des méthodes `ConnectReadComponents`, `ConnectWriteComponents` et `ConnectConvertComponents`. Ils demanderaient de préserver l’ordre d’initialisation et des événements de sélection, ainsi que l’accès aux catalogues et réglages actualisés. Après examen de ces dépendances, l’utilisateur choisit de conserver le fonctionnement et l’organisation actuels.

#### 2.2.4 Reprendre `OptionsWindow`

- [x] Vérifier les responsabilités restantes dans `OptionsWindow.xaml.cs` sans refaire celles déjà correctement extraites.
  - [x] Général.
  - [x] Contrôleurs et lecteurs.
  - [x] Profils.
  - [x] Journaux.
  - [x] Host Tools.
  - [x] Tags.
  - [x] Sauvegarde immédiate et fermeture.
- [x] **Clos par décision utilisateur — ne pas toucher** : conserver les responsabilités actuelles d’`OptionsWindow` et abandonner les déplacements supplémentaires proposés.
- [x] **Clos par décision utilisateur** : conserver le rôle actuel de la fenêtre, y compris la coordination de la sauvegarde et de la fermeture.
- [x] Vérifier que les changements automatiques restent appliqués sans bouton Enregistrer général.

Décision utilisateur du 5 septembre 2026 : conserver l’organisation actuelle d’`OptionsWindow`, comme pour `MainWindow` en 2.2.3. Les contrôleurs et les raccordements existants restent en place. Les tâches d’extraction supplémentaire sont closes par cette décision, sans prétendre que ces extractions ont été réalisées. Une nouvelle demande explicite sera nécessaire pour reprendre ce chantier.

#### 2.2.5 Réorganiser services et ViewModels de l’application

- [x] Classer tous les fichiers de `Services` selon leur consommateur et leur portée.
  - [x] Services réellement globaux.
  - [x] Services de Lecture.
  - [x] Services d’Écriture.
  - [x] Services de Conversion.
  - [x] Services du Visualisateur.
  - [x] Services de l’Explorateur.
  - [x] Services matériels.
  - [x] Services de fenêtre et navigation.
  - [x] Services d’exécution, progression, terminal et journaux.
- [x] Classer tous les fichiers de `ViewModels` selon leur fonction.
- [x] Vérifier qu’un ViewModel ne manipule pas directement un contrôle WPF.
- [ ] Vérifier qu’un service spécifique à un onglet n’est pas présenté comme service global.
- [ ] Vérifier que les services globaux ne dépendent pas d’un onglet concret.

### 2.3 Réorganiser `GWGUI.MediaEngine`

Constat de la revue de 2.2 : le rangement est présent dans `Views/Windows`, `Views/Dialogs`, `Views/Controls`, `Services` et `ViewModels`, avec des sous-dossiers fonctionnels. `StoragePaths` et `ThemeManager` sont respectivement dans `Services/Storage` et `Services/Theming`. Aucun accès direct à un contrôle WPF n’a été trouvé dans les 20 ViewModels. Les contrôleurs de Lecture, Écriture, Conversion, cycle de vie et Options existent déjà : ne pas refaire leurs extractions. Les chantiers supplémentaires de `MainWindow` et `OptionsWindow` sont clos par décision utilisateur en 2.2.3 et 2.2.4.

#### 2.3.1 Reprendre entièrement le dossier `Images`

- [x] Classer chaque fichier actuellement présent directement dans `Images`.
  - [x] Lecteurs de conteneurs et images sectorielles.
  - [x] Géométries et signatures.
  - [x] Métadonnées, systèmes et protections.
  - [x] Exploration des images.
  - [x] Détection et interprétation.
  - [x] Normalisation des images reconnues.
  - [x] Conversion interne.
  - [x] Visualisation des images sectorielles.
  - [x] Contrats et résultats partagés.
- [x] Déplacer chaque fichier vers la catégorie approuvée dans la structure cible.
- [x] Créer des sous-dossiers par famille uniquement lorsque plusieurs fichiers spécialisés de cette famille doivent rester ensemble.
  - [x] Acorn/BBC.
  - [x] Amiga.
  - [x] Amstrad.
  - [x] Apple.
  - [x] Atari.
  - [x] Commodore.
  - [x] DEC.
  - [x] Epson.
  - [x] IBM PC et compatibles.
  - [x] MSX.
  - [x] UCSD.
  - [x] Autres familles déjà prises en charge.
- [x] Ne pas ranger un lecteur commun dans une famille particulière uniquement parce qu’elle a été la première à l’utiliser.
- [x] Vérifier que l’ajout d’un nouveau lecteur possède un point d’enregistrement clairement identifié.

#### 2.3.2 Reprendre les conteneurs

- [x] Vérifier chaque politique de `Images/Containers`.
  - [x] Un conteneur doit seulement reconnaître et ouvrir son type d’image.
  - [x] Une politique de conteneur ne doit pas analyser le système de fichiers à la place du module prévu.
  - [x] Une politique générique ne doit pas porter le nom d’une machine particulière.
  - [x] Les délégations doivent avoir une destination unique et visible.
- [x] Regrouper les contrats, contextes, registres et implémentations sans les mélanger.

#### 2.3.3 Reprendre la détection et l’interprétation

- [x] Vérifier séparément les responsabilités de `ScpDetection` et `Interpretations`.
  - [x] Détection des familles compatibles.
  - [x] Production de plusieurs résultats compatibles pour une image multiformat.
  - [x] Classement des résultats sans supprimer les autres résultats valides.
  - [x] Interprétation supplémentaire d’une image déjà ouverte.
  - [x] Normalisation des résultats reconnus.
- [x] Retirer les essais successifs inutiles lorsqu’un conteneur ou une famille permet de limiter les candidats.
- [x] Conserver plusieurs candidats lorsque le support peut réellement contenir plusieurs systèmes.
- [x] Vérifier que la sélection manuelle choisit le traitement demandé sans détruire la capacité multiformat de la détection automatique.
- [x] Documenter le chemin automatique et le chemin manuel après leur réorganisation.

#### 2.3.4 Reprendre `SectorImages`

- [x] Classer tous les lecteurs et reconstructeurs sectoriels par responsabilité réelle.
  - [x] Contrats et modèles sectoriels communs.
  - [x] Reconstruction ISO FM/MFM commune.
  - [x] Politiques ISO propres aux machines.
  - [x] Reconstruction Apple.
  - [x] Reconstruction Commodore.
  - [x] Reconstruction DEC.
  - [x] Autres reconstructions spécialisées.
- [x] Vérifier `AtariScpSectorImageReader.cs` comme exemple, puis contrôler tous les autres lecteurs.
  - [x] Ne conserver dans un lecteur Atari que les décisions propres à Atari.
  - [x] Placer Amstrad dans son module.
  - [x] Placer BBC/Acorn dans son module.
  - [x] Placer Epson dans son module.
  - [x] Placer IBM PC dans son module.
  - [x] Placer UCSD dans son module.
  - [x] Placer les opérations ISO communes dans un composant au nom neutre.
- [x] Rechercher le même défaut dans chaque autre lecteur nommé selon une machine.
- [x] Corriger chaque nom qui ne représente pas le contenu réel du fichier.
- [x] Vérifier qu’une règle de géométrie spécialisée ne fuit pas dans le constructeur sectoriel commun.

#### 2.3.5 Reprendre décodage et encodage

- [x] Vérifier l’organisation complète de `Decoding`.
  - [x] Un décodeur spécialisé par fichier.
  - [x] Bases partagées séparées des décodeurs concrets.
  - [x] Modèles et contrats séparés des implémentations.
  - [x] Registre unique des décodeurs disponibles.
- [x] Vérifier l’organisation complète de `Encoding`.
  - [x] Un encodeur spécialisé par fichier.
  - [x] Bases partagées séparées des encodeurs concrets.
  - [x] Modèles et contrats séparés des implémentations.
  - [x] Registre unique des encodeurs disponibles.
- [x] Comparer les registres de décodage et d’encodage sans supposer que toutes les capacités sont forcément symétriques.
- [x] Vérifier que les primitives partagées ne contiennent aucune décision propre à une machine.
- [x] Vérifier que les implémentations spécialisées ne recopient pas une primitive déjà disponible.

#### 2.3.6 Reprendre les systèmes de fichiers

- [x] Réorganiser `FileSystems` pour distinguer clairement contrats, modèles, registre, aides communes et lecteurs concrets.
- [x] Classer les lecteurs concrets selon la structure cible approuvée.
- [x] Vérifier qu’un lecteur de système de fichiers ne prend pas en charge un conteneur.
- [x] Vérifier qu’un lecteur de système de fichiers ne décide pas du rendu visuel.
- [x] Vérifier qu’une aide commune n’encode pas une règle propre à une seule famille.
- [x] Vérifier que le registre est l’unique point de découverte des lecteurs disponibles.

#### 2.3.7 Reprendre la visualisation technique

- [x] Vérifier la séparation entre reconstruction des données et classification visuelle.
- [x] Classer les politiques de visualisation selon la structure cible.
- [x] Conserver dans le registre le point unique de sélection de la politique.
- [x] Vérifier que les couleurs et catégories visuelles ne modifient pas le résultat du décodage.
- [x] Vérifier que le Visualisateur ne connaît pas directement chaque lecteur concret.

#### 2.3.8 Reprendre les modèles SCP et primitives

- [x] Vérifier la séparation entre lecteur SCP, modèles SCP, informations de capture et constantes de structure.
- [x] Vérifier que `ScpReader` ne contient que la lecture du conteneur SCP.
- [x] Vérifier que les primitives de bits et CRC ne dépendent ni de l’interface ni d’une machine précise.
- [x] Signaler dans la tâche 03 les constantes encore mal placées sans les traiter dans cette phase.
- [x] Signaler dans la tâche 04 les modèles ou contrats encore mélangés sans les traiter dans cette phase.

### 2.4 Réorganiser `GWGUI.Domain`

#### 2.4.1 Vérifier les frontières fonctionnelles

- [x] Examiner l’arborescence actuelle et les responsabilités déclarées dans chaque dossier du domaine.
  - [x] Commandes.
  - [x] Conversion.
  - [x] Formats.
  - [x] Matériel.
  - [x] Host Tools.
  - [x] Maintenance.
  - [x] Nommage.
  - [x] Profils.
  - [x] Lecture.
  - [x] Réglages.
  - [x] Écriture.
  - [x] Parité, également présente dans le domaine actuel.
- [ ] Vérifier que chaque fichier appartient réellement à son dossier.
- [ ] Déplacer les fichiers mal rangés vers la fonction qui les possède.
- [x] Vérifier qu’aucun fichier du domaine ne dépend de WPF.
- [x] Vérifier qu’aucun fichier du domaine ne dépend d’une implémentation Windows ou d’un stockage concret.

#### 2.4.2 Reprendre les commandes et opérations

- [ ] Distinguer construction, validation, planification et exécution des commandes.
- [x] Vérifier séparément Lecture, Écriture et Conversion.
- [ ] Vérifier que les options communes ne sont pas recopiées dans chaque constructeur.
- [x] Vérifier que les différences propres aux opérations restent dans leur fonction.
- [x] Vérifier que la compatibilité d’un format ne dépend pas d’un contrôle WPF.

#### 2.4.3 Reprendre formats et capacités

- [x] Vérifier les responsabilités du catalogue intégré, du catalogue tenant compte de GW et des modèles de format.
- [ ] Conserver une source commune pour les formats proposés à Lecture, Écriture, Conversion, Explorateur et Visualisateur.
- [ ] Vérifier que les capacités propres à GW restent distinctes des capacités internes de GW GUI.
- [ ] Vérifier que les définitions de disquette intégrées ont un propriétaire unique.

Les trois constructeurs d’opérations sont distincts et utilisent `GwOptionValidator`, `BuiltInDiskDefinitions` et `CommandLineTokenizer`. Ils recopient encore la boucle d’ajout des options et, pour Lecture/Écriture, la primitive `Add` : la mutualisation reste ouverte en 2.4.2, 2.6 et phase 05.

#### 2.4.4 Reprendre réglages, profils et matériel

- [x] Vérifier que les réglages sont répartis par domaine sans recréer un fichier monolithique.
- [x] Vérifier que les profils restent séparés par opération.
- [x] Vérifier que la description physique d’un lecteur ne devient pas une option GW.
- [x] Vérifier que routage matériel, registre matériel et découverte Windows restent séparés.

`OperationProfileCollection` partitionne les magasins par `OperationKind`. `HardwareRoutingPolicy` déduit les arguments du routage et du nombre de périphériques, sans transformer taille, densité ou RPM en options de commande. Les essais matériels restent distincts de ce contrôle du code.

### 2.5 Réorganiser `GWGUI.Infrastructure`

#### 2.5.1 Vérifier chaque implémentation technique

- [x] Classer les implémentations par domaine technique.
  - [x] Découverte matérielle Windows.
  - [x] Registre matériel Greaseweazle.
  - [x] Installation et capacités Host Tools.
  - [x] Exécution des processus.
  - [x] Journaux d’opération.
  - [x] Stockage des réglages.
- [ ] Vérifier que chaque implémentation réalise un contrat du domaine ou un besoin clairement identifié de l’application.
- [x] Vérifier que l’infrastructure ne contient aucune décision d’affichage WPF.
- [ ] Vérifier que l’infrastructure ne décide pas du format métier à la place des catalogues du domaine.
- [x] Vérifier que les classes propres à Windows sont identifiables par leur emplacement et leur nom.

### 2.6 Supprimer les duplications architecturales

#### 2.6.1 Rechercher les doubles chemins

- [ ] Rechercher les responsabilités possédant encore deux implémentations après les déplacements.
  - [ ] Détection de format.
  - [ ] Catalogue des formats.
  - [ ] Sélection machine/format/protection.
  - [ ] Ouverture d’image.
  - [ ] Reconstruction sectorielle.
  - [ ] Exploration des systèmes de fichiers.
  - [ ] Classification visuelle.
  - [ ] Gestion des profils.
  - [ ] Exécution des commandes.
  - [ ] Progression.
  - [ ] Terminal et journaux.
  - [ ] Placement des fenêtres.
- [ ] Désigner un seul propriétaire pour chaque responsabilité dupliquée.
- [ ] Raccorder tous les consommateurs au propriétaire retenu.
- [ ] Supprimer l’ancien chemin après le raccordement.

#### 2.6.2 Rechercher les algorithmes recopiés

- [ ] Identifier les blocs identiques ou presque identiques entre familles.
- [ ] Distinguer une vraie primitive commune d’une ressemblance accidentelle.
- [ ] Extraire uniquement les primitives réellement communes.
- [ ] Paramétrer les différences simples déjà représentées par un identifiant de machine ou de format.
- [ ] Conserver des implémentations séparées lorsque les règles du format diffèrent réellement.
- [ ] Reporter dans les tâches 03, 04 ou 05 les changements qui appartiennent aux constantes, modèles, contrats, fonctions ou services.

### 2.7 Réorganiser les tests sans les multiplier inutilement

#### 2.7.1 Ranger les tests selon le code de production

- [x] **Sans objet pour l’ancien document** : y redéfinir l’arborescence cible des tests. La séparation actuelle est décrite dans `docs/architecture/overview.md` : suite courante `GWGUI.Tests`, corpus privé `GWGUI.LocalDiskImageTests` et catégorie `GpuExhaustive`.
- [ ] Regrouper les tests par projet et fonction de production.
- [x] Séparer les tests unitaires ciblés des tests utilisant le corpus d’images.
- [x] Conserver les images externes hors du dépôt selon les règles déjà décidées.
- [x] **Sans objet comme tâche autonome** : ne pas créer de tests redondants. Cette consigne reste applicable à tout ajout de test.

#### 2.7.2 Définir des blocs de contrôle rapides

- [ ] Définir un bloc de contrôle pour `GWGUI.App`.
- [ ] Définir un bloc de contrôle pour `GWGUI.Domain`.
- [ ] Définir un bloc de contrôle pour `GWGUI.Infrastructure`.
- [ ] Définir un bloc de contrôle pour `GWGUI.MediaEngine`.
- [ ] Définir un bloc ciblé pour la détection multiformat.
- [ ] Définir un bloc ciblé pour les registres de formats, décodeurs, encodeurs et systèmes de fichiers.
- [x] **Sans objet comme tâche autonome** : Utiliser le bloc concerné après une série cohérente de déplacements. Consigne à appliquer aux blocs futurs.
- [x] **Sans objet comme tâche autonome** : Réserver la compilation complète et la suite complète à la clôture d’un grand bloc ou de la phase. Consigne à appliquer aux blocs futurs.

### 2.8 Contrôler chaque bloc de refactorisation

**Sans objet comme liste de travaux autonomes** : ce bloc décrit des consignes récurrentes, déjà prescrites par les règles de travail. Les cases sont closes à ce titre, sans affirmer que chaque ancien bloc a reçu tous ces contrôles. Ces consignes restent obligatoires pour les modifications futures ; les validations finales encore nécessaires restent ouvertes en 2.9.

#### 2.8.1 Contrôle après chaque tâche autonome

- [x] Vérifier uniquement les fichiers et dépendances modifiés.
- [x] Exécuter le bloc de tests ciblé correspondant.
- [x] Vérifier qu’aucun abonnement, enregistrement ou appel n’est dupliqué.
- [x] Vérifier que l’ancien chemin a été supprimé lorsqu’il n’est plus utilisé.
- [x] Mettre à jour le document de structure si la destination finale a dû être corrigée.
- [x] Cocher immédiatement les sous-tâches réellement terminées.
- [x] Créer le commit de la tâche autonome terminée.

#### 2.8.2 Contrôle à la fin d’un bloc cohérent

- [x] Compiler les projets concernés ensemble.
- [x] Exécuter les groupes de tests concernés ensemble.
- [x] Contrôler les dépendances entre projets.
- [x] Vérifier que le comportement observable est conservé.
- [x] Pousser le bloc complet.

### 2.9 Clôturer réellement la phase 02

#### 2.9.1 Vérifier la structure finale

- [x] **Sans objet pour l’ancien plan** : Comparer l’arborescence obtenue à la structure cible validée. la proposition initiale a été supprimée ; contrôler la structure actuelle et les destinations des prochains blocs.
- [x] **Sans objet pour l’ancien plan** : Expliquer dans le document toute différence conservée volontairement. ne pas reconstruire une justification rétroactive de chaque écart à la proposition supprimée.
- [x] **Sans objet pour l’ancien plan** : Vérifier qu’aucun fichier de production n’a été oublié dans la table de déplacement. la table historique n’est plus à reconstruire (2.1.2).
- [ ] Vérifier que les racines des projets ne contiennent plus de fichiers mal rangés.
- [ ] Vérifier que les dossiers génériques ne sont plus des dossiers fourre-tout.
- [x] **Clos par décision utilisateur (2.2.3)** : le rôle actuel documenté de `MainWindow` est conservé ; aucune réduction supplémentaire n’est demandée.
- [x] **Clos par décision utilisateur (2.2.4)** : le rôle actuel documenté d’`OptionsWindow` est conservé ; aucune réduction supplémentaire n’est demandée.
- [x] Vérifier que `Images` est réellement structuré par responsabilités.
- [ ] Vérifier que les comportements multiformats sont toujours présents.

#### 2.9.2 Validation finale de la phase

- [ ] Compiler la solution complète une seule fois pour la clôture.
- [ ] Exécuter la suite de tests complète une seule fois pour la clôture.
- [ ] Corriger les régressions provoquées par le refactor.
- [ ] Refaire uniquement les contrôles concernés par une correction de clôture.
- [ ] Mettre à jour la documentation d’architecture avec la structure réellement obtenue.
- [ ] Vérifier que toutes les cases correspondent à un travail réellement terminé.
- [ ] Créer le commit final de documentation de la phase.
- [ ] Pousser le bloc complet de la phase 02.

### Résultat attendu

La phase 02 sera terminée uniquement lorsque :

- les déplacements encore nécessaires auront une structure cible préparée et validée avant leur exécution ;
- chaque fichier de production aura une destination et une responsabilité claires ;
- les rôles actuels de `MainWindow` et d’`OptionsWindow` seront conservés conformément aux décisions de 2.2.3 et 2.2.4 ;
- `GWGUI.App`, `GWGUI.Domain`, `GWGUI.Infrastructure` et `GWGUI.MediaEngine` seront rangés selon leurs responsabilités réelles ;
- les dossiers actuels auront des responsabilités claires, sans refaire les rangements déjà réalisés ;
- la détection automatique, la sélection manuelle et les disquettes multiformats conserveront leur comportement ;
- les contrôles auront été effectués par blocs rapides, puis une fois complètement à la clôture ;
- la documentation représentera exactement le code final.

## Phase 03 — Constantes et textes techniques

Les anciens noms `Images`, `ScpDetection` et `Interpretations` de la phase 02 désignent des étapes historiques ; l’organisation actuelle est décrite dans [l’architecture média](../architecture/media.md). Les anciennes étapes de préparation ne doivent pas entraîner le déplacement à nouveau du code déjà rangé ; la validation historique de la structure n’est pas attestée rétroactivement par cette revue.

### 3.1 Constantes

- [x] Inventorier toutes les valeurs fixes trouvées pendant l’audit.
- [ ] Créer les fichiers de constantes nécessaires pour chaque domaine identifié par l’audit.
- [ ] Séparer notamment identifiants de machines, formats, codecs, protections, systèmes de fichiers, conteneurs, extensions, commandes `gw`, géométries et contrôles d’intégrité.
- [ ] Remplacer les chaînes et nombres recopiés par ces définitions.
- [ ] Documenter la source des valeurs techniques non évidentes.
- [x] Intégrer dans les données embarquées toute définition spéciale de disquette nécessaire au produit.

Signalement issu de 2.3.8 et de la revue : la structure SCP dispose déjà de `Containers/Scp/ScpFormatConstants.cs`. Il reste notamment des extensions dans `ImageFormatWorkspace.FallbackImageExtensions`, des identifiants et extensions dans `CapabilityAwareImageFormatCatalog`, ainsi que des arguments `--device`, `--drive`, `--format` et noms de commandes dans les constructeurs Lecture/Écriture/Conversion. Les cases globales de constantes ne sont donc pas closes.

### 3.2 Aucun texte brut visible

- [x] Retirer tout libellé, message, infobulle, titre ou erreur visible écrit directement dans C# ou XAML.
- [x] Envoyer chaque texte utilisateur vers la ressource de traduction de son domaine.
- [x] Garder les noms techniques non traduisibles dans les catalogues neutres de `00-Base`.
- [x] Vérifier que le même nom technique n’est pas recopié dans trente langues s’il doit rester identique.
- [x] Distinguer messages de journaux techniques et messages destinés à l’utilisateur.

### 3.3 Contrôles

Le nettoyage des noms invariants est réalisé par `scripts/translate-resx-argos.py` (`INVARIANT_VALUE_PATTERN`, `INVARIANT_KEY_PATTERNS`, `audit_resources`) ; son audit réussi est consigné dans `interface/emulation/video-host-separation.md`. Les contrôles ci-dessous concernent les identifiants métier, constantes et textes du code, au-delà de cet audit des RESX.

- [ ] Ajouter un contrôle des identifiants utilisés mais absents des catalogues.
- [ ] Ajouter un contrôle des constantes dupliquées.
- [ ] Ajouter un contrôle des textes visibles codés en dur avec une liste blanche technique documentée.

## Phase 04 — Enums, modèles et contrats

### 4.1 Modèles de données

- [ ] Créer des DTO ou records distincts pour machine, format, géométrie, média, codec, protection, conteneur et système de fichiers.
- [x] Disposer de représentations distinctes pour les pistes (`ProtectedTrack`, `IPiste`), secteurs (`DecodedSector`), révolutions (`FluxRevolution`, `TrackFluxRevolution`) et avertissements (`IDiagnostic`).
- [ ] Vérifier si la représentation de l’intégrité répond entièrement au modèle distinct demandé ; `SectorIntegrityKind` et `IntegrityValid` existent déjà dans `DecodedSector`.
- [ ] Séparer les modèles publics partagés des structures internes propres à un algorithme.
- [x] Ajouter aux modèles les métadonnées nécessaires aux images multiformats et protégées.
- [ ] Définir les invariants et validations à la construction des données.

Signalement issu de 2.3.8 : `IImageDisquette.cs` contient encore deux contrats publics (`IImageDisquette`, `IMetadonneesImage`) et `ImageFormatModels.cs` deux records (`ImageExtension`, `DiskFormat`). `DecodedSector` copie les données, mais n’impose pas toutes les validations de tailles et de coordonnées. Les tâches globales de séparation et d’invariants restent ouvertes. Les métadonnées multiformats et protégées existent via `IImageDisquette.FormatsDetectes` et `DiskImageMetadata.SystemIds` / `ProtectionId`.

### 4.2 Enums et identifiants extensibles

- [ ] Utiliser des enums pour les ensembles fermés et stables : état d’intégrité, type de média, face, état d’opération et capacité.
- [ ] Utiliser des identifiants catalogués pour les formats, protections et définitions extensibles.
- [x] Vérifier que les formats provenant des `diskdefs` et autres définitions extensibles restent catalogués dynamiquement.

### 4.3 Interfaces

- [ ] Définir des contrats distincts pour lecture/écriture de conteneur, décodage/encodage de piste, reconstruction sectorielle, système de fichiers, détection, visualisation et conversion.
- [ ] Justifier chaque nouvelle interface par une frontière réelle ou par des implémentations interchangeables.
- [x] Vérifier les dépendances autorisées entre projets.

Les interfaces spécialisées du moteur existent déjà (`IScpReader`, `IScpWriter`, `IFluxDecoder`, `ITrackEncoder`, `IIsoScpSectorImagePolicy`, `IFileSystemReader`, `IDiskImageRecognitionPolicy`, `ISectorImageVisualizationPolicy`). La case globale conserve l’examen des frontières restantes, notamment la conversion ; elle ne demande pas de recréer ces interfaces. Les références `.csproj` ont été contrôlées : `Infrastructure` dépend du domaine ; Amiga/Atari dépendent d’Emulation et MediaEngine ; `VideoPresentation` reste indépendant des modules ; App compose ces bibliothèques. `ImageFormatWorkspace.AddDiskDefinitions` enrichit dynamiquement le catalogue.

## Phase 05 — Fonctions et services

- [ ] Relever les fonctions trop longues ou qui réalisent plusieurs opérations indépendantes.
- [ ] Extraire les fonctions communes dans le service propriétaire de la responsabilité, pas dans un fourre-tout `Helpers`.
- [ ] Séparer parsing, validation, transformation, sélection et présentation.
- [ ] Regrouper dans un même fichier les petites fonctions qui constituent une seule primitive cohérente.
- [ ] Isoler les accès disque, processus, réglages, journaux et dialogues derrière les services existants ou des contrats justifiés.
- [x] Réutiliser le même résultat d’analyse entre Explorateur et Visualisateur.
- [x] Annuler immédiatement l’analyse précédente lorsqu’une nouvelle image est chargée.
- [ ] Vérifier que chaque service a un propriétaire, une responsabilité et des tests ciblés.

Éléments déjà présents à réutiliser : `DiskImageWorkspaceController.LoadAsync` transmet le résultat de l’Explorateur au Visualisateur ; `DiskImageCancellationScope` annule et libère l’analyse précédente de chaque type. Les essais de changements rapides et de fermeture restent en 7.2. Des constantes, modèles et contrats spécialisés existent déjà dans le moteur média ; les cases globales des phases 03 et 04 restent ouvertes tant que leur couverture complète n’est pas établie.

## Phase 06 — Traductions

### 6.1 Arborescence

- [x] Organiser les ressources sous `Resources/00-Base` pour les catalogues neutres et sous un dossier par culture distribuée.
- [x] Séparer les ressources dans les 21 catalogues de textes actuels : `About`, `Actions`, `Advanced`, `Common`, `Conversion`, `Emulation`, `Errors`, `Explorer`, `ExplorerWarnings`, `Formats`, `Hardware`, `HostTools`, `Logs`, `Menus`, `Options`, `Profiles`, `Read`, `Shell`, `Tools`, `Visualizer` et `Write`, plus le catalogue neutre `Icons` (22 catalogues au total).
- [x] Fournir pour chaque catalogue de textes une ressource neutre et une ressource par culture distribuée ; conserver `Icons` uniquement dans `00-Base`.
- [x] Garder dans `Common` uniquement ce qui est réellement commun et basique.
- [ ] Placer une erreur spécialisée dans son catalogue fonctionnel ou dans `Errors` uniquement lorsqu’elle est réellement partagée.

`Common.resx` ne contient actuellement que `Common.Unknown` et `Common.Eject`. L’organisation est donc faite ; les contrôles restants concernent les erreurs spécialisées et l’application effective des langues.

### 6.2 Chargeur de ressources

- [x] Charger les catalogues actuels sans changer leurs clés.
- [x] Conserver le repli géré par `ResourceManager` vers la culture parente puis la ressource neutre.
- [x] Refuser les clés dupliquées entre catalogues lors de la construction de l’index.
- [ ] Vérifier le changement de langue immédiat dans toutes les fenêtres déjà ouvertes.
- [x] Vérifier que le sélecteur conserve le nom natif de chaque langue.

### 6.3 Vérification

- [x] **Sans objet sous sa forme initiale** : imposer des clés identiques dans toutes les langues. Les entrées techniques invariantes et les doublons anglais utilisent désormais le repli vers `00-Base`.
- [ ] Vérifier la couverture effective des traductions nécessaires, en tenant compte de ce repli.
- [x] Vérifier les placeholders, retours à la ligne et valeurs vides.
- [ ] Détecter les corruptions d’encodage et le mojibake.
- [ ] Vérifier les langues de droite à gauche.
- [ ] Vérifier séparément les traductions de l’application et celles de l’installateur.
- [ ] Vérifier que les listes de formats des cinq fonctions utilisent les mêmes noms du catalogue central.

Contrôle statique du 5 septembre 2026 : 631 fichiers RESX XML/UTF-8 analysés, aucun placeholder divergent, aucun nombre de retours à la ligne divergent, aucune valeur vide ni clé localisée inconnue. Aucun caractère de remplacement Unicode détecté ; cela ne suffit pas à exclure tout mojibake sémantique ni à valider visuellement les langues de droite à gauche. L’audit RESX antérieur (29 cultures, 22 catalogues) est également enregistré dans `interface/emulation/video-host-separation.md`. Les vérifications de couverture et d’installateur restent distinctes.

## Phase 07 — Interface, robustesse et maintenance

Ces tâches viennent après le refactor principal et avant la validation finale des images.

### 7.1 Interface

- [ ] Vérifier chaque fenêtre à taille normale, réduite, maximisée et avec plusieurs DPI.
- [ ] Vérifier qu’aucun contrôle ne dépasse de son cadre et qu’aucun texte n’est tronqué.
- [ ] Vérifier les défilements : visibles uniquement lorsque nécessaires et fonctionnement correct à la molette.
- [ ] Vérifier l’indépendance des composants réutilisés entre onglets.
- [ ] Vérifier les onglets, focus, survols, icônes, cadres, alignements et états désactivés.
- [ ] Reprendre le thème sombre plus tard, sans le mélanger au refactor fonctionnel.
- [ ] Vérifier la restauration de position et taille avant l’affichage de la fenêtre.
- [ ] Vérifier les silhouettes de disquettes et leur correspondance avec le média détecté.
- [ ] Vérifier les sélecteurs automatiques et manuels Machine, Format et Protection.

### 7.2 Performance

- [ ] Mesurer séparément conteneur, décodage, reconstruction, système de fichiers et rendu.
- [ ] Supprimer les analyses répétées inutiles.
- [ ] Mettre en cache uniquement les résultats immuables réutilisables.
- [ ] Afficher progressivement les données lorsque le traitement est long.
- [ ] Vérifier changement rapide d’image, annulation et fermeture pendant une analyse.
- [ ] Vérifier spécialement la détection multiformat sur les images de flux sans ralentir les images sectorielles ciblées.

### 7.3 Erreurs, journaux et persistance

- [x] Recenser les dialogues et états visibles qui présentaient le chemin du journal comme seule explication.
- [x] Centraliser l'interprétation des exceptions courantes sans exposer leur message technique non traduit.
- [x] Décrire les erreurs réseau, HTTP, disque, fichiers, permissions, données et état de l'application dans la langue sélectionnée.
- [x] Générer les 19 descriptions dans les 29 cultures distribuées avec Argos et vérifier leurs placeholders.
- [x] Tester en français le délai de connexion réseau et le fichier absent, sans mention du journal.
- [x] Terminer le groupe « descriptions localisées des erreurs ».
- [x] **Doublon de suivi, sans objet ici** : localiser les messages utilisateur et conserver le détail technique dans les journaux est suivi en 3.2 et dans le groupe « descriptions localisées des erreurs » ci-dessus.
- [ ] Remplacer la boîte d’erreur d’un format simplement non reconnu par l’état d’interface prévu.
- [ ] Vérifier un journal par action, rotation, archivage et ouverture du dossier Logs.
- [ ] Vérifier nettoyage des fichiers temporaires et partiels.
- [ ] Vérifier les réglages absents, anciens, partiels ou corrompus.
- [x] Ajouter les migrations existantes : `SettingsMigrator` (schéma 8) et migration des anciens profils vidéo hôtes ; cette dernière est validée dans `interface/emulation/video-host-separation.md`.
- [ ] Compléter la vérification des migrations générales de réglages et de leurs cas anciens, partiels ou corrompus.

Les tâches d’interface (7.1) et de performance (7.2) restent nécessaires : les builds et tests vidéo réussis ne prouvent ni tous les DPI, ni tous les parcours d’images, ni les essais matériels. Les services de journaux, de persistance et de migration sont déjà présents ; les cases ouvertes de 7.3 portent sur leurs validations restantes, pas sur leur recréation.

### 7.4 Documentation et crédits

- [x] **Sans objet comme tâche ponctuelle** : Maintenir les documents actuels après chaque bloc terminé. Obligation permanente, à appliquer lors de chaque changement concerné.
- [ ] Mettre à jour l’état du projet et la liste des formats réellement pris en charge.
- [x] **Sans objet comme tâche ponctuelle** : Maintenir les crédits, licences et liens des projets réellement utilisés ou étudiés. Obligation permanente, à appliquer lors de chaque changement concerné.

