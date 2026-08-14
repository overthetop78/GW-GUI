# Émulation Atari ST, STE, TT et Falcon — Hatari et Libretro

## Sommaire

- [Décision et choix du cœur](#décision-et-choix-du-cœur)
- [Sources officielles](#sources-officielles)
- [Couverture](#couverture)
- [Firmware et médias](#firmware-et-médias)
- [API et configuration](#api-et-configuration)
- [Entrées et sorties](#entrées-et-sorties)
- [Limites du port Libretro actuel](#limites-du-port-libretro-actuel)
- [Autres cœurs Atari](#autres-cœurs-atari)
- [Tâches d’intégration](#tâches-dintégration)
- [Critères de validation](#critères-de-validation)

## Décision et choix du cœur

Le cœur [`libretro/hatari`](https://github.com/libretro/hatari) émule la famille Atari ST/STE/TT/Falcon. `GWGUI.Emulation.Atari.dll` l’encapsulera comme `GWGUI.Emulation.Amiga.dll` encapsule PUAE. `GWGUI.App` n’appelle jamais les exports `retro_*`. Le code ABI commun peut être partagé au niveau source entre les deux moteurs, sans créer de projet ni de DLL `GWGUI.Emulation.Libretro`.

Cette architecture ne crée aucune fenêtre. Le moteur Atari publie commandes, trames, audio et état ; la présentation ultérieure reste entièrement du ressort de `GWGUI.App`.

Le port Libretro Hatari documenté est toutefois moins complet côté frontend que PUAE : pas de Disk Control Libretro, d’états Libretro ni de sauvegardes annoncées. Hatari possède des fonctions internes équivalentes dans son propre menu/configuration. Il faudra donc soit compléter le port Libretro, soit exposer proprement les fonctions Hatari existantes dans notre branche.

## Sources officielles

- [Dépôt du cœur Hatari Libretro](https://github.com/libretro/hatari)
- [Dossier d’adaptation Libretro](https://github.com/libretro/hatari/tree/master/libretro)
- [Documentation officielle du cœur](https://docs.libretro.com/library/hatari/)
- [Issues du port Libretro](https://github.com/libretro/hatari/issues)
- [Info du cœur Hatari](https://github.com/libretro/libretro-super/blob/master/dist/info/hatari_libretro.info)
- [Dépôt miroir Hatari](https://github.com/hatari/hatari)
- [Site et documentation Hatari](https://www.hatari-emu.org/docs.html)
- [Manuel Hatari](https://hatari.frama.io/doc/manual.html)
- [Compatibilité Hatari](https://hatari.tuxfamily.org/doc/compatibility.html)
- [API canonique `libretro.h`](https://github.com/libretro/libretro-common/blob/master/include/libretro.h)
- [Guide frontend/cœur Libretro](https://docs.libretro.com/development/cores/developing-cores/)

## Couverture

Hatari émule :

- Atari ST et Mega ST ;
- Atari STE et Mega STE ;
- Atari TT ;
- Atari Falcon ;
- 68000 et 68030, FPU/MMU selon configuration ;
- GLUE, MMU, Shifter/TT Shifter/Videl ;
- MFP, ACIA, IKBD, YM2149, audio DMA STE/Falcon et DSP Falcon ;
- WD1772, ACSI/SCSI/IDE et dossiers GEMDOS selon machine/configuration ;
- clavier, souris, joysticks et MIDI selon capacités Hatari.

Le port Libretro expose officiellement une option de résolution interne. Les autres réglages passent surtout par `hatari.cfg` et le menu Hatari intégré. Notre objectif est de rendre ces réglages programmatiques, sans imposer ce menu.

## Firmware et médias

### TOS

Le cœur attend par défaut `tos.img` dans le dossier système. Correspondances matérielles :

- ST : TOS 1.00, 1.02, 1.04 ou 2.06 selon la documentation du cœur ;
- STE : TOS 1.x ou 2.x compatible STE ;
- TT : TOS 3.0x ;
- Falcon : TOS 4.0x ;
- EmuTOS peut fournir une alternative libre pour plusieurs modèles.

Les TOS rejoignent le même dépôt central que les Kickstart, sous `Data/Emulation/Firmware/Atari/TOS`. L’index commun référence chaque fichier une seule fois et l’associe à toutes les machines compatibles. GW GUI doit calculer les empreintes, identifier version/région et choisir celle du modèle sans dupliquer la ROM.

### Formats chargés par le cœur

```text
.st  .msa  .zip  .stx  .dim  .ipf
```

IPF dépend de CAPSImg selon la construction Hatari. Les disques durs et dossiers GEMDOS relèvent de `hatari.cfg`/des options Hatari plutôt que de la petite liste de contenus Libretro.

## API et configuration

Le cœur utilise les mêmes exports standard décrits dans [le dossier Amiga](emulation-amiga.md#api-libretro-à-héberger). L’adaptateur privé partagé au niveau source doit donc être réutilisé sans exposer Libretro au reste de la solution pour :

- chargement de DLL et ABI ;
- environnement ;
- vidéo, audio et entrées ;
- boucle `retro_run` ;
- messages et logs ;
- répertoires système/save/assets ;
- options Libretro.

### Option Libretro documentée

`Hatari_resolution` : `640x480`, `832x576`, `832x588`, `800x600`, `960x720`, `1024x768`, `1024x1024`.

### Configuration Hatari à rendre programmable

La source de vérité est le [manuel Hatari](https://hatari.frama.io/doc/manual.html) et la structure de configuration dans le dépôt. Groupes à inventorier et exposer :

- machine ST/Mega ST/STE/Mega STE/TT/Falcon ;
- CPU, fréquence, compatibilité/cycle-exact, cache, MMU et FPU ;
- RAM ST et TT-RAM ;
- TOS, patch TOS et cartouche ;
- moniteur mono/RGB/VGA, VDI, bordures, overscan et frameskip ;
- YM2149, audio DMA, codec Falcon, DSP et fréquence audio ;
- lecteur A/B, protection, vitesse, insertion et éjection ;
- ACSI/SCSI/IDE, images disque et dossier GEMDOS ;
- clavier, souris, joysticks, IKBD et MIDI ;
- imprimante, série et périphériques ;
- sauvegardes mémoire internes, debugger et traces.

Deux stratégies possibles : générer `hatari.cfg` avant `retro_load_game`, ou étendre l’adaptation Libretro avec des core options. La seconde donnera le meilleur contrôle à long terme.

## Entrées et sorties

### Entrées officiellement documentées

- RetroPad : directions, tir, entrée dans le GUI, bascule souris, clavier virtuel, sélection joystick et vitesse souris ;
- clavier Libretro complet ;
- souris relative et boutons gauche/droit ;
- remappage géré par le frontend.

### Vidéo/audio

- framebuffer logiciel reçu par `retro_video_refresh_t` ;
- résolution déterminée par l’option interne et le mode Atari ;
- géométrie/FPS/fréquence audio lus après chargement par `retro_get_system_av_info` ;
- PCM stéréo via callback Libretro ;
- accepter les changements de mode ST basse/moyenne/haute résolution et les modes TT/Falcon.

## Limites du port Libretro actuel

Selon la documentation officielle du cœur :

- Core Options : oui, mais une seule option documentée ;
- Controls/Remapping : oui ;
- Restart, Saves, States, Rewind : non exposés au frontend ;
- Disk Control : non exposé ;
- LEDs et multi-souris : non exposés ;
- netplay, cheats, rumble, capteurs/caméra/localisation : absents et hors périmètre.

Ces « non » décrivent le **port Libretro**, pas nécessairement le moteur Hatari. Le menu Hatari sait notamment changer les disquettes et sauvegarder un état. Les tâches devront relier ces fonctions à l’API Libretro ou à une extension contrôlée par notre hôte.

## Autres cœurs Atari

### Atari 8-bit

Les Atari 400/800/XL/XE/XEGS déjà gérés en disquette ne sont pas des Atari ST. Cœurs Libretro pertinents :

- [Atari800 Libretro](https://github.com/libretro/libretro-atari800) — Atari 8-bit et 5200 ;
- [documentation Atari800](https://docs.libretro.com/library/atari800/) ;
- [info du cœur](https://github.com/libretro/libretro-super/blob/master/dist/info/atari800_libretro.info).

Il faudra un dossier séparé lors de son intégration : CPU 6502C, ANTIC, GTIA, POKEY et SIO n’ont rien de commun avec le ST au-delà de la marque.

### HatariB et variantes

Des forks/paquets appelés HatariB existent dans certains environnements Libretro, mais le cœur de référence documenté reste `libretro/hatari`. Ne pas baser l’architecture sur une variante sans dépôt, version et API identifiés.

## Tâches d’intégration

### Phase A — moteur Atari dans la solution

- [ ] Créer `GWGUI.Emulation.Atari` x64 et ses tests, sans dépendance WPF.
- [ ] Exposer uniquement les contrats machine, réglages, média, vidéo, audio, entrées et état de GW GUI.
- [ ] Obtenir/construire `hatari_libretro.dll` Windows x64.
- [ ] Charger le cœur derrière l’adaptateur Libretro privé du moteur.
- [ ] Fournir le dossier système, `tos.img` et `hatari.cfg` minimal.
- [ ] Charger une image ST puis MSA.
- [ ] Afficher vidéo, jouer audio et transmettre clavier/souris/joystick.
- [ ] Capturer toutes les commandes d’environnement demandées.
- [ ] Test : démarrer TOS/EmuTOS sans disque, puis une image ST connue, sans appel Libretro depuis `GWGUI.App`.

### Phase B — inventaire exact du port

- [ ] Lire entièrement `libretro/` et relever exports, options et périphériques.
- [ ] Comparer l’info du cœur à son comportement réel.
- [ ] Identifier comment le chemin du contenu est injecté dans Drive A.
- [ ] Cartographier `hatari.cfg` vers les structures internes Hatari.
- [ ] Relever les fonctions internes reset, floppy insert/eject, snapshot et LEDs.
- [ ] Vérifier CAPSImg/STX/IPF dans la construction Windows x64.

### Phase C — contrôle complet sans menu Hatari

- [ ] Ajouter des core options par modèle, CPU, mémoire, TOS, vidéo, audio et stockage.
- [ ] Exposer insertion/éjection/changement A/B via Disk Control étendu.
- [ ] Exposer reset froid/chaud.
- [ ] Relier snapshots Hatari à `retro_serialize*`, ou documenter un format externe stable.
- [ ] Exposer activité lecteurs par interface LED.
- [ ] Supprimer la nécessité d’ouvrir le GUI Hatari pour une opération normale.
- [ ] Persister plusieurs configurations de modèles sans écraser un unique `hatari.cfg` global.

### Phase D — couverture modèles

- [ ] ST/Mega ST avec TOS 1.0x/2.06 et 512 Kio–4 Mio.
- [ ] STE/Mega STE avec Blitter, audio DMA et 8/16 MHz.
- [ ] TT avec 68030, TT-RAM, TT Shifter, SCSI et TOS 3.x.
- [ ] Falcon avec Videl, DSP, IDE/SCSI, audio et TOS 4.x.
- [ ] ST, MSA, STX, DIM, IPF et ZIP.
- [ ] Lecteur A/B, disques multiples et écriture/protection.
- [ ] PAL, mono haute résolution et modes couleur.
- [ ] MIDI avec une boucle/port Windows facultatif.

### Phase E — autres Atari

- [ ] Créer le dossier Atari 8-bit/Atari800.
- [ ] Créer son moteur de famille et réutiliser les fichiers ABI/adaptateur Libretro au niveau source.
- [ ] Tester ATR issu de GW GUI, SIO, clavier, joystick et son POKEY.

## Critères de validation

- EmuTOS/TOS démarre sans image et affiche le bureau.
- ST et MSA démarrent sans passer par le menu Hatari.
- souris, clavier et joystick restent correctement synchronisés.
- YM2149 et audio DMA STE/Falcon ne sous-alimentent pas le tampon.
- changement de résolution sans corruption du framebuffer.
- modèle/TOS/RAM réellement appliqués et vérifiables dans la machine.
- insertion/éjection A/B programmatiques après extension du port.
- ST, STE, TT et Falcon validés séparément ; aucune configuration « Atari générique » trompeuse.
