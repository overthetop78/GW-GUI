# Émulateurs Amstrad

La première version contient uniquement `Emulators/Caprice32`.

Cet adaptateur possède l’identité du cœur, `cap32_libretro.dll`, le téléchargement, le protocole
d’hébergement, les callbacks Libretro, les options `cap32_*`, les règles de média et ses erreurs.
Il implémente `Common/Interfaces/IEmulatorAdapter.cs` ; ni `Common`, ni `Modules`, ni l’App ne
référencent directement un type Caprice32.

Les autres émulateurs et les autres familles Amstrad seront ajoutés dans des chantiers séparés.
