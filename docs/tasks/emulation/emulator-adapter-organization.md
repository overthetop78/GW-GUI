# Organisation interne des adaptateurs d’émulateurs

Cette feuille concerne uniquement le rangement et les interfaces internes des modules de famille.
Elle ne remplace pas les feuilles fonctionnelles Atari ou Amstrad et n’ajoute aucune nouvelle machine.

Architecture à conserver :

`GWGUI.App` → contrats de `GWGUI.Emulation` → gestion commune du module de famille → adaptateur du cœur choisi.

L’App ne connaît ni les projets de famille, ni leurs DLL, ni leurs implémentations. Les commandes des
joysticks, claviers, souris et trackballs restent définies par machine. Chaque adaptateur traduit ces
commandes stables et les options communes vers l’API native de son cœur.

- [x] 1. Établir le rangement exact sans changer le fonctionnement
  - [x] 1.1 Inventorier la propriété des fichiers Atari et Amiga
    - [x] Créer `docs/reference/emulator-adapter-file-map.md` avec, pour chaque fichier de `src/GWGUI.Emulation.Atari` et `src/GWGUI.Emulation.Amiga`, son propriétaire (`Common` ou un cœur précis), son chemin actuel et son chemin cible.
  - [x] 1.2 Figer les déplacements avant leur exécution
    - [x] Modifier `docs/tasks/emulation/emulator-adapter-organization.md` pour désigner chaque ligne de `docs/reference/emulator-adapter-file-map.md` dont les deux chemins diffèrent comme une action de déplacement exacte des points 3.2 ou 4.2. Les espaces de noms publics existants seront conservés : le rangement demandé est physique et ne doit pas provoquer une rupture d’API inutile.

- [x] 2. Compléter le contrat générique de choix du cœur
  - [x] 2.0 Partager les mêmes contrats entre toutes les familles
    - [x] Créer `src/GWGUI.Emulation/Contracts/EmulationMachineCreationContext.cs` avec le contexte de création commun à tous les adaptateurs de machines.
    - [x] Créer `src/GWGUI.Emulation/Interfaces/IEmulationMachineFactory.cs` avec l’identifiant générique du cœur et la création d’un `IEmulatedMachine` depuis une `IEmulationConfiguration`.
    - [x] Supprimer `src/GWGUI.Emulation.Atari/Interfaces/IAtariMachineFactory.cs` et `src/GWGUI.Emulation.Amiga/Interfaces/IAmigaMachineFactory.cs` après migration vers `IEmulationMachineFactory`.
    - [x] Supprimer `src/GWGUI.Emulation.Atari/Contracts/AtariMachineCreationContext.cs` et `src/GWGUI.Emulation.Amiga/Contracts/AmigaMachineCreationContext.cs` après migration vers `EmulationMachineCreationContext`.
  - [x] 2.1 Exposer les données nécessaires à l’App
    - [x] Créer `src/GWGUI.Emulation/Contracts/EmulationEmulatorDefinition.cs` avec l’identifiant, le nom invariant, la clé de description et les machines compatibles communs à toutes les familles.
    - [x] Modifier `src/GWGUI.Emulation/Contracts/EmulationEmulatorInstallation.cs` pour transporter la définition générique et l’état d’installation, sans référence à un type Atari, Amiga ou Amstrad.
    - [x] Modifier `src/GWGUI.Emulation/Interfaces/IEmulationEmulatorManager.cs` pour valider et retourner ces données génériques pour tous les cœurs compatibles avec la machine en cours de configuration.
  - [x] 2.2 Afficher les données sans connaître les cœurs
    - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Options/EmulationEmulatorManagementController.cs` pour afficher le nom fourni par le module, résoudre sa description avec `IEmulationModuleLocalization` et conserver la règle actuelle : un seul cœur par configuration, choix verrouillé après l’enregistrement.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationCoreManagementPanel.cs` pour afficher la description localisée et l’état d’installation du cœur sélectionné sans ajouter de dépendance vers les modules de famille.

- [x] 3. Séparer la gestion commune et les cœurs Atari
  - [x] 3.1 Définir les capacités internes communes
    - [x] Modifier `src/GWGUI.Emulation.Atari/Factories/AtariMachineFactory.cs` pour implémenter le contrat partagé `IEmulationMachineFactory` et traduire la configuration générique en configuration Atari.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Dictionaries/AtariCoreCatalog.cs` pour enregistrer les adaptateurs disponibles, leurs machines compatibles et leurs données génériques de présentation.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Services/AtariEngine.cs` pour ne sélectionner et utiliser les cœurs qu’au travers du catalogue et de `IEmulationMachineFactory`.
  - [x] 3.2 Ranger chaque implémentation dans son dossier
    - [x] Modifier `docs/reference/emulator-adapter-file-map.md` pour conserver les catégories Atari partagées comme gestion commune du module, sans les dupliquer sous un dossier `Common`.
    - [x] Déplacer les fichiers propres à Hatari dans `src/GWGUI.Emulation.Atari/Emulators/Hatari/` et modifier leurs espaces de noms et références.
    - [x] Déplacer les fichiers propres à Atari800 dans `src/GWGUI.Emulation.Atari/Emulators/Atari800/` et modifier leurs espaces de noms et références.
    - [x] Déplacer les fichiers propres à Stella dans `src/GWGUI.Emulation.Atari/Emulators/Stella/` et modifier leurs espaces de noms et références.
    - [x] Déplacer les fichiers propres à ProSystem dans `src/GWGUI.Emulation.Atari/Emulators/ProSystem/` et modifier leurs espaces de noms et références.
    - [x] Déplacer les fichiers propres à Beetle Lynx dans `src/GWGUI.Emulation.Atari/Emulators/BeetleLynx/` et modifier leurs espaces de noms et références.
    - [x] Déplacer les fichiers propres à Virtual Jaguar dans `src/GWGUI.Emulation.Atari/Emulators/VirtualJaguar/` et modifier leurs espaces de noms et références.
  - [x] 3.3 Conserver les commandes par machine
    - [x] Créer `src/GWGUI.Emulation.Atari/Emulators/README.md` pour imposer aux adaptateurs la traduction des commandes déjà définies par les catalogues de machines Atari, sans les redéfinir par cœur.

- [x] 4. Séparer la gestion commune et les cœurs Amiga
  - [x] 4.1 Définir les capacités internes communes
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Factories/PuaeMachineFactory.cs` pour implémenter le contrat partagé `IEmulationMachineFactory` et traduire la configuration générique en configuration Amiga.
    - [x] Créer `src/GWGUI.Emulation.Amiga/Dictionaries/AmigaCoreCatalog.cs` avec l’enregistrement des adaptateurs, leurs machines compatibles et leurs données génériques de présentation.
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Services/AmigaEngine.cs` pour supprimer la création directe de `PuaeMachineFactory` et sélectionner le cœur au travers de `IEmulationMachineFactory`.
  - [x] 4.2 Ranger PUAE derrière les interfaces communes
    - [x] Modifier `docs/reference/emulator-adapter-file-map.md` pour conserver les catégories Amiga partagées comme gestion commune du module, sans les dupliquer sous un dossier `Common`.
    - [x] Déplacer les fichiers propres à PUAE dans `src/GWGUI.Emulation.Amiga/Emulators/PUAE/` et modifier leurs espaces de noms et références.
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Emulators/PUAE/Factories/PuaeMachineFactory.cs` pour fournir la création du runtime et la traduction de la configuration par les interfaces communes de `GWGUI.Emulation`.
  - [x] 4.3 Conserver les commandes par machine
    - [x] Créer `src/GWGUI.Emulation.Amiga/Emulators/README.md` pour imposer aux adaptateurs la traduction des commandes déjà définies par les modèles Amiga, sans les redéfinir par cœur.

- [x] 5. Appliquer le même modèle au squelette Amstrad
  - [x] 5.1 Préparer les emplacements communs
    - [x] Créer `src/GWGUI.Emulation.Amstrad/Common/README.md` avec la liste des contrats communs de `GWGUI.Emulation` que la future gestion Amstrad devra utiliser.
    - [x] Créer `src/GWGUI.Emulation.Amstrad/Emulators/README.md` avec la règle d’ajout d’un cœur : créer un dossier à son nom, traduire ses entrées, sorties et options vers les interfaces communes, puis l’enregistrer dans le catalogue Amstrad.

- [x] 5.2 Rétablir la couche de traduction des familles
  - [x] 5.2.1 Retirer la connexion directe des cœurs vers Emulation
    - [x] Supprimer `src/GWGUI.Emulation/Interfaces/IEmulationMachineFactory.cs` après remplacement par les adaptateurs internes de famille.
    - [x] Supprimer `src/GWGUI.Emulation/Contracts/EmulationMachineCreationContext.cs` après remplacement par les contextes internes de famille.
  - [x] 5.2.2 Créer la prise interne Atari
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Interfaces/IEmulatorAdapter.cs` avec l’identité du cœur et la création depuis les types internes Atari uniquement.
    - [x] Créer `src/GWGUI.Emulation.Atari/Common/Contracts/EmulatorCreationContext.cs` avec les services transmis par la gestion commune Atari aux adaptateurs.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Services/AtariEngine.cs` pour être la couche commune qui sélectionne un `IEmulatorAdapter` et renvoie le résultat vers les contrats de `GWGUI.Emulation`.
  - [x] 5.2.3 Créer la prise interne Amiga
    - [x] Créer `src/GWGUI.Emulation.Amiga/Common/Interfaces/IEmulatorAdapter.cs` avec l’identité du cœur et la création depuis les types internes Amiga uniquement.
    - [x] Créer `src/GWGUI.Emulation.Amiga/Common/Contracts/EmulatorCreationContext.cs` avec les services transmis par la gestion commune Amiga aux adaptateurs.
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Services/AmigaEngine.cs` pour être la couche commune qui sélectionne un `IEmulatorAdapter` et renvoie le résultat vers les contrats de `GWGUI.Emulation`.
  - [x] 5.2.4 Aligner les namespaces physiques
    - [x] Modifier tous les fichiers sous `src/GWGUI.Emulation.Atari/Emulators/<cœur>/` pour employer `GWGUI.Emulation.Atari.Emulators.<cœur>.<catégorie>` et mettre à jour leurs consommateurs.
    - [x] Modifier tous les fichiers sous `src/GWGUI.Emulation.Amiga/Emulators/PUAE/` pour employer `GWGUI.Emulation.Amiga.Emulators.PUAE.<catégorie>` et mettre à jour leurs consommateurs.

- [x] 6. Gérer les descriptions localisées des cœurs
  - [x] 6.1 Conserver les noms invariants dans les catalogues
    - [x] Modifier `src/GWGUI.Emulation.Atari/Dictionaries/AtariCoreCatalog.cs` pour fournir les noms invariants des cœurs et leurs clés de descriptions localisées.
    - [x] Créer `src/GWGUI.Emulation.Amiga/Dictionaries/AmigaCoreCatalog.cs` avec le nom invariant de PUAE et sa clé de description localisée.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/00-Base/Emulation.resx` et `src/GWGUI.Emulation.Amiga/Resources/00-Base/Emulation.resx` pour ajouter uniquement les descriptions sources.
  - [x] 6.2 Traduire uniquement les descriptions
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/en-US/Emulation.resx` et `src/GWGUI.Emulation.Amiga/Resources/en-US/Emulation.resx` pour y copier toutes les clés traduisibles déjà présentes dans les cultures avant la synchronisation Argos.
    - [x] Modifier tous les fichiers `src/GWGUI.Emulation.Atari/Resources/<culture>/Emulation.resx` avec les descriptions traduites par `scripts/tools/translate-resx-argos.py`, sans dupliquer les noms invariants des cœurs.
    - [x] Modifier tous les fichiers `src/GWGUI.Emulation.Amiga/Resources/<culture>/Emulation.resx` avec la description PUAE traduite par `scripts/tools/translate-resx-argos.py`, sans dupliquer son nom invariant.

- [x] 7. Vérifier l’architecture et les comportements conservés
  - [x] 7.1 Vérifier les frontières de dépendances
    - [x] Créer `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` pour vérifier que `GWGUI.App` et `GWGUI.Emulation` ne référencent aucun module de famille et que les prises internes Atari et Amiga possèdent les mêmes noms et membres.
  - [x] 7.2 Vérifier la sélection et les adaptateurs
    - [x] Modifier `tests/GWGUI.Tests/Interface/SettingsViews/EmulationModuleSettingsNavigationScenarios.cs` pour utiliser les définitions génériques du sélecteur tout en vérifiant le choix unique et son verrouillage après enregistrement.
    - [x] Créer `tests/GWGUI.Tests/Emulation/EmulatorManagerTests.cs` pour vérifier les cœurs compatibles retournés par machine, leurs données de présentation et leur état d’installation.
    - [x] Créer `tests/GWGUI.Tests/Emulation/Atari/AtariEmulatorAdapterTests.cs` pour vérifier les namespaces physiques des adaptateurs et la concordance entre le catalogue Atari et les adaptateurs enregistrés.
    - [x] Créer `tests/GWGUI.Tests/Emulation/Amiga/AmigaEmulatorAdapterTests.cs` pour vérifier que PUAE utilise son namespace physique et implémente uniquement la prise interne Amiga.
  - [x] 7.3 Consigner les vérifications exécutées
    - [x] Modifier `docs/tasks/emulation/emulator-adapter-organization.md` avec les commandes et résultats des compilations, audits Argos et tests ciblés après leur réussite.

Résultats :

- `dotnet build tests/GWGUI.Tests/GWGUI.Tests.csproj --no-restore` réussit sans avertissement ni erreur.
- Les 8 tests ciblés d’architecture, de catalogues, d’adaptateurs, de sélection et de cycle de vie réussissent.
- L’audit Argos réussit pour les 28 cultures Atari (2 268 entrées) et les 28 cultures Amiga (448 entrées).
