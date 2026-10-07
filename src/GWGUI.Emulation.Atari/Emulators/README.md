# Adaptateurs Atari

Les ports et commandes disponibles sont définis par le modèle Atari dans les catalogues et fonctions
communs du module. Ils ne sont jamais définis par un cœur.

Chaque dossier de cœur contient uniquement son implémentation et traduit les commandes stables de la
machine vers ses boutons, axes, touches, souris, paddles ou trackballs natifs. Une nouvelle implémentation
doit utiliser `Common/Interfaces/IEmulatorAdapter.cs`, jamais communiquer directement avec
`GWGUI.Emulation`, et fournir elle-même son identité, sa définition, sa DLL, sa source et ses données
d’installation. `Common/Dictionaries/EmulatorCatalog.cs` découvre ensuite les adaptateurs sans
connaître en dur un cœur concret.

L’Atari 2600 propose trois profils distincts : `Stella`, `Stella2014` et
`Stella2023`, avec les fichiers `stella_libretro.dll`, `stella2014_libretro.dll`
et `stella2023_libretro.dll`. Ils ne sont proposés pour aucune autre machine.
Le profil auparavant nommé `Stella` était Stella 2023 : son dossier et son
adaptateur portent maintenant le nom `Stella2023`. Sa valeur numérique enregistrée
reste identique, ainsi que son identifiant d’installation `stella2023`.

`Emulators/Common/Interfaces/IEmulatorCore.cs` définit le contrat d’exécution.
L’hôte commun reçoit une fabrique de ce contrat et le transport reçoit la commande
de lancement fournie par l’adaptateur. Le module utilise `IEmulatorAdapter` pour
les options, les extensions, la sélection et l’installation. Un autre backend peut
implémenter ces contrats sans utiliser libretro.

`Emulators/Common/Interop` contient les appels natifs, callbacks, probes et
services d’installation des cœurs actuels. Les contrats de frames, audio, entrées,
médias, états et messages restent dans `Common`. Les descriptions des trois
profils 2600 partagent la ressource déjà traduite dans toutes les langues.

Sources : [Stella](https://github.com/stella-emu/stella),
[Stella 2014](https://github.com/libretro/stella2014-libretro),
[Stella 2023](https://github.com/libretro/stella2023).

L'Atari 5200 propose aussi [a5200](https://github.com/libretro/a5200), via
`a5200_libretro.dll`, pour les cartouches `.a52` et `.bin`. Atari800 reste
le profil par défaut des configurations existantes. Le BIOS utilisateur
sélectionné est copié par le service de firmware commun; sans sélection,
l'adaptateur choisit le BIOS Altirra intégré. Les options natives restent
configurables par le mécanisme commun. Ce cœur utilise deux contrôleurs,
dont le premier avec le pavé à boutons directs; les deux autres ports
matériels ne sont pas pris en charge par a5200.

[HatariB](https://github.com/bbbradsmith/hatariB) est propose pour ST, STf,
STfm, Mega ST, STE, Mega STE, TT et Falcon, via `hatarib_libretro.dll`.
Hatari reste le profil par defaut. Les deux profils reutilisent la preparation
commune des disquettes, disques durs et dossiers GEMDOS; leurs conversions
vers les options natives restent distinctes. HatariB utilise EmuTOS integre
sans TOS utilisateur, ou le TOS selectionne dans la configuration. Ses options
natives sont exposees par le mecanisme commun existant. Les images IPF et CTR
necessitent la bibliotheque CAPS optionnelle du coeur.

[Hatari 2014](https://github.com/libretro/hatari/tree/hitari2014-mercurial),
via `hatari2014_libretro.dll`, est propose pour les memes huit modeles ST,
STf, STfm, Mega ST, STE, Mega STE, TT et Falcon. Il exige un TOS utilisateur
et un media de demarrage. Hatari reste le profil par defaut. Les conversions
des options `hatari_*`, la preparation des medias et les indicateurs de lecteurs
sont partages avec Hatari dans `Emulators/Common/Interop`; les contrats generaux
restent utilisables par un backend non libretro. La description des machines
reutilise la ressource ST deja traduite dans toutes les langues.
