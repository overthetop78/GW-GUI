# Intégration du module d’émulation Amstrad

## Statut et objet

Ce document fixe l’architecture à réaliser pour `GWGUI.Emulation.Amstrad`. Il décrit la cible
confirmée ; il ne prétend pas que le module existe déjà. Les actions ouvertes et leur ordre sont
dans [`../tasks/emulation/amstrad.md`](../tasks/emulation/amstrad.md).

Le module porte l’identifiant stable `amstrad` et reste une seule DLL. Il doit accueillir les CPC,
les CPC Plus, la GX4000 et, plus tard, les PCW. La première réalisation utilise les cœurs Libretro
Caprice32 et CrocoDS. L’architecture ne doit toutefois pas imposer Libretro à un futur moteur écrit
pour GW GUI.

Il n’existe pas aujourd’hui de document d’intégration équivalent pour Amiga. Atari possède la
référence technique [`../reference/atari-libretro.md`](../reference/atari-libretro.md), mais son
architecture actuelle n’est pas le modèle à recopier : elle associe encore un seul cœur à chaque
modèle. Amstrad devient le modèle propre à suivre lorsque les modules Amiga et Atari seront repris.

## Décisions qui ne doivent plus être réinterprétées

- Une machine possède une seule configuration enregistrée dans l’interface actuelle. Il n’existe
  pas une configuration « CPC 6128 Caprice32 » et une seconde « CPC 6128 CrocoDS ».
- Le moteur sélectionné ne fait pas partie de la configuration matérielle de la machine. Changer de
  moteur ne crée, ne remplace et ne sauvegarde aucune configuration de machine.
- Le choix du moteur est mémorisé séparément, par identifiant de machine, par le module concerné.
- L’App affiche le choix et appelle le contrat commun ; elle ne connaît ni Caprice32, ni CrocoDS, ni
  PUAE, ni les moteurs Atari.
- Une famille conserve une seule DLL. Les moteurs sont des implémentations internes distinctes de
  cette DLL, raccordées à une base commune par contrat et catalogue.
- Aucun contrat ancien parallèle, adaptateur de compatibilité ou repli vers un moteur inscrit dans
  une configuration ne doit être conservé. Le passage au nouveau contrat est une bascule unique.
- Les anciennes données Amiga ou Atari ne doivent jamais décider du moteur après cette bascule. Un
  ancien champ de cœur éventuellement rencontré est une propriété obsolète, pas une source de choix.
- PCW reste dans le module `amstrad`, mais aucun PCW ne paraît dans `Machines` tant qu’un moteur PCW
  n’est pas intégré et validé. MAME est un candidat futur, pas une dépendance de la première version.
- Le développement futur d’un moteur interne remplace ou complète une implémentation de moteur ; il
  ne modifie ni l’identité des machines, ni leurs configurations, ni le contrat App/module.

## Fonctionnement actuel constaté et point à corriger

`GWGUI.App` charge dynamiquement chaque module depuis `Modules/<id>/module.json`, puis lui transmet
un `EmulationModuleContext`. Son `ModuleDirectory` correspond au dossier persistant
`Data/Emulation/Machines/<id>`, et non au dossier contenant la DLL distribuée.

L’écran générique possède déjà une `ComboBox` de moteurs. Cependant,
`EmulationEmulatorManagementController.RefreshAsync` demande une seule installation au module,
place son identifiant dans un tableau d’un élément et force `SelectedIndex = 0`. Le contrat
`IEmulationEmulatorManager` ne sait actuellement ni énumérer les moteurs compatibles, ni lire le
choix, ni l’enregistrer.

Atari possède plusieurs cœurs, mais `AtariConfigurationFunctions.GetCore(model)` en choisit un seul,
`AtariMachineConfiguration.Core` le transporte, et `AtariCoreCatalogFunctions` interdit qu’un modèle
soit associé à plusieurs cœurs. Amiga conserve aussi `AmigaMachineConfiguration.Core`, alors que
`AmigaEngine` lance toujours PUAE. Ces deux mécanismes doivent être retirés après la réalisation
d’Amstrad ; ils ne doivent pas être reproduits.

## Périmètre fonctionnel

### Périmètre de la branche CPC/GX4000

Le catalogue cible du module comprend les identifiants stables suivants :

| Identifiant | Machine | Moteur initial prévu pour les essais |
|---|---|---|
| `cpc-464` | Amstrad CPC 464 | Caprice32 |
| `cpc-664` | Amstrad CPC 664 | Caprice32 |
| `cpc-6128` | Amstrad CPC 6128 | Caprice32 et CrocoDS |
| `cpc-464-plus` | Amstrad CPC 464 Plus | Caprice32, association à confirmer par les essais |
| `cpc-6128-plus` | Amstrad CPC 6128 Plus | Caprice32 |
| `gx4000` | Amstrad GX4000 | Caprice32 |

Les six modèles font partie de `Machines` et sont visibles dans l’App sur la branche `amstrad` dès
leur raccordement. Une validation manquante ne masque pas le modèle : elle garde la tâche ouverte et
signifie que la branche n’est pas terminée. La branche ne doit pas être intégrée à `main` et aucune
version complète ne doit être publiée tant que chaque modèle n’a pas prouvé démarrage, vidéo, audio,
entrées, médias, reset, arrêt et libération des ressources. Si le moteur prévu ne permet pas de faire
fonctionner un modèle, il faut résoudre ce manque ou demander une décision ; il ne faut pas masquer
le modèle pour déclarer la branche terminée.

### Étape PCW ultérieure

Les modèles PCW 8256, 8512, 9512, 9256 et 9512+ restent documentés dans le catalogue matériel
général. Ils seront ajoutés au même module seulement après choix et validation d’un moteur. Leur
future configuration, leurs ROM, leur clavier, leur imprimante et leurs lecteurs ne doivent pas être
simulés par des options CPC. Aucun identifiant PCW n’est ajouté au catalogue exécutable pendant la
première étape.

## Moteurs initiaux et capacités vérifiées

Les capacités suivantes proviennent de la documentation et des fichiers d’information officiels
Libretro consultés le 24 septembre 2026. Elles servent de borne ; l’implémentation doit encore les
vérifier avec les DLL Windows x64 réellement proposées avant de considérer la branche terminée.

| Capacité | Caprice32 | CrocoDS |
|---|---|---|
| Identifiant GW GUI | `caprice32` | `crocods` |
| Bibliothèque Windows x64 | `cap32_libretro.dll` | `crocods_libretro.dll` |
| Systèmes officiellement annoncés | CPC et GX4000 | CPC |
| Formats annoncés | DSK, SNA, ZIP, TAP, CDT, VOC, CPR, M3U | DSK, SNA, KCR |
| Chemin complet requis | oui | non |
| Démarrage sans contenu | oui | non |
| États sérialisés | oui | oui |
| Sauvegardes persistantes Libretro | oui | non |
| Contrôle multidisque Libretro | oui | non |
| Fréquence vidéo annoncée | 50 Hz | 50 Hz |
| Fréquence audio annoncée | 44 100 Hz | 44 100 Hz |
| Licence annoncée | GPL-2.0 | MIT |

Caprice32 expose les modèles `464`, `664`, `6128` et `6128+ (experimental)`. Son fichier
d’information annonce aussi GX4000 et les cartouches CPR. Le support exact du 464 Plus ne peut pas
être déduit du seul libellé général : le modèle reste visible sur la branche pour permettre les
essais, mais la branche reste inachevée tant que son mode Plus à 64 Kio n’est pas correctement pris
en charge.

CrocoDS annonce seulement « CPC » et n’expose pas un sélecteur de modèle matériel comparable à
Caprice32. La première association prévue est donc `cpc-6128`, sous réserve du contrôle de la machine
effectivement démarrée. Aucune association CrocoDS avec CPC 464, CPC 664, CPC Plus ou GX4000 ne doit
être inventée à partir de ce libellé générique.

Caprice32 est le moteur par défaut de chaque machine avec laquelle il est validé. CrocoDS est un
choix alternatif, jamais un repli silencieux. Si le moteur sélectionné n’est pas installé, le
lancement affiche l’erreur commune « émulateur non installé » pour cet identifiant ; il ne lance pas
Caprice32 à sa place.

Sources techniques : [documentation Caprice32](https://docs.libretro.com/library/caprice32/),
[informations du cœur Caprice32](https://github.com/libretro/libretro-core-info/blob/master/cap32_libretro.info),
[dépôt Caprice32 Libretro](https://github.com/libretro/libretro-cap32),
[documentation CrocoDS](https://docs.libretro.com/library/crocods/),
[informations du cœur CrocoDS](https://github.com/libretro/libretro-core-info/blob/master/crocods_libretro.info)
et [dépôt CrocoDS Libretro](https://github.com/libretro/libretro-crocods).

## Séparation des trois données persistantes

### Configuration de machine

`AmstradMachineConfiguration` décrit uniquement la machine et les choix utilisateur indépendants du
moteur : `ModuleId`, `SchemaVersion`, `Id`, `MachineId`, écran couleur ou monochrome lorsque le
matériel le permet, audio, entrées, lecteurs et médias configurés. Elle ne contient aucun
`Emulator`, `Engine`, `Core`, chemin de DLL, version installée ou option native `cap32_*`/`crocods_*`.

Les valeurs natives sont produites au lancement par l’adaptateur du moteur depuis la configuration
commune. Une caractéristique impossible à exprimer sans nom de cœur reste dans l’adaptateur ; elle
ne devient pas un champ de la configuration. L’interface ne doit pas exposer toutes les options
natives simplement parce qu’un cœur les publie.

Les configurations résident, selon la convention actuelle, sous :

```text
Data/Emulation/Machines/amstrad/Configurations/<configuration-id>/machine.json
```

L’App continue d’en retenir une par `MachineId`. Le stockage emploie une écriture atomique et des
chemins relatifs à `DataDirectory` lorsque cela est possible.

### Sélection du moteur

Le choix réside séparément sous :

```text
Data/Emulation/Machines/amstrad/Emulators/Selections/<machine-id>.json
```

Chaque fichier contient exactement :

```json
{
  "schemaVersion": 1,
  "machineId": "cpc-6128",
  "emulatorId": "caprice32"
}
```

Le nom de fichier provient uniquement d’un identifiant de machine présent dans le catalogue, jamais
d’un texte utilisateur. Le store vérifie que le `machineId` du document correspond au fichier et que
le moteur figure dans la liste compatible. L’écriture passe par un fichier temporaire placé dans le
même dossier, puis par remplacement atomique ; le temporaire est supprimé dans `finally` après
réussite, erreur ou annulation.

En l’absence de fichier, le catalogue retourne son moteur par défaut sans créer de donnée. Une
sélection explicite écrit le fichier. Un fichier invalide n’est ni interprété comme une
configuration, ni transformé en ancien champ de cœur, ni écrasé silencieusement ; l’erreur remonte
par le parcours contrôlé de l’App.

### Installation des moteurs

Une installation reste indépendante du choix : plusieurs moteurs peuvent être installés en même
temps. La disposition commune est :

```text
Data/Emulation/Machines/amstrad/Core/
  caprice32/
    active.json
    <version>/
      cap32_libretro.dll
      core.json
  crocods/
    active.json
    <version>/
      crocods_libretro.dll
      core.json
```

`active.json` désigne la version active d’un moteur installé ; il ne désigne jamais le moteur choisi
pour une machine. `core.json` conserve l’URL, la date, la version déclarée, le SHA-256, l’architecture
PE et les exports contrôlés. Le téléchargement, l’extraction et le remplacement sont annulables ;
les fichiers `.download` et `.extract` sont supprimés dans `finally`.

## Contrat commun App/module à obtenir

Le sélecteur n’est pas à créer : `EmulationCoreManagementPanel.Emulators` est déjà une `ComboBox`,
`EmulationModuleSettingsSection` place déjà ce panneau dans l’onglet Général et appelle déjà
`RefreshAsync` à l’ouverture, au changement de machine et au changement de configuration. Le travail
dans l’App consiste uniquement à fournir plusieurs éléments à cette liste et à traiter le changement
de sélection.

Le contrat actuel sait déjà retourner l’installation du moteur courant, rechercher ses versions et
l’installer. Il manque seulement l’énumération des moteurs compatibles et l’enregistrement du choix.
La forme cible minimale est donc :

```csharp
public interface IEmulationEmulatorManager
{
    IReadOnlyList<EmulationEmulatorDefinition> GetEmulators(string machineId);
    ValueTask SelectEmulatorAsync(string machineId, string emulatorId,
        CancellationToken cancellationToken = default);
    ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId,
        CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId,
        CancellationToken cancellationToken = default);
    ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}
```

`EmulationEmulatorDefinition` contient `Id` et `DisplayResourceKey`. Le moteur courant est déjà donné
par `EmulationEmulatorInstallation.EmulatorId`; ajouter une seconde méthode de lecture du choix
dupliquerait cette information. `EmulationEmulatorInstallation` contient aussi `InstalledVersion` et
`InstallationPath`. Ces deux valeurs sont nulles quand le moteur n’est pas installé et toutes les
deux renseignées quand l’installation active est valide. Le chemin est celui de la DLL effectivement
chargée, pas seulement celui du dossier de versions. Les trois méthodes d’installation existantes
continuent d’agir sur le moteur sélectionné pour la machine. Chaque module valide l’identifiant
demandé par `SelectEmulatorAsync` et ne choisit jamais un repli silencieux.

Cette évolution du contrat public impose l’API hôte `2.0` et le paquet `GWGUI.Emulation.SDK`
`2.0.0`, sans créer d’interface `V2` ni conserver l’ancien contrat en parallèle. Elle ne justifie
aucune modification de `IEmulationModule.RuntimeOptions`, de `EmulationMachineRuntime`, de
`EmulationSectionConfigurationFunctions` ou du protocole générique de création des runtimes.

Pour Amstrad, `RuntimeOptions(configuration)` produit des réglages sémantiques communs. La machine
Amstrad déjà créée connaît son propre adaptateur et traduit ces réglages vers les variables natives
du cœur actif. Un changement préparé dans le sélecteur ne peut donc pas changer le moteur d’une
machine déjà ouverte ; il ne prend effet qu’à la prochaine création du runtime.

Les noms affichés restent traduits par le module : l’App reçoit la définition, puis appelle
`LocExtension.GetForModule(module, DisplayResourceKey)`. Les noms propres invariants Caprice32 et
CrocoDS sont placés dans `Resources/00-Base`, pas recopiés dans les 29 cultures.

## Parcours exact dans l’App

1. Le `RefreshAsync` existant appelle `GetEmulators(machineId)`.
2. L’App localise chaque libellé et peuple la liste dans l’ordre du catalogue.
3. Elle appelle `GetEmulatorInstallationAsync(machineId)` et sélectionne son `EmulatorId` sans
   déclencher une écriture due au chargement visuel.
4. Si version et chemin sont absents, elle affiche le bouton d’installation. S’ils sont présents,
   elle affiche le badge installé et conserve toujours le chemin complet de la DLL visible dans le
   statut, y compris après réouverture, changement de machine ou nouveau rafraîchissement.
5. Un choix utilisateur appelle `SelectEmulatorAsync`, puis rafraîchit seulement l’état
   d’installation et le statut. Il n’appelle ni `ApplySettings`, ni `SaveConfigurationAsync`.
6. Les méthodes existantes de recherche et d’installation portent sur le moteur sélectionné. Un changement de machine ou de
   moteur annule l’opération en cours avant d’en commencer une autre.
7. Une machine déjà active conserve son moteur jusqu’à sa fermeture. Le nouveau choix s’applique à
   la prochaine création du runtime.

`EmulationCoreManagementPanel` conserve sa structure actuelle : aucun nouveau sélecteur, aucun
second panneau et aucun changement de disposition ne sont prévus. Le contrôleur existant reçoit le
module uniquement pour localiser les libellés, ajoute l’abonnement `SelectionChanged`, distingue un
rafraîchissement d’un geste utilisateur et protège la vue contre une réponse asynchrone devenue
obsolète. Comme il possède ses abonnements et son `CancellationTokenSource`, il les détache, annule
et libère quand la section est fermée.

## Structure du module Amstrad

```text
src/GWGUI.Emulation.Amstrad/
  GWGUI.Emulation.Amstrad.csproj
  module.json
  Constants/       identifiants, chemins, protocole et erreurs stables
  Contracts/       configuration, médias, sélection et diagnostics sérialisés
  Enums/           modèles, moteurs, catégories de médias et matériel
  Dictionaries/    catalogues immuables des machines, moteurs et compatibilités
  Interfaces/      contrats internes des moteurs et factories
  Functions/       validations et transformations pures
  Modules/         factory dynamique et façade IEmulationModule
  Services/        stores, téléchargement, installation et orchestration
  Engines/
    Common/         cycle de vie Amstrad réellement partagé
    Libretro/       hôte Libretro partagé par Caprice32 et CrocoDS seulement
    Caprice32/      options, médias, entrées et factory propres à Caprice32
    CrocoDS/        options, médias, entrées et factory propres à CrocoDS
  Resources/
    00-Base/
    ar-SA/ ... zh-Hant/
```

`IAmstradEngine` annonce son identifiant et crée une machine à partir de la configuration commune,
du chemin de son binaire et du contexte d’exécution. `AmstradEngineCatalog` enregistre chaque moteur
une seule fois. `AmstradEmulationModule.CreateRuntimeAsync` lit le choix par `MachineId`, valide la
compatibilité, résout la version installée, puis demande au catalogue la factory correspondante.
La machine créée conserve son adaptateur de moteur ; aucun ajout d’identifiant n’est nécessaire dans
`EmulationMachineRuntime`. Il n’utilise pas de `switch` dispersé sur l’identité du moteur.

La couche `Engines/Libretro` peut réutiliser l’ABI commune de `GWGUI.Emulation` et mutualiser les
callbacks vidéo, audio, entrée, environnement, disque et état réellement identiques. Les valeurs de
variables, les formats, l’amorçage et les limites de média restent dans `Caprice32` ou `CrocoDS`.
Un futur moteur interne implémente `IAmstradEngine` directement et ne dépend pas de cette couche.

Le cœur natif s’exécute hors du processus graphique, comme les moteurs actuels. Le module reconnaît
une commande hôte comprenant l’identifiant du moteur, le pipe et la mémoire vidéo. Le propriétaire
du processus, des pipes, mappings, callbacks, threads et DLL les arrête, détache et libère dans un
bloc `finally`, attend la fin effective du processus et ne détruit jamais un objet emprunté à l’App.

## Configuration matérielle et traduction vers les moteurs

Le catalogue machine porte les caractéristiques fixes : modèle, famille Classic/Plus/GX4000,
mémoire nominale, clavier, nombre de ports, lecteurs présents et types de médias. Ces données ne sont
pas des options de moteur.

Les adaptateurs traduisent ensuite la même configuration :

- Caprice32 reçoit `cap32_model` depuis le modèle validé, `cap32_ram` depuis la mémoire matérielle,
  et les options d’écran/entrée depuis les champs communs correspondants ;
- CrocoDS reçoit seulement les variables qu’il annonce réellement, notamment écran et taille
  d’image ; il ne reçoit pas une fausse variable de modèle ;
- l’autorun reste une politique explicite du moteur et ne modifie pas l’identité de la machine ;
- une option native non comprise par la configuration commune n’est pas persistée dans
  `AmstradMachineConfiguration`.

Le résumé de configuration affiche le matériel, les médias et les choix utilisateur communs, jamais
le moteur sélectionné. Le moteur apparaît dans le panneau de gestion dédié.

## Médias, entrées et états

- CPC 464 expose la cassette et seulement les lecteurs réellement configurés/validés ; CPC 664 et
  CPC 6128 exposent leur lecteur de disquette ; GX4000 expose la cartouche.
- CPC Plus n’est pas assimilé à GX4000 : les périphériques viennent du modèle, pas de l’extension du
  fichier chargé.
- Caprice32 accepte les formats annoncés seulement sur les emplacements cohérents : DSK/M3U pour la
  disquette, TAP/CDT/VOC pour la cassette, CPR pour la cartouche et SNA pour un chargement d’état
  explicitement pris en charge.
- CrocoDS accepte DSK, SNA et KCR, sans annoncer de changement de disque à chaud. Son périphérique
  disquette porte `RequiresMachineRecreation = true` lorsqu’un remplacement exige de recréer la
  machine.
- ZIP n’est transmis à Caprice32 qu’après validation du comportement exact et de l’unicité du contenu ;
  GW GUI ne choisit pas arbitrairement un fichier dans une archive ambiguë.
- Le lecteur CPCEMU DSK/EDSK, la reconnaissance CPC/PCW, le système de fichiers CP/M Amstrad et le
  décodage CDT déjà présents dans les bibliothèques média sont réutilisés par leurs API publiques.
  Ils ne sont ni copiés ni déplacés dans le module.
- La configuration des entrées décrit le clavier CPC et les deux ports joystick communs. Chaque
  moteur traduit ensuite les touches et périphériques Libretro qu’il gère réellement.
- `IEmulationSavedStates.IsSupported` reflète la sonde du cœur chargé. Un état porte au minimum
  l’identité du moteur et sa version afin de refuser proprement un chargement incompatible.

## Installation, manifeste et distribution

Le projet cible `net10.0` et `x64`, référence `GWGUI.Emulation` et `GWGUI.MediaEngine`, embarque ses
29 cultures plus `00-Base`, et expose exactement une `AmstradEmulationModuleFactory` publique.

Le manifeste initial utilise `id: amstrad`, `entryAssembly: gwgui.emulation.amstrad.dll`, une
`moduleVersion` commençant à `1.0.0`, les bornes d’API hôte `2.0` et le catalogue
`module-amstrad-catalog/update-catalog.json`. Le registre public reçoit `module-registry/amstrad.json`
seulement dans le même chantier de distribution.

Les scripts existants découvrent automatiquement les projets `src/GWGUI.Emulation.*` contenant un
`module.json`. Il ne faut pas ajouter une liste Amstrad en dur à leurs boucles. La solution et les
projets de tests reçoivent en revanche leurs références explicites.

## Traductions

Le module fournit `IEmulationModuleLocalization`. Les noms propres de famille, machines, moteurs,
CPU, RAM, ROM et formats restent dans `Resources/00-Base/Emulation.resx`. Chaque phrase, explication,
état et libellé traduisible existe dans les 29 catalogues distribués. Argos est utilisé avec le
script existant, puis chaque ressource est relue ; Google Traduction n’est pas utilisé.

Les nouveaux textes génériques du sélecteur appartiennent à
`GWGUI.App/Resources/<culture>/Emulation/EmulationCore.resx`. Les textes propres au CPC et aux
moteurs appartiennent au module Amstrad.

## Reprise d’Atari et d’Amiga après Amstrad

La reprise se fait après validation du module Amstrad, avec le même contrat final et sans couche de
compatibilité.

### Atari

- retirer `Core` de `AtariMachineConfiguration` et retirer `GetCore(model)` ;
- permettre à plusieurs entrées moteur d’annoncer le même modèle et désigner un défaut dans le
  catalogue, sans modifier les associations actuellement offertes tant qu’aucun second moteur
  Atari n’est validé ;
- stocker la sélection par machine dans `Emulators/Selections`, séparément des configurations ;
- passer explicitement le moteur sélectionné à `AtariEngine` et à la factory ;
- conserver les installations versionnées par moteur déjà présentes sous `Core/<id>` ;
- adapter l’interface, les tests et la documentation sans interpréter un ancien champ `Core`.

### Amiga

- retirer `Core` et `ValidatedCoreSha256` de `AmigaMachineConfiguration` lorsqu’ils ne décrivent pas
  le matériel ;
- créer un catalogue moteur où PUAE est l’unique choix initial et le défaut de toutes les machines
  compatibles ;
- adopter les mêmes dossiers de sélection et d’installation versionnée qu’Amstrad ;
- faire choisir sa factory à `AmigaEngine` par identifiant explicite, afin qu’un second moteur puisse
  être ajouté sans modifier la configuration ;
- adapter l’interface, les tests et la documentation sans conserver l’ancien contrat.

Ces reprises doivent préserver les comportements matériels déjà fonctionnels : machines, médias,
firmwares, options communes, entrées, vidéo, audio, états et arrêt. Elles ne sont pas l’occasion de
changer ces comportements ni d’ajouter de nouveaux moteurs.

## Critères d’acceptation

La réalisation est complète seulement si :

- une seule configuration existe par machine dans le parcours actuel ;
- CPC 6128 propose Caprice32 et CrocoDS après validation, mémorise le choix séparément et le retrouve
  après redémarrage ;
- deux moteurs peuvent être installés simultanément et posséder chacun leur version active ;
- le moteur sélectionné installé affiche toujours le chemin complet de sa DLL active, pas seulement
  immédiatement après le téléchargement ;
- changer de moteur ne modifie pas `machine.json` et changer les réglages matériels ne modifie pas le
  fichier de sélection ;
- le runtime lance exactement le moteur choisi, sans repli silencieux ;
- les six machines restent visibles pendant le développement et chacune a réussi ses essais
  fonctionnels avec chaque moteur finalement proposé avant toute intégration à `main` ;
- fermeture, annulation, erreur et répétition de lancement ne laissent aucun processus, thread,
  handle, mapping, DLL verrouillée ou résidu graphique ;
- Amiga et Atari compilent sur le contrat final après leur reprise et conservent leur comportement ;
- le SDK, le modèle de module, les manifestes, les ressources, les documents et les tests décrivent
  tous la même architecture, sans texte différé ou contrat 1.0 contradictoire.
