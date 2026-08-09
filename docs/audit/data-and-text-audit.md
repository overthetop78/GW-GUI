# Audit des données, constantes et textes

## Sources de vérité actuellement dispersées

| Connaissance | Emplacements principaux | Risque |
|---|---|---|
| machines et familles | `DiskClassificationCatalog`, `ImageFormatCatalog`, `DiskImageExplorer`, icônes Explorer, ressources Formats | différence entre les listes des onglets |
| formats et géométries | catalogues Domain, lecteurs d’images, reconstructeurs SCP, tests | valeurs recopiées et corrections partielles |
| extensions | catalogue formats, routage Explorer, classification icônes, dialogues de fichier | une extension acceptée dans un onglet mais absente d’un autre |
| codecs | registres decode/encode, visualiseur, reconstructeurs | codec disponible techniquement mais invisible ou non branché |
| protections | classification, lecteurs Apple, conversion interne, Explorer | protection détectée mais non affichée ou non convertible |
| systèmes de fichiers | registre FS, métadonnées Explorer, icônes/types de fichiers | premier lecteur gagnant au lieu de conserver plusieurs résultats |
| compatibilités de conversion | catalogue, `ConversionSourceCompatibility`, `ConversionPlanner`, capacités gw | choix invalides ou sorties manquantes |
| noms visibles | ressources Formats et valeurs de repli des catalogues | anglais codé en dur ou doublons de traduction |

La phase 3 devra introduire des identifiants stables et des définitions ciblées. Elle ne doit pas créer un unique fichier `Constants.cs` contenant toutes les machines, CRC, couleurs et textes sans relation.

## Nombres techniques

Les codecs et lecteurs contiennent légitimement des valeurs numériques : marques de synchronisation, polynômes CRC, tailles de secteurs, nombres de pistes, vitesses et tables GCR. L’audit distingue :

- une valeur locale nommée, propre à un algorithme : elle peut rester dans la classe du codec ;
- une géométrie partagée par plusieurs lecteurs : elle doit devenir une définition de format ;
- une valeur recopiée dans UI, détection et conversion : elle doit être centralisée ;
- un nombre de mise en page : il appartient à une ressource/style WPF, pas à un catalogue disque.

Les zones prioritaires sont `AtariScpSectorImageReader`, `AppleDiskImageReader`, `IbmPcImageReader`, les lecteurs Commodore, `DiskImageExplorer` et `ImageFormatCatalog`.

## Textes visibles

Les XAML utilisent majoritairement `{l:Loc ...}`. Les catégories restantes à contrôler pendant la phase 3/6 sont :

- messages d’exception anglais remontés tels quels dans un dialogue ;
- libellés de repli anglais dans les catalogues techniques ;
- textes produits par les scripts et affichés à l’utilisateur ;
- noms d’actions ou d’états assemblés par concaténation ;
- symboles `—`, unités et ponctuation qui doivent rester cohérents selon la langue.

Les messages d’erreur doivent conserver deux niveaux :

1. message utilisateur localisé et compréhensible ;
2. détail technique non traduit dans le journal, avec exception et pile.

## Noms qui ne doivent pas être traduits

- noms officiels : `AmigaDOS`, `ProDOS`, `RWTS18`, `CP/M`, `FAT12`, `Greaseweazle` ;
- extensions et arguments : `.scp`, `.adf`, `--format`, etc. ;
- identifiants internes persistés dans les réglages ;
- noms de codecs lorsque le terme officiel est identique dans toutes les langues.

Ces valeurs peuvent être placées dans la ressource neutre commune si elles doivent être affichées, sans recopier artificiellement la même chaîne dans 29 traductions.

## Modèles mêlés à la logique

Les fichiers à séparer lors des phases prévues sont :

- `AppSettings.cs` : modèles de plusieurs domaines et contrat de store ;
- `ImageFormatCatalog.cs` : modèles et implémentations ;
- `ReadRequest.cs`, `WriteRequest.cs`, `ConversionPlanner.cs` : modèles et builders/services ;
- `ScpImage.cs` : modèles de conteneur et parser ;
- `ExplorerSection.xaml.cs`, `ExplorerDetailsPanel.xaml.cs` : modèles de vue et contrôles ;
- `OptionsWindow.xaml.cs` : modèles de lignes et logique de fenêtre.

## Contrôle de ressources

La structure actuelle possède 20 catalogues et 30 variantes chacun. `LocalizationTests.cs` est le garde-fou existant pour :

- parité des clés ;
- valeurs vides ;
- placeholders ;
- encodage corrompu ;
- doublons entre catalogues.

Tout déplacement de ressources devra d’abord adapter le chargeur composite et ces tests, puis déplacer un catalogue à la fois sans changer ses clés ni ses valeurs.

