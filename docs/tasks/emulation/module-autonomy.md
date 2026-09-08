# Autonomie des modules d'émulation — tâches à réaliser

## Périmètre et règles de suivi

Ordre retenu : traductions embarquées, manifestes et compatibilité, paquets indépendants,
recherche des mises à jour, puis application de mise à jour dédiée.
Le travail suit les cases ci-dessous ; une case ouverte ne constitue pas une réalisation.

Appliquer les consignes de `.codex/config.toml` : une action terminale à la fois, dans
l'ordre ; cocher uniquement après réalisation et vérification. Cocher les parents seulement
après tous leurs enfants. Écrire toute action manquante avant de l'exécuter et réorganiser
la suite immédiatement après la dernière action cochée, sans modifier l'ordre du travail accompli.

Les chemins de fichiers à créer ci-dessous désignent les livrables proposés. Si la lecture
du code montre qu'un fichier existant remplit déjà ce rôle, modifier cette feuille avant
l'action pour le réutiliser. Les inventaires prévus doivent compléter la liste avec les
chemins exacts des consommateurs à modifier, avant toute modification de ces consommateurs.
Une décision produit non résolue doit être soumise à l'utilisateur après examen de l'existant.

Conserver `GWGUI.Emulation` comme couche de contrats et de traitements communs entre App et
les modules. Conserver une DLL de module par famille, le dépôt commun et les données existantes.
Le découpage interne des moteurs reste [différé](../../future/emulation-engine-organization.md).
NuGet, dépôts séparés, sélection des modules dans l'installateur, page de diagnostic dédiée
et remplacement à chaud ne font pas partie de ce parcours.

Les tests nouveaux conservés doivent être utiles et autonomes. Les essais ajoutés qui créent
des fichiers ou dépendent de fichiers, applications ou DLL externes sont temporaires : les
supprimer après utilisation, avec leurs seuls artefacts. Préserver les tests et données
préexistants. Consigner les résultats réels, les limites et les validations utilisateur
encore attendues dans `docs/tasks/emulation/module-autonomy-validation.md` ; ne pas présenter
un résultat historique comme une validation nouvelle.

## Liste ordonnée

- [ ] 1. Donner aux modules leurs traductions embarquées prioritaires
  - [x] 1.1. Définir le raccordement à la traduction commune existante
    - [x] 1.1.1. Formaliser la répartition des textes et les appels à adapter
      - [x] Créer `docs/architecture/emulation-module-localization.md` à partir de `Localization/Extensions/LocExtension.cs`, `Localization/Sources/LocalizationSource.cs`, `Constants/Localization/LocalizationCatalogNames.cs`, `Dictionaries/Localization/UiLanguageCatalog.cs` sous `src/GWGUI.App`, des ressources App et des clés émises par Amiga et Atari ; y inscrire les clés communes, invariantes et propres à chaque module, toutes les cultures distribuées et les chemins des consommateurs concernés.
      - [x] Compléter ce document avec la résolution prévue : catalogue du module concerné avant les ressources App, repli de langue du module, conservation de `00-Base`, du formatage et du comportement actuel de clé absente ; préciser le changement de langue et la prévention des collisions entre familles. Soumettre toute ambiguïté restante avant de coder.
      - [x] Compléter les sous-tâches de 1.2 et 1.3 dans ce fichier avec chaque chemin de consommateur identifié et les remplacements précis à réaliser.
  - [ ] 1.2. Raccorder un catalogue de module sans dépendance vers App
    - [ ] 1.2.1. Ajouter le contrat et sa résolution commune
      - [ ] Créer `src/GWGUI.Emulation/Interfaces/IEmulationModuleLocalization.cs` avec la capacité de rechercher un texte par clé et culture, en distinguant une clé absente d'une valeur présente ; ne pas exposer de type App ou WPF.
      - [ ] Créer `src/GWGUI.Emulation/Services/EmulationModuleLocalization.cs` pour lire les ressources embarquées d'un module et appliquer le repli documenté, sans dépendre d'un moteur concret.
      - [ ] Modifier `src/GWGUI.App/Localization/Extensions/LocExtension.cs` pour proposer la recherche et les bindings dans le contexte d'un module, avec priorité au module puis repli vers la résolution App existante ; conserver les appels sans module.
      - [ ] Modifier `src/GWGUI.App/Services/Emulation/EmulationModuleRegistry.cs` pour rendre accessibles les catalogues des modules chargés sans construire de dépendance circulaire entre initialisation des modules et localisation.
    - [ ] 1.2.2. Relier les consommateurs aux catalogues
      - [ ] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs` pour traduire les métadonnées avec `_module`, y compris les choix et explications, et conserver le rafraîchissement existant.
      - [ ] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs` pour traduire les familles avec leur module lors de la construction et du rafraîchissement.
      - [ ] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs` pour traduire le nom de machine dans son contexte de module.
      - [ ] Modifier `src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionConfigurationFunctions.cs` pour traduire les familles avec leur module.
      - [ ] Modifier `src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionLayoutFunctions.cs` pour traduire les titres de machines et runtimes avec le module sélectionné ou l'identifiant de configuration.
      - [ ] Modifier `src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationPresenter.cs` pour traduire le résumé avec le module reçu.
      - [ ] Modifier `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs` pour accepter le contexte de module dans `DisplayValue` sans modifier les calculs numériques.
      - [ ] Modifier `src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs` pour transmettre le module à la traduction du nom et du choix CPU.
      - [ ] Modifier `src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditor.xaml.cs` pour accepter un contexte de traduction optionnel dans `SetRows`, en conservant les raccourcis hôte sans module.
      - [ ] Modifier `src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs` pour transmettre le module aux choix des périphériques et aux trois appels d'affichage des associations ; conserver les noms des contrôleurs physiques communs.
      - [ ] Modifier `src/GWGUI.App/Views/Dialogs/Emulation/Storage/FloppyDriveConfigurationDialog.cs` pour accepter un contexte de traduction optionnel des modèles de lecteur.
      - [ ] Modifier `src/GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs` pour traduire les modèles et transmettre le contexte au dialogue de lecteur.
      - [ ] Modifier `src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs` pour traduire le libellé du champ destinataire avec le module reçu.
      - [ ] Modifier `src/GWGUI.App/Services/Emulation/HardDiskDeletionService.cs` pour traduire les noms de machines référentes avec chaque module parcouru.
      - [ ] Compléter `docs/architecture/emulation-module-localization.md` avec la vérification des consommateurs préservés : `EmulationModuleHardwareSettingsSection.cs`, `EmulationEmulatorManagementController.cs`, `ControlErrorPresenter.cs` et touches hôte ; inscrire ici toute adaptation supplémentaire nécessaire avant de l'exécuter.
  - [ ] 1.3. Transférer les ressources des deux familles
    - [ ] 1.3.1. Embarquer les traductions Amiga
      - [ ] Créer `src/GWGUI.Emulation.Amiga/Resources/00-Base/Emulation.resx` et les `Resources/<culture>/Emulation.resx` de toutes les cultures inventoriées ; déplacer les clés propres à Amiga depuis les catalogues App recensés, sans changer les identifiants ni retraduire les textes déjà présents. Garder les valeurs invariantes dans la base appropriée uniquement.
      - [ ] Modifier `src/GWGUI.Emulation.Amiga/GWGUI.Emulation.Amiga.csproj` pour embarquer ces catalogues de toutes les cultures dans la DLL du module et leur donner les noms attendus par le lecteur commun.
      - [ ] Modifier `src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs` pour exposer son catalogue par le contrat commun.
    - [ ] 1.3.2. Embarquer les traductions Atari
      - [ ] Créer `src/GWGUI.Emulation.Atari/Resources/00-Base/Emulation.resx` et les `Resources/<culture>/Emulation.resx` de toutes les cultures inventoriées ; déplacer les clés propres à Atari depuis les catalogues App recensés, en conservant textes, identifiants et séparation des invariants.
      - [ ] Modifier `src/GWGUI.Emulation.Atari/GWGUI.Emulation.Atari.csproj` pour embarquer tous ces catalogues dans la DLL du module avec les noms attendus.
      - [ ] Modifier `src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs` pour exposer son catalogue par le contrat commun.
    - [ ] 1.3.3. Maintenir les langues et la documentation
      - [ ] Modifier `scripts/translate-resx-argos.py` pour accepter la racine de ressources d'un module en conservant la racine App par défaut et les opérations existantes ; utiliser les modèles Argos installés sans réexaminer leur installation.
      - [ ] Compléter avec cet outil les seules traductions nouvelles ou manquantes des ressources concernées dans toutes les langues ; conserver les paramètres de formatage et éviter les doublons invariants.
      - [ ] Modifier `docs/architecture/emulation-module-authoring.md` et `docs/future/emulation-plugins.md` pour décrire les traductions réellement embarquées, leur priorité et leur maintenance ; retirer la présentation des traductions externes comme cible retenue.
  - [ ] 1.4. Valider la traduction des modules
    - [ ] 1.4.1. Vérifier la priorité et le comportement existant
      - [ ] Ajouter dans `tests/GWGUI.Tests/Interface/Localization` des scénarios autonomes avec catalogues en mémoire pour la priorité, le repli, les collisions entre modules, le formatage et les clés absentes ; compléter la feuille avec les noms exacts des fichiers avant création, selon le fonctionnement des tests existants.
      - [ ] Créer `docs/tasks/emulation/module-autonomy-validation.md` et y enregistrer les résultats des tests pertinents et de `scripts/build.ps1 -Configuration Debug`, après leur exécution.
      - [ ] Compléter ce relevé après essai de changement de langue et d'affichage des réglages Amiga et Atari, contrôle de toutes les cultures et essai temporaire d'une clé concurrente module/App ; préciser toute validation interactive encore attendue.
      - [ ] Supprimer les scénarios temporaires, fichiers et modifications de ressources créés uniquement pour ces essais ; conserver les tests autonomes utiles et inscrire le nettoyage dans le relevé.

- [ ] 2. Identifier les modules et contrôler leur compatibilité
  - [ ] 2.1. Fixer le manifeste et la version d'API
    - [ ] 2.1.1. Documenter les contrats exacts avant le chargeur
      - [ ] Compléter `docs/architecture/emulation-modules.md` avec le schéma versionné de `module.json`, les champs d'identité, DLL d'entrée, version du module et bornes d'API ; préciser la comparaison des versions, les identifiants, les chemins autorisés et la transition depuis les DLL directement dans `Modules`. Résoudre avec l'utilisateur toute décision non établie.
      - [ ] Créer `src/GWGUI.Emulation/Contracts/EmulationModuleManifest.cs` selon ce schéma et `src/GWGUI.Emulation/Constants/EmulationHostApi.cs` pour la version des contrats partagés, distincte du produit et des modules ; conserver les traitements actuels d'Emulation.
      - [ ] Créer `src/GWGUI.Emulation.Amiga/module.json` et `src/GWGUI.Emulation.Atari/module.json` avec les identifiants existants et les versions définies à l'étape précédente ; configurer leur copie dans les deux fichiers `.csproj`.
  - [ ] 2.2. Adapter la découverte et la distribution des binaires
    - [ ] 2.2.1. Valider avant l'initialisation
      - [ ] Créer `src/GWGUI.App/Services/Emulation/EmulationModuleManifestReader.cs` pour lire et valider chaque manifeste, la compatibilité et le confinement du chemin de la DLL avant la création de la factory.
      - [ ] Modifier `src/GWGUI.App/Services/Emulation/EmulationModuleRegistry.cs` pour découvrir les paquets selon la transition documentée, contrôler les doublons et la concordance des identités, puis journaliser version, chemin et raisons de refus ; conserver l'indépendance des erreurs et les chemins de données existants.
      - [ ] Modifier `scripts/build.ps1` pour produire `Modules/Amiga` et `Modules/Atari` avec leurs manifestes et DLL ; conserver `Data/Emulation/Machines/<Id>` et les données des utilisateurs.
      - [ ] Compléter `docs/architecture/emulation-module-authoring.md` avec un exemple de manifeste conforme, les règles d'évolution de l'API et l'installation locale des paquets.
  - [ ] 2.3. Valider le chargement et la compatibilité
    - [ ] 2.3.1. Couvrir les refus utiles sans conserver de fixtures externes
      - [ ] Ajouter des scénarios autonomes de validation des manifestes et versions dans `tests/GWGUI.Tests/Emulation/Modules`, avec données en mémoire ; inscrire les fichiers exacts dans cette liste avant leur création.
      - [ ] Compléter `docs/tasks/emulation/module-autonomy-validation.md` après tests et essais temporaires : dossier absent/vide, Amiga seul, Atari seul, les deux, manifeste absent/invalide/incompatible, doublon, DLL invalide, factory en échec et module retiré avec données conservées ; inclure le routage des commandes de processus des moteurs.
      - [ ] Supprimer les modules factices, scripts d'essai et fichiers produits uniquement pour cette validation, restaurer le paquet de travail et inscrire le nettoyage dans le relevé.

- [ ] 3. Produire des paquets de modules indépendants et un paquet complet
  - [ ] 3.1. Préparer les dépendances et leur résolution
    - [ ] 3.1.1. Définir puis appliquer la frontière des bibliothèques
      - [ ] Compléter `docs/architecture/emulation-modules.md` avec l'inventaire des dépendances produites par les projets Amiga et Atari, leur destination commune ou privée, les ressources embarquées et les fichiers nécessaires au lancement des processus ; distinguer binaires distribués et cœurs téléchargés par les services existants.
      - [ ] Créer `src/GWGUI.App/Services/Emulation/EmulationModuleLoadContext.cs` pour résoudre les dépendances privées depuis le paquet et partager la bibliothèque de contrats avec App, sans déchargement à chaud ; raccorder ce contexte dans `EmulationModuleRegistry.cs`.
      - [ ] Modifier `scripts/build.ps1` et, selon l'inventaire, `scripts/organize-application-output.ps1` pour copier les dépendances nécessaires sans déplacer les fichiers privés dans les bibliothèques communes ni dupliquer les contrats partagés.
  - [ ] 3.2. Ajouter la fabrication et la publication par module
    - [ ] 3.2.1. Fabriquer des archives vérifiables
      - [ ] Créer `scripts/package-module.ps1` pour construire un module sélectionné, lire sa version dans son manifeste, vérifier les fichiers attendus et produire son archive et son empreinte dans `dist` ; ne pas remplacer les contrats installés avec GW GUI.
      - [ ] Modifier `scripts/package.ps1` pour inclure les paquets officiels compatibles dans la distribution complète en réutilisant la fabrication des modules ; préserver le packaging portable et l'installateur existants.
      - [ ] Modifier `.github/workflows/release.yml` pour joindre les archives de modules et leurs empreintes aux publications complètes, en conservant les contrôles existants et la version unique du produit.
      - [ ] Créer `.github/workflows/module-release.yml` pour préparer une publication d'un seul module avec sa propre version, ses changements et ses contrôles ; documenter dans `docs/project/release.md` les noms de tags distincts et la procédure avant toute publication réelle.
  - [ ] 3.3. Valider les paquets
    - [ ] 3.3.1. Vérifier les distributions complètes et indépendantes
      - [ ] Compléter `docs/tasks/emulation/module-autonomy-validation.md` après fabrication Debug et Release, contrôles des archives/empreintes et remplacement temporaire d'un seul module, application fermée ; vérifier le démarrage, les traductions, l'autre module et les configurations existantes.
      - [ ] Compléter le relevé après exécution des contrôles existants pertinents de l'installateur et du portable ; distinguer les validations effectuées de celles encore attendues.
      - [ ] Supprimer les scripts, fichiers et installations temporaires ajoutés pour ces essais, préserver les données préexistantes et noter le nettoyage.

- [ ] 4. Préparer le catalogue et la recherche des mises à jour
  - [ ] 4.1. Définir les informations de publication et la sélection
    - [ ] 4.1.1. Formaliser le flux avant d'ajouter des commandes utilisateur
      - [ ] Créer `docs/architecture/emulation-module-updates.md` avec le catalogue officiel commun : application et modules, identifiants, versions, compatibilité, URL des paquets, empreintes et notes ; y préciser les trois recherches application/modules/ensemble et le résultat pour un module exigeant une API plus récente.
      - [ ] Compléter ce document après examen des commandes de mise à jour déjà présentes dans App et du workflow GitHub : inscrire les chemins exacts des points d'intégration, l'emplacement de publication du catalogue et la politique de sélection des versions. Demander les décisions utilisateur qui ne sont pas déjà établies, puis détailler les modifications dans cette feuille.
  - [ ] 4.2. Construire la recherche et le plan commun
    - [ ] 4.2.1. Ajouter les contrats et les services nécessaires
      - [ ] Créer les fichiers de contrats de catalogue, de version disponible et de plan de mise à jour aux emplacements arrêtés en 4.1, après les avoir inscrits individuellement ici ; représenter les dépendances d'API sans imposer de mise à jour automatique.
      - [ ] Créer `scripts/build-update-catalog.ps1` pour générer le catalogue depuis les métadonnées et empreintes des paquets ; adapter les deux workflows de publication pour préserver les entrées des composants non modifiés.
      - [ ] Créer le service de recherche dans le chemin App arrêté en 4.1 : comparer les manifestes installés au catalogue, proposer les versions compatibles et signaler une mise à jour App nécessaire ; conserver les recherches de versions des cœurs existantes.
      - [ ] Ajouter les commandes et résultats dans les fichiers d'interface identifiés en 4.1, avec sélection application/modules/ensemble et versions souhaitées ; ajouter toutes les nouvelles chaînes dans les ressources App de toutes les cultures avec Argos, en utilisant `00-Base` pour les invariants.
  - [ ] 4.3. Valider la recherche sans appliquer de mise à jour
    - [ ] 4.3.1. Vérifier les comparaisons et la sélection
      - [ ] Ajouter des tests autonomes sur des catalogues en mémoire : module à jour, version compatible, API trop ancienne, recherche modules seuls et plan de plusieurs mises à jour ; inscrire leurs fichiers exacts dans cette feuille avant création.
      - [ ] Compléter `docs/tasks/emulation/module-autonomy-validation.md` avec les résultats et un essai temporaire de récupération du catalogue et d'affichage multilingue, sans remplacer de fichiers installés.
      - [ ] Supprimer les catalogues, scripts et fichiers de test temporaires et inscrire leur nettoyage dans le relevé.

- [ ] 5. Appliquer les mises à jour avec un exécutable dédié
  - [ ] 5.1. Définir le protocole de préparation, fermeture et restauration
    - [ ] 5.1.1. Décrire le fonctionnement complet avant le remplacement
      - [ ] Compléter `docs/architecture/emulation-module-updates.md` à partir de `src/GWGUI.Launcher/Program.cs`, `LauncherPolicy.cs`, du démarrage/arrêt App et de `installer/GWGUI.iss` : chemins d'installation, droits, préparation, plan complet, attente de fermeture, sauvegarde, remplacement, relance unique et restauration ; préciser le signal de démarrage réussi avec l'utilisateur si nécessaire.
      - [ ] Inscrire dans cette feuille les fichiers exacts du démarrage/arrêt App à modifier et les contrats du protocole ; résoudre toute décision non établie avant l'implémentation.
  - [ ] 5.2. Construire et raccorder l'outil
    - [ ] 5.2.1. Préparer tous les paquets avant la fermeture
      - [ ] Ajouter au service App de 4.2 le téléchargement annulable, la vérification des empreintes et de la compatibilité de l'ensemble sélectionné, puis la préparation du plan dans un dossier temporaire ; ne lancer aucun remplacement si la préparation échoue.
      - [ ] Créer `src/GWGUI.Updater/GWGUI.Updater.csproj` et `Program.cs`, puis ajouter le projet à `GWGUI.sln` : exécutable minimal ne chargeant aucun module et indépendant des DLL qu'il doit remplacer.
      - [ ] Détailler puis créer les fichiers internes de l'updater pour lire le plan, demander la fermeture et attendre la fin effective de GW GUI et de ses machines, sauvegarder puis remplacer les seuls composants sélectionnés ; préserver les données utilisateur et restaurer après échec selon le protocole de 5.1.
      - [ ] Modifier les fichiers App identifiés en 5.1 pour transmettre le plan à l'updater, fermer proprement les machines et l'application, puis signaler le résultat du nouveau démarrage ; traduire tous les nouveaux messages dans toutes les cultures avec Argos.
      - [ ] Modifier `scripts/build.ps1`, `scripts/package.ps1` et les règles concernées de `installer/GWGUI.iss` pour distribuer l'updater ; traiter son propre remplacement uniquement selon le protocole documenté.
  - [ ] 5.3. Valider le parcours complet et terminer la documentation
    - [ ] 5.3.1. Exercer le remplacement dans une installation temporaire
      - [ ] Compléter `docs/tasks/emulation/module-autonomy-validation.md` après essais temporaires : module seul, application seule, plusieurs composants avec une fermeture/relance, téléchargement interrompu, empreinte invalide, fermeture non terminée, remplacement en échec et restauration après démarrage échoué ; vérifier les données et les traductions.
      - [ ] Supprimer les programmes, scénarios, paquets et installations temporaires créés pour ces essais après vérification de leurs chemins ; conserver uniquement les tests autonomes utiles et le relevé des résultats.
      - [ ] Mettre à jour `docs/architecture/emulation-modules.md`, `docs/architecture/emulation-module-authoring.md`, `docs/architecture/emulation-module-updates.md`, `docs/project/release.md` et `docs/future/emulation-plugins.md` pour distinguer les fonctions réalisées des points toujours différés.
