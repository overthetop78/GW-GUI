# Sélecteur des configurations et suppression

- [ ] Remplacer le sélecteur de marques et synchroniser les suppressions.
  - [x] Créer `src/GWGUI.App/Functions/Views/Emulation/Machine/EmulationSelectorButtonFunctions.cs` en y déplaçant le rendu existant des boutons avec image ; modifier `src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionLayoutFunctions.cs` pour réutiliser ce rendu.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationPreferencesSection.cs`, `EmulationPreferencesSectionLayoutFunctions.cs` et `EmulationPreferencesSectionConfigurationFunctions.cs` pour remplacer le ComboBox par les logos, conserver la sélection lors du rechargement et rafraîchir le tableau après suppression.
  - [ ] Modifier `src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSection.cs` et `EmulationSectionConfigurationFunctions.cs` pour recharger les logos et images des configurations après suppression, en détachant les abonnements lorsque la vue est déchargée.
  - [ ] Modifier `docs/tasks/emulation/configuration-selector.md` avec les vérifications des sources et les limites de validation, sans suppression de configuration personnelle ni build non demandé.

Le fichier `docs/tasks/emulation/commun.md` contient déjà un conflit Git à l’entrée de cette tâche ; il est laissé intact.
