# Architecture modulaire de l’émulation

## Règle impérative de progression du travail

Le présent document contient les décisions nécessaires à la réorganisation. Tant qu'une décision déjà écrite permet de déduire correctement l'implémentation, le travail continue sans demander une nouvelle confirmation et sans interrompre la refonte pour des détails non bloquants.

Si une information indispensable à la poursuite du travail n'est pas définie dans ce document, si plusieurs solutions réellement différentes modifieraient le comportement attendu, ou si l'application d'une règle risquerait de casser une fonctionnalité sans qu'une réponse puisse être trouvée dans le code existant, le travail s'arrête précisément sur ce point. Une seule question claire est alors posée à l'utilisateur, avec uniquement les informations nécessaires pour comprendre la décision.

Après la réponse de l'utilisateur, la décision est immédiatement ajoutée à ce document. Le travail reprend ensuite sans nouvelle interruption jusqu'au prochain blocage réel. Une question non indispensable, une préférence de mise en œuvre déductible des règles existantes ou un détail pouvant être résolu par l'examen du code ne constitue pas un blocage.

Cette autonomie concerne uniquement les opérations mécaniques dont le résultat est déjà imposé sans ambiguïté par le document et par le fonctionnement vérifié du code. Elle n'autorise pas à inventer un comportement, une règle, un contrat, une abstraction, un nom important, un découpage architectural ou une généralisation qui n'a pas été accepté.

Lorsqu'une modification nécessite un choix nouveau, même si plusieurs solutions sont techniquement faisables, il faut de préférence demander la décision à l'utilisateur plutôt que d'extrapoler son intention. La question doit être posée avant d'implémenter ce choix. Après la réponse, la décision est ajoutée au document avant la reprise du travail.

Toute décision architecturale ou fonctionnelle prise pendant la refonte doit être inscrite dans ce document, y compris lorsqu'elle a été explicitement demandée pendant l'exécution ou qu'elle confirme un fonctionnement déjà présent dans le code. Le document doit rester la référence complète permettant de comprendre pourquoi la solution a été retenue et d'éviter qu'une implémentation ultérieure adopte une règle différente.

## Règle impérative d'examen comparatif du code existant

Avant de créer, modifier, déplacer, découper ou renommer un fichier lié à l'émulation, il faut examiner tous les fichiers concernés et suivre leur fonctionnement réel. Cet examen comprend obligatoirement les implémentations équivalentes existant dans App, `GWGUI.Emulation.Amiga`, `GWGUI.Emulation.Atari` et, lorsqu'elles existent, les autres bibliothèques `GWGUI.Emulation.xxx`. Il ne faut jamais concevoir la partie Atari sans vérifier son équivalent Amiga, ni l'inverse.

Le but est d'éviter les implémentations répétées, les architectures parallèles incompatibles, les fichiers inutiles et les noms différents pour une même responsabilité. Par exemple, deux fichiers tels que `AmigaVideoOutputMachine.cs` et `AtariVideoOptions.cs` ne doivent pas être conservés comme deux concepts distincts s'ils remplissent réellement la même fonction. Il faut d'abord déterminer ce qui peut devenir commun. Si une implémentation spécialisée reste nécessaire dans chaque bibliothèque, elle doit suivre une structure, une responsabilité et un nom cohérents dans toutes les bibliothèques concernées.

Le découpage d'une bibliothèque ne doit pas être reproduit aveuglément s'il est inutilement complexe. Si une même fonction est répartie dans de nombreux fichiers Atari mais possède une organisation Amiga plus simple et plus propre, il faut comparer le contenu et retenir une base commune cohérente, sans conserver quarante-cinq fichiers uniquement parce qu'ils existent déjà. L'inverse s'applique également : la meilleure organisation existante sert de référence après vérification qu'elle couvre bien les besoins des autres implémentations.

Les bibliothèques doivent autant que possible employer les mêmes catégories de fichiers, les mêmes noms de responsabilités, les mêmes contrats communs et la même organisation générale. Le code interne permettant de réaliser une opération peut différer selon l'émulateur, mais la fonction représentée, son emplacement architectural et son nom ne doivent pas varier arbitrairement.

Avant d'ajouter un nouveau fichier ou une nouvelle abstraction, il faut donc vérifier successivement :

1. si la même fonction existe déjà ailleurs ;
2. si elle peut être placée dans le code commun ;
3. si une spécialisation par bibliothèque est réellement nécessaire ;
4. quel nom et quel découpage sont déjà les plus clairs ;
5. si la solution retenue pourra être appliquée de façon cohérente à toutes les bibliothèques concernées sans duplication graphique ou fonctionnelle inutile.

## Règle impérative concernant les textes

**Jamais de texte brut destiné à l'utilisateur dans le code.** Tous les textes affichables sont identifiés et manipulés au moyen de constantes stables permettant de les relier au système de traduction. Cette règle concerne tous les mots, libellés, titres, descriptions, messages, avertissements, erreurs, boutons, choix de sélecteurs et phrases susceptibles d'apparaître dans l'interface.

Chaque constante de texte doit posséder une valeur traduite dans toutes les langues prises en charge. Les bibliothèques `GWGUI.Emulation.xxx` transmettent les identifiants et les données nécessaires ; elles ne construisent pas elles-mêmes les phrases finales et ne renvoient pas de texte utilisateur déjà formaté.

Les termes génériques invariants, par exemple `CPU`, `RAM` ou `ROM`, restent eux aussi identifiés par une clé de ressource et passent par le même système de localisation que les autres textes. Leur clé et leur valeur sont définies uniquement dans le catalogue neutre. Elles sont volontairement absentes des catalogues propres aux langues : le mécanisme normal de repli des ressources retourne alors la valeur neutre inchangée, quelle que soit la langue active. Il ne faut ni recopier ces invariants dans chaque fichier de langue, ni les écrire directement dans le code de l'interface.

## Règle impérative concernant les noms du code

Les identifiants créés par GW GUI ne doivent pas employer `Kind`, `...Kind`, `GetKind` ou une autre variante construite autour de ce mot. Cette règle s'applique aux méthodes, fonctions, classes, interfaces, records, propriétés, champs, constantes, enums et autres éléments déclarés dans App, `GWGUI.Emulation` et toutes les bibliothèques `GWGUI.Emulation.xxx`.

Chaque nom doit employer un terme simple, concret, techniquement exact et facilement compréhensible indépendamment de la langue du développeur, par exemple `Category`, `Target`, `Code`, `Format`, `Model`, `Family`, `Device`, `Media` ou un autre mot décrivant précisément la donnée. Il ne faut pas remplacer mécaniquement tous les usages de `Kind` par un même mot : le remplacement est choisi suivant le rôle réel de chaque élément.

Cette règle ne concerne pas les identifiants imposés par le langage, le framework ou une bibliothèque externe, par exemple `LayoutKind` ou `UriKind`. GW GUI peut appeler ces API avec leur nom officiel, mais ne doit pas reprendre cette convention pour nommer ses propres éléments.

## 1. Objet

Ce document fixe la séparation entre `GWGUI.App`, `GWGUI.Emulation` et les bibliothèques `GWGUI.Emulation.xxx`.

L’interface graphique doit être commune. Chaque bibliothèque fournit les données et règles propres à ses machines. La réorganisation doit préserver toutes les fonctionnalités, valeurs enregistrées et comportements existants.

## 2. Architecture retenue

Les bibliothèques prises en charge restent référencées à la compilation par `GWGUI.App`. Aucune découverte automatique de DLL externes n’est nécessaire.

Actuellement, `GWGUI.App.csproj` référence notamment :

- `GWGUI.Emulation` ;
- `GWGUI.Emulation.Amiga` ;
- `GWGUI.Emulation.Atari`.

Une future bibliothèque `GWGUI.Emulation.Sega` sera ajoutée de la même manière. Cette solution explicite évite une infrastructure inutile de découverte, de versionnement et de gestion des DLL.

## 3. Les deux raccordements dans App

Une bibliothèque intervient à deux endroits parce que l’application possède deux écrans différents.

### 3.1 Options d’émulation

`OptionsEmulationSection` représente **Options → Émulation**. Une instance de ce contrôle est créée avec la fenêtre Options, puis elle construit actuellement les onglets Amiga et Atari.

Cet emplacement est correct. Pour ajouter Sega avec l’architecture actuelle, il faut y raccorder l’onglet Sega. Le rendu reste commun dans App et reçoit les données Sega. Il ne faut pas copier une page graphique `Sega...` dans App.

Chaque famille possède sa propre instance d’onglet. La construction graphique est commune, mais chaque onglet crée ses propres instances de boutons, sélecteurs, champs, listes et autres contrôles. Aucun objet graphique n’est partagé entre les onglets Amiga, Atari ou une future famille.

Les contrôles de chaque instance sont remplis avec les données de la bibliothèque correspondante : `GWGUI.Emulation.Amiga` pour l’onglet Amiga, `GWGUI.Emulation.Atari` pour l’onglet Atari, puis la bibliothèque concernée pour toute nouvelle famille.

Ces instances restent en mémoire pendant la durée de vie de la fenêtre Options. Lorsqu’un utilisateur passe d’un onglet de famille à un autre puis revient au premier, les contrôles de ce premier onglet conservent donc leur état courant. Seuls le code de construction, la disposition et les comportements graphiques sont communs ; les objets créés et les données qu’ils contiennent sont indépendants.

L’onglet **Configurations** reste commun à toutes les familles. Il rassemble dans une même liste les configurations enregistrées Amiga, Atari et celles de toute future bibliothèque raccordée, comme Sega. Il ne doit pas être dupliqué dans chaque onglet de famille.

### 3.2 Exécution des machines

`EmulationSection` représente l’onglet **Émulation** de la fenêtre principale. Il charge les configurations et ouvre la machine demandée.

Il dirige une configuration Amiga vers `AmigaEngine` et une configuration Atari vers `AtariEngine`. Chaque bibliothèque possède un seul Engine de famille, chargé uniquement de créer ou recréer les instances de machines :

- `AmigaEngine` et `AmigaMachine` sont dans `src/GWGUI.Emulation.Amiga` ;
- `AtariEngine` et `AtariMachine` sont dans `src/GWGUI.Emulation.Atari`.

Pour Sega, `SegaEngine` et `SegaMachine` seront créés dans `src/GWGUI.Emulation.Sega`, puis raccordés à `EmulationSection`.

Une nouvelle instance de machine est créée pour chaque onglet d’exécution. Chaque machine conserve ainsi ses propres états, médias et réglages d’exécution. L'Engine de famille n'est pas dupliqué par onglet.

## 4. Responsabilité de GWGUI.Emulation

`GWGUI.Emulation` contient les contrats et comportements réellement communs. Il ne contient pas les règles propres à Amiga, Atari ou une autre famille.

`GWGUI.Emulation` est une dépendance commune obligatoire de l'application et de toutes les bibliothèques `GWGUI.Emulation.xxx`. `GWGUI.App`, `GWGUI.Emulation.Amiga`, `GWGUI.Emulation.Atari` et les futures bibliothèques utilisent directement les interfaces, enums et contrats définis dans ce projet. Il ne sert pas de passerelle d'exécution : App appelle la bibliothèque spécialisée ciblée, en utilisant les types communs pour lui transmettre les données et recevoir son résultat.

Les appels passent directement entre `GWGUI.App` et la bibliothèque `GWGUI.Emulation.xxx` ciblée. `GWGUI.Emulation` n'intercepte pas et ne retransmet pas ces appels. Chaque bibliothèque spécialisée traduit elle-même les identifiants, choix et valeurs communs vers les paramètres exacts attendus par son ou ses émulateurs. Elle effectue également la conversion inverse avant de renvoyer à App un résultat exprimé avec les contrats communs.

La DLL produite par ce projet doit donc être distribuée avec l'application. Si elle manque, App et les bibliothèques spécialisées ne peuvent pas charger, puisqu'il leur manque les définitions communes contre lesquelles elles ont été compilées.

Les contrats communs décrivent notamment :

- les machines ;
- les sous-onglets ;
- les blocs et options ;
- les valeurs, choix et valeurs par défaut ;
- la visibilité et l’état des éléments ;
- les capacités de stockage et d’entrée ;
- les actions prises en charge.

Ils ne contiennent aucun objet WPF.

## 5. Responsabilité de GWGUI.Emulation.xxx

Chaque bibliothèque spécialisée contient ses moteurs, machines, configurations, capacités et règles propres.

Chaque bibliothèque spécialisée expose à App une API publique organisée conforme aux contrats communs. Il ne s'agit pas nécessairement d'un objet principal instancié et partagé. App connaît ainsi les fonctions publiques du module Atari, Amiga ou d'une autre famille, mais ne connaît pas les différents émulateurs gérés à l'intérieur de ce module. Lorsqu'une famille utilise plusieurs émulateurs, la bibliothèque sélectionne en interne celui qui correspond à la machine et à la configuration demandées.

Cette API publique ne doit pas être regroupée dans un fichier fourre-tout. Ses responsabilités sont réparties entre des services internes distincts, rangés dans les fichiers appropriés, notamment le catalogue des machines, la description des options, l'analyse des ROM et la création des machines. L'organisation interne de la bibliothèque reste invisible pour App.

L'API publique de la bibliothèque n'est pas un routeur des commandes d'exécution. Chaque bibliothèque possède un seul Engine de famille, par exemple `AtariEngine` ou `AmigaEngine`. Lors de l'ouverture d'une configuration enregistrée, cet Engine choisit la Factory correspondant à l'émulateur demandé et lui fait créer la nouvelle instance de machine destinée à l'onglet.

Les Factories sont internes à la bibliothèque et sont séparées par émulateur, avec des noms décrivant cet émulateur, par exemple `HatariMachineFactory`, `Atari800MachineFactory` ou `PuaeMachineFactory`. Ajouter un autre émulateur à une famille consiste à ajouter sa Factory et à permettre à l'Engine de famille de la sélectionner.

Après la création, toutes les actions propres à la machine sont envoyées directement à son instance : souris, clavier, manette, alimentation, réinitialisation, insertion, éjection ou remplacement d'une disquette, d'un CD, d'une cartouche et de tout autre support pris en charge. Plus aucune commande d'exécution ne passe par l'Engine. Celui-ci est rappelé uniquement lorsqu'App doit créer ou recréer une instance.

App conserve séparément, pour chaque onglet de machine, l'état courant des supports : chemins des images de disquettes, CD, disques durs et autres fichiers montés, emplacement de chaque support et état inséré ou éjecté. Une insertion, une éjection ou un remplacement est envoyé directement à l'instance, puis App met à jour l'état de l'onglet si l'opération réussit. Lorsqu'une instance est détruite puis recréée, App réinjecte cet état dans la configuration de création.

Cet état reste au minimum en mémoire pendant toute la durée de vie de l'onglet. Si les onglets doivent être restaurés après la fermeture complète de GW GUI, App peut également l'enregistrer dans un fichier d'état de session et le relire au démarrage, sans modifier obligatoirement la configuration enregistrée de la machine.

Les derniers dossiers utilisés par les boîtes de sélection de fichiers appartiennent également à App. Ils sont mémorisés par machine et par type de support afin que chaque machine retrouve son dossier de disquettes, CD, disques durs ou autres supports. Ils ne sont conservés ni par l'Engine ni par l'instance émulée.

### 5.1 Gestion et installation des émulateurs

Chaque bibliothèque `GWGUI.Emulation.xxx` gère entièrement les émulateurs qu'elle prend en charge. Cette responsabilité comprend la recherche des versions disponibles, le téléchargement, la vérification du fichier obtenu, l'extraction, le placement dans l'arborescence prévue, l'écriture et la lecture du manifeste de l'installation courante et la résolution du chemin de la bibliothèque à charger.

Une seule version est installée pour chaque émulateur. L'installation d'une autre version remplace la bibliothèque déjà installée après validation du nouveau fichier. Les anciennes versions ne sont pas conservées dans des dossiers parallèles et il n'existe pas de sélection persistante entre plusieurs installations locales. Le manifeste décrit uniquement la version actuellement installée.

App ne recherche pas elle-même le chemin d'une bibliothèque installée et ne contient aucun `AmigaCoreProvider`, `AtariCoreProvider` ou équivalent propre à une famille. App fournit uniquement l'interface commune permettant de rechercher une version, lancer ou annuler son installation, suivre sa progression et afficher son état ou ses erreurs. Le clic est transmis au service de la bibliothèque associée à l'onglet.

Le dossier racine réservé aux installations peut être fourni par App afin de conserver une organisation générale cohérente des données. À partir de cette racine, la bibliothèque spécialisée choisit et gère seule ses sous-dossiers, noms de fichiers et manifestes. Ces détails ne sont ni reconstruits ni interprétés par App.

Lors de la création d'une machine, l'Engine de famille sélectionne sa Factory interne. Cette Factory demande directement au service d'installation de sa propre bibliothèque le chemin actif de l'émulateur nécessaire. Si aucun exemplaire utilisable n'est installé, la bibliothèque renvoie une erreur conforme au contrat commun ; App se limite à présenter cette erreur.

L'ouverture d'une machine ne déclenche jamais automatiquement le téléchargement d'un émulateur manquant. Si l'émulateur nécessaire n'est pas installé, la création est refusée et App affiche l'erreur commune renvoyée par la bibliothèque, avec l'indication qu'il doit être installé depuis Options. La recherche, le choix de version, le téléchargement et l'installation sont toujours des actions explicites effectuées depuis l'écran de gestion des émulateurs.

Une bibliothèque d'émulation utilisée par une instance en cours d'exécution peut être verrouillée par Windows. Chaque `Emulation.xxx` connaît les instances créées par ses Factories et conserve, pour chaque émulateur, le nombre d'instances encore actives. La fonction de téléchargement et d'installation vérifie elle-même ce nombre dès son entrée, avant toute requête réseau, création de fichier temporaire ou modification de l'installation. Si une instance de l'émulateur ciblé est active, la fonction refuse immédiatement l'opération et renvoie l'erreur commune indiquant qu'il faut fermer les machines concernées.

Cette vérification appartient à `Emulation.xxx`, pas à App et pas à l'état visuel des onglets. Un onglet ouvert ne prouve pas qu'une instance est encore active, et le texte affiché dans l'onglet ne doit jamais être analysé. App appelle simplement la fonction d'installation et affiche son résultat ou son erreur. Elle ne ferme jamais automatiquement les machines et la bibliothèque ne reporte pas silencieusement l'installation. Le blocage est appliqué à l'émulateur précis : une instance utilisant Hatari n'empêche pas, par exemple, l'installation d'Atari800.

Lorsqu'une installation est refusée ou échoue, `Emulation.xxx` lève une exception spécialisée conforme aux contrats communs. Cette exception contient un code stable et les données utiles, par exemple l'identifiant de l'émulateur concerné ; elle ne contient pas le texte final de l'interface. App intercepte cette exception au point où elle a lancé l'action, choisit la traduction correspondant au code et appelle sa boîte de dialogue d'erreur commune. `Emulation.xxx` n'appelle jamais directement une fenêtre, un dialogue ou une fonction graphique d'App.

Cette règle s'applique à tous les messages créés par GW GUI, et pas uniquement aux erreurs d'installation. Aucun texte destiné à l'utilisateur ne doit être construit en texte brut par `GWGUI.Emulation` ou par une bibliothèque `GWGUI.Emulation.xxx`.

Un message provenant directement d'un émulateur suit une règle distincte. `Emulation.xxx` cherche d'abord une correspondance connue et stable permettant de le convertir en `MessageCode`, afin qu'App utilise la traduction correspondante. Si aucune correspondance n'est connue, `Emulation.xxx` peut transmettre le texte original de l'émulateur sans traduction. Ce texte de repli ne doit jamais être confondu avec un message produit par GW GUI.

Le contrat commun d'un message reste plat et contient six données :

- `Category` classe le domaine du message, par exemple les supports, l'émulateur ou le firmware ;
- `MessageCode` identifie précisément le message et constitue la liaison stable avec sa traduction ;
- `Severity` indique son importance et détermine son apparence commune, notamment l'icône et la couleur ;
- `Target` désigne son unique destination d'affichage dans App, par exemple une boîte de dialogue, une zone intégrée, la barre d'état ou aucun affichage avec `Silent` ;
- `Context` contient les données nécessaires au message, sans phrase déjà formatée.
- `OriginalText` contient uniquement le texte original reçu directement d'un émulateur lorsqu'aucune correspondance traduite n'a été trouvée. Il reste vide pour les messages reconnus et pour tous les messages créés par GW GUI.

Les journaux techniques produits par un émulateur sont reçus et traités par la bibliothèque `Emulation.xxx` qui gère cet émulateur. Ils restent les journaux propres à cette bibliothèque et à ses émulateurs ; ils ne sont pas transformés en messages utilisateur et ne sont pas intégrés aux journaux généraux d'App.

Chaque bibliothèque `Emulation.xxx` possède son propre journal. Le nom du fichier journal correspond au nom de la bibliothèque afin que son origine soit identifiable directement. Par exemple, `GWGUI.Emulation.Atari` utilise son journal de bibliothèque `emulation-atari.log` et `GWGUI.Emulation.Amiga` utilise `emulation-amiga.log`. Toutes les instances et tous les émulateurs gérés par une même bibliothèque alimentent ce journal commun à la bibliothèque.

Les messages que l'émulateur destine à l'utilisateur suivent un chemin différent. `Emulation.xxx` cherche obligatoirement une correspondance traduite avant de remplir `OriginalText`. Lorsqu'il reconnaît le message, il le convertit vers le contrat commun avec son `MessageCode` et laisse `OriginalText` vide. Dans le cas contraire seulement, `MessageCode` prend la valeur générique `UntranslatedEmulatorMessage` et `OriginalText` contient le texte original à afficher sans traduction. Dans les deux cas, `Emulation.xxx` transmet également `Severity` et `Target`, puis App applique l'apparence et la destination demandées. `Silent` signifie qu'App ne produit aucun affichage pour ce message. Le message original reste enregistré séparément dans le journal propre à la bibliothèque.

Il n'existe pas de troisième mécanisme commun nommé « Diagnostic ». Cette notion est supprimée : une donnée provenant d'un émulateur est soit un journal technique géré par son `Emulation.xxx`, soit un message utilisateur transmis par l'interface commune.

`Context` contient une seule instance, mais cette instance peut implémenter plusieurs interfaces de contexte lorsque le message nécessite plusieurs ensembles de données. Les contextes restent strictement structurés et typés ; il ne faut pas employer un dictionnaire libre acceptant n'importe quelle clé ou valeur. Ils peuvent contenir, par exemple, l'identifiant d'une machine, l'identifiant d'un émulateur, un chemin ou une liste de supports attendus. Ils ne contiennent jamais une phrase destinée à l'utilisateur.

App possède la correspondance entre chaque `MessageCode` et sa clé de traduction. Elle sélectionne la langue active, récupère le texte traduit, injecte les valeurs du contexte dans les emplacements prévus, applique l'apparence indiquée par `Severity`, puis utilise la destination indiquée par `Target`. Toutes les langues prises en charge doivent définir la ressource correspondant à chaque code affichable. Une traduction absente doit être détectable comme une ressource manquante et ne doit pas être remplacée silencieusement par un texte anglais fourni par la bibliothèque.

Les composants visuels actuels de gestion des émulateurs deviennent communs et réutilisables. Ils reçoivent les versions, états, textes traduits et progressions nécessaires, sans contenir les règles de téléchargement ou d'installation d'Amiga, Atari ou d'une autre famille.

Les commandes de clavier, souris et manette sont envoyées directement par l'onglet App à son instance de machine. Il en va de même pour les changements de disquette et de CD. App choisit l'instance grâce à la référence déjà conservée par l'onglet ; aucun routage par l'Engine n'intervient.

Plusieurs machines d'une même famille produisent donc plusieurs chaînes indépendantes : chaque onglet App correspond directement à une instance de machine de la bibliothèque concernée. Une machine Atari exécute l'implémentation fournie par `GWGUI.Emulation.Atari`, tandis qu'une machine Amiga exécute celle de `GWGUI.Emulation.Amiga`. App appelle les interfaces communes ; la référence de chaque instance détermine déjà la bonne implémentation, sans recherche dynamique de DLL.

Elle fournit uniquement des données et comportements, jamais des boutons, sélecteurs, onglets ou autres objets graphiques.

Pour chaque machine, elle détermine :

- les sous-onglets visibles ;
- les blocs et options visibles ;
- les valeurs proposées ;
- les compatibilités et contraintes ;
- les correspondances entre les choix de GW GUI et ceux de l’émulateur.

### 5.2 Interface commune des instances en cours d’exécution

Après sa création par l’Engine et la Factory appropriée, chaque machine est remise à App sous la forme de l’interface commune `IEmulatedMachine`. Cette interface est déclarée une seule fois dans `GWGUI.Emulation`, dans le dossier `Interfaces`. Elle constitue le contrat commun entre App et toutes les implémentations de machines fournies par `GWGUI.Emulation.xxx`.

La même définition est utilisée des deux côtés :

- `GWGUI.App` référence l’interface pour connaître les propriétés qu’il peut lire et les méthodes qu’il peut appeler ;
- `GWGUI.Emulation.Amiga`, `GWGUI.Emulation.Atari` et les futures bibliothèques référencent cette même interface pour l’implémenter dans leurs classes de machines concrètes ;
- une instance concrète, par exemple `AmigaMachine` ou `AtariMachine`, exécute réellement le code demandé.

Il n’existe pas une copie de l’interface dans App et une autre dans chaque bibliothèque. Il n’existe pas non plus d’objet intermédiaire qui reçoit, copie puis retransmet chaque commande. Une variable typée avec une interface désigne directement l’instance concrète. L’interface indique uniquement les membres disponibles, les données acceptées, les résultats renvoyés et les garanties fournies par l’instance.

Le sens d’un appel d’exécution est donc direct :

```text
contrôle commun dans App
        ↓ appel d’une interface commune
instance concrète AmigaMachine, AtariMachine ou autre
        ↓
émulateur utilisé par cette instance
```

L’Engine n’intervient pas dans cette chaîne après la création de l’instance. Il n’est rappelé que si App doit créer ou recréer une machine.

### 5.3 Composition de l’interface principale

`IEmulatedMachine` expose l’identité et l’état général de l’instance, puis donne accès à des interfaces communes séparées par responsabilité. Cette composition évite une interface unique fourre-tout tout en donnant à App un seul point d’accès à la machine.

La structure visée est :

```csharp
public interface IEmulatedMachine : IAsyncDisposable
{
    Guid Id { get; }
    EmulationMachineState State { get; }

    IEmulationLifecycle Lifecycle { get; }
    IEmulationInput Input { get; }
    IEmulationMedia Media { get; }
    IEmulationAudio Audio { get; }
    IEmulationVideo Video { get; }
    IEmulationSavedStates SavedStates { get; }
    IEmulationRuntime Runtime { get; }
}
```

Ces propriétés ne provoquent aucun transfert supplémentaire. Une classe concrète peut implémenter elle-même plusieurs de ces interfaces et renvoyer `this`, ou déléguer une responsabilité à un objet interne. Dans les deux cas, App appelle directement l’implémentation appartenant à l’instance de l’onglet concerné.

Chaque interface doit être placée dans son propre fichier sous `src/GWGUI.Emulation/Interfaces`. Les records utilisés pour transporter les données vont dans `Contracts`, les enums dans `Enums` et les constantes dans `Constants`.

### 5.4 Cycle de vie

`IEmulationLifecycle` regroupe uniquement les commandes qui changent l’état général de la machine :

```csharp
public interface IEmulationLifecycle
{
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    ValueTask PauseAsync(CancellationToken cancellationToken = default);
    ValueTask ResumeAsync(CancellationToken cancellationToken = default);
    ValueTask SoftResetAsync(CancellationToken cancellationToken = default);
    ValueTask HardResetAsync(CancellationToken cancellationToken = default);
    ValueTask StopAsync(CancellationToken cancellationToken = default);
}
```

Les redémarrages à chaud et à froid restent deux commandes distinctes. Les boutons communs d’App appellent ces méthodes sur l’interface de l’instance affichée dans leur onglet.

### 5.5 Entrées

`IEmulationInput` reçoit les entrées produites par App. Comme dans le fonctionnement actuel, un `EmulationInputSnapshot` commun regroupe l’état du clavier, de la souris et des manettes pour l’instant traité :

```csharp
public interface IEmulationInput
{
    void SetInput(EmulationInputSnapshot snapshot);
    void SetControllerPortDevice(int port, EmulationPeripheralCategory peripheral);
}
```

App détecte les périphériques physiques et construit l’instantané. L’instance concrète traduit ensuite cet instantané vers les entrées attendues par son émulateur. Il n’est pas nécessaire de créer une chaîne d’interfaces clavier, souris et manette tant que le même instantané commun suffit. Si une machine ne permet pas de modifier en direct le périphérique associé à un port, cette capacité est déclarée indisponible par ses données de configuration.

### 5.6 Médias

`IEmulationMedia` représente les opérations communes sur les supports montés :

```csharp
public interface IEmulationMedia
{
    IReadOnlyList<EmulationMountedMedia> MountedMedia { get; }

    ValueTask InsertAsync(
        EmulationMedia media,
        CancellationToken cancellationToken = default);

    ValueTask EjectAsync(
        EmulationMediaSlot slot,
        CancellationToken cancellationToken = default);

    ValueTask SelectDiskAsync(
        EmulationMediaSlot slot,
        int index,
        CancellationToken cancellationToken = default);
}
```

Les extensions acceptées ne sont pas stockées dans le périphérique commun et ne sont pas fournies globalement par l'instance. App demande à la bibliothèque `Emulation.xxx` associée quelles extensions sont acceptées pour l'émulateur sélectionné et le lecteur ciblé. La bibliothèque renvoie cette liste à partir des capacités réelles de ce couple émulateur/lecteur. App l'utilise pour filtrer la boîte de sélection et pour empêcher une association manifestement invalide. Deux lecteurs de même catégorie peuvent ainsi recevoir des listes différentes si leur émulateur ou leur modèle le nécessite.

La catégorie du périphérique reste définie par un enum commun listant les périphériques possibles, par exemple `FloppyDrive`, `CompactDiscDrive`, `HardDisk`, `CartridgeSlot` ou `CassetteDrive`. `Directory` n'est pas un périphérique et ne doit pas être proposé comme support montable. Un disque dur reçoit une image disque, pas un dossier. En revanche, l'emplacement ne doit pas être un enum fermé contenant des valeurs comme `Floppy0`, `Floppy1` ou `Cd0`. Il associe la catégorie à un index :

```csharp
public sealed record EmulationMediaSlot(
    EmulationMediaCategory Category,
    int Index);
```

Une machine peut ainsi déclarer quatre lecteurs de disquettes avec les index 0 à 3 et trois lecteurs CD avec les index 0 à 2. Le contrat commun ne fixe aucune quantité maximale ; `Emulation.xxx` déclare uniquement les périphériques réellement disponibles pour la machine et pris en charge par son émulateur.

Les contrats `EmulationMedia`, `EmulationMediaSlot` et `EmulationMountedMedia` décrivent de manière commune le type du support, son chemin, son emplacement, son état inséré ou éjecté et, lorsque nécessaire, la liste et l’index d’un ensemble de disquettes. Les implémentations Amiga et Atari convertissent ces données vers leurs structures internes. Les méthodes particulières comme `InsertFloppyAsync` ne restent pas dans l’API commune si l’opération peut être exprimée par `InsertAsync` avec un type de support disquette.

App conserve parallèlement l’état des supports de chaque onglet. L’interface sert à appliquer l’opération à l’instance ; elle ne remplace pas cet état conservé par App pour permettre une recréation de la machine.

### 5.7 Vidéo et audio

`IEmulationVideo` expose les images produites par l’instance :

```csharp
public interface IEmulationVideo
{
    VideoFrame? LatestFrame { get; }
    event EventHandler<VideoFrame>? FrameReady;
}
```

`IEmulationAudio` expose les blocs audio et les commandes d’exécution réellement communes :

```csharp
public interface IEmulationAudio
{
    AudioChunk? LatestChunk { get; }
    bool IsMuted { get; }
    float Volume { get; }

    event EventHandler<AudioChunk>? ChunkReady;

    void SetMuted(bool muted);
    void SetVolume(float volume);
    void SetOutputFactory(Func<IAudioOutput?>? factory);
}
```

Le volume général fait partie du contrat commun, mais son utilisation dépend des capacités déclarées par la machine. App affiche et active le réglage uniquement pour les machines qui permettent de modifier ce volume. Une machine qui ne prend pas cette capacité en charge n’est pas obligée de simuler un réglage sans effet.

La sélection et la détection des périphériques audio physiques restent gérées par App. Lors de la création, la Factory de la machine reçoit la fabrique de sortie correspondant au périphérique enregistré. App peut également remplacer cette fabrique pendant l’exécution avec `SetOutputFactory`.

L’instance ferme alors proprement l’ancienne sortie, crée la nouvelle et reprend la production audio sans recréer toute la machine. Ce comportement doit être implémenté de la même manière par Amiga, Atari et les futures bibliothèques. Il remplace le fonctionnement dissymétrique actuel où Atari sait remplacer sa fabrique audio alors qu’Amiga conserve uniquement la sortie créée au démarrage.

### 5.8 Sauvegardes d’état

`IEmulationSavedStates` unifie la prise en charge des états sauvegardés :

```csharp
public interface IEmulationSavedStates
{
    bool IsSupported { get; }

    ValueTask SaveAsync(
        string path,
        CancellationToken cancellationToken = default);

    ValueTask LoadAsync(
        string path,
        CancellationToken cancellationToken = default);
}
```

App appelle cette interface directement sur l’instance de l’onglet. Une bibliothèque qui ne prend pas en charge les sauvegardes d’état renvoie `false` dans `IsSupported`, ce qui permet à App de masquer ou désactiver les commandes correspondantes sans tester la famille de la machine.

### 5.9 État d’exécution et options natives

`IEmulationRuntime` expose les informations d’exécution communes :

```csharp
public interface IEmulationRuntime
{
    IReadOnlyDictionary<int, bool> LedStates { get; }
    string EmulatorName { get; }
    string EmulatorVersion { get; }
    IReadOnlyList<EmulationOption> AvailableOptions { get; }

    ValueTask SetOptionAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default);
}
```

`AmigaCoreOption` et `AtariCoreOption` doivent être convertis par leur bibliothèque en un contrat commun `EmulationOption`. Les noms et valeurs natifs propres à l’émulateur restent internes à `GWGUI.Emulation.xxx`. Les données d’état supplémentaires qui n’ont aucun sens commun restent également internes tant qu’App n’en a pas besoin pour une fonction générique.

Ce contrat commun doit être établi à partir des deux implémentations existantes, et non à partir du seul objet `AtariRuntimeStatus`. Les machines Amiga et Atari exposent déjà toutes les deux les dernières images et données audio, les événements correspondants, les états des voyants, le nom et la version de l’émulateur, les options disponibles et l’erreur éventuelle. Ces éléments passent donc par les interfaces communes décrites ci-dessus.

La cadence vidéo et la fréquence audio existent également dans les deux implémentations internes. La cadence vidéo appartient aux informations vidéo ; la fréquence audio est déjà portée par les blocs `AudioChunk` et peut aussi être exposée par l’interface audio si App doit l’afficher avant la réception du premier bloc. L’activité des lecteurs doit provenir des états de voyants fournis par chaque instance afin que le même composant App fonctionne pour Amiga et Atari.

Les compteurs de tampon audio, le nombre de dépassements ou de manques audio, la région Atari et l’état ou l’identifiant du processus hôte ne sont pas imposés à toutes les machines. Ils restent dans l’implémentation spécialisée tant qu’aucune fonction commune d’App ne les utilise. Si une fonction commune en a ensuite besoin, elle est ajoutée comme capacité commune facultative ; elle ne doit pas être placée dans un objet global spécifique à Atari puis consommée directement par App.

Après migration, `AtariRuntimeStatus` ne doit donc plus servir de contrat entre App et Atari. App doit obtenir les informations dont elle a besoin par les mêmes interfaces communes que pour Amiga. Une donnée réellement propre à Atari reste interne à `GWGUI.Emulation.Atari` et ne force pas Amiga à fournir une valeur artificielle.

### 5.10 Séparation avec la configuration

Les interfaces précédentes servent aux échanges avec une machine déjà créée. Elles ne contiennent pas l’ensemble de toutes les options de configuration de tous les émulateurs.

Les choix de machine, RAM, ROM, vidéo, audio, stockage, clavier, souris et manettes affichés dans Options suivent le système de descriptions défini dans les autres sections de ce document. App enregistre la configuration complète, puis la transmet à l’Engine et à la Factory lors de la création de l’instance. Les interfaces d’exécution contiennent uniquement les commandes et informations nécessaires pendant que cette instance existe.

La distinction est donc :

```text
descriptions de configuration
    → construisent et enregistrent les choix dans Options
    → sont transmises à l’Engine et à la Factory lors de la création

interfaces de l’instance
    → servent ensuite aux commandes et données en cours d’exécution
    → sont appelées directement par l’onglet App associé à cette instance
```

### 5.11 Utilisation commune par App

Un onglet d’exécution conserve une seule référence vers son `IEmulatedMachine`. Les contrôles communs de cet onglet utilisent ensuite uniquement les interfaces communes :

- les boutons alimentation, pause et redémarrage appellent `Lifecycle` ;
- le gestionnaire clavier, souris et manettes appelle `Input` ;
- la barre des lecteurs et les sélecteurs de fichiers appellent `Media` ;
- le rendu d’écran écoute `Video` ;
- la sortie audio écoute et commande `Audio` ;
- les boutons de sauvegarde et restauration appellent `SavedStates` ;
- les journaux, LED et informations sur l’émulateur lisent `Runtime`.

Les classes concrètes de `GWGUI.Emulation.xxx` implémentent les mêmes interfaces communes. App n’a donc pas besoin de tester `IAmigaMachine` ou `IAtariMachine` pour ces opérations et ne dépend pas de leurs membres spécifiques. La référence conservée par chaque onglet garantit que les commandes atteignent la bonne instance lorsqu’il existe plusieurs machines simultanément.

## 6. Responsabilité de GWGUI.App

`GWGUI.App` contient l’interface : onglets, boutons, sélecteurs, champs, listes, tableaux, dialogues, styles, icônes et traductions.

Les traductions restent dans App. Elles sont des ressources d’affichage ; il est inutile de créer un système linguistique indépendant dans chaque bibliothèque intégrée.

Les contrôles peuvent être spécialisés par fonction — ROM, mémoire, lecteur, CPU, etc. — mais jamais par marque. Ils affichent les éléments demandés par la bibliothèque associée à l’onglet courant.

Les données Atari vont uniquement dans l’onglet Atari et les données Amiga uniquement dans l’onglet Amiga.

## 7. Sous-onglets et options

Le sous-onglet Général permet notamment la sélection de la machine. Son contenu est néanmoins piloté par les données de la bibliothèque.

La machine ne peut être changée que depuis ce sous-onglet Général. Lors du changement de machine, Général est donc déjà actif ; masquer ou afficher les autres sous-onglets ne nécessite aucune règle de redirection vers un autre sous-onglet.

### 7.1 Sélecteur commun des machines

Chaque onglet de famille crée sa propre instance du sélecteur commun des machines. La bibliothèque `Emulation.xxx` correspondante fournit à ce sélecteur uniquement la liste nécessaire à son remplissage : pour chaque machine, un identifiant stable et la clé permettant à App d'afficher son nom traduit.

Le sélecteur ne reçoit pas toutes les caractéristiques matérielles de toutes les machines. Lorsqu'une machine est sélectionnée, App transmet son identifiant à la bibliothèque associée à l'onglet. Cette bibliothèque recherche alors la machine dans son propre catalogue et renvoie seulement les données nécessaires à la configuration sélectionnée : sous-onglets, blocs et éléments visibles, valeurs proposées, valeurs actuelles, contraintes et compatibilités.

Le catalogue complet et les règles propres aux machines restent ainsi dans `Emulation.xxx`. App ne conserve et n'interprète pas leurs caractéristiques ; il crée les contrôles communs, traduit les textes et affiche les données reçues.

Chaque bibliothèque définit la visibilité par défaut de CPU, RAM, ROM, Vidéo, Audio, Stockage, Clavier, Souris, Manettes et des autres sous-onglets communs. Une machine peut remplacer un défaut :

- défaut du module visible → toutes ses machines l’affichent ;
- si la machine ne fournit aucune valeur pour ce sous-onglet, elle conserve la valeur définie par le module ;
- si la machine fournit explicitement `true`, le sous-onglet est visible pour cette machine ;
- si la machine fournit explicitement `false`, le sous-onglet est masqué pour cette machine, même si le module le rend visible par défaut.

Lorsqu’un sous-onglet est visible, la bibliothèque indique exactement quels blocs et éléments communs apparaissent. Lorsqu’il est masqué, son contenu n’est ni affiché ni utilisé.

Chaque élément peut être modifiable, informatif, visible mais désactivé, ou masqué.

La même règle de valeurs par défaut et de remplacement s'applique aux blocs et aux options. Une bibliothèque peut définir en général les éléments communs à ses machines, puis chaque machine peut forcer leur affichage ou leur masquage. Si les machines d'une bibliothèque sont trop différentes, la bibliothèque peut ne définir aucun élément général et déclarer séparément, pour chaque machine, tout ce qui doit être affiché. Les deux méthodes peuvent coexister dans une même bibliothèque selon les éléments concernés.

App ne masque pas automatiquement un bloc parce que toutes ses options sont masquées. La visibilité du bloc suit uniquement les données déclarées. Si une déclaration produit un bloc vide, elle doit être corrigée dans la bibliothèque concernée.

Pour un sélecteur commun dont les valeurs sont connues par App, comme les quantités de RAM, la bibliothèque fournit la clé du sélecteur, les clés des choix autorisés et la clé sélectionnée. App possède les correspondances d’affichage et les traductions de ces valeurs. Un module ne duplique donc pas les libellés communs.

Les sous-onglets, blocs, options et choix transmis entre App et les bibliothèques sont désignés par des identifiants communs définis dans `GWGUI.Emulation`, et non par des chaînes écrites librement. Les écritures telles que `mouseTab = true` ou `pokeyStereoSelect = true` décrivent seulement le principe de fonctionnement ; elles ne constituent pas le format technique retenu.

Les périphériques réels de l’ordinateur hôte, notamment les sorties audio et les manettes physiques détectées, sont recensés et gérés par App. Ils ne sont pas fournis par `Emulation.xxx`. La bibliothèque indique seulement si la machine utilise la fonction concernée et, lorsque cela est nécessaire, les capacités ou types d’entrées qu’elle accepte.

La séparation générale est la suivante : tout ce qui décrit ou détecte le matériel physique de l’ordinateur hôte appartient à App ; tout ce qui décrit la machine émulée, son matériel, ses capacités, ses options et les valeurs disponibles est fourni par la bibliothèque `Emulation.xxx` concernée.

Pour les contrôleurs, App fournit la liste des périphériques physiques détectés, tandis que `Emulation.xxx` fournit les ports émulés et les types de contrôleurs acceptés par la machine. L'association choisie entre un périphérique physique et un port émulé est enregistrée dans la configuration de la machine.

### 7.2 Actualisation des ROM détectées

Le bouton et la gestion du clic sur **Actualiser** sont communs dans App. App détermine la bibliothèque associée à l'onglet actif, récupère l'identifiant de la machine sélectionnée et le chemin de son dossier ROM, puis appelle la fonction d'analyse de cette bibliothèque.

La bibliothèque ciblée parcourt le dossier, lit et identifie les fichiers, puis calcule pour chacun le nom détecté, la version, le type et la compatibilité avec la machine sélectionnée. Elle renvoie ces résultats à App au moyen d'un contrat commun. Les fichiers ne sont ni déplacés ni renommés ; seul leur nom d'affichage est produit par l'analyse.

App applique ensuite le tri commun et construit la liste graphique. La bibliothèque détermine le statut de chaque résultat, mais App garantit le même ordre pour toutes les familles : officiel, compatible, partiellement compatible, incompatible, puis inconnu. À statut identique, App peut trier par nom et version.

Le bouton commun **Utiliser** transmet à la bibliothèque associée à l'onglet la ROM sélectionnée et l'identifiant de la machine ciblée. La bibliothèque détermine l'emplacement correspondant à ce type de ROM — ROM système, ROM étendue, clé ROM ou autre emplacement déclaré — puis renvoie à App la configuration mise à jour. App actualise les contrôles communs avec ces données et ne contient aucune règle Atari, Amiga ou propre à une autre famille pour choisir l'emplacement.

### 7.3 Traitements vidéo communs de GW GUI

Les traitements vidéo réalisés après la production d’une `VideoFrame` sont communs à toutes les
familles de machines. Ils ne sont pas décrits ni exécutés séparément par
`GWGUI.Emulation.Amiga`, `GWGUI.Emulation.Atari` ou une future bibliothèque spécialisée.

La séparation obligatoire est la suivante :

```text
GWGUI.Emulation
    → contrats sérialisables de configuration vidéo
    → identifiants, valeurs neutres, groupes et compatibilités
    → aucune dépendance vers WPF, Veldrid, OpenGL ou un langage de shader

GWGUI.Emulation.xxx
    → porte la configuration commune dans chaque configuration de machine
    → conserve séparément les options vidéo natives de son émulateur
    → ne traduit pas une option PAL, NTSC, composite, RF ou équivalente en filtre GW GUI

GWGUI.App
    → construit l’interface commune de l’onglet Vidéo
    → compose la chaîne de traitement à partir de la configuration
    → exécute cette chaîne dans WPF/CPU, OpenGL ou Veldrid
    → applique immédiatement une nouvelle configuration à l’instance ciblée
```

Le catalogue commun décrit les technologies d’affichage, les fonctionnalités, leurs paramètres,
leurs valeurs neutres, leurs dépendances et leurs incompatibilités. Les textes visibles ne se
trouvent pas dans ce catalogue : il transporte uniquement des clés de ressources traduites par
App. Les algorithmes, shaders, pipelines, textures temporaires et objets graphiques restent dans
App.

La chaîne reçoit une image commune et l’ordonne ainsi : normalisation de la source, restauration
éventuelle, mise à l’échelle, technologie d’affichage, réglages généraux, puis sortie. Une étape
déclare son espace de couleur, ses besoins en textures intermédiaires et son éventuel historique de
trames. Une même définition de chaîne est utilisée par les quatre renderers ; seuls les exécuteurs
et ressources propres au backend diffèrent.

Direct3D 11 et Vulkan partagent l’exécution Veldrid et les définitions portables de shaders.
OpenGL conserve son renderer et exécute la même chaîne avec textures, quad, programmes et
framebuffers OpenGL. WPF conserve son rôle de renderer et de repli au moyen d’une exécution CPU de
référence pour toutes les fonctionnalités déclarées compatibles. Aucun renderer ne peut être
supprimé, changé ou privé silencieusement d’une fonctionnalité.

#### Snapshot

`Snapshot` représente la sortie finale de la chaîne de traitements GW GUI, avant tout habillage
externe futur comme un bezel ou un cadre. Il produit ainsi la même image que celle visible dans la
zone d’émulation, quelle que soit la surface WPF, OpenGL, Direct3D 11 ou Vulkan utilisée. Une surface
GPU effectue la lecture de sa ressource de sortie à la demande ou réutilise une copie lisible ; elle
ne provoque pas une lecture GPU systématique à chaque trame uniquement pour préparer une capture.

#### Enregistrement et application aux instances

La configuration des traitements appartient à la configuration de chaque machine. Elle n’est ni
globale, ni partagée comme objet mutable entre Amiga, Atari ou plusieurs instances ouvertes. Les
valeurs absentes d’une ancienne configuration prennent les valeurs neutres sans migration
destructive.

Lorsqu’une configuration existante est modifiée, App l’enregistre selon le mécanisme automatique
déjà utilisé par l’éditeur, puis cible l’onglet ouvert par le couple
`(ModuleId, ConfigurationId)`. Seule cette instance reçoit la nouvelle configuration vidéo. Une
modification d’un paramètre numérique met à jour les constantes de la chaîne sans recréer la
machine. Une modification structurelle construit une nouvelle chaîne de façon atomique ; l’ancienne
reste active si la construction échoue.

Si aucune instance correspondante n’est ouverte, aucune chaîne d’exécution n’est créée. Les valeurs
enregistrées sont simplement relues et appliquées lors de la prochaine création de cette machine.
Un post-traitement GW GUI ne demande jamais à l’Engine de recréer l’instance émulée.

Une erreur de compilation ou d’exécution d’un traitement ne modifie pas les valeurs enregistrées.
Le repli actuel vers WPF reçoit la même configuration. Une limitation de backend est déclarée et
présentée par un texte localisé ; elle ne provoque pas la disparition silencieuse d’un réglage.

#### Interface de l’onglet Vidéo

Les contrôles des traitements GW GUI existent uniquement dans le sous-onglet **Vidéo** de la
configuration de machine. Ils sont visuellement séparés des options internes de l’émulateur. Ils ne
sont ajoutés ni dans la barre d’outils de l’instance, ni dans les options générales de l’application,
ni dans une page propre à Amiga ou Atari.

Luminosité, contraste, gamma, saturation et netteté restent visibles en permanence. Un sélecteur
unique choisit l’échantillonnage. Un autre sélecteur unique choisit la technologie d’affichage :
Normal, CRT, écran à pixels fixes, Plasma ou Écran vectoriel. Le panneau placé sous ce second
sélecteur affiche uniquement les paramètres de la technologie choisie.

CRT contient son rendu couleur ou monochrome, ses palettes, son faisceau, son masque, sa géométrie,
ses scanlines et sa trame volontaire. L’écran à pixels fixes contient son sous-choix LCD, LCD/LED ou
OLED et partage les réglages communs, avec des paramètres conditionnels uniquement lorsqu’ils ont un
effet visuel propre. Plasma et Écran vectoriel possèdent chacun leur panneau. Les autres familles du
catalogue sont ajoutées progressivement sans créer une nouvelle page de configuration.

Le choix principal d’une technologie remplace naturellement la technologie précédente et ne demande
pas de confirmation. Une confirmation Oui/Non est utilisée uniquement lorsqu’une fonctionnalité
indépendante activée doit désactiver une autre fonctionnalité indépendante incompatible. En cas de
refus, toutes les valeurs restent inchangées.

Les présélections appliquent un ensemble de valeurs aux mêmes contrats ; elles ne constituent pas
un second format de configuration et ne gardent pas de dépendance vers un fichier Libretro. Toute
valeur modifiée est enregistrée immédiatement et transmise à la seule instance ouverte
correspondante.

## 8. Migration du code existant

À la fin de la réorganisation, aucun fichier propre à Amiga ou Atari ne doit rester dans `GWGUI.App`, hormis les raccordements explicites nécessaires aux bibliothèques référencées.

Chaque fichier `Amiga...cs` ou `Atari...cs` présent dans App doit être examiné :

- sa construction graphique devient un contrôle commun dans App ;
- ses règles propres rejoignent `GWGUI.Emulation.Amiga` ou `GWGUI.Emulation.Atari` ;
- ses comportements réellement communs rejoignent le projet commun approprié.

Il ne faut pas déplacer un fichier entier aveuglément : son contenu doit être séparé selon ses responsabilités. Tout objet graphique placé à tort dans une bibliothèque d’émulation doit revenir dans App.

## 9. Découpage des fichiers

Chaque fichier doit avoir une responsabilité claire. Les fichiers fourre-tout sont interdits, quelle que soit leur longueur.

Un fichier dépassant environ 300 lignes doit être examiné et découpé s’il traite plusieurs responsabilités. Il peut rester plus long uniquement si son contenu forme réellement une unité et qu’un découpage nuirait à sa cohérence.

Une méthode ne doit pas contenir la déclaration d’une autre fonction. Les fonctions doivent être placées au niveau et dans le fichier correspondant à leur responsabilité.

Les interfaces, enums, constantes, contrats et dictionnaires ne doivent pas être mélangés aux fichiers de méthodes.

## 10. Organisation des dossiers

Dans App, `GWGUI.Emulation` et chaque `GWGUI.Emulation.xxx`, les fichiers sont rangés selon leur rôle :

- `Controls` : composants graphiques ;
- `Interfaces` : interfaces ;
- `Contracts` : records et objets de transfert ;
- `Enums` : énumérations ;
- `Constants` : constantes ;
- `Dictionaries` : dictionnaires, catalogues et correspondances ;
- `Functions` : fonctions regroupées par responsabilité.

Une interface doit normalement avoir son fichier. Un enum doit de préférence avoir son fichier. Plusieurs petits enums ne peuvent partager un fichier que s’ils décrivent exactement le même sujet.

D’autres dossiers précis sont permis lorsqu’une responsabilité distincte le justifie. Aucun dossier ne doit devenir un fourre-tout.

## 11. Nommage

Le nom d’un fichier doit décrire son contenu réel. Un contrôle devenu commun perd son préfixe Atari ou Amiga. Un type réellement propre à une bibliothèque peut conserver le nom de sa famille ou machine.

Un nom générique ne doit jamais masquer une dépendance spécifique.

## 12. Ajouter une nouvelle famille

Pour ajouter Sega avec l’architecture retenue :

1. créer `GWGUI.Emulation.Sega` ;
2. y créer moteurs, machines, configurations, capacités et règles ;
3. ajouter sa référence dans `GWGUI.App.csproj` ;
4. la raccorder dans `OptionsEmulationSection` pour les paramètres ;
5. la raccorder dans `EmulationSection` pour charger et ouvrir ses machines ;
6. ajouter ses traductions et ressources visuelles dans App ;
7. réutiliser les contrôles communs, ou ajouter un nouveau contrôle commun si aucune représentation existante ne convient.

Il ne faut ni copier une interface Sega complète ni introduire une découverte automatique de DLL sans nouveau besoin explicite.

## 13. Validation

La réorganisation est conforme uniquement si :

- Amiga et Atari conservent toutes leurs fonctionnalités ;
- les raccordements Options et exécution restent fonctionnels ;
- chaque onglet reçoit uniquement les données de sa bibliothèque ;
- les éléments graphiques réutilisables sont communs dans App ;
- aucune bibliothèque d’émulation ne construit d’objet WPF ;
- aucune règle de machine ne reste cachée dans un contrôle commun ;
- aucune option existante n’est perdue ;
- les fichiers sont séparés et rangés selon leur responsabilité ;
- interfaces, enums, constantes, contrats et dictionnaires sont isolés ;
- les noms correspondent au contenu ;
- la compilation et les tests fonctionnels réussissent à la fin.

## 14. Vérifications ultérieures hors de la refonte initiale

Les points de cette section sont conservés pour ne pas être oubliés, mais ils ne font pas partie des modifications indispensables à la première réorganisation décrite par ce document.

- Vérifier la réaction d'App lorsqu'une instance s'arrête ou rencontre une erreur après avoir démarré. Actuellement, les instances modifient leur état interne, tandis que les contrôleurs App mettent surtout leurs boutons à jour après leurs propres commandes. Si App doit réagir immédiatement à un changement autonome, étudier un événement commun de changement d'état. Ne pas ajouter cet événement au contrat initial avant d'avoir confirmé le comportement attendu et son utilité réelle.
