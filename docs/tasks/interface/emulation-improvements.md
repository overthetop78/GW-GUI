# Améliorations souhaitées pour l’interface d’émulation

Ce fichier est le sommaire du suivi d’émulation. Les demandes, décisions et checklists sont réparties par thème dans le dossier [emulation](emulation/), sans changement de leur état d’avancement.

Lire les [règles communes](emulation/rules.md) avant de modifier une checklist. Mettre à jour le fichier du thème concerné ; ne pas recopier les tâches dans ce sommaire. Les numéros des points d’origine sont conservés dans les documents.

## Demandes et suivi par thème

| Thème | Document |
|---|---|
| Identification, brouillons et enregistrement automatique | [Configuration des machines](emulation/machine-configuration.md) |
| Tableau, modification et suppression | [Tableau des configurations](emulation/configuration-table.md) |
| Retour du focus à l’instance active | [Focus](emulation/focus.md) |
| Destination des ROM détectées | [ROM](emulation/firmware-destination.md) |
| Aides au survol et au clic, inventaire des champs | [Aides contextuelles](emulation/contextual-help.md) |
| Associations, ports et visualisation des appuis | [Manettes et joysticks](emulation/controllers.md) |
| Coordonnées et commandes des profils | [Zones des profils](emulation/controller-profile-zones.md) |
| Images matérielles à créer ou déjà validées | [Visuels matériels](emulation/controller-artwork-backlog.md) |
| Recherche, décisions validées et architecture initiale | [Socle vidéo](emulation/video-foundations.md) |
| Groupes d’options, échantillonnage et rendus | [Ergonomie vidéo](emulation/video-settings.md) |
| Découplage de la présentation et des DLL d’émulation | [Présentation vidéo côté hôte](emulation/video-host-separation.md) |
| Technologies d’écran, filtres et validation globale | [Technologies vidéo](emulation/video-technologies.md) |
| Habillages d’écran — réalisation non autorisée à ce stade | [Habillages](emulation/screen-bezels.md) |

## Historique des décisions vidéo

Ces fichiers conservent le journal historique ; les checklists thématiques portent le suivi courant.

- [Socle, gamma, captures et propagation de configuration](emulation/video-decisions-foundations.md)
- [Plasma, vectoriel, agrandissement et restauration](emulation/video-decisions-filters.md)
- [VFD, matrices, segments, papier électronique et projection](emulation/video-decisions-displays.md)
- [Rémanence, mouvement, signaux, effets stylistiques et validation](emulation/video-decisions-effects.md)

## But du document

Ce document reprend uniquement les demandes et les idées formulées à partir des six images de l’interface.

Le suivi distingue les demandes validées des pistes encore à étudier. L’ordre général reste ci-dessous ; les demandes et les checklists techniques des points 1 à 8 se trouvent dans les documents thématiques liés plus haut.

## Points généraux restant à décider ou à étudier

- la présence et l’apparence éventuelle d’une icône accompagnant la couleur des machines déjà configurées ;
- la présentation graphique définitive du tableau des configurations et de ses icônes ;
- la liste complète des filtres vidéo, leurs groupes, leurs réglages et leur méthode technique de réalisation ;
- les aspects encore ouverts de l’idée future des habillages d’écran ;
- le pourcentage du seuil utilisé avant tout changement visuel analogique ;
- les périphériques supplémentaires à représenter après les modèles basiques.

## Ordre général de réalisation

Cet ordre de groupes est validé. Les checklists détaillées de chaque point devront respecter cet ordre global, même lorsque deux éléments appartiennent à une même section fonctionnelle du présent document.

1. Enregistrement automatique fiable de toutes les configurations.
2. Nouveau tableau des configurations et suppression correcte.
3. Retour automatique du focus à l’émulation.
4. Destination des ROM.
5. Aides contextuelles.
6. Réutilisation et amélioration du visualiseur de manettes.
7. Recherche et architecture des filtres vidéo.
8. Habillages d’écran, beaucoup plus tard.
