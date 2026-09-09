# Validation du rendu des machines masquées

## Protocole

Comparer avant/après sur le même PC, mêmes machines, mêmes réglages par instance,
même taille de fenêtre. Pour chaque palier, laisser stabiliser 10 secondes puis mesurer
30 secondes ; séparer CPU du processus App, CPU des moteurs, GPU App et mémoire.
Les captures utilisateur ci-dessous sont historiques et ponctuelles, pas des moyennes.

| Étape | Machines actives | Onglet | CPU total historique | GPU historique |
|---|---:|---|---:|---:|
| 1 | 0 | Machine arrêtée | 0,1 % | 0 % |
| 2 | 0 | Quatre ouvertes | 0,2 % | 0,2 % |
| 3 | 1 | Amiga 1200 | 6,7 % | 0,1 % |
| 4 | 2 | CD32 | 13,7 % | 2,9 % |
| 5 | 2 | Amiga 1200 | 14,8 % | 6,2 % |
| 6 | 3 | Atari 800XL | 17,6 % | 4 % |
| 7 | 3 | Amiga 1200 | 17,7 % | 15 % |
| 8 | 3 | Lecture | 18,1 % | 12,5 % |
| 9 | 4 | Atari ST | 19,6 % | 5,7 % |
| 10 | 4 | Amiga 1200 | 19,7 % | 12,9 % |
| 11 | 0 | Toutes arrêtées | 0,7 % | 0,5 % |

Les démarrages sont cumulatifs jusqu'à l'étape 11. Les shaders diffèrent entre instances,
mais n'ont pas été modifiés pendant ces captures.

## Mesures utilisateur après modification

Les machines sont démarrées cumulativement : Amiga 1200 avec PUAE, Amiga CD32 avec une
seconde instance de PUAE, Atari 800XL avec Atari800, puis Atari ST avec Hatari. Les shaders
sont configurés par instance avant les relevés et ne changent qu'à l'étape 15, où les
effets de l'Atari 800XL sont remis à zéro. Ces captures restent des valeurs ponctuelles.

| Étape | Machines actives | Onglet visible | CPU total | CPU App | GPU App | Mémoire totale |
|---|---:|---|---:|---:|---:|---:|
| 1 | 0 | Quatre machines prêtes | 0,5 % | 0,5 % | 0,1 % | 79,6 Mo |
| 2 | 1 | Amiga 1200 | 8,1 % | 4,2 % | 0,1 % | 277,5 Mo |
| 3 | 1 | Lecture | 7,7 % | 3,7 % | 0,3 % | 298,2 Mo |
| 4 | 1 | Amiga CD32 arrêtée | 6,9 % | 3,1 % | 0,3 % | 289,4 Mo |
| 5 | 2 | Amiga CD32 | 13,5 % | 5,5 % | 7,8 % | 399,1 Mo |
| 6 | 2 | Atari 800XL arrêté | 13,0 % | 5,6 % | 0,4 % | 400,5 Mo |
| 7 | 3 | Atari 800XL | 15,0 % | 7,4 % | 5,7 % | 432,9 Mo |
| 8 | 3 | Amiga 1200 | 16,8 % | 7,9 % | 0,2 % | 433,3 Mo |
| 9 | 3 | Amiga CD32 | 14,5 % | 6,6 % | 5,8 % | 431,7 Mo |
| 10 | 4 | Atari ST | 18,3 % | 9,9 % | 2,5 % | 475,6 Mo |
| 11 | 4 | Amiga CD32 | 18,2 % | 9,5 % | 8,7 % | 480,0 Mo |
| 12 | 4 | Accueil Émulation | 18,7 % | 9,1 % | 0,2 % | 479,8 Mo |
| 13 | 4 | Atari ST | 19,4 % | 9,8 % | 4,8 % | 472,2 Mo |
| 14 | 4 | Atari 800XL | 19,2 % | 10,7 % | 4,9 % | 475,1 Mo |
| 15 | 4 | Atari 800XL, effets à zéro | 19,6 % | 10,6 % | 5,6 % | 536,8 Mo |

Le GPU du processus principal retombe à 0,4 % sur l'onglet Atari 800XL arrêté avec
deux machines actives, puis à 0,2 % sur Accueil Émulation avec les quatre machines
toujours actives. Cela confirme que la présentation GPU des machines masquées est
suspendue tout en laissant leurs cœurs fonctionner. Le retour sur chaque onglet rétablit
son rendu avec ses propres effets.

Avec quatre machines actives et aucune vidéo visible, le CPU reste à 18,7 % : environ
9,5 % pour les quatre processus moteurs réunis et 9,1 % pour le processus principal.
Cette consommation CPU distincte du rendu GPU motive le point 4 de la feuille de tâches.

## Vérifications effectuées

- Tests ciblés : 44 réussites, aucun échec ni test ignoré, avec le filtre
  `FullyQualifiedName~HiddenMachineRenderingTests|FullyQualifiedName~Emulation.Video.VideoTests|FullyQualifiedName~EmulationViewsTests`.
  Six nouveaux cas autonomes sont conservés : masquage/reprise WPF et GPU, transfert plein
  écran et minimisation, frame GPU engagée avec abandon de la frame en attente, remise à zéro
  de l'entrelacement logiciel et suspension de sa file de travail. Ils utilisent les objets
  factices et le dispatcher de tests existants ; les fenêtres de test sont fermées.
- Essai temporaire avec les surfaces réelles : 3 réussites (Vulkan, Direct3D 11, OpenGL).
  Compilation initiale avec masque de chargement, absence de présentation des frames masquées,
  reprise sur la frame courante et identité du pipeline/programme avant/après vérifiées.
  Aucune machine ni DLL de cœur d'émulation n'a été démarrée par cet essai.
  `TemporaryHiddenMachineGpuTests.cs` a été supprimé après exécution.
- Première série : 41 réussites et un échec dans le repli GPU vers WPF à cause d'une double
  présentation introduite par la reprise automatique ; correction dans le présentateur.
  Un littéral de ratio du nouveau test a également été corrigé en float avant les résultats finaux.

Les essais synthétiques ont ensuite été complétés par la validation utilisateur sur les
quatre machines réelles. Le changement d'onglet, Lecture, Accueil Émulation, le retour sur
les machines et le plein écran restent fonctionnels ; les machines masquées continuent de
tourner et retrouvent leur affichage sans rechargement visible de leurs shaders.

Build Debug via scripts/build.ps1 : réussi ; build/Debug/GW GUI/gwgui.exe vérifié.

Contrôle supplémentaire des onglets et du plein écran sur surfaces GPU réelles : 3 réussites
(Vulkan, Direct3D 11, OpenGL). Un TabControl alterne machine et Lecture ; Screen est ensuite
déplacé vers une autre fenêtre puis remis dans son conteneur. Le pipeline/programme reste
le même objet et la présentation reprend. `TemporaryHiddenMachineTabTests.cs` est supprimé.

Nettoyage final : les deux tests GPU temporaires sont absents ; aucun script, jeu, firmware,
capture ou fichier de données n'a été ajouté par ces essais. Après leur suppression, la DLL
de tests a été recompilée et les 44 tests ciblés ont de nouveau réussi (aucun échec/ignoré).
Les sorties habituelles de compilation restent dans les dossiers ignorés.

## Conclusion

La suspension du rendu des machines masquées est validée dans le paquet Debug avec les
machines réelles. Le gain GPU attendu est visible lorsque l'onglet actif n'affiche aucune
machine en fonctionnement. L'analyse et la réduction éventuelle du CPU restent un chantier
séparé inscrit au point 4.

Aucun commit ni push effectué pour cette modification.
