# Architecture technique actuelle

## Projets de production

- `GWGUI.App` : application WPF, composition des fenêtres et contrôles, présentation, contrôleurs applicatifs et services propres à l’interface.
- `GWGUI.Infrastructure` : implémentations techniques liées à Greaseweazle, Windows, la persistance et les services externes.
- `GWGUI.MediaEngine` : API média de l’application, reconnaissance et décodage des images, lecture et écriture physiques, conversion d’images et visualisation.
- `GWGUI.MediaFileSystems` : détection des volumes et lecture des vrais dossiers et fichiers des images décodées ; construction des volumes cibles pour la migration de fichiers.
- `GWGUI.MediaAnalysis` : reconnaissance des types, catégories, icônes et clés de traduction des fichiers.
- `GWGUI.Emulation` : contrats, services et fonctions communs à toutes les familles émulées.
- `GWGUI.Emulation.Amiga` : modèles, catalogues, fonctions et modules propres à l’Amiga.
- `GWGUI.Emulation.Atari` : modèles, catalogues, fonctions et modules propres aux machines Atari.
- `GWGUI.VideoPresentation` : profils et traitements de présentation de la vidéo d'émulation.
- `GWGUI.Updates` : catalogues, validation et plans de mise à jour de l'application et des modules.
- `GWGUI.Launcher` : démarrage de l'application et résolution des DLL du paquet.
- `GWGUI.Updater` : remplacement des fichiers lors d'une mise à jour préparée et vérification du redémarrage.

## Responsabilités et échanges

Les responsabilités ci-dessous sont la cible retenue pour séparer les projets. Les écarts indiqués
comme *actuels* décrivent le code en cours de rangement ; ils ne transforment pas ces écarts en règles
d'architecture.

### Application et médias

**`GWGUI.App`** possède l'interface : fenêtres, contrôles, affichage, traduction des clés reçues,
état de présentation et réaction aux commandes de l'utilisateur. Elle transmet ces commandes au
service responsable et présente ses résultats. Elle assemble les services au démarrage et gère le
cycle de vie de ses fenêtres. Pour les médias, son seul point d'accès est `GWGUI.MediaEngine` : elle
ne demande pas directement une lecture à `GWGUI.MediaFileSystems` ni une classification à
`GWGUI.MediaAnalysis`. « Interface seulement » comprend cette coordination de l'interface ; cela
n'inclut pas la reconnaissance d'un format, d'un système de fichiers ou d'un type de fichier.

**`GWGUI.MediaEngine`** est le moteur des médias, pas un moteur d'émulation. Il reconnaît un format
d'image, charge et décode le média une seule fois, gère les représentations de flux, pistes, secteurs,
blocs ou données séquentielles, et fournit les données techniques du visualiseur. Il prend en charge
la lecture d'une disquette physique vers une image et l'écriture d'une disquette physique depuis une
image, ainsi que la conversion entre formats d'images compatibles. L'affichage des données du
visualiseur reste dans `App`. Pour l'explorateur, MediaEngine transmet le média déjà décodé à
`GWGUI.MediaFileSystems` et retourne le résultat à App. Pour une migration de fichiers, il lit
l'image source, crée l'image cible vierge, demande à MediaFileSystems d'y construire le système de
fichiers et d'y injecter les fichiers, puis écrit l'image résultante. Il peut fournir un média à un
module d'émulation sans gérer les machines, manettes ou sauvegardes de l'émulateur.

**`GWGUI.MediaFileSystems`** traite le contenu logique de l'image que MediaEngine lui fournit. Il
retrouve les vrais volumes, partitions, dossiers, fichiers, noms enregistrés, métadonnées et contenus
accessibles, puis construit l'arborescence de l'explorateur. Il ne recharge pas et ne redécode pas
l'image, n'invente pas de nom de fichier et ne reconnaît pas lui-même le type sémantique d'un
fichier. Il demande ce dernier travail à `GWGUI.MediaAnalysis`, puis retourne à MediaEngine les
données nécessaires à la réponse. Pour la migration, il planifie les noms et métadonnées compatibles
avec le système de fichiers cible et construit le volume qui reçoit les fichiers ; MediaEngine garde
la création et l'écriture du conteneur image.

**`GWGUI.MediaAnalysis`** reçoit de MediaFileSystems les dossiers et fichiers effectivement trouvés.
Il identifie leur type, leur catégorie, l'identifiant d'icône et la clé de traduction, et retourne
ces informations à MediaFileSystems. App traduit la clé et affiche le résultat. D'autres propriétés,
comme le logiciel capable d'ouvrir un fichier, restent une possibilité future, sans contrat arrêté
pour le moment. MediaAnalysis ne lit ni le support physique ni le conteneur image et ne construit
pas l'arborescence.

**État actuel de cette séparation :** les références de projets suivent
`App → MediaEngine → MediaFileSystems → MediaAnalysis`, mais les appels de production ne sont pas
encore tous raccordés. Plusieurs lecteurs de systèmes de fichiers restent dans MediaEngine, et la
classification et les catalogues de types utilisés par l'explorateur restent en partie dans App.
App assemble encore directement les adaptateurs de lecture et d'écriture physiques, en plus de ses
commandes d'interface. MediaFileSystems n'appelle pas encore MediaAnalysis pour classer les fichiers.
La progression exacte figure dans la [feuille de séparation](../tasks/media-library-separation.md).

### Matériel et accès techniques

**`GWGUI.Infrastructure`** contient actuellement l'accès au matériel et aux outils externes :
découverte des périphériques, protocole et transport Greaseweazle, lancement de `gw.exe`, lecture de
ses capacités, gestion de son installation, processus et journaux techniques. Les opérations média
restent décidées par MediaEngine ; Infrastructure fournit les adaptateurs matériels et les commandes
externes nécessaires. Les paramètres propres aux contrôleurs, lecteurs et outils externes ont aussi
leur place dans ce périmètre.

Infrastructure contient **également aujourd'hui** `AppSettings` et `JsonSettingsStore`. Ils
enregistrent bien plus que les paramètres du matériel : langue, thème, dossiers d'images et
d'émulation, raccourcis, placements de fenêtres, journaux, profils, préférences de lecture, écriture
et conversion, en plus des contrôleurs, lecteurs et chemins de `gw.exe`. Ce constat répond à la
question « persistance de quoi ? ». Si Infrastructure doit être limité à l'accès au matériel, aux
outils externes et à leurs paramètres, ces réglages généraux devront recevoir un autre propriétaire ;
ce document ne leur en attribue pas un arbitrairement.

Les contrats d'acquisition et d'écriture physiques sont déclarés par MediaEngine et implémentés
pour Greaseweazle dans Infrastructure. App assemble actuellement ces implémentations pour les
commandes de lecture et d'écriture. Cela explique pourquoi la référence de projet va
`Infrastructure → MediaEngine` sans référence inverse, alors que MediaEngine reçoit bien les
résultats du matériel.

### Émulation et vidéo

**`GWGUI.Emulation`** concerne la gestion commune des émulateurs : contrats des modules et des
machines, sessions, médias insérés, disques durs émulés, entrées et manettes côté émulateur, images
vidéo, audio, configurations et états de sauvegarde. Les adaptations aux périphériques Windows et
les écrans qui commandent ces fonctions ne sont pas le cœur de l'émulation. Actuellement, une partie
des services d'entrée, de chargement des modules et de persistance des configurations se trouve
encore dans App ; les réglages généraux d'émulation enregistrés par `AppSettings` sont dans
Infrastructure. Cette répartition actuelle ne change pas la responsabilité fonctionnelle de
GWGUI.Emulation : gérer l'émulation commune aux machines.

**`GWGUI.Emulation.Amiga`, `GWGUI.Emulation.Atari` et les futurs modules de machine** fournissent
les modèles, choix de machines, configurations et implémentations propres à leur famille. Les modules
Amiga et Atari référencent `GWGUI.Emulation` pour les contrats communs et `GWGUI.MediaEngine` quand
ils ont besoin d'une opération sur une image média. App les découvre et les charge comme modules ;
ils ne référencent pas App et ne se référencent pas mutuellement.

**`GWGUI.VideoPresentation`** définit les profils, réglages, préréglages et calculs de présentation
vidéo de l'émulation. App reçoit les images produites via les contrats d'Emulation, applique les
choix de présentation et effectue l'affichage dans ses surfaces graphiques. VideoPresentation est
référencé par App ; il ne référence actuellement ni App ni Emulation. Il n'est pas le propriétaire
des commandes de la machine émulée.

### Mises à jour et démarrage

**`GWGUI.Updates`** définit et valide les catalogues des versions de l'application et des modules,
compare les versions disponibles et prépare le plan d'installation. Il référence les contrats
d'Emulation nécessaires pour vérifier la compatibilité des modules. Actuellement, les services App
effectuent aussi les requêtes réseau et téléchargent les archives ; cette logique dépasse une App
strictement limitée à l'interface et reste à ranger si ce périmètre est appliqué partout.

**`GWGUI.Updater`** est l'outil appelé lorsqu'une mise à jour est prête. Il lit le plan fourni par
Updates, attend la fermeture des processus concernés, remplace les fichiers, contrôle le démarrage
de la nouvelle version et restaure l'ancienne si la vérification échoue. Ce n'est pas le composant
qui cherche ou télécharge les versions.

**`GWGUI.Launcher`** est l'exécutable de lancement. Il charge `gwgui.app.dll` et résout les DLL du
paquet dans `lib` ainsi que les ressources de langue. La découverte des modules d'émulation et la
gestion de leurs machines sont assurées après le démarrage par App et les bibliothèques d'émulation ;
Launcher ne les pilote pas.

## Projets de validation

- `GWGUI.Tests` contient la suite rapide par défaut et une catégorie `GpuExhaustive` séparée pour les matrices complètes de shaders et de moteurs de rendu.
- `GWGUI.LocalDiskImageTests` reste un outil local séparé de la solution principale pour les contrôles utilisant le corpus privé.

## Dépendances entre projets

Les références ci-dessous sont celles des projets de production ; une flèche signifie que le projet
de gauche référence celui de droite, et ne signifie pas que toutes les fonctions sont déjà
raccordées :

```text
App → MediaEngine → MediaFileSystems → MediaAnalysis
App → Infrastructure → MediaEngine
App → Emulation
App → VideoPresentation
App → Updates → Emulation
Emulation.Amiga / Emulation.Atari → Emulation, MediaEngine
Updater → Updates
Launcher → aucun projet GW GUI ; il charge App à l'exécution
```

`MediaAnalysis`, `Emulation` et `VideoPresentation` n'ont pas de référence vers un autre projet
GW GUI. App découvre les modules Amiga et Atari dans `Modules` sans référence directe à leurs
projets. Les modules ne se référencent pas mutuellement. Aucune bibliothèque de production ne
référence App, et il n'y a pas de référence circulaire entre les projets médias.

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

- `Reading`, `Recognition` et `Formats` pour ouvrir et reconnaître les images ;
- `Decoding`, `Encoding`, `Reconstruction` et `Representations` pour les données de flux, pistes,
  secteurs, blocs et contenus séquentiels ;
- `Acquisition` et `PhysicalWriting` pour les opérations sur les médias physiques ;
- `Conversion` pour les transformations d'images et l'orchestration de la migration de fichiers ;
- `Exploration` pour coordonner l'explorateur ; les lecteurs encore présents dans `FileSystems`
  doivent être examinés un par un dans la séparation en cours ;
- `Visualization` pour les données techniques du visualiseur ;
- `Composition` pour assembler les services du moteur.

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
