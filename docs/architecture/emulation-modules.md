# Modules d'émulation dynamiques

Le contrat de raccordement exhaustif destiné à un auteur qui ne possède pas le code de GW GUI est
décrit dans [`emulation-module-authoring.md`](emulation-module-authoring.md).

## Fonctionnement

`GWGUI.App` ne référence aucun projet `GWGUI.Emulation.<Famille>`. Au démarrage, elle cherche les
paquets dans les sous-dossiers directs de `Modules`, à côté de `gwgui.exe`. Un paquet absent supprime
simplement les machines et onglets de sa famille. L'application fonctionne aussi sans dossier
`Modules` ou avec un dossier vide.

Le chargement est effectué une fois au démarrage : ajouter, remplacer ou retirer une DLL demande de
redémarrer GW GUI. L'échec d'un module est journalisé sans empêcher les autres de fonctionner. Le
retrait d'une DLL ne supprime jamais les configurations ni les données utilisateur.

## Manifeste obligatoire et version d'API

Décision validée : chaque paquet contient `module.json`. Les DLL déposées directement dans
`Modules` sans manifeste ne sont plus chargées ; aucun chemin de compatibilité n'est conservé.
Amiga et Atari sont adaptés ensemble. Les données `Data/Emulation/Machines/<Id>` ne changent pas.

```text
Modules/Amiga/module.json
Modules/Amiga/gwgui.emulation.amiga.dll
Modules/Atari/module.json
Modules/Atari/gwgui.emulation.atari.dll
```

```json
{
  "schemaVersion": 1,
  "id": "amiga",
  "entryAssembly": "gwgui.emulation.amiga.dll",
  "moduleVersion": "1.0.0",
  "hostApiMinimum": "1.0",
  "hostApiMaximum": "1.0"
}
```

Les six champs sont obligatoires. `schemaVersion` identifie le format JSON, actuellement `1`.
`moduleVersion` est un numéro à trois composantes numériques propre au module ; `1.0.0`
correspond à l'identité initiale des projets Amiga et Atari. Il ne versionne ni GW GUI ni le cœur.
Les bornes d'API sont des numéros `majeure.mineure`, inclusifs, comparés numériquement.
L'API actuelle est `1.0` et les deux bornes des modules actuels valent `1.0` : aucun joker `1.x`
ne promet une compatibilité future. Une borne minimale supérieure à la maximale est invalide.

L'identifiant est stable et comparé sans distinction de casse entre manifeste, factory et module.
Il doit être utilisable comme nom de dossier : pas de séparateur, de chemin absolu, de caractère
interdit, de nom réservé Windows, de nom `.`/`..` ni de point ou espace final. Les identifiants
existants `amiga` et `atari` sont conservés, y compris pour les chemins persistants.

`entryAssembly` est un nom de fichier `.dll` directement dans le dossier du paquet. Les chemins
absolus, sous-chemins et remontées sont refusés. Le dossier, le manifeste et la DLL d'entrée ne
doivent pas être des liens redirigeant la lecture hors du paquet. Le chargeur contrôle le manifeste,
les bornes d'API et l'existence de la DLL avant d'instancier la factory. Une DLL d'entrée doit exposer
une seule factory publique concrète, sans paramètre et sans paramètre générique ouvert.

Les doublons et les identités incohérentes sont refusés. Le journal indique le chemin, la version
déclarée et la raison du refus ; un échec n'empêche pas le traitement des autres sous-dossiers.
Les changements d'API seront déclarés et leur compatibilité vérifiée avant d'élargir les bornes.

## Frontière de dépendances

Un module référence `GWGUI.Emulation` et ses bibliothèques techniques. Il ne référence ni
`GWGUI.App`, ni un autre module. `GWGUI.App` référence seulement les contrats de
`GWGUI.Emulation` : aucun type concret ou `ProjectReference` vers une famille n'y est autorisé.

## Entrée obligatoire de la DLL

L'assembly expose une classe `public`, non abstraite, possédant un constructeur public sans
paramètre et implémentant :

```csharp
public interface IEmulationModuleFactory
{
    string Id { get; }
    IEmulationModule Create(EmulationModuleContext context);
}
```

`Id` doit être stable, unique et utilisable comme nom de dossier. `factory.Id` et
`IEmulationModule.Id` doivent être égaux. Le constructeur reste sans travail lourd ;
l'initialisation se fait dans `Create`.

## Entrées fournies par GW GUI

| Entrée | Contenu | Utilisation |
|---|---|---|
| `DataDirectory` | Racine persistante de GW GUI | Seulement lorsqu'une API commune existante exige la racine générale |
| `ModuleDirectory` | `Data/Emulation/Machines/<Id>` | Configurations, cœurs téléchargés et données propres au module |
| `HttpClient` | Client HTTP partagé fourni par l'application | Le module l'utilise mais ne le libère pas |

Le module ne calcule pas le chemin global lui-même et ne dépend d'aucun service interne de
`GWGUI.App`.

## Sorties attendues par GW GUI

`Create` retourne l'unique sortie fonctionnelle, un `IEmulationModule` :

| Sortie | Rôle |
|---|---|
| `Id`, `DisplayResourceKey` | Identité technique et libellé de la famille |
| `Machines` | Catalogue alimentant la sélection des machines |
| `DefaultVisibility` | Onglets et réglages communs visibles par défaut |
| `Describe`, `CreateConfiguration`, `ChangeMachine`, `ApplySettings` | Description et modification neutres des réglages |
| `LoadConfigurationsAsync`, `SaveConfigurationAsync`, `DeleteConfigurationAsync` | Persistance appartenant au module |
| `SummarizeConfiguration`, `RuntimeOptions` | Données neutres présentées ou transmises par App |
| `CreateRuntimeAsync` | Raccordement du runtime aux services vidéo, audio, entrées, messages et médias |
| `TryHandleHostCommand` | Commandes internes d'un cœur hébergé dans un processus séparé |

Tout objet traversant cette frontière appartient à `GWGUI.Emulation`. Une signature publique ne
doit exposer aucun type de `GWGUI.App`, d'un autre module ou d'une dépendance privée.

```text
GWGUI.App
  -> lit Modules/<Famille>/module.json et contrôle l'API
  -> charge la DLL d'entrée déclarée
  -> instancie IEmulationModuleFactory
  -> fournit EmulationModuleContext
  <- reçoit IEmulationModule
  -> fournit EmulationRuntimeServices au lancement d'une machine
  <- reçoit EmulationMachineRuntime et les sorties communes
```

Le choix du cœur, ses options natives et ses types privés restent entièrement internes au module.

## Ajouter un module

1. Créer `src/GWGUI.Emulation.<Famille>` et référencer `GWGUI.Emulation`.
2. Implémenter la factory et `IEmulationModule`, sans référence à `GWGUI.App`.
3. Ajouter un `module.json` conforme et configurer sa copie dans le projet. Ajouter le projet et
   le nom de son assembly à `$modules` dans `scripts/build.ps1` pour l'inclure
   dans le paquet officiel.
4. Construire : le script publie le projet séparément et copie sa DLL et son manifeste dans le
   sous-dossier du module.
5. Vérifier le paquet avec ce module, avec les autres, puis avec sa DLL retirée.

Un dossier contenant une DLL compatible et son manifeste peut aussi être déposé manuellement
dans `Modules` sans recompiler App, si ses dépendances sont déjà distribuées avec GW GUI.

## Erreurs à isoler

- dossier absent ou vide : démarrage normal, aucune famille proposée ;
- DLL invalide, factory défectueuse ou dépendance manquante : journalisation, autres modules chargés ;
- manifeste absent, invalide ou incompatible : paquet refusé avant initialisation ;
- identifiant invalide, incohérent ou dupliqué : module refusé ;
- module retiré : données conservées, aucun type concret du module désérialisé par App.

Les modules sont du code approuvé exécuté avec les droits de GW GUI. Ce mécanisme découple les
familles mais ne constitue pas une frontière de sécurité et ne permet pas le déchargement à chaud.
