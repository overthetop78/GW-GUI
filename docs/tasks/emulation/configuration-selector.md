# Sélecteur des configurations et suppression

- [x] Remplacer le sélecteur de marques et synchroniser les suppressions.
  - [x] Créer `src/GWGUI.App/Functions/Views/Emulation/Machine/EmulationSelectorButtonFunctions.cs` en y déplaçant le rendu existant des boutons avec image ; modifier `src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionLayoutFunctions.cs` pour réutiliser ce rendu.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationPreferencesSection.cs`, `EmulationPreferencesSectionLayoutFunctions.cs` et `EmulationPreferencesSectionConfigurationFunctions.cs` pour remplacer le ComboBox par les logos, conserver la sélection lors du rechargement et rafraîchir le tableau après suppression.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSection.cs` pour recharger les logos et images des configurations après suppression, en détachant les abonnements lorsque la vue est déchargée.
  - [x] Modifier `docs/tasks/emulation/configuration-selector.md` avec les vérifications des sources et les limites de validation, sans suppression de configuration personnelle ni build non demandé.

Le fichier `docs/tasks/emulation/commun.md` contient déjà un conflit Git à l’entrée de cette tâche ; il est laissé intact.


Résultat : les marques configurées sont présentées avec les mêmes logos, dimensions,
infobulles et état sélectionné que le sélecteur principal. Le tableau sélectionne la
première marque disponible à l’ouverture, conserve celle en cours après rechargement,
et passe à une marque restante lorsque la dernière configuration de la marque est supprimée.
Après suppression effective et rechargement du tableau, la notification attend le
rechargement du sélecteur principal. Son filtre existant retire l’image d’une machine
sans configuration et le logo d’une marque sans configuration. Si une autre configuration
du même modèle subsiste, son image reste disponible. Les abonnements de la vue principale
sont détachés au déchargement et rétablis au chargement, qui recharge également la liste.

Vérifications : analyse syntaxique Roslyn des six fichiers C# concernés et
`git diff --check` réussis ; absence des références à l’ancien ComboBox vérifiée.
Aucune nouvelle traduction : réutilisation des libellés et infobulles localisés existants.
Aucune configuration personnelle supprimée, aucun test visuel ni build lancé.
Le conflit initial du suivi commun a été résolu dans une fusion externe pendant le travail ;
ce fichier commun n’a pas été modifié par cette tâche.


- [x] Réutiliser le bloc de sélection complet entre les deux écrans.
  - [x] Créer `src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationConfigurationSelector.cs` avec les deux lignes Marque/Machine, les logos, les images et les événements de sélection, en réutilisant le rendu des boutons existant.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSection.cs`, `EmulationSectionLayoutFunctions.cs` et `EmulationSectionConfigurationFunctions.cs` pour héberger le contrôle commun tout en conservant ouverture, double-clic et repli du bloc.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationPreferencesSection.cs`, `EmulationPreferencesSectionLayoutFunctions.cs` et `EmulationPreferencesSectionConfigurationFunctions.cs` pour héberger le même bloc et filtrer le tableau par marque et machine, en supprimant le rendu dupliqué.
  - [x] Modifier `docs/tasks/emulation/configuration-selector.md` avec les contrôles statiques effectués et le fonctionnement partagé, sans build non demandé.


Le contrôle partagé `EmulationConfigurationSelector` possède désormais les deux lignes
Marque/Machine, leurs libellés localisés, le chargement des images, les états sélectionnés
et les événements de clic/double-clic. Les deux écrans lui transmettent les marques et
modèles encore configurés. L’écran principal conserve son bouton d’ouverture et son
repli ; les préférences utilisent la sélection de machine pour filtrer le tableau.
Un second clic sur la machine sélectionnée retire ce filtre. Le rechargement après
suppression réinitialise ce filtre si la dernière configuration de la machine disparaît.
Le rendu des boutons reste commun à ce contrôle et n’est plus appelé directement par
les deux écrans. Aucun accès aux modules concrets n’a été ajouté.

Contrôles : syntaxe Roslyn des sept fichiers C# valide, `git diff --check` sans erreur,
références aux anciens panneaux et aux anciennes méthodes de rendu dupliquées absentes.
Aucun build ni test visuel lancé ; aucune configuration personnelle supprimée.
