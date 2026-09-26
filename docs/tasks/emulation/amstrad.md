# Réalisation du module d’émulation Amstrad CPC/GX4000

Cette feuille exécute [`../../project/amstrad-emulation.md`](../../project/amstrad-emulation.md).
Elle couvre uniquement les CPC classiques, les CPC Plus et la GX4000 avec Caprice32.

- [x] 1. Fixer le périmètre
  - [x] Retenir Caprice32 comme seul émulateur de cette étape.
  - [x] Séparer CPC classiques, CPC Plus et GX4000 dans `Common/Machines`.
  - [x] Reporter PCW, PcW16, NC, PDA600, PC Amstrad et Mega PC.

- [ ] 2. Construire les catalogues matériels
  - [ ] Créer les catalogues CPC classiques, CPC Plus et GX4000.
  - [ ] Agréger les six modèles dans le catalogue commun.
  - [ ] Décrire RAM, clavier, contrôleurs et périphériques sans option native.

- [ ] 3. Adapter le socle commun
  - [ ] Retirer chaque donnée Amiga copiée dans le module Amstrad.
  - [ ] Implémenter configuration, résumé, réglages, entrées, médias et stockage CPC.
  - [ ] Implémenter persistance, cycle de vie, états et libération des ressources.

- [ ] 4. Intégrer Caprice32
  - [ ] Ajouter l’adaptateur et la traduction `cap32_model`/`cap32_ram`.
  - [ ] Ajouter téléchargement, installation et résolution de `cap32_libretro.dll`.
  - [ ] Ajouter l’hôte Libretro hors processus et ses callbacks.
  - [ ] Ajouter la façade et la factory du module.

- [ ] 5. Raccorder le module
  - [ ] Ajouter les ressources invariantes et traduites.
  - [ ] Ajouter le projet à la solution et aux tests.
  - [ ] Vérifier les six machines par tests ciblés.
  - [ ] Réussir le build Debug complet et produire `build/Debug/GW GUI/gwgui.exe`.
