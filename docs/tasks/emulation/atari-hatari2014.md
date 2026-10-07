# Hatari 2014

- [x] Ajouter Hatari 2014 aux machines compatibles
  - [x] Preparer les sources et la validation
    - [x] Creer `artifacts/temp/atari-hatari2014-validation` : conserver temporairement les sources officielles, la DLL Windows x64 et les fichiers de verification.
  - [x] Partager les traitements natifs identiques
    - [x] Creer `artifacts/temp/atari-hatari2014-validation/emutos.zip` : telecharger EmuTOS libre pour la validation native avec TOS externe.
    - [x] Deplacer `src/GWGUI.Emulation.Atari/Emulators/Hatari/Constants/HatariOptionConstants.cs` vers `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Constants/StLegacyOptionConstants.cs` : partager les options natives identiques des deux ports Hatari.
    - [x] Deplacer `src/GWGUI.Emulation.Atari/Emulators/Hatari/Functions/HatariOptionFunctions.cs` vers `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Functions/StLegacyOptionFunctions.cs` : partager les conversions sans reference a un profil concret.
    - [x] Creer `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Factories/StLegacyMachineFactory.cs` : reutiliser les traitements ST existants, les options communes et le controle des disquettes.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Hatari/Factories/HatariMachineFactory.cs` : utiliser la fabrique commune avec son identite existante.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/MachineAdapters/MachineConfigurationMappingScenarios.cs` : adapter les appels a la conversion partagee deplacee.
    - [x] Supprimer `src/GWGUI.Emulation.Atari/Emulators/Hatari/Functions` : retirer le dossier devenu vide.
  - [x] Integrer le profil Hatari 2014
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Enums/CoreEnums.cs` : ajouter Hatari2014 en fin d enumeration.
    - [x] Creer `src/GWGUI.Emulation.Atari/Emulators/Hatari2014/Constants/EmulatorConstants.cs` : identite, DLL, source officielle, revision et machines compatibles.
    - [x] Creer `src/GWGUI.Emulation.Atari/Emulators/Hatari2014/Factories/Hatari2014MachineFactory.cs` : utiliser la fabrique commune et les services existants de selection, installation et options.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Services/Machine.Commands.cs` : transmettre le type de reset au nouveau profil.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Common/Interop/Services/ExternalHostCallbacks.AudioVideoInput.cs` : reutiliser la detection existante des indicateurs Hatari pour le port 2014.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions/MediaFunctions.Activity.cs` : associer les memes indicateurs aux lecteurs du port 2014.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/README.md` : documenter les machines compatibles, le TOS utilisateur et les options partagees.
  - [x] Verifier et nettoyer
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Atari/AtariEmulatorAdapterTests.cs` : verifier selection, options partagees, perimetre et preservation des profils existants.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` : inclure le dossier Hatari2014.
    - [x] Creer `tests/GWGUI.Tests/Emulation/Atari/Hatari2014NativeValidationTests.cs` : verifier temporairement les options et l execution du coeur officiel, avec liberation dans finally.
    - [x] Supprimer `tests/GWGUI.Tests/Emulation/Atari/Hatari2014NativeValidationTests.cs` : retirer le test dependant de fichiers externes apres execution.
    - [x] Supprimer `artifacts/temp/atari-hatari2014-validation` : retirer les sources, DLL et fichiers de verification temporaires.
    - [x] Modifier `docs/tasks/emulation/atari-hatari2014.md` : enregistrer les validations, le build Debug --modules=A et les chemins verifies.

## Resultats

- Source officielle inspectee : `libretro/hatari`, branche `hitari2014-mercurial`, revision `ab55c3ed0e620c91e7f059a6d3fbef7acf9bfca8`.
- Profil natif : Hatari2014, DLL `hatari2014_libretro.dll`, base Hatari 1.8.
- DLL officielle Windows x64 de validation, SHA-256 : `b1ceea93d3c3fcf7849ddbe38946a70282123524f6bd7f735e1dec6d01e92a40`.
- 177 tests reussis avant nettoyage, dont la lecture native des options et huit demarrages ST, STf, STfm, Mega ST, STE, Mega STE, TT et Falcon avec EmuTOS 1.4 externe et disquette temporaire : frames, sauvegarde/restauration d etat et reset doux.
- 168 tests permanents reussis apres suppression du test natif et de tous ses fichiers temporaires.
- Build `scripts\local-building.cmd --building=debug --modules=A` termine avec code 0.
- Application et huit modules verifies : `F:\GW GUI\build\Debug\GW GUI\gwgui.exe`; amstrad, atari, commodore, microsoft, nec, nintendo, sega, sony.
- Hatari reste le profil par defaut; les valeurs numeriques des profils precedents sont conservees.
- Aucun parcours de jeu complet effectue.