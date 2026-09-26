# Intégration du module d’émulation Amstrad

## Périmètre actuel

`GWGUI.Emulation.Amstrad` est un module unique chargé par les interfaces génériques de
`GWGUI.Emulation`. La première version code uniquement les familles suivantes :

| Famille | Machines | Émulateur |
|---|---|---|
| CPC classiques | CPC 464, CPC 664, CPC 6128 | Caprice32 Libretro |
| CPC Plus | CPC 464 Plus, CPC 6128 Plus | Caprice32 Libretro |
| Console | GX4000 | Caprice32 Libretro |

Les trois familles restent séparées sous `Common/Machines` même lorsqu’elles partagent le processeur,
le format de configuration ou une partie des périphériques. Le matériel commun demeure dans
`Common/Machines/Common`; les identifiants et caractéristiques propres restent dans le dossier de
leur famille.

Caprice32 est raccordé sous `Emulators/Caprice32`. Les différences natives — DLL, téléchargement,
variables `cap32_*`, ABI Libretro, médias et erreurs — ne remontent pas dans les contrats communs.
Le module expose un seul émulateur et ne persiste donc aucun choix artificiel de moteur.

## Machines et traduction Caprice32

| Machine | Modèle Caprice32 | RAM | Clavier | Média principal |
|---|---:|---:|---|---|
| CPC 464 | `464` | 64 Kio | oui | cassette, disquette facultative |
| CPC 664 | `664` | 64 Kio | oui | disquette |
| CPC 6128 | `6128` | 128 Kio | oui | disquette |
| CPC 464 Plus | `6128+ (experimental)` | 64 Kio | oui | disquette, cassette, cartouche |
| CPC 6128 Plus | `6128+ (experimental)` | 128 Kio | oui | disquette, cassette, cartouche |
| GX4000 | `6128+ (experimental)` | 64 Kio | non | cartouche CPR |

Le couple `cap32_model`/`cap32_ram` provient du catalogue matériel. Il n’est ni demandé à
l’utilisateur ni stocké comme option libre. Un fichier CPR ne transforme pas un CPC Plus en GX4000 :
la machine choisie reste la source de vérité.

Caprice32 accepte les contenus DSK, M3U, CDT, TAP, VOC, CPR et SNA annoncés par son interface
Libretro. Les périphériques exposés dépendent de la machine : la GX4000 n’affiche ni clavier,
ni cassette, ni lecteur de disquette ; les CPC classiques et Plus ne reçoivent que leurs
périphériques matériels.

## Chaîne d’intégration

```text
GWGUI.App
  -> GWGUI.Emulation (interfaces publiques)
  -> GWGUI.Emulation.Amstrad (module et Common)
  -> Common/Interfaces/IEmulatorAdapter
  -> Emulators/Caprice32
  -> cap32_libretro.dll dans le processus hôte d’émulation
```

`GWGUI.App` ne connaît ni Caprice32 ni les modèles natifs du cœur. La façade du module crée une
configuration commune, décrit ses réglages et ses médias, puis l’adaptateur Caprice32 effectue la
traduction au lancement. Tous les processus, threads, pipes, mappings, callbacks et bibliothèques
natives possédés par le module sont arrêtés et libérés dans les parcours de fermeture et d’erreur.

## Configuration et médias

La configuration sérialisée contient l’identifiant de machine, les réglages utilisateur communs,
les entrées, l’audio et les médias. Elle ne contient pas de type Caprice32, de chemin de DLL ni de
variable `cap32_*`. Les configurations résident sous :

```text
Data/Emulation/Machines/amstrad/Configurations/<configuration-id>/machine.json
```

Les formats sont associés aux prises communes :

- disquette : DSK et listes M3U ;
- cassette : CDT, TAP et VOC ;
- cartouche : CPR ;
- SNA : contenu de démarrage Caprice32, sans être présenté comme un périphérique matériel.

## Émulateurs et machines gardés pour plus tard

Ils sont documentés mais ne figurent pas dans le catalogue exécutable de cette étape :

- PCem Libretro sera utilisé pour les PC Amstrad qu’il couvre et pourra servir à d’autres familles
  de PC compatibles ; MAME et 86Box ne sont pas ajoutés en parallèle pour ces configurations ;
- le Mega PC pourra utiliser PCem avec l’état **partiel**, partie PC uniquement et sans cartouches ;
- un véritable Mega PC combinant émulation PC et Mega Drive fera l’objet d’un moteur distinct ;
- JOYCE/ANNE est une base disponible pour les PCW et PcW16 ;
- nc100em est une base disponible pour les NC100, NC150 et NC200 ;
- Ana Rosa et CP/M Box ne sont pas intégrés ;
- le cœur MiSTer PCW est une référence FPGA, pas une DLL Windows ;
- les PCW, PcW16, NC, PDA600 et PC Amstrad restent absents de `Machines` jusqu’à leur chantier.

CrocoDS n’est pas retenu : il n’apporte pas de capacité nécessaire à cette première version par
rapport à Caprice32.

## Ressources

Les noms propres invariants — Amstrad, CPC, GX4000, Caprice32, CPU, RAM et formats — résident dans
`Resources/00-Base`. Chaque phrase traduisible existe dans les catalogues culturels du module et est
produite par le script Argos du dépôt.

## Critères d’acceptation

- les six machines sont visibles et rangées dans leur famille correcte ;
- la configuration commune ne contient aucune donnée native Caprice32 ;
- le module télécharge, détecte et lance `cap32_libretro.dll` ;
- chaque modèle impose les valeurs natives correspondant à son matériel ;
- les périphériques et formats incohérents avec la machine sont refusés ;
- vidéo, audio, clavier, manettes, reset, médias et états passent par les interfaces génériques ;
- fermeture, annulation et erreur ne laissent aucun processus, thread, handle, mapping ou DLL
  appartenant au module ;
- Amiga et Atari ne sont pas modifiés pour réaliser le module Amstrad.
