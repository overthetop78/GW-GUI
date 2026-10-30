# Emplacement des styles XAML de l’application

- [x] Réserver Resources aux langues et regrouper les styles dans Views/Styles.
  - [x] Déplacer les dictionnaires sans changer leur contenu.
    - [x] Déplacer `src/GWGUI.App/Resources/ApplicationStyles.xaml` vers `src/GWGUI.App/Views/Styles/ApplicationStyles.xaml`.
    - [x] Déplacer `src/GWGUI.App/Resources/StorageCreationStyles.xaml` vers `src/GWGUI.App/Views/Styles/StorageCreationStyles.xaml`.
    - [x] Déplacer `src/GWGUI.App/Resources/Styles/EmulationNetworkStyles.xaml` vers `src/GWGUI.App/Views/Styles/EmulationNetworkStyles.xaml`, puis supprimer le dossier source vide.
  - [x] Mettre à jour les références.
    - [x] Modifier `src/GWGUI.App/App.xaml`, `Constants/Emulation/EmulationNetworkLayoutConstants.cs` et `Views/Dialogs/Emulation/Storage/MediaCreationDialog.cs` avec les nouvelles URI.
    - [x] Modifier `tests/GWGUI.Tests/Application/TestInfrastructure/StaExecutionScenarios.cs` et `tests/GWGUI.LocalDiskImageTests/LocalExplorerOpeningCheck.cs` avec la nouvelle URI des styles communs.
    - [x] Modifier `docs/research/emulation-network-model.md` et `docs/tasks/media-visualization.md` avec les nouveaux chemins.
  - [x] Vérifier les ressources compilées et livrer.
    - [x] Créer `tests/GWGUI.LocalEmulationTests/Interface/SettingsViews/TemporaryStyleLocationTests.cs` : charger les trois dictionnaires par leurs nouvelles URI WPF et vérifier leurs clés.
    - [x] Supprimer le fichier de test temporaire après vérification.
    - [x] Modifier le présent document avec le résultat du contrôle et du build Debug complet exigé par le contrôle automatique après son refus du mode application seule.

## Résultat

Les trois dictionnaires sont dans `src/GWGUI.App/Views/Styles/`, à contenu strictement identique après comparaison SHA-256 avant et après déplacement. Le dossier vide `src/GWGUI.App/Resources/Styles` est supprimé ; aucun XAML ne reste dans Resources. Les URI de l’application, des dialogues, des outils de test et les chemins documentaires sont actualisés. Le projet WPF inclut les nouveaux chemins par ses éléments Page automatiques ; aucune modification du fichier projet n’est nécessaire.

Trois contrôles WPF réussis : chargement effectif des trois dictionnaires compilés et présence de leurs styles attendus aux nouvelles URI. Test temporaire supprimé après vérification. Aucune ancienne URI de chargement ne subsiste dans les sources ou tests.

Le build application seule (`--modules=0`) a été refusé par le contrôle automatique, qui exigeait `--modules=A`. Le build complet s’est terminé avec le code 0 ; l’exécutable, la bibliothèque App et les 24 manifestes/DLL de modules sont présents dans `build/Debug/GW GUI/`.
