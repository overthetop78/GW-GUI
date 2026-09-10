# Atari — travail restant

L’architecture du module, sa publication indépendante, ses fenêtres de paramètres et les parcours
déjà validés sont documentés ailleurs et ne figurent plus ici. Ce plan est reporté; il ne bloque pas
le fonctionnement ou la publication actuelle.

- [ ] 1. Vérifier l’accessibilité et la mise en page du module Atari
  - [ ] 1.1 Contrôler les fenêtres avec les contenus réels
    - [ ] Modifier `docs/ui/emulation.md` après vérification du clavier, des noms accessibles, du DPI, des listes de machines, du défilement et des onglets variables pour consigner les corrections effectivement appliquées.

- [ ] 2. Valider les catalogues et les configurations Atari
  - [ ] 2.1 Parcourir les modèles et leurs champs
    - [ ] Modifier `docs/reference/emulation-machines.md` après vérification de chaque modèle Atari, de son moteur, de ses supports, de ses firmwares requis ou facultatifs et de la persistance de sa configuration.
  - [ ] 2.2 Vérifier les moteurs réellement proposés
    - [ ] Modifier `docs/reference/atari-libretro.md` après les essais de Hatari, Atari800, Stella, ProSystem, Handy et Virtual Jaguar avec les machines et médias compatibles disponibles.

- [ ] 3. Terminer les validations manuelles des familles Atari
  - [ ] 3.1 Valider les ordinateurs ST et 8 bits
    - [ ] Modifier `docs/reference/atari-libretro.md` après des sessions Hatari et Atari800 couvrant démarrage, arrêt, reset, médias, firmware, audio, vidéo et entrées.
  - [ ] 3.2 Valider les consoles à cartouche
    - [ ] Modifier `docs/reference/atari-libretro.md` après des sessions Atari 2600, Atari 7800, Lynx et Jaguar avec leurs formats de cartouche et leurs contrôleurs.
  - [ ] 3.3 Valider Jaguar CD
    - [ ] Modifier `docs/reference/atari-libretro.md` après un essai Jaguar CD complet avec BIOS et image CD valides, changement puis éjection du disque, en consignant toute erreur du cœur.

- [ ] 4. Corriger l’éjection Jaguar CD si le défaut est reproduit
  - [ ] 4.1 Diagnostiquer la transition exacte
    - [ ] Modifier `docs/reference/atari-libretro.md` avec le journal et la transition reproduite avant de modifier le code de montage ou d’éjection du moteur Jaguar.
  - [ ] 4.2 Appliquer et vérifier la correction
    - [ ] Modifier les fonctions Jaguar concernées sous `src/GWGUI.Emulation.Atari` puis compléter `docs/reference/atari-libretro.md` avec le test réussi de changement et d’éjection du disque.

- [ ] 5. Écrire le guide utilisateur Atari
  - [ ] 5.1 Documenter les parcours réellement validés
    - [ ] Créer `wiki/fr-FR/Emulation-Atari.md` et `wiki/en-US/Atari-Emulation.md` avec l’installation du module, le choix d’une machine, des firmwares, des médias, du moteur et des contrôleurs après validation des parcours correspondants.

- [ ] 6. Étudier Atari System 1 avec MAME et FBNeo
  - [ ] 6.1 Établir la faisabilité avant toute implémentation
    - [ ] Créer `docs/future/atari-system-1.md` avec les jeux visés, les formats de ROM, les exigences BIOS, les capacités comparées de MAME et FBNeo, les licences et les limites d’intégration constatées.
  - [ ] 6.2 Ouvrir un plan d’implémentation seulement après décision
    - [ ] Créer une nouvelle feuille sous `docs/tasks/emulation/` à partir de `docs/future/atari-system-1.md` uniquement après validation explicite du périmètre.
