# Organisation interne d’un module de famille d’émulation

Ce document fixe l’organisation cible de `GWGUI.Emulation.Atari`,
`GWGUI.Emulation.Amiga` et des futurs modules comme `GWGUI.Emulation.Amstrad`.

## Frontière avec Emulation

`GWGUI.Emulation` communique uniquement avec le module familial au travers de ses
contrats publics. Il ne référence jamais un cœur concret, son dossier, sa DLL ou
ses types.

Dans chaque module familial, `Common` reçoit les demandes de `GWGUI.Emulation`,
sélectionne un adaptateur et traduit le résultat vers les contrats publics.
Un adaptateur concret traduit ensuite entre les contrats internes de `Common` et
l’API native de son cœur.

```text
GWGUI.App
  -> GWGUI.Emulation
    -> GWGUI.Emulation.<Famille>/Common
      -> GWGUI.Emulation.<Famille>/Emulators/<Emulateur>
```

La dépendance inverse est interdite : un fichier de `Common` ne doit pas utiliser
un type placé sous `Emulators/<Emulateur>`.

## Arborescence cible

```text
GWGUI.Emulation.<Famille>/
├── Common/
│   ├── Constants/
│   ├── Contracts/
│   ├── Dictionaries/
│   ├── Enums/
│   ├── Exceptions/
│   ├── Factories/
│   ├── Functions/
│   ├── Interfaces/
│   └── Services/
├── Emulators/
│   └── <Emulateur>/
│       ├── Constants/
│       ├── Contracts/
│       ├── Dictionaries/
│       ├── Enums/
│       ├── Exceptions/
│       ├── Factories/
│       ├── Functions/
│       ├── Interfaces/
│       └── Services/
├── Modules/
├── Resources/
├── EmulationGlobalUsings.cs
├── GWGUI.Emulation.<Famille>.csproj
└── module.json
```

Une catégorie vide n’est pas créée. Les espaces de noms suivent toujours les
dossiers physiques.

## Noms communs aux familles

Les types qui remplissent le même rôle dans chaque module familial portent le
même nom et occupent le même chemin relatif sous `Common`. Leur contenu peut
différer selon la famille.

Exemples :

- `Common/Interfaces/IEmulatorAdapter.cs` ;
- `Common/Contracts/EmulatorCreationContext.cs` ;
- `Common/Dictionaries/EmulatorCatalog.cs` ;
- `Common/Contracts/MachineConfiguration.cs` ;
- `Common/Enums/Emulator.cs`.

Le préfixe `Atari`, `Amiga` ou `Amstrad` est retiré seulement lorsque le type est
le contrat général équivalent du module. Un type réellement propre à une machine
conserve un nom explicite, par exemple `AtariStModelDefinition`.

## Propriété des fichiers

Un fichier appartient à `Common` lorsqu’il décrit la famille, ses machines, sa
configuration, ses médias, ses commandes, ses périphériques ou un mécanisme
partagé par plusieurs adaptateurs.

Un fichier appartient à `Emulators/<Emulateur>` lorsqu’il connaît une API, un
protocole, un téléchargement, une DLL, une option ou un format propre à ce cœur.
Ses constantes, contrats, fonctions et services restent tous dans le dossier de
ce cœur.

Les commandes de joystick, clavier, souris et trackball restent définies par
machine dans `Common`. L’adaptateur concret les traduit vers les entrées natives
du cœur choisi.

## Cycle de vie

L’adaptateur concret possède son cœur, ses processus, ses threads et ses
ressources natives. Il les ferme et les libère dans un bloc `finally`, puis
attend leur destruction effective. `Common` ne détruit jamais un objet qu’il a
seulement reçu d’un appelant.

