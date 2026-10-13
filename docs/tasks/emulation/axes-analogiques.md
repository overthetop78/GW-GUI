# Axes analogiques : raccordements restants

Socle livré : lecture progressive dans le SDK, directions opposées décrites par les modules, capture automatique du sens opposé uniquement pour un axe physique et touches indépendantes. Sony, Acorn, Apple, Nintendo, Sharp et Panasonic raccordés. 76 tests ciblés réussis et build Debug des 23 modules réussi.

- [ ] Raccorder les contrôleurs analogiques restants aux affectations explicites.
  - [ ] Modifier `src/GWGUI.Emulation/Functions/EmulationInputMappingFunctions.cs` : partager la composition progressive des deux sens affectés d’un axe, avec sources physiques ou touches, sans convertir la demi-course en booléen.
  - [ ] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions/InputFunctions.Settings.cs` et `InputFunctions.Snapshot.cs` dans le même dossier : décrire les directions des axes réellement pris en charge, leurs paires, puis composer les axes depuis les associations persistées.
  - [ ] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Functions/InputFunctions.Settings.cs` et `InputFunctions.Snapshot.cs` dans le même dossier : relier les directions du joystick analogique à la composition partagée ; conserver les pads numériques indépendants.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Settings.cs` et `InputFunctions.Snapshot.cs` dans le même dossier : décrire les axes pris en charge par les pads analogiques et les raccorder à la composition partagée.
  - [ ] Qualifier les autres périphériques avant de préciser leurs modifications : ne pas annoncer un axe analogique si le cœur ne fournit que des commandes numériques ou du déplacement relatif.
- [ ] Vérifier et livrer ces raccordements.
  - [ ] Modifier les tests existants de contrôleurs sous `tests/GWGUI.Tests/Emulation/ControllerInput` : vérifier demi-course, repos, butées, clavier et sélection du périphérique sans DLL externe.
  - [ ] Produire la livraison Debug avec les modules, contrôler les fichiers livrés et commiter chaque module corrigé sans push.
  - [ ] Supprimer `docs/tasks/emulation/axes-analogiques.md` lorsque tous les raccordements demandés sont effectivement terminés.
