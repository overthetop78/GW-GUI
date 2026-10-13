# Axes analogiques : périphériques supplémentaires

Livré : affectations par direction, association automatique du sens opposé d’un axe physique, maintien des valeurs progressives et touches indépendantes. SDK et éditeur partagés ; Sony, Acorn, Apple, Nintendo, Sharp, Panasonic, Atari, Commodore et Sega raccordés pour leurs axes décrits. Pressions Sony, GameCube et Sega composées explicitement ; requêtes de pression prises en charge par le callback Sega. Capture des mouvements progressifs corrigée. 272 tests ciblés réussis et build Debug des 23 modules réussi.

- [ ] Compléter les périphériques analogiques particuliers réellement pris en charge par les cœurs.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Settings.cs` et `InputFunctions.Snapshot.cs` dans le même dossier : qualifier les commandes du Mission Stick, Arcade Racer, XE-1AP et des autres périphériques proposés par les adaptateurs ; exposer et composer uniquement leurs axes effectivement disponibles, avec les paires de directions correspondantes.
  - [ ] Modifier les définitions et convertisseurs des autres périphériques concernés après qualification de leurs canaux : préserver la distinction entre position analogique, mouvement relatif et commandes numériques, sans annoncer de capacité absente.
- [ ] Vérifier et livrer les compléments.
  - [ ] Modifier `tests/GWGUI.Tests/Emulation/ControllerInput/LegacyAnalogAxisTests.cs` avec les cas des canaux nouvellement raccordés et leurs limites.
  - [ ] Produire Debug avec tous les modules, vérifier les fichiers livrés et commiter chaque module corrigé sans push.
  - [ ] Supprimer `docs/tasks/emulation/axes-analogiques.md` quand tous les périphériques concernés sont effectivement traités.
