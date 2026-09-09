# Développer un module d'émulation sans connaître GW GUI

## Objet de ce guide

Ce document décrit le contrat complet entre GW GUI et une bibliothèque d'émulation. Un auteur de
module ne doit pas avoir besoin de lire `GWGUI.App`, Amiga ou Atari. Il doit seulement disposer de
la bibliothèque de contrats `gwgui.emulation.dll`, de ce document et d'un projet .NET compatible.

État actuel important : le chargement dynamique, les paquets avec dépendances privées et les mises
à jour après redémarrage fonctionnent. `GWGUI.Emulation` n'est pas encore publié comme paquet SDK
autonome. Un module développé hors du dépôt doit donc temporairement référencer la DLL issue du
même build de GW GUI. L'API actuelle est `1.0` et chaque module doit déclarer ses bornes compatibles
dans `module.json`. Les traductions propres au module passent par `IEmulationModuleLocalization`.

## 1. Ce que contient un module

Un module représente une famille, par exemple Commodore. Il peut contenir plusieurs machines et
utiliser plusieurs cœurs. Le choix du cœur appartient au module ; GW GUI ne connaît que les contrats
communs.

Le projet produit une DLL ayant les dépendances suivantes :

```text
MonModule
  -> gwgui.emulation.dll       contrat obligatoire
  -> bibliothèques du module   facultatives
  -X-> gwgui.app.dll           interdit
  -X-> autre module            interdit
```

Le module ne fournit pas de fenêtre, de `UserControl`, de XAML ou de type WPF. Il décrit les
machines, les champs et le runtime ; GW GUI construit toute l'interface.

## 2. Projet minimal

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>gwgui.emulation.commodore</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="gwgui.emulation">
      <HintPath>sdk\gwgui.emulation.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <None Update="module.json" CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
```

La DLL finale et `module.json` sont déposés dans `Modules/Commodore`. GW GUI contrôle le manifeste
au prochain démarrage, avant d'initialiser la factory. Une DLL seule directement dans `Modules`
n'est pas chargée.

Exemple de manifeste pour un module construit et validé avec l'API actuelle :

```json
{
  "schemaVersion": 1,
  "id": "Commodore",
  "entryAssembly": "gwgui.emulation.commodore.dll",
  "moduleVersion": "1.0.0",
  "hostApiMinimum": "1.0",
  "hostApiMaximum": "1.0"
}
```

La version du module contient trois composantes numériques. Les versions d'API contiennent
deux composantes et les bornes sont inclusives. Un joker comme `1.x` n'est pas accepté :
la compatibilité future n'est pas supposée. Une modification des contrats doit être accompagnée
d'une évaluation de la version d'API ; n'élargir les bornes d'un module qu'après vérification.
Une correction interne au produit ou au moteur n'impose pas à elle seule de changer l'API.

L'identifiant doit être un nom de dossier valide et stable. `entryAssembly` est un simple nom
de DLL dans ce dossier : aucun chemin absolu, sous-chemin ou lien externe. Les trois identités
(manifeste, factory, module) doivent correspondre sans distinction de casse. Le
[schéma et les règles de chargement](emulation-modules.md) précisent les validations.

Pour un essai local, fermer GW GUI, déposer le dossier complet dans `Modules`, puis relancer.
Les modules Amiga et Atari sont produits automatiquement avec leur manifeste par
`scripts/build.ps1 -Configuration Debug`. Les succès sont inscrits dans `Data/Logs/information-*.log`
et les refus dans `Data/Logs/errors-*.log`. Les configurations et cœurs téléchargés restent dans
les dossiers de données existants.

## 3. Factory : point d'entrée obligatoire

Une seule classe publique, non abstraite, sans paramètre générique ouvert, avec constructeur
public sans paramètre implémente :

```csharp
public interface IEmulationModuleFactory
{
    string Id { get; }
    IEmulationModule Create(EmulationModuleContext context);
}
```

- `Id` est stable, unique, non vide et ne contient aucun séparateur de chemin ;
- le constructeur ne charge ni fichier, ni cœur, ni réseau ;
- `Create` construit le module ;
- `IEmulationModule.Id` doit être identique à `IEmulationModuleFactory.Id`, sans tenir compte de la casse ;
- toute exception refuse uniquement ce module et est inscrite dans le journal de GW GUI.

Exemple :

```csharp
public sealed class CommodoreModuleFactory : IEmulationModuleFactory
{
    public string Id => "Commodore";

    public IEmulationModule Create(EmulationModuleContext context) =>
        new CommodoreModule(context);
}
```

## 4. Entrées reçues à la création

```csharp
public sealed record EmulationModuleContext(
    string DataDirectory,
    string ModuleDirectory,
    HttpClient HttpClient);
```

| Valeur | Garantie et responsabilité |
|---|---|
| `DataDirectory` | Racine de données générale déjà résolue. Ne pas y inventer de nouveaux dossiers globaux. |
| `ModuleDirectory` | Racine persistante et isolée du module : `Data/Emulation/Machines/<Id>`. Le module crée dessous `Configurations`, `Core`, caches et fichiers propres. |
| `HttpClient` | Instance partagée pour télécharger cœurs ou métadonnées. Ne jamais appeler `Dispose` dessus. |

La factory et le module ne conservent aucun type provenant de `GWGUI.App`.

## 5. Sortie principale : `IEmulationModule`

```csharp
public interface IEmulationModule
{
    string Id { get; }
    string DisplayResourceKey { get; }
    IReadOnlyList<EmulationMachineDefinition> Machines { get; }
    EmulationSettingsVisibility DefaultVisibility { get; }
    bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode);

    EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null);
    IEmulationConfiguration CreateConfiguration(string machineId);
    IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId);
    IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration,
        IReadOnlyDictionary<string, string?> values);
    IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration);
    EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration);
    ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration,
        EmulationRuntimeServices services, CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(
        CancellationToken cancellationToken = default);
    ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default);
    ValueTask DeleteConfigurationAsync(Guid configurationId,
        CancellationToken cancellationToken = default);
}
```

### Identité et catalogue

- `Id` identifie la famille et doit rester compatible avec les configurations déjà enregistrées.
- `DisplayResourceKey` désigne son libellé traduit. La recherche consulte d'abord le catalogue
  du module, puis les ressources de GW GUI en repli.
- `Machines` contient des `EmulationMachineDefinition(Id, DisplayResourceKey)`. Les identifiants
  machine sont uniques dans le module et stables dans le temps.
- `DefaultVisibility` annonce les onglets, blocs et champs généralement visibles.

### Configuration

Le type concret de configuration reste privé au module mais implémente :

```csharp
public interface IEmulationConfiguration
{
    string ModuleId { get; }
    Guid Id { get; }
    string MachineId { get; }
}
```

- `CreateConfiguration` retourne une configuration complète avec valeurs par défaut et nouvel `Id`.
- `ChangeMachine` conserve l'identité de la configuration et adapte les valeurs compatibles.
- `Describe` transforme la configuration en description neutre de l'interface.
- `ApplySettings` reçoit toutes les valeurs modifiées indexées par l'identifiant du champ et retourne
  la nouvelle configuration. Le module valide les valeurs et ne modifie pas silencieusement le fichier.
- `SaveConfigurationAsync` assure la persistance atomique dans `ModuleDirectory`.
- `LoadConfigurationsAsync` ignore ou récupère proprement les fichiers endommagés selon la politique
  du module ; il ne doit pas empêcher l'application de démarrer.
- `DeleteConfigurationAsync` supprime uniquement la configuration demandée.
- `SummarizeConfiguration` produit le nom de machine et les lignes de résumé affichées dans les listes.
- `RuntimeOptions` retourne les options natives destinées au cœur pour cette configuration.

`ModuleId`, `MachineId`, les identifiants de champs, de blocs, de contrôleurs et de périphériques sont
des identifiants persistants : les renommer nécessite une migration dans le module.

## 5.1 Interfaces facultatives du module

GW GUI détecte aussi quatre capacités sur l'objet retourné. Elles sont facultatives : ne pas
implémenter une interface masque la fonction correspondante.

```csharp
public interface IEmulationEmulatorManager
{
    ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId,
        CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId,
        CancellationToken cancellationToken = default);
    ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}
```

Cette capacité active la détection, la liste des versions et l'installation d'un cœur. Le chemin
retourné est celui de l'installation terminée et la progression va de 0 à 1.

```csharp
public interface IEmulationFirmwareManager
{
    string GetFirmwareDirectory(string machineId);
    ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId,
        IEmulationConfiguration configuration, CancellationToken cancellationToken = default);
    IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration,
        EmulationFirmwareCandidate firmware);
}
```

Cette capacité active les ROM/firmwares. Chaque candidat est classé comme officiel, compatible,
partiellement compatible, incompatible ou inconnu. `UseFirmware` retourne une configuration mise à
jour sans l'enregistrer implicitement.

```csharp
public interface IEmulationInputSettingsManager
{
    EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration);
    IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration,
        EmulationInputSettings settings);
    ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default);
}
```

Cette capacité active les panneaux clavier, souris et manettes. Elle décrit les associations, ports,
périphériques émulés et périphériques physiques sélectionnés.

```csharp
public interface IEmulationStorageSettingsManager
{
    EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration);
    IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration,
        EmulationStorageSettings settings);
}
```

Cette capacité active l'ajout, la suppression et la configuration des lecteurs. Elle distingue les
périphériques disponibles, emplacements configurés, médias montés et réglages propres aux lecteurs.

## 6. Décrire l'interface graphique

GW GUI possède et dessine l'interface. Le module fournit seulement des données :

```text
EmulationMachineSettings
  -> Visibility : onglets/blocs/champs visibles
  -> Blocks[]
       -> Id, Tab, titre, icône, colonnes
       -> Fields[]
            -> Id, libellé, type d'éditeur, valeur, choix, règles de rafraîchissement
  -> Rules[] : relations entre champs
```

Les onglets disponibles sont `General`, `Cpu`, `Ram`, `Rom`, `Video`, `Audio`, `Storage`,
`Keyboard`, `Mouse` et `Controllers`.

Les éditeurs disponibles sont `Selection`, `Toggle`, `Text`, `Path`, `DirectoryPath`, `Number`,
`Percentage` et `Information`. Un module ne peut pas envoyer arbitrairement un nouveau bloc WPF.
Si un besoin ne peut pas être décrit avec ces contrats, il faut enrichir le SDK commun de manière
générique avant de l'utiliser.

Pour chaque `EmulationSettingsField` :

- `Id` relie l'affichage à la valeur reçue par `ApplySettings` ;
- `Tab` et `BlockId` déterminent l'emplacement ;
- `LabelResourceKey`, `ExplanationResourceKey` et `DetailedExplanationResourceKey` désignent les textes ;
- `Value` est la valeur sérialisée échangée avec App ;
- `Choices` décrit les valeurs autorisées d'une sélection ;
- `IsEnabled` et `IsVisible` contrôlent l'état courant ;+- `RefreshSettingsOnChange` demande de rappeler `Describe` immédiatement après modification ;
- `RequiresRestart` indique que le runtime courant ne peut pas appliquer la valeur à chaud ;
- `DefaultFolderCategory` choisit le dernier dossier indépendant pour un sélecteur de chemin ;
- `ChoiceSource.AudioOutputDevices` demande à App de fournir la liste des sorties audio.

Les règles communes sont `MutuallyExclusive` et `VisibleWhenSourceDiffers`. Toute logique plus riche
est recalculée par `Describe` après un changement marqué `RefreshSettingsOnChange`.

## 7. Stockage et médias

`EmulationMachineRuntime.MediaDevices` décrit les emplacements réellement disponibles. Chaque
`EmulationMediaDevice` fournit : emplacement, type, extensions, caractère amovible, nécessité de
recréer la machine, options de lecteur de disquette, interfaces et formats de disque dur.

Les catégories sont disquette, disque dur, CD, cartouche et cassette. Un emplacement est un couple
`EmulationMediaSlot(Category, Index)` ; les index commencent à zéro (`D0`, `HD0`, `CD0`, `CART0`,
`TAPE0`). Plusieurs emplacements utilisent les index suivants.

Un média monté est décrit par son chemin, son emplacement, son type, la lecture seule et l'état
d'insertion. Le runtime implémente `IEmulationMedia` :

- `InsertAsync` insère ou remplace un média ;
- `EjectAsync` éjecte l'emplacement ;
- `SelectDiskAsync` choisit une image dans un jeu multidisque ;
- `MountedMedia` reflète immédiatement l'état effectif.

`RequiresMachineRecreation` doit être vrai lorsque le cœur ne sait pas appliquer l'opération à chaud.
`PrepareMediaAsync`, facultatif, convertit ou prépare un média avant la création de la machine.

## 8. Création et cycle de vie du runtime

GW GUI appelle `CreateRuntimeAsync` avec :

```csharp
public sealed record EmulationRuntimeServices(
    string SessionsDirectory,
    string StatesDirectory,
    string ConvertedMediaDirectory,
    string HostExecutablePath,
    Func<string?, int, IAudioOutput> CreateAudioOutput);
```

Ces chemins sont propres à l'exécution. `HostExecutablePath` permet de relancer `gwgui.exe` pour un
cœur isolé ; la ligne de commande correspondante doit être reconnue par `TryHandleHostCommand`.
La factory audio crée la sortie demandée avec la latence souhaitée.

Le résultat `EmulationMachineRuntime` contient la configuration finale, les périphériques, les médias
montés, le libellé, la capture du pointeur, l'éventuelle préparation des médias et une factory
`CreateMachine`. GW GUI appelle cette factory avec la liste effective des médias.

L'`IEmulatedMachine` retournée expose :

| Interface | Responsabilité |
|---|---|
| `IEmulationLifecycle` | Start, pause, reprise, reset logiciel/matériel et arrêt asynchrones |
| `IEmulationInput` | Instantanés clavier/souris/manettes et sélection des périphériques |
| `IEmulationMedia` | Médias montés et opérations à chaud |
| `IEmulationVideo` | Dernière trame, FPS et événement `FrameReady` |
| `IEmulationAudio` | Dernier bloc, volume, mute, sortie et événement `ChunkReady` |
| `IEmulationSavedStates` | Support, sauvegarde et chargement d'états |
| `IEmulationRuntime` | Identité/version du cœur, extensions, options à chaud et activité des lecteurs |
| `IEmulationCassetteTransport` | Facultatif : état et commandes du magnétophone |

La machine commence dans l'état `Created`, publie des transitions cohérentes, rend toutes les
opérations annulables et libère processus, DLL natives, flux et tâches dans `DisposeAsync`.

## 9. Vidéo, audio et entrées

Une `VideoFrame` fournit les pixels, largeur, hauteur, pitch, format (`Rgb565`, `Xrgb8888` ou
`Rgb1555`), rapport d'image, séquence et horodatage. Le tampon doit rester valide pendant le
traitement de l'événement. Toute variation de géométrie doit apparaître dès la trame concernée.

Un `AudioChunk` contient des échantillons `short` stéréo entrelacés. Sa longueur vaut
`FrameCount * 2`. Le module publie la fréquence, respecte mute/volume et écrit dans la sortie créée
par GW GUI.

`SetInput` reçoit à chaque mise à jour un `EmulationInputSnapshot` contenant :

- l'ensemble des `EmulationKey` pressées ;
- les deltas, boutons et molettes du pointeur ;
- les états et contrôles nommés des manettes.

Le module traduit cet instantané vers le cœur. Les associations configurables sont décrites par
`EmulationInputSettings`, `EmulationInputBindingSet`, `EmulationControllerPort` et
`EmulationControllerChoice`. Une touche ou un contrôle inconnu est ignoré sans erreur.

## 10. Activité, options, cassette et états

- `IEmulationRuntime.MediaActivity` associe chaque emplacement à son activité instantanée ; GW GUI
  s'en sert pour les LED.
- `AvailableOptions` décrit les options natives réellement modifiables ; `SetOptionAsync` applique
  une valeur et signale proprement les refus.
- `IEmulationCassetteTransport.AvailableCommands` annonce uniquement les commandes gérées et `State`
  reflète `Empty`, `Stopped`, `Playing`, `Paused`, `Recording` ou `EndOfTape`.
- `IEmulationSavedStates.IsSupported` masque les commandes lorsque le cœur ne sait pas sauvegarder.
  Les erreurs de version ou de format sont remontées comme messages d'émulation contrôlés.

## 11. Messages et erreurs

Les erreurs fonctionnelles utilisent `EmulationMessage` ou `EmulationMessageException` avec une
catégorie, un code, une gravité, une cible et un contexte neutre. `OriginalText` conserve le message
du cœur pour le diagnostic. Le module ne montre jamais directement une boîte de dialogue et ne
manipule jamais l'interface.

Les exceptions inattendues peuvent remonter : GW GUI les journalise à la frontière du module. Une
erreur attendue de média, firmware, état ou option doit utiliser le contrat de message correspondant.

## 12. Ordre des appels

```text
démarrage de GW GUI
  factory constructeur -> factory.Create(context)
  module.Machines / DefaultVisibility
  module.LoadConfigurationsAsync()

édition
  module.Describe(machine, configuration)
  utilisateur change des valeurs
  module.ApplySettings(configuration, valeurs)
  éventuellement module.Describe(...) à nouveau
  module.SaveConfigurationAsync(configuration)

lancement
  module.CreateRuntimeAsync(configuration, services)
  runtime.CreateMachine(médias)
  machine.Lifecycle.StartAsync()
  échanges FrameReady / ChunkReady / SetInput / médias / options
  machine.Lifecycle.StopAsync()
  machine.DisposeAsync()
```

Chaque méthode valide que la configuration reçue appartient au bon `ModuleId` et que son
`MachineId` est connu.

## 13. Vérifications minimales d'un nouveau module

- GW GUI démarre avec sa DLL seule, avec les autres DLL et après son retrait ;
- catalogue et création de chaque machine ;
- sauvegarde, rechargement, changement et suppression de configuration ;
- tous les types d'éditeurs et règles déclarés ;
- lancement/arrêt répétés sans processus, fichier ou DLL native restant verrouillé ;
- changement de résolution, son, clavier, souris et manettes ;
- insertion, remplacement et éjection de chaque média à froid et à chaud ;
- LED d'activité et transport cassette lorsqu'ils existent ;
- sauvegarde d'état ou masquage correct si non supportée ;
- cœur absent, firmware absent, média invalide et configuration endommagée ;
- annulation et fermeture pendant une opération asynchrone.

## 14. Fonctions réalisées et limites pour un module externe

Les fonctions suivantes sont réalisées pour les modules officiels :

- manifeste obligatoire et contrôle des versions d'API avant chargement ;
- sous-dossier autonome avec résolution prioritaire de ses dépendances privées ;
- archive indépendante créée par `scripts/package-module.ps1` avec empreinte SHA-256 ;
- catalogue de versions, recherche sélective et installation par l'updater après fermeture ;
- traductions embarquées décrites à la section suivante.

L'écosystème externe reste différé sur les points suivants :

- pas de paquet NuGet `GWGUI.Emulation.SDK` versionné ;
- aucune garantie publiée de compatibilité pour un projet tiers séparé ;
- aucun déchargement à chaud : ajouter, retirer ou mettre à jour un module demande un redémarrage.

Ces limites n'empêchent pas les modules officiels actuels. Le SDK et sa politique de compatibilité
doivent être établis avant de promettre qu'un projet tiers restera compatible avec plusieurs
versions de GW GUI.

## 15. Traductions embarquées

Le module peut implémenter cette capacité facultative :

```csharp
public interface IEmulationModuleLocalization
{
    bool TryGetString(string key, CultureInfo culture, out string value);
}
```

Retourner `false` pour une clé absente et `true` pour une valeur présente, même vide.
`EmulationModuleLocalization` fournit le lecteur commun des catalogues RESX compilés et
embarqués dans la DLL principale. Amiga et Atari l'utilisent avec les 29 cultures distribuées
et une base `00-Base`. Aucun fichier de traduction externe n'est nécessaire pour ces modules.

La recherche reçoit le module concerné : culture demandée, parents de culture, `en-US`, base
du module, puis résolution App existante. La recherche invariante utilise uniquement les bases.
Les valeurs invariantes (CPU, formats, noms de machines…) restent dans leur base sans copie
par langue. Les textes communs de l'hôte restent dans App.

Les bindings suivent le changement de culture existant. Pour compléter les traductions,
utiliser `scripts/translate-resx-argos.py --root src/GWGUI.Emulation.<Famille>/Resources` avec
les options existantes d'Argos. Le [raccordement détaillé](emulation-module-localization.md)
décrit les consommateurs et les noms des ressources.
