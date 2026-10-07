# Adaptateurs Amiga

Trois profils sont disponibles : PUAE (`puae_libretro.dll`), PUAE 2021
(`puae2021_libretro.dll`) et Amiberry (`amiberry_libretro.dll`). Le profil
preexistant est PUAE actuel ; il reste celui utilise par defaut. Son enum
porte desormais le nom PUAE ; aucun alias External n est conserve.

Les contrats d execution se trouvent dans `Emulators/Common/Interfaces`,
relies aux contrats et services de `Common` a la racine du module Commodore.
Ils restent implementables par un backend non libretro. L hote natif,
les callbacks, le transport et l installation sont dans
`Emulators/Common/Interop`. Chaque profil conserve ses metadonnees et sa
fabrique ; PUAE et PUAE 2021 partagent leur conversion native UAE.

La selection et l installation passent par la configuration du profil choisi.
Les options publiees par le coeur sont exposees par l hote existant.
Le Kickstart, la ROM etendue et rom.key restent selectionnables via les
champs ROM existants. Amiberry recoit le Kickstart choisi et ses noms de
ROM etendue CD32/CDTV, qui different de ceux des deux variantes PUAE.
Ses configurations UAE permettent aussi de preparer le demarrage sans
disque, les machines A2000/A3000 et les medias montes simultanement.

Sources : [PUAE](https://github.com/libretro/libretro-uae),
[PUAE 2021](https://github.com/libretro/libretro-uae/tree/2.6.1),
[Amiberry](https://github.com/BlitterStudio/amiberry).

Les ports, boutons, touches et boutons de souris disponibles sont définis par le modèle Amiga dans la
gestion commune du module. Ils ne sont jamais définis par un cœur.

Chaque dossier de cœur traduit ces commandes stables vers son API native. Une nouvelle implémentation
doit utiliser `Common/Interfaces/IEmulatorAdapter.cs`, jamais communiquer directement avec
`GWGUI.Emulation`, et fournir elle-même son identité, sa définition, sa DLL, sa source et ses données
d’installation. `Common/Dictionaries/EmulatorCatalog.cs` découvre ensuite les adaptateurs sans
connaître en dur PUAE ou un autre cœur concret.
