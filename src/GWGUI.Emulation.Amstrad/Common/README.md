# Gestion commune Amstrad

Le module Amstrad doit utiliser directement les contrats génériques de `GWGUI.Emulation` :

- `IEmulationModule` pour le catalogue des machines et les configurations ;
- `IEmulationEmulatorManager` pour le choix, la recherche et l’installation des cœurs ;
- `IEmulatedMachine` et ses interfaces de capacités pour les entrées, médias, vidéo, audio, états et cycle de vie ;
- `EmulationEmulatorDefinition` et `EmulationEmulatorInstallation` pour les données communes.

La gestion commune du module doit exposer aux cœurs ses propres fichiers
`Common/Interfaces/IEmulatorAdapter.cs` et `Common/Contracts/EmulatorCreationContext.cs`. Ces fichiers
gardent exactement les mêmes noms, rôles et emplacements dans chaque projet `GWGUI.Emulation.Xxxx`.

Une interface propre à Amstrad ne doit être créée que pour une donnée matérielle qui ne peut pas être
représentée par ces contrats. Elle reste alors interne au module et ne remplace jamais un contrat générique.
