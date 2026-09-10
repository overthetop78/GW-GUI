# Tâches de GW GUI

Ce dossier contient uniquement le travail encore ouvert. Les résultats terminés sont transférés dans
la documentation durable puis leur feuille est supprimée. Les règles permanentes se trouvent dans
[`../project/rules.md`](../project/rules.md) et l’organisation documentaire dans
[`../project/documentation.md`](../project/documentation.md).

## Ordre obligatoire du chantier média

Les feuilles suivantes forment une seule séquence. Une feuille ne commence que lorsque toutes les
cases de la précédente sont cochées et que son éventuel point de contrôle Git demandé par
l'utilisateur est terminé.

1. [Orchestration commune des formats et représentations](media-format-orchestration.md).
2. [Extension aux HDD, supports optiques, cassettes et bandes](media-exploration.md), points 1 à 7.
3. Premier commit demandé dans `media-exploration.md`.
4. [Affichage graphique de tous les supports](media-exploration.md), point 8.
5. Deuxième commit demandé dans `media-exploration.md`.
6. [Création et cycle de vie des images HDD](hard-disk-images.md), avec les compléments qui restent
   après l'orchestration, l'exploration et l'affichage des HDD, CD/DVD/optiques et cassettes/bandes.
7. Troisième commit demandé dans `hard-disk-images.md`.
8. [Validation finale](validation.md), en commençant par les essais manuels du corpus `image_test`.

Avant chaque action, vérifier qu'elle est la première case non cochée de cette séquence. Si une
action nécessaire manque, l'ajouter à la suite de la dernière action cochée et avant l'action qui en
dépend. Si l'ordre restant est faux, le corriger avant toute modification du code. Réécrire une
action imprécise avant de l'exécuter et supprimer une action devenue fausse ou sans objet.

## Travail à reprendre après validation ou disponibilité

- [Publications restantes](release.md) — prochaines publications de l’application, du module Atari et, si son contrat évolue, du SDK.
- [Atari](emulation/atari.md) — validations, accessibilité, Jaguar CD, guide et étude Atari System 1; reporté.
- [Validations d’émulation](emulation/remaining-validations.md) — Amiga, cassette Atari800, GameInput, CPU masqué et distribution indépendante; reporté.
- [Contrôleurs d’émulation](interface/emulation/controllers.md) — régressions visuelles, associations et validations matérielles; reporté.
- [Validation finale](validation.md) — corpus `image_test`, Greaseweazle et entrées/sorties internes;
  commence seulement après le troisième commit du chantier média.

## Plans facultatifs ou différés par décision

- [Visuels matériels supplémentaires](interface/controller-artwork-backlog.md) — backlog facultatif.
- [Organisation GitHub et suivi public](project/github-public-project.md) — changement important explicitement reporté.

Chaque feuille conserve son ordre d’exécution. Une case est cochée seulement après réalisation et
vérification de l’action correspondante.

La présence d’une action `commit`, `push` ou création de tag dans une feuille ne l’autorise pas. Ces
opérations Git sont exécutées uniquement après une demande explicite de l’utilisateur; une demande
explicite de publication autorise les opérations prévues par sa procédure.
