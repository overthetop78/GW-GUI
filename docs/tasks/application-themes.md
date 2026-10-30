# Six thèmes : Base et cinq interfaces nouvelles

Les commits de thèmes `410ad59e0` et `e6325cc48` ont été retirés de la branche locale sans supprimer le travail. Aucun nouveau commit. Référence conservée : `42a173148`.

- [x] Conserver Base et remplacer les cinq propositions refusées.
  - [x] Modifier `src/GWGUI.Infrastructure/Settings/AppThemeDesign.cs`, `AppSettings.cs` et `src/GWGUI.App/Dictionaries/Theming/ThemeCatalog.cs` : Base, Dock, Flow, Desktop, Instrument et Index ; identifiants numériques stables, design distinct de la variante.
  - [x] Supprimer les définitions et captures de `src/GWGUI.App/Theme/{Graphite,Cobalt,Paper,Phosphor,Prism}` et l’ancienne navigation partagée.
  - [x] Restaurer `src/GWGUI.App/Views/Controls/Common/MainTabHeader.xaml` à sa version avant les thèmes ; les nouveaux thèmes dessinent leurs propres en-têtes.
- [x] Mettre les présentations dans les dossiers des thèmes.
  - [x] Créer `src/GWGUI.App/Theme/Dock/{Workspace,Forms,Window,Layout,Dark}.xaml` : atelier à panneaux ancrés et inspecteur séparé.
  - [x] Créer `src/GWGUI.App/Theme/Flow/{Workspace,Forms,Window,Layout,Light}.xaml` : formulaires organisés en étapes distinctes.
  - [x] Créer `src/GWGUI.App/Theme/Desktop/{Workspace,Forms,Window,Layout,Light,Dark}.xaml` : cadre de document interne et barre des opérations.
  - [x] Créer `src/GWGUI.App/Theme/Instrument/{Workspace,Forms,Window,Layout,Dark}.xaml` : réglages compacts en lignes et cadre de fenêtre avec commandes en bas.
  - [x] Créer `src/GWGUI.App/Theme/Index/{Workspace,Forms,Window,Layout,Light}.xaml` : pages indexées, champs en lignes et cadre latéral.
  - [x] Créer `src/GWGUI.App/Theme/SharedControls.xaml`, `WindowChrome.xaml` et `.xaml.cs` : contrôles, focus, cases, panneaux repliables et commandes système ; chaque thème possède son gabarit de fenêtre.
  - [x] Créer `src/GWGUI.App/Contracts/Theming/ThemeFormContent.cs`, `Functions/Theming/ThemeFormLayout.cs` et `ThemeFieldLayout.cs` : fournir les mêmes contrôles métier aux gabarits, conserver leurs modèles et commandes, restaurer les parents et positions de Base.
  - [x] Modifier `src/GWGUI.App/Services/Theming/ThemeManager.cs`, `Constants/Theming/*`, `Theme/ThemeTokens.xaml`, `App.xaml` et `.xaml.cs` : priorité correcte des styles, application à chaud et restauration du design initial.
  - [x] Modifier `src/GWGUI.App/Views/Windows/Shell/MainWindow.xaml` et ses fichiers Theming, les contrôles Read/Write/Conversion, `Services/Terminal/TerminalPanelController.cs` et les réglages de console : compositions sans duplication des écrans ou commandes ; largeur et hauteur de console indépendantes.
- [x] Choisir et conserver le thème.
  - [x] Créer `src/GWGUI.App/Views/Controls/Options/OptionsThemesSection.xaml` et `.xaml.cs` : liste, aperçu, informations, variantes autorisées et action Choisir ; sélectionner seul ne sauvegarde pas.
  - [x] Modifier `src/GWGUI.App/Views/Windows/Preferences/PreferencesWindow.xaml` et ses fichiers C# : onglet Thèmes, navigation propre au design, sauvegarde explicite et restauration après erreur.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Options/OptionsGeneralSection.xaml`, `.xaml.cs` et `Options/Controllers/GeneralOptionsController.cs` : retirer l’ancien sélecteur sans écraser le nouveau choix.
  - [x] Modifier `src/GWGUI.App/Resources/*/Options.resx` : traductions Argos puis revue des 28 langues ; noms invariants en base commune, aide cohérente avec Choisir.
  - [x] Modifier `src/GWGUI.App/GWGUI.App.csproj` et créer les huit `src/GWGUI.App/Theme/*/*Preview.png` : captures réelles incorporées, au maximum 1600×900.
- [x] Vérifier et nettoyer.
  - [x] Modifier les scénarios existants de `tests/GWGUI.Tests/Interface/SettingsViews/` et créer `Interface/Themes/ThemeDesignTests.cs` : sauvegarde, reprise après erreur et rechargement du couple design/variante dans le stockage mémoire.
  - [x] Créer puis supprimer `tests/GWGUI.Tests/Interface/Themes/TemporaryThemeRedesignTests.cs` : parcours des sept fonctions et des étapes Flow, identité des contenus et modèles, retour à Base identique pixel par pixel, aperçus embarqués et véritables dialogues à 1120×650 et 980×560 ; fermeture des fenêtres et libération de leurs handles.
  - [x] Régénérer les aperçus Base avec le style Window de l’application, absent du fixture de test, puis restaurer le style emprunté en finally ; nouveau contrôle de rendu réussi.
  - [x] Supprimer `artifacts/theme-redesign/` et les journaux temporaires de traduction/validation, en conservant les aperçus livrés dans Theme.
- [x] Livrer sans commit.
  - [x] Modifier ce document avec le résultat final du script officiel Debug et vérifier `build/Debug/GW GUI/gwgui.exe`, `lib/gwgui.app.dll` et les huit aperçus incorporés.

Validation : 149 contrôles réussis (thèmes, préférences et navigation). Le retour à Base est comparé pixel par pixel avant/après tous les thèmes ; les formulaires conservent les instances métier et le modèle initial. Contrôle supplémentaire réussi après mise en conformité des captures Base avec le style Window réel de l’application. Audit XML : six designs exactement, clés et traductions présentes, anciens noms retirés, noms invariants uniquement en base commune.

Build final réussi (code 0) : `scripts/local-building.cmd --building=debug --modules=0`. Exécutable et DLL vérifiés : `F:\GW GUI\build\Debug\GW GUI\gwgui.exe` et `lib/gwgui.app.dll`. Les huit aperçus embarqués correspondent exactement aux PNG sources (comparaison SHA-256). Aucun commit supplémentaire ; la branche reste sur `42a173148`.

# Corrections et séparation thème / déclinaison / style

Demande : corriger tous les défauts constatés sur les vraies fenêtres, différencier réellement les organisations et livrer avec Acorn (build de tous les modules). Les exemples historiques ne sont pas une liste imposée. Thème = organisation ; déclinaison = habillage interne au thème ; style = clair/sombre. Aucun commit.

- [x] Corriger l’application réelle des habillages.
  - [x] Modifier `src/GWGUI.App/Services/Theming/ThemeManager.cs`, `Constants/Theming/ThemeConstants.cs`, `App.xaml` et `App.xaml.cs` : appliquer le thème à chaque fenêtre dérivée lors de son ouverture sans dépendre du style implicite Window ; conserver les surfaces explicitement sans bordure.
  - [x] Créer `src/GWGUI.App/Theme/Base/Inventory.md` : inventaire de toutes les fenêtres, dialogues, vues et des 24 modules ; distinguer interface et rendu matériel après audit des émulateurs.
  - [x] Déplacer `src/GWGUI.App/Views/` vers `src/GWGUI.App/Theme/Base/Views/` : rassembler la présentation d’origine et les interfaces génériques des modules sans modifier leurs namespaces ou leurs commandes.
  - [x] Modifier `src/GWGUI.App/App.xaml`, `Constants/Emulation/EmulationNetworkLayoutConstants.cs`, le dialogue de création de média déplacé et `tests/GWGUI.Tests/Application/TestInfrastructure/StaExecutionScenarios.cs` : corriger les URI des ressources Base déplacées.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Styles/ApplicationStyles.xaml` : boutons et états désactivés lisibles en Base sombre ; fonds et textes cohérents.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Light.xaml`, `Dark.xaml`, `ThemeConstants.cs`, `Services/Theming/ThemeManager.cs` et les vues Base de visualisation/terminal : centraliser les couleurs de surface et de danger en ressources, conserver les couleurs matérielles.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Common/DiskClassificationSelector.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Common/FileEntryIcon.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Common/MediaDocumentIdentity.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Common/MediaRecognitionProgressCard.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Emulation/Input/InputBindingEditor.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Explorer/ExplorerSection.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Options/OptionsGeneralSection.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Options/OptionsHardwareSection.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Tools/ToolsTabSection.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Visualization/BlockMediaView.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Visualization/MediaInspectorPanel.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Visualization/OpticalMediaView.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Visualization/ScpDiskView.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Visualization/SectorMediaView.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Visualization/SequentialMediaView.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Visualization/VisualizerHeaderSection.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Visualization/VisualizerLegend.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Visualization/VisualizerTabSection.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Controls/Write/WriteAdvancedSection.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Dialogs/Explorer/ExplorerIssuesWindow.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Windows/About/AboutWindow.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Modifier `src/GWGUI.App/Theme/Base/Views/Windows/Tools/GwToolWindow.xaml` : remplacer les pinceaux d’habillage fixes par des ressources sémantiques Base/thème.
  - [x] Créer `tests/GWGUI.Tests/Interface/Themes/TemporaryThemeRepairTests.cs` : test de fenêtres dérivées ; première compilation arrêtée par une qualification de namespace à corriger. La demande suivante remplace les cinq thèmes.

# Livraison de Base seule avant tout nouveau thème

Ordre demandé : supprimer les cinq thèmes refusés, terminer Base, tester, construire tous les modules, attendre la validation utilisateur puis seulement committer. Les prochains thèmes seront réalisés un à un après cette validation.

- [x] Retirer les cinq thèmes et leur code de présentation.
  - [x] Supprimer `src/GWGUI.App/Theme/{Dock,Flow,Desktop,Index,Instrument}/` et leurs traductions `Options.Themes.<nom>.*` de `src/GWGUI.App/Resources/*/Options.resx` ; ne garder que Base dans `AppThemeDesign.cs` et `ThemeCatalog.cs`.
  - [x] Modifier `Services/Theming/ThemeManager.cs`, `Constants/Theming/ThemeConstants.cs` et `Contracts/Theming/ThemeDefinition.cs` : conserver le chargement clair/sombre Base et le traitement automatique des fenêtres ; retirer les chargements des thèmes supprimés.
  - [x] Supprimer `Theme/{SharedControls,WindowChrome}.xaml`, `Theme/WindowChrome.xaml.cs`, `Functions/Theming/{ThemeFormLayout,ThemeFieldLayout}.cs`, `Contracts/Theming/ThemeFormContent.cs`, `Constants/Theming/ThemeShellConstants.cs` et les deux fichiers `Theme/Base/Views/Windows/{Shell/MainWindow,Preferences/PreferencesWindow}.Theming.cs` ; retirer leurs appels des vues et restaurer `Services/Terminal/TerminalPanelController.cs` et `Infrastructure/Settings/Window/ConsolePanelSettings.cs` avant ces thèmes.
  - [x] Modifier les tests `Interface/Themes/{ThemeDesignTests,TemporaryThemeRepairTests}.cs`, `Interface/SettingsViews/{SettingsEditingScenarios,SettingsFailureScenarios}.cs` : exercer Base et ses styles, le repli des anciens identifiants, la sauvegarde et l'ouverture de fenêtres sans style imposé par le test.
- [x] Terminer les ressources de Base et vérifier l'affichage.
  - [x] Modifier `Theme/Base/Views/Controls/Common/{MediaRecognitionProgressCard.xaml,FileEntryIcon.xaml.cs}` et `Theme/Base/Views/Windows/About/AboutWindow.xaml` : supprimer les dernières couleurs fixes des surfaces et icônes d'état.
  - [x] Modifier `Theme/Base/Inventory.md` : inventaire de la présentation conservée et résultat détaillé de l'audit des émulateurs ; aucune palette matérielle modifiée.
  - [x] Déplacer `src/GWGUI.App/Functions/Views/` et `Factories/Views/` vers `Theme/Base/{Functions,Factories}/Views/`, ainsi que `Functions/Emulation/Shortcuts/EmulationShortcutViewFunctions.cs` vers `Theme/Base/Functions/Emulation/Shortcuts/` : regrouper aussi les constructeurs de présentation C# sans changer leurs namespaces.
  - [x] Modifier `Theme/Base/Functions/Views/Emulation/Storage/StorageDialogUi.cs`, `Theme/Base/Views/Styles/ApplicationStyles.xaml`, `Theme/ThemeTokens.xaml`, `Theme/Base/Views/Controls/Options/OptionsUpdatesSection.xaml.cs`, ses modèles et son XAML : ressources des panneaux d'aide, boutons du terminal et états de mise à jour ; couleurs dynamiques lors du changement clair/sombre.
  - [x] Modifier `Theme/Base/Functions/Views/Emulation/Settings/{EmulationFirmwareSettingsLayout,EmulationMachineChoiceLayout}.cs`, `ViewModels/Input/InputBindingRow.cs` et `Theme/Base/Views/Controls/Emulation/Input/InputBindingEditor.xaml` : badges et contrôles de configuration utilisant les couleurs dynamiques Base.
  - [x] Modifier `tests/GWGUI.Tests/Interface/Navigation/ControlContractScenarios.cs` : vérifier les états typés des mises à jour, les couleurs appartenant désormais aux vues et non aux modèles.
  - [x] Modifier `Interface/Themes/TemporaryThemeRepairTests.cs` si nécessaire : vérifier les fenêtres réelles Base clair/sombre à taille limitée, produire des captures sous `artifacts/theme-repair/`, fermer et libérer chaque fenêtre dans finally.
  - [x] Modifier `tests/GWGUI.Tests/Interface/SettingsViews/SettingsFailureScenarios.cs` : retrouver l'option sombre courante après le rafraîchissement de la vue, sans conserver une ancienne case retirée de l'arbre.
  - [x] Compléter `tests/GWGUI.Tests/Interface/Themes/TemporaryThemeRepairTests.cs` : captures des sept fonctions, Préférences à 980×560 et changement clair/sombre à chaud ; restaurer les vues empruntées et fermer les fenêtres possédées.
  - [x] Déplacer `Theme/ThemeTokens.xaml` vers `Theme/Base/Tokens.xaml`, modifier `App.xaml` et `Constants/Theming/ThemeConstants.cs` : conserver les ressources de référence dans Base.
  - [x] Modifier `tests/GWGUI.Tests/Interface/Themes/ThemeDesignTests.cs` : garantir le repli vers Base pour chacun des cinq identifiants supprimés et conserver le style enregistré.
  - [x] Modifier `Theme/Base/Inventory.md` : corriger les chemins déplacés et les comptes de fichiers après le retrait des gabarits.
  - [x] Remplacer `Theme/Base/{LightPreview,DarkPreview}.png` par les nouveaux aperçus vérifiés ; supprimer `Interface/Themes/TemporaryThemeRepairTests.cs` et `artifacts/theme-repair/` après validation des tests.
- [ ] Livrer Base pour validation, sans commit préalable.
  - [x] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuWd1770Tests.cs` : le test conserve une attente antérieure au filtrage demandé ; vérifier le refus du modèle double densité avec 8271 et son acceptation après sélection du WD1770, sans modifier le moteur Acorn inchangé.
  - [x] Modifier ce document avec le résultat des contrôles de navigation, préférences, Base et Acorn, puis du script `scripts/local-building.cmd --building=debug --modules=A` ; vérifier l'exécutable et les 24 modules.
  - [ ] Après validation explicite de l'utilisateur : créer le commit Base ; conserver les prochains thèmes en attente jusqu'à cette validation.

## Résultat de la livraison Base seule

- Les cinq thèmes refusés sont supprimés, y compris leurs gabarits, leurs références et leurs traductions. Le dossier Theme contient uniquement Base.
- Base rassemble les fenêtres, dialogues, contrôles et constructeurs de présentation : 64 vues XAML et 377 fichiers C#. Les commandes et moteurs restent inchangés.
- Fenêtres dérivées ouvertes après sélection de Base clair/sombre, changement à chaud et fermeture effective contrôlés sans appliquer manuellement leur style dans le test d'ouverture. Captures des sept fonctions et de Préférences à 1120×650 et 980×560 examinées ; aperçus Base mis à jour.
- 170 contrôles ciblés application réussis, dont quatre tests temporaires de rendu et cycle de vie. 296 contrôles Acorn réussis. Le test WD1770 a été corrigé pour tenir compte du filtrage déjà présent dans le moteur ; aucun changement au moteur Acorn.
- 97 fichiers XML valides ; ressources des cinq thèmes retirées dans toutes les langues. Test temporaire et artefacts de capture supprimés.
- Script officiel terminé avec le code 0 : `scripts/local-building.cmd --building=debug --modules=A`.
- Vérifiés après assemblage : `F:\GW GUI\build\Debug\GW GUI\gwgui.exe`, `lib/gwgui.app.dll`, les 24 manifestes et leurs DLL déclarées, dont Acorn et Amstrad. Les versions livrées correspondent aux manifestes sources.
- Aucun commit créé : HEAD reste `42a173148`. Validation utilisateur de Base en attente ; le commit Base puis le prochain thème restent conditionnés à cette validation.

Validation explicite de Base reçue de l'utilisateur le 30 octobre 2026. Le commit Base est maintenant autorisé, suivi du prochain thème seulement.

