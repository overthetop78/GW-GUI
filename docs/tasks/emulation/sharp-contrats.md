# Contrats Sharp

## Sources et preuves

Les paquets Windows x64 officiels et leurs SHA256 sont relevés dans `artifacts/sharp-source/catalogue.json`. Les identités, formats et options ci-dessous proviennent des DLL effectivement interrogées dans `binary-options.json`, pas d'une ancienne documentation. PX68k : dépôt `libretro/px68k-libretro`, révision `0ad84d7058a12b7db4f7f7a906e87fad4e2f26f6`. X1 : dépôt `libretro/xmil-libretro`, révision `3106aa54ffb244cca4019a6e95dacaa698781bc3`, sélectionné depuis la recette officielle de distribution. Les répertoires d'un ancien fork X1 ne servent pas de référence.

## Machines et matériel

| Machine demandée | Moteur et adaptation | Limite à conserver dans les vérifications |
|---|---|---|
| X68000 | PX68k ; CPU nominal 10 MHz, IPL X68000 | Validation du démarrage avec firmware et sortie réelle avant déclaration de fonctionnement. |
| X68000 XVI | PX68k ; CPU nominal 16 MHz, IPL XVI | Le chargeur accepte `iplromxv.dat` ; isoler le firmware sélectionné pour éviter que `iplrom.dat` prenne la priorité. |
| X68030 | PX68k charge `iplrom30.dat`, vitesse disponible 25 MHz | Le moteur compilé utilise C68K ; le chargement de l'IPL ne prouve pas une émulation complète du jeu d'instructions 68030. Vérifier le démarrage et distinguer matériel décrit et CPU effectivement implémenté avant de déclarer la compatibilité. |
| X1 et compatibles | X1 ; option native `X1_ROMTYPE` : `X1`, `TURBO`, `TURBOZ` | Les modèles proposés doivent correspondre aux types natifs et aux données matérielles attestées ; ne pas multiplier les machines par supposition. |

Une machine sans adaptation compatible reste dans le catalogue avec une liste de moteurs vide. Ne pas ajouter un cadre de message sur un moteur incompatible. Les infos matérielles utilisent des constantes et enums nommés, la fréquence est présentée en MHz ; les buffers natifs ne définissent pas la mémoire physique du modèle.

## Firmware

| Moteur | Fichier attendu | Taille constatée localement | Rôle et adaptation |
|---|---|---|---|
| PX68k | `iplrom.dat`, `iplrom30.dat`, `iplromco.dat` ou `iplromxv.dat` | 131072 octets chacun | IPL obligatoire ; le chargeur cherche ces noms dans cet ordre. Préparer uniquement l'IPL choisi dans le dossier privé du moteur, avec son nom attendu. |
| PX68k | `cgrom.dat`, sinon `cgrom.tmp` | 786432 octets | Police obligatoire ; respecter l'ordre de repli du chargeur. |
| X1 | `IPLROM.X1` | 4096 octets | IPL externe facultatif pour `X1` ; le moteur initialise auparavant `biosmem` avec `defaultiplrom`. La taille de lecture maximale de 32768 octets n'est pas une exigence de taille du fichier. |
| X1 | `IPLROM.X1T` | 32768 octets | IPL externe facultatif pour `TURBO` et `TURBOZ`, même mécanisme de repli. Ne pas confondre un commentaire du catalogue avec le rôle réel dans `io/memio.c`. |

Les originaux locaux sont sous `F:/Retro/Sega/Roms/Recalbox_10.0.8_BIOS_Pack/keropi/` et `xmil/`. Les emprunter uniquement en lecture pour les vérifications ; les copies de session appartiennent au module. Aucun choix de dossiers par défaut dans Général ; la sélection des fichiers ROM reste dans ROM.

## Supports et périphériques

| Moteur | Formats annoncés par le binaire | Capacités attestées dans la source |
|---|---|---|
| PX68k | `dim`, `img`, `d88`, `88d`, `hdm`, `dup`, `2hd`, `xdf`, `hdf`, `cmd`, `m3u` | Deux lecteurs FDD ; sélection du lecteur pour le changement de disque ; démarrage avec deux chemins de disquettes ; HDD `.hdf` via `-h` ; listes M3U et commandes CMD ; démarrage sans contenu accepté. |
| X1 | `dx1`, `zip`, `2d`, `2hd`, `tfd`, `d88`, `88d`, `hdm`, `xdf`, `dup`, `cmd` | Le chemin chargé est transmis à `xmil_main`, qui monte le lecteur 0. Les callbacks de changement de disque ne traitent que ce lecteur. Ne pas présenter une deuxième unité interchangeable ni un interpréteur CMD sans preuve supplémentaire du chemin natif. Le chargement sans contenu n'est pas accepté. |

Les capacités viennent des adaptateurs, pas de tests d'identité du moteur dans la machine commune. Ne pas assimiler tous les formats annoncés à des médias réellement interprétés : vérifier les chemins ZIP/CMD et le format retenu avant de les offrir. Les écritures natives sur disquette doivent emprunter la préparation des médias existante et respecter les supports montés. Ne pas annoncer de protection en écriture absente du chemin natif. Le SRAM PX68k et sa configuration native sont des sauvegardes internes, pas des Memory Card inventées.

## Entrées

Le clavier physique ordinaire suit les affectations communes du SDK ; seule une touche propre à la machine qui manque sur le clavier PC obtient une affectation spéciale éditable, persistée et appliquée. Ne pas employer des identifiants Amiga, Acorn ou d'une autre marque pour ces touches. Une ancienne touche physique réaffectée ne doit plus activer la commande spéciale par un alias natif implicite.

PX68k : `libretro.c/handle_retrok` est le chemin de polling effectif. Help utilise le code clavier natif F11 ; Undo utilise End ; OPT.1 et OPT.2 utilisent les codes natifs Super/Alt ; Symbol et Register utilisent Print et ScrollLock. XF1 à XF5 et les commandes Kana, Roman, Code, Hiragana, Full-width sont accessibles par Compose avec F1 à F10. Encoder ces combinaisons dans l'adaptateur, pas dans les contrats communs ; maintenir correctement l'état de Compose à l'appui et au relâchement et vérifier les interactions simultanées. Les entrées `#if 0` ne prouvent pas une commande active. F12 ouvre le menu interne du moteur : ce n'est pas une touche matérielle à inventer dans le catalogue.

X1 : `libretro/sdlkbd.c` convertit le polling clavier. Les touches PC communes sont déjà prises en charge. Une entrée absente de cette table, notamment une commande Kana, ne doit pas être proposée sans chemin natif attesté. Les codes du pavé numérique égale et les autres touches absentes du clavier PC doivent réutiliser un identifiant commun réellement approprié ou une nouvelle identité Sharp, jamais un alias étranger. `mousemng.c` contient des fonctions internes, mais l'adaptateur Libretro ne poll pas `RETRO_DEVICE_MOUSE` et n'appelle ni `mousemng_sync` ni `mousemng_buttonevent` : ne pas proposer de capture de souris hôte ni d'onglet de boutons souris pour ce moteur sur cette seule présence. L'option `Mouse-Key` ne prouve pas ce raccordement.

PX68k fournit deux ports et les types natifs 2 boutons, CPSF-MD, CPSF-SFC ; le premier port propose aussi Cyberstick numérique et analogique. X1 lit le port 0, quatre directions et quatre boutons A/B/X/Y dans `libretro/joymng.c`. Aucune source physique n'est associée automatiquement. Les types, commandes et visuels doivent décrire du matériel attesté ; les réglages de souris et de manette vont dans leurs onglets respectifs, pas dans Général. Une commande sans association retourne zéro au cœur.

## Options et disposition

Les 25 options PX68k et 16 options X1 sont conservées comme protocole dans leurs adaptateurs respectifs. Dédupliquer les valeurs répétées, notamment `4` dans `X1_CPU_CLOCK`, sans changer leur texte natif. Les clés, valeurs, tailles et limites utilisent des constantes ; les choix réellement fermés utilisent des enums adaptés. Les valeurs invariantes restent dans la base de ressources.

| Préoccupation | Placement et traitement |
|---|---|
| Vitesse CPU et mémoire | CPU et RAM ; valeur nominale distincte de l'accélération. |
| Sélection ROM X1 | Déduite du modèle ; pas un deuxième choix contradictoire. |
| Résolution, rotation éventuelle, cadence, saut d'images, affichage des couches | Vidéo ; seules les fonctions effectivement natives sont proposées. Préserver le bloc de traitement GW GUI existant et les shaders communs. |
| ADPCM, OPM, échantillonnage, filtre éventuel | Audio ; préserver le cadre audio commun et placer les options natives dans le cadre dédié. |
| Type de joystick, analogique, permutation de boutons, souris | Manettes ou Souris ; conserver les associations utilisateur et éviter les réglages qui les contredisent. |
| Chemins FDD/HDD, dossier, taille de police du menu natif | Interne à l'adaptateur si nécessaire ; aucune génération automatique de champ dans l'interface. |
| Options de désynchronisation ou d'ordre audio/vidéo | N'exposer que si utiles et fonctionnelles dans le parcours existant, pas par copie de toutes les variables natives. |

Aucun suffixe « redémarrage requis » dans les libellés ; les besoins internes de redémarrage restent gérés par le cycle de vie. Traduire les textes réellement nouveaux avec Argos dans toutes les cultures et réutiliser la base commune. Tous les fichiers suivent la règle des instructions et entrées complètes sur une ligne physique.

## Cycle de vie et validation restante

Les contrats, la fabrique et la machine communs sont indépendants de Libretro. Le chargement de DLL, les callbacks, les tables natives, la préparation des dossiers attendus et les conversions de codes résident dans Interop et dans l'adaptation de chaque moteur. L'installation doit télécharger le paquet officiel réellement vérifié, pas une reconstruction locale.

Créer le cœur dans le processus hôte déjà utilisé ; attendre la destruction du processus, fermer les canaux et libérer les buffers, threads et fichiers de session dans finally, y compris après échec de firmware, premier rendu ou interruption. La fenêtre et le Dispatcher App appartiennent à leurs propriétaires ; ne pas détruire un objet emprunté.

Lire la géométrie, la cadence, l'audio et la taille d'état seulement dans la phase native appropriée. PX68k annonce un maximum 800 × 600 et un rapport 4:3, avec dimensions courantes variables. X1 prévoit 640 × 400 ou une construction QVGA 320 × 200 ; relever ce que publie le paquet installé, sans déduire sa construction du nom. Les implémentations de sauvegarde présentes dans les sources ne prouvent pas un aller-retour d'état réussi.

La preuve finale doit couvrir les vues App, chaque moteur réellement installé, un premier rendu avec audio, les affectations utilisateur, les montages disponibles, pause/reprise, réinitialisations, sauvegarde/restauration si fonctionnelles, démarrages en erreur et absence de processus ou sessions après fermeture. Ce contrat ne constitue pas une preuve de fonctionnement de Sharp ; ces contrôles restent dans le plan.

## Capacités transmises aux services communs

La fabrique PX68k transmet la capture souris à la machine commune ; X1 ne la transmet pas. La machine reçoit cette capacité comme booléen et ne teste aucun identifiant de moteur. Dans les sources X1 inspectées, `retro_reset` est vide : sa fabrique doit transmettre `requiresResetRestart: true` pour que le reset ferme le processus et recrée le cœur. Les états natifs sont ensuite vérifiés avec un nouveau processus. Le besoin de redémarrage d'une option est une donnée du catalogue concret et ne dépend pas d'un libellé affiché.
