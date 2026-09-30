# Microsoft : note de conception des adaptateurs d’émulation

> Document de préparation. Il décrit une direction technique pour ajouter plus tard des DLL natives et, si possible, des cœurs Libretro. Il ne prétend pas fournir une implémentation d’émulateur aujourd’hui.

## 1. Objectif et périmètre

Le module Microsoft doit pouvoir afficher les machines Xbox et leurs informations même lorsqu’aucun émulateur n’est installé. La création d’une configuration doit toutefois rester impossible tant qu’aucun adaptateur compatible n’est listé et choisi.

Le travail futur doit s’intégrer au pont déjà présent dans `GWGUI.Emulation` : gestionnaire d’émulateurs, configuration opaque et cycle de sauvegarde existant. Il ne faut pas créer un second catalogue global, un second format de configuration ou un contrat parallèle.

Les familles couvertes par cette note sont :

- Xbox originale ;
- Xbox 360 ;
- Xbox One, avec une intégration de type lanceur lorsque le logiciel n’est pas un cœur d’émulation embarquable.

## 2. Candidats relevés

| Machine | Projet étudié | Forme d’intégration constatée dans la note | Orientation |
| --- | --- | --- | --- |
| Xbox | Cxbx-Reloaded | Projet C++ avec noyau, vidéo, audio et entrées ; pas de DLL publique stabilisée | Premier candidat pour un adaptateur natif expérimental |
| Xbox | xemu | Émulateur actif, architecture proche de QEMU ; API publique `LoadGame/RunFrame` non disponible | À étudier après Cxbx, probablement via processus ou fork contrôlé |
| Xbox | XQEMU | Base lourde et moins adaptée au premier prototype | Ne pas retenir pour le premier jalon |
| Xbox 360 | Xenia / Xenia Canary | Projet C++ avec sous-systèmes `Emulator`, CPU, GPU, APU et kernel ; pas de DLL publique stabilisée | Premier candidat Xbox 360 |
| Xbox One | WinDurango | Couche de compatibilité et DLL système, pas un émulateur à piloter image par image | Adaptateur de processus/lanceur |
| Xbox One | XWine1 | Projet expérimental et fragmentaire (`SlimEra`, `XboxAudio2`, `XDLCompiler`) | Veille uniquement, pas de dépendance initiale |

Les projets et leurs dépôts de référence sont listés à la fin du document. Les versions, la compatibilité et l’état des API devront être revérifiés au moment de l’implémentation.

## 3. Architecture cible dans GW GUI

### 3.1 Contrat commun

L’application C# doit parler à un contrat commun, indépendamment de la technologie du cœur :

```text
GW GUI
  └─ GWGUI.Emulation (contrat et cycle de configuration)
       └─ Microsoft adapter host
            ├─ CxbxCore.dll (futur, Xbox)
            ├─ XeniaCore.dll (futur, Xbox 360)
            ├─ LibretroCoreAdapter (futur, si un core compatible existe)
            └─ ProcessAdapter (Xbox One / émulateur autonome)
```

Le contrat de haut niveau à stabiliser avant toute DLL est :

```text
LoadCore / LoadGame
RunFrame (uniquement pour un cœur qui le garantit)
GetVideo / GetAudio
SetInput
SaveState / LoadState (si supporté)
Reset
Pause / Resume
Shutdown
```

`RunFrame`, les états et les buffers ne doivent pas être simulés par un adaptateur qui ne les expose pas réellement. Un adaptateur de processus peut proposer le lancement, l’arrêt, la détection de fenêtre et le retour à l’application, sans inventer une fausse API image par image.

### 3.2 Raccordement aux contrats existants

- Ajouter les définitions d’émulateurs au gestionnaire déjà utilisé par `GWGUI.Emulation`.
- Exposer une définition par installation/version/architecture réellement disponible.
- Conserver un seul `EmulatorId` dans la configuration sauvegardée.
- Proposer les choix avant la sauvegarde et verrouiller le sélecteur après sauvegarde, conformément au cycle existant.
- Laisser la fenêtre d’options s’ouvrir avec toutes les informations même si la liste est vide ; refuser seulement la création ou la sauvegarde sans émulateur sélectionné.
- Réutiliser la base de traductions commune pour tous les nouveaux libellés et messages.

## 4. Frontière DLL native

### 4.1 Règles ABI

La DLL ne doit pas exposer directement des classes C++ à P/Invoke. Prévoir une ABI C stable, avec :

- fonctions exportées en `extern "C"` ;
- handles opaques (`void*` ou identifiants) ;
- types de largeur explicite (`uint32_t`, `uint64_t`) ;
- chaînes avec convention documentée (UTF-8 recommandée) ;
- structure de version et capacités ;
- codes d’erreur stables et fonction de récupération du dernier message ;
- ownership explicite des buffers : soit buffer fourni par l’appelant, soit fonction de libération correspondante.

Un premier en-tête conceptuel peut ressembler à ceci :

```c
typedef struct gw_core gw_core;

typedef struct gw_core_version {
    uint32_t abi_major;
    uint32_t abi_minor;
    uint64_t capabilities;
} gw_core_version;

int gw_core_get_version(gw_core_version* out_version);
int gw_core_create(const char* config_utf8, gw_core** out_core);
int gw_core_load_game(gw_core* core, const char* media_utf8);
int gw_core_run_frame(gw_core* core);
int gw_core_set_input(gw_core* core, uint32_t port, const void* state, uint32_t size);
int gw_core_get_video(gw_core* core, const void** data, uint32_t* width,
                      uint32_t* height, uint32_t* stride, uint32_t* format);
int gw_core_get_audio(gw_core* core, const void** data, uint32_t* frames,
                      uint32_t* channels, uint32_t* sample_rate);
int gw_core_serialize(gw_core* core, void* buffer, uint64_t* size);
int gw_core_unserialize(gw_core* core, const void* buffer, uint64_t size);
int gw_core_reset(gw_core* core);
void gw_core_destroy(gw_core* core);
```

Cet en-tête est une proposition de contrat, pas une promesse que Cxbx-Reloaded ou Xenia l’implémentent déjà. Un wrapper doit traduire vers leurs API internes sans exposer leurs types privés à l’application.

### 4.2 Cycle de vie et nettoyage

Chaque propriétaire doit libérer ce qu’il crée, y compris après erreur :

1. charger la DLL et vérifier l’ABI ;
2. créer le handle ;
3. charger le média ;
4. démarrer les threads, surfaces ou fenêtres nécessaires ;
5. arrêter le cœur et détacher les callbacks ;
6. attendre la fin effective des threads/processus ;
7. détruire le handle puis décharger la DLL.

Le code C# et les wrappers natifs doivent mettre les étapes de fermeture dans des blocs `finally`. Un objet reçu d’un appelant reste sous la responsabilité de son propriétaire.

## 5. Compatibilité Libretro

### 5.1 Couche dédiée

Prévoir un `LibretroCoreAdapter` qui mappe le contrat commun vers les points d’entrée Libretro classiques :

```text
retro_init / retro_deinit
retro_get_system_info / retro_get_system_av_info
retro_load_game / retro_unload_game
retro_run
retro_video_refresh
retro_audio_sample_batch
retro_input_poll / retro_input_state
retro_serialize / retro_unserialize
retro_reset
```

Le chargement dynamique (`retro_get_api_version`, recherche des symboles, callbacks) doit rester isolé dans cet adaptateur. Les formats vidéo/audio, la taille des buffers et les callbacks doivent être copiés ou consommés selon une règle d’ownership documentée avant de retourner de l’appel natif.

### 5.2 Ne pas forcer les émulateurs autonomes

Un cœur Libretro et un émulateur autonome n’ont pas le même cycle de vie. Cxbx-Reloaded, xemu ou Xenia ne doivent pas être artificiellement déclarés Libretro tant qu’un port réellement maintenu n’existe pas. Le même contrat de haut niveau peut être partagé, mais :

- `LibretroCoreAdapter` fonctionne en mémoire et fournit `RunFrame` si le core le permet ;
- `NativeCoreAdapter` enveloppe une DLL C conçue pour GW GUI ;
- `ProcessAdapter` pilote un exécutable autonome et ne fournit que les capacités qu’il peut garantir.

Cette séparation permet d’ajouter ultérieurement un core Libretro sans modifier la configuration utilisateur ni le catalogue des machines.

## 6. Médias, installations et sécurité de configuration

- Les chemins de média, BIOS, clés et répertoires d’installation doivent rester dans la configuration de l’émulateur, pas dans le catalogue de machines.
- Les chemins fournis par l’utilisateur doivent être validés avant lancement et ne doivent jamais être interprétés comme une commande.
- Les tests doivent utiliser des médias et BIOS légalement détenus par l’utilisateur ou des logiciels libres de droits.
- Une machine sans adaptateur installé reste visible et configurable au niveau informatif, mais sa sauvegarde doit être refusée avec un message traduit indiquant qu’un émulateur doit être choisi.
- Une machine avec adaptateur sélectionné doit conserver exactement cet identifiant ; les changements d’installation doivent passer par le flux de modification prévu par `GWGUI.Emulation`.

## 7. Découpage de réalisation proposé

### Jalon A — contrat et catalogue

- Stabiliser les capacités (`RunFrame`, vidéo, audio, entrée, états, processus).
- Ajouter les identifiants Xbox, Xbox 360 et Xbox One au catalogue Microsoft sans prétendre qu’un adaptateur est disponible.
- Ajouter les tests de création refusée quand aucune définition d’émulateur n’est installée.

### Jalon B — premier adaptateur natif

- Créer un répertoire de wrapper sous `src/GWGUI.Emulation.Microsoft/Emulators/`.
- Choisir une seule cible de compilation (x64 en premier) et une seule ABI.
- Implémenter le chargement de DLL, la vérification de version, le mapping des erreurs et le nettoyage complet.
- Ajouter un test C# d’ABI qui crée, charge, arrête et détruit un handle sans fuite.

### Jalon C — Libretro

- Ajouter `LibretroCoreAdapter` dans le même contrat, sans dupliquer le catalogue.
- Tester un core de référence légal et documenter précisément les capacités réellement exposées.
- Ne publier un adaptateur Microsoft que lorsque le lancement, l’arrêt, l’entrée, la vidéo et l’audio ont été validés.

### Emplacements à prévoir

```text
src/GWGUI.Emulation.Microsoft/
  Emulators/
    Cxbx/
    Xenia/
    Libretro/
    Process/
tests/GWGUI.Tests/Emulation/Microsoft/
docs/project/microsoft-emulator-adapters.md
```

Les sources externes ne doivent pas être copiées aveuglément dans le dépôt GW GUI. Documenter le commit, la licence, la toolchain et la procédure de reproduction du wrapper choisi.

## 8. Validation avant intégration

Avant d’exposer un adaptateur dans l’interface :

1. compiler la DLL et vérifier les exports avec l’architecture attendue ;
2. exécuter le smoke test P/Invoke sur création/chargement/arrêt/destruction ;
3. vérifier la détection d’un média valide et le refus d’un chemin invalide ;
4. vérifier vidéo, audio et entrée sur un cas de test reproductible ;
5. vérifier `SaveState/LoadState` uniquement si la capacité est annoncée ;
6. vérifier qu’une fermeture normale, une erreur et une interruption libèrent fenêtre, surface, thread, processus et handle ;
7. vérifier que l’interface reste utilisable quand aucun adaptateur n’est installé ;
8. exécuter les tests ciblés puis la suite existante, sans ajouter de tests jetables permanents.

## 9. Décisions restant à prendre

- DLL native dans le processus ou hôte séparé pour Cxbx/Xenia ;
- première console réellement supportée par un wrapper ;
- version x64 et toolchain de compilation ;
- politique de distribution des binaires et des dépendances ;
- cores Libretro effectivement disponibles pour Xbox/Xbox 360 ;
- capacités minimales exigées avant qu’un adaptateur apparaisse dans la liste de sélection.

## 10. Références fournies dans la note source

- Cxbx-Reloaded : <https://github.com/cxbx-reloaded/cxbx-reloaded>
- xemu : <https://github.com/xemu-project>
- Xenia : <https://github.com/xenia-project/xenia>
- WinDurango : <https://github.com/WinDurango/WinDurango>
- XWine1 : <https://github.com/xwine1>

