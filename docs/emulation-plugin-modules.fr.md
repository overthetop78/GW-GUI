# Évolution future — modules d’émulation chargeables

## 1. Statut et objectif

Ce document décrit une évolution future. Il ne décrit pas le fonctionnement actuel et n’autorise aucune modification tant que sa mise en œuvre n’a pas été demandée.

Aujourd’hui, `GWGUI.App` référence directement `GWGUI.Emulation.Amiga` et `GWGUI.Emulation.Atari`. Ces DLL sont donc obligatoires au démarrage. L’objectif est de permettre l’ajout, le retrait et la mise à jour d’un module d’émulation sans modifier ni recompiler l’application principale.

Résultat attendu :

- chaque famille d’émulation est fournie par un module autonome ;
- aucun module ne dépend d’un autre module ;
- l’application démarre même si aucun module n’est installé ;
- un module absent, incompatible ou défectueux ne bloque pas les autres ;
- un futur module s’installe en déposant son dossier dans le répertoire prévu ;
- l’interface reste commune et reçoit les données des modules par des contrats stables ;
- les configurations et comportements existants restent compatibles pendant la migration.

Lorsqu’elle sera implémentée, cette évolution remplacera la décision de liaison statique décrite dans `emulation-ui-module-architecture.fr.md`.

## 2. Organisation cible

```text
GW GUI/
├── gwgui.exe
├── lib/
├── Languages/
└── Modules/
    ├── Amiga/
    │   ├── module.json
    │   └── gwgui.emulation.amiga.dll
    └── Atari/
        ├── module.json
        └── gwgui.emulation.atari.dll
```

Chaque module conserve dans son dossier son assembly, son manifeste et ses dépendances privées. Les bibliothèques communes restent dans `lib`. Les données utilisateur restent dans le dossier de données de GW GUI, isolées par identifiant de module, jamais à côté des DLL.

## 3. Séparation des responsabilités

### `GWGUI.Emulation`

Ce projet devient le SDK commun. Il contient uniquement :

- les interfaces publiques nécessaires aux modules ;
- les contrats de données, enums et identifiants communs ;
- les fonctions et constantes réellement partagées ;
- la version de l’API des modules.

Il ne référence aucun module concret. Les entrées communes, telles que `EmulationInputSnapshot` et `EmulationKey`, y restent afin que l’application puisse transmettre les mêmes données à chaque module.

### `GWGUI.App`

L’application dépend de `GWGUI.Emulation`, mais plus directement d’un module concret. Elle assure la découverte, la validation, le chargement, la fourniture des services autorisés, l’interface commune et le diagnostic.

### `GWGUI.Emulation.<Famille>`

Chaque module contient ses machines, firmwares, médias, entrées, stockages, états sauvegardés, cœurs et commandes internes. Il référence le SDK commun et les bibliothèques techniques nécessaires, mais jamais `GWGUI.App` ni un autre module.

## 4. Point d’entrée obligatoire

Le SDK devra exposer une factory publique et minimale :

```csharp
public interface IEmulationModuleFactory
{
    EmulationModuleMetadata Metadata { get; }
    IEmulationModule Create(IEmulationModuleContext context);
}
```

Chaque assembly expose exactement une implémentation publique de cette interface, instanciable sans paramètre. Son constructeur ne réalise aucun travail lourd.

`EmulationModuleMetadata` fournit au minimum :

- un identifiant stable, unique et insensible à la casse ;
- le nom technique et la version du module ;
- la plage de versions de l’API commune acceptée ;
- la clé de ressource d’affichage ;
- les capacités principales annoncées.

L’identifiant ne change jamais après publication, car il sert aux chemins, configurations et diagnostics.

## 5. Contexte fourni au module

L’application ne transmet pas ses classes internes ni ses chemins globaux. Elle fournit un contrat limité :

```csharp
public interface IEmulationModuleContext
{
    string ModuleDataDirectory { get; }
    string ModuleCacheDirectory { get; }
    HttpClient HttpClient { get; }
    IEmulationLogger Logger { get; }
    Version HostApiVersion { get; }
}
```

Les chemins sont déjà résolus et isolés pour le module. Le cache peut être supprimé sans perte de configuration. Tout nouveau service passe par un contrat commun compatible, jamais par une dépendance vers `GWGUI.App`.

## 6. Points d’accroche fonctionnels

`IEmulationModule` reste le point d’accès principal et doit couvrir les responsabilités suivantes.

### Machines et configurations

- identité et catalogue des machines ;
- capacités de chaque machine ;
- création et changement de machine ;
- description, validation et application des réglages ;
- résumé, chargement, enregistrement et suppression des configurations ;
- migration des anciennes versions appartenant au module.

Une configuration enregistrée porte l’identifiant du module et de la machine. L’application ne désérialise jamais directement un type concret provenant d’un module absent.

### Médias et firmware

- catégories, emplacements, extensions et compatibilités ;
- insertion, remplacement et éjection ;
- médias obligatoires et activité des lecteurs ;
- firmwares attendus, détection, vérification et compatibilité ;
- installation uniquement lorsqu’elle est explicitement autorisée.

### Entrées

- ports et périphériques disponibles ;
- associations et réglages par défaut ;
- conversion des entrées communes vers les codes de la machine ;
- gestion des touches propres à une famille.

`EmulationKey` reste commun. Chaque module ignore les touches qu’il ne prend pas en charge. L’enum reçoit uniquement les touches réellement nécessaires, pas toutes les touches historiques possibles par anticipation.

### Stockage et états sauvegardés

- description et validation des périphériques de stockage ;
- création, chargement et suppression des états ;
- version et contrôle de compatibilité du format propre au module.

### Exécution

- création d’un runtime depuis une configuration commune ;
- démarrage, pause, reprise, arrêt et réinitialisation ;
- sorties vidéo et audio ;
- messages, changements d’état et libération déterministe des ressources.

### Commandes de processus interne

Les commandes servant à héberger un cœur dans un processus séparé restent traitées par le module. L’application charge les modules avant d’acheminer la ligne de commande, puis appelle `TryHandleHostCommand` sur chacun. Une commande appartenant à un module absent retourne une erreur contrôlée, jamais une erreur non gérée de chargement d’assembly.

### Messages et erreurs

Les modules renvoient des codes, catégories, contextes et clés de ressources stables. Ils ne construisent pas de texte utilisateur brut. L’application assure la présentation et la traduction.

## 7. Manifeste

Un fichier `module.json` permet de valider un module avant de charger son code :

```json
{
  "id": "atari",
  "assembly": "gwgui.emulation.atari.dll",
  "moduleVersion": "1.0.0",
  "hostApi": {
    "minimum": "1.0",
    "maximum": "1.x"
  }
}
```

Le chargeur vérifie les champs, l’unicité de l’identifiant, le chemin et la compatibilité. Les chemins absolus, les remontées `..` et les assemblies hors du dossier du module sont refusés. Le manifeste et les métadonnées de la factory doivent correspondre.

## 8. Découverte et chargement

Au démarrage, le registre devra :

1. énumérer les sous-dossiers directs de `Modules` ;
2. lire et valider chaque manifeste ;
3. vérifier la compatibilité de l’API ;
4. charger l’assembly déclaré ;
5. trouver l’unique `IEmulationModuleFactory` publique ;
6. comparer ses métadonnées au manifeste ;
7. créer le contexte isolé puis le module ;
8. enregistrer le module seulement si l’initialisation réussit.

Chaque dossier est traité indépendamment. Toute exception est capturée au niveau du module, journalisée et transformée en diagnostic. Les identifiants dupliqués sont refusés. L’ordre d’affichage vient d’une propriété contractuelle puis de l’identifiant, jamais de l’ordre du système de fichiers.

## 9. Isolation et sécurité

La première version peut utiliser le contexte de chargement par défaut. Les modules deviennent facultatifs, mais ne sont pas remplaçables à chaud. Un `AssemblyLoadContext` collectable ne sera ajouté que si le déchargement sans redémarrage devient nécessaire.

Un module s’exécute avec les droits de GW GUI : le dossier `Modules` n’est pas une frontière de sécurité. Seuls des modules approuvés doivent être installés. La signature, les empreintes et la politique de distribution devront être décidées avant l’acceptation de modules tiers non maîtrisés.

## 10. Compatibilité et versionnement

Trois versions sont distinctes :

- version du produit GW GUI ;
- version de l’API commune ;
- version propre du module.

Une évolution additive conserve la version majeure de l’API. Une rupture de signature ou de comportement contractuel augmente cette version majeure. Un module incompatible est refusé avant la création de sa factory, avec un diagnostic indiquant les versions attendue et disponible.

Les contrats publics n’exposent aucun type provenant de `GWGUI.App`, d’un autre module ou d’une dépendance facultative.

## 11. Ressources et traductions

Le mécanisme exact devra être validé par un prototype : assemblies satellites dans le dossier du module ou catalogue interrogé par une interface commune. Il devra garantir :

- le repli vers la langue neutre ;
- l’absence de collision entre modules ;
- des clés préfixées par l’identifiant du module ;
- le fonctionnement dans toutes les langues prises en charge.

Les ressources Amiga et Atari ne doivent pas être déplacées avant cette décision.

## 12. Build, packaging et installation

`scripts/build.ps1` devra publier séparément l’application et chaque module, puis construire `Modules/<Identifiant>`. Il devra :

- ne pas réintroduire de références directes dans `GWGUI.App` ;
- copier manifeste, assembly et dépendances privées ;
- vérifier identifiants et versions ;
- échouer si un module déclaré est incomplet ;
- permettre un paquet sans certains modules.

L’installateur pourra proposer les modules officiels comme composants. La désinstallation d’un module retire ses binaires, mais jamais ses configurations ou données utilisateur sans action distincte et explicite.

## 13. Diagnostic utilisateur

Une page devra présenter :

- les modules chargés, leur version et leur chemin ;
- leur compatibilité avec l’API ;
- les modules ignorés et la cause exacte ;
- les dépendances manquantes ;
- les doublons et erreurs d’initialisation.

L’absence volontaire d’un module n’est pas une erreur générale. Une configuration appartenant à un module absent reste conservée et apparaît comme indisponible, sans suppression ni modification.

## 14. Tests obligatoires

La mise en œuvre devra tester au minimum :

- absence du dossier `Modules` et dossier vide ;
- Amiga seul, Atari seul, les deux, puis aucun ;
- ajout d’un module minimal indépendant de test ;
- manifeste absent, invalide ou incompatible ;
- DLL corrompue ou dépendance privée absente ;
- factory absente, multiple ou impossible à construire ;
- identifiant dupliqué ;
- exception d’un module sans impact sur les autres ;
- commande interne avec module présent puis absent ;
- configuration appartenant à un module absent ;
- traductions et repli neutre ;
- paquets Debug et Release avec différentes sélections de modules.

Le module minimal de test doit prouver que l’API est générique et ne dépend ni d’Amiga ni d’Atari.

## 15. Ordre de migration

1. Stabiliser les contrats publics de `GWGUI.Emulation`.
2. Ajouter la version d’API, le manifeste, la factory et le contexte.
3. Créer le module minimal de test et le chargeur dynamique.
4. Ajouter les diagnostics et les tests d’échec.
5. Adapter Amiga à la factory sans retirer sa référence statique.
6. Adapter Atari de la même manière.
7. Valider temporairement les deux chemins de chargement.
8. Résoudre et tester les traductions propres aux modules.
9. Retirer de `GWGUI.App` les références et constructions concrètes.
10. Adapter le build, l’installateur et la publication.
11. Tester les paquets sans module, avec un seul et avec tous.
12. Supprimer l’ancien chargement seulement après validation complète.

À chaque étape, le code est adapté ou déplacé avant le retrait de l’ancien raccordement. Aucune donnée utilisateur n’est supprimée automatiquement.

## 16. Guide destiné aux auteurs de modules

Avant d’ouvrir l’API, un guide séparé devra fournir :

- la liste complète des interfaces et responsabilités ;
- la structure minimale du projet et du paquet ;
- un exemple compilable complet ;
- les conventions de noms, identifiants et ressources ;
- les règles de chemins et de persistance ;
- le cycle de vie et la libération des ressources ;
- la création des machines, configurations et runtimes ;
- les médias, firmwares, entrées, stockages et états ;
- les erreurs et diagnostics ;
- le schéma du manifeste et la matrice de versions ;
- les commandes de build, test et installation locale ;
- les restrictions de sécurité et la publication.

Ce guide devra permettre de créer un module sans consulter le code Amiga ou Atari et sans modifier `GWGUI.App`.

## 17. Décisions à confirmer avant réalisation

Les points suivants ne devront pas être inventés pendant l’implémentation :

- schéma définitif et versionné de `module.json` ;
- règle exacte de compatibilité de l’API ;
- mécanisme de ressources et traductions ;
- services finaux de `IEmulationModuleContext` ;
- contexte de chargement par défaut ou dédié ;
- politique de vérification des modules tiers ;
- modules officiels obligatoires ou facultatifs dans l’installateur ;
- présentation des configurations dont le module est absent.

Chaque décision devra être ajoutée à ce document avant la modification correspondante du code.
