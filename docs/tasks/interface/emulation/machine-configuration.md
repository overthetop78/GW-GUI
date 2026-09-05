# Identification et enregistrement des machines

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

## 2. Identification de la machine modifiée

Il faut rendre plus évident ce que l’utilisateur est en train de modifier dans les options d’émulation.

### Machines possédant déjà une configuration

Dans la liste des machines, celles qui possèdent déjà une configuration doivent être distinguées par un fond d’une nuance de gris clair et un texte vert forêt en gras. Une icône autre qu’une coche peut également accompagner cette présentation ; son apparence reste à choisir.

Cette indication doit être visible :

- dans la liste déroulante lorsqu’elle est ouverte ;
- sur la machine sélectionnée lorsque la liste est fermée.

### Barre de titre

Lorsque l’utilisateur modifie une machine dans la partie **Émulation**, la barre de titre de la fenêtre doit indiquer la marque et la machine concernées. Cette indication reste visible dans tous les sous-onglets, contrairement au champ **Modèle**, qui n’est visible que dans l’onglet **Général** de la marque.

Cette information ne doit pas être ajoutée dans le libellé de l’onglet.

Le format retenu est **Options — Amiga : Amiga 500**, avec le format correspondant pour Atari, par exemple **Options — Atari : Atari ST**.

Dans l’onglet général **Configuration**, où aucune machine particulière n’est en cours de modification, la barre de titre reste simplement **Options**. Aucun titre **Options — Émulation — Configurations** ne doit être ajouté.

### Chargement d’une configuration existante

Lorsqu’une machine possédant déjà une configuration est sélectionnée, ses réglages enregistrés doivent être chargés dans l’onglet correspondant à cette machine, côté Amiga ou Atari.

### Création et enregistrement des changements

Le comportement dépend de l’existence de la configuration :

- si la configuration de la machine n’existe pas encore, chaque changement effectué par l’utilisateur est conservé dans un brouillon en mémoire pendant toute l’exécution de l’application, sans écrire de fichier tant que l’utilisateur ne clique pas sur **Créer** ;
- chaque machine sans configuration possède son propre brouillon en mémoire et le conserve lorsque l’utilisateur affiche une autre machine ou ferme puis rouvre la fenêtre **Paramètres** ;
- après la création, le bouton **Créer** disparaît immédiatement ;
- si la configuration de cette machine est ensuite supprimée depuis la liste des configurations, le bouton **Créer** réapparaît lorsque cette machine est sélectionnée ;
- aucune configuration existante n’affiche de bouton **Modifier**, puisque chaque changement est enregistré automatiquement ;
- aucun enregistrement automatique ne demande de confirmation.

Tous les champs modifiables d’une configuration existante doivent être couverts par l’enregistrement automatique. Il faudra vérifier chaque champ de l’ensemble des onglets Amiga et Atari.

Le moment de l’enregistrement dépend du contrôle :

- liste, case, bouton de choix, curseur ou autre sélecteur : dès que la valeur change ;
- champ de saisie : lorsque le champ perd le focus ;
- association de clavier, souris ou manette : dès que l’association est créée, remplacée, supprimée ou restaurée ;
- action modifiant plusieurs valeurs : toutes les valeurs concernées sont enregistrées à la fin de l’action.

Il n’est pas prévu de créer plusieurs configurations nommées pour une même machine ni d’ajouter un nom de profil.

## Checklist détaillée — Point 2 : identification et enregistrement de la machine modifiée

Cette checklist couvre uniquement les modifications faites par l’utilisateur. Une machine qui possède un fichier est enregistrée directement dans ce fichier. Une machine qui n’en possède pas conserve un brouillon distinct en mémoire pendant toute l’exécution de l’application, jusqu’à l’utilisation du bouton Créer. Les champs de saisie sont pris en compte à la perte du focus ; les sélecteurs, options, cases et actions spécialisées le sont immédiatement.

- [x] Conserver les brouillons des machines non créées pendant l’exécution de l’application
  - [x] Créer le stockage applicatif avant toute utilisation
    - [x] Créer le fichier vide src/GWGUI.App/Services/Emulation/EmulationConfigurationDraftStore.cs.
    - [x] Modifier src/GWGUI.App/Services/Emulation/EmulationConfigurationDraftStore.cs pour conserver en mémoire au plus un IEmulationConfiguration par identifiant de module et identifiant de machine.
    - [x] Modifier src/GWGUI.App/Services/Emulation/EmulationConfigurationDraftStore.cs pour permettre la lecture, le remplacement et le retrait du brouillon d’une machine sans écrire de fichier.

- [x] Traiter chaque modification faite par l’utilisateur
  - [x] Créer le traitement commun avant de raccorder les contrôles
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter un traitement commun qui capture dans _configuration les champs génériques, les entrées et le stockage actuellement affichés.
    - [x] Modifier ce traitement dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour remplacer le brouillon de la machine dans EmulationConfigurationDraftStore lorsqu’aucune configuration enregistrée correspondante n’existe.
    - [x] Modifier ce traitement dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler IEmulationModule.SaveConfigurationAsync et signaler ConfigurationSaved lorsque la configuration de la machine existe déjà.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour utiliser le sémaphore existant lors des écritures déclenchées par l’utilisateur et écrire la dernière modification reçue après une écriture déjà en cours.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour exécuter ce traitement par ExecuteAsync afin d’afficher une erreur lorsqu’une écriture échoue.
  - [x] Raccorder les sélecteurs, options et cases
    - [x] Modifier CreateSelection dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun immédiatement après un changement effectué par l’utilisateur.
    - [x] Modifier CreateToggle dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun immédiatement après un changement effectué par l’utilisateur.
    - [x] Modifier CreateSelection et CreateToggle dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour différer le raccordement de leurs gestionnaires utilisateur et y terminer CaptureEditorValues ainsi que la reconstruction demandée par RefreshSettingsOnChange avant l’appel au traitement commun.
    - [x] Modifier ApplySettingsRules dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour raccorder ensuite les gestionnaires utilisateur différés, terminer les règles existantes avant leur appel au traitement commun et ne pas traiter séparément les changements de contrôle produits par ces règles.
  - [x] Raccorder les champs de saisie
    - [x] Modifier CreateField dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun à la perte du focus des éditeurs Text, Number et Percentage.
    - [x] Modifier CreatePath dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun à la perte du focus du chemin saisi et immédiatement après la sélection réussie d’un fichier.
    - [x] Modifier CreateDirectoryPath dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun à la perte du focus du chemin saisi et immédiatement après la sélection réussie d’un dossier.
  - [x] Raccorder les actions spécialisées
    - [x] Ajouter ConfigurationChanged dans src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs et le déclencher immédiatement après qu’un utilisateur a appliqué un firmware compatible avec Utiliser.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun lorsque EmulationFirmwareManagementController signale ConfigurationChanged.
    - [x] Ajouter SettingsChanged dans src/GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs et le déclencher immédiatement après qu’un utilisateur a ajouté, supprimé ou configuré un lecteur ou son média.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun lorsque EmulationStorageSettingsController signale SettingsChanged.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour remplacer la sauvegarde particulière des entrées par le traitement commun lorsque EmulationInputSettingsController signale SettingsChanged.

- [x] Charger, créer et supprimer les configurations sans perdre les brouillons
  - [x] Charger la machine demandée depuis la bonne source
    - [x] Modifier ReloadAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour afficher la configuration enregistrée de cette machine lorsqu’elle existe, sinon son brouillon applicatif lorsqu’il existe, sinon les valeurs de base créées par le module.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter ReloadWhenOpenedAsync qui exécute ReloadAsync par ExecuteAsync et conserve la présentation d’erreur existante.
    - [x] Modifier ModuleTabSelectionChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour appeler ReloadWhenOpenedAsync chaque fois que l’utilisateur rouvre un onglet Amiga ou Atari dont la section existe déjà, en conservant le chargement initial par Loaded lors de sa première ouverture.
    - [x] Modifier MachineChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour afficher la configuration enregistrée, le brouillon applicatif ou les valeurs de base de la machine choisie sans réutiliser les valeurs d’une autre machine.
  - [x] Disposer de Common.Create avant de modifier le bouton existant
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/00-Base/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ar-SA/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/cs-CZ/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/da-DK/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/de-DE/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/el-GR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/en-US/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/es-ES/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/fi-FI/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/fr-FR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/he-IL/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/hu-HU/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/id-ID/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/it-IT/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ja-JP/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ko-KR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/nb-NO/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/nl-NL/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/pl-PL/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/pt-BR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/pt-PT/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ro-RO/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ru-RU/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/sv-SE/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/th-TH/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/tr-TR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/uk-UA/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/vi-VN/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/zh-Hans/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/zh-Hant/Actions.resx.
  - [x] Utiliser le bouton existant uniquement lorsque la configuration n’existe pas
    - [x] Modifier BuildGeneralHeader dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour conserver le bouton local existant, lui affecter Common.Create et le masquer lorsque les configurations chargées contiennent la machine affichée.
    - [x] Modifier SaveAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour retirer le brouillon correspondant uniquement après la réussite de SaveConfigurationAsync, signaler ConfigurationSaved et reconstruire l’éditeur afin de masquer Créer.
  - [x] Revenir à une machine non créée après sa suppression
    - [x] Modifier DeleteSelectedConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour demander à la section déjà créée du même module de recharger ses configurations après la suppression réussie.

- [x] Distinguer visuellement les machines possédant une configuration
  - [x] Porter l’existence de la configuration dans chaque choix
    - [x] Modifier src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineChoice.cs pour ajouter l’état enregistré de Definition sans modifier DisplayName ni ToString.
    - [x] Modifier la création des choix dans le constructeur, ReloadAsync et RefreshLocalizedContent de src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour établir cet état uniquement depuis les configurations chargées du module.
  - [x] Construire la présentation commune du sélecteur
    - [x] Créer le fichier vide src/GWGUI.App/Constants/Controls/Visual/EmulationMachineChoiceVisualConstants.cs.
    - [x] Modifier src/GWGUI.App/Constants/Controls/Visual/EmulationMachineChoiceVisualConstants.cs pour définir les couleurs du fond gris clair et du texte vert forêt.
    - [x] Créer le fichier vide src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs.
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs pour créer le DataTemplate qui affiche normalement une machine non créée et applique le fond gris clair, le texte vert forêt et la graisse forte à une machine configurée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour affecter ce DataTemplate à _machines dans la liste ouverte et dans la sélection fermée.

- [x] Afficher la marque et la machine modifiées dans le titre de Paramètres
  - [x] Ajouter le format localisé du titre avant son utilisation
    - [x] Modifier src/GWGUI.App/Resources/00-Base/Options.resx pour créer Options.EmulationMachineTitle avec les paramètres du titre Paramètres, de la marque et de la machine.
    - [x] Modifier src/GWGUI.App/Resources/ar-SA/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/cs-CZ/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/da-DK/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/de-DE/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/el-GR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/en-US/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/es-ES/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/fi-FI/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/fr-FR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/he-IL/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/hu-HU/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/id-ID/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/it-IT/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/ja-JP/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/ko-KR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/nb-NO/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/nl-NL/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/pl-PL/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/pt-BR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/pt-PT/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/ro-RO/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/ru-RU/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/sv-SE/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/th-TH/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/tr-TR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/uk-UA/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/vi-VN/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/zh-Hans/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/zh-Hant/Options.resx pour ajouter Options.EmulationMachineTitle.
  - [x] Faire remonter la machine affichée jusqu’à Paramètres
    - [x] Créer le fichier vide src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineEditingContext.cs.
    - [x] Modifier src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineEditingContext.cs pour porter le nom localisé du module et le DisplayName de la machine affichée.
    - [x] Ajouter EditingContextChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs et le déclencher après ReloadAsync, MachineChanged et RefreshLocalizedContent.
    - [x] Ajouter EditingContextChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour transmettre le contexte de la section Amiga ou Atari active et l’absence de contexte dans Général, Raccourcis et Configuration.
  - [x] Modifier uniquement le titre de la fenêtre
    - [x] Modifier src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml et src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml.cs pour raccorder SelectionChanged de Navigation, écouter EditingContextChanged, afficher Options.Title seul hors de l’éditeur d’une machine et afficher Options.EmulationMachineTitle dans tous les sous-onglets de cette machine.
    - [x] Modifier RefreshLocalizedContent dans src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml.cs pour recalculer le titre après un changement de langue sans modifier le texte des onglets.

- [x] Corriger la présentation des machines possédant une configuration
  - [x] Modifier docs/tasks/interface/emulation/machine-configuration.md pour inscrire, avant toute correction, la fermeture de l’instance ouverte, la correction des couleurs sur toute la ligne et toute la sélection fermée, la compilation, la vérification visuelle puis la fermeture.
  - [x] Fermer l’instance de GW GUI actuellement ouverte avant de modifier les fichiers utilisés par l’application.
  - [x] Modifier docs/tasks/interface/emulation/machine-configuration.md pour inscrire le déplacement préalable de la palette Compatible vers les constantes visuelles communes et sa compilation avant sa réutilisation par le sélecteur de machines.
  - [x] Modifier src/GWGUI.App/Constants/Controls/Visual/ControlVisualConstants.cs et src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour déplacer les trois couleurs existantes de l’état Compatible vers les constantes visuelles communes, remplacer immédiatement leurs anciennes valeurs locales et conserver exactement le rendu du badge.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier uniquement le déplacement de la palette Compatible.
  - [x] Modifier src/GWGUI.App/Constants/Controls/Visual/EmulationMachineChoiceVisualConstants.cs pour remplacer le gris par le fond vert clair, le texte vert et la bordure verte de la palette Compatible commune.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs pour retirer le fond limité au texte, conserver le texte en gras pour une machine configurée et créer les styles qui appliquent le fond, le texte et la bordure à toute la ligne déroulée ainsi qu’à toute la sélection fermée.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appliquer au ComboBox des machines les deux styles créés, sans modifier sa sélection ni son fonctionnement.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette correction visuelle.
  - [x] Modifier docs/tasks/interface/emulation/machine-configuration.md après la première vérification visuelle pour inscrire la correction de la liaison de l’état configuré au contexte de données réel de chaque ComboBoxItem avant de relancer l’application.
  - [x] Modifier CreateItemContainerStyle dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs pour lier directement HasSavedConfiguration depuis EmulationMachineChoice au lieu de rechercher Content.HasSavedConfiguration sur cet objet.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette correction de liaison.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier que chaque machine configurée colore toute sa ligne et toute la sélection fermée en vert clair, sans rectangle gris limité au texte, tandis qu’une machine non configurée conserve la présentation normale.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.
