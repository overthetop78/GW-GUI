# Affichage des processus d'émulation dans Windows

## Diagnostic du 8 septembre 2026

Relevé effectué avec les machines Atari ST et Amiga 1200 démarrées par l'utilisateur.

| PID observé | Commande / rôle | Description du fichier | Fenêtre visible |
|---|---|---|---|
| 43100 | Interface principale | `gwgui` | `GW GUI` |
| 47376 | `--amiga-core-host` | `gwgui` | Aucune |
| 49184 | `--atari-core-host` | `gwgui` | Aucune |

Les deux moteurs ont le PID 43100 comme parent, utilisent le même chemin
`build/Debug/GW GUI/gwgui.exe` et ne présentent que des fenêtres techniques invisibles
sans titre applicatif. Leurs lancements utilisent tous deux `UseShellExecute = false`
et `CreateNoWindow = true`.

La capture du Gestionnaire des tâches faite pendant ce relevé montre le groupe
`gwgui (4)` développé avec les quatre lignes suivantes :

- `GameInputRawInputProxy.exe` ;
- `GW GUI` ;
- `gwgui` ;
- `gwgui`.

Dans ce relevé, les deux moteurs sont donc affichés avec le même nom. La ligne
`GW GUI` est la fenêtre principale, et non le nom d'un moteur Atari. La description
`gwgui` vient du même fichier exécutable pour les trois processus. Le compteur du
groupe inclut également le processus auxiliaire GameInput : ce n'est pas un compteur
de machines démarrées.

La différence Amiga/Atari signalée dans les captures précédentes n'est pas reproduite
ici. Ces anciennes captures ne comportent pas les PID : elles ne permettent pas
d'attribuer rétrospectivement chaque ligne à un moteur ni d'établir la raison exacte
du regroupement à cet instant. Les réponses antérieures attribuant cette différence
aux fenêtres propres aux deux moteurs étaient insuffisamment fondées.

L'interface UI Automation du Gestionnaire des tâches n'a exposé que huit éléments
sans les lignes de processus. Le diagnostic visuel repose donc sur une capture,
complétée par les commandes/PID et l'énumération des fenêtres Win32 en lecture seule.

## Code concerné

- `src/GWGUI.Launcher/GWGUI.Launcher.csproj` définit l'exécutable commun `gwgui`.
- `src/GWGUI.App/App.xaml.cs` traite les commandes hôtes avant de créer la fenêtre principale.
- `src/GWGUI.Emulation.Amiga/Services/AmigaProcessCore.cs` lance le processus Amiga.
- `src/GWGUI.Emulation.Atari/Services/AtariProcessCore.cs` lance le processus Atari.
- `src/GWGUI.Emulation.Amiga/Services/AmigaCoreHost.cs` et
  `src/GWGUI.Emulation.Atari/Services/AtariCoreHost.cs` hébergent les moteurs sans fenêtre visible.

## Noms des machines dans la liste : différé

La demande vise le vrai nom de chaque machine démarrée, pas seulement sa famille.
Modifier la description du lanceur commun donnerait le même texte à tous ses processus.
Modifier le titre de la fenêtre principale ne renommerait pas les processus moteurs.
Les fenêtres techniques invisibles ne constituent pas une solution établie pour obtenir
une ligne visible nommée par machine dans le Gestionnaire des tâches.

Aucune adaptation simple et fiable de ces lignes n'a été établie avec le fonctionnement
actuel. La modification est laissée de côté conformément à la demande « sauf si c'est trop
complexe ». Aucun faux affichage de fenêtre, duplication d'exécutable par machine ou
changement des moteurs n'est introduit. Une future solution devra être validée dans le
Gestionnaire des tâches lui-même, et reprendre le vrai nom de la configuration démarrée.

Références Microsoft : [classement selon les fenêtres visibles](https://devblogs.microsoft.com/oldnewthing/20171219-00/?p=97606),
[distinction entre PID, nom du processus et titre de fenêtre](https://learn.microsoft.com/en-us/windows-hardware/drivers/debugger/finding-the-process-id).
Ces références décrivent les éléments d'affichage ; elles ne prouvent pas la cause du
regroupement particulier des anciennes captures.

Aucun code de production modifié, aucun build nécessaire, aucun test permanent ajouté.
Le script et la capture temporaires sont supprimés à la fin du diagnostic. Aucun commit ni push.

L'analyse distincte de la consommation CPU de l'hôte et des moteurs reste différée. Elle ne doit pas
être confondue avec ce diagnostic des noms et du regroupement affichés par le Gestionnaire des tâches.
