# Structure cible de la phase 02

## Statut du document

Ce document prépare uniquement la structure cible demandée par la section 2.1.1 de la phase 02. Il reprend les projets existants et les frontières établies par l’audit sans refaire leur cartographie fonctionnelle complète.

La structure ci-dessous est une **proposition à contrôler et à faire valider** avant tout nouveau déplacement. La table exhaustive des emplacements actuels et cibles relève de la section 2.1.2 et n’est pas incluse ici.

## Projets existants

La solution contient actuellement six projets de production et deux projets de tests :

- `GWGUI.App` : interface WPF, composition et coordination entre fonctions ;
- `GWGUI.Domain` : règles métier, contrats et modèles indépendants de l’interface et des implémentations techniques ;
- `GWGUI.Infrastructure` : implémentations Windows, processus, matériel, stockage et Host Tools ;
- `GWGUI.MediaEngine` : lecture, reconnaissance, reconstruction, conversion et représentation des médias ;
- `GWGUI.Emulation` : contrats et modèles communs aux moteurs d’émulation ;
- `GWGUI.Emulation.Amiga` : implémentation Amiga et intégration de ses cœurs ;
- `GWGUI.Tests` : tests automatisés du produit ;
- `GWGUI.LocalDiskImageTests` : tests dépendant du corpus local d’images non distribué.

## Arborescence actuelle utile

Cette vue est volontairement limitée aux niveaux qui permettent de comprendre les futurs rangements. `bin` et `obj` sont omis.

```text
src/
├── GWGUI.App/
│   ├── Assets/
│   ├── Controls/
│   ├── Input/
│   ├── Localization/
│   ├── Options/
│   ├── Rendering/
│   ├── Resources/
│   ├── Services/
│   ├── ViewModels/
│   └── fenêtres, vues, StoragePaths et ThemeManager à la racine
├── GWGUI.Domain/
│   ├── Commands/       ├── Conversion/   ├── Formats/
│   ├── Hardware/       ├── HostTools/    ├── Maintenance/
│   ├── Naming/         ├── Parity/       ├── Profiles/
│   ├── Read/           ├── Settings/     └── Write/
├── GWGUI.Infrastructure/
│   ├── Hardware/       ├── HostTools/
│   ├── Processes/      └── Settings/
├── GWGUI.MediaEngine/
│   ├── Composition/    ├── Containers/      ├── Conversion/
│   ├── Decoding/       ├── Definitions/     ├── Encoding/
│   ├── Exploration/    ├── FileSystems/     ├── Flux/
│   ├── Geometries/     ├── Migration/       ├── Primitives/
│   ├── Recognition/    ├── Reconstruction/  ├── Representations/
│   ├── SectorImages/   ├── TrackImages/     └── Visualization/
├── GWGUI.Emulation/
│   └── contrats communs à la racine
└── GWGUI.Emulation.Amiga/
    ├── Cores/
    └── services, modèles et stockage Amiga à la racine

tests/
├── GWGUI.Tests/
└── GWGUI.LocalDiskImageTests/
```

## Arborescence cible proposée

Les noms ci-dessous définissent les propriétaires fonctionnels recherchés. Ils n’imposent pas la création d’un dossier vide ni le déplacement d’un fichier avant l’établissement de la table de la section 2.1.2.

```text
src/
├── GWGUI.App/
│   ├── Bootstrap/                 # démarrage et composition de l’application
│   ├── Shell/                     # MainWindow, navigation, menu, terminal, statut, progression
│   ├── Features/
│   │   ├── Read/                  # présentation et contrôleurs propres à Lecture
│   │   ├── Write/                 # présentation et contrôleurs propres à Écriture
│   │   ├── Conversion/            # présentation et contrôleurs propres à Conversion
│   │   ├── Visualizer/            # vues et présentation du visualisateur
│   │   ├── Explorer/              # vues et présentation de l’explorateur
│   │   ├── Tools/                 # outils GW et maintenance
│   │   ├── Options/               # fenêtre, pages et contrôleurs d’options
│   │   └── Emulation/             # interface et coordination des émulateurs
│   ├── Shared/
│   │   ├── Controls/              # contrôles visuels réellement réutilisables
│   │   ├── Dialogs/               # abstractions et dialogues partagés
│   │   ├── Navigation/            # ouverture et placement des fenêtres
│   │   └── Presentation/          # modèles de présentation communs sans règle métier
│   ├── Rendering/                 # rendu WPF/Skia propre à l’application
│   ├── Input/                     # adaptation des entrées de l’interface
│   ├── Localization/              # raccordement WPF aux ressources localisées
│   ├── Resources/                 # ressources WPF et textes distribués
│   └── Assets/                    # ressources binaires copiées au build
├── GWGUI.Domain/
│   ├── Commands/                  # contrats et règles communs de commande
│   ├── Read/                      # requêtes, validation et planification de Lecture
│   ├── Write/                     # requêtes, validation et planification d’Écriture
│   ├── Conversion/                # planification et compatibilité de Conversion
│   ├── Formats/                   # catalogue métier commun et capacités déclarées
│   ├── Hardware/                  # description et routage abstraits du matériel
│   ├── HostTools/                 # contrats et modèles des outils hôtes
│   ├── Maintenance/               # opérations de maintenance
│   ├── Naming/                    # règles de nommage et de conflits
│   ├── Profiles/                  # contrats et modèles de profils par opération
│   ├── Settings/                  # modèles de réglages par domaine et migrations abstraites
│   └── Parity/                    # règles métier de parité lorsqu’elles sont indépendantes du média
├── GWGUI.Infrastructure/
│   ├── Hardware/Windows/          # découverte et identification propres à Windows
│   ├── Hardware/Greaseweazle/     # registre matériel et adaptation Greaseweazle
│   ├── HostTools/                 # installation et détection des outils externes
│   ├── Processes/                 # exécution, annulation et sorties de processus
│   ├── Logging/                   # sessions, rotation et stockage des journaux
│   └── Settings/                  # persistance concrète des réglages
├── GWGUI.MediaEngine/
│   ├── Composition/               # assemblage des registres et composants du moteur
│   ├── Containers/                # reconnaissance et ouverture des conteneurs
│   ├── Recognition/               # détection, interprétations et normalisation
│   ├── Decoding/                  # contrats, registre et décodeurs de flux
│   ├── Reconstruction/            # reconstruction sectorielle par famille
│   ├── SectorImages/              # modèles sectoriels communs
│   ├── FileSystems/               # contrats, registre, aides et lecteurs de systèmes
│   ├── Encoding/                  # contrats, registre et encodeurs de pistes
│   ├── Conversion/                # conversions internes de médias
│   ├── Exploration/               # orchestration et résultats d’exploration
│   ├── Visualization/             # politiques de classification visuelle
│   ├── Representations/           # représentations reconnues partagées
│   ├── TrackImages/               # modèles de pistes et protections
│   ├── Flux/                      # modèles et opérations propres au flux
│   ├── Geometries/                # géométries spécialisées par famille
│   ├── Definitions/               # identifiants et définitions techniques stables
│   ├── Primitives/                # bits, CRC et primitives neutres
│   └── Migration/                 # migration entre systèmes de fichiers
├── GWGUI.Emulation/
│   ├── Contracts/                 # interfaces des machines et moteurs
│   └── Models/                    # état et données communs sans dépendance UI
└── GWGUI.Emulation.Amiga/
    ├── Cores/                     # adaptations des cœurs Amiga
    ├── Configuration/             # modèles, catalogue et persistance Amiga
    ├── Firmware/                  # découverte et gestion des firmwares
    ├── Input/                     # adaptation des entrées Amiga
    ├── Runtime/                   # moteur, machine et cycle d’exécution
    └── Storage/                   # états, médias et stockage propres à Amiga

tests/
├── GWGUI.Tests/                   # tests rapides rangés par projet et fonction de production
└── GWGUI.LocalDiskImageTests/     # corpus local et scénarios lourds séparés
```

## Rôle et frontière de chaque projet

### `GWGUI.App`

Possède uniquement l’interface WPF, les modèles de présentation, les adaptateurs d’entrée et de rendu ainsi que la composition globale. Une fonction UI possède ses contrôles, son état de présentation et ses gestionnaires. `MainWindow` conserve seulement la coque et les échanges réellement transversaux.

### `GWGUI.Domain`

Possède les règles métier qui restent valides sans WPF, Windows, stockage JSON ni processus concret. Les contrats nécessaires aux implémentations techniques sont définis ici lorsque leur sens appartient au métier principal.

### `GWGUI.Infrastructure`

Implémente les contrats du domaine pour Windows, Greaseweazle, les processus, les journaux, les Host Tools et la persistance. Elle ne choisit ni l’affichage ni les formats proposés par l’application.

### `GWGUI.MediaEngine`

Possède le traitement interne des médias, depuis le conteneur jusqu’aux représentations, systèmes de fichiers, conversions et politiques de visualisation technique. Il reste utilisable sans WPF, sans Windows et sans Greaseweazle.

### `GWGUI.Emulation`

Expose les contrats et modèles communs permettant à l’application de piloter un moteur d’émulation sans connaître son implémentation. Il ne dépend d’aucune machine particulière ni de l’interface WPF.

### `GWGUI.Emulation.Amiga`

Implémente les contrats d’émulation pour Amiga. Les règles, firmwares, cœurs et stockages propres à Amiga restent dans ce projet et ne remontent pas dans le projet commun.

### Projets de tests

`GWGUI.Tests` vérifie rapidement les unités et intégrations ciblées. `GWGUI.LocalDiskImageTests` isole les scénarios dépendant d’images externes. Leur organisation détaillée sera arrêtée dans la section 2.7.

## Dépendances autorisées

```text
GWGUI.Domain
    ↑
GWGUI.Infrastructure

GWGUI.MediaEngine

GWGUI.Emulation
    ↑
GWGUI.Emulation.Amiga

GWGUI.App ──→ GWGUI.Domain
          ├─→ GWGUI.Infrastructure
          ├─→ GWGUI.MediaEngine
          ├─→ GWGUI.Emulation
          └─→ GWGUI.Emulation.Amiga

GWGUI.Tests et GWGUI.LocalDiskImageTests ──→ projets nécessaires aux scénarios testés
```

Règles autorisées :

- `GWGUI.Infrastructure` peut référencer `GWGUI.Domain` pour en implémenter les contrats ;
- `GWGUI.Emulation.Amiga` peut référencer `GWGUI.Emulation` pour implémenter ses contrats communs ;
- `GWGUI.App` peut référencer tous les projets de production puisqu’il constitue le point de composition ;
- les projets de tests peuvent référencer les projets nécessaires à leurs scénarios ;
- les dépendances vers des bibliothèques externes restent localisées dans le projet qui adapte leur technologie.

## Dépendances interdites

- `GWGUI.Domain` ne dépend d’aucun autre projet de production, de WPF, de Windows, du stockage concret ou d’un moteur de média ;
- `GWGUI.MediaEngine` ne dépend ni de l’application, ni du domaine principal, ni de l’infrastructure, ni de l’émulation ;
- `GWGUI.Emulation` ne dépend ni de l’application, ni d’une implémentation de machine, ni de WPF ;
- `GWGUI.Infrastructure` ne dépend pas de `GWGUI.App`, de WPF, de `GWGUI.MediaEngine` ou d’une implémentation d’émulation ;
- `GWGUI.Emulation.Amiga` ne dépend pas de `GWGUI.App`, de WPF, de `GWGUI.Infrastructure` ou de `GWGUI.MediaEngine` ;
- aucun projet de production ne dépend d’un projet de tests ;
- aucune dépendance circulaire entre projets n’est autorisée ;
- un dossier `Shared`, `Common`, `Services` ou équivalent ne doit pas servir à contourner ces frontières.

## Limite avant la suite

Cette proposition ne vaut pas validation de la structure. La section 2.1.2 doit encore attribuer une destination à chaque fichier de production, puis la section 2.1.3 doit contrôler et faire valider la proposition avant tout déplacement supplémentaire.
