# Adaptateurs Atari

Les ports et commandes disponibles sont définis par le modèle Atari dans les catalogues et fonctions
communs du module. Ils ne sont jamais définis par un cœur.

Chaque dossier de cœur contient uniquement son implémentation et traduit les commandes stables de la
machine vers ses boutons, axes, touches, souris, paddles ou trackballs natifs. Une nouvelle implémentation
doit utiliser `Common/Interfaces/IEmulatorAdapter.cs`, jamais communiquer directement avec
`GWGUI.Emulation`, et fournir elle-même son identité, sa définition, sa DLL, sa source et ses données
d’installation. `Common/Dictionaries/EmulatorCatalog.cs` découvre ensuite les adaptateurs sans
connaître en dur un cœur concret.
