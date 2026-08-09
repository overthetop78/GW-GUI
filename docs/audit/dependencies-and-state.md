# Dépendances, composition et état partagé

## Dépendances entre projets

```text
GWGUI.Domain
    ↑
GWGUI.Infrastructure

GWGUI.Scp

GWGUI.App ──→ GWGUI.Domain
          ├─→ GWGUI.Infrastructure
          └─→ GWGUI.Scp

GWGUI.Tests ─→ les quatre projets
```

- `GWGUI.Domain` ne référence aucun autre projet : contrats, requêtes, règles de commande, formats, matériel, profils et réglages.
- `GWGUI.Infrastructure` référence `GWGUI.Domain` : Windows, processus, JSON, journaux et installation des Host Tools.
- `GWGUI.Scp` ne référence aucun autre projet : conteneurs/images, flux, codecs, secteurs et systèmes de fichiers.
- `GWGUI.App` référence les trois autres projets et SkiaSharp : composition, WPF, rendu et orchestration.
- `GWGUI.Tests` référence tout : tests unitaires, corpus locaux et tests d’intégration.

Cette direction générale est saine. Le problème principal n’est pas une référence circulaire entre projets, mais le nombre d’implémentations concrètes composées ou pilotées directement par `MainWindow` et `DiskImageExplorer`.

## Point de composition actuel

`MainWindow` construit ou conserve directement :

- le stockage des réglages et les trois magasins de profils ;
- les services de dialogues et navigation ;
- la présentation des formats ;
- le registre des décodeurs ;
- `DiskImageExplorer.CreateDefault()` ;
- le visualisateur sectoriel ;
- le suivi de progression, le verrou de visualisation et le minuteur ;
- les ViewModels d’opération et plusieurs services techniques.

Il s’agit à la fois d’un point de composition et d’un orchestrateur de toutes les fonctions. La séparation future doit conserver une composition unique, mais transmettre des contrats ciblés à chaque onglet au lieu de laisser la fenêtre connaître toutes les implémentations.

## États qui doivent rester globaux

- une seule commande Greaseweazle active ;
- le matériel configuré et son état connecté/déconnecté ;
- les réglages persistants ;
- la langue et le thème ;
- le document disque courant partagé entre Visualisateur et Explorateur lorsqu’un même fichier est ouvert ;
- la console et la barre d’état communes.

## États qui doivent rester propres à une opération

- profil Lecture, profil Écriture et profil Conversion ;
- paramètres avancés de chaque onglet ;
- source, destination et sélection de formats de chaque opération ;
- progression et résultat de la commande active, avec réinitialisation au démarrage d’une nouvelle commande ;
- état d’annulation et nettoyage de la sortie partielle.

## Risques repérés

- `MainWindow.xaml.cs` accède directement à de nombreux contrôles nommés : déplacer un bloc WPF sans contrat explicite casserait sa portée.
- les stores de profils sont distincts mais coordonnés par la même classe ; leur séparation doit conserver l’indépendance par onglet.
- Explorateur et Visualisateur partagent le fichier courant, mais chacun peut relancer analyse, reconstruction ou rendu. Un futur document disque commun devra éviter les recalculs sans figer l’interface.
- `FileSystemRegistry` et `DiskImageExplorer` dépendent de l’ordre des lecteurs lors de la détection automatique ; cet ordre ne doit pas devenir un comportement implicite non testé.

