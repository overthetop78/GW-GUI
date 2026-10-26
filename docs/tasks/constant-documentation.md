# Audit et documentation des constantes

L’audit initial de `src` et `tests` a parcouru 1 454 fichiers de constantes : 39 fichiers contenaient des déclarations regroupées et 18 316 déclarations n’avaient pas de documentation XML immédiatement associée. Les trois fichiers signalés ont été corrigés en premier. La portée des corrections historiques est en attente de la réponse à la question de périmètre.

- [x] Corriger les fichiers du travail en cours pendant la clarification du périmètre historique.
  - [x] Modifier `src/GWGUI.Emulation/Constants/EmulationMechanicalSoundConstants.cs` : séparer et documenter les relais SDK des constantes MediaEngine.
  - [x] Modifier `src/GWGUI.Emulation/Constants/EmulationTapeSettingsConstants.cs` : séparer et documenter les champs, groupes et présélections du lecteur de bande.
  - [x] Modifier `src/GWGUI.App/Constants/Emulation/StorageConfigurationLayoutConstants.cs` : séparer et documenter les dimensions, rapports et limites de présentation.
  - [x] Modifier `src/GWGUI.App/Constants/Emulation/MediaCreationConstants.cs` : séparer et documenter les dimensions et clés de traduction du dialogue.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Constants/StorageConstants.cs` : séparer et documenter les identifiants, valeurs, limites et unités de stockage.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Amstrad/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Bandai/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Example/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Mattel/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Microsoft/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Panasonic/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Philips/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Sharp/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Sinclair/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Snk/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Thomson/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Watara/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Wenquxing/Common/Constants/EmulationModuleConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation/Constants/EmulationAssetPaths.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Constants/MediaConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Amstrad/Common/Machines/Common/Constants/MachineCatalogConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.MediaEngine/Constants/DiskImageFileExtensions.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Constants/MachineCatalogConstants.cs` : documenter les constantes de chemins, identifiants ou formats et conserver leurs valeurs.
  - [x] Modifier ce document après vérification : consigner la conservation exacte des identifiants, types et expressions et la vérification XML, sans build de GW GUI.

Vérification effectuée : 37 fichiers de constantes du travail en cours, 539 déclarations individuelles documentées, XML valide. Les transformations ont vérifié la conservation des noms, types et expressions avant/après écriture. Aucun build de GW GUI. La reprise des fichiers historiques du dépôt attend la réponse à la question de périmètre.
