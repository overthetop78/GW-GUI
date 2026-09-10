# Architecture technique actuelle

## Projets de production

- `GWGUI.App` : application WPF, composition des fenêtres et contrôles, présentation, contrôleurs applicatifs et services propres à l’interface.
- `GWGUI.Domain` : contrats et règles métier indépendants de WPF.
- `GWGUI.Infrastructure` : implémentations techniques liées à Greaseweazle, Windows, la persistance et les services externes.
- `GWGUI.MediaEngine` : reconnaissance des images, conteneurs, flux, décodage, encodage, reconstruction, systèmes de fichiers, exploration et conversion interne.
- `GWGUI.Emulation` : contrats, services et fonctions communs à toutes les familles émulées.
- `GWGUI.Emulation.Amiga` : modèles, catalogues, fonctions et modules propres à l’Amiga.
- `GWGUI.Emulation.Atari` : modèles, catalogues, fonctions et modules propres aux machines Atari.
- `GWGUI.Launcher` : démarrage et sélection du binaire de l’application.

## Projets de validation

- `GWGUI.Tests` contient la suite rapide par défaut et une catégorie `GpuExhaustive` séparée pour les matrices complètes de shaders et de moteurs de rendu.
- `GWGUI.LocalDiskImageTests` reste un outil local séparé de la solution principale pour les contrôles utilisant le corpus privé.

## Dépendances entre projets

`GWGUI.Domain`, `GWGUI.MediaEngine`, `GWGUI.Emulation` et `GWGUI.Launcher` ne référencent aucun autre projet GW GUI.

`GWGUI.Infrastructure` référence uniquement `GWGUI.Domain`.

`GWGUI.Emulation.Amiga` et `GWGUI.Emulation.Atari` référencent `GWGUI.Emulation` et `GWGUI.MediaEngine`. Elles ne se référencent jamais entre elles. `GWGUI.App` ne les référence pas : il les découvre dynamiquement dans `Modules`.

`GWGUI.App` compose `GWGUI.Domain`, `GWGUI.Infrastructure`, `GWGUI.MediaEngine` et les trois projets d’émulation. Les bibliothèques de production ne référencent jamais `GWGUI.App`.

## Organisation commune du code

Chaque projet place les éléments dans le dossier correspondant à leur responsabilité réelle :

- `Constants` pour les constantes ;
- `Contracts` pour les contrats et données transportées ;
- `Dictionaries` pour les catalogues et correspondances ;
- `Enums` pour les ensembles fermés ;
- `Factories` pour la composition d’objets ;
- `Functions` pour les fonctions sans état ;
- `Interfaces` pour les frontières justifiées ;
- `Services` pour les responsabilités possédant un cycle de vie ou des dépendances ;
- `Views`, `ViewModels`, `Presenters` et `Controllers` uniquement dans l’application lorsque la responsabilité appartient à l’interface.

Une fonction ou une donnée réellement commune à plusieurs familles d’émulation appartient à `GWGUI.Emulation`. Une différence propre à Amiga ou Atari reste dans sa bibliothèque spécialisée.

## Moteur média

`GWGUI.MediaEngine` sépare actuellement :

- `Containers` et `Recognition` pour reconnaître et ouvrir les sources ;
- `Flux` et `TrackImages` pour les données de pistes et de révolutions ;
- `Decoding` et `Encoding` pour les codecs ;
- `Reconstruction` et `SectorImages` pour produire les secteurs et géométries ;
- `FileSystems` et `Exploration` pour les volumes, dossiers et fichiers ;
- `Conversion` et `Migration` pour les transformations internes ;
- `Visualization` et `Representations` pour les projections techniques ;
- `Composition` pour assembler les services publics du moteur.

La chaîne complète, les registres actuels et le contrat `IImageDisquette` sont décrits dans [l’architecture média](media.md).

## Émulation

`GWGUI.Emulation` définit les contrats communs des modules, machines, médias, entrées, vidéo, audio, messages et sauvegardes d’état. Les modules Amiga et Atari fournissent leurs catalogues et implémentations spécialisées à l’application.

Les règles détaillées de cette séparation sont décrites dans [l’architecture modulaire de l’émulation](emulation.md).

## Application WPF

`GWGUI.App` sépare les fenêtres dans `Views/Windows`, les composants réutilisables dans `Views/Controls` et les responsabilités non visuelles dans leurs contrôleurs, présentateurs, fonctions et services respectifs.

Les réglages et outils de gestion sont répartis entre trois fenêtres accessibles depuis la fenêtre principale :

- **Options > Préférences…** ouvre les réglages généraux de GW GUI : général, journaux, contrôleurs et lecteurs, moteurs, profils et manettes ;
- **Émulation > Préférences d’émulation…** ouvre seulement les réglages communs Général, Raccourcis et Configurations ;
- chaque autre entrée dynamique du menu **Émulation** ouvre une boîte de dialogue indépendante alimentée par le module installé correspondant ;
- **Options > Mises à jour…** ouvre le gestionnaire des versions de GW GUI et des modules d’émulation.

Ces fenêtres conservent des responsabilités distinctes. Les préférences enregistrent le comportement général de l’application, les préférences d’émulation enregistrent les réglages partagés, chaque fenêtre de module configure directement ses machines, et la fenêtre des mises à jour gère la découverte, la préparation groupée, l’installation et le remplacement des composants distribués.

Les ressources de langue sont réparties par culture sous `Resources`. `00-Base` contient les catalogues neutres ; chaque culture distribuée possède les mêmes catalogues spécialisés.

## Persistance et état

Les réglages, profils, configurations d’émulation, journaux, matériel et placements de fenêtres restent gérés par leurs services propriétaires. Une vue ne doit pas devenir la source persistante d’une donnée métier.

## Évolution de l’architecture

Ce document décrit le code actuel. Les travaux encore ouverts sont indexés dans
[`../tasks/README.md`](../tasks/README.md); un nouvel audit général de qualité recevra sa propre
feuille lorsqu’il sera demandé.
