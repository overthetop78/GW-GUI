# Intégration des machines et émulateurs manquants

- [ ] Compléter les systèmes de liste-emulateur-libretro.md
  - [ ] Établir la couverture et les actions par module
    - [x] Modifier `docs/tasks/emulation/remaining-machines-emulators.md` : ajouter l'inventaire des DLL absentes, les correspondances avec les modules existants et les modèles explicitement documentés ; distinguer les variantes d'un même émulateur.
    - [x] Modifier `docs/tasks/emulation/remaining-machines-emulators.md` : inscrire la première intégration vérifiée (bsnes C++98), ses fichiers et les contrôles ; les intégrations suivantes seront détaillées avant réalisation.
  - [ ] Compléter Nintendo
    - [x] Créer `src/GWGUI.Emulation.Nintendo/Emulators/BsnesCpp98/Constants/CoreConstants.cs` et `Factories/MachineFactory.cs` : définir bsnes C++98 v085, les extensions officielles, le téléchargement Windows x64, la machine SNES et les listes vides d’options/firmwares conformément aux métadonnées officielles.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs` et `Emulators/Common/Interop/Dictionaries/CoreCatalog.cs` : enregistrer l’adaptateur et sa définition.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Resources/*/Emulators.resx` : ajouter la description de bsnes C++98 dans toutes les langues avec Argos, conserver son nom dans 00-Base uniquement.
  - [x] Compléter Commodore C128D et C116
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/C128/Constants/ModelConstants.cs`, `C128/Dictionaries/ModelCatalog.cs`, `Plus4/Constants/ModelConstants.cs` et `Plus4/Dictionaries/ModelCatalog.cs` : ajouter C128D (lecteur intégré) et C116 (profil matériel C16) avec identifiants et libellés constants.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Dictionaries/ModelCatalog.cs`, `MachineCatalog.cs` et `Constants/MachineCatalogConstants.cs` : exposer les deux modèles et leurs clés de ressources.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Emulators/ViceX128/Constants/EmulatorConstants.cs` et `ViceXPlus4/Constants/EmulatorConstants.cs` : sélectionner C128 D PAL/c128d et C16 PAL/c16pal pour ces modèles.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Resources/00-Base/Model.resx` : ajouter les noms invariants des deux modèles.
  - [x] Compléter les associations GB/GBC de mGBA et VBA-M
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Mgba/Constants/CoreConstants.cs`, `VbaM/Constants/CoreConstants.cs` et leurs `FirmwareConstants.cs` : associer GB/GBC et réserver chaque BIOS à sa machine, y compris SGB pour les jeux GB compatibles.
    - [x] Créer `src/GWGUI.Emulation.Nintendo/Emulators/Mgba/Constants/OptionMachineConstants.cs` et `VbaM/Constants/OptionMachineConstants.cs`, modifier leurs `OptionConstants.cs` : limiter les réglages propres aux jeux GB ou GBA aux machines concernées et utiliser des listes explicites.
  - [x] Contrôler les premières intégrations
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Nintendo/NintendoEmulatorCatalogTests.cs` et `tests/GWGUI.Tests/Emulation/Commodore/CommodoreMachineCatalogTests.cs` : actualiser les catalogues attendus pour les nouveaux cœurs, associations et modèles, puis exécuter ces tests existants.
  - [x] Construire les premières intégrations
    - [x] Modifier `docs/tasks/emulation/remaining-machines-emulators.md` : inscrire le résultat du build Debug de l’application avec tous les modules après les premières intégrations.
  - [x] Appliquer les corrections demandées sur les premiers ajouts
    - [x] Modifier les catalogues, constantes et ressource `Model.resx` de `src/GWGUI.Emulation.Commodore` et `tests/GWGUI.Tests/Emulation/Commodore/CommodoreMachineCatalogTests.cs` : supprimer toutes les références de l'entrée C116 et actualiser le nombre de modèles attendu.
    - [x] Créer `tmp/c128d-source.jpg` : télécharger la photo Rama / Musée Bolo depuis Wikimedia Commons pour son détourage.
    - [x] Créer `src/GWGUI.Emulation.Commodore/Assets/Machines/C128D.png` : intégrer une photo détourée du C128D, canal alpha transparent, dimension maximale 512 pixels ; modifier `Common/Machines/Common/Constants/MachineCatalogConstants.cs` et `Dictionaries/MachineCatalog.cs` pour la relier au modèle. Inscrire la source dans ce fichier de suivi.
    - [x] Supprimer `tmp/c128d-source.jpg` après intégration du PNG final.
    - [x] Corriger et vérifier mGBA/VBA-M pour GB/GBC
      - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Mgba/Constants/OptionConstants.cs`, `VbaM/Constants/OptionConstants.cs` et `Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` : définir les modèles GB/GBC par défaut selon la machine sélectionnée, conserver les choix natifs explicites.
      - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Mgba/Constants/FirmwareConstants.cs` et `VbaM/Constants/FirmwareConstants.cs` : rendre les BIOS GB compatibles accessibles pour GB/GBC puisque le modèle matériel natif reste sélectionnable ; garder le BIOS GBA uniquement sur GBA.
      - [x] Modifier `tests/GWGUI.Tests/Emulation/Nintendo/NintendoEmulatorCatalogTests.cs` : vérifier les modèles par défaut, BIOS GB/GBC, noms personnels de BIOS, filtres d'options et extensions de cartouches ; exécuter les tests existants Nintendo/Commodore.
    - [x] Modifier `docs/tasks/emulation/remaining-machines-emulators.md` : inscrire les vérifications et le build Debug avec tous les modules après ces corrections.
  - [ ] Compléter les autres associations, machines et familles
    - [ ] Modifier `docs/tasks/emulation/remaining-machines-emulators.md` : détailler les prochaines intégrations vérifiées avant tout changement de leurs fichiers.
  - [ ] Valider l’ensemble
    - [ ] Modifier `docs/tasks/emulation/remaining-machines-emulators.md` : noter la couverture finale, les contrôles pertinents et le résultat du build Debug avec tous les modules.

Les jeux, moteurs, interpréteurs et utilitaires ne sont pas des machines. Les consoles virtuelles sont à inventorier comme systèmes. Les catégories dépourvues de modèles précis nécessitent la réponse à la question posée avant leur implémentation.

## Inventaire initial des DLL absentes

| Famille documentée | DLL | Machines documentées |
| --- | --- | --- |
| Acorn | `b2_libretro.dll` | BBC Micro Model A, Model B, Master 128 |
| Apple | `applewin_libretro.dll` | Apple II, Apple II+, Apple IIe |
| Apple | `minivmac_libretro.dll` | Macintosh 128K, 512K, Plus, Classic et autres Macintosh 68k compatibles selon configuration |
| Arduboy | `ardens_libretro.dll` | Arduboy, Arduboy FX |
| Arduboy | `arduous_libretro.dll` | Arduboy |
| Atari | `holani.dll` | Atari Lynx — DLL hors nommage Libretro standard |
| Atari | `holani_retro.dll` | Atari Lynx — variante ancienne |
| Bandai | `mednafen_wswan_libretro.dll` | WonderSwan, WonderSwan Color, SwanCrystal |
| Bandai | `playdiaemu_libretro.dll` | Playdia |
| Bandai | `tamalibretro_libretro.dll` | Tamagotchi P1 |
| Coleco | `gearcoleco_libretro.dll` | ColecoVision |
| Coleco | `jollycv_libretro.dll` | ColecoVision |
| Emerson / Interton / Elektor | `amiarcadia_libretro.dll` | Emerson Arcadia 2001 et compatibles, Interton VC 4000 et compatibles, Elektor TV Games Computer, systèmes arcade Zaccaria/Malzak compatibles |
| Epoch | `emuscv_libretro.dll` | Super Cassette Vision |
| Epoch | `pd777_libretro.dll` | Cassette Vision |
| Fairchild | `freechaf_libretro.dll` | Channel F, Channel F System II |
| Gakken / Bit Corporation | `sameduck_libretro.dll` | Mega Duck, Cougar Boy |
| Magnavox / Philips | `cdi2015_libretro.dll` | Philips CD-i |
| Magnavox / Philips | `m2000_libretro.dll` | Philips P2000T |
| Magnavox / Philips | `o2em_libretro.dll` | Odyssey², Philips Videopac G7000 |
| Magnavox / Philips | `same_cdi_libretro.dll` | Philips CD-i |
| Mattel | `freeintv_libretro.dll` | Intellivision |
| Nintendo | `bsnes_cplusplus98_libretro.dll` | SNES, Super Famicom |
| Sharp | `px68k_libretro.dll` | X68000, X68000 XVI, X68030 |
| Sharp | `x1_libretro.dll` | Sharp X1 et compatibles |
| Sinclair | `81_libretro.dll` | ZX80, ZX81 |
| Sinclair | `fuse_libretro.dll` | ZX Spectrum 16K, 48K, 128K, +2, +2A, +3 et compatibles |
| SNK | `geolith_libretro.dll` | Neo Geo AES, Neo Geo MVS |
| SNK | `mednafen_ngp_libretro.dll` | Neo Geo Pocket, Neo Geo Pocket Color |
| SNK | `neocd_libretro.dll` | Neo Geo CD, Neo Geo CDZ |
| SNK | `race_libretro.dll` | Neo Geo Pocket, Neo Geo Pocket Color |
| Sony | `mednafen_psx_libretro.dll` | PlayStation / PS1 |
| Sony | `mednafen_psx_hw_libretro.dll` | PlayStation / PS1 |
| Sony | `pcee2_libretro.dll` | PlayStation 2 |
| Sony | `pcsx_rearmed_libretro.dll` | PlayStation / PS1 |
| Sony | `play_libretro.dll` | PlayStation 2 |
| Sony | `pokketstation_libretro.dll` | PocketStation |
| Sony | `rpcs3_libretro.dll` | PlayStation 3 |
| Bandai / VTech / Autres consoles portables | `dingooemu_libretro.dll` | Dingoo A320 |
| Bandai / VTech / Autres consoles portables | `gam4980_libretro.dll` | Gamate |
| Bandai / VTech / Autres consoles portables | `nuance_libretro.dll` | VM Labs NUON |
| Bandai / VTech / Autres consoles portables | `opera_libretro.dll` | 3DO Interactive Multiplayer |
| Bandai / VTech / Autres consoles portables | `potator_libretro.dll` | Watara Supervision |
| Bandai / VTech / Autres consoles portables | `spmp8000emu_libretro.dll` | Consoles portables Sunplus SPMP8000 |
| Ordinateurs et machines diverses | `bbkemu_libretro.dll` | Dictionnaires électroniques BBK et plateformes de jeux associées |
| Ordinateurs et machines diverses | `bk_libretro.dll` | Elektronika BK-0010, BK-0011, BK-0011M |
| Ordinateurs et machines diverses | `dosbox_libretro.dll` | PC compatibles IBM, DOS |
| Ordinateurs et machines diverses | `dosbox_core_libretro.dll` | PC compatibles IBM, DOS |
| Ordinateurs et machines diverses | `dosbox_pure_libretro.dll` | PC compatibles IBM, DOS |
| Ordinateurs et machines diverses | `dosbox_svn_libretro.dll` | PC compatibles IBM, DOS |
| Ordinateurs et machines diverses | `ep128emu_core_libretro.dll` | Enterprise 64, Enterprise 128 et autres machines compatibles prises en charge |
| Ordinateurs et machines diverses | `fmsx_libretro.dll` | MSX, MSX2, MSX2+ |
| Ordinateurs et machines diverses | `galaksija_libretro.dll` | Galaksija |
| Ordinateurs et machines diverses | `jaxe_libretro.dll` | CHIP-8, SUPER-CHIP, XO-CHIP |
| Ordinateurs et machines diverses | `native32emu_libretro.dll` | Plateforme Sunplus Native32 |
| Ordinateurs et machines diverses | `numero_libretro.dll` | Calculatrice TI-83 |
| Ordinateurs et machines diverses | `pcem_libretro.dll` | IBM PC, XT, AT, compatibles x86 historiques |
| Ordinateurs et machines diverses | `qemu_libretro.dll` | Machines virtuelles, architectures et PC pris en charge par QEMU |
| Ordinateurs et machines diverses | `rust_dos_libretro.dll` | Environnement PC DOS |
| Ordinateurs et machines diverses | `squirreljme_libretro.dll` | Téléphones et appareils Java ME / J2ME |
| Ordinateurs et machines diverses | `theodore_libretro.dll` | Thomson MO5, MO6, TO7, TO7/70, TO8, TO8D, TO9, TO9+ |
| Ordinateurs et machines diverses | `virtualxt_libretro.dll` | IBM PC/XT et compatibles |
| Ordinateurs et machines diverses | `wqxemu_libretro.dll` | Wenquxing NC1020, PC1000, CC800, NC2000, NC3000 |
| Arcade | `dice_libretro.dll` | Bornes d'arcade anciennes à logique discrète, notamment Pong et jeux compatibles |
| Arcade | `fbalpha_libretro.dll` | Systèmes arcade multiples : Capcom CPS-1/2/3, Neo Geo MVS et autres |
| Arcade | `fbalpha2012_libretro.dll` | Systèmes arcade multiples : Capcom CPS, Neo Geo MVS et autres |
| Arcade | `fbalpha2012_cps1_libretro.dll` | Capcom CPS-1 |
| Arcade | `fbalpha2012_cps2_libretro.dll` | Capcom CPS-2 |
| Arcade | `fbalpha2012_cps3_libretro.dll` | Capcom CPS-3 |
| Arcade | `fbalpha2012_neogeo_libretro.dll` | SNK Neo Geo MVS / AES |
| Arcade | `fbneo_libretro.dll` | Systèmes arcade multiples : Capcom CPS-1/2/3, Neo Geo MVS et autres |
| Arcade | `hbmame_libretro.dll` | Systèmes arcade multiples, variantes et ROM hacks |
| Arcade | `mame_libretro.dll` | Systèmes arcade et ordinateurs/consoles pris en charge par MAME |
| Arcade | `mame2000_libretro.dll` | Systèmes arcade pris en charge par MAME 0.37b5 |
| Arcade | `mame2003_libretro.dll` | Systèmes arcade pris en charge par MAME 0.78 |
| Arcade | `mame2003_midway_libretro.dll` | Systèmes arcade Midway compatibles |
| Arcade | `mame2003_plus_libretro.dll` | Systèmes arcade pris en charge par MAME 2003-Plus |
| Arcade | `mame2010_libretro.dll` | Systèmes arcade pris en charge par MAME 0.139 |
| Arcade | `mame2015_libretro.dll` | Systèmes arcade pris en charge par MAME 0.160 |
| Arcade | `mame2016_libretro.dll` | Systèmes arcade pris en charge par MAME 0.174 |
| Vectrex / Uzebox / Machines spécialisées | `oberon_libretro.dll` | Ordinateur Oberon RISC |
| Vectrex / Uzebox / Machines spécialisées | `tia_libretro.dll` | Atari 2600 — composant TIA / émulation expérimentale |
| Vectrex / Uzebox / Machines spécialisées | `uzem_libretro.dll` | Uzebox |
| Vectrex / Uzebox / Machines spécialisées | `vecx_libretro.dll` | Vectrex |

Les variantes holani.dll et holani_retro.dll doivent être distinguées des émulateurs : Holani est déjà présent. Les regroupements génériques MAME/QEMU ne définissent pas de modèles. La présence d’un nom de DLL ne prouve pas que toutes les associations machine/émulateur soient déjà implémentées.

## Modules existants à compléter

Atari : variante TIA et contrôle des associations Atari800/Hatari. Commodore : modèles C128D, CBM-II 500 et contrôle des profils VICE. Nintendo : bsnes C++98, New Nintendo 3DS et contrôle des associations FDS/GB/GBC. NEC : associations SuperGrafx et LaserActive. Sony : nouveaux cœurs PS1/PS2, PocketStation, PS3. Microsoft contient actuellement Xbox uniquement ; MSX et IBM PC/DOS sont des systèmes distincts à raccorder dans leurs familles respectives, sans les classer arbitrairement sous Microsoft.

## Familles absentes

Acorn, Apple, Arduboy, Bandai, Coleco, Emerson, Interton, Elektor, Epoch, Fairchild, Gakken/Bit Corporation, Philips/Magnavox, Mattel, Sharp, Sinclair, SNK ; autres systèmes décrits dans les sections ordinateurs, arcade et machines spécialisées. Leur raccordement au SDK et à la création de modules doit suivre les modules existants, sans rendre les contrats Common spécifiques à Libretro.

Métadonnées officielles bsnes C++98 : https://raw.githubusercontent.com/libretro/libretro-core-info/master/bsnes_cplusplus98_libretro.info (v085 Performance, extensions sfc/smc/gb/gbc/st/bs, aucune option de cœur déclarée, aucun firmware déclaré). La liste officielle Windows x64 expose les paquets de toutes les DLL documentées ; les variantes historiques ne doivent pas devenir des doublons arbitraires de machines.

Sources VICE : libretro/libretro-core.c reconnaît C128 D PAL ; vice/src/c128/c128model.c définit son lecteur 1571 intégré. C116 utilise le profil C16, les options xplus4 ne définissent pas de modèle C116 distinct. La documentation mentionne CBM-II 500 mais xcbm5x0 propose seulement 510 PAL/NTSC : correspondance à résoudre, sans afficher un modèle faussement distinct.

Contrôle des premières intégrations : 83 tests existants Nintendo/Commodore réussis. Métadonnées disponibles pour 78 des 84 noms de DLL initialement absents ; les six sans fichier .info sont holani.dll, holani_retro.dll, playdiaemu_libretro.dll, rpcs3_libretro.dll, rust_dos_libretro.dll et fbalpha_libretro.dll. Cela ne signifie pas que les paquets sont indisponibles ; leur intégration nécessite d’autres sources officielles. Les sept consoles virtuelles de la section dédiée restent également à intégrer et ne sont pas comprises dans le compte initial de 84.

Build des premières intégrations : scripts/local-building.cmd --building=debug --modules=A réussi (code 0). Exécutable build/Debug/GW GUI/gwgui.exe et les huit modules présents. Ressources Nintendo/Commodore XML valides, sans clés dupliquées. L’intégration exhaustive reste en cours ; les groupes finaux ne sont pas cochés.

Image C128D : photo Rama / Musée Bolo, https://commons.wikimedia.org/wiki/File:Commodore_128D-IMG_1726.jpg ; licence CC BY-SA 2.0 France https://creativecommons.org/licenses/by-sa/2.0/fr/ . Fond supprimé avec imagegen et image redimensionnée en PNG RGBA 512 × 512. Adaptation sous la même licence.

Corrections demandées : C116 supprimé du module, des ressources et des tests, à ne pas réintroduire. C128D doté de sa miniature transparente. Modèles GB/GBC par défaut corrigés pour mGBA/VBA-M ; les BIOS GB/GBC sont accessibles dans cette famille pour les variantes matérielles natives sélectionnables ; BIOS GBA réservé à GBA. 100 tests Nintendo/Commodore réussis. Build Debug avec --modules=A terminé, code 0, exécutable et huit DLL présents. Ces contrôles valident les catalogues et configurations ; aucun essai en jeu n'a été effectué.
