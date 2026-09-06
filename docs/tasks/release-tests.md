# Plan des tests de release

## Cadre

Suite : `tests/GWGUI.Tests`. Exécution automatique dans le workflow GitHub de release existant, pour les releases et snapshots. Les tests doivent être rapides, déterministes et fonctionner sur son runner Windows sans afficher l’application ni demander une intervention humaine.

- Tester le code de GW GUI avec des entrées et résultats attendus explicites. Simuler les frontières externes, pas la fonction que le test doit vérifier.
- Utiliser de petits buffers, documents, volumes et événements synthétiques. Aucun corpus d’images réelles, ROM, matériel physique, `gw.exe` ou moteur d’émulation externe.
- Pour l’UI, instancier les vues en mémoire sur un contexte STA ; calculer leur disposition et contrôler propriétés, liaisons et actions. Aucun affichage de fenêtre, focus réel, injection clavier/souris ou rendu dépendant du bureau.
- Les assertions UI restent neutres : identifiants et clés de ressources, sans imposer une traduction ni une lettre de raccourci.
- Exercer réellement les calculs et transformations intégrés ; simuler les sorties audio, GPU et périphériques. Ne pas présenter leurs résultats simulés comme une validation du moteur externe.
- Isoler les cultures, ressources et données en mémoire ; piloter les attentes avec des signaux contrôlés. Tous les accès aux fichiers et dossiers sont simulés : aucun fichier de données réel à créer, lire, modifier ou supprimer, même temporaire. Cette règle couvre aussi les initialisations et le nettoyage.
- `GWGUI.LocalDiskImageTests` reste indépendant.

Les fonctionnalités de GW GUI restent réellement exercées sur ces simulations. Pour une API qui impose un chemin disque, isoler son accès aux fichiers avant de la tester ; ne pas contourner cette règle par un fichier temporaire.

## Organisation

| Niveau | Exemple | Élément correspondant |
|---|---|---|
| Groupe de groupes de tâches | **1 — Interface utilisateur** | Dossier `Interface/` |
| Groupe de tâches | **1.3 — Lecture** | Dossier `Interface/ReadViews/`, avec `ReadViewsTests.cs` qui regroupe les tests |
| Tâche | **1.3.1 — Choix du format** | Fichier `ReadFormatScenarios.cs` |
| Sous-tâche | **1.3.1.1 — Changer de mode** | Cas à traiter dans ce fichier |

Le fichier du groupe contient les cas xUnit découvrables individuellement et appelle les scénarios des fichiers de tâches. Les sous-tâches détaillent leurs vérifications ; elles ne créent pas de fichiers supplémentaires. Le nombre de tâches dépend de la fonctionnalité.

Une case cochée correspond à un travail réalisé et vérifié. Cocher une tâche lorsque ses sous-tâches sont terminées et ses tests exécutés. Le fichier du groupe possède sa propre case de réalisation.

Les règles communes sont testées dans leur groupe propriétaire. Une vue vérifie leur utilisation et leur présentation, sans répéter toutes les combinaisons métier.

## 1 — Interface utilisateur

Groupe de groupes de tâches — dossier `Interface/`.

Vérifier la disposition, les liaisons, les états et les actions des vues en mémoire. Les services métier appelés par les vues peuvent être simulés ; leurs règles sont testées dans les domaines suivants.

### 1.1 — Navigation, disposition et contrats des contrôles

Groupe de tâches — dossier `Interface/Navigation/`.

- [x] Fichier du groupe : `NavigationTests.cs`.

- [x] **1.1.1 — Vérifier les sections et le routage des actions** — `NavigationScenarios.cs`
  - [x] **1.1.1.1** — Sélectionner les sept onglets et vérifier la section attendue, les valeurs éditées et la conservation des vues initialement vides.
  - [x] **1.1.1.2** — Déclencher les événements des 13 commandes de menu ; contrôler l’action et l’émetteur, avec un appel unique.
  - [x] **1.1.1.3** — Vérifier le routage des préférences, de l’historique et des informations depuis MainWindow vers le service simulé.
  - [x] **1.1.1.4** — Contrôler les paramètres et le propriétaire transmis au présentateur simulé des dialogues ; préserver le contrat de retour existant et propager une erreur sans réessai.

- [x] **1.1.2 — Vérifier la disposition hors écran** — `WindowLayoutScenarios.cs`
  - [x] **1.1.2.1** — Mesurer et arranger les sept sections aux deux tailles de contenu prévues, puis vérifier les limites des en-têtes, du menu, de la barre d’état et des commandes principales.
  - [x] **1.1.2.2** — Contrôler les positions et tailles restaurées avec des géométries simulées : écran retiré, coordonnées négatives, petite zone disponible et valeurs invalides.

- [x] **1.1.3 — Vérifier les propriétés et liaisons utiles de l’interface** — `ControlContractScenarios.cs`
  - [x] **1.1.3.1** — Pour chaque langue du catalogue, vérifier que les liaisons des libellés de menu sont actives et produisent un texte résolu, sans imposer de traduction.
  - [x] **1.1.3.2** — Vérifier les identifiants d’automatisation, la participation déclarée au parcours clavier et la transmission unique de l’action des commandes principales Lecture/Écriture/Conversion.
  - [x] **1.1.3.3** — Vérifier que le champ de chemin expose la valeur et le nom accessible liés, reste en lecture seule et présente la commande Parcourir.

Validation acquise : 102 tests réussis en configuration Release, sans accès supplémentaire au bureau. La confirmation dans le workflow reste attendue à la prochaine release. Les propriétés de parcours clavier sont contrôlées ; le focus Windows et la modalité native ne sont pas couverts.

### 1.2 — Accessibilité des contrôles

Groupe de tâches — dossier `Interface/Accessibility/`.

- [x] Fichier du groupe : `AccessibilityTests.cs`.

- [x] **1.2.1 — Vérifier les informations accessibles** — `AccessibleControlsScenarios.cs`
  - [x] **1.2.1.1** — Sur les vues en mémoire, contrôler les noms et rôles accessibles des commandes visibles dans les différents états métier ; vérifier les identifiants techniques et les liaisons qui les alimentent.
  - [x] **1.2.1.2** — Contrôler les libellés, propriétés de participation au parcours clavier, disponibilités et événements des contrôles GW GUI. Ne pas injecter de touches ni tester les mécanismes de déplacement du focus de WPF.

### 1.3 — Lecture

Groupe de tâches — dossier `Interface/ReadViews/`.

- [x] Fichier du groupe : `ReadViewsTests.cs`.

- [x] **1.3.1 — Choisir le mode et le format de lecture** — `ReadFormatScenarios.cs`
  - [x] **1.3.1.1** — Passer entre flux brut et format reconnu ; vérifier les sélecteurs visibles et les choix compatibles.
  - [x] **1.3.1.2** — Modifier famille, format et extension ; vérifier la requête produite et la conservation des valeurs encore valides.

- [x] **1.3.2 — Configurer la destination et le nom** — `ReadDestinationScenarios.cs`
  - [x] **1.3.2.1** — Fournir dossier, nom et numérotation synthétiques ; contrôler aperçu et paramètres transmis. Vérifier que le nom reste littéral : la substitution des tags appartient à la conversion.
  - [x] **1.3.2.2** — Simuler sélection annulée, nom absent et conflit ; vérifier la décision de la vue avant exécution.

- [x] **1.3.3 — Présenter les états de lecture** — `ReadOperationScenarios.cs`
  - [x] **1.3.3.1** — Piloter le contrôleur avec une lecture simulée : attente, progression par face, réussite et erreur ; vérifier commandes, résumé et bannière.
  - [x] **1.3.3.2** — Demander annulation ou exécution répétée ; contrôler les appels et le retour à un état utilisable. Le coordinateur commun est couvert en 2.2.

### 1.4 — Écriture

Groupe de tâches — dossier `Interface/WriteViews/`.

- [x] Fichier du groupe : `WriteViewsTests.cs`.

- [x] **1.4.1 — Sélectionner et reconnaître la source** — `WriteSourceScenarios.cs`
  - [x] **1.4.1.1** — Injecter des documents synthétiques reconnus, ambigus et inconnus ; vérifier détection, formats proposés et informations affichées.
  - [x] **1.4.1.2** — Changer de source ou annuler sa sélection ; vérifier que la requête ne conserve pas des informations incompatibles.

- [x] **1.4.2 — Valider les options et la confirmation** — `WriteConfirmationScenarios.cs`
  - [x] **1.4.2.1** — Modifier pistes, faces et options avancées ; contrôler les valeurs transmises à la demande d’écriture.
  - [x] **1.4.2.2** — Simuler acceptation et refus de confirmation ; vérifier qu’un refus empêche toute demande d’écriture.

- [x] **1.4.3 — Présenter les états d’écriture** — `WriteOperationScenarios.cs`
  - [x] **1.4.3.1** — Injecter progression, vérification concordante ou divergente et erreur ; contrôler les états des commandes et le bilan affichable.
  - [x] **1.4.3.2** — Demander annulation ou exécution répétée ; vérifier que la vue cible l’opération courante et ne relance pas l’écriture.

### 1.5 — Conversion

Groupe de tâches — dossier `Interface/ConversionViews/`.

- [x] Fichier du groupe : `ConversionViewsTests.cs`.

- [x] **1.5.1 — Choisir source et destinations** — `ConversionSelectionScenarios.cs`
  - [x] **1.5.1.1** — Injecter une source synthétique et modifier formats de sortie ; vérifier les destinations proposées et les incompatibilités signalées.
  - [x] **1.5.1.2** — Modifier nom, extension et options ; comparer les paramètres transmis au plan de conversion attendu.

- [x] **1.5.2 — Traiter les conflits et confirmations** — `ConversionConflictScenarios.cs`
  - [x] **1.5.2.1** — Simuler les choix remplacer, ignorer et numéroter ; vérifier leur traduction dans la demande de conversion.
  - [x] **1.5.2.2** — Annuler une sélection ou refuser une confirmation ; vérifier l’absence de lancement et la conservation des saisies utiles.

- [x] **1.5.3 — Présenter les résultats de conversion** — `ConversionOperationScenarios.cs`
  - [x] **1.5.3.1** — Piloter progression, succès partiel et erreur d’un lot simulé ; vérifier les résultats par destination et le bilan.
  - [x] **1.5.3.2** — Annuler ou demander deux exécutions ; contrôler les appels de la vue. Les contenus convertis et l’algorithme de lot sont couverts en 4.5 et 2.2.

### 1.6 — Outils de maintenance

Groupe de tâches — dossier `Interface/MaintenanceViews/`.

- [x] Fichier du groupe : `MaintenanceViewsTests.cs`.

- [x] **1.6.1 — Choisir l’outil et ses paramètres** — `ToolSelectionScenarios.cs`
  - [x] **1.6.1.1** — Sélectionner effacement et nettoyage ; vérifier le panneau présenté, les champs applicables et les valeurs conservées.
  - [x] **1.6.1.2** — Fournir limites et valeurs invalides ; vérifier aperçu, messages et paramètres de la demande.

- [x] **1.6.2 — Confirmer l’arrêt d’une opération de maintenance** — `ToolConfirmationScenarios.cs`
  - [x] **1.6.2.1** — Refuser la confirmation d’arrêt : l’opération courante continue et aucun second lancement n’est demandé.
  - [x] **1.6.2.2** — Accepter l’arrêt : le jeton de l’opération courante est annulé et les commandes redeviennent utilisables. Le lancement initial d’effacement/nettoyage ne présente pas de confirmation dans le code actuel ; ses paramètres et son routage sont vérifiés en 1.6.3.

- [x] **1.6.3 — Présenter les états des outils** — `ToolOperationScenarios.cs`
  - [x] **1.6.3.1** — Injecter progression, succès et erreur ; vérifier commandes disponibles, compteurs et informations affichables.
  - [x] **1.6.3.2** — Demander arrêt et exécution répétée ; vérifier les demandes envoyées. Le protocole et les diagnostics métier sont couverts en 3.5.

### 1.7 — Visualisation

Groupe de tâches — dossier `Interface/VisualizerViews/`.

- [x] Fichier du groupe : `VisualizerViewsTests.cs`.

- [x] **1.7.1 — Présenter le document courant** — `VisualizerDocumentScenarios.cs`
  - [x] **1.7.1.1** — Injecter des documents synthétiques simple face, double face, ambigus et inconnus ; vérifier les sélections et l’état vide.
  - [x] **1.7.1.2** — Achever un ancien chargement après un nouveau ; vérifier que les informations de visualisation restent celles du document courant.

- [x] **1.7.2 — Inspecter une sélection** — `InspectorSelectionScenarios.cs`
  - [x] **1.7.2.1** — Sélectionner une piste sur chaque face ; contrôler les informations des révolutions, des structures et des secteurs calculées et liées à l’inspecteur. Les secteurs et révolutions sont des listes de détails, pas des commandes de sélection distinctes.
  - [x] **1.7.2.2** — Changer ou vider la sélection ; vérifier qu’aucune information du secteur précédent ne subsiste.

- [x] **1.7.3 — Calculer la présentation des vues** — `ViewportGeometryScenarios.cs`
  - [x] **1.7.3.1** — Exercer zoom, déplacement et liaison des vues avec des géométries synthétiques ; comparer transformations et limites.
  - [x] **1.7.3.2** — Réinitialiser ou changer de document ; vérifier les coordonnées et la disposition calculées hors écran.

### 1.8 — Explorateur de fichiers

Groupe de tâches — dossier `Interface/ExplorerViews/`.

- [x] Fichier du groupe : `ExplorerViewsTests.cs`.

- [x] **1.8.1 — Présenter volumes et formats** — `ExplorerDocumentScenarios.cs`
  - [x] **1.8.1.1** — Injecter des documents synthétiques avec plusieurs volumes ou sans format reconnu ; vérifier les choix et l’état vide.
  - [x] **1.8.1.2** — Changer de document ; vérifier que l’arbre et les informations correspondent au document courant, sans relancer les tests du chargeur.

- [x] **1.8.2 — Parcourir l’arbre de fichiers** — `ExplorerTreeScenarios.cs`
  - [x] **1.8.2.1** — Sélectionner volumes, dossiers et fichiers d’un arbre synthétique via la vue et son contrôleur.
  - [x] **1.8.2.2** — Contrôler chemin, métadonnées, dossier vide et retour au parent ; vérifier la conservation de la sélection attendue.

- [x] **1.8.3 — Présenter chargement et erreurs** — `ExplorerFailureScenarios.cs`
  - [x] **1.8.3.1** — Injecter attente, erreur de lecture et document indisponible ; contrôler l’état de chargement et les commandes disponibles.
  - [x] **1.8.3.2** — Vérifier qu’une erreur ne laisse pas visibles les données d’un autre fichier et qu’une nouvelle sélection reste possible.

### 1.9 — Réglages de l’application

Groupe de tâches — dossier `Interface/SettingsViews/`.

- [x] Fichier du groupe : `SettingsViewsTests.cs`.

- [x] **1.9.1 — Modifier les réglages** — `SettingsEditingScenarios.cs`
  - [x] **1.9.1.1** — Modifier des valeurs et déclencher les commandes applicatives ; comparer les réglages transmis au stockage simulé.
  - [x] **1.9.1.2** — Vérifier l’initialisation sans sauvegarde parasite, l’application automatique des modifications et l’annulation du choix de dossier. Les préférences ne possèdent pas de commandes globales Annuler ou Réinitialiser ; le retour au profil par défaut est couvert en 1.10 et le stockage en 2.3.

- [x] **1.9.2 — Valider les dépendances entre réglages** — `SettingsValidationScenarios.cs`
  - [x] **1.9.2.1** — Fournir valeurs hors limites et combinaisons incompatibles ; vérifier messages et disponibilité des commandes.
  - [x] **1.9.2.2** — Changer une option conditionnant d’autres champs ; contrôler leur état et la conservation des réglages des autres sections.

- [x] **1.9.3 — Traiter les erreurs de sauvegarde** — `SettingsFailureScenarios.cs`
  - [x] **1.9.3.1** — Simuler réussite et erreur de stockage ; vérifier le résultat présenté et les données encore disponibles dans la vue.
  - [x] **1.9.3.2** — Reprendre l’édition après une erreur ; vérifier les valeurs de la nouvelle demande sans écrire dans les réglages personnels.

### 1.10 — Édition des profils

Groupe de tâches — dossier `Interface/ProfileViews/`.

- [x] Fichier du groupe : `ProfileViewsTests.cs`.

- [x] **1.10.1 — Créer et modifier un profil** — `ProfileEditingScenarios.cs`
  - [x] **1.10.1.1** — Créer et renommer un profil au travers des commandes de la vue ; comparer les données transmises au service.
  - [x] **1.10.1.2** — Fournir un nom vide ou en conflit ; vérifier le refus et les données conservées.

- [x] **1.10.2 — Sélectionner et restaurer un profil** — `ProfileSelectionScenarios.cs`
  - [x] **1.10.2.1** — Changer de profil ; contrôler les valeurs présentées et l’identité du profil ciblé par les modifications.
  - [x] **1.10.2.2** — Réinitialiser le profil par défaut ; vérifier les valeurs et commandes résultantes sans dupliquer les règles de stockage de 2.4.

- [x] **1.10.3 — Supprimer un profil** — `ProfileDeletionScenarios.cs`
  - [x] **1.10.3.1** — Simuler acceptation et refus de suppression ; vérifier la demande et la liste résultante.
  - [x] **1.10.3.2** — Supprimer le profil sélectionné ou simuler une erreur ; contrôler la sélection de remplacement et les données encore éditables.

### 1.11 — Réglages des périphériques

Groupe de tâches — dossier `Interface/DeviceSettingsViews/`.

- [x] Fichier du groupe : `DeviceSettingsViewsTests.cs`.

- [x] **1.11.1 — Présenter les périphériques disponibles** — `DeviceListScenarios.cs`
  - [x] **1.11.1.1** — Injecter ajout, retrait, matériel connu et inconnu ; vérifier liste, noms et sélection.
  - [x] **1.11.1.2** — Vérifier les options proposées et les commandes disponibles lorsque le périphérique sélectionné disparaît.

- [x] **1.11.2 — Présenter les entrées simulées** — `DeviceInputPreviewScenarios.cs`
  - [x] **1.11.2.1** — Injecter axes et boutons synthétiques ; vérifier les valeurs et indicateurs des contrôles.
  - [x] **1.11.2.2** — Changer de périphérique ; vérifier que les informations et affectations concernent la nouvelle sélection.

- [x] **1.11.3 — Demander un retour périphérique** — `DeviceFeedbackScenarios.cs`
  - [x] **1.11.3.1** — Déclencher la commande de test de vibration et comparer cible, intensité et arrêt transmis au service simulé.
  - [x] **1.11.3.2** — Simuler indisponibilité et erreur ; vérifier commandes et messages sans ouvrir de périphérique physique.

### 1.12 — Interface d’émulation

Groupe de tâches — dossier `Interface/EmulationViews/`.

- [x] Fichier du groupe : `EmulationViewsTests.cs`.

- [x] **1.12.1 — Vérifier les onglets de machines** — `MachineTabsScenarios.cs`
  - [x] **1.12.1.1** — Créer plusieurs sessions simulées, changer d’onglet, fermer une session et vérifier la session ciblée par chaque commande.
  - [x] **1.12.1.2** — Vérifier les états démarrée, arrêtée, en pause et en erreur ainsi que leurs actions disponibles.

- [x] **1.12.2 — Vérifier les formulaires de configuration** — `MachineConfigurationScenarios.cs`
  - [x] **1.12.2.1** — Vérifier les catégories et options proposées à partir des capacités d’un faux module.
  - [x] **1.12.2.2** — Vérifier sauvegarde, abandon du brouillon et erreurs sans démarrer de cœur d’émulation.

- [x] **1.12.3 — Vérifier les décisions de routage et de présentation** — `EmulationInteractionScenarios.cs`
  - [x] **1.12.3.1** — Injecter activation/désactivation de session et demandes de commandes ; vérifier les requêtes de capture/libération transmises aux services simulés. Les événements remplacent le focus réel et les entrées Windows.
  - [x] **1.12.3.2** — Injecter de petites frames et des statuts de média/audio/vidéo ; vérifier que les commandes et réglages visent la bonne session.

### 1.13 — Localisation des vues

Groupe de tâches — dossier `Interface/Localization/`.

- [x] Fichier du groupe : `LocalizationTests.cs`.

- [x] **1.13.1 — Vérifier les vues localisées** — `LocalizedViewsScenarios.cs`
  - [x] **1.13.1.1** — Parcourir les cultures du catalogue avec les ressources intégrées : résolution des clés, substitutions et mise à jour des liaisons lors du changement de langue, sans texte traduit codé en dur dans les assertions.
  - [x] **1.13.1.2** — Mesurer et arranger les vues avec textes longs et dispositions droite-à-gauche ; vérifier les zones disponibles, le retour à la ligne et l’accès aux commandes. Observer les tailles et propriétés calculées, sans capture ni affichage.

### 1.14 — Thèmes de l’interface

Groupe de tâches — dossier `Interface/Themes/`.

- [x] Fichier du groupe : `ThemesTests.cs`.

- [x] **1.14.1 — Appliquer les ressources du thème** — `ThemeResourcesScenarios.cs`
  - [x] **1.14.1.1** — Appliquer les thèmes aux ressources en mémoire et vérifier couleurs, styles et états des commandes. Simuler le choix du thème système ; ne pas appeler le gestionnaire de fenêtres natif pour peindre une fenêtre.
  - [x] **1.14.1.2** — Changer de thème sur une vue déjà construite ; vérifier que les ressources dynamiques se mettent à jour et que les styles nécessaires restent résolus.

### 1.15 — Aide et documentation

Groupe de tâches — dossier `Interface/Help/`.

- [x] Fichier du groupe : `HelpTests.cs`.

- [x] **1.15.1 — Choisir la page du wiki** — `HelpTargetScenarios.cs`
  - [x] **1.15.1.1** — Vérifier la cible Wiki pour chaque langue, les alias chinois et le repli anglais ; intercepter l’ouverture du navigateur.
  - [x] **1.15.1.2** — Déclencher la commande Documentation depuis chacun des sept onglets avec un navigateur simulé ; vérifier le guide de la langue courante, le traitement d’une erreur et la reprise. Cette commande ouvre le guide général ; elle ne possède pas de page distincte par onglet.

## 2 — Fonctions communes de l’application

Groupe de groupes de tâches — dossier `Application/`.

Vérifier les services partagés indépendamment des écrans : opérations, réglages, profils, sorties et intégrations techniques.

### 2.1 — Socle des tests rapides

Groupe de tâches — dossier `Application/TestInfrastructure/`.

- [x] Fichier du groupe : `TestInfrastructureTests.cs`.

- [x] **2.1.1 — Préparer l’exécution WPF isolée** — `StaExecutionScenarios.cs`
  - [x] **2.1.1.1** — Fournir un contexte STA et un Dispatcher bornés pour les vues WPF en mémoire, avec les ressources partagées, sans démarrage de l’application ni handle de fenêtre natif. Utilisé et vérifié par 1.1.
  - [x] **2.1.1.2** — Contrôler nettoyage et propagation des exceptions du support ; sérialiser Application, cultures et thèmes. Ne pas construire de scénarios de validation de WPF lui-même.

- [x] **2.1.2 — Préparer les simulations communes** — `ControlledDependencies.cs`
  - [x] **2.1.2.1** — Fournir des doubles ciblés pour horloge, présentation des dialogues, transport, processus, HTTP et périphériques selon les interfaces effectivement utilisées. Les dialogues enregistrent requête et propriétaire et renvoient réponse/erreur sans afficher de fenêtre.
  - [x] **2.1.2.2** — Permettre succès, attente contrôlée, erreur et annulation ; enregistrer les appels et arguments sans réimplémenter le métier testé.

- [x] **2.1.3 — Préparer les données synthétiques** — `SyntheticData.cs`
  - [x] **2.1.3.1** — Construire de petits documents, buffers, arbres de fichiers, frames et paquets de protocole dont le résultat attendu est fixé indépendamment.
  - [x] **2.1.3.2** — Fournir un système de fichiers simulé en mémoire : chemins, contenus, existence, création, remplacement et suppression. Enregistrer les appels et injecter les erreurs sans accéder au disque.

### 2.2 — Exécution, concurrence et annulation

Groupe de tâches — dossier `Application/OperationLifecycle/`.

- [x] Fichier du groupe : `OperationLifecycleTests.cs`.

- [x] **2.2.1 — Vérifier le cycle d’une opération** — `OperationStateScenarios.cs`
  - [x] **2.2.1.1** — Contrôler les transitions attente/exécution/fin, la valeur retournée et la propagation d’erreur.
  - [x] **2.2.1.2** — Refuser une opération concurrente lorsque le coordinateur est occupé ; autoriser une nouvelle opération après succès ou échec.

- [x] **2.2.2 — Vérifier l’annulation** — `CancellationScenarios.cs`
  - [x] **2.2.2.1** — Contrôler la propagation du jeton et les annulations avant lancement, pendant l’attente et en fin d’opération.
  - [x] **2.2.2.2** — Vérifier libération, signalement de fin et nettoyage sans temporisation réelle ni boucle d’attente arbitraire.

- [x] **2.2.3 — Vérifier les opérations par lot** — `BatchExecutionScenarios.cs`
  - [x] **2.2.3.1** — Contrôler l’ordre, les résultats individuels et le bilan global d’un lot avec succès et échecs mélangés.
  - [x] **2.2.3.2** — Vérifier la politique réelle de poursuite et d’annulation, et qu’aucun élément n’est exécuté deux fois.

### 2.3 — Réglages et stockage

Groupe de tâches — dossier `Application/SettingsStorage/`.

- [x] Fichier du groupe : `SettingsStorageTests.cs`.

- [x] **2.3.1 — Vérifier lecture et écriture des réglages** — `SettingsRoundTripScenarios.cs`
  - [x] **2.3.1.1** — Exercer la sérialisation et la désérialisation des réglages sur des chaînes ou flux en mémoire ; vérifier le contenu produit, les valeurs relues et les valeurs par défaut avec un stockage simulé.
  - [x] **2.3.1.2** — Vérifier document absent, JSON invalide et champs manquants sans effacer les valeurs récupérables.

- [x] **2.3.2 — Vérifier migration et récupération** — `MigrationAndRecoveryScenarios.cs`
  - [x] **2.3.2.1** — Exercer les anciennes structures réellement prises en charge et vérifier le résultat migré indépendamment du migrateur.
  - [x] **2.3.2.2** — Simuler une écriture interrompue ou un accès refusé ; vérifier le contenu conservé en mémoire et les demandes de remplacement ou de suppression transmises au système de fichiers simulé.

- [x] **2.3.3 — Vérifier les emplacements de données** — `StorageLocationScenarios.cs`
  - [x] **2.3.3.1** — Exercer modes portable et installé à partir d’emplacements simulés ; vérifier les chemins calculés.
  - [x] **2.3.3.2** — Simuler tous les accès disque, y compris existence et énumération : aucun réglage, profil, journal ou fichier temporaire réel ne doit être utilisé.

### 2.4 — Gestion des profils

Groupe de tâches — dossier `Application/Profiles/`.

- [x] Fichier du groupe : `ProfilesTests.cs`.

- [x] **2.4.1 — Vérifier les données des profils** — `ProfileStateScenarios.cs`
  - [x] **2.4.1.1** — Contrôler création, modification, sélection et restauration des profils d’opération.
  - [x] **2.4.1.2** — Vérifier les champs persistants et ceux de session qui ne doivent pas entrer dans le profil.

### 2.5 — Gestion des processus externes

Groupe de tâches — dossier `Application/ExternalProcesses/`.

- [x] Fichier du groupe : `ExternalProcessesTests.cs`.

- [x] **2.5.1 — Vérifier notre gestion d’un processus** — `ProcessBoundaryScenarios.cs`
  - [x] **2.5.1.1** — Simuler sortie standard, erreur, code de retour, arrêt et processus ne répondant plus.
  - [x] **2.5.1.2** — Vérifier les décisions de GW GUI, les arguments et la libération des ressources ; aucun processus gw.exe ou émulateur réel.

### 2.6 — Noms de sortie et conflits

Groupe de tâches — dossier `Application/OutputNaming/`.

- [x] Fichier du groupe : `OutputNamingTests.cs`.

- [x] **2.6.1 — Vérifier noms, compteurs et tags** — `SequenceAndTagsScenarios.cs`
  - [x] **2.6.1.1** — Tester les limites des compteurs numériques et alphabétiques, les formats de dates et les substitutions de tags.
  - [x] **2.6.1.2** — Contrôler que l’incrémentation et le nom final correspondent au résultat effectif de l’opération.

- [x] **2.6.2 — Vérifier les décisions de conflit** — `OutputConflictScenarios.cs`
  - [x] **2.6.2.1** — Simuler des noms existants et contrôler ignorer, remplacer ou chercher le prochain nom disponible.
  - [x] **2.6.2.2** — Vérifier les conflits multiples et l’absence d’écrasement lorsque la décision utilisateur l’interdit.

### 2.7 — Acquisition des composants

Groupe de tâches — dossier `Application/ComponentAcquisition/`.

- [x] Fichier du groupe : `ComponentAcquisitionTests.cs`.

- [x] **2.7.1 — Vérifier notre gestion des téléchargements** — `ComponentAcquisitionScenarios.cs`
  - [x] **2.7.1.1** — Simuler réponse HTTP, flux de téléchargement en mémoire, progression et installation dans une arborescence virtuelle ; vérifier les contenus et opérations demandées sans créer de répertoire réel.
  - [x] **2.7.1.2** — Vérifier échec, annulation, fichier incomplet et sélection de version ; aucun réseau, téléchargement de cœur ou chargement de DLL externe.

### 2.8 — Décisions du lanceur

Groupe de tâches — dossier `Application/Launcher/`.

- [x] Fichier du groupe : `LauncherTests.cs`.

- [x] **2.8.1 — Vérifier les décisions du lanceur** — `LauncherDecisionScenarios.cs`
  - [x] **2.8.1.1** — Exercer la résolution du binaire et les arguments transmis à partir d’une arborescence synthétique.
  - [x] **2.8.1.2** — Vérifier fichier absent et erreur de lancement en interceptant le démarrage ; tester notre logique sans lancer un programme tiers.

## 3 — Opérations matérielles de GW GUI

Groupe de groupes de tâches — dossier `Hardware/`.

Vérifier nos commandes, notre protocole et nos traitements de lecture, écriture et maintenance avec un matériel et des échanges simulés.

### 3.1 — Construction des commandes et interprétation des sorties

Groupe de tâches — dossier `Hardware/CommandsAndParsing/`.

- [x] Fichier du groupe : `CommandsAndParsingTests.cs`.

- [x] **3.1.1 — Vérifier la construction des arguments** — `CommandArgumentsScenarios.cs`
  - [x] **3.1.1.1** — Contrôler les chemins avec espaces, guillemets, options activées/désactivées et arguments experts.
  - [x] **3.1.1.2** — Vérifier valeurs invalides, options incompatibles et séparation entre nom de fichier et arguments ; ne jamais lancer gw.exe.

- [x] **3.1.2 — Vérifier les parseurs GW GUI** — `ExternalOutputParsingScenarios.cs`
  - [x] **3.1.2.1** — Fournir des chaînes synthétiques d’informations matériel, capacités et progression ; vérifier les objets métier obtenus.
  - [x] **3.1.2.2** — Tester sorties partielles, inconnues et malformées sans déduire la réussite d’un texte incomplet.

- [x] **3.1.3 — Vérifier progression et journaux** — `ProgressAndLogsScenarios.cs`
  - [x] **3.1.3.1** — Contrôler les bornes et l’affectation de la progression aux faces et opérations.
  - [x] **3.1.3.2** — Vérifier description d’erreurs, journalisation et rotation sur de petits contenus, sans masquer l’erreur d’origine.

### 3.2 — Sélection et identité du matériel

Groupe de tâches — dossier `Hardware/HardwareSelection/`.

- [x] Fichier du groupe : `HardwareSelectionTests.cs`.

- [x] **3.2.1 — Vérifier les résultats de découverte** — `DeviceDiscoveryScenarios.cs`
  - [x] **3.2.1.1** — Injecter plusieurs périphériques et ports, connus ou inconnus, et vérifier leur classement et identité.
  - [x] **3.2.1.2** — Exercer ajout, retrait, doublon et absence de matériel sans consulter les ports ou le registre du poste.

- [x] **3.2.2 — Vérifier le routage vers un lecteur** — `DriveRoutingScenarios.cs`
  - [x] **3.2.2.1** — Contrôler la résolution du périphérique et du lecteur pour chaque requête.
  - [x] **3.2.2.2** — Vérifier sélection devenue invalide, lecteur absent et plusieurs lecteurs sans réutiliser silencieusement un ancien choix.

- [x] **3.2.3 — Vérifier la découverte au démarrage** — `HardwareRefreshScenarios.cs`
  - [x] **3.2.3.1** — Maintenir une découverte en attente puis fournir son résultat ; vérifier la fusion des identités connues, les nouveaux périphériques, les absents et la sauvegarde. Une annulation conserve les données précédentes.
  - [x] **3.2.3.2** — Vérifier le cas où l’outil configuré est absent, avec et sans matériel mémorisé. Le contrôle Greaseweazle est ponctuel et les reprises du contrôleur de démarrage sont séquentielles : il n’existe pas de suivi permanent ni d’abonnement à arrêter ici. Le suivi des manettes relève de 1.11.

### 3.3 — Lecture physique côté GW GUI

Groupe de tâches — dossier `Hardware/PhysicalReading/`.

- [x] Fichier du groupe : `PhysicalReadingTests.cs`.

- [x] **3.3.1 — Vérifier le plan de lecture** — `ReadPlanningScenarios.cs`
  - [x] **3.3.1.1** — Contrôler pistes, faces, révolutions, géométrie et choix du moteur à partir d’une requête synthétique.
  - [x] **3.3.1.2** — Vérifier paramètres impossibles, matériel absent et refus d’un format connu par le moteur interne, qui accepte uniquement la capture SCP brute, avant tout échange avec le périphérique simulé.

- [x] **3.3.2 — Vérifier le traitement des données reçues** — `ReadAcquisitionScenarios.cs`
  - [x] **3.3.2.1** — Fournir de petits flux déterministes par un faux IGreaseweazleReadDevice ; exercer le vrai traitement GW GUI.
  - [x] **3.3.2.2** — Contrôler assemblage des flux, index réel/simulé et secteurs durs, nombre de tentatives, progression et résultat produit. La validité des secteurs décodés relève du groupe 4.3.

- [x] **3.3.3 — Vérifier les échecs de lecture** — `ReadFailureScenarios.cs`
  - [x] **3.3.3.1** — Simuler déconnexion, réponse invalide et annulation au milieu d’une piste.
  - [x] **3.3.3.2** — Vérifier la fermeture du périphérique simulé, les pistes déjà transmises par la progression et leurs compteurs après une erreur. Ce service retourne une image en mémoire ; l’écriture SCP est couverte en 4.2.17 et la gestion de la destination en 1.4.

### 3.4 — Écriture physique côté GW GUI

Groupe de tâches — dossier `Hardware/PhysicalWriting/`.

- [x] Fichier du groupe : `PhysicalWritingTests.cs`.

- [x] **3.4.1 — Vérifier le plan d’écriture** — `WritePlanningScenarios.cs`
  - [x] **3.4.1.1** — Créer une représentation synthétique minimale et contrôler ordre des pistes, faces, options et routage.
  - [x] **3.4.1.2** — Vérifier que des paramètres incompatibles empêchent l’envoi avant toute modification simulée.

- [x] **3.4.2 — Vérifier écriture et relecture** — `WriteVerificationScenarios.cs`
  - [x] **3.4.2.1** — Enregistrer les données envoyées au faux périphérique et les comparer aux valeurs attendues.
  - [x] **3.4.2.2** — Simuler vérification concordante ou divergente ; contrôler erreurs, tentatives et bilan.

- [x] **3.4.3 — Vérifier l’interruption** — `WriteCancellationScenarios.cs`
  - [x] **3.4.3.1** — Simuler refus de confirmation, déconnexion et annulation pendant l’écriture.
  - [x] **3.4.3.2** — Vérifier qu’aucune piste suivante n’est écrite et que les ressources sont libérées.

### 3.5 — Outils et diagnostics matériel côté GW GUI

Groupe de tâches — dossier `Hardware/Maintenance/`.

- [x] Fichier du groupe : `MaintenanceTests.cs`.

- [x] **3.5.1 — Vérifier les demandes d’outils** — `MaintenanceRequestScenarios.cs`
  - [x] **3.5.1.1** — Contrôler les demandes d’effacement, nettoyage, déplacement, vitesse, broches et temporisations exposées par le code.
  - [x] **3.5.1.2** — Vérifier les validations et confirmations ; observer seulement les appels au transport ou à l’exécuteur simulé.

- [x] **3.5.2 — Vérifier les résultats des diagnostics** — `DiagnosticResultScenarios.cs`
  - [x] **3.5.2.1** — Injecter mesures, informations et erreurs ; vérifier les valeurs affichables et les états retournés.
  - [x] **3.5.2.2** — Exercer réponse vide, valeur hors plage et périphérique disparu.

### 3.6 — Protocole implémenté dans GW GUI

Groupe de tâches — dossier `Hardware/GreaseweazleProtocol/`.

- [x] Fichier du groupe : `GreaseweazleProtocolTests.cs`.

- [x] **3.6.1 — Vérifier les échanges du protocole** — `ProtocolFramesScenarios.cs`
  - [x] **3.6.1.1** — Utiliser IGreaseweazleSerialTransport simulé pour vérifier les octets émis et les acquittements interprétés.
  - [x] **3.6.1.2** — Exercer réponses fragmentées, longueur invalide, code d’erreur et annulation sans port série réel.

- [x] **3.6.2 — Vérifier codage et décodage des flux** — `FluxEncodingScenarios.cs`
  - [x] **3.6.2.1** — Utiliser de petits vecteurs d’octets et d’intervalles dont les valeurs attendues sont écrites indépendamment.
  - [x] **3.6.2.2** — Tester limites, opcodes, index et troncatures ; compléter les allers-retours par des vecteurs attendus.

- [x] **3.6.3 — Vérifier index et révolutions** — `RotationNormalizationScenarios.cs`
  - [x] **3.6.3.1** — Exercer un nombre réduit de révolutions, index manquant et intervalle limite.
  - [x] **3.6.3.2** — Contrôler les bornes, la géométrie résultante et les erreurs sans temps d’attente matériel.

### 3.7 — Mise à jour du firmware côté GW GUI

Groupe de tâches — dossier `Hardware/FirmwareUpdate/`.

- [x] Fichier du groupe : `FirmwareUpdateTests.cs`.

- [x] **3.7.1 — Vérifier notre orchestration du firmware** — `FirmwareUpdateScenarios.cs`
  - [x] **3.7.1.1** — Simuler refus, succès, annulation et erreur de la commande de mise à jour. Le parcours actuel ne propose pas de fichier à sélectionner ; les lignes de progression communes sont couvertes en 2.5.
  - [x] **3.7.1.2** — Vérifier confirmations et arrêt du parcours au bon moment ; aucune mise à jour réelle et aucune exécution de gw.exe.

## 4 — Traitement des médias

Groupe de groupes de tâches — dossier `Media/`.

Exercer les lecteurs, écrivains, codecs, systèmes de fichiers et conversions intégrés avec des buffers et flux en mémoire. Les volumes et fichiers représentés sont virtuels ; aucun accès au système de fichiers du runner.

### 4.1 — Reconnaissance et compatibilité des images

Groupe de tâches — dossier `Media/ImageRecognition/`.

- [x] Fichier du groupe : `ImageRecognitionTests.cs`.

- [x] **4.1.1 — Vérifier les preuves de reconnaissance** — `RecognitionEvidenceScenarios.cs`
  - [x] **4.1.1.1** — Fournir des en-têtes, tailles, métadonnées et secteurs synthétiques aux véritables règles de reconnaissance.
  - [x] **4.1.1.2** — Vérifier confiance, absence de preuve et contradiction entre extension et contenu.

- [x] **4.1.2 — Vérifier les reconnaissances multiples** — `MultipleFormatsScenarios.cs`
  - [x] **4.1.2.1** — Construire un petit cas présentant plusieurs indices valides et vérifier leur conservation.
  - [x] **4.1.2.2** — Exercer le choix manuel et la détection automatique selon le contrat existant, sans écraser les autres résultats reconnus.

- [x] **4.1.3 — Vérifier les opérations compatibles** — `CapabilitiesScenarios.cs`
  - [x] **4.1.3.1** — Contrôler les sorties proposées selon format, géométrie et capacités synthétiques du moteur.
  - [x] **4.1.3.2** — Vérifier qu’un changement de source retire les choix incompatibles et remet la détection à vide si nécessaire.

### 4.2 — Conteneurs et lecture/écriture d’images synthétiques

Groupe de tâches — dossier `Media/ImageContainers/`.

- [x] Fichier du groupe : `ImageContainersTests.cs`.

- [x] **4.2.1 — Vérifier les conteneurs Acorn** — `AcornContainerScenarios.cs`
  - [x] **4.2.1.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Acorn, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.1.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.2 — Vérifier les conteneurs Adf** — `AdfContainerScenarios.cs`
  - [x] **4.2.2.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Adf, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.2.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.3 — Vérifier les conteneurs Amstrad** — `AmstradContainerScenarios.cs`
  - [x] **4.2.3.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Amstrad, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.3.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.4 — Vérifier les conteneurs Apple** — `AppleContainerScenarios.cs`
  - [x] **4.2.4.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Apple, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.4.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.5 — Vérifier les conteneurs Atari** — `AtariContainerScenarios.cs`
  - [x] **4.2.5.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Atari, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.5.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.6 — Vérifier les conteneurs Coherent** — `CoherentContainerScenarios.cs`
  - [x] **4.2.6.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Coherent, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.6.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.7 — Vérifier les conteneurs Commodore** — `CommodoreContainerScenarios.cs`
  - [x] **4.2.7.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Commodore, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.7.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.8 — Vérifier les conteneurs Cp2** — `Cp2ContainerScenarios.cs`
  - [x] **4.2.8.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Cp2, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.8.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.9 — Vérifier les conteneurs Dec** — `DecContainerScenarios.cs`
  - [x] **4.2.9.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Dec, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.9.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.10 — Vérifier les conteneurs Epson** — `EpsonContainerScenarios.cs`
  - [x] **4.2.10.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Epson, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.10.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.11 — Vérifier les conteneurs Hfe** — `HfeContainerScenarios.cs`
  - [x] **4.2.11.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Hfe, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.11.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.12 — Vérifier les conteneurs I86f** — `I86fContainerScenarios.cs`
  - [x] **4.2.12.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/I86f, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.12.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.13 — Vérifier les conteneurs Ibm** — `IbmContainerScenarios.cs`
  - [x] **4.2.13.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Ibm, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.13.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.14 — Vérifier les conteneurs ImageDisk** — `ImageDiskContainerScenarios.cs`
  - [x] **4.2.14.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/ImageDisk, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.14.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.15 — Vérifier les conteneurs Msx** — `MsxContainerScenarios.cs`
  - [x] **4.2.15.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Msx, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.15.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.16 — Vérifier les conteneurs Raw** — `RawContainerScenarios.cs`
  - [x] **4.2.16.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Raw, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.16.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.17 — Vérifier les conteneurs Scp** — `ScpContainerScenarios.cs`
  - [x] **4.2.17.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Scp, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.17.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.18 — Vérifier les conteneurs TeleDisk** — `TeleDiskContainerScenarios.cs`
  - [x] **4.2.18.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/TeleDisk, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.18.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

- [x] **4.2.19 — Vérifier les conteneurs Ucsd** — `UcsdContainerScenarios.cs`
  - [x] **4.2.19.1** — Lister les lecteurs, écrivains et variantes réellement implémentés dans Containers/Ucsd, puis associer chaque contrat distinct à un cas nommé : petit buffer valide et résultat attendu indépendant.
  - [x] **4.2.19.2** — Tester signature, taille, offsets, données tronquées et limites applicables ; pour chaque écrivain existant, vérifier les octets et métadonnées produits, avec annulation si l’API la permet. Aucun fichier image réel.

### 4.3 — Encodage, décodage et reconstruction

Groupe de tâches — dossier `Media/MediaCodecs/`.

- [x] Fichier du groupe : `MediaCodecsTests.cs`.

- [x] **4.3.1 — Vérifier les primitives binaires** — `BitAndChecksumScenarios.cs`
  - [x] **4.3.1.1** — Contrôler packing, ordre des bits et CRC/checksums à partir de vecteurs connus calculés indépendamment du code testé.
  - [x] **4.3.1.2** — Tester zéro, valeur maximale et frontière d’octet sans mutualiser des algorithmes différents uniquement parce que leurs noms se ressemblent.

- [x] **4.3.2 — Vérifier les codecs intégrés** — `TrackCodecScenarios.cs`
  - [x] **4.3.2.1** — Décliner des cas courts pour les codecs FM, MFM, GCR et variantes présents dans les registres.
  - [x] **4.3.2.2** — Vérifier marques, synchronisation, données et erreurs d’intégrité avec quelques secteurs ; ne pas produire de disquette complète inutilement.

- [x] **4.3.3 — Vérifier la reconstruction** — `SectorReconstructionScenarios.cs`
  - [x] **4.3.3.1** — Fournir des candidats synthétiques sur quelques révolutions : manquant, doublon, valide et défectueux.
  - [x] **4.3.3.2** — Contrôler sélection, ordre, statut et contenu des secteurs pour les règles communes et les variantes spécialisées présentes.

### 4.4 — Systèmes de fichiers intégrés

Groupe de tâches — dossier `Media/FileSystems/`.

- [x] Fichier du groupe : `FileSystemsTests.cs`.

- [x] **4.4.1 — Vérifier les systèmes de fichiers Acorn** — `AcornFileSystemScenarios.cs`
  - [x] **4.4.1.1** — Recenser les variantes implémentées dans FileSystems/Acorn ; construire les plus petits volumes synthétiques nécessaires et vérifier nom, répertoires, métadonnées et octets des fichiers, y compris vide et fragmenté.
  - [x] **4.4.1.2** — Tester chaîne de blocs en boucle, références hors volume, troncatures et espace insuffisant selon le format ; exercer les écritures réellement disponibles avec des résultats attendus indépendants.

- [x] **4.4.2 — Vérifier les systèmes de fichiers Amiga** — `AmigaFileSystemScenarios.cs`
  - [x] **4.4.2.1** — Recenser les variantes implémentées dans FileSystems/Amiga ; construire les plus petits volumes synthétiques nécessaires et vérifier nom, répertoires, métadonnées et octets des fichiers, y compris vide et fragmenté.
  - [x] **4.4.2.2** — Tester chaîne de blocs en boucle, références hors volume, troncatures et espace insuffisant selon le format ; exercer les écritures réellement disponibles avec des résultats attendus indépendants.

- [x] **4.4.3 — Vérifier les systèmes de fichiers Apple** — `AppleFileSystemScenarios.cs`
  - [x] **4.4.3.1** — Recenser les variantes implémentées dans FileSystems/Apple ; construire les plus petits volumes synthétiques nécessaires et vérifier nom, répertoires, métadonnées et octets des fichiers, y compris vide et fragmenté.
  - [x] **4.4.3.2** — Tester chaîne de blocs en boucle, références hors volume, troncatures et espace insuffisant selon le format ; exercer les écritures réellement disponibles avec des résultats attendus indépendants.

- [x] **4.4.4 — Vérifier les systèmes de fichiers Atari** — `AtariFileSystemScenarios.cs`
  - [x] **4.4.4.1** — Recenser les variantes implémentées dans FileSystems/Atari ; construire les plus petits volumes synthétiques nécessaires et vérifier nom, répertoires, métadonnées et octets des fichiers, y compris vide et fragmenté.
  - [x] **4.4.4.2** — Tester chaîne de blocs en boucle, références hors volume, troncatures et espace insuffisant selon le format ; exercer les écritures réellement disponibles avec des résultats attendus indépendants.

- [x] **4.4.5 — Vérifier les systèmes de fichiers Coherent** — `CoherentFileSystemScenarios.cs`
  - [x] **4.4.5.1** — Recenser les variantes implémentées dans FileSystems/Coherent ; construire les plus petits volumes synthétiques nécessaires et vérifier nom, répertoires, métadonnées et octets des fichiers, y compris vide et fragmenté.
  - [x] **4.4.5.2** — Tester chaîne de blocs en boucle, références hors volume, troncatures et espace insuffisant selon le format ; exercer les écritures réellement disponibles avec des résultats attendus indépendants.

- [x] **4.4.6 — Vérifier les systèmes de fichiers Commodore** — `CommodoreFileSystemScenarios.cs`
  - [x] **4.4.6.1** — Recenser les variantes implémentées dans FileSystems/Commodore ; construire les plus petits volumes synthétiques nécessaires et vérifier nom, répertoires, métadonnées et octets des fichiers, y compris vide et fragmenté.
  - [x] **4.4.6.2** — Tester chaîne de blocs en boucle, références hors volume, troncatures et espace insuffisant selon le format ; exercer les écritures réellement disponibles avec des résultats attendus indépendants.

- [x] **4.4.7 — Vérifier les systèmes de fichiers Cpm** — `CpmFileSystemScenarios.cs`
  - [x] **4.4.7.1** — Recenser les variantes implémentées dans FileSystems/Cpm ; construire les plus petits volumes synthétiques nécessaires et vérifier nom, répertoires, métadonnées et octets des fichiers, y compris vide et fragmenté.
  - [x] **4.4.7.2** — Tester chaîne de blocs en boucle, références hors volume, troncatures et espace insuffisant selon le format ; exercer les écritures réellement disponibles avec des résultats attendus indépendants.

- [x] **4.4.8 — Vérifier les systèmes de fichiers Dec** — `DecFileSystemScenarios.cs`
  - [x] **4.4.8.1** — Recenser les variantes implémentées dans FileSystems/Dec ; construire les plus petits volumes synthétiques nécessaires et vérifier nom, répertoires, métadonnées et octets des fichiers, y compris vide et fragmenté.
  - [x] **4.4.8.2** — Tester chaîne de blocs en boucle, références hors volume, troncatures et espace insuffisant selon le format ; exercer les écritures réellement disponibles avec des résultats attendus indépendants.

- [x] **4.4.9 — Vérifier les systèmes de fichiers Fat12** — `Fat12FileSystemScenarios.cs`
  - [x] **4.4.9.1** — Recenser les variantes implémentées dans FileSystems/Fat12 ; construire les plus petits volumes synthétiques nécessaires et vérifier nom, répertoires, métadonnées et octets des fichiers, y compris vide et fragmenté.
  - [x] **4.4.9.2** — Tester chaîne de blocs en boucle, références hors volume, troncatures et espace insuffisant selon le format ; exercer les écritures réellement disponibles avec des résultats attendus indépendants.

- [x] **4.4.10 — Vérifier les systèmes de fichiers Macintosh** — `MacintoshFileSystemScenarios.cs`
  - [x] **4.4.10.1** — Recenser les variantes implémentées dans FileSystems/Macintosh ; construire les plus petits volumes synthétiques nécessaires et vérifier nom, répertoires, métadonnées et octets des fichiers, y compris vide et fragmenté.
  - [x] **4.4.10.2** — Tester chaîne de blocs en boucle, références hors volume, troncatures et espace insuffisant selon le format ; exercer les écritures réellement disponibles avec des résultats attendus indépendants.

- [x] **4.4.11 — Vérifier les systèmes de fichiers Sos** — `SosFileSystemScenarios.cs`
  - [x] **4.4.11.1** — Recenser les variantes implémentées dans FileSystems/Sos ; construire les plus petits volumes synthétiques nécessaires et vérifier nom, répertoires, métadonnées et octets des fichiers, y compris vide et fragmenté.
  - [x] **4.4.11.2** — Tester chaîne de blocs en boucle, références hors volume, troncatures et espace insuffisant selon le format ; exercer les écritures réellement disponibles avec des résultats attendus indépendants.

- [x] **4.4.12 — Vérifier les systèmes de fichiers Ucsd** — `UcsdFileSystemScenarios.cs`
  - [x] **4.4.12.1** — Recenser les variantes implémentées dans FileSystems/Ucsd ; construire les plus petits volumes synthétiques nécessaires et vérifier nom, répertoires, métadonnées et octets des fichiers, y compris vide et fragmenté.
  - [x] **4.4.12.2** — Tester chaîne de blocs en boucle, références hors volume, troncatures et espace insuffisant selon le format ; exercer les écritures réellement disponibles avec des résultats attendus indépendants.

### 4.5 — Conversion des images

Groupe de tâches — dossier `Media/Conversion/`.

- [x] Fichier du groupe : `ConversionTests.cs`.

- [x] **4.5.1 — Vérifier les destinations et leur planification** — `ConversionPlanScenarios.cs`
  - [x] **4.5.1.1** — Exercer plusieurs destinations, extensions implicites/explicites, tags et compatibilité sur une source synthétique.
  - [x] **4.5.1.2** — Vérifier les pertes annoncées et le rejet des destinations impossibles avant exécution.

- [x] **4.5.2 — Vérifier le contenu converti** — `ConversionContentScenarios.cs`
  - [x] **4.5.2.1** — Exécuter de petits parcours avec le vrai convertisseur GW GUI et des buffers synthétiques.
  - [x] **4.5.2.2** — Comparer secteurs, géométrie et métadonnées à des valeurs attendues, pas seulement au fait qu’un fichier existe.

### 4.6 — Migration des fichiers entre volumes

Groupe de tâches — dossier `Media/FileMigration/`.

- [x] Fichier du groupe : `FileMigrationTests.cs`.

- [x] **4.6.1 — Vérifier les migrations internes** — `FileMigrationScenarios.cs`
  - [x] **4.6.1.1** — Exercer les transferts de fichiers pris en charge entre volumes synthétiques : noms, arborescences et contenu.
  - [x] **4.6.1.2** — Tester espace insuffisant, conflit, erreur et annulation ; vérifier le bilan et les données conservées.

## 5 — Intégration de l’émulation

Groupe de groupes de tâches — dossier `Emulation/`.

Vérifier les sessions, entrées, adaptations et traitements audio/vidéo de GW GUI avec des cœurs et périphériques simulés.

### 5.1 — Cycle commun de l’émulation

Groupe de tâches — dossier `Emulation/EmulationContracts/`.

- [x] Fichier du groupe : `EmulationContractsTests.cs`.

- [x] **5.1.1 — Vérifier les sessions avec un faux module** — `SessionLifecycleScenarios.cs`
  - [x] **5.1.1.1** — Exercer démarrer, mettre en pause, reprendre, arrêter et fermer sur IEmulatedMachine et ses interfaces simulées.
  - [x] **5.1.1.2** — Vérifier erreurs, annulation, indépendance de plusieurs sessions et désabonnements.

- [x] **5.1.2 — Vérifier les médias et états de session** — `MediaAndSavedStateScenarios.cs`
  - [x] **5.1.2.1** — Simuler insertion, retrait et changement de média ainsi que sauvegarde et restauration d’état.
  - [x] **5.1.2.2** — Vérifier compatibilité déclarée, erreur et absence de média sans lancer le moteur qui l’interprète.

- [x] **5.1.3 — Vérifier brouillons et sauvegardes différées** — `DraftAndSaveScenarios.cs`
  - [x] **5.1.3.1** — Modifier, abandonner, appliquer et recharger une configuration ; vérifier isolation des brouillons.
  - [x] **5.1.3.2** — Piloter la file des sauvegardes différées des profils vidéo : fusion des changements, dernière valeur lors de `FlushPending`, isolation des configurations, suppression d’une écriture en attente et reprise après erreur. Le service utilisé ordonnance des tâches ; le debouncer à délai n’a aucun appelant dans l’application.

### 5.2 — Entrées clavier, souris et manettes

Groupe de tâches — dossier `Emulation/ControllerInput/`.

- [x] Fichier du groupe : `ControllerInputTests.cs`.

- [x] **5.2.1 — Vérifier identité et modèle** — `DeviceClassificationScenarios.cs`
  - [x] **5.2.1.1** — Injecter descriptions et rapports synthétiques de périphériques connus, inconnus et composites.
  - [x] **5.2.1.2** — Contrôler nom, modèle, capacités, doublons et retrait ; aucune énumération native du matériel.

- [x] **5.2.2 — Vérifier les valeurs d’entrée** — `AnalogAndHidScenarios.cs`
  - [x] **5.2.2.1** — Contrôler décodage HID, axes, gâchettes, zones mortes et normalisation avec des bornes explicites.
  - [x] **5.2.2.2** — Vérifier centrage, inversion et valeurs extrêmes sans acquisition GameInput réelle.

- [x] **5.2.3 — Vérifier affectations et retours** — `BindingsAndFeedbackScenarios.cs`
  - [x] **5.2.3.1** — Fournir des états d’entrée et des notifications synthétiques de session inactive ; vérifier remappage, profils, relâchement des touches et session destinataire. Aucune capture du clavier ou déplacement du focus du poste.
  - [x] **5.2.3.2** — Intercepter la vibration ; vérifier cible, intensité et arrêt sans périphérique physique. Les scénarios du groupe 1.11 exercent les quatre moteurs depuis le contrôleur réel de la vue ; ne pas les dupliquer.

### 5.3 — Présentation vidéo

Groupe de tâches — dossier `Emulation/Video/`.

- [x] Fichier du groupe : `VideoTests.cs`.

- [x] **5.3.1 — Vérifier la géométrie et les réglages vidéo** — `VideoGeometryScenarios.cs`
  - [x] **5.3.1.1** — Contrôler rapport d’aspect, résolution et limites des réglages sur de petites frames synthétiques. Le recadrage est une option transmise aux adaptateurs, couverte en 5.4.2 ; aucun recadrage commun n’est implémenté dans le présentateur.
  - [x] **5.3.1.2** — Vérifier ordre des étapes et compatibilités des profils/filtres avec des capacités de rendu simulées.

- [x] **5.3.2 — Vérifier le traitement vidéo de GW GUI** — `VideoProcessingScenarios.cs`
  - [x] **5.3.2.1** — Exercer les fonctions réelles de traitement accessibles avec une petite matrice de pixels et des résultats indépendants.
  - [x] **5.3.2.2** — Pour une étape GPU, vérifier uniquement les paramètres, ressources et commandes transmis au moteur simulé ainsi que le traitement de ses erreurs. Aucun contexte GPU ni surface native ; le rendu effectif du shader est explicitement hors de la couverture de cette suite. Les transformations CPU intégrées restent exercées réellement.

### 5.4 — Adaptations propres aux familles de machines

Groupe de tâches — dossier `Emulation/MachineAdapters/`.

- [x] Fichier du groupe : `MachineAdaptersTests.cs`.

- [x] **5.4.1 — Vérifier les catalogues et compatibilités des modules** — `MachineCapabilitiesScenarios.cs`
  - [x] **5.4.1.1** — Décliner les modèles, capacités, médias et réglages actuellement implémentés, avec une liste de cas identifiée par module.
  - [x] **5.4.1.2** — Vérifier valeurs limites et combinaisons incompatibles ; tester les décisions GW GUI, pas les capacités annoncées sans implémentation.

- [x] **5.4.2 — Vérifier les traductions de configuration** — `MachineConfigurationMappingScenarios.cs`
  - [x] **5.4.2.1** — Contrôler la conversion des réglages GW GUI en paramètres de chaque adaptateur, notamment le recadrage vidéo, et les données transmises au stockage simulé.
  - [x] **5.4.2.2** — Vérifier chaque différence de famille pertinente sans démarrer WinUAE, un cœur Libretro ou toute autre DLL d’émulation externe.

- [x] **5.4.3 — Vérifier les erreurs des adaptateurs** — `MachineAdapterFailureScenarios.cs`
  - [x] **5.4.3.1** — Simuler cœur indisponible, firmware absent, refus de média et arrêt inattendu au niveau des interfaces.
  - [x] **5.4.3.2** — Contrôler messages, état et nettoyage côté GW GUI ; aucun téléchargement de ROM, firmware ou moteur réel.

### 5.5 — Sortie audio

Groupe de tâches — dossier `Emulation/Audio/`.

- [x] Fichier du groupe : `AudioTests.cs`.

- [x] **5.5.1 — Vérifier notre gestion audio** — `AudioBufferScenarios.cs`
  - [x] **5.5.1.1** — Fournir de petits buffers de samples aux fonctions intégrées ; contrôler canaux, taille, volume et silence selon les traitements existants.
  - [x] **5.5.1.2** — Simuler la sortie audio pour tester démarrage, pause, vidage et fermeture sans ouvrir de périphérique WASAPI réel.

## Exécution et suivi

Les **122 tâches des 41 groupes** sont terminées et cochées. Dernière vérification locale : **1 579 tests réussis, 0 échec, 0 ignoré, en 18 secondes**.

Le raccordement existant exécute `dotnet test --configuration Release` avant le packaging et bloque sur échec ou absence de tests exécutés. Ce contrôle lit directement la sortie de la commande et ne conserve aucun rapport. Les anciens filtres de tests et l’appel à l’audit interactif ont été retirés. Le résultat sur GitHub reste à confirmer lors de la prochaine release autorisée.

À la réalisation de chaque groupe :

- ajouter les références nécessaires au projet, sans masquer de tests par un filtre ;
- exécuter les scénarios et vérifier leurs résultats, leur isolation et leur durée ;
- cocher les travaux réellement vérifiés et mettre à jour [l’état des tests](../project/testing.md).

Les contrôles d’installation et de mise à jour, ainsi que la validation du wiki, restent distincts de cette suite. L’automatisation des tests reste limitée au workflow de release existant.
