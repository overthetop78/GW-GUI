# Plan de compréhension et de refactoring du code

Ce document décrit les découpages à envisager sans autoriser leur réalisation immédiate. Un fichier long n’est pas automatiquement mauvais et un fichier court n’est pas automatiquement bien organisé. La décision dépend d’abord de ses responsabilités, de sa croissance prévue et de la facilité à tester une modification isolée.

## Règles

- Ne modifier aucun comportement pendant un découpage structurel.
- Déplacer et tester une responsabilité à la fois.
- Conserver l’interface à onglets; séparer le contenu interne des onglets ne change pas la navigation visible.
- Éviter les fichiers `partial` utilisés uniquement pour cacher la taille d’une classe qui garderait toutes ses responsabilités.
- Un type métier principal par fichier lorsque ce type contient une logique propre.
- Plusieurs petits records ou enums peuvent rester ensemble s’ils constituent un même contrat.
- Extraire un algorithme commun seulement si ses paramètres sont réellement identiques. Deux CRC portant le même nom peuvent employer un polynôme, une valeur initiale, un ordre de bits ou une finalisation différents.
- Tout refactoring doit conserver les tests existants et ajouter un test d’architecture lorsque cela empêche une nouvelle classe monolithique.
- Lorsqu’une fonction est décidée par l’utilisateur, la réaliser complètement et non comme une version minimale provisoire.
- Une possibilité découverte pendant l’analyse reste une proposition jusqu’à décision explicite de l’utilisateur. L’architecture peut la rendre possible sans déclencher son développement ni changer le périmètre en silence.

## 1. Moteur SCP et formats de disquette

### État du moteur de décodage

Le monolithe `FluxDecoding.cs` a été supprimé. Ses quatre responsabilités sont maintenant séparées :

1. contrats et résultats de décodage dans `FluxDecodeModels.cs` et `IFluxDecoder.cs`;
2. registre et sélection automatique dans `FluxDecoderRegistry.cs`;
3. reconstruction d’un flux en cellules binaires dans `Flux/FluxBitstream.cs`;
4. dix-neuf décodeurs, chacun dans son propre fichier sous `Decoding/Decoders`.

La base de reconnaissance commune reste dans `Decoding/Base/SignatureMfmDecoder.cs`. Les générateurs de signatures MFM/FM existants sont isolés dans `Encoding/FluxEncoding.cs`; ce ne sont pas encore des encodeurs complets de pistes ou de conteneurs. Le déplacement a été validé par la compilation Release et les 285 tests existants, sans changement fonctionnel.

Ce moteur ne contient pas encore tous les formats de disquette du produit. Le catalogue Lecture/Écriture/Conversion, les capacités de `gw`, les `diskdefs` et le lecteur du conteneur SCP se trouvent dans d’autres fichiers. Chaque futur décodeur devra conserver cette organisation en obtenant son propre fichier.

### Découpage cible

```text
GWGUI.Scp/
├── Containers/
│   └── Scp/
│       ├── ScpModels.cs
│       ├── IScpReader.cs
│       ├── ScpReader.cs
│       └── ScpWriter.cs                 # futur, seulement si nécessaire
├── Flux/
│   ├── FluxBitstream.cs
│   └── FluxTimingAnalyzer.cs            # futur : estimation et anomalies partagées
├── Decoding/
│   ├── FluxDecodeModels.cs
│   ├── IFluxDecoder.cs
│   ├── FluxDecoderRegistry.cs
│   ├── Base/
│   │   └── SignatureMfmDecoder.cs
│   └── Decoders/
│       ├── RawFluxDecoder.cs
│       ├── IsoMfmDecoder.cs
│       ├── IsoFmDecoder.cs
│       ├── AmigaMfmDecoder.cs
│       ├── AppleIIGcrDecoder.cs
│       ├── AppleMacGcrDecoder.cs
│       ├── CommodoreGcrDecoder.cs
│       ├── MembrainMfmDecoder.cs
│       ├── Aed6200pMfmDecoder.cs
│       ├── QdMo5MfmDecoder.cs
│       ├── CenturionMfmDecoder.cs
│       ├── NorthstarMfmDecoder.cs
│       ├── HeathkitFmDecoder.cs
│       ├── MicralNFmDecoder.cs
│       ├── EmuFmDecoder.cs
│       ├── TycomFmDecoder.cs
│       ├── DecRx02Decoder.cs
│       ├── ArburgDecoder.cs
│       └── Victor9kGcrDecoder.cs
├── SectorImages/                       # modèle intermédiaire, base Amiga réalisée
│   ├── SectorImage.cs
│   ├── SectorCandidate.cs
│   ├── SectorSelectionPolicy.cs
│   └── GeometryResolver.cs
├── Encoding/
│   ├── FluxEncoding.cs                 # générateurs de signatures MFM/FM existants
│   ├── ITrackEncoder.cs                # futur
│   └── Encoders/...                    # futurs encodeurs complets
├── FileSystems/                        # registre et AmigaDOS réalisés
│   ├── IFileSystemReader.cs
│   ├── FileSystemRegistry.cs
│   ├── FileSystemModels.cs
│   └── Readers/                         # un fichier par système de fichiers
└── Images/                             # lecteur ADF et orchestrateur réalisés
    ├── IImageReader.cs
    ├── IImageWriter.cs
    ├── AdfImageWriter.cs
    ├── StImageWriter.cs
    ├── ImaImageWriter.cs
    └── autres conteneurs selon besoin
```

Chaque nouveau décodeur doit avoir son fichier, ses vecteurs de test et une description de référence. Le registre pourra rester explicite afin de connaître l’ordre de détection, mais sa liste ne doit plus partager le fichier des algorithmes.

Le découpage de `FluxDecoding.cs` est réalisé pour les éléments qui existent actuellement. Il sépare explicitement :

- les contrats et résultats communs ;
- la conversion des timings de flux en cellules ;
- le registre et la détection automatique ;
- chaque famille de flux et chaque décodeur dans son propre fichier ;
- les utilitaires d’encodage existants dans une arborescence distincte des décodeurs ;

Les éléments suivants ont maintenant une première réalisation complète pour Amiga, sans être encore généralisés aux autres familles :

- lecture ADF DD/HD vers le modèle sectoriel ;
- reconstruction de secteurs Amiga depuis les charges utiles SCP et choix de la meilleure révolution ;
- interprétation AmigaDOS OFS/FFS utilisée par l’onglet `Explorateur`.

Les écrivains de conteneurs, la reconstruction des autres familles et leurs interpréteurs de systèmes de fichiers restent à développer séparément.

Le découpage a été effectué sans changement de comportement : les tests existants ont été conservés, chaque décodeur garde exactement son algorithme et les traitements communs ne sont pas dupliqués. Les futurs encodeurs et les décodeurs resteront séparés même lorsqu’ils concernent la même famille MFM, FM ou GCR.

Les sources de HxCFloppyEmulator/libhxcfe restent une référence technique majeure pour identifier les structures, marques, encodages, géométries et comportements attendus. Comme `libhxcfe` est distribué sous GPL alors que GW GUI est sous MIT, les algorithmes nécessaires doivent être réimplémentés indépendamment en C#, avec références consignées et vecteurs de tests propres; aucun code HxC n’est copié directement dans le produit.

### Décodeur, encodeur et conteneur

Ces fonctions ne sont pas équivalentes :

- un **lecteur SCP** lit le conteneur et restitue les intervalles de flux;
- un **décodeur** transforme les intervalles/cellules en structures, secteurs et octets;
- un **reconstructeur d’image sectorielle** choisit entre les secteurs trouvés sur plusieurs révolutions et construit une géométrie cohérente;
- un **écrivain d’image** produit un fichier ADF, ST, IMA, D64, etc.;
- un **encodeur de piste** effectue le chemin inverse, secteurs vers MFM/FM/GCR et flux;
- un **écrivain SCP** produit le conteneur brut autour de ce flux.

Le code actuel possède le lecteur SCP, les décodeurs, un modèle sectoriel d'encodage et les 21 encodeurs de pistes correspondants. Pour Amiga, il possède aussi le lecteur ADF, la reconstruction sectorielle SCP, la sélection entre révolutions et le lecteur AmigaDOS. Il ne possède pas encore l’équivalent pour tous les autres formats, les écrivains d’images, l'écrivain SCP complet ni le branchement de ces couches à la conversion interne de l'application.

Les décodeurs de flux ne remplacent pas les interpréteurs de systèmes de fichiers. Cette séparation est maintenant matérialisée : le résultat sectoriel Amiga est transmis à `AmigaDosFileSystemReader`, qui interprète le volume, les répertoires, les fichiers, leurs attributs et les erreurs du système de fichiers.

L’architecture prévoit toutes ces couches afin de ne pas devoir la recommencer lorsqu’elles deviendront utiles. L’utilisateur décide de leur ordre de réalisation. Lorsqu’une couche est retenue, elle doit être réalisée complètement selon le périmètre décidé, sans version volontairement minimale.

### Conversions possibles sans `gw`

Une conversion interne est techniquement possible, mais doit être qualifiée par niveau :

| Conversion | Faisabilité sans `gw` | Travail nécessaire |
|---|---|---|
| SCP vers rapport/secteurs extraits | Élevée | Présentation/export autour des résultats existants |
| SCP vers ADF/ST/IMA | Réaliste pour les décodeurs complets | Géométrie, arbitrage des révolutions, secteurs absents, écrivain du conteneur et tests de comparaison |
| Image sectorielle vers un autre conteneur de même géométrie | Réaliste | Lecteurs/écrivains des deux conteneurs et validation de compatibilité |
| MSA compressé vers ST, ou inversement | Réaliste | Codec MSA et tests connus |
| ADF/ST/IMA vers SCP | Prévu plus tard | Encodeur MFM/FM/GCR, timing, index, révolutions et écrivain SCP; développement repoussé après les priorités actuelles |
| Format logique d’une machine vers une autre machine | À ne pas présenter comme conversion de système de fichiers | Un changement de conteneur ne transforme pas le contenu logique |
| Lecture ou écriture d’une disquette physique | Toujours assurée par `gw` actuellement | Implémenter nous-mêmes le protocole matériel serait un projet séparé inutile pour le moment |

Pour le moment, toutes les conversions de l’application continuent à utiliser `gw`. À terme, GW GUI pourra convertir certains fichiers sans lancer les Host Tools, en priorité SCP vers rapport, secteurs ou images sectorielles. Le moteur interne devra d’abord prouver que chaque sortie est identique ou équivalente sur un corpus de référence et revenir à `gw` pour les formats non couverts. Le chemin inverse vers SCP reste prévu, mais sa réalisation est repoussée à plus tard.

### Fonctions supplémentaires issues du décodage

- rapport d’état par piste, face et secteur;
- liste des secteurs valides, corrompus, absents, dupliqués ou divergents;
- comparaison des révolutions et justification du secteur retenu;
- export des données décodées, JSON et CSV;
- comparaison de deux captures d’une même disquette;
- détection des pistes faibles ou instables;
- reconstruction contrôlée d’images sectorielles;
- base future pour des conversions internes.

## 2. Fenêtre principale

### Problème

`MainWindow.xaml.cs` contient 1 127 lignes. Une seule classe coordonne actuellement Visualisation, Lecture, Écriture, Conversion, profils, `diskdefs`, matériel, Outils, console, progression, placement, fermeture et Host Tools.

`MainWindow.xaml` contient les cinq écrans dans 490 lignes. La taille n’est pas catastrophique, mais toute reprise visuelle charge et modifie le même fichier.

### Découpage cible sans supprimer les onglets

```text
GWGUI.App/
├── MainWindow.xaml                       # TabControl, console et état global
├── MainWindow.xaml.cs                    # composition et événements réellement globaux
├── Tabs/
│   ├── ReadTab.xaml
│   ├── ReadTab.xaml.cs
│   ├── WriteTab.xaml
│   ├── WriteTab.xaml.cs
│   ├── ConvertTab.xaml
│   ├── ConvertTab.xaml.cs
│   ├── VisualizerTab.xaml
│   ├── VisualizerTab.xaml.cs
│   ├── ToolsTab.xaml
│   └── ToolsTab.xaml.cs
├── ViewModels/
│   ├── ReadOperationViewModel.cs
│   ├── WriteOperationViewModel.cs
│   ├── ConversionOperationViewModel.cs
│   ├── VisualizerViewModel.cs             # futur
│   └── ToolsViewModel.cs                  # futur
└── Services/
    ├── OperationExecutionService.cs       # cycle commun
    ├── ProfileApplicationService.cs
    └── FormatCatalogService.cs
```

La fenêtre affichera toujours les mêmes cinq `TabItem`. Chaque `TabItem` contiendra simplement un contrôle séparé. Cette séparation facilite la reprise pas à pas demandée par l’utilisateur et empêche une modification de Lecture de toucher involontairement Conversion.

Ordre conseillé : extraire d’abord Visualisation, puis Outils, puis Conversion, Écriture et enfin Lecture. Les deux premiers sont les plus indépendants; Lecture partage aujourd’hui davantage d’état avec le reste.

## 3. Options et outils

### `OptionsWindow.xaml/.cs`

La fenêtre possède déjà quatre pages logiques : Général, Host Tools, Matériel et Profils. À 225 lignes de code, le découpage n’est pas urgent, mais les langues, versions et validations matérielles la feront grossir.

Découpage à réaliser au moment de sa reprise visuelle :

```text
Options/
├── OptionsWindow.xaml
├── GeneralOptionsPage.xaml
├── HostToolsOptionsPage.xaml
├── HardwareOptionsPage.xaml
├── ProfilesOptionsPage.xaml
└── OptionsViewModel.cs
```

La fenêtre reste une seule boîte Options avec navigation latérale. Les pages séparées ne deviennent pas de nouvelles fenêtres.

### `GwToolWindow.xaml.cs`

Cette fenêtre générique couvre plusieurs diagnostics à partir du verbe demandé. Elle peut rester unique tant que les différences sont décrites par des modèles de champs et que le code ne grossit pas fortement. Si de nouvelles commandes ajoutent des interfaces très différentes, extraire un `ToolDefinitionCatalog` et des définitions par outil, sans créer automatiquement neuf fenêtres dupliquées.

### Petites fenêtres et services de dialogue

`AboutWindow`, `ProfileNameWindow`, `ReadConflictWindow`, `DriveEditorWindow`, `LogHistoryWindow` et les services de dialogue ont des responsabilités courtes et claires. Ils restent séparés tels quels. Les records/enums de requête peuvent rester avec leur interface tant qu’ils ne sont utilisés que par ce contrat.

## 4. Formats, commandes et réglages

### `ImageFormatCatalog.cs`

Ce fichier mélange : modèles de format, interface, adaptation aux capacités de `gw` et catalogue intégré. Il ne fait que 138 lignes, mais le catalogue doit grandir.

Découpage à effectuer avant une extension massive des formats :

```text
Formats/
├── DiskFormat.cs
├── ImageExtension.cs
├── IImageFormatCatalog.cs
├── BuiltInImageFormatCatalog.cs
├── CapabilityAwareImageFormatCatalog.cs
├── FormatCompatibility.cs
└── Catalogs/                             # si les données deviennent nombreuses
    ├── AmigaFormats.cs
    ├── AtariFormats.cs
    ├── IbmPcFormats.cs
    ├── CommodoreFormats.cs
    └── autres familles
```

Il faudra éviter de disperser quelques lignes par machine trop tôt. Le sous-dossier `Catalogs` devient utile lorsque les formats supplémentaires rendent réellement le catalogue central difficile à relire.

### `AppSettings.cs`

Il contient le schéma principal et huit types de réglages. Sa taille actuelle reste correcte. Avec les futures langues, mises à jour et options par onglet, séparer plus tard :

- `AppSettings.cs` pour la racine et le numéro de schéma;
- `UiSettings.cs` pour fenêtre/thème/langue;
- `OperationSettings.cs` pour Lecture/Écriture/Conversion;
- `HardwareSettings.cs` pour contrôleurs et lecteurs;
- `ProfileSettings.cs` pour la persistance des profils.

La migration doit rester centralisée dans `SettingsMigrator.cs`; elle ne doit pas être éparpillée dans les modèles.

### Requêtes Lecture/Écriture/Conversion

`ReadRequest.cs` contient aussi `EnabledOption` et le tokenizer de ligne de commande. À terme :

- déplacer `EnabledOption` dans `Commands` car il est partagé;
- déplacer `CommandLineTokenizer` dans son propre fichier;
- conserver `ReadRequest` et `ReadCommandBuilder` ensemble ou les séparer seulement si chacun gagne une logique importante.

`WriteRequest.cs` mélange détection de format et construction de commande. Séparer ultérieurement `ImageFormatDetector.cs`, `DetectedImageFormat.cs`, `WriteRequest.cs` et `WriteCommandBuilder.cs`.

`ConversionPlanner.cs` mélange modèles de sélection, planificateur et constructeur historique. Séparer les records lorsque les conversions internes seront ajoutées afin de distinguer clairement planification via `gw` et planification native.

### Validation et maintenance

`GwOptionValidator.cs`, `ToolCommandBuilder.cs`, `GwProgressTracker.cs` et les petits constructeurs de commandes restent d’une taille raisonnable et ont des responsabilités identifiables. Ne pas les découper simplement pour réduire leur nombre de lignes. Si de nouvelles grammaires sont ajoutées, créer un validateur par type structuré (`TrackSpec`, `PllSpec`, `PrecompSpec`) plutôt qu’allonger une seule suite de conditions.

## 5. Infrastructure

`GwInstallationManager.cs`, `GreaseweazleRunner.cs`, `JsonSettingsStore.cs`, `RotatingOperationLogWriter.cs` et le registre matériel ont chacun une responsabilité principale. Ils doivent rester séparés comme aujourd’hui.

Évolutions possibles uniquement si leur fonction grandit :

- extraire un client GitHub Releases commun lorsque la mise à jour de GW GUI sera ajoutée, afin de ne pas dupliquer le code réseau des Host Tools;
- extraire téléchargement, validation ZIP et installation de `GwInstallationManager` si ces étapes deviennent réutilisées;
- conserver la découverte série Windows dans Infrastructure et jamais dans les vues.

## 6. Localisation

Les fichiers `.resx` contiennent 407 clés mais plusieurs entrées XML sont compactées sur une même ligne. Avec onze langues, la difficulté sera davantage la cohérence que la taille d’un fichier individuel.

Avant les nouvelles traductions :

- reformater les ressources de manière stable et lisible;
- créer un catalogue central des cultures;
- ajouter un outil de parité, placeholders, doublons, valeurs vides et longueurs suspectes;
- maintenir un glossaire technique;
- conserver au départ un fichier `Strings.<culture>.resx` par langue pour ne pas multiplier les `ResourceManager`;
- ne séparer en plusieurs familles de ressources que si les 407 clés augmentent au point de rendre les outils ou l’édition réellement pénibles.

`LocExtension.cs` peut rester petit. Il devra dépendre du futur catalogue de cultures et appliquer le repli vers l’anglais.

## 7. Tests

`CoreTests.cs` contient 2 672 lignes et couvre de nombreuses fonctions sans rapport. Son découpage améliorerait la navigation mais ne change ni le logiciel ni la couverture.

Décision : priorité très basse. Il pourra être séparé beaucoup plus tard en fichiers par domaine, ou lorsqu’une modification des tests devient réellement difficile. Les nouveaux tests peuvent dès maintenant être créés dans de nouveaux fichiers ciblés sans déplacer immédiatement les anciens.

Découpage futur possible :

```text
Tests/
├── Commands/
├── Conversion/
├── Formats/
├── Hardware/
├── Profiles/
├── Settings/
├── Scp/
│   └── Decoders/                         # un fichier de tests par décodeur
├── UI/
└── Infrastructure/
```

## 8. Classement des travaux

### À faire avant que les fonctions concernées grossissent

1. Diviser `FluxDecoding.cs` avant d’ajouter une nouvelle série importante de décodeurs.
2. Séparer `MainWindow.xaml` et `MainWindow.xaml.cs` pendant la reprise progressive des écrans.
3. Séparer le catalogue intégré avant l’ajout massif de formats.
4. Préparer les ressources et outils de localisation avant les neuf langues supplémentaires.

### À faire au moment de la fonction associée

5. Le modèle sectoriel et les 21 encodeurs de pistes sont désormais créés dans `GWGUI.Scp/Encoding`, avec un fichier par encodeur et un registre vérifiant la parité avec les décodeurs. L'écriture d'un conteneur SCP complet et son branchement à la conversion native restent des travaux distincts.

6. Séparer les pages Options lors de leur reprise visuelle.
7. Extraire un client de releases commun lors de la notification de mise à jour GW GUI.
8. Séparer les modèles de réglages lors du prochain changement de schéma important.

### À laisser pour bien plus tard

9. Réorganiser `CoreTests.cs`.
10. Séparer les fichiers entre 50 et 140 lignes qui conservent une seule responsabilité claire.

## 9. Séquence sûre de refactoring

Pour chaque futur découpage :

1. exécuter les tests avant modification;
2. déplacer uniquement les types, sans réécrire les algorithmes;
3. compiler et exécuter les tests;
4. vérifier qu’aucune API publique ni chaîne localisée n’a changé;
5. seulement ensuite extraire les duplications réellement identiques;
6. recompiler et tester chaque extraction;
7. effectuer les améliorations fonctionnelles dans des commits séparés.

Cette séparation entre déplacement structurel et évolution fonctionnelle permettra d’identifier immédiatement l’origine d’une régression.

## 10. Crédits, dépendances et références

La fenêtre `Aide → À propos` doit proposer une section lisible et localisée avec liens cliquables. Chaque entrée indique son rôle afin de ne pas laisser croire qu’une référence étudiée est intégrée au programme.

Entrées actuellement identifiées :

| Projet | Rôle dans GW GUI | Lien |
|---|---|---|
| Greaseweazle | Matériel et Host Tools exécutés par l’application | `https://github.com/keirf/greaseweazle` |
| HxCFloppyEmulator/libhxcfe | Référence technique étudiée pour formats, décodeurs et visualisation; code non intégré | `https://github.com/jfdelnero/HxCFloppyEmulator` |
| SkiaSharp | Bibliothèque de rendu utilisée par le visualiseur | `https://github.com/mono/SkiaSharp` |
| .NET et WPF | Plateforme et interface de l’application | `https://github.com/dotnet/wpf` |
| Inno Setup | Construction de l’installateur Windows | `https://jrsoftware.org/isinfo.php` |

La liste doit être générée depuis une définition maintenable, pas écrite en plusieurs endroits. Toute nouvelle dépendance ou référence réellement utilisée ajoute son entrée, sa licence et son lien. Les mentions légales distribuées avec les bibliothèques restent incluses lorsque leurs licences l’exigent.
