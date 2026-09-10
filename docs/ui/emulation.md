# Interface d’émulation et gestion des modules

## Menu Émulation

Le menu principal **Émulation** est distinct du menu **Options**. Il contient :

- **Préférences d’émulation…**, qui ouvre les réglages communs ;
- une entrée ajoutée automatiquement pour chaque module chargé, par exemple **Amiga** ou **Atari**.

GW GUI ne contient aucune entrée de famille codée en dur. Installer ou retirer un module modifie le
menu au prochain démarrage.

## Préférences communes

La fenêtre **Préférences d’émulation** contient uniquement :

- **Général** pour les dossiers communs des émulateurs, captures et états sauvegardés ;
- **Raccourcis** pour les commandes communes ;
- **Configurations** pour la table réunissant les configurations de tous les modules installés.

La table permet de filtrer, sélectionner, ouvrir et supprimer une configuration sans dupliquer ces
fonctions dans chaque module.

## Paramètres d’un module

Chaque entrée de module ouvre une fenêtre indépendante intitulée **Paramètres d’émulation {module}**.
La fenêtre utilise le même contrôle hôte générique, alimenté par les données du module :

- les machines apparaissent dans une liste verticale ;
- une machine ayant une configuration enregistrée reçoit l’état visuel prévu ;
- la partie droite affiche directement ses onglets Général, CPU, RAM, ROM, Vidéo, Audio, Stockage,
  Clavier, Souris ou Manettes selon les capacités annoncées ;
- l’onglet Général présente le moteur associé et permet de l’installer lorsqu’il manque ;
- les champs de firmware apparaissent seulement lorsque le moteur accepte une ROM obligatoire ou
  optionnelle. Un onglet ROM sans fichier utilisateur possible est masqué.

Changer de machine conserve l’onglet applicable et recharge les réglages, firmwares, supports,
entrées, moteur et profil vidéo de la machine choisie.

## Mises à jour et modules disponibles

**Options > Mises à jour** ouvre une fenêtre dédiée. Sa navigation sépare :

- la mise à jour de GW GUI ;
- les modules disponibles dans le répertoire officiel ;
- chaque module installé et sa propre recherche de mise à jour ;
- l’installation avancée depuis un ZIP ou une URL de catalogue.

Télécharger un module ne redémarre pas immédiatement l’application. Son badge devient
**Téléchargé**. Plusieurs téléchargements peuvent être préparés, puis le bouton de pied de fenêtre
devient **Finir l’installation**. Une seule transaction installe tous les modules et redémarre GW GUI
une seule fois. Après reprise, leur badge est **Installé**.

Le bouton de fermeture `X` demande si l’installation doit être terminée. Refuser annule les
téléchargements préparés et ferme la fenêtre.

Installer un module ajoute ses machines et son intégration à GW GUI. Les cœurs Libretro et les
firmwares nécessaires sont gérés séparément dans les paramètres d’émulation ; les firmwares restent
fournis légalement par l’utilisateur.

## Vidéo et focus

La présentation vidéo appartient à l’hôte et reste indépendante des modules. Les réglages sont
enregistrés par couple `(ModuleId, ConfigurationId)`. Les modules produisent les images brutes ;
OpenGL, Direct3D, Vulkan ou le repli WPF appliquent ensuite les traitements communs.

Le focus est rendu à la surface d’émulation lors des changements d’onglet, de surface ou de plein
écran. La capture de souris reste limitée aux machines et réglages qui la demandent.

Lorsqu’une surface n’est plus visible, sa présentation est suspendue sans arrêter la machine,
l’audio ni les entrées. La dernière image est reprise sans accumuler les anciennes frames.
