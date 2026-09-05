# Tableau des configurations

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

## 3. Tableau des configurations

La présentation textuelle actuelle doit être remplacée par un tableau plus joli et plus facile à lire.

### Filtre par marque

Une liste déroulante permet de choisir la marque dont les configurations doivent être affichées. Elle contient uniquement les marques pour lesquelles au moins une configuration existe.

Si aucune marque ne possède de configuration, la liste déroulante est vide, le tableau est vide et rien n’est sélectionné.

Si la dernière configuration de la marque affichée est supprimée, aucune autre marque ne doit être sélectionnée automatiquement. La liste déroulante revient à un état sans sélection et le tableau devient vide jusqu’à ce que l’utilisateur choisisse lui-même une autre marque.

### Absence de sélection dans le tableau

Le tableau ne possède aucun mécanisme de sélection de ligne. Un simple clic sur une ligne ne la sélectionne pas et ne charge rien.

Les seules interactions disponibles sur une configuration sont :

- un double-clic sur sa ligne pour la modifier ;
- le bouton avec une icône de crayon pour la modifier ;
- le bouton avec une icône de poubelle pour demander sa suppression.

Cliquer ailleurs ne conserve ou ne crée aucune sélection.

### Informations affichées

Les configurations sont classées par ordre alphabétique du nom de la machine. Un système de tri supplémentaire n’est pas nécessaire, car le nombre de configurations par marque restera limité.

Une machine ne possédant qu’une seule configuration, son nom suffit à identifier la ligne. Aucun identifiant technique ne doit être affiché.

Les colonnes retenues sont :

- **Machine**, placée en premier ;
- **CPU** ;
- **RAM totale** ;
- **Lecteurs** ;
- **Périphériques** ;
- **Actions**.

Le moteur d’affichage vidéo, par exemple Direct3D 11, ainsi que les informations audio ne doivent pas être ajoutés au tableau. Il ne faut pas ajouter d’informations inutiles simplement pour remplir le tableau.

### Lecteurs

La colonne **Lecteurs** affiche une icône pour chaque lecteur configuré. Deux lecteurs sont représentés par deux icônes et non par une seule icône accompagnée du nombre deux.

Les icônes doivent distinguer les différents types de lecteurs, par exemple disquette, disque dur, CD, cassette ou cartouche selon les machines prises en charge.

### Périphériques

La colonne **Périphériques** utilise des icônes pour indiquer les périphériques configurés, notamment le clavier, la souris, les joysticks et les manettes.

Le nombre de joysticks ou de manettes configurés doit être visible en affichant le nombre correspondant d’icônes.

### Modification

Le bouton avec une icône de crayon et le double-clic sur la ligne déclenchent exactement la même action :

- ouvrir l’onglet **Général** de la marque correspondante ;
- afficher la machine concernée ;
- charger sa configuration complète afin de remplir les champs de tous les onglets de cette marque.

Cette page ne permet pas de lancer la machine.

### Suppression

Le bouton avec une icône de poubelle ouvre une boîte de dialogue Oui/Non avant toute suppression.

La boîte de dialogue affiche seulement le minimum permettant d’identifier sans ambiguïté la configuration supprimée : la marque et la machine. Elle ne doit pas devenir une fiche détaillée et ne doit afficher ni CPU, ni RAM, ni ROM, ni lecteurs, ni périphériques, ni réglages vidéo ou audio, ni identifiant technique.

Si la configuration supprimée était chargée dans l’éditeur de sa marque :

- les données correspondantes sont retirées de la mémoire ;
- les champs reviennent à leurs valeurs de base ;
- le bouton **Créer** réapparaît puisque cette machine ne possède plus de configuration.

Lorsque l’utilisateur ouvre l’onglet d’une marque, l’application relit les configurations existantes. La présence du bouton **Créer** dépend donc de l’existence réelle de la configuration de la machine affichée.

### Points restant à décider

- la présentation graphique définitive du tableau et de ses icônes.

## Checklist détaillée — Point 3 : tableau des configurations

Dans l’ordre général, ce point constitue le groupe 2. Il utilise l’état fiable des configurations établi au point 2 et doit être terminé avant le retour automatique du focus du point 1. La présentation textuelle actuelle est remplacée par un tableau sans sélection de ligne. Seuls le filtre de marque, le crayon, le double-clic et la poubelle produisent une action.

- [x] Préparer les fonctions de présentation communes
  - [x] Créer le fichier commun avant d’y déplacer les fonctions
    - [x] Créer src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs vide.
  - [x] Déplacer entièrement chaque fonction avant de passer à la suivante
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs et src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour déplacer DisplayValue dans le fichier commun avec exactement les mêmes choix, valeurs de repli et règles de localisation, remplacer ses appels puis supprimer immédiatement sa définition privée d’origine.
    - [x] Modifier src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineEditingContext.cs pour remplacer le record qui hérite de EventArgs par une classe sealed conservant le même constructeur et les mêmes propriétés ModuleDisplayName et MachineDisplayName.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement complet de DisplayValue.
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs et src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour déplacer DefaultNumericValue dans le fichier commun avec exactement les mêmes sources numériques et la même valeur de repli, remplacer son appel puis supprimer immédiatement sa définition privée d’origine.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement complet de DefaultNumericValue.
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs et src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour déplacer FormatMemorySize dans le fichier commun avec exactement les mêmes seuils, formats et unités, remplacer son appel puis supprimer immédiatement sa définition privée d’origine.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement complet de FormatMemorySize.

- [x] Préparer les constantes et le style déjà utilisés
  - [x] Déplacer le glyphe du clavier vers sa portée commune
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationInputSettingsConstants.cs et src/GWGUI.App/Constants/Emulation/EmulationMachineTabConstants.cs pour déplacer la valeur U+E765 dans KeyboardIcon, remplacer immédiatement la valeur littérale de l’onglet Clavier puis ne conserver qu’une seule définition du glyphe.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement du glyphe Clavier.
  - [x] Ajouter le glyphe de modification manquant
    - [x] Modifier src/GWGUI.App/Constants/Controls/Visual/ControlVisualConstants.cs pour ajouter EditGlyph avec la valeur U+E70F déjà utilisée par l’action de modification, sans modifier DeleteGlyph.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier EditGlyph.
  - [x] Déplacer le style d’en-tête vers les ressources globales
    - [x] Modifier src/GWGUI.App/App.xaml et src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditor.xaml pour déplacer TableHeaderText dans Application.Resources avec FontWeight à SemiBold, VerticalAlignment à Center et Margin à 14,0, laisser InputBindingEditor utiliser la ressource déplacée puis supprimer immédiatement sa déclaration locale.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement complet de TableHeaderText.

- [x] Ajouter les textes visibles du nouveau tableau
  - [x] Créer les ressources de base
    - [x] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx pour créer Emulation.Configuration.Brand, Emulation.Configuration.Machine, Emulation.Configuration.TotalRam, Emulation.Configuration.Readers, Emulation.Configuration.Peripherals, Emulation.Configuration.Actions et Emulation.Configuration.DeleteConfirm ; DeleteConfirm reçoit uniquement la marque et la machine.
  - [x] Ajouter les sept ressources dans toutes les langues prises en charge
    - [x] Modifier src/GWGUI.App/Resources/ar-SA/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/cs-CZ/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/da-DK/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/de-DE/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/el-GR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/en-US/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/es-ES/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/fi-FI/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/fr-FR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/he-IL/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/hu-HU/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/id-ID/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/it-IT/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/ja-JP/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/ko-KR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/nb-NO/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/nl-NL/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/pl-PL/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/pt-BR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/pt-PT/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/ro-RO/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/ru-RU/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/sv-SE/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/th-TH/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/tr-TR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/uk-UA/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/vi-VN/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/zh-Hans/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/zh-Hant/Emulation.resx.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier toutes les ressources ajoutées.

- [x] Créer les données structurées des lignes
  - [x] Créer le contrat avant son contenu
    - [x] Créer src/GWGUI.App/Contracts/Emulation/Configurations/EmulationConfigurationTableRow.cs vide.
  - [x] Définir uniquement les données nécessaires aux colonnes et aux actions
    - [x] Modifier src/GWGUI.App/Contracts/Emulation/Configurations/EmulationConfigurationTableRow.cs pour porter IEmulationModule, IEmulationConfiguration, le nom localisé de la machine, le CPU, la RAM totale, la liste des glyphes de lecteurs et la liste des glyphes de périphériques.
  - [x] Créer le présentateur avant son contenu
    - [x] Créer src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs vide.
  - [x] Produire Machine, CPU et RAM totale depuis les données structurées
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour créer une ligne depuis IEmulationModule et IEmulationConfiguration, retrouver la machine dans IEmulationModule.Machines et localiser sa DisplayResourceKey sans analyser EmulationConfigurationSummary ni le DisplayName textuel existant.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationHardwareSettingsConstants.cs et src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour déplacer la clé existante Emulation.Cpu.Model dans CpuModelResourceKey, remplacer immédiatement son utilisation actuelle puis ne conserver qu’une seule définition de cette valeur.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement de CpuModelResourceKey.
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour obtenir le CPU depuis le champ visible Emulation.Cpu.Model de l’onglet CPU renvoyé par IEmulationModule.Describe et le formater avec EmulationSettingsValuePresentationFunctions.DisplayValue.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationMemorySettingsConstants.cs pour ajouter ValueUnitSeparator avec un espace unique destiné à séparer la valeur de RAM de son unité dans le tableau.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier ValueUnitSeparator.
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour additionner les valeurs numériques des champs visibles de l’onglet RAM avec EmulationSettingsValuePresentationFunctions.DefaultNumericValue, formater le total avec FormatMemorySize et laisser CPU ou RAM vide uniquement en l’absence réelle de donnée correspondante.
  - [x] Produire exactement une icône par lecteur configuré
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour appeler IEmulationStorageSettingsManager.DescribeStorageSettings lorsque le module fournit ce service, parcourir ConfiguredSlots, retrouver chaque périphérique par EmulationMediaSlot dans AvailableDevices et produire un glyphe par occurrence avec FloppyGlyph, HardDiskGlyph, CompactDiscGlyph, CassetteGlyph ou CartridgeGlyph selon EmulationMediaType.
  - [x] Produire exactement une icône par périphérique configuré
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour appeler IEmulationInputSettingsManager.DescribeInputSettings lorsque le module fournit ce service, ajouter KeyboardIcon lorsque Keyboard existe et MouseIcon lorsque Mouse existe.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationInputSettingsConstants.cs pour ajouter NoneControllerResourceKey avec Emulation.Controller.None, KeyboardControllerId avec Keyboard et MouseControllerId avec Mouse afin d’identifier les choix de port sans valeur brute.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier les identifiants de contrôleur.
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour résoudre chaque SelectedControllerId dans ControllerChoices, ignorer le choix dont la ressource est Emulation.Controller.None et ajouter pour chaque autre port le glyphe clavier, souris ou manette correspondant au choix.
  - [x] Classer et limiter les lignes
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour classer les lignes par nom de machine avec StringComparer.CurrentCulture et ne produire aucun identifiant technique affichable, ROM, moteur vidéo, format vidéo, état audio ou action de lancement.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le contrat et le présentateur.

- [x] Créer le tableau sans mécanisme de sélection
  - [x] Créer le contrôle avant son contenu
    - [x] Créer src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs vide.
  - [x] Construire les six colonnes et les lignes
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour recevoir des EmulationConfigurationTableRow, utiliser un ItemsControl dans un ScrollViewer et ne créer ni SelectedItem, ni SelectedIndex, ni état visuel de sélection.
    - [x] Créer src/GWGUI.App/Constants/Emulation/EmulationConfigurationTableConstants.cs vide.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationConfigurationTableConstants.cs pour définir dans l’ordre les six clés d’en-tête, TableHeaderTextStyleResource, CellMargin, HeaderSeparatorThickness et RowSeparatorThickness avec les valeurs déjà utilisées par les tableaux existants.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour créer dans l’ordre Machine, CPU, RAM totale, Lecteurs, Périphériques et Actions en utilisant les ressources Emulation.Configuration correspondantes, Emulation.Tab.Cpu, TableHeaderText, CardBrush et BorderBrush.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour afficher Machine, CPU et RAM totale, puis chaque glyphe de lecteur et de périphérique séparément sans nombre, texte permanent, infobulle de port ni information supplémentaire.
  - [x] Ajouter uniquement les trois interactions autorisées
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour ajouter EditRequested et DeleteRequested en transmettant directement la EmulationConfigurationTableRow concernée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour créer le bouton crayon avec EditGlyph et le bouton poubelle avec DeleteGlyph dans la colonne Actions.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour envoyer le bouton crayon et le double-clic de ligne vers le même chemin interne qui déclenche EditRequested.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour traiter l’action du bouton poubelle avant DeleteRequested afin qu’un double-clic sur ce bouton ne remonte jamais vers EditRequested.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour ne déclencher aucune action lors d’un simple clic ailleurs dans une ligne.
  - [x] Actualiser les textes du contrôle
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour ajouter RefreshLocalizedContent et y reconstruire les six en-têtes avec les ressources de la langue active.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationConfigurationTableConstants.cs pour définir CellMargin avec les quatre côtés attendus par Thickness.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le contrôle avant son raccordement.

- [x] Préparer l’ouverture complète d’une configuration
  - [x] Déplacer entièrement la création différée d’une section de marque
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour déplacer le bloc de création de EmulationModuleSettingsSection depuis ModuleTabSelectionChanged vers GetOrCreateModuleSection, conserver les abonnements ConfigurationSaved et EditingContextChanged, l’ajout dans _moduleSections et l’affectation à TabItem.Content, remplacer immédiatement l’ancien bloc par l’appel à la méthode puis supprimer le bloc d’origine.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier que l’ouverture manuelle des onglets de marque utilise GetOrCreateModuleSection et conserve ReloadWhenOpenedAsync.
  - [x] Ajouter l’ouverture explicite de la configuration choisie
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter EditConfigurationAsync recevant IEmulationConfiguration, recharger _saved, retenir exactement la configuration transmise, sélectionner sa machine, fixer _selectedTab à EmulationMachineTab.General, reconstruire tous les sous-onglets et actualiser l’état de l’émulateur installé sans lancer la machine.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier l’entrée explicite de l’éditeur.
  - [x] Ajouter la remise à zéro après une suppression
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter ReloadAfterConfigurationDeletedAsync recevant l’identifiant et la machine supprimés, retirer le brouillon de cette machine uniquement si l’identifiant supprimé est actuellement chargé, puis réutiliser ReloadAsync afin de recharger _saved, reconstruire les valeurs de base de cette machine et faire réapparaître Créer.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour typer configurationId en Guid comme IEmulationConfiguration.Id.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier la remise à zéro ciblée.

- [x] Préparer le nouveau contenu avant de remplacer l’ancienne liste
  - [x] Ajouter les champs du filtre et du tableau
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour ajouter le TextBlock du libellé Marque, le ComboBox de marque, la collection de EmulationModuleListItem des marques configurées, la liste complète de EmulationConfigurationTableRow et EmulationConfigurationTable sans supprimer encore _configurations, _configurationList ni _removeConfiguration.
  - [x] Alimenter le filtre et le tableau pendant le chargement existant
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour construire les nouvelles lignes avec EmulationConfigurationTablePresenter après chaque chargement tout en continuant provisoirement d’alimenter _configurations.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour reconstruire la liste des marques avec uniquement les modules possédant au moins une configuration, conserver la marque choisie si elle existe encore et laisser le ComboBox sans sélection si elle a disparu ou si aucune configuration n’existe.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour afficher uniquement les lignes de la marque choisie et laisser le tableau vide sans marque choisie.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour raccorder SelectionChanged du ComboBox à ce filtrage sans créer de sélection dans le tableau.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier les données du nouveau contenu.
  - [x] Raccorder l’ouverture de ligne
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour ajouter EditConfigurationAsync recevant EmulationConfigurationTableRow, obtenir la section de row.Module par GetOrCreateModuleSection, appeler son EditConfigurationAsync avec row.Configuration puis sélectionner le TabItem correspondant une fois la configuration chargée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour raccorder EditRequested du tableau à cette méthode unique afin que le crayon et le double-clic ne puissent pas diverger.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le raccordement de l’ouverture.
  - [x] Raccorder la suppression de ligne
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour ajouter DeleteConfigurationAsync recevant EmulationConfigurationTableRow et ouvrir une MessageBox Oui/Non utilisant Common.Delete comme titre et Emulation.Configuration.DeleteConfirm comme message avec uniquement la marque localisée et la machine localisée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour ne rien supprimer ni recharger lorsque la réponse n’est pas Oui, appeler row.Module.DeleteConfigurationAsync avec row.Configuration.Id uniquement après Oui et présenter toute erreur avec ControlErrorPresenter sans retirer la ligne lorsque la suppression échoue.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour recharger après une suppression réussie, conserver la marque tant qu’elle possède une ligne, laisser le ComboBox et le tableau sans sélection après sa dernière ligne sans choisir une autre marque, puis appeler ReloadAfterConfigurationDeletedAsync sur la section correspondante lorsqu’elle existe déjà.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour raccorder DeleteRequested à DeleteConfigurationAsync sans utiliser SelectedItem.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour importer System.Windows utilisé par la MessageBox de confirmation.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le raccordement de la suppression.
  - [x] Raccorder l’actualisation localisée
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour importer EmulationConfigurationTablePresenter utilisé par RefreshLocalizedContent.
    - [x] Modifier RefreshLocalizedContent dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour actualiser le libellé Marque, appeler EmulationConfigurationTable.RefreshLocalizedContent et reconstruire les marques et les lignes localisées en conservant la marque choisie si elle existe encore sans en sélectionner une autre.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier l’actualisation localisée du nouveau contenu.

- [x] Remplacer définitivement l’ancienne liste par le nouveau contenu
  - [x] Effectuer le remplacement complet dans une seule tâche
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionLayoutFunctions.cs, src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs et src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour ajouter le libellé Marque, son ComboBox et EmulationConfigurationTable dans BuildConfigurationsTab, puis supprimer immédiatement l’ancienne ListBox, le bouton Supprimer global, RemoveConfiguration, DeleteSelectedConfigurationAsync, l’alimentation de _configurations, les champs _configurations, _configurationList et _removeConfiguration, leur gestionnaire SelectionChanged, l’ancienne actualisation de Common.Delete et les directives using devenues inutiles.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le remplacement complet et la suppression de tout le fonctionnement fondé sur une ligne sélectionnée.

- [x] Espacer uniformément les icônes des lecteurs et des périphériques
  - [x] Modifier docs/tasks/interface/emulation/configuration-table.md pour inscrire, avant toute correction, l’ajout d’un espacement commun de 8 pixels, son application aux deux colonnes, la compilation, la vérification visuelle puis la fermeture.
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationConfigurationTableConstants.cs pour ajouter un espacement horizontal de 8 pixels entre deux icônes d’une même cellule.
  - [x] Modifier GlyphCell dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour appliquer cet espacement entre les icônes sans marge extérieure supplémentaire, afin que Lecteurs et Périphériques utilisent exactement la même règle.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cet espacement.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier dans le tableau Configuration que toutes les icônes multiples de Lecteurs et de Périphériques possèdent le même espacement, sans modifier le nombre ni l’ordre des icônes.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.
