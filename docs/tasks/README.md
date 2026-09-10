# Tâches de GW GUI

Ce dossier contient uniquement le travail encore ouvert. Les résultats terminés sont transférés dans
la documentation durable puis leur feuille est supprimée. Les règles permanentes se trouvent dans
[`../project/rules.md`](../project/rules.md) et l’organisation documentaire dans
[`../project/documentation.md`](../project/documentation.md).

## Travail à reprendre après validation ou disponibilité

- [Publications restantes](release.md) — prochaines publications de l’application, du module Atari et, si son contrat évolue, du SDK.
- [Atari](emulation/atari.md) — validations, accessibilité, Jaguar CD, guide et étude Atari System 1; reporté.
- [Validations d’émulation](emulation/remaining-validations.md) — Amiga, cassette Atari800, GameInput, CPU masqué et distribution indépendante; reporté.
- [Contrôleurs d’émulation](interface/emulation/controllers.md) — régressions visuelles, associations et validations matérielles; reporté.
- [Validation finale](validation.md) — corpus `image_test`, Greaseweazle et entrées/sorties internes; reporté.

## Images HDD et supports optiques

- [Création et cycle de vie des images HDD](hard-disk-images.md) — capacités et variantes encore ouvertes.
- [Exploration et visualisation des HDD et supports optiques](media-exploration.md) — chantier différé après le socle HDD.

## Plans facultatifs ou différés par décision

- [Visuels matériels supplémentaires](interface/controller-artwork-backlog.md) — backlog facultatif.
- [Organisation GitHub et suivi public](project/github-public-project.md) — changement important explicitement reporté.

Chaque feuille conserve son ordre d’exécution. Une case est cochée seulement après réalisation et
vérification de l’action correspondante.

La présence d’une action `commit`, `push` ou création de tag dans une feuille ne l’autorise pas. Ces
opérations Git sont exécutées uniquement après une demande explicite de l’utilisateur; une demande
explicite de publication autorise les opérations prévues par sa procédure.
