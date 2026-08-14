# Émulation Amiga — tâches de code exécutables

## But

Intégrer `puae_libretro.dll` derrière deux assemblies .NET, démarrer un A500 avec le Kickstart local, afficher la vidéo, amorcer un ADF, puis ajouter audio, entrées, médias, sauvegardes, tous les modèles PUAE et plusieurs machines simultanées.

```text
GWGUI.App
  ├─> GWGUI.Emulation.dll
  └─> GWGUI.Emulation.Amiga.dll
          ├─> GWGUI.Emulation.dll
          └─> puae_libretro.dll
```

`GWGUI.App` ne contient aucun appel `retro_*`. `GWGUI.Emulation` ne référence ni WPF, ni PUAE, ni Libretro. Tout l’ABI Libretro reste `internal` dans `GWGUI.Emulation.Amiga`.

## Sources de code verrouillées

- [PUAE Libretro](https://github.com/libretro/libretro-uae)
- [Implémentation du cœur `libretro-core.c`](https://github.com/libretro/libretro-uae/blob/master/libretro/libretro-core.c)
- [Structures propres à PUAE](https://github.com/libretro/libretro-uae/blob/master/libretro/libretro-core.h)
- [API canonique `libretro.h`](https://github.com/libretro/libretro-common/blob/master/include/libretro.h)
- [README PUAE : ROM, modèles et médias](https://github.com/libretro/libretro-uae/blob/master/README.md)
- [Documentation PUAE](https://docs.libretro.com/library/puae/)
- [Buildbot Windows x64](https://buildbot.libretro.com/nightly/windows/x86_64/latest/)
- [NAudio 2.3.0](https://www.nuget.org/packages/NAudio/2.3.0)

Révision PUAE inspectée pour établir ces tâches : `96ebfcfc2c66233ad37f6dc99ee991211dc719ad`. La révision du binaire réellement testé sera inscrite dans le manifeste du cœur, pas supposée identique.

## Arborescence à produire

```text
src/
  GWGUI.Emulation/
    GWGUI.Emulation.csproj
    Machine/
      IEmulationEngine.cs
      IEmulatedMachine.cs
      EmulationMachineState.cs
      EmulationError.cs
    Video/
      EmulationPixelFormat.cs
      VideoFrame.cs
    Audio/
      AudioChunk.cs
      IAudioOutput.cs
    Input/
      EmulationInputSnapshot.cs
      EmulationKey.cs
      EmulationPointerState.cs
      EmulationControllerState.cs
    Media/
      EmulationMedia.cs
      EmulationMediaSlot.cs

  GWGUI.Emulation.Amiga/
    GWGUI.Emulation.Amiga.csproj
    AmigaEngine.cs
    AmigaMachine.cs
    AmigaMachineConfiguration.cs
    AmigaModel.cs
    AmigaModelCatalog.cs
    Cores/
      IAmigaCore.cs
      AmigaExternalCore.cs
      AmigaExternalHostCallbacks.cs
      AmigaExternalOptionCatalog.cs
      AmigaExternalDiskControl.cs
    Runtime/
      AmigaRunLoop.cs
      AmigaVideoSink.cs
      AmigaAudioSink.cs
      AmigaInputSource.cs
    Media/
      AmigaMediaController.cs
    Configuration/
      AmigaConfigurationStore.cs
    States/
      AmigaStateStore.cs

  GWGUI.App/
    Services/Emulation/WasapiAudioOutput.cs
    Services/Emulation/AmigaFirmwareCatalog.cs

tests/GWGUI.Tests/Emulation/Amiga/
  Cores/
    AmigaExternalCoreTests.cs
    AmigaExternalHostCallbacksTests.cs
  AmigaA500BootTests.cs
  AmigaAdfBootTests.cs
  AmigaVideoTests.cs
  AmigaAudioTests.cs
  AmigaInputTests.cs
  AmigaMediaTests.cs
  AmigaConfigurationAndStateTests.cs
  AmigaModelTests.cs
  AmigaMultiInstanceTests.cs
```

Tout ce qui est affiché sous `src/` ci-dessus correspond à des fichiers source `.cs` compilés dans les DLL ou dans l’application. Ces dossiers n’existent pas chez l’utilisateur après installation. Les seuls dossiers de données créés à l’exécution sont ceux explicitement placés sous `Data/Emulation/`.

Le frontend choisit les chemins de ROM et de médias. Il construit `AmigaMachineConfiguration` avec, notamment, `KickstartPath`, `ExtendedRomPath`, `RomKeyPath` et les médias initiaux. `GWGUI.Emulation.Amiga.dll` vérifie les chemins reçus et les transmet au cœur actif ; il ne choisit jamais une ROM à la place du frontend.

Les fichiers temporaires du cœur sont regroupés dans `artifacts/ppua/`. `artifacts/` est déjà ignoré par Git. À l’exécution, la DLL est copiée vers `Emulation/puae_libretro.dll` sous le dossier de sortie. Le dossier `artifacts/ppua/` sera supprimé lorsque le mode d’installation définitif du cœur sera fonctionnel.

## Ordre d’exécution obligatoire

### A — Créer les projets et contrats

#### AMI-001 — Créer les deux projets d’émulation

- [x] Créer `src/GWGUI.Emulation/GWGUI.Emulation.csproj` avec `TargetFramework=net10.0`, `Nullable=enable` et `ImplicitUsings=enable`.
- [x] Créer `src/GWGUI.Emulation.Amiga/GWGUI.Emulation.Amiga.csproj` avec les mêmes propriétés et une référence vers `GWGUI.Emulation`.
- [x] Ne mettre aucun `UseWPF`, aucun package et aucune référence vers un autre projet GW GUI.
- [x] Ajouter les deux projets à `GWGUI.sln` sous le dossier solution `src`.
- [x] Ajouter les références nécessaires dans `GWGUI.App` et `GWGUI.Tests`; ne jamais référencer `GWGUI.App` depuis l’un des moteurs.
- [x] Faire passer `dotnet build GWGUI.sln -c Debug -p:Platform=x64`.

#### AMI-002 — Écrire le cycle de vie commun

- [x] Créer `EmulationMachineState` avec exactement `Created`, `Starting`, `Running`, `Paused`, `Stopping`, `Stopped` et `Faulted`.
- [x] Créer `IEmulatedMachine` avec `Guid Id`, `EmulationMachineState State`, `StartAsync`, `PauseAsync`, `ResumeAsync`, `HardResetAsync`, `StopAsync` et `IAsyncDisposable`.
- [x] Donner un `CancellationToken` à chaque commande asynchrone.
- [x] Créer `IEmulationEngine<TConfiguration>` avec `CreateMachine(TConfiguration configuration)`.
- [x] Tester qu’une fausse machine refuse `Resume` avant `Start`, accepte deux `Stop` successifs et termine dans `Stopped`.

#### AMI-003 — Écrire les sorties communes

- [x] Créer `EmulationPixelFormat` avec `Rgb565` et `Xrgb8888` ; ne pas exposer les valeurs numériques Libretro.
- [x] Créer `VideoFrame` avec `ReadOnlyMemory<byte> Pixels`, `Width`, `Height`, `Pitch`, `PixelFormat`, `AspectRatio`, `Sequence` et `Timestamp`.
- [x] Créer `AudioChunk` avec `ReadOnlyMemory<short> InterleavedStereo`, `SampleRate`, `FrameCount`, `Sequence` et `Timestamp`.
- [x] Créer `IAudioOutput` avec `Start(int sampleRate)`, `Write(ReadOnlySpan<short>)`, `Flush()` et `Stop()`.
- [x] Tester que `FrameCount * 2` est égal au nombre d’échantillons stéréo fourni.

#### AMI-004 — Écrire les entrées et médias communs

- [x] Créer `EmulationInputSnapshot` comme valeur immuable contenant clavier, souris relative et quatre contrôleurs.
- [x] Représenter chaque touche par `EmulationKey`, pas par `System.Windows.Input.Key`.
- [x] Représenter souris par deltas X/Y, molette et boutons gauche/droit/milieu.
- [x] Représenter un contrôleur par boutons, deux sticks et deux gâchettes normalisés en `short`.
- [x] Créer `EmulationMediaSlot` avec `Floppy0` à `Floppy3`, `HardDisk0` et `Cd0`.
- [x] Créer `EmulationMedia` avec chemin absolu, slot, type, lecture seule et état inséré.

#### AMI-004A — Séparer la machine Amiga de son cœur concret

- [x] Créer `Cores/IAmigaCore.cs`; cette interface reçoit une `AmigaMachineConfiguration` et expose initialisation, frame, reset, arrêt, vidéo, audio, entrées, médias et états.
- [x] Faire dépendre `AmigaMachine` uniquement de `IAmigaCore`, jamais directement de `AmigaExternalCore` ni d’un export natif.
- [x] Créer `Cores/AmigaExternalCore.cs` comme première implémentation de `IAmigaCore`.
- [x] Instancier `AmigaExternalCore` dans `AmigaEngine` via une factory sélectionnée par le type de cœur enregistré dans la configuration.
- [x] Enregistrer `AmigaCoreKind.External` dans la configuration ; ne pas déduire le type du nom de la DLL.
- [x] Tester `AmigaMachine` avec un faux `IAmigaCore`, puis tester séparément `AmigaExternalCore` avec la DLL native.

### B — Installer et charger PUAE

#### AMI-005 — Épingler le binaire de développement

- [x] Télécharger l’archive officielle contenant `puae_libretro.dll` sous `artifacts/ppua/puae_libretro.dll.zip`.
- [x] Extraire uniquement `puae_libretro.dll` sous `artifacts/ppua/puae_libretro.dll`.
- [x] Créer `artifacts/ppua/core.json` avec type de cœur `External`, URL exacte, date du build, taille ZIP, taille DLL, SHA-256 et architecture `x64`.
- [x] Ajouter dans `GWGUI.Emulation.Amiga.csproj` une cible `CopyAmigaExternalCore` exécutée après `Build` qui copie la DLL vers `$(OutDir)Emulation/` si elle existe.
- [x] Ne jamais chercher un cœur dans le `PATH` Windows.
- [x] Tester que la sortie contient exactement une copie et que le hash correspond au manifeste.

#### AMI-006 — Ajouter les appels natifs au cœur externe

- [x] Déclarer dans `Cores/AmigaExternalApi.cs` les delegates Cdecl internes pour tous les exports `retro_*`, le callback environnement, vidéo, audio unitaire, audio batch, input poll et input state.
- [x] Utiliser `nuint` pour `size_t`, `IntPtr` pour les pointeurs et `[return: MarshalAs(UnmanagedType.I1)]` pour chaque `bool` C.
- [x] Déclarer dans `Cores/AmigaExternalApi.cs` avec `LayoutKind.Sequential` : `retro_system_info`, `retro_game_geometry`, `retro_system_timing`, `retro_system_av_info`, `retro_game_info` et `retro_variable`.
- [x] Déclarer ensuite les structures clavier, contrôleurs, Disk Control et Core Options uniquement dans les tickets qui les utilisent.
- [x] Ajouter dans `AmigaExternalCoreTests` les assertions qui comparent `Marshal.SizeOf` et `Marshal.OffsetOf` aux tailles/offsets attendus en x64.

#### AMI-007 — Charger tous les exports

- [x] Écrire la méthode privée `LoadNativeCore(string absolutePath)` dans `AmigaExternalCore` avec `NativeLibrary.Load`.
- [x] Refuser un chemin relatif et produire `AmigaCoreNotFound` si le fichier manque.
- [x] Résoudre chaque export avec `NativeLibrary.GetExport` puis `Marshal.GetDelegateForFunctionPointer<T>`.
- [x] Résoudre exactement : `retro_api_version`, tous les `retro_set_*`, `retro_init`, `retro_deinit`, `retro_get_system_info`, `retro_get_system_av_info`, `retro_set_controller_port_device`, `retro_reset`, `retro_run`, `retro_load_game`, `retro_unload_game`, `retro_get_region`, `retro_get_memory_data`, `retro_get_memory_size`, `retro_serialize_size`, `retro_serialize` et `retro_unserialize`.
- [x] Appeler `retro_api_version` et refuser toute valeur différente de `RETRO_API_VERSION` défini par l’en-tête épinglé.
- [x] Libérer le module une seule fois dans `Dispose`; rendre un second `Dispose` inoffensif.
- [x] Tester chemin relatif, DLL absente, DLL sans exports requis, chargement valide et double libération.

### C — Fournir les services hôtes demandés par PUAE

#### AMI-008 — Gérer les chaînes natives pendant toute la session

- [x] Créer dans `AmigaExternalHostCallbacks` un registre d’allocations UTF-8 via `Marshal.StringToCoTaskMemUTF8`.
- [x] Ne jamais renvoyer à PUAE un pointeur vers une chaîne gérée temporaire.
- [x] Réutiliser le même pointeur pour un chemin ou une valeur inchangée.
- [x] Libérer toutes les allocations après `retro_deinit`, jamais avant.
- [x] Tester qu’un GC forcé entre `retro_set_environment` et `retro_run` ne change aucun pointeur retourné.

#### AMI-009 — Répondre aux chemins au moment correct

- [x] Créer les dossiers de session `System`, `Saves` et `Content` avant l’appel à `retro_set_environment`.
- [x] Pour `GET_SYSTEM_DIRECTORY`, écrire dans `char** data` le pointeur UTF-8 vers `System`.
- [x] Pour `GET_SAVE_DIRECTORY`, retourner `Save`.
- [x] Traiter `GET_CORE_ASSETS_DIRECTORY` comme l’alias numérique de `GET_CONTENT_DIRECTORY` défini par libretro et retourner le même dossier `Content`.
- [x] Pour `GET_CONTENT_DIRECTORY`, retourner le dossier du média ou `Content` en l’absence de média.
- [x] Toujours fournir des chemins absolus non vides pour les trois dossiers de session ; aucune valeur facultative vide n’est envoyée par cet hôte.
- [x] Tester directement les trois commandes numériques distinctes avec un emplacement `IntPtr` alloué, puis vérifier la stabilité de chaque pointeur.

#### AMI-010 — Implémenter le sous-ensemble d’environnement du premier boot

- [x] Dans `AmigaExternalCore.cs`, déclarer avec les signatures les numéros de commandes imposés par l’API native, sans valeur inventée.
- [x] Retourner `true` à `SET_SUPPORT_NO_GAME` et mémoriser la valeur envoyée par PUAE.
- [x] Retourner `true` à `GET_CAN_DUPE` et écrire `true` dans `bool* data`.
- [x] Accepter `SET_PIXEL_FORMAT` uniquement pour `RGB565` et `XRGB8888`; mémoriser le format actif.
- [x] Copier `SET_GEOMETRY` et `SET_SYSTEM_AV_INFO` dans des valeurs gérées sans conserver le pointeur natif.
- [x] Accepter `SET_MESSAGE`, `SET_MESSAGE_EXT`, `SET_INPUT_DESCRIPTORS`, `SET_CONTROLLER_INFO`, `SET_MEMORY_MAPS` et `SET_SUPPORT_ACHIEVEMENTS`.
- [x] Retourner `false` à `GET_LOG_INTERFACE`, `GET_PERF_INTERFACE`, `GET_VFS_INTERFACE` et `GET_LED_INTERFACE` tant que leur ticket n’est pas implémenté.
- [x] Journaliser le numéro de toute commande inconnue une seule fois par session.
- [x] Tester chaque branche avec un buffer natif contenant une structure connue.

#### AMI-011 — Héberger les Core Options sans en perdre

- [x] Répondre à `GET_CORE_OPTIONS_VERSION` avec la version 2.
- [x] Déclarer les structures V2 : catégories, définition, valeur, conteneur US et conteneur international.
- [x] À `SET_CORE_OPTIONS_V2`, parcourir les tableaux terminés par une clé nulle et copier chaque catégorie, clé, libellé, aide, valeur, label et défaut dans `AmigaExternalOptionCatalog`.
- [x] À `SET_CORE_OPTIONS_V2_INTL`, importer d’abord le bloc US puis appliquer les libellés locaux lorsqu’ils existent.
- [x] Implémenter également les fallbacks V1 et `SET_VARIABLES` pour qu’un autre build PUAE reste chargeable.
- [x] À `GET_VARIABLE`, retrouver la clé, choisir la valeur configurée ou le défaut et écrire un pointeur UTF-8 stable dans `retro_variable.value`.
- [x] À `GET_VARIABLE_UPDATE`, écrire le drapeau `OptionsChanged`, puis le remettre à `false` après lecture.
- [x] À `SET_VARIABLE`, valider la valeur contre celles annoncées avant de remplacer la valeur courante.
- [x] À `SET_CORE_OPTIONS_DISPLAY`, mémoriser la visibilité courante de la clé.
- [x] Enregistrer le callback `SET_CORE_OPTIONS_UPDATE_DISPLAY_CALLBACK` et l’invoquer après un changement susceptible de modifier la visibilité.
- [x] Tester qu’aucune clé annoncée par la DLL n’est absente du registre et que `puae_model=A500` ressort de `GET_VARIABLE`.

#### AMI-012 — Enregistrer les interfaces fournies par PUAE

- [x] À `GET_DISK_CONTROL_INTERFACE_VERSION`, écrire `1` et retourner `true`.
- [x] À `SET_DISK_CONTROL_EXT_INTERFACE`, copier tous les delegates de la structure dans `AmigaExternalDiskControl` et conserver leur durée de vie.
- [x] Accepter le fallback `SET_DISK_CONTROL_INTERFACE` si le cœur ne fournit pas l’interface étendue.
- [x] À `SET_KEYBOARD_CALLBACK`, copier le delegate clavier fourni par PUAE.
- [x] À `GET_INPUT_BITMASKS` avec `data == IntPtr.Zero`, retourner `true` comme attendu par PUAE.
- [x] Retourner `false` à `SET_FASTFORWARDING_OVERRIDE` tant que GW GUI ne pilote pas cette fonction.
- [x] Tester qu’après `retro_init`, Disk Control et le callback clavier sont effectivement présents.

### D — Démarrer l’A500 et produire la vidéo

#### AMI-013 — Préparer la ROM A500 pour le test

- [x] Utiliser `image_test/Roms/Bios/Kickstart 1.3.rom`, déjà vérifié à 524 288 octets, MD5 `192D6D950D0ED3DF8040B788502831C2` et SHA-256 `1D68BA18412501D2A4B307A0A632B94A50B839C2C7C5FF2DF6DE2C38B99A921F`.
- [x] Vérifier à chaque test sa taille et son SHA-256 avant de la donner au cœur, sans inscrire la ROM dans Git.
- [x] Transmettre le chemin absolu original par l’option `puae_kickstart`; le cœur validé l’accepte sans copie ni renommage.
- [x] Conserver et transmettre les 524 288 octets strictement inchangés ; cette ROM est déjà validée comme Kickstart 1.3 personnalisé fonctionnel sur A500 dans FS-UAE.
- [x] Ne jamais tronquer, concaténer, patcher ou remplacer automatiquement cette ROM.
- [x] Fixer avant `retro_init`/`retro_load_game` : `puae_model=A500`, `puae_video_standard=PAL`, `puae_floppy_multidrive=disabled` et `puae_floppy_write_protection=disabled` si ces clés sont annoncées.
- [x] Tester directement avant le boot que la source fait 524 288 octets et porte le SHA-256 attendu puisqu’aucune copie temporaire n’est créée.

#### AMI-014 — Respecter l’ordre d’appel réel de PUAE

- [x] Dans `AmigaMachine.StartAsync`, charger le module puis appeler `retro_set_environment` avant `retro_init`, car PUAE demande ses chemins et options dans `retro_set_environment`.
- [x] Appeler ensuite `retro_set_video_refresh`, les deux callbacks audio, `retro_set_input_poll` et `retro_set_input_state`.
- [x] Appeler `retro_init`, puis `retro_get_system_info` et vérifier `library_name == "PUAE"`, `need_fullpath == true` et que `adf` est dans `valid_extensions`.
- [x] Pour un boot sans disque, appeler `retro_load_game(IntPtr.Zero)` uniquement si PUAE a envoyé `SET_SUPPORT_NO_GAME=true`.
- [x] Appeler `retro_get_system_av_info` seulement après le chargement réussi.
- [x] En cas d’échec, appeler seulement les opérations de nettoyage correspondant aux étapes déjà réussies.
- [x] Tester l’ordre exact avec un faux module enregistrant chaque appel.

#### AMI-015 — Écrire le thread `AmigaRunLoop`

- [x] Créer un thread `LongRunning` dédié nommé `GWGUI Amiga <id>` ; tous les appels au cœur passent par ce thread.
- [x] Utiliser une `ConcurrentQueue<PendingCommand>` mono-consommateur réveillée par `Monitor` pour reset, média, option et état ; pause, reprise et arrêt pilotent directement la boucle sous verrou.
- [x] Tant que l’état est `Running`, traiter les commandes en attente puis appeler `retro_run` une fois.
- [x] En pause, traiter les commandes sans appeler `retro_run` et attendre le signal de reprise/arrêt.
- [x] Cadencer initialement avec `Stopwatch.GetTimestamp` selon `system_av_info.timing.fps`; remplacer ce cadenceur par l’asservissement borné défini dans AMI-022 dès que AMI-020 et AMI-021 passent.
- [x] À l’arrêt : sortir de la boucle, appeler `retro_unload_game`, `retro_deinit`, libérer les chaînes puis la DLL.
- [x] Transformer toute exception en état `Faulted`, conserver l’erreur et exécuter le même nettoyage.
- [x] Tester 300 frames, pause de 100 ms sans frame supplémentaire, reprise, arrêt et double arrêt.

#### AMI-016 — Copier correctement le framebuffer

- [x] Dans `AmigaVideoSink.OnVideo`, traiter `data == IntPtr.Zero` comme duplication de la dernière frame.
- [x] Calculer `byteCount = pitch * height`; ne jamais calculer avec `width * bytesPerPixel` lorsque le pitch diffère.
- [x] Louer deux buffers via `ArrayPool<byte>` et alterner écriture/publication pour ne jamais exposer un buffer en cours de copie.
- [x] Copier exactement `pitch` octets par ligne depuis le pointeur natif.
- [x] Publier largeur, hauteur, pitch, format actif, ratio et numéro de séquence.
- [x] Remplacer les buffers lorsque `pitch * height` dépasse leur capacité ; restituer les anciens à l’arrêt.
- [x] Tester une source synthétique avec padding de ligne, RGB565, XRGB8888, pointeur nul et changement de résolution.

#### AMI-017 — Prouver le boot Kickstart

- [x] Créer `AmigaA500BootTests` marqué comme test local nécessitant le cœur et la ROM.
- [x] Démarrer sans média avec la configuration du ticket AMI-013.
- [x] Attendre au maximum 15 secondes une frame non vide ; échouer en joignant état, messages PUAE et géométrie.
- [x] Calculer plusieurs hashes de frames espacées pour prouver que le cœur tourne et non qu’une image fixe factice est publiée.
- [x] Enregistrer la dernière frame en PNG sous `TestResults/Amiga/` uniquement en cas d’échec.
- [x] Arrêter la machine et vérifier que la DLL et le Kickstart temporaire ne sont plus verrouillés.

### E — Charger un ADF

#### AMI-018 — Construire `retro_game_info` sans copier le disque

- [x] Normaliser le chemin ADF avec `Path.GetFullPath` et vérifier existence/lecture avant l’appel natif.
- [x] Allouer le chemin UTF-8 jusqu’au retour de `retro_unload_game`.
- [x] Construire `retro_game_info` avec `path=<pointeur>`, `data=IntPtr.Zero`, `size=0` et `meta=IntPtr.Zero`, puisque PUAE annonce `need_fullpath=true`.
- [x] Passer un pointeur vers cette structure à `retro_load_game`; ne jamais charger tout l’ADF dans `data`.
- [x] Conserver le média comme `Floppy0` dans l’état géré seulement après retour `true`.
- [x] Tester chemin absent, extension refusée et structure valide.

#### AMI-019 — Amorcer les deux ADF de référence

- [x] Premier test : `image_test/validated_images/Commodore/Amiga/3.5 pouces DD - AmigaDOS OFS/Boot-DD-OFS.adf`, SHA-256 `0634BF6DACBAEF1C4959428D5416017DB85F97633A651EF33EBD32CC1A874D06`.
- [x] Second test : `F:/Disquettes/Amiga Workbench/Amiga_Workbench_1.3.3.adf`, SHA-256 `D0EE9914893EF4678572F5E0B1D2C2141133B1E48D6F9D70204B5A24B6A69647`.
- [x] Vérifier avant lancement la signature `DOS\0`, la présence de code après l’octet 11 et une somme de bootblock égale à `0xFFFFFFFF` avec retenue circulaire.
- [x] Attendre une séquence de frames différente de l’écran d’insertion sans disque.
- [x] Arrêter puis rouvrir chaque ADF en accès exclusif pour prouver l’absence de handle restant.

### F — Ajouter l’audio WASAPI contrôlé

#### AMI-020 — Mettre le PCM dans une file bornée

- [x] Dans `AmigaAudioSink`, convertir le compteur retourné par `retro_audio_sample_batch_t` en `frames * 2` échantillons `short`.
- [x] Copier chaque span natif dans une file PCM bornée à 200 ms selon le taux d’échantillonnage annoncé par le cœur.
- [x] En cas de dépassement, supprimer les blocs les plus anciens et incrémenter `AudioOverrunCount`.
- [x] Laisser `BufferedWaveProvider.ReadFully` fournir le silence en cas de sous-alimentation ; le moteur ne fabrique pas de faux `AudioChunk`.
- [x] Faire pointer le callback audio unitaire vers le même chemin en empilant une frame stéréo.
- [x] Tester l’ordre gauche/droite, le callback unitaire, la limite de 200 ms, la suppression des blocs anciens et le compteur d’overflow.

#### AMI-021 — Sortir le son avec NAudio 2.3.0

- [x] Ajouter `NAudio` version `2.3.0` uniquement à `GWGUI.App`.
- [x] Créer `WasapiAudioOutput : IAudioOutput` avec `WasapiOut` et un provider PCM 16 bits stéréo.
- [x] Démarrer en mode partagé, périphérique de rendu par défaut, latence demandée de 50 ms.
- [x] Faire lire au provider les données du tampon du moteur sans appel bloquant vers le thread PUAE.
- [x] Sur changement de périphérique ou erreur WASAPI, arrêter l’ancien client, recréer la sortie et conserver la machine en marche.
- [x] Sur pause/arrêt/reset, vider le provider pour ne pas rejouer du son ancien.
- [x] Tester avec un faux `IAudioOutput`; réserver le test matériel WASAPI à un test local explicite.

#### AMI-022 — Asservir la boucle au son

- [x] Lire `fps` et `sample_rate` depuis `retro_get_system_av_info`; accepter leurs mises à jour via `SET_SYSTEM_AV_INFO`.
- [ ] Définir une cible audio de 60 ms et une plage valide de 30 à 100 ms.
- [ ] Appeler `retro_run` sans attente lorsque le tampon descend sous 30 ms.
- [ ] Retarder la frame suivante avec une attente annulable lorsque le tampon dépasse 100 ms.
- [x] Ne jamais appeler `Thread.Sleep` depuis un callback natif.
- [ ] Tester dix minutes PAL puis NTSC et affirmer que la dérive reste bornée et que la mémoire du tampon ne croît pas.

### G — Ajouter clavier, souris et manettes

#### AMI-023 — Figer les entrées par frame

- [x] Stocker le dernier `EmulationInputSnapshot` complet par échange atomique.
- [x] Dans `input_poll`, copier cette valeur vers `_polledSnapshot`.
- [x] Dans tous les appels `input_state`, répondre uniquement depuis `_polledSnapshot` jusqu’au prochain `input_poll`.
- [x] Retourner zéro pour port, device, index ou id inconnu.
- [x] Tester qu’un changement concurrent en milieu de frame n’altère pas les réponses de cette frame.

#### AMI-024 — Mapper tout le clavier

- [x] Créer une table exhaustive `EmulationKey -> RETROK_*` couvrant lettres, chiffres, ponctuation, fonctions, navigation, pavé numérique et modificateurs.
- [x] Appeler le callback clavier PUAE lors de chaque transition avec `down`, code `RETROK`, caractère Unicode si disponible et modificateurs Libretro.
- [x] Maintenir également l’état interrogé par `input_state` si le cœur le demande.
- [x] À la perte de focus, générer les relâchements de toutes les touches encore pressées.
- [x] Tester enfoncement, relâchement, Shift+A, Alt, touches françaises physiques et absence de touche bloquée.

#### AMI-025 — Mapper la souris

- [x] Pour `RETRO_DEVICE_MOUSE`, retourner puis consommer les deltas X/Y accumulés depuis le dernier poll.
- [x] Retourner 0/1 pour boutons gauche, droit et milieu ; retourner la molette sur les IDs Libretro correspondants.
- [x] Ne pas appliquer d’accélération dans le moteur ; transmettre les deltas bruts du frontend.
- [x] Tester signe, saturation `short`, consommation unique du delta et maintien des boutons.

#### AMI-026 — Mapper joystick et CD32

- [x] Importer les types de contrôleurs envoyés par `SET_CONTROLLER_INFO`.
- [x] Exposer `None`, `RetroPad`, joysticks PUAE et `CD32Pad` disponibles pour chaque port.
- [x] Appeler `retro_set_controller_port_device` uniquement sur le thread PUAE entre deux frames.
- [x] Mapper D-pad, boutons, sticks analogiques et boutons CD32 vers les IDs exacts annoncés.
- [x] Inverser les ports frontend 0/1 uniquement comme PUAE le fait en interne ; tester le résultat observé pour éviter une double inversion.
- [x] Tester joystick A500 et les sept boutons CD32 avec snapshots synthétiques.

### H — Médias, écritures et sauvegardes

#### AMI-027 — Encapsuler Disk Control

- [x] Écrire dans `AmigaMediaController` une méthode par delegate fourni : état d’éjection, index courant, nombre d’images, changement d’index, remplacement, ajout, chemin et libellé.
- [x] Exécuter chaque delegate sur le thread PUAE par la channel de commandes.
- [x] Mettre à jour l’état géré seulement après retour `true` du cœur.
- [x] Pour changer un disque : `set_eject_state(true)`, `replace_image_index`, `set_image_index`, puis `set_eject_state(false)`.
- [x] Copier le `retro_game_info` et son chemin UTF-8 jusqu’à ce que le remplacement soit terminé.
- [x] Tester refus à chaque sous-étape sans désynchroniser l’état affiché.

#### AMI-028 — Gérer DF0 à DF3 et M3U

- [x] Régler `puae_floppy_multidrive` avant chargement selon le nombre de lecteurs configurés.
- [x] Associer `Floppy0..Floppy3` aux index et lecteurs réellement annoncés par PUAE.
- [x] Générer un M3U de session UTF-8 avec un chemin par ligne pour un jeu multidisque.
- [x] Préserver les libellés `DISK_FILE|DISK_LABEL` et `#SAVEDISK:<label>`.
- [x] Tester échange Disk 1/Disk 2 et présence simultanée de DF0/DF1.

#### AMI-029 — Contrôler l’écriture virtuelle

> Blocage constaté le 14 août 2026 : la commande AmigaDOS s’exécute sur une copie ADF, mais le build PUAE validé ne persiste pas les octets après fermeture. Les tests d’écriture restent donc volontairement non cochés.

- [x] Laisser `puae_floppy_write_protection=disabled` pour une image déclarée modifiable.
- [x] Utiliser `enabled` pour une image en lecture seule.
- [x] N’activer `puae_floppy_write_redirect` que lorsque la configuration demande explicitement une copie de sauvegarde ; le mode normal écrit l’image montée.
- [x] Attendre `retro_unload_game` et la fermeture Disk Control avant de signaler l’arrêt terminé.
- [ ] Tester une écriture dans une copie de `Boot-DD-OFS.adf`, relancer et vérifier les octets modifiés.
- [ ] Tester que l’original en lecture seule conserve son SHA-256.

#### AMI-030 — Sauvegarder/restaurer un état

- [x] Appeler `retro_serialize_size` après `retro_load_game` et louer exactement cette capacité.
- [x] Appeler `retro_serialize(buffer, capacity)` sur le thread PUAE et écrire le résultat dans un fichier temporaire.
- [x] Préfixer le fichier final par un en-tête GW GUI contenant version du format, hash du cœur, modèle, options et hashes firmware/médias.
- [x] Remplacer atomiquement l’ancien état seulement après écriture complète.
- [x] Avant `retro_unserialize`, comparer l’en-tête à la machine active et refuser une incompatibilité.
- [x] Tester sauvegarde, exécution de 100 frames, restauration et retour au hash vidéo attendu.

### I — Chemins frontend, modèles et configurations

#### AMI-031 — Créer les chemins définitifs

- [x] Ajouter à `StoragePaths` `EmulationDirectory` puis `AmigaMachinesDirectory`.
- [x] Résoudre en portable sous `<application>/Data/Emulation/Machines/Amiga/` et en installé sous `%AppData%/GW GUI/Emulation/Machines/Amiga/`.
- [x] Créer `Firmware/Kickstart`, `Firmware/Extended`, `Firmware/Keys` et `Configurations` à la première utilisation.
- [x] Tester les deux modes avec `StoragePaths.ResolveDataDirectory` sans dépendre du profil Windows réel.

#### AMI-032 — Indexer les ROM sans les copier

- [x] Écrire `GWGUI.App/Services/Emulation/AmigaFirmwareCatalog.cs`; ce code appartient au frontend et non au moteur Amiga.
- [x] À l’ouverture de Paramètres > Émulation, énumérer les fichiers des trois dossiers firmware et calculer taille, MD5 et SHA-256 par flux.
- [x] Construire les lignes affichables avec chemin, type, hashes, version/région reconnues et modèles compatibles.
- [x] Reconnaître les ROM d’après la table PUAE épinglée, pas uniquement leur nom.
- [x] Garder toute ROM inconnue sélectionnable explicitement avec `IsKnown=false`.
- [x] Lorsque l’utilisateur choisit une ROM, écrire seulement son chemin dans `AmigaMachineConfiguration.KickstartPath`, `ExtendedRomPath` ou `RomKeyPath`.
- [x] Dans `AmigaMachine.StartAsync`, vérifier l’existence et la lisibilité des chemins reçus, puis les passer au cœur sans nouvelle sélection automatique.
- [x] Tester doublon de contenu sous deux noms, ROM supprimée, `rom.key`, ROM CDTV étendue et ROM CD32 combinée.

#### AMI-033 — Définir les modèles comme données validées

- [x] Créer un `AmigaModel` pour `A500OG`, `A500`, `A500PLUS`, `A600`, `A1200OG`, `A1200`, `A2000OG`, `A2000`, `A4030`, `A4040`, `CDTV`, `CD32` et `CD32FR`.
- [x] Pour chaque entrée, fixer valeur `puae_model`, chipset, CPU par défaut, mémoire par défaut, type de firmware et médias disponibles.
- [x] Construire les valeurs configurables depuis les Core Options réellement capturées ; ne jamais proposer une valeur absente de la DLL.
- [x] Rejeter une configuration dont une valeur n’appartient pas à la liste du cœur avant création de la machine.
- [x] Tester chaque modèle et toutes les valeurs par défaut contre le registre d’options de la DLL.

#### AMI-034 — Persister une configuration complète

- [x] Créer `Configurations/<guid>/machine.json`; le GUID sert uniquement à éviter les collisions.
- [x] Sérialiser version de schéma, modèle, hash/version du cœur, firmware, options natives complètes, DF0–DF3, disques durs, CD, clavier, souris, contrôleurs, identifiants de périphériques et mappings.
- [x] Conserver des chemins relatifs lorsqu’ils se trouvent sous `Data`; conserver les chemins absolus externes comme `F:/Disquettes/...`.
- [x] Écrire par fichier temporaire puis remplacement atomique.
- [x] Charger toutes les configurations valides sans démarrer les machines ; isoler une configuration corrompue et continuer les autres.
- [x] À la suppression, effacer uniquement le dossier `<guid>` après confirmation du frontend ; ne jamais effacer une ROM partagée.
- [x] Tester deux A500 différents, sauvegarde/chargement identiques et suppression sans toucher au firmware.

### J — Tous les modèles et plusieurs machines

#### AMI-035 — Créer la matrice de tests modèles

- [x] Créer un jeu de données xUnit par modèle avec Core Option, firmware requis, RAM, standard vidéo et média de test.
- [ ] Tester successivement A500OG/A500, A500+/A600, A2000OG/A2000, A1200OG/A1200, A4030/A4040, CDTV puis CD32/CD32FR.
- [ ] Pour chaque ligne, affirmer chargement réussi, 300 frames, géométrie valide, audio produit et arrêt propre.
- [ ] Marquer le test `Skipped` avec le chemin précis du firmware/média manquant, jamais comme réussite.
- [ ] Ne déclarer un modèle disponible dans l’application qu’après réussite de sa ligne.

#### AMI-036 — Isoler deux PUAE dans le même processus

- [x] Copier la DLL sous deux chemins uniques par instance avant `NativeLibrary.Load`.
- [x] Démarrer deux A500 avec options, dossiers save, ADF et callbacks distincts.
- [x] Envoyer une entrée et un changement de disque à une seule instance puis vérifier que l’autre ne change pas.
- [x] Arrêter la première et vérifier que la seconde produit encore vidéo et audio.
- [x] Si un état global est partagé malgré les chemins distincts, ne pas masquer l’échec : passer au ticket AMI-037.

#### AMI-037 — Ajouter un processus isolé uniquement si AMI-036 échoue

- [ ] Créer `GWGUI.Emulation.Amiga.Host` comme exécutable sans fenêtre chargé d’une seule instance PUAE.
- [ ] Transporter commandes/états par named pipe, vidéo par mémoire partagée double-buffer et audio par ring buffer partagé.
- [ ] Numéroter chaque message et répondre avec succès ou erreur structurée.
- [x] Tuer uniquement le host fautif sur timeout ; conserver les autres machines.
- [x] Rejouer tous les tests multi-instance contre ce transport.

### K — Brancher dans GW GUI après fonctionnement du moteur

#### AMI-038 — Créer la surface d’utilisation minimale

- [x] Ajouter un septième onglet principal localisé `Émulation` dans `MainWindow.xaml`.
- [x] Créer `AmigaEmulationSection` et `AmigaMachineView` alimentés par la dernière `VideoFrame`.
- [x] Copier RGB565/XRGB8888 vers un `WriteableBitmap` sans appeler le cœur natif depuis le contrôle.
- [x] Ajouter sélection de configuration, modèle, Kickstart, média initial, démarrage et plusieurs machines conservées dans des sous-onglets.
- [x] Relier l’onglet principal à Paramètres > Émulation et recharger les configurations après fermeture des paramètres.
- [x] Afficher la vidéo dans un cadre noir centré dont la surface reste strictement au format 4:3.
- [x] Transmettre focus clavier et mouvement relatif de souris au snapshot de la machine active.
- [x] Relier `WasapiAudioOutput` au démarrage/arrêt de la machine.
- [x] Tester le mapping clavier et le calcul 4:3 indépendamment du cœur, puis valider le rendu avec le test local PUAE.

#### AMI-039 — Créer la gestion des configurations dans Paramètres

- [x] Ajouter une page `Émulation` dans `OptionsWindow` et rescanner firmware/configurations à son ouverture.
- [x] Ajouter création depuis un modèle, modification des valeurs compatibles et suppression confirmée.
- [x] Afficher chaque Core Option annoncée avec catégorie, nom, clé, valeur actuelle et toutes les valeurs autorisées ; conserver aussi les clés inconnues du frontend.
- [x] Enregistrer les modifications dans la configuration et les appliquer au prochain démarrage de cette machine ; la page Paramètres ne modifie jamais silencieusement une machine active.
- [x] Ajouter le bouton ouvrant `Data/Emulation/Machines/Amiga/Firmware/` dans l’Explorateur Windows.
- [x] Ne pas implémenter de duplication de configuration.

### L — Distribution et validation finale

#### AMI-040 — Verrouiller le cœur livré

- [x] Comparer le binaire buildbot validé à une compilation du commit PUAE choisi.
- [x] Choisir après mesure entre inclusion et téléchargement ; dans les deux cas conserver un manifeste exact par version de GW GUI.
- [x] Si téléchargement, utiliser URL primaire versionnée, miroir GitHub versionné, taille et SHA-256 ; ne jamais utiliser `latest` à l’exécution.
- [x] Télécharger vers `.tmp`, vérifier PE x64, hash et exports, puis renommer atomiquement.
- [x] Refuser toute mise à jour du cœur indépendante d’une version GW GUI.
- [x] Tester absence réseau, téléchargement tronqué, mauvais hash, mauvais x86/x64 et fallback.

#### AMI-041 — Exécuter la validation complète

- [x] Exécuter les tests ordinaires sans DLL/ROM et vérifier qu’aucun ne dépend du corpus local.
- [x] Exécuter séparément les tests PUAE locaux avec la DLL, Kickstart et ADF.
- [x] Boucler 100 démarrages/arrêts A500 et vérifier threads, handles, fichiers verrouillés et mémoire.
- [ ] Exécuter 30 minutes PAL puis 30 minutes NTSC avec vidéo/audio/entrées et relever underruns/overruns.
- [ ] Vérifier pause, reprise, hard reset, changement de disque, écriture, état et deux machines.
- [x] Vérifier qu’aucune ROM, `rom.key`, image de disquette ou chemin personnel n’entre dans Git ou les artefacts publiés.
- [x] Vérifier par recherche et test d’architecture qu’aucun `retro_*` n’est appelé depuis `GWGUI.App` ou `GWGUI.Emulation`.

## Premier résultat à obtenir avant toute interface

Les tickets AMI-001 à AMI-019 forment le premier passage obligatoire :

1. la DLL charge ;
2. l’environnement répond aux appels réels de PUAE ;
3. `Kickstart 1.3.rom` devient `kick34005.A500` dans le dossier système de session ;
4. `puae_model=A500` est renvoyé au cœur ;
5. un A500 sans disque produit l’écran Kickstart ;
6. `Boot-DD-OFS.adf` puis Workbench 1.3.3 sont passés par `retro_game_info.path` ;
7. le framebuffer prouve l’amorçage ;
8. l’arrêt libère DLL, ROM temporaire et ADF.

Tant que ces huit résultats ne passent pas, ne pas commencer la fenêtre de paramètres, les modèles avancés ou le conditionnement final.
