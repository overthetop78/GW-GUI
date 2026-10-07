# Émulateurs Amstrad

Les adaptateurs `Caprice32` et `CrocoDS` implémentent
`Common/Interfaces/IEmulatorAdapter.cs`. L'application utilise les contrats génériques
du module pour sélectionner, installer et configurer chaque émulateur.

`Emulators/Common` contient les contrats, le transport par processus isolé et le
téléchargement partagés. `CoreHost` exécute la fabrique de `IEmulatorCore` fournie
par l'adaptateur ; `ProcessCore` reçoit sa commande de lancement explicitement.
Ces services ne choisissent aucun profil et n'appellent aucune API libretro.

Chaque adaptateur fournit sa `CoreDefinition` : identité, fichier, URL officielle,
commande d'hébergement, libellés de publication, architecture et validation
facultative du fichier installé. Chaque installation reste dans `Core/<id>`.

`Emulators/Common/Interop` contient l'implémentation native utilisée par Caprice32
et CrocoDS : appels libretro, callbacks, lecture des options et validation Windows
x64. Un adaptateur utilisant un autre backend implémente `IEmulatorCore` et fournit
sa propre fabrique à l'hôte, sans utiliser ces services natifs. Le contrat reste
celui d'un cœur exécuté par frames, avec vidéo, audio, entrées, médias et états.
La machine commune utilise la capacité de capture du pointeur déclarée par le cœur.

Le `Common` racine relie ces services aux contrats du module :
`Common/Services/Engine` sélectionne l'adaptateur, et
`Common/Interfaces/IEmulatorAdapter.GetOptions` fait remonter ses options installées.
Le module ne charge pas directement un cœur pour construire ses réglages.

Caprice32 conserve les machines et options `cap32_*` existantes. CrocoDS est proposé
pour le CPC 6128 : ROM intégrée, RAM fixe de 128 KiB, clavier et contrôleurs,
un seul contenu DSK/SNA/KCR. Le cœur CrocoDS ne fournit pas le contrôle des disques
libretro ; changer le contenu recrée donc la machine.

CrocoDS indique `need_fullpath=false`. L'hôte lui transmet les octets du contenu et
garde le buffer vivant jusqu'au déchargement. Les options de configuration sont
lues dans les déclarations de la DLL installée, sans initialiser l'émulation.
Les options natives et leurs valeurs sont persistées puis transmises au cœur.

La DLL officielle vérifiée le 7 octobre 2026 ne déclare aucune option. Les trois
options historiques documentées par Libretro (`crocods_greenmonitor`,
`crocods_resize`, `crocods_hack`) sont commentées dans le code officiel actuel.
Leurs libellés et choix sont disponibles si une DLL les déclare ; l'interface ne
les affiche pas pour une version qui ne les prend pas en charge.

Sources : [code CrocoDS](https://github.com/libretro/libretro-crocods/blob/master/libretro.c),
[documentation Libretro](https://docs.libretro.com/library/crocods/).
