# Adaptateurs Amiga

Les ports, boutons, touches et boutons de souris disponibles sont définis par le modèle Amiga dans la
gestion commune du module. Ils ne sont jamais définis par un cœur.

Chaque dossier de cœur traduit ces commandes stables vers son API native. Une nouvelle implémentation
doit utiliser `Common/Interfaces/IEmulatorAdapter.cs`, jamais communiquer directement avec
`GWGUI.Emulation`, et fournir elle-même son identité, sa définition, sa DLL, sa source et ses données
d’installation. `Common/Dictionaries/EmulatorCatalog.cs` découvre ensuite les adaptateurs sans
connaître en dur PUAE ou un autre cœur concret.
