# Contrôleurs d’émulation — travail restant

Le fonctionnement actuel est décrit dans [`../../../ui/controllers.md`](../../../ui/controllers.md).
Les actions ci-dessous sont reportées jusqu’à une nouvelle session de validation visuelle et
matérielle.

- [ ] 1. Corriger les associations lors d’un changement de type
  - [ ] 1.1 Reproduire avec une ancienne configuration
    - [ ] Modifier `docs/ui/controllers.md` avec la transition reproduite et le résultat attendu avant de corriger la reprise d’associations physiques génériques ou appartenant à l’ancien type.
  - [ ] 1.2 Appliquer la correction
    - [ ] Modifier les fonctions de changement de type sous `src/GWGUI.App/Controllers/Emulation/Input` et compléter `docs/ui/controllers.md` après vérification qu’aucune association incompatible n’est recyclée.

- [ ] 2. Corriger la disposition du tableau et du visualiseur
  - [ ] 2.1 Stabiliser les dimensions et le défilement
    - [ ] Modifier les vues sous `src/GWGUI.App/Views/Controls/Emulation/Input` et `EmulationControllerSettingsLayout.cs` pour garder le visualiseur visible à droite, faire défiler uniquement les lignes, rendre les listes de types et de visuels défilantes et conserver l’icône d’état entière.
  - [ ] 2.2 Ajuster l’image sans déformation
    - [ ] Modifier `ControllerVisualizer` sous `src/GWGUI.App/Views/Controls/Options/ControllerVisualization` pour agrandir l’image dans son espace fixe en conservant son rapport d’aspect, puis consigner le résultat dans `docs/ui/controllers.md`.

- [ ] 3. Corriger les surimpressions restantes
  - [ ] 3.1 Nettoyer les visuels communs et Konix
    - [ ] Modifier les profils et fonctions de rendu sous `src/GWGUI.App/Views/Controls/Options/ControllerVisualization` pour retirer le point blanc et la barre de manche indésirables et compléter les zones de boutons des deux Konix Speedking.
  - [ ] 3.2 Valider les rendus analogiques avec du matériel
    - [ ] Modifier `docs/ui/controllers.md` après vérification des sticks, manches, gâchettes et halos avec plusieurs périphériques physiques pour décrire les formes réellement validées.

- [ ] 4. Valider le parcours complet dans Amiga et Atari
  - [ ] 4.1 Tester chaque port et chaque source d’entrée disponible
    - [ ] Modifier `docs/project/testing.md` après vérification des changements de port, du défilement, des clics de zones, des captures, des anciennes configurations et des associations simultanées clavier, souris et GameInput dans Amiga et Atari.
